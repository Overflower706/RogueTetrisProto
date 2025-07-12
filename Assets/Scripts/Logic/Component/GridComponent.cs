using OVFL.ECS;

namespace Minomino
{
    public class GridComponent : IComponent
    {
        public const int WIDTH = 10;
        public const int HEIGHT = 20;

        public int[,] Grid; // 각 셀에 해당하는 테트리미노 Entity ID (0=빈공간)

        public GridComponent()
        {
            Grid = new int[WIDTH, HEIGHT];
        }
    }
}