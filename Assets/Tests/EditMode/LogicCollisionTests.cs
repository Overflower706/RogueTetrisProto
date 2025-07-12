using System.Collections;
using Minomino;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LogicCollisionTests
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
    public void DropTetrimino_ShouldStopWhenHittingFixedBlocks()
    {
        // Arrange - 바닥에 장애물 블록 배치
        var gameData = logicManager.GetGameData();

        // 바닥에 일부 블록들을 미리 배치 (y=0, y=1에 블록들 배치)
        gameData.Board.PlaceBlock(new Vector2Int(4, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(5, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(6, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(7, 0), 1);        // I 블록을 높은 위치에서 시작
        var testTetrimino = new Tetrimino(TetriminoType.I, 1); // 빨간색
        testTetrimino.position = new Vector2Int(5, 15);
        gameData.CurrentTetrimino = testTetrimino;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Drop with Fixed Blocks");

        // Act - Drop 실행
        logicManager.DropTetrimino();

        // Assert - 결과 검증
        var updatedGameData = logicManager.GetGameData();

        // Drop 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Drop with Fixed Blocks");

        // 새로운 테트리미노가 스폰되었어야 함 (이전 테트리미노가 고정됨)
        Assert.IsNotNull(updatedGameData.CurrentTetrimino, "새로운 테트리미노가 스폰되어야 함");        // 보드에 블록들이 고정되었는지 확인 (y=1에 I블록이 배치되었어야 함)
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(4, 1), "y=1에 I블록이 고정되어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(5, 1), "y=1에 I블록이 고정되어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(6, 1), "y=1에 I블록이 고정되어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(7, 1), "y=1에 I블록이 고정되어야 함");
    }

    [Test]
    public void MoveTetrimino_ShouldNotMoveIntoFixedBlocks()
    {
        // Arrange - 오른쪽에 고정 블록 배치
        var gameData = logicManager.GetGameData();

        // 오른쪽에 블록 배치 (x=7에 블록)
        gameData.Board.PlaceBlock(new Vector2Int(7, 10), 1);        // I 블록을 블록 근처에 배치
        var testTetrimino = new Tetrimino(TetriminoType.I, 2); // 초록색
        testTetrimino.position = new Vector2Int(5, 10); // I블록: 4,5,6,7 위치
        gameData.CurrentTetrimino = testTetrimino;

        var initialPosition = testTetrimino.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Move into Fixed Block");

        // Act - 오른쪽으로 이동 시도 (블록과 충돌)
        logicManager.MoveTetrimino(Vector2Int.right);

        // Assert - 이동하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Move into Fixed Block");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialPosition, currentTetrimino.position,
            "고정 블록과 충돌하므로 이동하지 않아야 함");
    }

    [Test]
    public void RotateTetrimino_ShouldNotRotateIntoFixedBlocks()
    {
        // Arrange - 회전할 공간에 고정 블록 배치
        var gameData = logicManager.GetGameData();

        // I블록이 세로로 회전할 때 필요한 공간에 블록 배치
        gameData.Board.PlaceBlock(new Vector2Int(5, 11), 1); // 위쪽 블록        // I 블록을 가로 상태로 배치
        var testTetrimino = new Tetrimino(TetriminoType.I, 3); // 파란색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0; // 가로 상태
        gameData.CurrentTetrimino = testTetrimino;

        var initialRotation = testTetrimino.rotation;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Rotate into Fixed Block");

        // Act - 회전 시도 (고정 블록과 충돌)
        logicManager.RotateTetrimino();

        // Assert - 회전하지 않아야 함
        var updatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Rotate into Fixed Block");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(initialRotation, currentTetrimino.rotation,
            "고정 블록과 충돌하므로 회전하지 않아야 함");
    }

    [Test]
    public void DropTetrimino_AllTypes_ShouldDropToCorrectPosition()
    {
        var gameData = logicManager.GetGameData();        // I 블록 Drop 테스트 - 빈 보드에서 바닥까지
        var iBlock = new Tetrimino(TetriminoType.I, 4); // 노란색
        iBlock.position = new Vector2Int(5, 15);
        gameData.CurrentTetrimino = iBlock;

        GameLogger.LogGame(gameData, "Before I-Block Drop");
        logicManager.DropTetrimino();
        var afterIDropData = logicManager.GetGameData();
        GameLogger.LogGame(afterIDropData, "After I-Block Drop");        // I블록이 바닥(y=0)에 고정되었는지 확인
        Assert.AreEqual(4, afterIDropData.Board.GetBlock(4, 0), "I블록 좌측이 바닥에 고정되어야 함");
        Assert.AreEqual(4, afterIDropData.Board.GetBlock(5, 0), "I블록 중앙이 바닥에 고정되어야 함");
        Assert.AreEqual(4, afterIDropData.Board.GetBlock(6, 0), "I블록 우측이 바닥에 고정되어야 함");
        Assert.AreEqual(4, afterIDropData.Board.GetBlock(7, 0), "I블록 우측끝이 바닥에 고정되어야 함");// O 블록 Drop 테스트 - I블록 위에 쌓이기
        var oBlock = new Tetrimino(TetriminoType.O, 1); // 빨간색
        oBlock.position = new Vector2Int(5, 15);
        gameData = afterIDropData; // 이전 상태 이어받기
        gameData.CurrentTetrimino = oBlock;

        GameLogger.LogGame(gameData, "Before O-Block Drop");
        logicManager.DropTetrimino();
        var afterODropData = logicManager.GetGameData();
        GameLogger.LogGame(afterODropData, "After O-Block Drop");        // O블록이 I블록 위(y=1)에 고정되었는지 확인
        Assert.AreEqual(1, afterODropData.Board.GetBlock(5, 1), "O블록 좌하단이 I블록 위에 고정되어야 함");
        Assert.AreEqual(1, afterODropData.Board.GetBlock(6, 1), "O블록 우하단이 I블록 위에 고정되어야 함");
        Assert.AreEqual(1, afterODropData.Board.GetBlock(5, 2), "O블록 좌상단이 고정되어야 함");
        Assert.AreEqual(1, afterODropData.Board.GetBlock(6, 2), "O블록 우상단이 고정되어야 함");
    }

    [Test]
    public void DropTetrimino_TBlock_ShouldDropAndStackCorrectly()
    {
        // Arrange - 바닥에 일부 블록 배치하여 T블록이 끼워맞춰지도록
        var gameData = logicManager.GetGameData();

        // 바닥에 ㅁㅁ_ㅁㅁ 형태로 블록 배치 (T블록이 들어갈 공간 만들기)
        gameData.Board.PlaceBlock(new Vector2Int(3, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(4, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(6, 0), 1);
        gameData.Board.PlaceBlock(new Vector2Int(7, 0), 1);        // T 블록 배치
        var tBlock = new Tetrimino(TetriminoType.T, 2); // 초록색
        tBlock.position = new Vector2Int(5, 15);
        tBlock.rotation = 0; // ㅗ 모양
        gameData.CurrentTetrimino = tBlock;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before T-Block Drop into Gap");

        // Act
        logicManager.DropTetrimino();

        // Assert
        var updatedGameData = logicManager.GetGameData();        // Drop 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After T-Block Drop into Gap");
        // T블록이 기존 블록들과 충돌하여 y=1에 착지: (4,1), (5,1), (6,1), (5,2)
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(4, 0), "기존 빨강(1) 블록이 (4,0)에 유지되어야 함");
        Assert.AreEqual(1, updatedGameData.Board.GetBlock(6, 0), "기존 빨강(1) 블록이 (6,0)에 유지되어야 함");
        Assert.AreEqual(2, updatedGameData.Board.GetBlock(4, 1), "T블록 좌측이 (4,1)에 고정되어야 함");
        Assert.AreEqual(2, updatedGameData.Board.GetBlock(5, 1), "T블록 중앙이 (5,1)에 고정되어야 함");
        Assert.AreEqual(2, updatedGameData.Board.GetBlock(6, 1), "T블록 우측이 (6,1)에 고정되어야 함");
        Assert.AreEqual(2, updatedGameData.Board.GetBlock(5, 2), "T블록 상단이 (5,2)에 고정되어야 함");
    }

    [Test]
    public void DropTetrimino_RotatedIBlock_ShouldDropVertically()
    {
        // Arrange - 세로로 회전된 I블록 테스트
        var gameData = logicManager.GetGameData(); var testTetrimino = new Tetrimino(TetriminoType.I, 3); // 파란색
        testTetrimino.position = new Vector2Int(5, 15);
        testTetrimino.rotation = 1; // 세로 방향
        gameData.CurrentTetrimino = testTetrimino;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Vertical I-Block Drop");

        // Act
        logicManager.DropTetrimino();

        // Assert
        var updatedGameData = logicManager.GetGameData();

        // Drop 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Vertical I-Block Drop");        // 세로 I블록이 x=5 열의 y=0,1,2,3에 고정되었는지 확인
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 0), "세로 I블록 하단이 바닥에 고정되어야 함");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 1), "세로 I블록이 y=1에 고정되어야 함");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 2), "세로 I블록이 y=2에 고정되어야 함");
        Assert.AreEqual(3, updatedGameData.Board.GetBlock(5, 3), "세로 I블록이 y=3에 고정되어야 함");
    }

    [Test]
    public void MoveTetrimino_ComplexShape_ShouldNavigateAroundFixedBlocks()
    {
        // Arrange - 복잡한 블록 배치로 L자 블록이 이동할 수 있는 경로 테스트
        var gameData = logicManager.GetGameData();

        // 장애물 배치: ㅁ_ㅁ 형태
        gameData.Board.PlaceBlock(new Vector2Int(3, 10), 1);
        gameData.Board.PlaceBlock(new Vector2Int(5, 10), 1);        // L 블록을 중간에 배치
        var lBlock = new Tetrimino(TetriminoType.L, 4); // 노란색
        lBlock.position = new Vector2Int(4, 11);
        gameData.CurrentTetrimino = lBlock;

        var initialPosition = lBlock.position;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before L-Block Navigation");

        // Act - 왼쪽으로 이동 (장애물 회피 가능)
        logicManager.MoveTetrimino(Vector2Int.left);

        // Assert
        var updatedGameData = logicManager.GetGameData();

        // 이동 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After L-Block Navigation");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.AreEqual(new Vector2Int(3, 11), currentTetrimino.position,
            "L블록이 장애물을 피해 이동할 수 있어야 함");
    }
}
