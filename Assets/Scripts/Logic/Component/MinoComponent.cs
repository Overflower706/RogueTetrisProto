using OVFL.ECS;

namespace Minomino
{
    public class MinoComponent : IComponent
    {
        public int ParentID;
        public MinoState State;
    }

    public enum MinoState
    {
        None = 0,
        Empty = 1,
        Living = 2,
    }
}