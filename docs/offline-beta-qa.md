# Beta Offline: Recorrido De Prueba

Este corte permite validar el cliente completo sin depender del backend. La
APK que contenga la Fase 5.91 debe probarse en horizontal y con el perfil
`RENDIMIENTO` primero.

## Entrada

1. Abrir la aplicacion y comprobar que aparece el splash propio, no el logo de
   Unity.
2. Esperar el acceso: debe verse el reliquiario cinematografico, la tarjeta de
   personaje y S-01 sin que el error de red ocupe la pantalla.
3. Crear personaje con las cuatro clases y ambos sexos; cerrar y volver a abrir
   para confirmar que la tarjeta recupera nombre, nivel, oro y clase.
4. Entrar sin servidor: el modo offline debe seguir funcionando aunque S-01 no
   responda.

## Mundo Y Combate

1. Abrir `MAS` y pulsar `MAPA`.
2. Confirmar que aparecen las diez zonas, su rango, mob normal, elite, jefe y
   reliquia.
3. Con `MODO PRUEBA` activo, viajar a cada zona y revisar modelo normal, elite,
   jefe, landmark, terreno, minimapa, sombras, aura de tier y LOD.
4. Desactivar `MODO PRUEBA`: una zona por encima del nivel actual debe indicar
   su requisito y bloquear el viaje.
5. En cada zona: mover con joystick, girar camara con el dedo, atacar y lanzar
   Q/E/R/F/G simultaneamente cuando corresponda. No deben aparecer saltos de
   orientacion ni bloquearse los controles.
6. Confirmar que al entrar a una zona no se ven mobs ni jefes de zonas vecinas;
   el jefe de esa zona debe aparecer solo al acercarse a su landmark.
7. Entrar al campamento de Zona 1: no debe permitir combate ni contener mobs.
8. Recibir dano y alejarse: la regeneracion debe iniciar solo tras el periodo
   sin recibir dano.

## Lectura Visual

1. Al inicio, `CHAT` y `MAS` deben permanecer cerrados; el centro de pantalla
   queda reservado al personaje y al combate.
2. Verificar que las habilidades muestran solo Q/E/R/F/G con nivel o segundos
   de enfriamiento, sin nombres largos desbordados.
3. Revisar que cada zona tenga suelo con detalle, borde de bioma y landmark;
   Zona 1 debe mostrar el campamento comercial y su fogata.
4. Usar `MAS` para abrir los sistemas cuando se necesiten y cerrarlos antes de
   continuar la prueba de combate.

## Sistemas

1. Revisar inventario, equipo, estadisticas, habilidades, misiones, contratos,
   temporada, mascota, montura, atuendo y alas.
2. Abrir `GRAFICOS`, probar `RENDIMIENTO` y `CALIDAD`; despues abrir `TEST` y
   exportar el JSON de diagnostico.
3. Anotar carga inicial, FPS, memoria, temperatura percibida, caidas, errores
   de UI y zona donde ocurrieron.

## Bloqueadores Para S-01

- Cierre inesperado o pantalla negra.
- Controles tactiles que se bloquean al usar ataque o camara.
- Personaje o mobs atravesando terreno/obstaculos.
- UI que tapa el centro de combate, minimapa o habilidades.
- FPS sostenido por debajo de 30 en `RENDIMIENTO`.
- Perdida o corrupcion del guardado local al crear, entrar o cambiar de zona.

La validacion se considera superada cuando el recorrido de las diez zonas se
completa en un telefono real, se exporta el JSON de `TEST` y no quedan
bloqueadores criticos.
