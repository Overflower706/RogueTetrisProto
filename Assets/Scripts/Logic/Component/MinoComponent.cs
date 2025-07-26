using OVFL.ECS;

namespace Minomino
{
    public class MinoComponent : IComponent
    {
        public int ParentID;
        public MinewType MinoType;
        public MinoColor MinoColor;
        public MinoState State;
    }

    public enum MinewType
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Big = 3,
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