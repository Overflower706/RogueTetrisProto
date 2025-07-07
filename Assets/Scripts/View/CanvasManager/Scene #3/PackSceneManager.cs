using DG.Tweening;
using UnityEngine;

public class PackSceneManager : MonoBehaviour, IMiniSceneManager
{
    [Header("관리 캔버스")]
    [SerializeField] private PackCanvasManager CanvasManager_Pack;

    public Game GameData => _panelSceneManager.GameData;
    private PanelSceneManager _panelSceneManager;

    public void Init(PanelSceneManager panelSceneManager)
    {
        _panelSceneManager = panelSceneManager;
    }

    public Tween LoadScene()
    {
        gameObject.SetActive(true);
        CanvasManager_Pack.Init(this);

        return CanvasManager_Pack.Show();
    }

    public Tween UnloadScene()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(CanvasManager_Pack.Hide());
        sequence.AppendCallback(() => CanvasManager_Pack.Clear());
        sequence.AppendCallback(() => gameObject.SetActive(false));

        return sequence;
    }

    public void Clear()
    {
        _panelSceneManager = null;
    }
}
