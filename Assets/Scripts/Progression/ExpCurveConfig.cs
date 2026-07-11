using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Parametros de la curva de experiencia. La formula vive aqui, no en la
    // logica de juego: cambiar el balance = editar el asset y regenerar la
    // tabla (menu MMORPG > Progression > Generate Level Table).
    [CreateAssetMenu(menuName = "MMORPG/Progression/Exp Curve Config", fileName = "ExpCurveConfig")]
    public sealed class ExpCurveConfig : ScriptableObject
    {
        [Min(2)] public int MaxLevel = 105;
        [Min(1)] public long BaseExperience = 100;
        [Range(1f, 3f)] public float Exponent = 1.4f;
        [Min(0)] public int AttributePointsPerLevel = 5;

        public long ExpToNext(int level)
        {
            var safeLevel = Math.Max(1, level);
            var raw = BaseExperience * Math.Pow(safeLevel, Exponent);
            var rounded = (long)Math.Round(raw / 10.0) * 10L;
            return Math.Max(1L, rounded);
        }
    }
}
