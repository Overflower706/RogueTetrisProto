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

            int width = GlobalSettings.Instance.SafeWidth;
            int height = GlobalSettings.Instance.BoardHeight;
            int safeHeight = GlobalSettings.Instance.SafeHeight;

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
                // SafeHeight와 BufferHeight 경계에 고도 제한 표시
                if (y == safeHeight - 1)
                {
                    sb.Append("├");
                    // 고도 제한 문구 중앙 정렬
                    string limitText = "고도 제한";
                    int padding = (width - limitText.Length) / 2;

                    // 좌측 패딩
                    for (int i = 0; i < padding; i++)
                    {
                        sb.Append("─");
                    }

                    // 고도 제한 텍스트
                    sb.Append(limitText);

                    // 우측 패딩 (남은 공간 채우기)
                    for (int i = padding + limitText.Length; i < width; i++)
                    {
                        sb.Append("─");
                    }
                    sb.AppendLine("┤");
                }

                sb.Append("│");
                for (int x = 0; x < width; x++)
                {
                    int entityID = board.Board[x, y];
                    if (entityID == 0)
                    {
                        sb.Append("□"); // 빈 칸
                    }
                    else
                    {
                        try
                        {
                            // Entity 찾기 (ID로 직접 검색)
                            Entity entity = context.FindEntityByID(entityID);
                            // entity ID와 이 Entity가 Context.GetEntities()에서의 index와 일치하는지 확인
                            if (entity == null)
                            {
                                Debug.LogWarning($"Entity ID {entityID}를 찾을 수 없습니다. 위치: ({x}, {y})");
                                sb.Append("?"); // 오류 표시
                                continue;
                            }

                            var minoComponent = entity.GetComponent<MinoComponent>();
                            if (minoComponent == null)
                            {
                                Debug.LogWarning($"Entity {entityID}에 MinoComponent가 없습니다. 위치: ({x}, {y})");
                                sb.Append("E"); // 에러 블록 표시
                                continue;
                            }

                            MinoState state = minoComponent.State;
                            sb.Append(GetColorByState(state));
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Entity {entityID} 처리 중 오류 발생 (위치: {x}, {y}): {ex.Message}");
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
        private static string GetColorByState(MinoState state)
        {
            string colorCode = GetColorNameByState(state);
            return $"<color={colorCode}>■</color>";
        }

        private static string GetColorNameByState(MinoState state)
        {
            return state switch
            {
                MinoState.None => "#8B0000",      // 진한 빨강 (DarkRed)
                MinoState.Empty => "#2F2F2F",     // 진한 회색
                MinoState.Living => "#00FF00",    // 밝은 초록
                _ => "#FFFFFF" // 기본 흰색
            };
        }
    }
}