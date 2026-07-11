import { WebSocket, WebSocketServer } from "ws";
import crypto from "node:crypto";

const port = Number(process.env.PORT ?? 7777);
const tickRateMs = 100;
const players = new Map();

const server = new WebSocketServer({ port });

server.on("connection", (socket, request) => {
  const id = crypto.randomUUID();
  const player = {
    id,
    name: `Player-${id.slice(0, 4)}`,
    className: "Guerrero",
    gender: "Masculino",
    level: 1,
    lastActionAt: 0,
    x: 0,
    y: 1,
    z: 0,
    yaw: 0,
    connectedAt: Date.now(),
    socket
  };

  players.set(id, player);
  send(socket, { type: "welcome", id, serverTime: Date.now() });
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

console.log(`MMORPG prototype server listening on ws://localhost:${port}`);

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
      player.name = sanitizeText(message.name, 24) || player.name;
      player.className = sanitizeText(message.className, 24) || player.className;
      player.gender = sanitizeGender(message.gender) ?? player.gender;
      player.level = sanitizeLevel(message.level) ?? player.level;
      broadcast({ type: "playerUpdated", player: publicPlayer(player) });
      break;

    case "position":
      player.x = toNumber(message.x, player.x);
      player.y = toNumber(message.y, player.y);
      player.z = toNumber(message.z, player.z);
      player.yaw = toNumber(message.yaw, player.yaw);
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

function sanitizeGender(value) {
  const gender = sanitizeText(value, 12);
  return gender === "Masculino" || gender === "Femenino" ? gender : null;
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
      return null;

    case "upgrade":
      if (!Number.isInteger(value) || value < 1 || value > 15) {
        return "invalid_upgrade";
      }
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
