using OVFL.ECS;

namespace Minomino
{
    public class BoardComponent : IComponent
    {
        public static int WIDTH => GlobalSettings.Instance.BoardWidth;
        public static int HEIGHT => GlobalSettings.Instance.BoardHeight;

        public int[,] Board; // 각 셀에 해당하는 테트리미노 Entity ID (0=빈공간)

        public BoardComponent()
        {
            Board = new int[WIDTH, HEIGHT];
        }
    }
}