using DG.Tweening;
using OVFL.ECS;
using UnityEngine;
using UnityEngine.UI;

public class TitleCanvasManager : MonoBehaviour, ICanvasManager, ISystem
{
    public Context Context { get; set; }
    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; private set; }

    [Header("담당 캔버스")]
    [SerializeField] private Canvas Canvas_Title;

    [Header("기본 UI")]
    [SerializeField] private Button Button_Play;
    [SerializeField] private Button Button_Quit;

    [Header("플레이 패널")]
    [SerializeField] private GameObject Panel_Play;
    [SerializeField] private Button Button_StartGame;
    [SerializeField] private Button Button_Back;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;

        Button_Play.onClick.AddListener(OnPlayButtonClicked);
        Button_Quit.onClick.AddListener(OnQuitButtonClicked);
        Button_StartGame.onClick.AddListener(OnStartGameButtonClicked);
        Button_Back.onClick.AddListener(OnBackButtonClicked);

        Panel_Play.SetActive(false);
        Canvas_Title.gameObject.SetActive(false);
    }

    public Tween Show()
    {
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() => Canvas_Title.gameObject.SetActive(true));

        return sequence;
    }

    public Tween Hide()
    {
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() => Panel_Play.SetActive(false));
        sequence.AppendCallback(() => Canvas_Title.gameObject.SetActive(false));

        return sequence;
    }

    public void Clear()
    {
        Button_Play.onClick.RemoveListener(OnPlayButtonClicked);
        Button_Quit.onClick.RemoveListener(OnQuitButtonClicked);
        Button_StartGame.onClick.RemoveListener(OnStartGameButtonClicked);
        Button_Back.onClick.RemoveListener(OnBackButtonClicked);

        Panel_Play.SetActive(false);
        Canvas_Title.gameObject.SetActive(false);
    }

    private void OnPlayButtonClicked()
    {
        Panel_Play.SetActive(true);
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnStartGameButtonClicked()
    {
        // PanelSceneManager.Instance.LoadGameScene();
        PanelSceneManager.Instance.LoadGameExScene();
    }

    private void OnBackButtonClicked()
    {
        Panel_Play.SetActive(false);
    }
}