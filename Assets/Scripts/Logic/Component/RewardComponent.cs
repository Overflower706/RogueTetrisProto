using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class RewardComponent : IComponent
    {
        public int Line;
        public bool IsReceived;
        public List<string> Rewards;
    }
}