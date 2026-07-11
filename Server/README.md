# MMORPG Prototype Server

Servidor WebSocket local para la Fase 3 temprana.

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

- `hello`: registra nombre y clase.
- `position`: actualiza posicion y rotacion.
- `chat`: envia mensaje de chat.

Servidor a cliente:

- `welcome`: confirma conexion.
- `snapshot`: lista de jugadores conectados.
- `playerJoined`: jugador nuevo.
- `playerUpdated`: datos actualizados.
- `playerLeft`: jugador desconectado.
- `chat`: mensaje de chat.

