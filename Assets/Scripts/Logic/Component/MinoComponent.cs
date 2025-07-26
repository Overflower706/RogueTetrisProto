using OVFL.ECS;

namespace Minomino
{
    public class MinoComponent : IComponent
    {
        public int ParentID;
        public MinueType MinueType;
        public MinoColor MinoColor;
        public MinoState State;
    }

    public enum MinueType
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Big = 3,
        ValAngelMineu = 4,
        MulAngelMineu = 5,
        ValDevilMineu = 6,
        MulDevilMineu = 7,
        ValSmallStarMineu = 8,
        ValMidiumStarMineu = 9,
        ValBigStarMineu = 10,
        MulSmallStarMineu = 11,
        MulMidiumStarMineu = 12,
        MulBigStarMineu = 13,
        ValSmallMoonMineu = 14,
        ValMidiumMoonMineu = 15,
        ValBigMoonMineu = 16,
        MulSmallMoonMineu = 17,
        MulMidiumMoonMineu = 18,
        MulBigMoonMineu = 19,
        ValSmallSunMineu = 20,
        ValMidiumSunMineu = 21,
        ValBigSunMineu = 22,
        MulSmallSunMineu = 23,
        MulMidiumSunMineu = 24,
        MulBigSunMineu = 25
    }

    public enum MinoColor
    {
        None = 0,
        Bright = 1,
        Beige = 2,
        Dark = 3,
    }

    public enum MinoState
    {
        None = 0,
        Empty = 1,
        Living = 2,
    }
}