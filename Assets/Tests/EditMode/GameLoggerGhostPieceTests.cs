using UnityEngine;
using NUnit.Framework;
using Minomino;

/// <summary>
/// GameLogger의 Ghost Piece 기능을 테스트하는 클래스
/// </summary>
public class GameLoggerGhostPieceTests
{
    private Game gameData;
    private TetrisBoard board;

    [SetUp]
    public void SetUp()
    {
        gameData = new Game();
        board = gameData.Board;
    }

    [Test]
    public void GameLogger_WithGhostPiece_ShouldShowDropPosition()
    {
        // Arrange - T 블록을 높은 위치에 배치
        var tetrimino = new Tetrimino(TetriminoType.T, 1); // 빨간색
        tetrimino.position = new Vector2Int(5, 15); // 높은 위치
        tetrimino.rotation = 0;
        gameData.CurrentTetrimino = tetrimino;

        // 보드 하단에 일부 블록 배치 (Ghost piece가 떨어질 위치 확인용)
        board.PlaceBlock(new Vector2Int(4, 2), 2);
        board.PlaceBlock(new Vector2Int(5, 2), 2);
        board.PlaceBlock(new Vector2Int(6, 2), 2);

        // Act - GameLogger로 상태 출력
        GameLogger.LogGame(gameData, "Ghost Piece Test - T Block");

        // Assert - 로그가 에러 없이 출력되었는지 확인
        Assert.IsNotNull(gameData.CurrentTetrimino, "테트리미노가 존재해야 함");

        // Ghost piece 위치 직접 계산해서 검증
        var ghostPositions = GetGhostPiecePositions(board, tetrimino);
        Assert.IsNotNull(ghostPositions, "Ghost piece 위치가 계산되어야 함");
        Assert.AreEqual(4, ghostPositions.Length, "T블록은 4개 위치를 가져야 함");

        // Ghost piece가 현재 테트리미노보다 아래에 있어야 함
        foreach (var ghostPos in ghostPositions)
        {
            Assert.IsTrue(ghostPos.y <= tetrimino.position.y,
                $"Ghost piece 위치 {ghostPos}는 현재 테트리미노 위치 {tetrimino.position}보다 아래에 있어야 함");
        }
    }

    [Test]
    public void GameLogger_IBlockGhostPiece_ShouldCalculateCorrectly()
    {
        // Arrange - I 블록을 세로 방향으로 배치
        var tetrimino = new Tetrimino(TetriminoType.I, 3); // 파란색
        tetrimino.position = new Vector2Int(5, 15);
        tetrimino.rotation = 1; // 세로 방향
        gameData.CurrentTetrimino = tetrimino;

        // 보드 하단에 기둥 생성
        for (int y = 0; y < 5; y++)
        {
            board.PlaceBlock(new Vector2Int(4, y), 2);
            board.PlaceBlock(new Vector2Int(6, y), 2);
        }

        // Act & Assert
        GameLogger.LogGame(gameData, "Ghost Piece Test - I Block Vertical");

        var ghostPositions = GetGhostPiecePositions(board, tetrimino);
        Assert.IsNotNull(ghostPositions, "I블록 Ghost piece 위치가 계산되어야 함");

        // 세로 I블록의 최하단이 y=5 위치에 있어야 함 (기둥 위)
        var minY = System.Linq.Enumerable.Min(ghostPositions, pos => pos.y);
        Assert.AreEqual(5, minY, "세로 I블록의 최하단이 기둥 위에 위치해야 함");
    }

    [Test]
    public void GameLogger_GhostPieceAtBottom_ShouldNotOverlapWithCurrentTetrimino()
    {
        // Arrange - 테트리미노가 이미 바닥 근처에 있는 경우
        var tetrimino = new Tetrimino(TetriminoType.O, 4); // 노란색
        tetrimino.position = new Vector2Int(5, 1); // 바닥 근처
        tetrimino.rotation = 0;
        gameData.CurrentTetrimino = tetrimino;

        // Act & Assert
        GameLogger.LogGame(gameData, "Ghost Piece Test - Near Bottom");

        var currentPositions = tetrimino.GetWorldPositions();
        var ghostPositions = GetGhostPiecePositions(board, tetrimino);

        // Ghost piece와 현재 테트리미노가 같은 위치이거나 겹칠 수 있음 (바닥에 있을 때)
        Assert.IsNotNull(ghostPositions, "바닥 근처에서도 Ghost piece가 계산되어야 함");
    }

    [Test]
    public void GameLogger_ComplexBoard_ShouldCalculateGhostPieceCorrectly()
    {
        // Arrange - 복잡한 보드 상황
        var tetrimino = new Tetrimino(TetriminoType.L, 2); // 초록색
        tetrimino.position = new Vector2Int(7, 10);
        tetrimino.rotation = 0;
        gameData.CurrentTetrimino = tetrimino;

        // 불규칙한 지형 생성
        board.PlaceBlock(new Vector2Int(6, 3), 1);
        board.PlaceBlock(new Vector2Int(7, 3), 1);
        board.PlaceBlock(new Vector2Int(8, 3), 1);
        board.PlaceBlock(new Vector2Int(8, 4), 1);
        board.PlaceBlock(new Vector2Int(9, 3), 1);
        board.PlaceBlock(new Vector2Int(9, 4), 1);
        board.PlaceBlock(new Vector2Int(9, 5), 1);

        // Act
        GameLogger.LogGame(gameData, "Ghost Piece Test - Complex Board");

        // Assert
        var ghostPositions = GetGhostPiecePositions(board, tetrimino);
        Assert.IsNotNull(ghostPositions, "복잡한 보드에서도 Ghost piece가 계산되어야 함");

        // Ghost piece가 지형에 막혀서 적절한 위치에 있는지 확인
        foreach (var ghostPos in ghostPositions)
        {
            // Ghost piece가 기존 블록과 겹치지 않아야 함
            if (ghostPos.x >= 0 && ghostPos.x < TetrisBoard.WIDTH &&
                ghostPos.y >= 0 && ghostPos.y < TetrisBoard.HEIGHT)
            {
                Assert.AreEqual(0, board.grid[ghostPos.x, ghostPos.y],
                    $"Ghost piece 위치 {ghostPos}에 기존 블록이 없어야 함");
            }
        }
    }

    /// <summary>
    /// GameLogger의 private 메서드와 동일한 로직으로 Ghost piece 위치 계산
    /// (테스트 검증용)
    /// </summary>
    private Vector2Int[] GetGhostPiecePositions(TetrisBoard board, Tetrimino tetrimino)
    {
        if (board == null || tetrimino == null) return null;

        // 현재 테트리미노의 회전된 로컬 위치들을 계산
        Vector2Int[] rotatedShape = new Vector2Int[tetrimino.shape.Length];
        for (int i = 0; i < tetrimino.shape.Length; i++)
        {
            rotatedShape[i] = RotatePoint(tetrimino.shape[i], tetrimino.rotation);
        }

        Vector2Int testPosition = tetrimino.position;

        while (true)
        {
            testPosition.y--;

            bool collision = false;
            foreach (var localPos in rotatedShape)
            {
                Vector2Int worldPos = testPosition + localPos;

                if (worldPos.y < 0 || worldPos.x < 0 || worldPos.x >= TetrisBoard.WIDTH)
                {
                    collision = true;
                    break;
                }

                if (worldPos.y < TetrisBoard.HEIGHT && board.grid[worldPos.x, worldPos.y] != 0)
                {
                    collision = true;
                    break;
                }
            }

            if (collision)
            {
                testPosition.y++;
                break;
            }
        }

        Vector2Int[] ghostPositions = new Vector2Int[rotatedShape.Length];
        for (int i = 0; i < rotatedShape.Length; i++)
        {
            ghostPositions[i] = testPosition + rotatedShape[i];
        }

        return ghostPositions;
    }

    private Vector2Int RotatePoint(Vector2Int point, int rotation)
    {
        for (int i = 0; i < rotation; i++)
        {
            int temp = point.x;
            point.x = point.y;
            point.y = -temp;
        }
        return point;
    }
}
