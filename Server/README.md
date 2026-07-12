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

Servidor a cliente:

- `welcome`: confirma conexion.
- `snapshot`: lista de jugadores conectados.
- `playerJoined`: jugador nuevo.
- `playerUpdated`: datos actualizados.
- `playerLeft`: jugador desconectado.
- `chat`: mensaje de chat.
- `activity`: accion validada difundida al resto.
- `actionRejected`: intencion rechazada por validacion del servidor.

## Persistencia

El servidor guarda estado online basico por `playerKey` en:

```text
Server/data/players.json
```

Ese archivo se ignora en git. Guarda identidad, nivel, posicion, rotacion y
ultima mejora validada. Inventario, equipo completo, misiones y almacen siguen
en el guardado local Unity por ahora.
