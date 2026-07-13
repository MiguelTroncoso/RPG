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

Para una prueba desde un telefono en la misma red, el servidor escucha en
todas las interfaces (`0.0.0.0`). En la APK se puede introducir la IP local
del Mac/PC, por ejemplo `ws://192.168.1.20:7777`.

Los perfiles se pueden ejecutar con variables de entorno para preparar varios
servidores sin duplicar el codigo:

```bash
SERVER_ID=S-01 SERVER_NAME="Valle Central" HOST=0.0.0.0 PORT=7777 MAX_PLAYERS=100 npm start
SERVER_ID=S-02 SERVER_NAME="Bosque de los Susurros" PORT=7778 npm start
```

`S-02` y `S-03` siguen siendo perfiles de preparacion hasta desplegar sus
instancias y datos separados.

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
