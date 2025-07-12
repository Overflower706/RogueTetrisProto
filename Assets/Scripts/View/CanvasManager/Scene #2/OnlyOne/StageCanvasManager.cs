using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using OVFL.ECS;

public class StageCanvasManager : MonoBehaviour, ICanvasManager, ISystem
{
    public Context Context { get; set; }

    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; set; }
    private GameSceneManager GameScene => SceneManager as GameSceneManager;


    [Header("관리 Canvas")]
    [SerializeField] private Canvas Canvas_Stage;

    [Header("패널 Small Blind")]
    [SerializeField] private GameObject Panel_SmallBlind;
    [SerializeField] private Button Button_StartSmallGame; // 워워 도현 just small
    [SerializeField] private Button Button_SkipSmallGame;

    [Header("패널 Big Blind")]
    [SerializeField] private GameObject Panel_BigBlind;
    [SerializeField] private Button Button_StartBigGame; // like Kingen
    [SerializeField] private Button Button_SkipBigGame;

    [Header("패널 Boss Blind")]
    [SerializeField] private GameObject Panel_BossBlind;
    [SerializeField] private Button Button_StartBossGame; // Unkillable
    [SerializeField] private Button Button_SkipBossGame;

    [Header("시작 화면으로 나가기 버튼")]
    [SerializeField] private Button Button_BackToTitle;

    private RectTransform _smallBlindRectTransform;
    private RectTransform _bigBlindRectTransform;
    private RectTransform _bossBlindRectTransform;
    private RectTransform _backToStartRectTransform;

    private Vector2 _smallBlindOriginalPosition;
    private Vector2 _bigBlindOriginalPosition;
    private Vector2 _bossBlindOriginalPosition;
    private Vector2 _backToStartOriginalPosition;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;

        _smallBlindRectTransform = Panel_SmallBlind.GetComponent<RectTransform>();
        _bigBlindRectTransform = Panel_BigBlind.GetComponent<RectTransform>();
        _bossBlindRectTransform = Panel_BossBlind.GetComponent<RectTransform>();
        _backToStartRectTransform = Button_BackToTitle.GetComponent<RectTransform>();

        _smallBlindOriginalPosition = _smallBlindRectTransform.anchoredPosition;
        _bigBlindOriginalPosition = _bigBlindRectTransform.anchoredPosition;
        _bossBlindOriginalPosition = _bossBlindRectTransform.anchoredPosition;
        _backToStartOriginalPosition = _backToStartRectTransform.anchoredPosition;

        // 모든 패널 처음에는 비활성화
        Panel_SmallBlind.SetActive(false);
        Panel_BigBlind.SetActive(false);
        Panel_BossBlind.SetActive(false);
        Button_BackToTitle.gameObject.SetActive(false);

        // StartSmallGame만 이벤트 등록합니다.
        Button_StartSmallGame.onClick.AddListener(OnStartSmallGameClicked);
        Button_BackToTitle.onClick.AddListener(() =>
        {
            // PanelSceneManager.Instance.LoadTitleScene();
        });

        Canvas_Stage.gameObject.SetActive(false);
    }

    public Tween Show()
    {
        Canvas_Stage.gameObject.SetActive(true);

        // 화면 아래로 이동
        _smallBlindRectTransform.anchoredPosition = new Vector2(_smallBlindOriginalPosition.x, _smallBlindOriginalPosition.y - 1080f);
        _bigBlindRectTransform.anchoredPosition = new Vector2(_bigBlindOriginalPosition.x, _bigBlindOriginalPosition.y - 1080f);
        _bossBlindRectTransform.anchoredPosition = new Vector2(_bossBlindOriginalPosition.x, _bossBlindOriginalPosition.y - 1080f);
        _backToStartRectTransform.anchoredPosition = new Vector2(_backToStartOriginalPosition.x, _backToStartOriginalPosition.y - 1080f);

        // 순차적으로 올라오는 애니메이션
        var sequence = DOTween.Sequence();

        sequence.AppendCallback(() => Canvas_Stage.GetComponent<CanvasGroup>().interactable = false);
        sequence.AppendCallback(() => Panel_SmallBlind.SetActive(true));
        sequence.AppendCallback(() => Button_StartSmallGame.interactable = true);
        sequence.Append(DOTween.To(() => _smallBlindRectTransform.anchoredPosition,
                                   x => _smallBlindRectTransform.anchoredPosition = x,
                                   _smallBlindOriginalPosition, 0.5f)
                               .SetEase(Ease.OutQuart));

        sequence.AppendCallback(() => Panel_BigBlind.SetActive(true));
        sequence.Append(DOTween.To(() => _bigBlindRectTransform.anchoredPosition,
                                   x => _bigBlindRectTransform.anchoredPosition = x,
                                   _bigBlindOriginalPosition, 0.5f)
                               .SetEase(Ease.OutQuart));

        sequence.AppendCallback(() => Panel_BossBlind.SetActive(true));
        sequence.Append(DOTween.To(() => _bossBlindRectTransform.anchoredPosition,
                                   x => _bossBlindRectTransform.anchoredPosition = x,
                                   _bossBlindOriginalPosition, 0.5f)
                               .SetEase(Ease.OutQuart));

        sequence.AppendCallback(() => Button_BackToTitle.gameObject.SetActive(true));
        sequence.Append(DOTween.To(() => _backToStartRectTransform.anchoredPosition,
                                   x => _backToStartRectTransform.anchoredPosition = x,
                                   _backToStartOriginalPosition, 0.5f)
                               .SetEase(Ease.OutQuart));
        sequence.AppendCallback(() => Canvas_Stage.GetComponent<CanvasGroup>().interactable = true);

        return sequence;
    }

    public Tween Hide()
    {
        Canvas_Stage.GetComponent<CanvasGroup>().interactable = false;

        var sequence = DOTween.Sequence();

        sequence.Append(DOTween.To(() => _smallBlindRectTransform.anchoredPosition,
                                  x => _smallBlindRectTransform.anchoredPosition = x,
                                  new Vector2(_smallBlindOriginalPosition.x, _smallBlindOriginalPosition.y - 1080f), 0.5f)
                                .SetEase(Ease.InQuart));

        sequence.Join(DOTween.To(() => _bigBlindRectTransform.anchoredPosition,
                                x => _bigBlindRectTransform.anchoredPosition = x,
                                new Vector2(_bigBlindOriginalPosition.x, _bigBlindOriginalPosition.y - 1080f), 0.5f)
                              .SetEase(Ease.InQuart));

        sequence.Join(DOTween.To(() => _bossBlindRectTransform.anchoredPosition,
                                x => _bossBlindRectTransform.anchoredPosition = x,
                                new Vector2(_bossBlindOriginalPosition.x, _bossBlindOriginalPosition.y - 1080f), 0.5f)
                              .SetEase(Ease.InQuart));

        sequence.Join(DOTween.To(() => _backToStartRectTransform.anchoredPosition,
                                x => _backToStartRectTransform.anchoredPosition = x,
                                new Vector2(_backToStartOriginalPosition.x, _backToStartOriginalPosition.y - 1080f), 0.5f)
                              .SetEase(Ease.InQuart));

        sequence.AppendCallback(() =>
        {
            Panel_SmallBlind.SetActive(false);
            Panel_BigBlind.SetActive(false);
            Panel_BossBlind.SetActive(false);
            Button_BackToTitle.gameObject.SetActive(false);
            Canvas_Stage.gameObject.SetActive(false);
        });

        return sequence;
    }

    public void Clear()
    {
        // 이벤트 리스너 제거
        Button_StartSmallGame.onClick.RemoveListener(OnStartSmallGameClicked);
        Button_SkipSmallGame.onClick.RemoveAllListeners();
        Button_StartBigGame.onClick.RemoveAllListeners();
        Button_SkipBigGame.onClick.RemoveAllListeners();
        Button_StartBossGame.onClick.RemoveAllListeners();
        Button_SkipBossGame.onClick.RemoveAllListeners();
        Button_BackToTitle.onClick.RemoveAllListeners();
        Button_BackToTitle.gameObject.SetActive(false);

        // 모든 패널과 캔버스를 닫는다.
        Panel_SmallBlind.SetActive(false);
        Panel_BigBlind.SetActive(false);
        Panel_BossBlind.SetActive(false);
        Button_BackToTitle.gameObject.SetActive(false);
        Canvas_Stage.gameObject.SetActive(false);

        // Panel들 다시 원위치
        _smallBlindRectTransform.anchoredPosition = _smallBlindOriginalPosition;
        _bigBlindRectTransform.anchoredPosition = _bigBlindOriginalPosition;
        _bossBlindRectTransform.anchoredPosition = _bossBlindOriginalPosition;
        _backToStartRectTransform.anchoredPosition = _backToStartOriginalPosition;
    }

    private void OnStartSmallGameClicked()
    {
        Debug.Log("Small Game 시작!");
        Button_StartSmallGame.interactable = false; // 버튼 비활성화

        GameScene.ShowGameCanvas();
    }
}