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
                        try
                        {
                            // Entity 찾기 (ID로 직접 검색)
                            Entity entity = FindEntityById(context, entityId);
                            // entity ID와 이 Entity가 Context.GetEntities()에서의 index와 일치하는지 확인
                            if (entity == null)
                            {
                                Debug.LogWarning($"Entity ID {entityId}를 찾을 수 없습니다. 위치: ({x}, {y})");
                                sb.Append("?"); // 오류 표시
                                continue;
                            }

                            var minoComponent = entity.GetComponent<MinoComponent>();
                            if (minoComponent == null)
                            {
                                Debug.LogWarning($"Entity {entityId}에 MinoComponent가 없습니다. 위치: ({x}, {y})");
                                sb.Append("E"); // 에러 블록 표시
                                continue;
                            }
                            var tetriminoComponent = FindEntityById(context, minoComponent.ParentID).GetComponent<TetriminoComponent>();

                            TetriminoColor color = tetriminoComponent.Color;
                            sb.Append(GetColorBlock(color));
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Entity {entityId} 처리 중 오류 발생 (위치: {x}, {y}): {ex.Message}");
                            sb.Append("X"); // 예외 표시
                        }
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


        /// <summary>
        /// Entity ID로 Entity를 안전하게 찾기
        /// </summary>
        private static Entity FindEntityById(Context context, int entityId)
        {
            try
            {
                // GetEntities()로 직접 인덱스 접근 대신 안전한 검색
                var allEntities = context.GetEntities();

                foreach (var entity in allEntities)
                {
                    if (entity != null && entity.ID == entityId)
                    {
                        return entity;
                    }
                }

                return null; // 찾지 못함
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"FindEntityById({entityId}) 오류: {ex.Message}");
                return null;
            }
        }
    }
}