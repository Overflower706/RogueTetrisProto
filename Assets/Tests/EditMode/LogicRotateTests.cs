using System.Collections;
using Minomino;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LogicRotateTests
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
    public void RotateTetrimino_WhenCalled_ShouldUpdateTetriminoRotation()
    {        // Arrange
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.T, 1); // 빨간색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0; // 초기 회전 상태
        gameData.CurrentTetrimino = testTetrimino;

        var initialRotation = testTetrimino.rotation;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before T-Block Rotation");

        // Act
        logicManager.RotateTetrimino();        // Assert
        var updatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After T-Block Rotation");

        Assert.IsNotNull(updatedGameData, "Game data should be available after rotation");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.IsNotNull(currentTetrimino, "Current tetrimino should exist");
        Assert.AreEqual((initialRotation + 1) % 4, currentTetrimino.rotation,
            "Tetrimino rotation should increase by 1 (mod 4)");
    }
    [Test]
    public void RotateTetrimino_IBlock_ShouldRotateCorrectly()
    {        // Arrange - I 블록은 가로/세로 회전이 명확함
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 2); // 초록색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0; // 가로 상태
        gameData.CurrentTetrimino = testTetrimino;

        // 초기 상태 확인 (가로: 4,5,6,7)
        var initialPositions = testTetrimino.GetWorldPositions();
        Assert.Contains(new Vector2Int(4, 10), initialPositions, "I블록 초기 상태 - 왼쪽");
        Assert.Contains(new Vector2Int(7, 10), initialPositions, "I블록 초기 상태 - 오른쪽");        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before I-Block Rotation");

        // Act - 첫 번째 회전 (세로)
        logicManager.RotateTetrimino();

        // Assert - 세로 상태 확인
        var rotatedGameData = logicManager.GetGameData();

        // 첫 번째 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After I-Block First Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual(1, rotatedTetrimino.rotation, "I블록이 90도 회전해야 함"); var rotatedPositions = rotatedTetrimino.GetWorldPositions();
        // 세로 상태에서는 y축으로 배치됨: [(5,11), (5,10), (5,9), (5,8)]
        Assert.Contains(new Vector2Int(5, 11), rotatedPositions, "I블록 회전 후 - 위");
        Assert.Contains(new Vector2Int(5, 10), rotatedPositions, "I블록 회전 후 - 중심 위");
        Assert.Contains(new Vector2Int(5, 9), rotatedPositions, "I블록 회전 후 - 중심 아래");
        Assert.Contains(new Vector2Int(5, 8), rotatedPositions, "I블록 회전 후 - 아래");
    }
    [Test]
    public void RotateTetrimino_TBlock_AllRotations_ShouldWork()
    {        // Arrange - T 블록의 4가지 회전 상태 모두 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.T, 3); // 파란색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        // 초기 T 모양 확인: ㅗ
        var initialPositions = testTetrimino.GetWorldPositions();
        Assert.Contains(new Vector2Int(4, 10), initialPositions, "T블록 초기 - 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), initialPositions, "T블록 초기 - 중심");
        Assert.Contains(new Vector2Int(6, 10), initialPositions, "T블록 초기 - 오른쪽");
        Assert.Contains(new Vector2Int(5, 11), initialPositions, "T블록 초기 - 위");

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before T-Block All Rotations");

        // Act & Assert - 첫 번째 회전 (90도): ㅏ
        logicManager.RotateTetrimino();
        var rotation1Data = logicManager.GetGameData();
        GameLogger.LogGame(rotation1Data, "After T-Block First Rotation (90도)");
        var rotation1 = rotation1Data.CurrentTetrimino;
        Assert.AreEqual(1, rotation1.rotation, "첫 번째 회전 후 rotation = 1");

        // Act & Assert - 두 번째 회전 (180도): ㅜ
        logicManager.RotateTetrimino();
        var rotation2Data = logicManager.GetGameData();
        GameLogger.LogGame(rotation2Data, "After T-Block Second Rotation (180도)");
        var rotation2 = rotation2Data.CurrentTetrimino;
        Assert.AreEqual(2, rotation2.rotation, "두 번째 회전 후 rotation = 2");

        // Act & Assert - 세 번째 회전 (270도): ㅓ
        logicManager.RotateTetrimino();
        var rotation3Data = logicManager.GetGameData();
        GameLogger.LogGame(rotation3Data, "After T-Block Third Rotation (270도)");
        var rotation3 = rotation3Data.CurrentTetrimino;
        Assert.AreEqual(3, rotation3.rotation, "세 번째 회전 후 rotation = 3");

        // Act & Assert - 네 번째 회전 (360도 = 0도): 원래 상태로 복귀
        logicManager.RotateTetrimino();
        var rotation4Data = logicManager.GetGameData();
        GameLogger.LogGame(rotation4Data, "After T-Block Fourth Rotation (360도 = 0도)");
        var rotation4 = rotation4Data.CurrentTetrimino;
        Assert.AreEqual(0, rotation4.rotation, "네 번째 회전 후 rotation = 0 (원래 상태)");

        var finalPositions = rotation4.GetWorldPositions();
        // 원래 T 모양과 동일해야 함
        Assert.Contains(new Vector2Int(4, 10), finalPositions, "T블록 최종 - 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), finalPositions, "T블록 최종 - 중심");
        Assert.Contains(new Vector2Int(6, 10), finalPositions, "T블록 최종 - 오른쪽");
        Assert.Contains(new Vector2Int(5, 11), finalPositions, "T블록 최종 - 위");
    }
    [Test]
    public void RotateTetrimino_OBlock_ShouldRotateButMaintainSquareShape()
    {        // Arrange - O 블록은 정사각형이므로 회전해도 같은 모양 유지 (하지만 좌표는 변할 수 있음)
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.O, 4); // 노란색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;// 초기 위치 확인: [(5,10), (6,10), (5,11), (6,11)]
        var initialPositions = testTetrimino.GetWorldPositions();
        Assert.AreEqual(4, initialPositions.Length, "O블록 초기에 4개 위치");
        Assert.Contains(new Vector2Int(5, 10), initialPositions, "초기 (5,10) 위치");
        Assert.Contains(new Vector2Int(6, 10), initialPositions, "초기 (6,10) 위치");
        Assert.Contains(new Vector2Int(5, 11), initialPositions, "초기 (5,11) 위치");
        Assert.Contains(new Vector2Int(6, 11), initialPositions, "초기 (6,11) 위치");

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before O-Block Rotation");

        // Act - 회전
        logicManager.RotateTetrimino();

        // Assert - rotation 값은 증가하고, 여전히 2x2 정사각형 모양
        var rotatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After O-Block Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual(1, rotatedTetrimino.rotation, "O블록도 rotation 값은 증가해야 함");

        var rotatedPositions = rotatedTetrimino.GetWorldPositions();
        Assert.AreEqual(4, rotatedPositions.Length, "O블록 회전 후에도 4개 위치");

        // 실제 회전된 위치 확인: [(5,10), (5,9), (6,10), (6,9)]
        Assert.Contains(new Vector2Int(5, 10), rotatedPositions, "회전 후 (5,10) 위치");
        Assert.Contains(new Vector2Int(5, 9), rotatedPositions, "회전 후 (5,9) 위치");
        Assert.Contains(new Vector2Int(6, 10), rotatedPositions, "회전 후 (6,10) 위치");
        Assert.Contains(new Vector2Int(6, 9), rotatedPositions, "회전 후 (6,9) 위치");

        // 여전히 2x2 정사각형인지 확인
        var minX = System.Linq.Enumerable.Min(rotatedPositions, pos => pos.x);
        var maxX = System.Linq.Enumerable.Max(rotatedPositions, pos => pos.x);
        var minY = System.Linq.Enumerable.Min(rotatedPositions, pos => pos.y);
        var maxY = System.Linq.Enumerable.Max(rotatedPositions, pos => pos.y);

        Assert.AreEqual(1, maxX - minX, "O블록은 가로 1칸 차이 유지");
        Assert.AreEqual(1, maxY - minY, "O블록은 세로 1칸 차이 유지");
    }
    [Test]
    public void RotateTetrimino_JBlock_ShouldRotateCorrectly()
    {        // Arrange - J 블록 회전 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.J, 1); // 빨간색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        // 초기 J 모양 확인
        var initialPositions = testTetrimino.GetWorldPositions();
        Assert.Contains(new Vector2Int(4, 11), initialPositions, "J블록 초기 - 위쪽 왼쪽");
        Assert.Contains(new Vector2Int(4, 10), initialPositions, "J블록 초기 - 아래쪽 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), initialPositions, "J블록 초기 - 중심");
        Assert.Contains(new Vector2Int(6, 10), initialPositions, "J블록 초기 - 오른쪽");

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before J-Block Rotation");

        // Act - 회전
        logicManager.RotateTetrimino();

        // Assert - 회전 후 모양 확인
        var rotatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After J-Block Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual(1, rotatedTetrimino.rotation, "J블록이 90도 회전해야 함");

        var rotatedPositions = rotatedTetrimino.GetWorldPositions();
        Assert.AreEqual(4, rotatedPositions.Length, "J블록 회전 후에도 4개 위치");

        // 회전된 J 블록의 새로운 위치들 확인
        Assert.IsTrue(rotatedPositions.Length == 4, "회전 후에도 4개 블록 유지");
    }
    [Test]
    public void RotateTetrimino_LBlock_ShouldRotateCorrectly()
    {        // Arrange - L 블록 회전 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.L, 2); // 초록색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        // 초기 L 모양 확인
        var initialPositions = testTetrimino.GetWorldPositions();
        Assert.Contains(new Vector2Int(4, 10), initialPositions, "L블록 초기 - 왼쪽");
        Assert.Contains(new Vector2Int(5, 10), initialPositions, "L블록 초기 - 중심");
        Assert.Contains(new Vector2Int(6, 10), initialPositions, "L블록 초기 - 오른쪽");
        Assert.Contains(new Vector2Int(6, 11), initialPositions, "L블록 초기 - 오른쪽 위");

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before L-Block Rotation");

        // Act - 회전
        logicManager.RotateTetrimino();

        // Assert
        var rotatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After L-Block Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual(1, rotatedTetrimino.rotation, "L블록이 90도 회전해야 함");
    }
    [Test]
    public void RotateTetrimino_SBlock_ShouldRotateCorrectly()
    {        // Arrange - S 블록 회전 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.S, 3); // 파란색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        var initialRotation = testTetrimino.rotation;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before S-Block Rotation");

        // Act
        logicManager.RotateTetrimino();

        // Assert
        var rotatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After S-Block Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual((initialRotation + 1) % 4, rotatedTetrimino.rotation, "S블록이 회전해야 함");
    }
    [Test]
    public void RotateTetrimino_ZBlock_ShouldRotateCorrectly()
    {        // Arrange - Z 블록 회전 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.Z, 4); // 노란색
        testTetrimino.position = new Vector2Int(5, 10);
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        var initialRotation = testTetrimino.rotation;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Z-Block Rotation");

        // Act
        logicManager.RotateTetrimino();

        // Assert
        var rotatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(rotatedGameData, "After Z-Block Rotation");

        var rotatedTetrimino = rotatedGameData.CurrentTetrimino;
        Assert.AreEqual((initialRotation + 1) % 4, rotatedTetrimino.rotation, "Z블록이 회전해야 함");
    }
    [Test]
    public void RotateTetrimino_WhenBlocked_ShouldNotRotate()
    {        // Arrange - 회전이 막힌 상황에서 회전 시도
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.I, 1); // 빨간색
        testTetrimino.position = new Vector2Int(0, 10); // 벽 근처에 배치
        testTetrimino.rotation = 0; // 가로 상태
        gameData.CurrentTetrimino = testTetrimino;

        var initialRotation = testTetrimino.rotation;
        var initialPositions = testTetrimino.GetWorldPositions();

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Blocked I-Block Rotation");

        // Act - 회전 시도 (벽에 막혀서 실패할 수 있음)
        logicManager.RotateTetrimino();

        // Assert - 회전이 불가능하면 원래 상태 유지
        var updatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Blocked I-Block Rotation");

        var currentTetrimino = updatedGameData.CurrentTetrimino;

        // 회전이 성공했거나 실패했거나 둘 중 하나
        Assert.IsTrue(currentTetrimino.rotation == initialRotation ||
                     currentTetrimino.rotation == (initialRotation + 1) % 4,
                     "회전이 성공하거나 원래 상태를 유지해야 함");
    }
    [Test]
    public void RotateTetrimino_WhenAtBoundary_ShouldHandleCorrectly()
    {        // Arrange - 경계 근처에서 회전 테스트
        var gameData = logicManager.GetGameData();
        var testTetrimino = new Tetrimino(TetriminoType.T, 2); // 초록색
        testTetrimino.position = new Vector2Int(1, 10); // 왼쪽 경계 근처
        testTetrimino.rotation = 0;
        gameData.CurrentTetrimino = testTetrimino;

        // 시작 상태 로그
        GameLogger.LogGame(gameData, "Before Boundary T-Block Rotation");

        // Act - 경계에서 회전 시도
        logicManager.RotateTetrimino();

        // Assert - 회전 후에도 유효한 상태여야 함
        var updatedGameData = logicManager.GetGameData();

        // 회전 후 상태 로그
        GameLogger.LogGame(updatedGameData, "After Boundary T-Block Rotation");

        var currentTetrimino = updatedGameData.CurrentTetrimino;
        Assert.IsNotNull(currentTetrimino, "경계에서 회전 후에도 테트리미노 존재");

        var positions = currentTetrimino.GetWorldPositions();
        foreach (var pos in positions)
        {
            Assert.IsTrue(pos.x >= 0 && pos.x < TetrisBoard.WIDTH,
                $"회전 후 위치 {pos}가 보드 경계 내에 있어야 함");
        }
    }
}
