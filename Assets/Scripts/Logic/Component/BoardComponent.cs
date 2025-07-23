using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class BoardComponent : IComponent
    {
        public int[,] Board; // 각 셀에 해당하는 테트리미노 Entity ID (0=빈공간)
    }
}