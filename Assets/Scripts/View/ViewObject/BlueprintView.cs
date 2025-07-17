using System.Collections.Generic;
using UnityEngine;

namespace Minomino
{
    public class BlueprintView : MonoBehaviour
    {
        [Header("Mino Grid")]
        [SerializeField] private List<MinoView> minoGrid = new List<MinoView>();

        /// <summary>
        /// 테트리미노 모양을 5x5 그리드에 그리기
        /// </summary>
        public void Refresh(TetriminoComponent tetrimino)
        {
            // 모든 미노 먼저 비활성화
            Clear();

            // 테트리미노 모양 그리기
            DrawTetrimino(tetrimino);
        }

        /// <summary>
        /// 그리드의 모든 미노 비활성화
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    minoGrid[x * 5 + y].Refresh(TetriminoColor.None);
                }
            }
        }

        /// <summary>
        /// 테트리미노 모양을 그리드에 그리기
        /// </summary>
        private void DrawTetrimino(TetriminoComponent tetrimino)
        {
            foreach (Vector2Int pos in tetrimino.Shape)
            {
                // 5x5 그리드의 중앙을 (2, 2)로 설정
                int gridX = pos.x + 2;
                int gridY = pos.y + 2;

                // 그리드 범위 확인
                if (gridX >= 0 && gridX < 5 && gridY >= 0 && gridY < 5)
                {
                    minoGrid[gridX * 5 + gridY].Refresh(tetrimino.Color);
                    Debug.Log($"그리드 위치 ({gridX}, {gridY})에 테트리미노 색상 {tetrimino.Color} 적용");
                }
                else
                {
                    Debug.LogWarning($"테트리미노 블록이 5x5 그리드를 벗어났습니다: ({gridX}, {gridY})");
                }
            }

            Debug.Log($"BlueprintView가 새 테트리미노 {tetrimino.Type}로 업데이트되었습니다.");
        }
    }
}