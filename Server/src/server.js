import { WebSocket, WebSocketServer } from "ws";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const serverId = process.env.SERVER_ID ?? "S-01";
const serverName = process.env.SERVER_NAME ?? "Valle Central";
const host = process.env.HOST ?? "0.0.0.0";
const port = Number(process.env.PORT ?? 7777);
const maxPlayers = Number(process.env.MAX_PLAYERS ?? 100);
const tickRateMs = 100;
const players = new Map();
const __dirname = path.dirname(fileURLToPath(import.meta.url));
const dataDir = path.resolve(__dirname, "../data");
const playerStorePath = path.join(dataDir, "players.json");
const persistedPlayers = loadPersistedPlayers();

const server = new WebSocketServer({ host, port });

server.on("connection", (socket, request) => {
  if (players.size >= maxPlayers) {
    socket.close(1013, "Server full");
    return;
  }

  const id = crypto.randomUUID();
  const player = {
    id,
    name: `Player-${id.slice(0, 4)}`,
    className: "Guerrero",
    gender: "Masculino",
    level: 1,
    playerKey: "",
    lastActionAt: 0,
    lastPersistAt: 0,
    lastUpgradeLevel: 0,
    x: 0,
    y: 1,
    z: 0,
    yaw: 0,
    connectedAt: Date.now(),
    socket
  };

  players.set(id, player);
  send(socket, { type: "welcome", id, serverId, serverName, maxPlayers, serverTime: Date.now() });
  sendRoster(socket);
  broadcast({ type: "playerJoined", player: publicPlayer(player) }, id);
  console.log(`[join] ${id} from ${request.socket.remoteAddress}`);

  socket.on("message", (raw) => handleMessage(player, raw));
  socket.on("close", () => disconnect(player));
  socket.on("error", (error) => {
    console.warn(`[socket-error] ${id}`, error.message);
    disconnect(player);
  });
});

setInterval(() => {
  const snapshot = [...players.values()].map(publicPlayer);
  broadcast({ type: "snapshot", players: snapshot });
}, tickRateMs);

console.log(`MMORPG server ${serverId} (${serverName}) listening on ws://${host}:${port} max=${maxPlayers}`);

function handleMessage(player, raw) {
  let message;

  try {
    message = JSON.parse(raw.toString());
  } catch {
    send(player.socket, { type: "error", message: "Invalid JSON" });
    return;
  }

  switch (message.type) {
    case "hello":
      {
        const incomingName = sanitizeText(message.name, 24);
        const incomingClass = sanitizeClass(message.className) ?? player.className;
        const incomingGender = sanitizeGender(message.gender) ?? player.gender;
        const incomingLevel = sanitizeLevel(message.level);
        const playerKey = sanitizeKey(message.playerKey) || keyFromName(incomingName || player.name);
        const saved = persistedPlayers[playerKey];

        player.playerKey = playerKey;
        if (saved) {
          applyPersistedPlayer(player, saved);
        }

        player.name = incomingName || player.name;
        player.className = incomingClass;
        player.gender = incomingGender;
        player.level = Math.max(player.level, incomingLevel ?? player.level);
        persistPlayer(player);
        sendSavedState(player, saved);
      }
      broadcast({ type: "playerUpdated", player: publicPlayer(player) });
      break;

    case "position":
      player.x = toNumber(message.x, player.x);
      player.y = toNumber(message.y, player.y);
      player.z = toNumber(message.z, player.z);
      player.yaw = toNumber(message.yaw, player.yaw);
      persistPlayerThrottled(player);
      break;

    case "chat":
      {
        const text = sanitizeText(message.text, 140);
        if (text.length > 0) {
          broadcast({
            type: "chat",
            id: player.id,
            name: player.name,
            text,
            serverTime: Date.now()
          });
        }
      }
      break;

    // Intencion de accion critica. El servidor valida plausibilidad y
    // ritmo antes de difundirla; una intencion rechazada solo vuelve al
    // emisor. Este es el punto donde crecera la autoridad del servidor.
    case "action":
      {
        const action = sanitizeText(message.action, 24);
        const detail = sanitizeText(message.detail, 90);
        const value = Number(message.value);

        if (action.length === 0 || detail.length === 0) {
          break;
        }

        const rejection = validateAction(player, action, value);
        if (rejection) {
          send(player.socket, { type: "actionRejected", action, reason: rejection });
          console.warn(`[action-rejected] ${player.id} ${action}=${value}: ${rejection}`);
          break;
        }

        player.lastActionAt = Date.now();
        broadcast({
          type: "activity",
          id: player.id,
          name: player.name,
          action,
          detail,
          serverTime: Date.now()
        });
      }
      break;

    case "saveState":
      {
        const incomingKey = sanitizeKey(message.playerKey) || player.playerKey;
        if (!incomingKey || (player.playerKey && incomingKey !== player.playerKey)) {
          send(player.socket, { type: "error", message: "Invalid playerKey for saveState" });
          break;
        }

        const parsed = parseJsonPayload(message.stateJson, 65536);
        if (!parsed) {
          send(player.socket, { type: "error", message: "Invalid saveState payload" });
          break;
        }

        player.playerKey = incomingKey;
        applySaveDataToPlayer(player, parsed);
        persistPlayer(player, JSON.stringify(parsed));
      }
      break;

    case "telemetry":
      {
        const incomingKey = sanitizeKey(message.playerKey) || player.playerKey;
        if (!incomingKey || (player.playerKey && incomingKey !== player.playerKey)) {
          break;
        }

        const parsed = parseJsonPayload(message.summaryJson, 65536);
        if (!parsed) {
          break;
        }

        player.playerKey = incomingKey;
        player.lastTelemetry = parsed;
        persistPlayer(player);
      }
      break;

    default:
      send(player.socket, { type: "error", message: `Unknown message type: ${message.type}` });
      break;
  }
}

function disconnect(player) {
  if (!players.has(player.id)) {
    return;
  }

  players.delete(player.id);
  persistPlayer(player);
  broadcast({ type: "playerLeft", id: player.id });
  console.log(`[leave] ${player.id}`);
}

function sendRoster(socket) {
  send(socket, {
    type: "snapshot",
    players: [...players.values()].map(publicPlayer)
  });
}

function broadcast(payload, exceptId = "") {
  const data = JSON.stringify(payload);

  for (const player of players.values()) {
    if (player.id === exceptId || player.socket.readyState !== WebSocket.OPEN) {
      continue;
    }

    player.socket.send(data);
  }
}

function send(socket, payload) {
  if (socket.readyState === WebSocket.OPEN) {
    socket.send(JSON.stringify(payload));
  }
}

function publicPlayer(player) {
  return {
    id: player.id,
    name: player.name,
    className: player.className,
    gender: player.gender,
    level: player.level,
    x: player.x,
    y: player.y,
    z: player.z,
    yaw: player.yaw
  };
}

function applyPersistedPlayer(player, saved) {
  player.name = sanitizeText(saved.name, 24) || player.name;
  player.className = sanitizeClass(saved.className) ?? player.className;
  player.gender = sanitizeGender(saved.gender) ?? player.gender;
  player.level = sanitizeLevel(saved.level) ?? player.level;
  player.x = toNumber(saved.x, player.x);
  player.y = toNumber(saved.y, player.y);
  player.z = toNumber(saved.z, player.z);
  player.yaw = toNumber(saved.yaw, player.yaw);
  player.lastUpgradeLevel = Number.isInteger(saved.lastUpgradeLevel) ? saved.lastUpgradeLevel : 0;
  player.lastTelemetry = saved.lastTelemetry && typeof saved.lastTelemetry === "object" ? saved.lastTelemetry : null;
}

function persistPlayerThrottled(player) {
  const now = Date.now();
  if (now - player.lastPersistAt < 1000) {
    return;
  }

  persistPlayer(player);
}

function persistPlayer(player, stateJson = null) {
  if (!player.playerKey) {
    return;
  }

  const previous = persistedPlayers[player.playerKey] ?? {};
  player.lastPersistAt = Date.now();
  persistedPlayers[player.playerKey] = {
    playerKey: player.playerKey,
    name: player.name,
    className: player.className,
    gender: player.gender,
    level: player.level,
    x: player.x,
    y: player.y,
    z: player.z,
    yaw: player.yaw,
    lastUpgradeLevel: player.lastUpgradeLevel ?? 0,
    stateJson: stateJson ?? previous.stateJson ?? "",
    lastTelemetry: player.lastTelemetry ?? previous.lastTelemetry ?? null,
    updatedAt: new Date(player.lastPersistAt).toISOString()
  };
  savePersistedPlayers();
}

function sendSavedState(player, saved) {
  if (!saved || typeof saved.stateJson !== "string" || saved.stateJson.length === 0) {
    return;
  }

  send(player.socket, {
    type: "savedState",
    stateJson: saved.stateJson,
    updatedAt: saved.updatedAt ?? ""
  });
}

function loadPersistedPlayers() {
  try {
    if (!fs.existsSync(playerStorePath)) {
      return {};
    }

    const parsed = JSON.parse(fs.readFileSync(playerStorePath, "utf8"));
    return parsed && typeof parsed === "object" ? parsed : {};
  } catch (error) {
    console.warn(`[persistence] could not load ${playerStorePath}: ${error.message}`);
    return {};
  }
}

function savePersistedPlayers() {
  fs.mkdirSync(dataDir, { recursive: true });
  const tempPath = `${playerStorePath}.tmp`;
  fs.writeFileSync(tempPath, JSON.stringify(persistedPlayers, null, 2));
  fs.renameSync(tempPath, playerStorePath);
}

function sanitizeGender(value) {
  const gender = sanitizeText(value, 12);
  return gender === "Masculino" || gender === "Femenino" ? gender : null;
}

function sanitizeClass(value) {
  const className = sanitizeText(value, 24);
  return ["Guerrero", "Ninja", "Chaman", "Umbra"].includes(className) ? className : null;
}

function sanitizeKey(value) {
  return sanitizeText(value, 80).replace(/[^a-zA-Z0-9_-]/g, "");
}

function keyFromName(name) {
  const safeName = sanitizeText(name, 24).toLowerCase().replace(/[^a-z0-9_-]/g, "_");
  return safeName || crypto.randomUUID();
}

function applySaveDataToPlayer(player, data) {
  player.name = sanitizeText(data.CharacterName ?? data.name, 24) || player.name;
  player.className = sanitizeClass(data.ClassName ?? data.className) ?? player.className;
  player.gender = sanitizeGender(data.GenderName ?? data.gender) ?? player.gender;
  player.level = sanitizeLevel(data.Level ?? data.level) ?? player.level;
  player.x = toNumber(data.PosX ?? data.x, player.x);
  player.y = toNumber(data.PosY ?? data.y, player.y);
  player.z = toNumber(data.PosZ ?? data.z, player.z);
  player.yaw = toNumber(data.Yaw ?? data.yaw, player.yaw);
  player.lastUpgradeLevel = Math.max(
    Number.isInteger(data.WeaponLevel) ? data.WeaponLevel : 0,
    Number.isInteger(data.ArmorLevel) ? data.ArmorLevel : 0,
    player.lastUpgradeLevel ?? 0
  );
}

function parseJsonPayload(value, maxBytes) {
  if (typeof value !== "string" || Buffer.byteLength(value, "utf8") > maxBytes) {
    return null;
  }

  try {
    const parsed = JSON.parse(value);
    return parsed && typeof parsed === "object" && !Array.isArray(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

function sanitizeLevel(value) {
  const level = Number(value);
  return Number.isInteger(level) && level >= 1 && level <= 105 ? level : null;
}

// Validacion de intenciones: null = aceptada, string = motivo de rechazo.
function validateAction(player, action, value) {
  const minActionIntervalMs = 800;
  if (Date.now() - player.lastActionAt < minActionIntervalMs) {
    return "too_fast";
  }

  switch (action) {
    case "level_up":
      if (!Number.isInteger(value) || value <= player.level || value > 105) {
        return "invalid_level";
      }
      player.level = value;
      persistPlayer(player);
      return null;

    case "upgrade":
      if (!Number.isInteger(value) || value < 1 || value > 15) {
        return "invalid_upgrade";
      }
      player.lastUpgradeLevel = Math.max(player.lastUpgradeLevel ?? 0, value);
      persistPlayer(player);
      return null;

    default:
      return "unknown_action";
  }
}

function toNumber(value, fallback) {
  const number = Number(value);
  return Number.isFinite(number) ? number : fallback;
}

function sanitizeText(value, maxLength) {
  return String(value ?? "")
    .replace(/\s+/g, " ")
    .trim()
    .slice(0, maxLength);
}
