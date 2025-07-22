using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class TetrominoQueueComponent : IComponent
    {
        public Queue<Entity> TetrominoQueue;
    }
}