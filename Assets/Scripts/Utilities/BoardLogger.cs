using UnityEngine;
using System.Text;
using OVFL.ECS;

namespace Minomino
{
    /// <summary>
    /// 게임 상태를 시각화해서 로그에 출력하는 유틸리티 클래스
    /// </summary>
    public static class BoardLogger
    {
        /// <summary>
        /// Game 객체의 상태를 시각화해서 Debug.Log로 출력
        /// </summary>
        /// <param name="game">로그할 Game 객체</param>
        /// <param name="title">로그 제목 (선택사항)</param>
        private static void LogGame(Context context, BoardComponent board)
        {
            StringBuilder sb = new StringBuilder();

            // 헤더
            sb.AppendLine($"=== Board Log ===");

            // 보드 시각화
            sb.AppendLine();
            sb.AppendLine("Board:");
            sb.AppendLine(VisualizeBoard(context, board));

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// 테트리스 보드를 ASCII 문자로 시각화
        /// </summary>
        /// <param name="board">시각화할 보드</param>
        /// <param name="currentTetrimino">현재 테트리미노 (활성 블록 표시용)</param>
        /// <returns>시각화된 보드 문자열</returns>
        public static string VisualizeBoard(Context context, BoardComponent board)
        {
            StringBuilder sb = new StringBuilder();

            int width = GlobalSettings.Instance.BoardWidth;
            int height = GlobalSettings.Instance.BoardHeight;

            // 상단 테두리
            sb.Append("┌");
            for (int x = 0; x < width; x++)
            {
                sb.Append("─");
            }
            sb.AppendLine("┐");

            // 보드 내용 (위에서 아래로)
            for (int y = height - 1; y >= 0; y--)
            {
                sb.Append("│");
                for (int x = 0; x < width; x++)
                {
                    int entityId = board.Board[x, y];
                    if (entityId == 0)
                    {
                        sb.Append("□"); // 빈 칸
                    }
                    else
                    {
                        var entity = context.GetEntities()[entityId];
                        var tetriminoComponent = entity.GetComponent<TetriminoComponent>();
                        TetriminoColor color = tetriminoComponent.Color;
                        sb.Append(GetColorBlock(color));
                    }
                }
                sb.AppendLine("│");
            }

            // 하단 테두리
            sb.Append("└");
            for (int x = 0; x < width; x++)
            {
                sb.Append("─");
            }
            sb.AppendLine("┘");

            return sb.ToString();
        }

        /// <summary>
        /// 색상에 따른 블록 문자 반환 (색상 코드 적용)
        /// </summary>
        private static string GetColorBlock(TetriminoColor color)
        {
            string colorCode = GetColorName(color);
            return $"<color={colorCode}>■</color>";
        }

        private static string GetColorName(TetriminoColor color)
        {
            return color switch
            {
                TetriminoColor.Red => "#8B0000",      // 진한 빨강 (DarkRed)
                TetriminoColor.Green => "#006400",    // 진한 초록 (DarkGreen)
                TetriminoColor.Blue => "#000080",     // 진한 파랑 (Navy)
                TetriminoColor.Yellow => "#B8860B",   // 진한 노랑 (DarkGoldenrod)
                _ => "#2F2F2F"                        // 진한 회색
            };
        }


        // /// <summary>
        // /// 현재 테트리미노가 떨어질 최종 위치를 계산
        // /// </summary>
        // /// <param name="board">게임 보드</param>
        // /// <param name="tetrimino">현재 테트리미노</param>
        // /// <returns>떨어질 위치의 테트리미노 블록들</returns>
        // private static Vector2Int[] CalculateGhostPiecePositions(TetrisBoard board, Tetrimino tetrimino)
        // {
        //     if (board == null || tetrimino == null) return null;

        //     // 현재 테트리미노의 회전된 로컬 위치들을 계산
        //     Vector2Int[] rotatedShape = new Vector2Int[tetrimino.shape.Length];
        //     for (int i = 0; i < tetrimino.shape.Length; i++)
        //     {
        //         rotatedShape[i] = RotatePoint(tetrimino.shape[i], tetrimino.rotation);
        //     }

        //     // 현재 위치에서 아래로 떨어뜨려 봄
        //     Vector2Int testPosition = tetrimino.position;

        //     // 아래로 계속 떨어뜨려서 충돌할 때까지 반복
        //     while (true)
        //     {
        //         testPosition.y--;

        //         // 이 위치에서 충돌하는지 확인
        //         bool collision = false;
        //         foreach (var localPos in rotatedShape)
        //         {
        //             Vector2Int worldPos = testPosition + localPos;

        //             // 보드 경계 확인
        //             if (worldPos.y < 0 || worldPos.x < 0 || worldPos.x >= TetrisBoard.WIDTH)
        //             {
        //                 collision = true;
        //                 break;
        //             }

        //             // 다른 블록과 충돌 확인
        //             if (worldPos.y < TetrisBoard.HEIGHT && board.grid[worldPos.x, worldPos.y] != 0)
        //             {
        //                 collision = true;
        //                 break;
        //             }
        //         }

        //         // 충돌했다면 이전 위치가 최종 위치
        //         if (collision)
        //         {
        //             testPosition.y++; // 한 칸 위로 되돌림
        //             break;
        //         }
        //     }

        //     // 최종 위치에서의 블록 위치들 계산
        //     Vector2Int[] ghostPositions = new Vector2Int[rotatedShape.Length];
        //     for (int i = 0; i < rotatedShape.Length; i++)
        //     {
        //         ghostPositions[i] = testPosition + rotatedShape[i];
        //     }

        //     return ghostPositions;
        // }
    }
}