using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class TetriminoComponent : IComponent
    {
        public TetriminoType Type;      // 테트리미노 종류 (I, O, T, S, Z, J, L)
        public Vector2Int[] Shape;      // 테트리미노 형태
        public int Rotation;            // 0, 1, 2, 3 (90도씩)
        public int ColorValue;          // 1~4 색상 값
    }

    public enum TetriminoType
    {
        I, O, T, S, Z, J, L
    }
}