using System.Collections.Generic;
using UnityEngine;

namespace MmorpgPrototype
{
    // Inventario de atuendos/alas. La tienda usa oro del juego en esta fase;
    // la validacion definitiva de compras se movera al servidor online.
    public sealed class CosmeticService : MonoBehaviour
    {
        public PrototypeHud Hud;
        public PlayerProgression Progression;
        public EquipmentUpgradeSystem UpgradeSystem;
        public PlayerAvatarVisual Avatar;

        private readonly List<CosmeticDefinition> definitions = new List<CosmeticDefinition>();
        private readonly HashSet<string> ownedIds = new HashSet<string>();
        private CosmeticDefinition activeOutfit;
        private CosmeticDefinition activeWings;

        public string ActiveOutfitId => activeOutfit != null ? activeOutfit.CosmeticId : string.Empty;
        public string ActiveWingsId => activeWings != null ? activeWings.CosmeticId : string.Empty;
        public int OwnedCount => ownedIds.Count;
        public int TotalCount => definitions.Count;
        public int DamageBonus => (activeOutfit != null ? activeOutfit.DamageBonus : 0) + (activeWings != null ? activeWings.DamageBonus : 0);
        public int MaxHealthBonus => (activeOutfit != null ? activeOutfit.MaxHealthBonus : 0) + (activeWings != null ? activeWings.MaxHealthBonus : 0);
        public float CritChanceBonus => (activeOutfit != null ? activeOutfit.CritChanceBonus : 0f) + (activeWings != null ? activeWings.CritChanceBonus : 0f);

        public void Initialize(List<CosmeticDefinition> loadedDefinitions)
        {
            definitions.Clear();
            ownedIds.Clear();
            if (loadedDefinitions != null)
            {
                foreach (var definition in loadedDefinitions)
                {
                    if (definition != null && !string.IsNullOrWhiteSpace(definition.CosmeticId))
                    {
                        definitions.Add(definition);
                    }
                }
            }

            if (Find(DefaultCosmetics.TravelerOutfit) != null)
            {
                ownedIds.Add(DefaultCosmetics.TravelerOutfit);
                activeOutfit = Find(DefaultCosmetics.TravelerOutfit);
            }

            ApplyChanges();
        }

        public bool IsOwned(string cosmeticId)
        {
            return !string.IsNullOrWhiteSpace(cosmeticId) && ownedIds.Contains(cosmeticId);
        }

        public List<string> ExportOwnedIds()
        {
            return new List<string>(ownedIds);
        }

        public void RestoreOwned(List<string> savedIds)
        {
            if (savedIds != null && savedIds.Count > 0)
            {
                ownedIds.Clear();
                foreach (var id in savedIds)
                {
                    if (Find(id) != null)
                    {
                        ownedIds.Add(id);
                    }
                }
            }

            if (Find(DefaultCosmetics.TravelerOutfit) != null && ownedIds.Count == 0)
            {
                ownedIds.Add(DefaultCosmetics.TravelerOutfit);
            }
        }

        public void RestoreActive(string outfitId, string wingsId)
        {
            activeOutfit = IsOwned(outfitId) ? Find(outfitId) : Find(DefaultCosmetics.TravelerOutfit);
            activeWings = IsOwned(wingsId) ? Find(wingsId) : null;
            ApplyChanges();
        }

        public void UnlockCosmetic(string cosmeticId, bool announce = true)
        {
            var definition = Find(cosmeticId);
            if (definition == null || !ownedIds.Add(cosmeticId))
            {
                return;
            }

            if (announce)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.unlocked", definition.DisplayName), 4f);
                Hud?.AddFeed(Localization.Tr("cosmetic.feed_unlocked", definition.DisplayName));
            }
        }

        public void TryBuyFeatured()
        {
            CosmeticDefinition featured = null;
            foreach (var definition in definitions)
            {
                if (definition.IsShopItem && !IsOwned(definition.CosmeticId))
                {
                    featured = definition;
                    break;
                }
            }

            if (featured == null)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.shop_empty"));
                return;
            }

            if (Progression == null || Progression.Level < featured.RequiredLevel)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.need_level", featured.RequiredLevel, featured.DisplayName));
                return;
            }

            if (Progression.Gold < featured.ShopPrice)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.need_gold", featured.ShopPrice));
                return;
            }

            if (!Progression.SpendGold(featured.ShopPrice))
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.need_gold", featured.ShopPrice));
                return;
            }
            UnlockCosmetic(featured.CosmeticId, false);
            Hud?.SetStatus(Localization.Tr("cosmetic.bought", featured.DisplayName), 4f);
            Hud?.AddFeed(Localization.Tr("cosmetic.feed_bought", featured.DisplayName, featured.ShopPrice));
        }

        public void ToggleOutfit()
        {
            var owned = FirstOwned(CosmeticSlot.Outfit);
            if (owned == null)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.none"));
                return;
            }

            activeOutfit = activeOutfit == null || activeOutfit.CosmeticId != owned.CosmeticId ? owned : null;
            ApplyChanges();
            Hud?.SetStatus(activeOutfit != null
                ? Localization.Tr("cosmetic.equipped", activeOutfit.DisplayName)
                : Localization.Tr("cosmetic.unequipped", "atuendo"));
        }

        public void ToggleWings()
        {
            var owned = FirstOwned(CosmeticSlot.Wings);
            if (owned == null)
            {
                Hud?.SetStatus(Localization.Tr("cosmetic.wings_none"));
                return;
            }

            activeWings = activeWings == null || activeWings.CosmeticId != owned.CosmeticId ? owned : null;
            ApplyChanges();
            Hud?.SetStatus(activeWings != null
                ? Localization.Tr("cosmetic.equipped", activeWings.DisplayName)
                : Localization.Tr("cosmetic.unequipped", "alas"));
        }

        public string Summary()
        {
            var outfit = activeOutfit != null ? activeOutfit.DisplayName : Localization.Tr("cosmetic.none");
            var wings = activeWings != null ? activeWings.DisplayName : Localization.Tr("cosmetic.wings_none");
            return Localization.Tr("cosmetic.summary", outfit, wings, OwnedCount, TotalCount);
        }

        public CosmeticDefinition GetActive(CosmeticSlot slot)
        {
            return slot == CosmeticSlot.Outfit ? activeOutfit : activeWings;
        }

        private CosmeticDefinition FirstOwned(CosmeticSlot slot)
        {
            foreach (var definition in definitions)
            {
                if (definition.Slot == slot && IsOwned(definition.CosmeticId))
                {
                    return definition;
                }
            }

            return null;
        }

        private CosmeticDefinition Find(string cosmeticId)
        {
            foreach (var definition in definitions)
            {
                if (definition.CosmeticId == cosmeticId)
                {
                    return definition;
                }
            }

            return null;
        }

        private void ApplyChanges()
        {
            Avatar?.ApplyCosmetics(this);
            UpgradeSystem?.ApplyBonuses();
        }
    }
}
