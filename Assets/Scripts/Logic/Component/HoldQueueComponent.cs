using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class HoldQueueComponent : IComponent
    {
        public Queue<Entity> HoldQueue;
    }
}