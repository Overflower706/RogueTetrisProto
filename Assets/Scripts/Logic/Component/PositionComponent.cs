using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class PositionComponent : IComponent
    {
        public Vector2Int GridPosition; // 그리드상의 위치
    }
}