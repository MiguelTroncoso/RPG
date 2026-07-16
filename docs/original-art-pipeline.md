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
`LOD0`, `LOD1` y `LOD2`, y los clips `Idle`, `Run` y `Attack`. `LOD1` y `LOD2`
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
`Assets/Resources/OriginalArt/Controllers`. El runtime busca primero el FBX
authored y conserva el generador procedural como fallback si falta un asset.

## Validacion

Para comprobar que un FBX conserva la estructura esperada:

```bash
assimp info Assets/Resources/OriginalArt/Characters/Guerrero_Masculino.fbx
```

La validacion de esta fase debe mostrar `Animations: 3`, huesos skinned,
materiales con Diffuse y Normals, los nombres `LOD0`/`LOD1`/`LOD2` y el objeto
`Starter Weapon`. Los ocho atlas deben ser 2048x2048; Unity los importa con
mipmaps, filtro trilineal, normal map en espacio no color y ETC2_RGBA8 para
Android, y debe regenerar tres clips por cada uno de los ocho controllers. La
auditoria de contenido del juego sigue siendo independiente y debe conservar
17/17.

## Siguiente mejora comercial

Estos meshes son authored originales estilizados, optimizados para validar el
pipeline y la lectura de clase en Android. La Fase 5.74 eleva la fuente de
detalle y la Fase 5.75 añade una pasada de retopologia objetivo, UV padding y
pintura procedural de desgaste, grano y tejido por familia. El resultado sigue
siendo un authored procedural avanzado: no sustituye una escultura manual
hecha por un artista. El siguiente salto comercial es aportar esculturas
high-poly manuales, retopologia/UV artística, LOD optimizado a mano y
animaciones profesionales de produccion.
