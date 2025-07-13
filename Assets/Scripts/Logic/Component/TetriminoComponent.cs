using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class TetriminoComponent : IComponent
    {
        public TetriminoType Type;      // 테트리미노 종류 (I, O, T, S, Z, J, L)
        public Vector2Int[] Shape;      // 테트리미노 형태
        public int Rotation;            // 0, 1, 2, 3 (90도씩)
        public TetriminoColor Color;          // 1~4 색상 값
    }

    public enum TetriminoType
    {
        None = 0,
        I = 1,   // I 테트리미노
        O = 2,   // O 테트리미노
        T = 3,   // T 테트리미노
        S = 4,   // S 테트리미노
        Z = 5,   // Z 테트리미노
        J = 6,   // J 테트리미노
        L = 7    // L 테트리미노
    }

    public enum TetriminoColor
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4
    }
}