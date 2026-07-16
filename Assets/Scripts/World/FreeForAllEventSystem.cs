using System;
using UnityEngine;

namespace MmorpgPrototype
{
    [Serializable]
    public sealed class FreeForAllEventSaveData
    {
        public string MatchId = string.Empty;
        public int Score;
        public int Kills;
        public int Deaths;
        public bool Joined;
    }

    // No simula rivales localmente: deja el contrato listo para matchmaking,
    // resultados firmados y ranking cuando S-01 tenga autoridad de combate.
    public sealed class FreeForAllEventSystem : MonoBehaviour
    {
        public PrototypeHud Hud;
        public PlayerPersistence Persistence;

        public string MatchId { get; private set; } = string.Empty;
        public int Score { get; private set; }
        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public bool Joined { get; private set; }

        public void Initialize() { }

        public void Restore(FreeForAllEventSaveData data)
        {
            MatchId = data != null ? data.MatchId : string.Empty;
            Score = data != null ? Mathf.Max(0, data.Score) : 0;
            Kills = data != null ? Mathf.Max(0, data.Kills) : 0;
            Deaths = data != null ? Mathf.Max(0, data.Deaths) : 0;
            Joined = data != null && data.Joined;
        }

        public FreeForAllEventSaveData Export()
        {
            return new FreeForAllEventSaveData
            {
                MatchId = MatchId,
                Score = Score,
                Kills = Kills,
                Deaths = Deaths,
                Joined = Joined
            };
        }

        public void ApplyServerResult(string matchId, int score, int kills, int deaths)
        {
            MatchId = matchId ?? string.Empty;
            Score = Mathf.Max(0, score);
            Kills = Mathf.Max(0, kills);
            Deaths = Mathf.Max(0, deaths);
            Joined = true;
            Hud?.RefreshQuest();
            Persistence?.SaveNow();
        }

        public string Summary()
        {
            if (!Joined)
            {
                return Localization.Tr("event.ffa_ready");
            }

            return Localization.Tr("event.ffa_summary", Kills, Deaths, Score);
        }
    }
}
