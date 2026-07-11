# Prompt De Traspaso Para Claude Code

```text
Estoy trabajando en un proyecto Unity 6.5 en /Users/migueltroncoso/Documents/Mmorpg. Es un prototipo MMORPG Android estilo accion/isometrico inspirado en MU Online/Metin2, pero con identidad propia. Ya existen fases 1-5.6:

- Fase 1: movimiento, camara, ataque, enemigos, HUD.
- Fase 2: 4 clases Guerrero/Ninja/Chaman/Umbra, habilidades Q/E, EXP, oro, loot, armas KayKit CC0.
- Fase 3: servidor WebSocket Node.js en Server/, cliente Unity online local, chat y jugadores remotos.
- Fase 4: inventario, mision, mercader, pociones, mejora de arma/armadura, Monolito Corrupto.
- Fase 5: preparacion Android, pantalla de carga, runtime 60 FPS, menu MMORPG > Android, safe area, HUD mobile, feed de actividad, generador placeholder de icono/splash.
- Fase 5.5: creacion de personaje MVP con nombre, clase, sexo masculino/femenino, etiqueta visible y nombre integrado al cliente online local.
- Fase 5.6: avatar visual procedural con silueta distinta por clase/sexo y jugadores remotos por clase.

No reescribas todo. Lee primero README.md, docs/roadmap.md, docs/technical-plan.md, docs/android-release-checklist.md y este archivo. Mantén el estilo C# simple y autocontenido, porque la escena se genera en runtime desde Assets/Scripts/Core/PrototypeBootstrap.cs.

Estado importante:
- Unity usado: 6000.5.3f1.
- Carpeta: /Users/migueltroncoso/Documents/Mmorpg.
- Android Build Support aun puede no estar instalado en Unity Hub; no intentes build Android real si falta PlaybackEngines/AndroidPlayer.
- Para ver el juego: abrir Assets/Scenes/Prototype.unity y Play.
- Para server online local: cd Server && npm install && npm start, luego conectar desde Unity con ws://localhost:7777.
- Para telefono fisico, usar la IP local del Mac en vez de localhost.

Siguiente objetivo recomendado:
Implementar persistencia local MVP: guardar nombre, clase, sexo, nivel/oro/inventario en PlayerPrefs o JSON local para que al cerrar y abrir no se pierda todo. Mantenerlo como prototipo local; la persistencia real deberia ir al servidor mas adelante. Alternativa visual: importar un pack CC0/MIT compatible y documentar licencias.

Reglas de trabajo:
- Antes de editar, revisa git status y usa rg para buscar archivos.
- Usa apply_patch para cambios manuales.
- No borres ni reviertas cambios existentes.
- No metas assets con licencia dudosa. Solo CC0/MIT/propios y documenta en ASSET_LICENSES.md si agregas assets.
- Al terminar, compila con Unity en una copia temporal excluyendo Library/Temp/Logs/.git/UserSettings/node_modules.
- Reporta claramente lo hecho, archivos cambiados y pruebas ejecutadas.
```
