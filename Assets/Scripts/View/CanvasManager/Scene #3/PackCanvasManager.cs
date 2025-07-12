using DG.Tweening;
using OVFL.ECS;
using UnityEngine;

public class PackCanvasManager : MonoBehaviour, ICanvasManager, ISystem
{
    public Context Context { get; set; }

    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; set; }
    private PackSceneManager _packSceneManager => SceneManager as PackSceneManager;

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