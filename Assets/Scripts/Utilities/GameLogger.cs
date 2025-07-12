using UnityEngine;
using System.Text;

/// <summary>
/// 게임 상태를 시각화해서 로그에 출력하는 유틸리티 클래스
/// </summary>
public static class GameLogger
{
    /// <summary>
    /// Game 객체의 상태를 시각화해서 Debug.Log로 출력
    /// </summary>
    /// <param name="game">로그할 Game 객체</param>
    /// <param name="title">로그 제목 (선택사항)</param>
    public static void LogGame(Game game, string title = "Game State")
    {
        if (game == null)
        {
            Debug.Log($"[{title}] Game is null");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // 헤더
        sb.AppendLine($"=== {title} ===");
        sb.AppendLine($"State: {game.CurrentState}");
        sb.AppendLine($"Score: {game.CurrentScore}/{game.TargetScore}");
        sb.AppendLine($"Currency: {game.Currency}");
        sb.AppendLine($"Time: {game.GameTime:F1}s");
        sb.AppendLine($"Active Effects: {game.ActiveEffects?.Count ?? 0}");

        // 현재 테트리미노 정보
        if (game.CurrentTetrimino != null)
        {
            sb.AppendLine($"Current Tetrimino: {game.CurrentTetrimino.type} at ({game.CurrentTetrimino.position.x}, {game.CurrentTetrimino.position.y}) rotation={game.CurrentTetrimino.rotation}");
        }
        else
        {
            sb.AppendLine("Current Tetrimino: None");
        }

        if (game.NextTetrimino != null)
        {
            sb.AppendLine($"Next Tetrimino: {game.NextTetrimino.type}");
        }

        // 보드 시각화
        sb.AppendLine();
        sb.AppendLine("Board:");
        sb.AppendLine(VisualizeBoard(game.Board, game.CurrentTetrimino));

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// 테트리스 보드를 ASCII 문자로 시각화
    /// </summary>
    /// <param name="board">시각화할 보드</param>
    /// <param name="currentTetrimino">현재 테트리미노 (활성 블록 표시용)</param>
    /// <returns>시각화된 보드 문자열</returns>
    private static string VisualizeBoard(TetrisBoard board, Tetrimino currentTetrimino = null)
    {
        if (board == null) return "Board is null";

        StringBuilder sb = new StringBuilder();

        // 현재 테트리미노의 위치들을 미리 계산
        Vector2Int[] currentTetriminoPositions = null;
        if (currentTetrimino != null)
        {
            currentTetriminoPositions = currentTetrimino.GetWorldPositions();
        }

        // Ghost piece 위치들 계산
        Vector2Int[] ghostPiecePositions = null;
        if (currentTetrimino != null)
        {
            ghostPiecePositions = CalculateGhostPiecePositions(board, currentTetrimino);
        }

        // 상단 경계선
        sb.Append("+");
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            sb.Append("--");
        }
        sb.AppendLine("+");

        // 보드를 위에서부터 아래로 그리기 (y가 큰 것부터)
        for (int y = TetrisBoard.HEIGHT - 1; y >= 0; y--)
        {
            sb.Append("|");

            for (int x = 0; x < TetrisBoard.WIDTH; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                bool isCurrentTetrimino = IsPositionInTetrimino(pos, currentTetriminoPositions);
                bool isGhostPiece = IsPositionInTetrimino(pos, ghostPiecePositions);
                bool isBoardBlock = board.grid[x, y] != 0;

                if (isCurrentTetrimino && isBoardBlock)
                {
                    // 겹침 (충돌 상황)
                    sb.Append("X");
                }
                else if (isCurrentTetrimino)
                {
                    // 현재 테트리미노 - 색상별 표시
                    sb.Append($"<color={GetColorName(currentTetrimino.color)}>■</color>");
                }
                else if (isGhostPiece && !isBoardBlock)
                {
                    // Ghost piece - 현재 테트리미노와 겹치지 않고 빈 공간인 경우에만 표시
                    sb.Append($"<color={GetColorName(currentTetrimino.color)}>▤</color>");
                }
                else if (isBoardBlock)
                {
                    // 고정된 블록 - 색상별 표시
                    int blockColor = board.grid[x, y];
                    sb.Append($"<color={GetColorName(blockColor)}>■</color>");
                }
                else
                {
                    // 빈 공간
                    sb.Append("□");
                }
            }

            sb.Append("│");

            // 오른쪽에 y좌표 표시 (5칸마다)
            if (y % 5 == 0)
            {
                sb.Append($" {y:D2}");
            }

            sb.AppendLine();
        }

        // 하단 경계선
        sb.Append("└");
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            sb.Append("--");
        }
        sb.AppendLine("┘");

        // x좌표 표시
        sb.Append(" ");
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            sb.Append($"{x % 10} ");
        }
        sb.AppendLine();

        // 범례
        sb.AppendLine();
        sb.AppendLine("Legend: <color=red>■</color>=현재 테트리미노, <color=red>▤</color>=떨어질 위치, <color=red>■</color>=고정 블록, X=충돌, □=빈 공간");

        return sb.ToString();
    }

    /// <summary>
    /// 주어진 위치가 테트리미노의 블록 위치인지 확인
    /// </summary>
    private static bool IsPositionInTetrimino(Vector2Int position, Vector2Int[] tetriminoPositions)
    {
        if (tetriminoPositions == null) return false;

        foreach (var tetriminoPos in tetriminoPositions)
        {
            if (tetriminoPos.x == position.x && tetriminoPos.y == position.y)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 현재 테트리미노가 떨어질 최종 위치를 계산
    /// </summary>
    /// <param name="board">게임 보드</param>
    /// <param name="tetrimino">현재 테트리미노</param>
    /// <returns>떨어질 위치의 테트리미노 블록들</returns>
    private static Vector2Int[] CalculateGhostPiecePositions(TetrisBoard board, Tetrimino tetrimino)
    {
        if (board == null || tetrimino == null) return null;

        // 현재 테트리미노의 회전된 로컬 위치들을 계산
        Vector2Int[] rotatedShape = new Vector2Int[tetrimino.shape.Length];
        for (int i = 0; i < tetrimino.shape.Length; i++)
        {
            rotatedShape[i] = RotatePoint(tetrimino.shape[i], tetrimino.rotation);
        }

        // 현재 위치에서 아래로 떨어뜨려 봄
        Vector2Int testPosition = tetrimino.position;

        // 아래로 계속 떨어뜨려서 충돌할 때까지 반복
        while (true)
        {
            testPosition.y--;

            // 이 위치에서 충돌하는지 확인
            bool collision = false;
            foreach (var localPos in rotatedShape)
            {
                Vector2Int worldPos = testPosition + localPos;

                // 보드 경계 확인
                if (worldPos.y < 0 || worldPos.x < 0 || worldPos.x >= TetrisBoard.WIDTH)
                {
                    collision = true;
                    break;
                }

                // 다른 블록과 충돌 확인
                if (worldPos.y < TetrisBoard.HEIGHT && board.grid[worldPos.x, worldPos.y] != 0)
                {
                    collision = true;
                    break;
                }
            }

            // 충돌했다면 이전 위치가 최종 위치
            if (collision)
            {
                testPosition.y++; // 한 칸 위로 되돌림
                break;
            }
        }

        // 최종 위치에서의 블록 위치들 계산
        Vector2Int[] ghostPositions = new Vector2Int[rotatedShape.Length];
        for (int i = 0; i < rotatedShape.Length; i++)
        {
            ghostPositions[i] = testPosition + rotatedShape[i];
        }

        return ghostPositions;
    }

    /// <summary>
    /// 점을 주어진 회전 수만큼 시계방향으로 회전
    /// </summary>
    /// <param name="point">회전할 점</param>
    /// <param name="rotation">회전 수 (90도 단위)</param>
    /// <returns>회전된 점</returns>
    private static Vector2Int RotatePoint(Vector2Int point, int rotation)
    {
        for (int i = 0; i < rotation; i++)
        {
            int temp = point.x;
            point.x = point.y;
            point.y = -temp;
        }
        return point;
    }

    /// <summary>
    /// 간단한 보드 상태만 로그 (디버깅용)
    /// </summary>
    /// <param name="board">로그할 보드</param>
    /// <param name="title">로그 제목</param>
    public static void LogBoard(TetrisBoard board, string title = "Board State")
    {
        Debug.Log($"=== {title} ===\n{VisualizeBoard(board)}");
    }

    /// <summary>
    /// 테트리미노 정보만 로그
    /// </summary>
    /// <param name="tetrimino">로그할 테트리미노</param>
    /// <param name="title">로그 제목</param>
    public static void LogTetrimino(Tetrimino tetrimino, string title = "Tetrimino Info")
    {
        if (tetrimino == null)
        {
            Debug.Log($"[{title}] Tetrimino is null");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== {title} ===");
        sb.AppendLine($"Type: {tetrimino.type}");
        sb.AppendLine($"Position: ({tetrimino.position.x}, {tetrimino.position.y})");
        sb.AppendLine($"Rotation: {tetrimino.rotation}");

        sb.AppendLine("World Positions:");
        var worldPositions = tetrimino.GetWorldPositions();
        for (int i = 0; i < worldPositions.Length; i++)
        {
            sb.AppendLine($"  Block {i}: ({worldPositions[i].x}, {worldPositions[i].y})");
        }

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Game 객체의 상태를 문자열로 반환 (UI 표시용)
    /// </summary>
    /// <param name="game">상태를 가져올 Game 객체</param>
    /// <param name="title">상태 제목 (선택사항)</param>
    /// <returns>게임 상태를 나타내는 문자열</returns>
    public static string GetGameStateString(Game game, string title = "Game State")
    {
        if (game == null)
        {
            return $"[{title}] Game is null";
        }

        StringBuilder sb = new StringBuilder();

        // 헤더
        sb.AppendLine($"=== {title} ===");
        sb.AppendLine($"State: {game.CurrentState}");
        sb.AppendLine($"Score: {game.CurrentScore}/{game.TargetScore}");
        sb.AppendLine($"Currency: {game.Currency}");
        sb.AppendLine($"Time: {game.GameTime:F1}s");
        sb.AppendLine($"Active Effects: {game.ActiveEffects?.Count ?? 0}");

        // 현재 테트리미노 정보
        if (game.CurrentTetrimino != null)
        {
            sb.AppendLine($"Current Tetrimino: {game.CurrentTetrimino.type} at ({game.CurrentTetrimino.position.x}, {game.CurrentTetrimino.position.y}) rotation={game.CurrentTetrimino.rotation}");
        }
        else
        {
            sb.AppendLine("Current Tetrimino: None");
        }

        if (game.NextTetrimino != null)
        {
            sb.AppendLine($"Next Tetrimino: {game.NextTetrimino.type}");
        }        // 보드 시각화
        sb.AppendLine();
        sb.AppendLine("Board:");
        sb.Append(VisualizeBoard(game.Board, game.CurrentTetrimino));

        return sb.ToString();
    }

    /// <summary>
    /// 보드 상태만 문자열로 반환 (UI 표시용)
    /// </summary>
    /// <param name="board">시각화할 보드</param>
    /// <param name="currentTetrimino">현재 테트리미노 (활성 블록 표시용)</param>
    /// <returns>보드만 시각화된 문자열</returns>
    public static string GetBoardString(TetrisBoard board, Tetrimino currentTetrimino = null)
    {
        return VisualizeBoard(board, currentTetrimino);
    }

    private static string GetColorName(int color)
    {
        switch (color)
        {
            case 1: return "#8B0000"; // 진한 빨강 (DarkRed)
            case 2: return "#006400"; // 진한 초록 (DarkGreen)
            case 3: return "#000080"; // 진한 파랑 (Navy)
            case 4: return "#B8860B"; // 진한 노랑 (DarkGoldenrod)
            default: return "#2F2F2F"; // 진한 회색
        }
    }
}
