using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class TetrominoComponent : IComponent
    {
        public TetrominoType Type;      // 테트리미노 종류 (I, O, T, S, Z, J, L)
        public Vector2Int[] Shape;      // 테트리미노 형태
        public int[] Minos; // 해당 테트리미노에 속하는 미노들의 Entity ID 배열
    }

    public enum TetrominoType
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
}