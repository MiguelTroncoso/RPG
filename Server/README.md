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

Tambien expone `GET http://localhost:7777/health` para health checks de
Hetzner, Docker, Nginx o un monitor externo. El servidor limita los mensajes
WebSocket a 64 KB, envia heartbeat cada 30 segundos y guarda las sesiones al
recibir `SIGTERM`/`SIGINT`.

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

## Paso S-01 En Hetzner

La siguiente infraestructura es la recomendada para la primera prueba publica:

1. Crear una instancia pequena Ubuntu LTS, instalar Node.js LTS y clonar este
   repositorio en una carpeta de servicio.
2. Ejecutar el proceso con `systemd` y `SERVER_ID=S-01`, escuchando solo en
   `127.0.0.1:7777` si Nginx sera el proxy publico.
3. Usar Nginx con un dominio, certificado Let's Encrypt y proxy WebSocket
   hacia `127.0.0.1:7777`. El cliente Android debe usar `wss://dominio/ws`.
4. Permitir por firewall solo `22`, `80` y `443`; no exponer el puerto 7777.
5. Respaldar `Server/data/players.json` y probar restauracion antes de invitar
   jugadores.

Plantillas listas para adaptar:

- `Server/deploy/mmorpg-s01.service.example`
- `Server/deploy/nginx-s01.conf.example`

Despues de copiarlas a sus ubicaciones del sistema, activar con
`systemctl enable --now mmorpg-s01` y comprobar `curl https://dominio/health`.

Ejemplo de variables del proceso:

```text
SERVER_ID=S-01
SERVER_NAME=Valle Central
HOST=127.0.0.1
PORT=7777
MAX_PLAYERS=100
```

La autoridad completa de combate, drops, inventario, habilidades, compras y
anti-cheat aun es una fase posterior. Esta capa sirve para probar conexion,
presencia, chat y persistencia con seguridad operacional basica.
