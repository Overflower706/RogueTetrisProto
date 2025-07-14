using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BoardTetriminoComponent : IComponent
    {
        public BoardTetriminoState State;    // 현재 상태 (현재, 보류, 고정 등)
        public Vector2Int Position;    // 보드 상의 위치
        public int Rotation;            // 0, 1, 2, 3 (90도씩)
    }

    public enum BoardTetriminoState
    {
        None,
        Current,
        Hold
    }
}