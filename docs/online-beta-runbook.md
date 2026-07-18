# Ruta Para La Beta Online S-01

## Decision Inicial

Para la primera beta cerrada recomiendo **OVHcloud**, usando la cuenta ya
disponible y una ubicacion de Estados Unidos oeste (Hillsboro) si las pruebas
desde Chile confirman mejor latencia que la costa este.

- Beta privada de dos a diez personas: instancia de 2 vCore, 4 GB RAM y NVMe.
- Beta cerrada con persistencia PostgreSQL y 20-50 personas: 2 vCore, 8 GB RAM
  como minimo.
- No se necesita GPU.
- Elige facturacion por hora al principio; no comprometer meses antes de medir
  latencia, estabilidad y consumo.

OVHcloud publica instancias de descubrimiento de 2 vCore/4 GB/50 GB NVMe y una
red de 250 Mbps; para la siguiente escala publica una instancia general de 2
vCore/8 GB/50 GB NVMe con red de 500 Mbps. El proveedor tambien incluye
proteccion anti-DDoS estandar para Public Cloud. Verificar el precio y la region
desde la consola antes de crear recursos.

## Alternativas

- **Hetzner Cloud**: opcion muy buena cuando se prioriza costo/rendimiento y se
  acepta operar los backups con cuidado. Usar Hillsboro (`hil`) o Ashburn (`ash`)
  segun el ping real; para carga sostenida, preferir CPU dedicada sobre shared.
- **Contabo**: util para staging, herramientas de contenido o un servidor de
  ensayo barato. No sera el S-01 publico inicial hasta medir rendimiento de CPU,
  red y soporte bajo una carga real.

## Arquitectura S-01

```text
Android (WSS)
      |
Dominio s01.tudominio.cl
      |
Caddy o Nginx: TLS, rate limit y WebSocket
      |
Node.js MMO gateway (localhost:7777)
      |
PostgreSQL: cuentas, personajes, inventario, progreso y auditoria
      |
Backups cifrados fuera de la instancia
```

No exponer el puerto 7777. Abrir solo 22, 80 y 443; usar llaves SSH, firewall,
actualizaciones automaticas de seguridad, secretos fuera de Git y backup diario
probado mediante restauracion.

## Fases Antes De Invitar Jugadores

1. Recuperar licencia Unity, compilar la APK de Fase 5.91 y superar
   `docs/offline-beta-qa.md`.
2. Ejecutar el servidor actual en LAN con dos telefonos; validar presencia,
   chat, reconexion y guardado remoto sin tocar economia real.
3. Migrar la persistencia de `players.json` a PostgreSQL y crear identidad de
   cuenta con token seguro. No usar nombre de personaje como autenticacion.
4. Convertir combate, drops, inventario, mejoras, eventos y compras en acciones
   validadas por servidor. El cliente solo envia intenciones.
5. Desplegar S-01 privado con WSS, health check, monitor, alarmas y backup.
6. Hacer una beta de 5-10 amistades, revisar telemetria y corregir antes de
   abrir S-02 o publicar en Google Play.

## Referencias Oficiales

- OVHcloud Public Cloud: https://www.ovhcloud.com/en/public-cloud/prices/
- OVHcloud ubicaciones: https://us.ovhcloud.com/about/global-infrastructure/locations/
- Hetzner Cloud ubicaciones: https://docs.hetzner.com/cloud/general/locations/
- Hetzner recursos shared/dedicated: https://docs.hetzner.com/cloud/servers/faq/
