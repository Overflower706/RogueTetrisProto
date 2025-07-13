using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class TetriminoQueueComponent : IComponent
    {
        public Queue<Entity> TetriminoQueue;
    }
}