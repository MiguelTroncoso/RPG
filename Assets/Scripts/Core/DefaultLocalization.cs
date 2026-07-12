using System.Collections.Generic;

namespace MmorpgPrototype
{
    // Tabla espanola por defecto. Fuente unica para el generador de assets
    // (MMORPG > Localization > Generate Localization) y el fallback runtime.
    public static class DefaultLocalization
    {
        public static Dictionary<string, string> CreateSpanish()
        {
            return new Dictionary<string, string>
            {
                // Progresion
                { "status.level_up", "Subiste a nivel {0}." },
                { "status.max_level", "Alcanzaste el nivel maximo {0}." },
                { "status.class_active", "Clase activa: {0}" },
                { "feed.attribute_points", "+{0} puntos de atributo" },
                { "net.level_up", "subio a nivel {0}" },
                { "net.status_offline", "Offline" },
                { "net.status_connecting", "Conectando..." },
                { "net.status_online", "Online: {0}" },
                { "net.status_no_connection", "Sin conexion: {0}" },
                { "net.status_server_disconnected", "Servidor desconectado." },
                { "net.status_connection_closed", "Conexion cerrada: {0}" },
                { "net.status_offline_hint", "Offline: inicia el server y presiona ONLINE." },
                { "net.status_send_failed", "Envio fallido: {0}" },
                { "net.chat_system", "Sistema" },
                { "net.chat_valley", "Valle" },
                { "net.chat_server", "Servidor" },
                { "net.connected", "Conectado al servidor local." },
                { "net.chat_placeholder", "mensaje..." },
                { "net.url_placeholder", "server url" },
                { "attr.no_points", "No tienes puntos de atributo disponibles." },
                { "attr.strength_up", "Fuerza {0} (+{1} dano)" },
                { "attr.vitality_up", "Vitalidad {0} (+{1} vida)" },
                { "attr.agility_up", "Agilidad {0} (velocidad y critico)" },

                // Mejora de equipo
                { "upgrade.need_weapon", "Equipa un arma primero (boton EQUIPAR)." },
                { "upgrade.need_armor", "Equipa una pieza de armadura primero (boton EQUIPAR)." },
                { "upgrade.not_ready", "Sistema de mejora no inicializado." },
                { "upgrade.at_max", "{0} ya esta al maximo (+{1})." },
                { "upgrade.need_gold", "Oro insuficiente: necesitas {0}." },
                { "upgrade.need_material", "Necesitas {0} {1}." },
                { "upgrade.rune_used", "Runa de proteccion aplicada" },
                { "upgrade.success", "{0} mejorado a +{1}." },
                { "upgrade.feed_success", "{0} +{1}" },
                { "upgrade.fail_protected", "La mejora fallo, pero la runa protegio {0}." },
                { "upgrade.fail_kept", "La mejora de {0} fallo. El objeto se mantiene." },
                { "upgrade.fail_down", "La mejora fallo: {0} bajo a +{1}." },
                { "upgrade.feed_down", "{0} bajo a +{1}" },
                { "upgrade.destroyed", "La mejora fallo: {0} se destruyo." },
                { "upgrade.feed_destroyed", "{0} destruido en la mejora" },
                { "net.upgrade", "mejoro {0} a +{1}" },
                { "net.rejected", "Accion '{0}' rechazada por el servidor ({1})." },

                // Pociones
                { "potion.none", "No tienes pociones." },
                { "potion.used", "Usaste Pocion menor." },
                { "potion.feed", "Pocion usada: +{0} vida" },

                // Equipamiento
                { "equip.feed", "Equipado: {0}" },
                { "equip.updated", "Equipo actualizado." },
                { "equip.nothing_better", "No hay equipo mejor en el inventario." },
                { "equip.need_level", "Necesitas nivel {0} para {1}." },
                { "equip.wrong_class", "Tu clase no puede usar {0}." },
                { "equip.cannot", "No puedes equipar {0}." },
                { "hud.equipment", "Equipo: {0}" },
                { "hud.no_pieces", "sin piezas" },

                // Inventario
                { "inv.feed_add", "+{0} {1}" },
                { "inv.empty", "Inventario: vacio" },
                { "inv.summary", "Inventario: {0}" },

                // Mascotas
                { "pet.none", "No tienes mascotas." },
                { "pet.summoned", "{0} te acompana (+{1}% EXP, +{2}% oro)." },
                { "pet.feed_summon", "Mascota invocada: {0}" },
                { "pet.dismissed", "{0} descansa." },
                { "pet.feed_dismiss", "Mascota guardada: {0}" },

                // Monturas
                { "mount.none", "No tienes monturas." },
                { "mount.need_level", "Necesitas nivel {0} para montar {1}." },
                { "mount.mounted", "Montaste {0} (velocidad x{1})." },
                { "mount.feed", "Montura: {0}" },
                { "mount.dismounted", "Bajaste de {0}." },

                // Almacen
                { "storage.nothing", "No tienes materiales para guardar." },
                { "storage.deposited", "Guardaste {0} materiales en el almacen." },
                { "storage.feed", "Almacen: +{0} materiales" },
                { "storage.withdrawn", "Retiraste todo del almacen." },
                { "storage.too_far", "Acercate al Almacen del campamento." },

                // NPCs
                { "shop.dialog", "Mercader: bienvenido, viajero. Compra pociones o mejora tu equipo." },
                { "shop.too_far", "Acercate al Mercader del Valle." },
                { "shop.need_gold", "Necesitas {0} oro para comprar pocion." },
                { "shop.bought_potion", "Compraste Pocion menor." },
                { "shop.feed_potion", "Mercader: pocion comprada" },
                { "smith.dialog", "Herrero: traeme oro y materiales y reforzare tu equipo." },
                { "smith.too_far", "Acercate al Herrero del campamento." },
                { "world.shop_label", "Mercader\ncompra pociones" },
                { "world.smith_label", "Herrero\nmejora tu equipo" },
                { "world.storage_label", "Almacen\nguarda materiales" },
                { "world.zone_sign", "{0}\nZona {1}-{2}" },

                // Misiones
                { "quest.none", "Mision: -" },
                { "quest.all_done", "Mision: valle estabilizado. Esperando nuevas tareas." },
                { "quest.summary", "Mision: {0} - {1}" },
                { "quest.feed_progress", "Mision: {0} {1}/{2}" },
                { "quest.completed", "Mision completada: {0}. {1}" },
                { "quest.new", "Nueva mision: {0}" },
                { "reward.feed", "{0}: +{1} EXP, +{2} oro" },

                // Combate
                { "combat.no_target", "No hay enemigos en rango." },
                { "combat.already_dead", "El objetivo ya fue derrotado." },
                { "combat.dodged", "{0} esquivo tu ataque." },
                { "combat.defeated", "{0} derrotado." },
                { "combat.crit", "Golpe critico a {0} por {1}." },
                { "combat.hit", "Golpeaste a {0} por {1}." },
                { "combat.miss_popup", "Fallo" },
                { "combat.dodge_popup", "Esquivado" },

                // Habilidades
                { "skill.one_cooldown", "Habilidad 1 en recarga." },
                { "skill.two_cooldown", "Habilidad 2 en recarga." },
                { "skill.cleave_hit", "Corte pesado golpeo {0} enemigo(s)." },
                { "skill.cleave_miss", "Corte pesado no encontro objetivo." },
                { "skill.ninja_dash_hit", "Estocada veloz conecto." },
                { "skill.ninja_dash", "Estocada veloz." },
                { "skill.no_target", "{0}: sin objetivo." },
                { "skill.hit", "{0} impacto a {1}." },
                { "skill.heal", "Bendicion espiritual: curacion." },
                { "skill.mark_no_target", "Marca del vacio: sin objetivo." },
                { "skill.mark_hit", "Marca del vacio debilito a {0}." },
                { "skill.buff", "{0}: dano aumentado." },
                { "skill.spiritual_bolt", "Rayo espiritual" },
                { "skill.dark_blade", "Hoja oscura" },
                { "skill.shadow_dash", "Sombra fugaz" },
                { "skill.battle_cry", "Grito de batalla" },

                // Recompensas de enemigos
                { "reward.kill", "+{0} EXP, +{1} oro" },
                { "reward.kill_loot", "+{0} EXP, +{1} oro, loot: {2}" },
                { "reward.kill_feed", "+{0} EXP  +{1} oro" },

                // Zonas
                { "zone.boss_respawn", "{0} reaparecera pronto" },
                { "zone.elite_label", "Elite" },
                { "zone.boss_label", "Jefe de zona" },
                { "event.monolith_name", "Monolito Corrupto" },
                { "event.monolith_label", "Monolito Corrupto\nEvento de mapa" },
                { "event.monolith_spawned", "Evento: aparecio el Monolito Corrupto al sur." },
                { "event.monolith_completed", "Evento completado: el monolito reaparecera despues." },

                // HUD
                { "ui.game_title", "Valle de las Reliquias" },
                { "ui.version_subtitle", "Prototipo Android 0.1" },
                { "ui.attack", "ATK" },
                { "ui.potion", "POTION" },
                { "ui.buy_potion", "BUY 25" },
                { "ui.weapon", "WEAPON" },
                { "ui.armor", "ARMOR" },
                { "ui.equip", "EQUIPAR" },
                { "ui.talk", "HABLAR" },
                { "ui.pet", "MASCOTA" },
                { "ui.mount", "MONTURA" },
                { "ui.storage", "ALMACEN" },
                { "ui.stats", "STATS" },
                { "ui.close", "CERRAR" },
                { "ui.online", "ONLINE" },
                { "ui.send", "SEND" },
                { "ui.chat_title", "Chat online local." },
                { "ui.stats_title", "Atributos" },
                { "hud.initial_status", "Fase 5: prototipo Android preparado para pruebas." },
                { "hud.controls_help", "WASD/joystick moverse  |  ATK/Space ataque  |  Q/E habilidades  |  Click derecho camara" },
                { "hud.hint", "Cambia clase, completa la mision, mejora equipo y busca el monolito." },
                { "hud.player_hp", "Jugador {0}/{1}" },
                { "hud.target_hp", "Objetivo {0}/{1}" },
                { "hud.no_target", "Sin objetivo" },
                { "hud.progression", "Nivel {0}  {1}  Oro {2}{3}{4}" },
                { "hud.exp", "EXP {0}/{1}" },
                { "hud.exp_max", "EXP MAX" },
                { "hud.points_suffix", "  Puntos {0}" },
                { "hud.exp_bonus_suffix", "  (+{0}% EXP)" },

                // Identidad / creacion de personaje
                { "identity.default_name", "Heroe" },
                { "identity.gender_male", "Masculino" },
                { "identity.gender_female", "Femenino" },
                { "identity.display", "{0} ({1})" },
                { "identity.name_label", "{0}\n{1} - {2}" },
                { "character.title", "Crear personaje" },
                { "character.subtitle", "Elige clase, sexo y nombre para entrar al Valle de las Reliquias." },
                { "character.name_placeholder", "nombre del personaje" },
                { "character.class_label", "Clase" },
                { "character.gender_label", "Sexo" },
                { "character.create_button", "CREAR" },
                { "character.continue_button", "CONTINUAR" },
                { "character.created", "Personaje creado: {0}" },
                { "character.created_feed", "Entraste como {0}" },
                { "character.welcome_back", "Bienvenido de nuevo: {0}" },
                { "character.loaded_feed", "Partida cargada: nivel {0}" },
                { "character.preview", "{0}  |  {1}  |  {2}" },
                { "class.guerrero", "Guerrero" },
                { "class.ninja", "Ninja" },
                { "class.chaman", "Chaman" },
                { "class.umbra", "Umbra" }
            };
        }
    }
}
