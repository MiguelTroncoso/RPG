# Pipeline De Arte 3D Original

Este proyecto usa Blender como herramienta gratuita para convertir la
direccion artistica de Unity en assets authored editables. Blender no es una
dependencia del APK: solo se necesita para regenerar o modificar los archivos
3D fuente.

## Herramientas

- Blender 5.2 LTS para macOS Apple Silicon.
- Unity 6000.5.3f1 para importar FBX y generar controllers.
- `assimp` para una validacion rapida fuera de Unity.

Blender se instala con Homebrew:

```bash
brew install --cask blender
```

## Regenerar los ocho personajes

Desde la raiz del repositorio:

```bash
blender --background --python Tools/blender/create_original_art_assets.py
```

El script crea:

- `Assets/Resources/OriginalArt/Characters/*.fbx`: Guerrero, Ninja, Chaman
  y Umbra, cada uno en masculino y femenino.
- `Assets/Resources/OriginalArt/Textures/OriginalArt_{Clase}_AlbedoAtlas_2K.png`.
- `Assets/Resources/OriginalArt/Textures/OriginalArt_{Clase}_NormalAtlas_2K.png`.
- `Assets/Art/Original/OriginalArt_Source.blend` como fuente editable.

Cada FBX contiene un armature skinned, bind poses, pesos por hueso, materiales
con albedo y normal, cuerpo y arma inicial separada como `Starter Weapon` en
`LOD0`, `LOD1` y `LOD2`, y los clips `Idle`, `Run`, `Attack`, `Hit` y `Death`.
`LOD1` y `LOD2`
se crean por decimacion geometrica real. Cada familia de clase tiene su propio
atlas 2K de albedo y normal con tiles 256x256, enlazado por rutas relativas.
El bake de normales se ejecuta en Blender desde una fuente high-poly temporal
con subdivision Catmull-Clark de nivel 2, detalle de escultura y microdetalle
separados, proyectandose sobre el objetivo skinned con UVs de produccion. La
jaula objetivo pasa una limpieza de vertices duplicados y normales antes del
bake; las fuentes temporales se eliminan al terminar y el `.blend` conserva la
escena authored editable.

## Generar los controllers de Unity

Unity los genera desde los clips importados. En el editor usar:

`MMORPG > Characters > Generate Original Art Controllers`

En modo batch:

```bash
"/Applications/Unity/Hub/Editor/6000.5.3f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD" \
  -executeMethod MmorpgPrototype.Editor.OriginalArtAnimatorAssetGenerator.Generate
```

El resultado son ocho controllers en
`Assets/Resources/OriginalArt/Controllers`. El runtime usa ahora primero el
FBX authored y conserva KayKit/Quaternius y el generador procedural como
fallback de compatibilidad si falta un asset.

## Validacion

Para comprobar que un FBX conserva la estructura esperada:

```bash
assimp info Assets/Resources/OriginalArt/Characters/Guerrero_Masculino.fbx
```

La validacion de esta fase debe mostrar `Animations: 5`, huesos skinned,
materiales con Diffuse y Normals, los nombres `LOD0`/`LOD1`/`LOD2` y el objeto
`Starter Weapon`. Los ocho atlas deben ser 2048x2048; Unity los importa con
mipmaps, filtro trilineal, normal map en espacio no color y ETC2_RGBA8 para
Android, y debe regenerar cinco clips por cada uno de los ocho controllers. La
auditoria de contenido del juego sigue siendo independiente y debe conservar
17/17.

## Fases 5.76-5.79

Guerrero y Ninja tienen una pasada authored de mayor detalle en masculino y
femenino. Sus placas, guardas, cintas, vendas, bolsos, ornamentos y crestas
son piezas separadas antes del join skinned, por lo que se pueden sustituir
por meshes manuales sin tocar el runtime. Los controllers de mobs y jefes
tambien exponen reacciones `Hit` y `Death`; `EnemyVisualController` las dispara
desde los eventos de `Health` y mantiene el acabado de zona/tier en las diez
zonas.

El pipeline sigue siendo authored procedural avanzado, no una escultura manual
de artista. Para el salto comercial se debe aportar un high-poly esculpido a
mano, retopologia/UV manual, texturas 2K pintadas y animaciones profesionales,
empezando por Zona 1 y reutilizando este contrato de nombres, materiales,
LOD, hitboxes y controllers.

## Siguiente mejora comercial

Estos meshes son authored originales estilizados, optimizados para validar el
pipeline y la lectura de clase en Android. La Fase 5.74 eleva la fuente de
detalle y la Fase 5.75 añade una pasada de retopologia objetivo, UV padding y
pintura procedural de desgaste, grano y tejido por familia. El resultado sigue
siendo un authored procedural avanzado: no sustituye una escultura manual
hecha por un artista. El siguiente salto comercial es aportar esculturas
high-poly manuales, retopologia/UV artística, LOD optimizado a mano y
animaciones profesionales de produccion.

## Fases 5.80-5.81: QA Android Y Mobs Authored Por Zona

`MobileRuntimeDiagnostics` exporta un snapshot JSON desde la ventana `TEST`.
El archivo incluye el dispositivo, resolucion, safe area, perfil de rendimiento,
FPS actual/promedio/minimo, memoria, enemigos y POI activos. La matriz de
recorrido y los comandos ADB estan en `docs/android-qa.md`.

`Tools/blender/create_original_mob_assets.py` genera 30 FBX propios en
`Assets/Resources/OriginalArt/Mobs`: normal, elite y jefe para `valley`,
`forest`, `ash`, `crystal`, `frost`, `sunken`, `obsidian`, `astral`, `eclipse`
y `throne`. Cada FBX contiene armature skinned, `LOD0`/`LOD1`/`LOD2`, atlas
albedo/normal 2K compartido y `Idle`/`Run`/`Attack`/`Hit`/`Death`.

Regenerar los modelos:

```bash
blender --background --python Tools/blender/create_original_mob_assets.py
```

Regenerar los controllers despues de importar los FBX:

```bash
/Applications/Unity/Hub/Editor/6000.5.3f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "$PWD" \
  -executeMethod MmorpgPrototype.Editor.OriginalMobAnimatorAssetGenerator.Generate
```

El runtime usa primero el recurso authored de la zona/tier, configura su
`LODGroup` y controller, luego prueba el set Quaternius y finalmente el
fallback procedural. El pipeline sigue siendo arte authored original
estilizado; la escultura manual high-poly, retopologia artistica y animacion
profesional siguen siendo una mejora comercial futura.
