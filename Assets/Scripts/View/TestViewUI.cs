using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Minomino;
using OVFL.ECS;

public class TestViewUI : MonoBehaviour, ISystem
{
    public Context Context { get; set; }

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI gameStateText;

    [Header("Input Settings")]
    [SerializeField] private float moveRepeatRate = 0.1f;
    [SerializeField] private float moveInitialDelay = 0.2f;

    private ICanvasManager _canvasManager;

    private float moveTimer = 0f;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isMovingDown = false;

    private bool isStarted = false;

    // Input System 관련
    private Keyboard keyboard;


    public void Init(ICanvasManager canvasManager)
    {
        _canvasManager = canvasManager;
        // Keyboard 입력 초기화
        keyboard = Keyboard.current;
    }

    public void Clear()
    {
        _canvasManager = null;
    }

    public void StartGame()
    {
        Debug.Log("=== TestViewUI Start 시작 ===");

        // 초기 UI 업데이트
        UpdateUI();

        isStarted = true;

        Debug.Log("=== TestViewUI 초기화 완료 ===");
    }

    public void UpdateGameState()
    {
        if (!isStarted) return;

        // 입력 처리
        HandleInput();

        // UI 업데이트
        UpdateUI();
    }

    private void HandleInput()
    {
        // var gameData = _canvasManager.GameData;
        // if (gameData == null || gameData.CurrentState != GameState.Playing) return;

        // bool inputReceived = false;

        // // 회전 (한 번 누르기)
        // if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame)
        // {
        //     LogicManager.Instance.RotateTetrimino();
        //     inputReceived = true;
        // }

        // // 하드 드롭 (한 번 누르기)
        // if (keyboard.spaceKey.wasPressedThisFrame)
        // {
        //     LogicManager.Instance.DropTetrimino();
        //     inputReceived = true;
        // }

        // // 연속 이동 처리
        // HandleContinuousMovement(ref inputReceived);

        // // 입력이 있었으면 UI 즉시 업데이트
        // if (inputReceived)
        // {
        //     UpdateUI();
        // }
    }
    private void HandleContinuousMovement(ref bool inputReceived)
    {
        if (keyboard == null) return;

        // 현재 프레임에서 눌린 키 확인
        bool leftPressed = keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed;
        bool rightPressed = keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed;
        bool downPressed = keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed;

        // 키가 눌렸을 때 즉시 한 번 이동 (첫 입력)
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            LogicManager.Instance.MoveTetrimino(Vector2Int.left);
            isMovingLeft = true;
            moveTimer = moveInitialDelay;
            inputReceived = true;
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            LogicManager.Instance.MoveTetrimino(Vector2Int.right);
            isMovingRight = true;
            moveTimer = moveInitialDelay;
            inputReceived = true;
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            LogicManager.Instance.SoftDrop();
            isMovingDown = true;
            moveTimer = moveInitialDelay;
            inputReceived = true;
        }

        // 키가 떼어졌을 때 연속 이동 중지
        if (!leftPressed) isMovingLeft = false;
        if (!rightPressed) isMovingRight = false;
        if (!downPressed) isMovingDown = false;

        // 연속 이동 타이머 업데이트
        if (isMovingLeft || isMovingRight || isMovingDown)
        {
            moveTimer -= Time.deltaTime;

            if (moveTimer <= 0f)
            {
                if (isMovingLeft && leftPressed)
                {
                    LogicManager.Instance.MoveTetrimino(Vector2Int.left);
                    inputReceived = true;
                }
                else if (isMovingRight && rightPressed)
                {
                    LogicManager.Instance.MoveTetrimino(Vector2Int.right);
                    inputReceived = true;
                }
                else if (isMovingDown && downPressed)
                {
                    LogicManager.Instance.SoftDrop();
                    inputReceived = true;
                }

                moveTimer = moveRepeatRate;
            }
        }
    }
    private void UpdateUI()
    {
        // var gameData = _canvasManager.GameData;
        // if (gameData == null) return;

        // // 간단한 정보 + 보드만 표시
        // string gameInfo = $"Score: {gameData.CurrentScore}/{gameData.TargetScore} | State: {gameData.CurrentState}\n\n";
        // string boardInfo = GameLogger.GetBoardString(gameData.Board, gameData.CurrentTetrimino);

        // // UI 텍스트 업데이트
        // gameStateText.text = gameInfo + boardInfo;

        // // 게임 오버나 승리 시 추가 정보 표시
        // if (gameData.CurrentState == GameState.GameOver)
        // {
        //     gameStateText.text += "\n\n=== 게임 오버 ===\n";
        // }
        // else if (gameData.CurrentState == GameState.Victory)
        // {
        //     gameStateText.text += "\n\n=== 승리! ===\n";
        // }
        // else if (gameData.CurrentState == GameState.Playing)
        // {
        //     gameStateText.text += "\n\n=== 조작법 ===\n" +
        //                           "← → : 좌우 이동\n" +
        //                           "↓ : 소프트 드롭\n" +
        //                           "↑ / Z : 회전\n" +
        //                           "스페이스 : 하드 드롭\n";
        // }
    }

    // 외부에서 UI 업데이트를 요청할 수 있는 메서드
    public void ForceUpdateUI()
    {
        UpdateUI();
    }

    // 디버그용 메서드들
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnGUI()
    {
        // var gameData = _canvasManager.GameData;
        // if (gameData == null) return;

        // // 화면 우상단에 간단한 정보 표시
        // GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 100));
        // GUILayout.Label($"Score: {gameData.CurrentScore}/{gameData.TargetScore}");
        // GUILayout.Label($"State: {gameData.CurrentState}");
        // GUILayout.Label($"Currency: {gameData.Currency}");
        // if (gameData.CurrentTetrimino != null)
        // {
        //     GUILayout.Label($"Current: {gameData.CurrentTetrimino.type}");
        // }
        // if (gameData.NextTetrimino != null)
        // {
        //     GUILayout.Label($"Next: {gameData.NextTetrimino.type}");
        // }
        // GUILayout.EndArea();
    }
}
