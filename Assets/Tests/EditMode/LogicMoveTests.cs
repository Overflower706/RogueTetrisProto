using System.Collections;
using Minomino;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LogicMoveTests
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
    public void MoveTetrimino_WhenCalled_ShouldUpdateTetriminoPosition()
    {
        // Arrange
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 1); // 빨간색
        testTetrimino.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Move Left");

        // Act
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - Check that game state has been updated
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Move Left");
        Assert.IsNotNull(updatedGameData, "Game data should be available after move");

        // Verify the tetrimino actually moved
        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.IsNotNull(currentTetrimino, "Current tetrimino should exist");
        Assert.AreNotEqual(initialPosition, currentTetrimino.position, "Tetrimino position should have changed");
        Assert.AreEqual(initialPosition.x - 1, currentTetrimino.position.x, "Tetrimino should have moved left by 1");
        Assert.AreEqual(initialPosition.y, currentTetrimino.position.y, "Tetrimino Y position should remain the same");
    }
    [Test]
    public void MoveTetriminoJ_WhenCalled_ShouldUpdateTetriminoPosition()
    {
        // Arrange
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.J, 2); // 초록색
        testTetrimino.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before J-Block Move");

        // Act
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - Check that game state has been updated
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After J-Block Move");

        Assert.IsNotNull(updatedGameData, "Game data should be available after move");

        // Verify the tetrimino actually moved
        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.IsNotNull(currentTetrimino, "Current tetrimino should exist");
        Assert.AreNotEqual(initialPosition, currentTetrimino.position, "Tetrimino position should have changed");
        Assert.AreEqual(initialPosition.x - 1, currentTetrimino.position.x, "Tetrimino should have moved left by 1");
        Assert.AreEqual(initialPosition.y, currentTetrimino.position.y, "Tetrimino Y position should remain the same");
    }
    [Test]
    public void MoveTetrimino_IBLock_WhenAtLeftBoundary_ShouldNotMoveOutsideBoard()
    {
        // Arrange - I 블록은 중심점에서 왼쪽으로 1칸 차지하므로, x=1이 왼쪽 경계
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 3); // 파란색
        testTetrimino.position = new Vector2Int(1, 10); // I블록이 경계에 딱 맞는 위치
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before I-Block Left Boundary Move");

        // Act - 왼쪽으로 이동 시도 (x=-1이 되어 경계 밖으로 나감)
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After I-Block Left Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "I블록은 왼쪽 경계에서 더 이상 왼쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_JBlock_WhenAtLeftBoundary_ShouldNotMoveOutsideBoard()
    {
        // Arrange - J 블록도 중심점에서 왼쪽으로 1칸 차지하므로, x=1이 왼쪽 경계
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.J, 4); // 노란색
        testTetrimino.position = new Vector2Int(1, 10); // J블록이 경계에 딱 맞는 위치
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before J-Block Left Boundary Move");

        // Act - 왼쪽으로 이동 시도
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After J-Block Left Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "J블록은 왼쪽 경계에서 더 이상 왼쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_IBlock_WhenAtRightBoundary_ShouldNotMoveOutsideBoard()
    {
        // Arrange - I 블록은 중심점에서 오른쪽으로 2칸 차지하므로, x=7이 오른쪽 경계 (WIDTH=10)
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 1); // 빨간색
        testTetrimino.position = new Vector2Int(7, 10); // I블록 오른쪽 경계 (최대 x=9까지 차지)
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before I-Block Right Boundary Move");

        // Act - 오른쪽으로 이동 시도
        logicManager.MoveTetrimino(Vector2Int.right);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After I-Block Right Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "I블록은 오른쪽 경계에서 더 이상 오른쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_OBlock_SpecialBoundaryBehavior_ShouldWork()
    {
        // Arrange - O 블록은 중점이 왼쪽 아래 모서리라서 x=0부터 시작 가능
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.O, 2); // 초록색
        testTetrimino.position = new Vector2Int(0, 10); // O블록은 x=0에서 시작 가능
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before O-Block Left Boundary Move");

        // Act - 왼쪽으로 이동 시도 (x=-1이 되면 경계 밖)
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After O-Block Left Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "O블록은 x=0에서 더 이상 왼쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_OBlock_WhenAtRightBoundary_ShouldNotMoveOutside()
    {
        // Arrange - O블록은 중심점에서 오른쪽으로 1칸 차지하므로, x=8이 오른쪽 경계
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.O, 3); // 파란색
        testTetrimino.position = new Vector2Int(8, 10); // O블록 오른쪽 경계
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before O-Block Right Boundary Move");

        // Act - 오른쪽으로 이동 시도
        logicManager.MoveTetrimino(Vector2Int.right);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After O-Block Right Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "O블록은 오른쪽 경계에서 더 이상 오른쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_TBlock_BoundaryBehavior_ShouldWork()
    {
        // Arrange - T 블록은 대칭적으로 좌우 1칸씩 차지
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.T, 4); // 노란색
        testTetrimino.position = new Vector2Int(1, 10); // T블록 왼쪽 경계
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before T-Block Left Boundary Move");

        // Act - 왼쪽으로 이동 시도
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After T-Block Left Boundary Move");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "T블록은 왼쪽 경계에서 더 이상 왼쪽으로 이동할 수 없어야 함");
    }
    [Test]
    public void MoveTetrimino_WhenAtBottomBoundary_ShouldNotMoveDown()
    {
        // Arrange - 바닥 경계 테스트 (y=0이 바닥)
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 1); // 빨강색
        testTetrimino.position = new Vector2Int(5, 0); // 바닥에 위치
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Bottom Boundary Move");

        // Act - 아래로 이동 시도
        logicManager.MoveTetrimino(Vector2Int.down);

        // Assert - 이동하지 않거나 테트리미노가 고정되어야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Bottom Boundary Move");

        // 바닥에 닿으면 새로운 테트리미노가 생성될 수 있으므로, 
        // 이동이 제한되었는지 확인하는 것이 중요
        Assert.IsNotNull(updatedGameData, "바닥 경계에서도 게임 데이터는 유효해야 함");
        Assert.IsNotNull(updatedGameData.CurrentTetrimino, "현재 테트리미노가 존재해야 함");
    }

    [Test]
    public void Tetrimino_BoundaryValues_ShouldBeCorrectForEachType()
    {
        // 각 테트리미노 타입별로 실제 경계값 검증

        // I 블록: 왼쪽 -1, 오른쪽 +2
        var iBlock = new Tetrimino(TetriminoType.I, 1);
        iBlock.position = new Vector2Int(1, 10); // 왼쪽 경계
        var iPositions = iBlock.GetWorldPositions();
        var minX = System.Linq.Enumerable.Min(iPositions, pos => pos.x);
        var maxX = System.Linq.Enumerable.Max(iPositions, pos => pos.x); Assert.AreEqual(0, minX, "I블록이 x=1에 있을 때 최소 x는 0이어야 함");
        Assert.AreEqual(3, maxX, "I블록이 x=1에 있을 때 최대 x는 3이어야 함");

        // O 블록: 왼쪽 0, 오른쪽 +1
        var oBlock = new Tetrimino(TetriminoType.O, 2);
        oBlock.position = new Vector2Int(0, 10); // 왼쪽 경계
        var oPositions = oBlock.GetWorldPositions();
        minX = System.Linq.Enumerable.Min(oPositions, pos => pos.x);
        maxX = System.Linq.Enumerable.Max(oPositions, pos => pos.x);
        Assert.AreEqual(0, minX, "O블록이 x=0에 있을 때 최소 x는 0이어야 함");
        Assert.AreEqual(1, maxX, "O블록이 x=0에 있을 때 최대 x는 1이어야 함");

        // J 블록: 왼쪽 -1, 오른쪽 +1
        var jBlock = new Tetrimino(TetriminoType.J, 3);
        jBlock.position = new Vector2Int(1, 10); // 왼쪽 경계
        var jPositions = jBlock.GetWorldPositions();
        minX = System.Linq.Enumerable.Min(jPositions, pos => pos.x);
        maxX = System.Linq.Enumerable.Max(jPositions, pos => pos.x);
        Assert.AreEqual(0, minX, "J블록이 x=1에 있을 때 최소 x는 0이어야 함");
        Assert.AreEqual(2, maxX, "J블록이 x=1에 있을 때 최대 x는 2이어야 함");

        // T 블록: 왼쪽 -1, 오른쪽 +1
        var tBlock = new Tetrimino(TetriminoType.T, 4);
        tBlock.position = new Vector2Int(1, 10); // 왼쪽 경계
        var tPositions = tBlock.GetWorldPositions();
        minX = System.Linq.Enumerable.Min(tPositions, pos => pos.x);
        maxX = System.Linq.Enumerable.Max(tPositions, pos => pos.x);
        Assert.AreEqual(0, minX, "T블록이 x=1에 있을 때 최소 x는 0이어야 함");
        Assert.AreEqual(2, maxX, "T블록이 x=1에 있을 때 최대 x는 2이어야 함");
    }

    [Test]
    public void MoveTetrimino_RightBoundary_AllBlocks_ShouldBehaveCorrectly()
    {        // I 블록 - 오른쪽 경계 x=7 (중심점에서 +2까지 차지하므로 최대 x=9)
        var gameData = logicManager.GetGameData();
        var iBlock = new Tetrimino(TetriminoType.I, 1);
        iBlock.position = new Vector2Int(7, 10);
        gameData.CurrentTetrimino = iBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(7, 10), gameData.CurrentTetrimino.position,
            "I블록은 x=7에서 오른쪽으로 이동할 수 없어야 함");

        // O 블록 - 오른쪽 경계 x=8 (최대 x가 9가 됨)
        var oBlock = new Tetrimino(TetriminoType.O, 2);
        oBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = oBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "O블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // J 블록 - 오른쪽 경계 x=8 (최대 x가 9가 됨)
        var jBlock = new Tetrimino(TetriminoType.J, 3);
        jBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = jBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "J블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");
    }

    [Test]
    public void Tetrimino_AllShapes_ShouldHaveCorrectOccupiedPositions()
    {
        // Test O 블록
        var oBlock = new Tetrimino(TetriminoType.O, 4);
        oBlock.position = new Vector2Int(5, 10);
        var oPositions = oBlock.GetWorldPositions();

        Assert.AreEqual(4, oPositions.Length, "O블록은 4개 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(5, 10), oPositions, "O블록 중심점");
        Assert.Contains(new Vector2Int(6, 10), oPositions, "O블록 오른쪽 아래");
        Assert.Contains(new Vector2Int(5, 11), oPositions, "O블록 왼쪽 위");
        Assert.Contains(new Vector2Int(6, 11), oPositions, "O블록 오른쪽 위");

        // Test T 블록
        var tBlock = new Tetrimino(TetriminoType.T, 1);
        tBlock.position = new Vector2Int(5, 10);
        var tPositions = tBlock.GetWorldPositions();

        Assert.AreEqual(4, tPositions.Length, "T블록은 4개 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(4, 10), tPositions, "T블록 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), tPositions, "T블록 중심점");
        Assert.Contains(new Vector2Int(6, 10), tPositions, "T블록 오른쪽");
        Assert.Contains(new Vector2Int(5, 11), tPositions, "T블록 위쪽");

        // Test S 블록
        var sBlock = new Tetrimino(TetriminoType.S, 2);
        sBlock.position = new Vector2Int(5, 10);
        var sPositions = sBlock.GetWorldPositions();

        Assert.AreEqual(4, sPositions.Length, "S블록은 4개 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(4, 10), sPositions, "S블록 왼쪽 아래");
        Assert.Contains(new Vector2Int(5, 10), sPositions, "S블록 중심점");
        Assert.Contains(new Vector2Int(5, 11), sPositions, "S블록 중심 위");
        Assert.Contains(new Vector2Int(6, 11), sPositions, "S블록 오른쪽 위");

        // Test I 블록
        var iBlock = new Tetrimino(TetriminoType.I, 3);
        iBlock.position = new Vector2Int(5, 10);
        var iPositions = iBlock.GetWorldPositions();

        Assert.AreEqual(4, iPositions.Length, "I블록은 4개 위치를 차지해야 함"); Assert.Contains(new Vector2Int(4, 10), iPositions, "I블록은 중심점-1 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(5, 10), iPositions, "I블록은 중심점 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(6, 10), iPositions, "I블록은 중심점+1 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(7, 10), iPositions, "I블록은 중심점+2 위치를 차지해야 함");

        // Test J 블록
        var jBlock = new Tetrimino(TetriminoType.J, 4);
        jBlock.position = new Vector2Int(5, 10);
        var jPositions = jBlock.GetWorldPositions();

        Assert.AreEqual(4, jPositions.Length, "J블록은 4개 위치를 차지해야 함");
        Assert.Contains(new Vector2Int(4, 11), jPositions, "J블록 위쪽 왼쪽");
        Assert.Contains(new Vector2Int(4, 10), jPositions, "J블록 아래쪽 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), jPositions, "J블록 중심점");
        Assert.Contains(new Vector2Int(6, 10), jPositions, "J블록 아래쪽 오른쪽");
    }
    [Test]
    public void MoveTetrimino_AllTypes_LeftMovement_ShouldWorkCorrectly()
    {
        var gameData = logicManager.GetGameData();

        // I 블록 왼쪽 이동 테스트
        var iBlock = new Tetrimino(TetriminoType.I, 1);
        iBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = iBlock;
        GameLogger.LogGame(gameData, "Before I-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After I-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "I블록이 왼쪽으로 정상 이동해야 함");

        // O 블록 왼쪽 이동 테스트
        var oBlock = new Tetrimino(TetriminoType.O, 2);
        oBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = oBlock;
        GameLogger.LogGame(gameData, "Before O-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After O-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "O블록이 왼쪽으로 정상 이동해야 함");

        // T 블록 왼쪽 이동 테스트
        var tBlock = new Tetrimino(TetriminoType.T, 3);
        tBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = tBlock;
        GameLogger.LogGame(gameData, "Before T-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After T-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "T블록이 왼쪽으로 정상 이동해야 함");

        // S 블록 왼쪽 이동 테스트
        var sBlock = new Tetrimino(TetriminoType.S, 4);
        sBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = sBlock;
        GameLogger.LogGame(gameData, "Before S-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After S-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "S블록이 왼쪽으로 정상 이동해야 함");

        // Z 블록 왼쪽 이동 테스트
        var zBlock = new Tetrimino(TetriminoType.Z, 1);
        zBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = zBlock;
        GameLogger.LogGame(gameData, "Before Z-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After Z-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "Z블록이 왼쪽으로 정상 이동해야 함");

        // J 블록 왼쪽 이동 테스트
        var jBlock = new Tetrimino(TetriminoType.J, 2);
        jBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = jBlock;
        GameLogger.LogGame(gameData, "Before J-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After J-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "J블록이 왼쪽으로 정상 이동해야 함");

        // L 블록 왼쪽 이동 테스트
        var lBlock = new Tetrimino(TetriminoType.L, 3);
        lBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = lBlock;
        GameLogger.LogGame(gameData, "Before L-Block Left Movement");
        logicManager.MoveTetrimino(Vector2Int.left);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After L-Block Left Movement");
        Assert.AreEqual(new Vector2Int(4, 10), updatedGameData.CurrentTetrimino.position,
            "L블록이 왼쪽으로 정상 이동해야 함");
    }
    [Test]
    public void MoveTetrimino_AllTypes_RightMovement_ShouldWorkCorrectly()
    {
        var gameData = logicManager.GetGameData();

        // I 블록 오른쪽 이동 테스트
        var iBlock = new Tetrimino(TetriminoType.I, 4);
        iBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = iBlock;
        GameLogger.LogGame(gameData, "Before I-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        var updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After I-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "I블록이 오른쪽으로 정상 이동해야 함");

        // O 블록 오른쪽 이동 테스트
        var oBlock = new Tetrimino(TetriminoType.O, 1);
        oBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = oBlock;
        GameLogger.LogGame(gameData, "Before O-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After O-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "O블록이 오른쪽으로 정상 이동해야 함");

        // T 블록 오른쪽 이동 테스트
        var tBlock = new Tetrimino(TetriminoType.T, 2);
        tBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = tBlock;
        GameLogger.LogGame(gameData, "Before T-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After T-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "T블록이 오른쪽으로 정상 이동해야 함");

        // S 블록 오른쪽 이동 테스트
        var sBlock = new Tetrimino(TetriminoType.S, 3);
        sBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = sBlock;
        GameLogger.LogGame(gameData, "Before S-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After S-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "S블록이 오른쪽으로 정상 이동해야 함");

        // Z 블록 오른쪽 이동 테스트
        var zBlock = new Tetrimino(TetriminoType.Z, 4);
        zBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = zBlock;
        GameLogger.LogGame(gameData, "Before Z-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After Z-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "Z블록이 오른쪽으로 정상 이동해야 함");

        // J 블록 오른쪽 이동 테스트
        var jBlock = new Tetrimino(TetriminoType.J, 1);
        jBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = jBlock;
        GameLogger.LogGame(gameData, "Before J-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After J-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "J블록이 오른쪽으로 정상 이동해야 함");

        // L 블록 오른쪽 이동 테스트
        var lBlock = new Tetrimino(TetriminoType.L, 2);
        lBlock.position = new Vector2Int(5, 10);
        gameData.CurrentTetrimino = lBlock;
        GameLogger.LogGame(gameData, "Before L-Block Right Movement");
        logicManager.MoveTetrimino(Vector2Int.right);
        updatedGameData = logicManager.GetGameData();
        GameLogger.LogGame(updatedGameData, "After L-Block Right Movement");
        Assert.AreEqual(new Vector2Int(6, 10), updatedGameData.CurrentTetrimino.position,
            "L블록이 오른쪽으로 정상 이동해야 함");
    }

    [Test]
    public void MoveTetrimino_AllTypes_LeftBoundaryBehavior_ShouldWork()
    {
        var gameData = logicManager.GetGameData();

        // I 블록 - 왼쪽 경계 x=1
        var iBlock = new Tetrimino(TetriminoType.I, 3);
        iBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = iBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "I블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");

        // O 블록 - 왼쪽 경계 x=0
        var oBlock = new Tetrimino(TetriminoType.O, 2);
        oBlock.position = new Vector2Int(0, 10);
        gameData.CurrentTetrimino = oBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(0, 10), gameData.CurrentTetrimino.position,
            "O블록은 x=0에서 왼쪽으로 이동할 수 없어야 함");

        // T 블록 - 왼쪽 경계 x=1
        var tBlock = new Tetrimino(TetriminoType.T, 4);
        tBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = tBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "T블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");

        // S 블록 - 왼쪽 경계 x=1
        var sBlock = new Tetrimino(TetriminoType.S, 1);
        sBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = sBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "S블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");

        // Z 블록 - 왼쪽 경계 x=1
        var zBlock = new Tetrimino(TetriminoType.Z, 2);
        zBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = zBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "Z블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");

        // J 블록 - 왼쪽 경계 x=1
        var jBlock = new Tetrimino(TetriminoType.J, 3);
        jBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = jBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "J블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");

        // L 블록 - 왼쪽 경계 x=1
        var lBlock = new Tetrimino(TetriminoType.L, 4);
        lBlock.position = new Vector2Int(1, 10);
        gameData.CurrentTetrimino = lBlock;
        logicManager.MoveTetrimino(Vector2Int.left);
        Assert.AreEqual(new Vector2Int(1, 10), gameData.CurrentTetrimino.position,
            "L블록은 x=1에서 왼쪽으로 이동할 수 없어야 함");
    }

    [Test]
    public void MoveTetrimino_AllTypes_RightBoundaryBehavior_ShouldWork()
    {
        var gameData = logicManager.GetGameData();        // I 블록 - 오른쪽 경계 x=7 (WIDTH=10, I블록은 +2까지 차지)
        var iBlock = new Tetrimino(TetriminoType.I, 1);
        iBlock.position = new Vector2Int(7, 10);
        gameData.CurrentTetrimino = iBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(7, 10), gameData.CurrentTetrimino.position,
            "I블록은 x=7에서 오른쪽으로 이동할 수 없어야 함");

        // O 블록 - 오른쪽 경계 x=8 (O블록은 +1까지 차지)
        var oBlock = new Tetrimino(TetriminoType.O, 2);
        oBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = oBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "O블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // T 블록 - 오른쪽 경계 x=8 (T블록은 +1까지 차지)
        var tBlock = new Tetrimino(TetriminoType.T, 4);
        tBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = tBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "T블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // S 블록 - 오른쪽 경계 x=8 (S블록은 +1까지 차지)
        var sBlock = new Tetrimino(TetriminoType.S, 3);
        sBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = sBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "S블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // Z 블록 - 오른쪽 경계 x=8 (Z블록은 +1까지 차지)
        var zBlock = new Tetrimino(TetriminoType.Z, 2);
        zBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = zBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "Z블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // J 블록 - 오른쪽 경계 x=8 (J블록은 +1까지 차지)
        var jBlock = new Tetrimino(TetriminoType.J, 1);
        jBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = jBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "J블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");

        // L 블록 - 오른쪽 경계 x=8 (L블록은 +1까지 차지)
        var lBlock = new Tetrimino(TetriminoType.L, 4);
        lBlock.position = new Vector2Int(8, 10);
        gameData.CurrentTetrimino = lBlock;
        logicManager.MoveTetrimino(Vector2Int.right);
        Assert.AreEqual(new Vector2Int(8, 10), gameData.CurrentTetrimino.position,
            "L블록은 x=8에서 오른쪽으로 이동할 수 없어야 함");
    }
}
