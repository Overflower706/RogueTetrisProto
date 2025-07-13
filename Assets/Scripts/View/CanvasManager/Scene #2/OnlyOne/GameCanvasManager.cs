using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using UnityEngine.InputSystem;

public class GameCanvasManager : MonoBehaviour, ICanvasManager, ISystem
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
    [SerializeField] private TestViewUI TestViewUI;

    [Header("게임 보드 패널")]
    [SerializeField] private GameBoardPanel gameboardPanel;


    private RectTransform _gameRectTransform;
    private Vector2 _gameOriginalPosition;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;

        TestViewUI.Init(this);

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
                   //    LogicManager.Instance.StartGame();
                   //    TestViewUI.StartGame();

                   gameboardPanel.Init(Context);
               });
    }

    private void Update()
    {
        if (Canvas_Game.gameObject.activeSelf)
        {
            // 게임 상태 업데이트
            TestViewUI.UpdateGameState();

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
        // PanelSceneManager.Instance.LoadTitleScene();
    }
}