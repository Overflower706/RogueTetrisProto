using DG.Tweening;
using UnityEngine;

public class PackCanvasManager : MonoBehaviour, ICanvasManager
{
    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; set; }
    private PackSceneManager _packSceneManager => SceneManager as PackSceneManager;
    public Game GameData => _packSceneManager.GameData;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;
    }

    public Tween Show()
    {
        return default;
    }

    public Tween Hide()
    {
        return default;
    }

    public void Clear()
    {

    }
}