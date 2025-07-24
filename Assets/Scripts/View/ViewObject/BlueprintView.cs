using System.Collections.Generic;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BlueprintView : MonoBehaviour
    {
        [Header("미노 아이콘 배열")]
        [SerializeField] private List<MinoIconView> minoGrid = new List<MinoIconView>();

        /// <summary>
        /// 테트리미노 모양을 5x5 그리드에 그리기
        /// </summary>
        public void Refresh(Context context, TetrominoComponent tetrimino)
        {
            // 모든 미노 먼저 비활성화
            Clear();

            // 테트리미노 모양 그리기
            DrawTetromino(context, tetrimino);
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
                    minoGrid[x * 5 + y].Refresh();
                }
            }
        }

        /// <summary>
        /// 테트리미노 모양을 그리드에 그리기
        /// </summary>
        private void DrawTetromino(Context context, TetrominoComponent tetromino)
        {
            if (tetromino == null)
            {
                Clear();
                return;
            }

            for (int i = 0; i < tetromino.Shape.Length; i++)
            {
                Vector2Int pos = tetromino.Shape[i];

                // 5x5 그리드의 중앙을 (2, 2)로 설정
                int gridX = pos.x + 2;
                int gridY = pos.y + 2;

                // 그리드 범위 확인
                if (gridX >= 0 && gridX < 5 && gridY >= 0 && gridY < 5)
                {
                    var minoEntity = context.FindEntityByID(tetromino.Minos[i]);
                    minoGrid[gridX * 5 + gridY].Refresh(minoEntity.GetComponent<MinoComponent>());
                }
                else
                {
                    Debug.LogWarning($"테트로미노 블록이 5x5 그리드를 벗어났습니다: ({gridX}, {gridY})");
                }
            }
        }
    }
}