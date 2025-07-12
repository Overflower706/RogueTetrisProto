using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text;
using Minomino;

public class LogicTests
{
    private GameObject logicManagerObject;
    private LogicManager logicManager;
    private Game gameData;

    [SetUp]
    public void SetUp()
    {
        // LogicManager 생성 및 초기화
        logicManagerObject = new GameObject("TestLogicManager");
        logicManager = logicManagerObject.AddComponent<LogicManager>();

        // Initialize 메서드 호출하여 초기화
        logicManager.Initialize();

        // GetGameData 메서드를 통해 게임 데이터 가져오기
        gameData = logicManager.GetGameData();
    }

    [TearDown]
    public void TearDown()
    {
        if (logicManagerObject != null)
        {
            Object.DestroyImmediate(logicManagerObject);
        }
    }

    [Test]
    public void LogicManager_Initialization_ShouldCreateGameData()
    {
        // Arrange & Act
        // SetUp에서 이미 초기화됨

        // Assert
        Assert.IsNotNull(logicManager, "LogicManager component should be created");
        Assert.IsNotNull(gameData, "Game data should be initialized");
    }

    [Test]
    public void RestartGame_WhenCalled_ShouldResetGameState()
    {
        // Arrange
        var initialGameData = logicManager.GetGameData();

        // Modify game state before restart
        initialGameData.CurrentScore = 500;

        // Act
        logicManager.RestartGame();        // Assert
        var updatedGameData = logicManager.GetGameData();
        Assert.IsNotNull(updatedGameData, "Game data should be available after restart");
        Assert.AreEqual(0, updatedGameData.CurrentScore, "Score should be reset to 0 after restart");
    }

    [Test]
    public void LineClear_SingleLine_ShouldClearLine()
    {
        // Arrange - 한 줄을 거의 채우고 마지막 블록으로 완성
        var gameData = logicManager.GetGameData();

        // 수동으로 9칸 채우기 (마지막 1칸 남김)
        for (int x = 0; x < 9; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 0), 1); // I블록으로 채움
        }        // I블록을 마지막 위치에 배치해서 라인 완성
        var iBlock = new Tetrimino(TetriminoType.I, 1); // 빨간색
        iBlock.position = new Vector2Int(9, 15); // 오른쪽 끝에서 시작
        iBlock.rotation = 1; // 세로로 회전
        gameData.CurrentTetrimino = iBlock;

        GameLogger.LogGame(gameData, "Before Line Clear - 9칸 채워진 상태");

        // Act - Drop으로 라인 완성
        logicManager.DropTetrimino();        // Assert
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After Line Clear - 라인이 클리어되었는지 확인");

        // y=0 라인이 클리어되고, 위에 있던 I블록의 나머지 부분이 아래로 내려왔는지 확인
        // 원래 9칸 + 세로 I블록의 아래 3칸이 y=0에 와야 함 (위의 1칸은 사라짐)

        // 점수가 증가했는지 확인
        Assert.Greater(updatedGameData.CurrentScore, 0, "라인 클리어 후 점수가 증가해야 함");        // y=0에는 세로 I블록의 나머지 부분이 있어야 함
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(9, 0), "I블록의 나머지 부분이 (9,0)에 있어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(9, 1), "I블록의 나머지 부분이 (9,1)에 있어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(9, 2), "I블록의 나머지 부분이 (9,2)에 있어야 함");

        // 원래 y=0에 있던 블록들은 클리어되었으므로 x=0~8은 비어있어야 함
        for (int x = 0; x < 9; x++)
        {
            Assert.AreEqual(0, updatedGameData.Board.GetBlock(x, 0), $"라인 클리어 후 ({x},0)이 비어있어야 함");
        }
    }
    [Test]
    public void LineClear_FourLines_WithVerticalIBlocks_ShouldClearAllFour()
    {
        // Arrange - 4줄을 모두 채우고 마지막에 세로 I블록으로 완성
        var gameData = logicManager.GetGameData();
        var initialScore = gameData.CurrentScore;

        // y=0~3 줄을 9칸씩 채움 (x=9만 비워둠)
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                gameData.Board.PlaceBlock(new Vector2Int(x, y), 1);
            }
        }

        GameLogger.LogGame(gameData, "4줄 클리어 전 - 9칸씩 채워진 상태");        // 세로 I블록을 x=9에 배치해서 4줄 동시 완성
        var iBlock = new Tetrimino(TetriminoType.I, 2); // 초록색
        iBlock.position = new Vector2Int(9, 15);
        iBlock.rotation = 1; // 세로로 회전
        gameData.CurrentTetrimino = iBlock;

        // Act
        logicManager.DropTetrimino();

        // Assert
        var finalGameData = logicManager.GetGameData();
        GameLogger.LogGame(finalGameData, "4줄 클리어 후 - 모든 줄이 클리어되었는지 확인");

        // 4줄이 모두 클리어되어서 y=0~3이 비어있어야 함
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < TetrisBoard.WIDTH; x++)
            {
                Assert.AreEqual(0, finalGameData.Board.GetBlock(x, y),
                    $"4줄 클리어 후 위치 ({x},{y})가 비어있어야 함");
            }
        }

        // 점수가 대폭 증가했는지 확인 (4줄 동시 클리어 보너스)
        Assert.Greater(finalGameData.CurrentScore, initialScore, "4줄 클리어 후 점수가 크게 증가해야 함");

        // 4줄 클리어는 특별히 높은 점수를 줘야 함 (최소 400점 이상)
        Assert.Greater(finalGameData.CurrentScore, 400, "4줄 동시 클리어는 높은 점수를 줘야 함");
    }

    [Test]
    public void LineClear_MultipleSeparateLines_ShouldClearOnlyFullLines()
    {
        // Arrange - 여러 줄 중 일부만 채워서 선택적 클리어 테스트
        var gameData = logicManager.GetGameData();

        // y=0 줄을 완전히 채움
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 0), 1);
        }

        // y=1 줄을 부분적으로 채움 (클리어되지 않아야 함)
        for (int x = 0; x < 8; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 1), 2);
        }

        // y=2 줄을 완전히 채움
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 2), 3);
        }

        GameLogger.LogGame(gameData, "라인 클리어 전 - y=0,2는 완전, y=1은 부분적");        // Act - 새 블록 배치 (라인 클리어 트리거)
        var triggerBlock = new Tetrimino(TetriminoType.O, 3); // 파란색
        triggerBlock.position = new Vector2Int(4, 15);
        gameData.CurrentTetrimino = triggerBlock;

        logicManager.DropTetrimino();

        // Assert
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "라인 클리어 후 - 완전한 줄만 클리어됨");

        // y=0이 클리어되어서 y=1의 부분적 블록들이 y=0로 내려와야 함
        int nonEmptyBlocksAtY0 = 0;
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            if (updatedGameData.Board.GetBlock(x, 0) != 0)
                nonEmptyBlocksAtY0++;
        }
        Assert.AreEqual(8, nonEmptyBlocksAtY0, "y=1에 있던 8개 블록이 y=0로 내려와야 함");        // O블록은 라인 클리어 전에 먼저 고정됨
        // O블록 color는 3 (파란색)
        bool foundOBlock = false;
        for (int y = 0; y < TetrisBoard.HEIGHT; y++)
        {
            for (int x = 0; x < TetrisBoard.WIDTH - 1; x++)
            {
                if (updatedGameData.Board.GetBlock(x, y) == 3 &&
                    updatedGameData.Board.GetBlock(x + 1, y) == 3 &&
                    updatedGameData.Board.GetBlock(x, y + 1) == 3 &&
                    updatedGameData.Board.GetBlock(x + 1, y + 1) == 3)
                {
                    foundOBlock = true;
                    break;
                }
            }
            if (foundOBlock) break;
        }
        Assert.IsTrue(foundOBlock, "O블록(color 3)이 보드에 고정되어야 함");
    }

    [Test]
    public void LineClear_NoFullLines_ShouldNotClearAnything()
    {
        // Arrange - 완전히 채워진 줄이 없는 상태
        var gameData = logicManager.GetGameData();
        var initialScore = gameData.CurrentScore;

        // 부분적으로만 블록 배치
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 7; x++) // 10칸 중 7칸만 채움
            {
                gameData.Board.PlaceBlock(new Vector2Int(x, y), 1);
            }
        }

        GameLogger.LogGame(gameData, "라인 클리어 전 - 완전한 줄 없음");        // Act
        var testBlock = new Tetrimino(TetriminoType.I, 4); // 노란색
        testBlock.position = new Vector2Int(8, 15);
        gameData.CurrentTetrimino = testBlock;

        logicManager.DropTetrimino();

        // Assert
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "라인 클리어 후 - 변화 없어야 함");

        // 점수 변화 없어야 함 (라인 클리어 없으므로)
        Assert.AreEqual(initialScore, updatedGameData.CurrentScore, "라인 클리어가 없으면 점수 변화 없어야 함");

        // 기존 블록들이 그대로 있어야 함
        for (int y = 0; y < 3; y++)
        {
            int blockCount = 0;
            for (int x = 0; x < TetrisBoard.WIDTH; x++)
            {
                if (updatedGameData.Board.GetBlock(x, y) != 0)
                    blockCount++;
            }
            // 원래 7개 + 새로 추가된 I블록의 일부
            Assert.GreaterOrEqual(blockCount, 7, $"y={y} 줄에 최소 7개 블록이 있어야 함");
        }
    }
    [Test]
    public void LineClear_TenVerticalIBlocks_ShouldClearFourLines()
    {
        // Arrange - I블록을 세로로 10개 연속 배치하여 4줄 동시 클리어 달성
        var gameData = logicManager.GetGameData();
        var initialScore = gameData.CurrentScore;

        GameLogger.LogGame(gameData, "세로 I블록 10개 연속 배치 시작");        // 세로 I블록 10개를 x=0~9 위치에 배치
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            var iBlock = new Tetrimino(TetriminoType.I, 1); // 빨간색
            iBlock.position = new Vector2Int(x, 15);
            iBlock.rotation = 1; // 세로로 회전
            gameData.CurrentTetrimino = iBlock;

            if (x == 8) // 9번째(x=8) 배치 후 상태 확인
            {
                GameLogger.LogGame(gameData, "9번째 I블록 배치 전 - 아직 4줄 미완성");
            }

            logicManager.DropTetrimino();
            gameData = logicManager.GetGameData(); // 업데이트된 게임 데이터 가져오기

            if (x == 8) // 9번째 배치 후
            {
                GameLogger.LogGame(gameData, "9번째 I블록 배치 완료 - 4줄이 9칸씩 채워짐");
            }
            else if (x == 9) // 10번째(마지막) 배치 후
            {
                GameLogger.LogGame(gameData, "10번째 I블록 배치 완료 - 4줄 동시 클리어 발생!");
            }
        }

        // Assert
        var finalGameData = logicManager.GetGameData();
        GameLogger.LogGame(finalGameData, "세로 I블록 10개 배치 완료");        // 10개의 세로 I블록을 배치하면 바닥 4줄이 모두 채워져서 4줄 동시 클리어가 발생해야 함
        // 각 I블록이 4칸씩 차지하므로 10개 배치하면 10열 x 4줄 = 바닥 4줄을 완전히 채움
        // 4줄이 모두 클리어되므로 최종적으로 보드는 완전히 비어있어야 함

        // 점수가 크게 증가했는지 확인 (4줄 동시 클리어)
        Assert.Greater(finalGameData.CurrentScore, initialScore, "4줄 동시 클리어로 점수가 증가해야 함");

        // 4줄 동시 클리어는 800점을 주므로 이를 확인
        Assert.GreaterOrEqual(finalGameData.CurrentScore, 800, "4줄 동시 클리어로 최소 800점은 나와야 함");

        // 실제로는 정확히 800점이어야 함 (4줄 클리어 = 100x4x2 = 800)
        Assert.AreEqual(800, finalGameData.CurrentScore, "4줄 동시 클리어로 정확히 800점이 나와야 함");        // 4줄 클리어 후 보드 상태 확인
        // 10개의 세로 I블록(각 4칸)을 10개 열에 배치하면 바닥 4줄이 완전히 채워짐
        // 4줄이 모두 클리어되어야 하므로 보드는 완전히 비어있어야 함
        int filledLinesAtBottom = 0;
        for (int y = 0; y < 4; y++)
        {
            bool isFull = true;
            for (int x = 0; x < TetrisBoard.WIDTH; x++)
            {
                if (finalGameData.Board.GetBlock(x, y) == 0)
                {
                    isFull = false;
                    break;
                }
            }
            if (isFull)
                filledLinesAtBottom++;
        }

        Assert.AreEqual(0, filledLinesAtBottom, "4줄 클리어 후 모든 줄이 비어있어야 함");

        // y=0~3이 모두 비어있어야 함 (4줄 클리어 후)
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < TetrisBoard.WIDTH; x++)
            {
                Assert.AreEqual(0, finalGameData.Board.GetBlock(x, y), $"4줄 클리어 후 ({x},{y})가 비어있어야 함");
            }
        }
    }
    [Test]
    public void LineClear_VerticalIBlock_ShouldMoveUpperBlocksDown()
    {
        // Arrange - 9-33-9 패턴으로 복잡한 스택 생성 (바닥 9개, 중간 33, 위 9개)
        var gameData = logicManager.GetGameData();

        // y=0 줄에 9개 블록 배치 (x=9만 비워둠)
        for (int x = 0; x < 9; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 0), 1);
        }

        // y=1 줄에 3개 블록 배치 (중간 33 패턴)
        gameData.Board.PlaceBlock(new Vector2Int(3, 1), 3);
        gameData.Board.PlaceBlock(new Vector2Int(4, 1), 3);
        gameData.Board.PlaceBlock(new Vector2Int(5, 1), 3);

        // y=2 줄에 3개 블록 배치 (중간 33 패턴)
        gameData.Board.PlaceBlock(new Vector2Int(3, 2), 3);
        gameData.Board.PlaceBlock(new Vector2Int(4, 2), 3);
        gameData.Board.PlaceBlock(new Vector2Int(5, 2), 3);

        // y=3 줄에 9개 블록 배치 (x=9만 비워둠) - 위쪽 9개
        for (int x = 0; x < 9; x++)
        {
            gameData.Board.PlaceBlock(new Vector2Int(x, 3), 2);
        }

        GameLogger.LogGame(gameData, "라인 클리어 전 - 9-33-9 패턴");        // 세로 I블록을 마지막 칸(x=9)에 배치해서 y=0과 y=3 라인 동시 완성
        var iBlock = new Tetrimino(TetriminoType.I, 2); // 초록색
        iBlock.position = new Vector2Int(9, 15);
        iBlock.rotation = 1; // 세로로 회전
        gameData.CurrentTetrimino = iBlock;

        // Act
        logicManager.DropTetrimino();

        // Assert
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "라인 클리어 후 - y=0,3 클리어되고 y=1,2의 33이 내려왔는지 확인");

        // y=0과 y=3 라인이 클리어되었으므로, 원래 y=1,2에 있던 33 블록들이 y=0,1로 내려와야 함
        // 원래 y=1의 3개 블록이 y=0으로 이동
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(3, 0), "y=1에 있던 33 블록이 y=0으로 내려옴 (3,0)");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(4, 0), "y=1에 있던 33 블록이 y=0으로 내려옴 (4,0)");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 0), "y=1에 있던 33 블록이 y=0으로 내려옴 (5,0)");

        // 원래 y=2의 3개 블록이 y=1로 이동
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(3, 1), "y=2에 있던 33 블록이 y=1로 내려옴 (3,1)");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(4, 1), "y=2에 있던 33 블록이 y=1로 내려옴 (4,1)");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 1), "y=2에 있던 33 블록이 y=1로 내려옴 (5,1)");

        // 클리어된 줄 위의 빈 공간들은 비어있어야 함 (33 블록이 있는 부분 제외)
        for (int x = 0; x < 9; x++)
        {
            if (x < 3 || x > 5) // 3,4,5는 33 블록이 내려와서 채워짐
            {
                Assert.AreEqual(0, updatedGameData.Board.GetBlock(x, 0), $"라인 클리어 후 ({x},0)이 비어있어야 함");
                Assert.AreEqual(0, updatedGameData.Board.GetBlock(x, 1), $"라인 클리어 후 ({x},1)이 비어있어야 함");
            }
        }

        // y=2, y=3은 완전히 비어있어야 함 (2줄 클리어 후)
        for (int x = 0; x < TetrisBoard.WIDTH; x++)
        {
            Assert.AreEqual(0, updatedGameData.Board.GetBlock(x, 2), $"2줄 클리어 후 ({x},2)가 비어있어야 함");
            Assert.AreEqual(0, updatedGameData.Board.GetBlock(x, 3), $"2줄 클리어 후 ({x},3)가 비어있어야 함");
        }

        // 점수가 증가했는지 확인 (2줄 동시 클리어)
        Assert.Greater(updatedGameData.CurrentScore, 0, "2줄 동시 클리어로 점수가 증가해야 함");

        // 2줄 클리어는 300점 이상이어야 함
        Assert.GreaterOrEqual(updatedGameData.CurrentScore, 300, "2줄 동시 클리어는 최소 300점 이상이어야 함");
    }
}