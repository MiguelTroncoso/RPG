# MMORPG Prototype Server

Servidor WebSocket local para el prototipo MMORPG.

## Uso

```bash
cd Server
npm install
npm start
```

Por defecto escucha en:

```text
ws://localhost:7777
```

## Protocolo Inicial

Cliente a servidor:

- `hello`: registra `playerKey`, nombre, clase, sexo y nivel.
- `position`: actualiza posicion y rotacion.
- `chat`: envia mensaje de chat.
- `action`: envia intenciones validadas (`level_up`, `upgrade`).
- `saveState`: envia snapshot completo `PlayerSaveData` como JSON string.
- `telemetry`: envia resumen agregado de combate como JSON string.

Servidor a cliente:

- `welcome`: confirma conexion.
- `snapshot`: lista de jugadores conectados.
- `savedState`: devuelve el snapshot remoto guardado para el `playerKey`.
- `playerJoined`: jugador nuevo.
- `playerUpdated`: datos actualizados.
- `playerLeft`: jugador desconectado.
- `chat`: mensaje de chat.
- `activity`: accion validada difundida al resto.
- `actionRejected`: intencion rechazada por validacion del servidor.

## Persistencia

El servidor guarda estado online por `playerKey` en:

```text
Server/data/players.json
```

Ese archivo se ignora en git. Guarda identidad, nivel, posicion, rotacion,
ultima mejora validada, el snapshot completo `PlayerSaveData` recibido por
`saveState` y el ultimo resumen de telemetria recibido por `telemetry`.

El cliente conserva el JSON local Unity como respaldo. Al reconectar, si el
guardado remoto esta mas avanzado, lo aplica; si el local esta mejor, lo
mantiene y vuelve a enviarlo al servidor.
