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
- `Assets/Resources/OriginalArt/Textures/OriginalArt_AlbedoAtlas.png`.
- `Assets/Resources/OriginalArt/Textures/OriginalArt_NormalAtlas.png`.
- `Assets/Art/Original/OriginalArt_Source.blend` como fuente editable.

Cada FBX contiene un armature skinned, bind poses, pesos por hueso, materiales
con albedo y normal, cuerpo y arma inicial separada como `Starter Weapon` en
`LOD0`, `LOD1` y `LOD2`, y los clips `Idle`, `Run` y `Attack`. `LOD1` y `LOD2`
se crean por decimacion geometrica real. Las texturas se enlazan con rutas
relativas al atlas central para evitar ocho copias dentro del repositorio.
El atlas actual es 1024x1024 con tiles 64x64; el normal atlas se genera offline
desde alturas detalladas por material para reforzar metal, tela, cuero, runa,
escama, piedra y hueso.

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
`Starter Weapon`. Los atlas deben ser 1024x1024 y Unity debe regenerar tres
clips por cada uno de los ocho controllers. La auditoria de contenido del juego
sigue siendo independiente y debe conservar 17/17.

## Siguiente mejora comercial

Estos meshes son authored originales estilizados, optimizados para validar el
pipeline y la lectura de clase en Android. El LOD geometrico base, el atlas 1K
y las poses de cuerpo completo ya estan implementados. El siguiente salto
artistico es reemplazar progresivamente sus formas por esculturas high-poly,
retopologia manual, bake de normales desde esa escultura, atlas 2K por familia,
LOD optimizado a mano y animaciones de combate de produccion.
