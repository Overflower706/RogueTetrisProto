using DG.Tweening;
using UnityEngine;

public class TitleSceneManager : MonoBehaviour, IMiniSceneManager
{
    [Header("관리 캔버스")]
    [SerializeField] private TitleCanvasManager CanvasManager_Title;

    private PanelSceneManager _panelSceneManager;
    public Game GameData { get { return _panelSceneManager.GameData; } }

    public void Init(PanelSceneManager panelSceneManager)
    {
        _panelSceneManager = panelSceneManager;
        CanvasManager_Title.Init(this);
        gameObject.SetActive(false);
    }

    public Tween LoadScene()
    {
        gameObject.SetActive(true);

        return CanvasManager_Title.Show();
    }

    public Tween UnloadScene()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(CanvasManager_Title.Hide())
                .AppendCallback(() => gameObject.SetActive(false));

        return sequence;
    }

    public void Clear()
    {
        CanvasManager_Title.Clear();
        _panelSceneManager = null;
    }
}