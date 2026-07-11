namespace MmorpgPrototype
{
    // Punto unico de entrega de recompensas: cualquier fuente (mision,
    // subida de nivel, cofres, eventos) pasa por aqui.
    public static class RewardService
    {
        public static void Grant(RewardBundle bundle, PlayerProgression progression, InventorySystem inventory, PrototypeHud hud, string sourceLabel)
        {
            if (bundle == null)
            {
                return;
            }

            if (bundle.Experience > 0)
            {
                progression?.AddExperience(bundle.Experience);
            }

            if (bundle.Gold > 0)
            {
                progression?.AddGold(bundle.Gold);
            }

            if (bundle.Items != null && inventory != null)
            {
                foreach (var item in bundle.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.ItemId) && item.Count > 0)
                    {
                        inventory.AddItem(item.ItemId, item.Count);
                    }
                }
            }

            if (hud != null && (bundle.Experience > 0 || bundle.Gold > 0))
            {
                hud.AddFeed(Localization.Tr("reward.feed", sourceLabel, bundle.Experience, bundle.Gold));
            }
        }
    }
}
