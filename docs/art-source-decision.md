# Decision de fuente artistica

## Eleccion

La fuente recomendada para la produccion de personajes es
[MMORPG Characters - Mega Collection - Warriors, Archers, Mages](https://ramens.artstation.com/store/ebJWn/mmorpg-characters-mega-collection-warriors-archers-mages), de Ida
Faber.

Esta es una decision de direccion y adquisicion. El paquete todavia no esta
descargado en el proyecto, por lo que no se presenta como integrado ni se
marcan assets como aprobados.

## Por que esta fuente

- Mantiene una familia visual comun para todo el elenco, algo que los packs
  mezclados no garantizan.
- Incluye 18 personajes modulares, 18 atuendos, 57 peinados y armas de
  guerrero, arquero y mago.
- Incluye rig humanoide, blendshapes, materiales PBR y archivos preparados para
  Unity. La pagina indica mapas de color, normales, roughness, metallic, AO y
  opciones de mascara/emision.
- La serie muestra variantes masculinas y femeninas por nivel. El contenido
  exacto se verificara despues de descargarlo antes de aprobar cada FBX.
- Permite llevar el proyecto a una lectura mas cercana a un MMORPG comercial
  sin depender de los FBX procedurales que hoy se ven como robots.

## Traduccion a nuestras clases

La coleccion no se importara literalmente como cuatro botones con el mismo
modelo. Se usara como base artistica y se hara una pasada de identidad:

| Clase del juego | Base de la coleccion | Tratamiento propio |
| --- | --- | --- |
| Guerrero | Warrior | espada, armadura pesada, escudo y metal envejecido |
| Ninja | Archer como base humanoide | dagas, capucha, telas oscuras, silueta agil y colores de sombra |
| Chaman | Mage | baston, talismanes, tejidos rituales, runas y paleta elemental |
| Umbra | Mage/Warrior | armadura oscura, capa, detalles violetas y accesorios de reliquia |

La adaptacion se hara sobre piezas modulares y materiales autorizados. No se
copiaran ni se fabricaran variantes que rompan la licencia del proveedor.

## Alcance que aun requiere fuentes adicionales

La coleccion es para personajes y armas. No sustituye por si sola:

- mobs normales, elites y jefes de las diez zonas;
- NPC de campamento y vendedores;
- monturas, mascotas y alas;
- vegetacion, ruinas, props, VFX y audio.

Despues del primer lote de personajes se elegiran fuentes compatibles para
mobs/NPC y se validara que sus materiales y siluetas no parezcan de otra
produccion. En Zona 1 se probara primero el conjunto Guerrero, Ninja, Chaman,
Umbra, Mercader, Almacen, Herrero y la familia de lobos antes de replicar a las
otras nueve zonas.

## Licencia y costo

La pagina muestra una licencia Standard de **USD 799.99**, para un proyecto
comercial con el limite indicado por el proveedor, y una licencia Extended de
**USD 1,499.99** sin ese limite. El precio, impuestos y condiciones deben
confirmarse en el checkout antes de comprar.

No se hara ninguna compra automatica. El proyecto no puede descargar un asset
de pago sin que el propietario de la cuenta confirme la adquisicion. Para una
prueba mas barata existe el bundle de personajes medievales de seis modelos
por **USD 349.99**, pero cubre solo la base guerrera y no resuelve el elenco de
cuatro clases; por eso no es la recomendacion final.

## Importacion cuando el paquete este disponible

1. Descargar desde la cuenta del propietario solamente `UnityAssets_MEGA_MMORPG`
   y las texturas necesarias.
2. Colocarlo localmente en `Assets/Art/External/IdaFaber/`. Esa carpeta esta
   excluida de GitHub.
3. No reemplazar el runtime de golpe. Primero se hara una escena de validacion
   con una variante masculina y una femenina del Guerrero.
4. Normalizar materiales para Android, crear LOD0/LOD1/LOD2, revisar escala,
   colision, nombres de huesos y aplicar las animaciones del juego.
5. Crear el sidecar `.production.json` con proveedor, factura/licencia,
   procedencia y revision visual.
6. Integrar el lote de Zona 1 y ejecutar la auditoria. Solo un asset que pase
   la auditoria y la prueba Android puede sustituir al fallback actual.

## Criterio de aceptacion

La fuente queda aprobada para el proyecto cuando el lote de Zona 1 tenga:

- las ocho variantes de clase y sexo con siluetas claramente distinguibles;
- armas y piezas equipables visibles en el personaje;
- materiales PBR autocontenidos, LOD real y animaciones de combate;
- NPC y mobs con el mismo nivel de acabado;
- capturas comparables en Unity y Android sin regresion de rendimiento;
- auditoria comercial con procedencia y licencia, no solo un FBX importado.

Mientras esos pasos no existan, el progreso de arte comercial sigue en 0%
aprobado. Esto es intencional y evita volver a confundir un prototipo tecnico
con la produccion final.
