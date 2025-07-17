using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;

public class GameCanvasManager : MonoBehaviour, ICanvasManager, ITickSystem
{
    public Context Context { get; set; }

    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; private set; }
    private GameSceneManager gameSceneManager => SceneManager as GameSceneManager;


    [Header("관리 Canvas")]
    [SerializeField] private Canvas Canvas_Game;

    [Header("게임 패널")]
    [SerializeField] private GameObject Panel_Game;
    [SerializeField] private Button Button_Win;
    [SerializeField] private Button Button_Lose;

    [Header("게임 보드 패널")]
    [SerializeField] private GameBoardPanel gameboardPanel;
    [Header("게임 스코어 패널")]
    [SerializeField] private ScoreBoardPanel scoreBoardPanel;

    [Header("미리보기 UI")]
    [SerializeField] private TetriminoImage[] holdTetriminoImages = new TetriminoImage[3];
    [SerializeField] private TetriminoImage[] nextTetriminoImages = new TetriminoImage[3];
    [SerializeField] private TextMeshProUGUI restCountText;

    // 게임 상태 감시용
    private GameState _lastGameState = GameState.None;


    #region ICanvasManager

    private RectTransform _gameRectTransform;
    private Vector2 _gameOriginalPosition;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;

        Panel_Game.SetActive(false);
        Canvas_Game.gameObject.SetActive(false);

        _gameRectTransform = Panel_Game.GetComponent<RectTransform>();
        _gameOriginalPosition = _gameRectTransform.anchoredPosition;

        Button_Win.onClick.AddListener(OnWinButtonClicked);
        Button_Lose.onClick.AddListener(OnLoseButtonClicked);
    }

    public Tween Show()
    {
        Canvas_Game.gameObject.SetActive(true);

        _gameRectTransform.anchoredPosition = new Vector2(_gameOriginalPosition.x, _gameOriginalPosition.y - 1080f);
        Panel_Game.SetActive(true);
        Canvas_Game.GetComponent<CanvasGroup>().interactable = false;

        return DOTween.To(() => _gameRectTransform.anchoredPosition,
                   x => _gameRectTransform.anchoredPosition = x,
                   _gameOriginalPosition, 0.5f)
               .SetEase(Ease.OutQuart)
               .OnComplete(() =>
               {
                   Canvas_Game.GetComponent<CanvasGroup>().interactable = true;

                   var entities = Context.GetEntitiesWithComponent<CommandRequestComponent>();
                   if (entities.Count == 1)
                   {
                       var commandReqeust = entities[0].GetComponent<CommandRequestComponent>();
                       commandReqeust.Requests.Enqueue(new CommandRequest()
                       {
                           Type = CommandType.StartGame,
                           PayLoad = null
                       });
                   }
                   else
                   {
                       Debug.LogWarning($"CommandRequestComponent가 {entities.Count}개 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                       gameSceneManager.ShowStageCanvas();
                   }

                   // 게임 보드 및 스코어 패널 초기화
                   gameboardPanel.Init(Context);
                   scoreBoardPanel.Init(Context);
                   InitPreviewImages();
               });
    }

    public void Tick()
    {
        // GameState 변화 감시
        MonitorGameState();

        var state = GetState();

        if (state.CurrentState != GameState.Playing)
        {
            return;
        }

        if (gameboardPanel.IsInit)
        {
            // 게임 보드 상태 업데이트
            var entities = Context.GetEntitiesWithComponent<BoardComponent>();
            if (entities.Count == 1)
            {
                var boardComponent = entities[0].GetComponent<BoardComponent>();
                gameboardPanel.SetBoard(boardComponent);
            }
            else
            {
                Debug.LogWarning($"BoardComponent가 {entities.Count}개 존재합니다. 하나의 엔티티만 사용해야 합니다.");
            }

            // 홀드 및 다음 블록 미리보기 UI 업데이트
            UpdatePreviewUI();
        }
    }

    public Tween Hide()
    {
        Canvas_Game.GetComponent<CanvasGroup>().interactable = false;

        // 아래로 내려가는 애니메이션 후 비활성화
        return DOTween.To(() => _gameRectTransform.anchoredPosition,
                   x => _gameRectTransform.anchoredPosition = x,
                   new Vector2(_gameOriginalPosition.x, _gameOriginalPosition.y - 1080f), 0.3f)
               .SetEase(Ease.InQuart)
               .OnComplete(() =>
               {
                   Panel_Game.SetActive(false);
                   Canvas_Game.gameObject.SetActive(false);
               });
    }

    public void Clear()
    {
        // GameBoardPanel 청소
        if (gameboardPanel != null)
        {
            gameboardPanel.Clear();
        }

        // 홀드 테트리미노 이미지들 청소
        for (int i = 0; i < holdTetriminoImages.Length; i++)
        {
            holdTetriminoImages[i]?.ClearDisplay();
        }

        // 다음 테트리미노 이미지들 청소
        for (int i = 0; i < nextTetriminoImages.Length; i++)
        {
            nextTetriminoImages[i]?.ClearDisplay();
        }

        // 원위치
        _gameRectTransform.anchoredPosition = _gameOriginalPosition;
        Button_Win.onClick.RemoveListener(OnWinButtonClicked);
        Button_Lose.onClick.RemoveListener(OnLoseButtonClicked);
        Panel_Game.SetActive(false);
        Canvas_Game.gameObject.SetActive(false);
    }

    private void OnWinButtonClicked()
    {
        // 승리 로직 처리
        Debug.Log("승리!");
        gameSceneManager.ShowShopCanvas();
    }

    private void OnLoseButtonClicked()
    {
        // 패배 로직 처리
        Debug.Log("패배!");
        PanelSceneManager.Instance.LoadTitleScene();
    }

    #endregion



    /// <summary>
    /// 미리보기 이미지들 초기화 (홀드 3개 + 다음 3개 블록)
    /// </summary>
    private void InitPreviewImages()
    {
        // 홀드 이미지들 초기화
        for (int i = 0; i < holdTetriminoImages.Length; i++)
        {
            holdTetriminoImages[i]?.Init();
        }

        // 다음 블록 이미지들 초기화
        for (int i = 0; i < nextTetriminoImages.Length; i++)
        {
            nextTetriminoImages[i]?.Init();
        }
    }

    /// <summary>
    /// 홀드 및 다음 블록 미리보기 UI 업데이트
    /// </summary>
    private void UpdatePreviewUI()
    {
        if (Context == null) return;

        UpdateHoldUI();
        UpdateNextTetriminoUI();
        UpdateRestCountText();
    }


    /// <summary>
    /// 홀드 테트리미노들 UI 업데이트 (최대 3개)
    /// </summary>
    private void UpdateHoldUI()
    {
        if (holdTetriminoImages == null) return;

        var holdTetriminos = GetHoldTetriminos();

        for (int i = 0; i < holdTetriminoImages.Length; i++)
        {
            if (holdTetriminoImages[i] == null) continue;

            // i번째 홀드 테트리미노가 존재하는지 확인
            if (i < holdTetriminos.Length && holdTetriminos[i] != null)
            {
                // 테트리미노가 있으면 표시
                holdTetriminoImages[i].UpdateImage(holdTetriminos[i]);
            }
            else
            {
                // 테트리미노가 없으면 해당 번째 홀드 UI를 클리어
                holdTetriminoImages[i].ClearDisplay();
            }
        }
    }

    /// <summary>
    /// 다음 테트리미노들 UI 업데이트
    /// </summary>
    private void UpdateNextTetriminoUI()
    {
        if (nextTetriminoImages == null) return;

        var nextTetriminos = GetNextTetriminos();

        for (int i = 0; i < nextTetriminoImages.Length; i++)
        {
            if (nextTetriminoImages[i] == null) continue;

            // i번째 미리보기 테트리미노가 존재하는지 확인
            if (i < nextTetriminos.Length && nextTetriminos[i] != null)
            {
                // 테트리미노가 있으면 표시
                nextTetriminoImages[i].UpdateImage(nextTetriminos[i]);
            }
            else
            {
                // 테트리미노가 없으면 해당 번째 미리보기 UI를 클리어
                nextTetriminoImages[i].ClearDisplay();
            }
        }
    }

    /// <summary>
    /// 다음에 올 테트리미노들을 가져오는 헬퍼 메서드 (큐에서 3개)
    /// </summary>
    private TetriminoComponent[] GetNextTetriminos()
    {
        var queueEntities = Context.GetEntitiesWithComponent<TetriminoQueueComponent>();
        if (queueEntities == null || queueEntities.Count == 0)
        {
            // 큐가 없으면 빈 배열 반환 (모든 미리보기 UI가 클리어됨)
            return new TetriminoComponent[0];
        }

        if (queueEntities.Count > 1)
        {
            Debug.LogWarning($"TetriminoQueueComponent가 {queueEntities.Count}개 존재합니다. 첫 번째 것을 사용합니다.");
        }

        var queueComponent = queueEntities[0].GetComponent<TetriminoQueueComponent>();
        if (queueComponent?.TetriminoQueue == null)
        {
            // 큐 컴포넌트가 null이면 빈 배열 반환
            return new TetriminoComponent[0];
        }

        // 큐에서 앞에서부터 최대 3개까지 가져오기 (큐를 수정하지 않고 미리보기만)
        var queueArray = queueComponent.TetriminoQueue.ToArray();
        var maxCount = Mathf.Min(3, queueArray.Length);

        if (maxCount == 0)
        {
            // 큐가 비어있으면 빈 배열 반환 (모든 미리보기 UI가 클리어됨)
            return new TetriminoComponent[0];
        }

        var nextTetriminos = new TetriminoComponent[maxCount];

        for (int i = 0; i < maxCount; i++)
        {
            var entity = queueArray[i];
            nextTetriminos[i] = entity?.GetComponent<TetriminoComponent>();
        }

        return nextTetriminos;
    }

    /// <summary>
    /// 홀드된 테트리미노들을 가져오는 헬퍼 메서드 (최대 3개)
    /// </summary>
    private TetriminoComponent[] GetHoldTetriminos()
    {
        var holdQueueEntities = Context.GetEntitiesWithComponent<HoldQueueComponent>();
        if (holdQueueEntities == null || holdQueueEntities.Count == 0)
        {
            // 홀드 큐가 없으면 빈 배열 반환 (모든 홀드 UI가 클리어됨)
            return new TetriminoComponent[0];
        }

        if (holdQueueEntities.Count > 1)
        {
            Debug.LogWarning($"HoldQueueComponent가 {holdQueueEntities.Count}개 존재합니다. 첫 번째 것을 사용합니다.");
        }

        var holdQueueComponent = holdQueueEntities[0].GetComponent<HoldQueueComponent>();
        if (holdQueueComponent?.HoldQueue == null)
        {
            // 홀드 큐 컴포넌트가 null이면 빈 배열 반환
            return new TetriminoComponent[0];
        }

        // 홀드 큐에서 앞에서부터 최대 3개까지 가져오기 (큐를 수정하지 않고 미리보기만)
        var holdQueueArray = holdQueueComponent.HoldQueue.ToArray();
        var maxCount = Mathf.Min(3, holdQueueArray.Length);

        if (maxCount == 0)
        {
            // 홀드 큐가 비어있으면 빈 배열 반환 (모든 홀드 UI가 클리어됨)
            return new TetriminoComponent[0];
        }

        var holdTetriminos = new TetriminoComponent[maxCount];

        for (int i = 0; i < maxCount; i++)
        {
            var entity = holdQueueArray[i];
            holdTetriminos[i] = entity?.GetComponent<TetriminoComponent>();
        }

        return holdTetriminos;
    }

    private void UpdateRestCountText()
    {
        var queueEntities = Context.GetEntitiesWithComponent<TetriminoQueueComponent>();
        if (queueEntities == null || queueEntities.Count == 0)
        {
            restCountText.text = "-----";
            return;
        }

        if (queueEntities.Count > 1)
        {
            Debug.LogWarning($"TetriminoQueueComponent가 {queueEntities.Count}개 존재합니다. 첫 번째 것을 사용합니다.");
        }

        var queueComponent = queueEntities[0].GetComponent<TetriminoQueueComponent>();
        if (queueComponent?.TetriminoQueue == null)
        {
            restCountText.text = "+++++";
            return;
        }

        // 큐에 남은 테트리미노 개수 가져오기
        int remainingCount = queueComponent.TetriminoQueue.Count;
        restCountText.text = $"{remainingCount}";
    }

    /// <summary>
    /// 게임 상태 변화를 감시하여 버튼 활성화 처리
    /// </summary>
    private void MonitorGameState()
    {
        var gameStateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();
        if (gameStateEntities.Count == 0) return;

        var gameStateComponent = gameStateEntities[0].GetComponent<GameStateComponent>();
        var currentGameState = gameStateComponent.CurrentState;

        // 게임 상태가 변경된 경우 버튼 활성화 처리
        if (currentGameState != _lastGameState)
        {
            _lastGameState = currentGameState;

            switch (currentGameState)
            {
                case GameState.Victory:
                    Button_Win.gameObject.SetActive(true);
                    Button_Lose.gameObject.SetActive(false);
                    Debug.Log("승리! Win 버튼 활성화");
                    break;
                case GameState.GameOver:
                    Button_Win.gameObject.SetActive(false);
                    Button_Lose.gameObject.SetActive(true);
                    Debug.Log("게임 오버! Lose 버튼 활성화");
                    break;
                case GameState.Playing:
                    Button_Win.gameObject.SetActive(false);
                    Button_Lose.gameObject.SetActive(false);
                    break;
            }
        }
    }

    private GameStateComponent GetState()
    {
        var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();
        if (stateEntities.Count == 0)
        {
            Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
            return null;
        }
        else if (stateEntities.Count > 1)
        {
            Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
            return null;
        }

        return stateEntities[0].GetComponent<GameStateComponent>();
    }
}