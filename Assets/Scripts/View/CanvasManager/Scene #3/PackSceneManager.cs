using DG.Tweening;
using OVFL.ECS;
using UnityEngine;

public class PackSceneManager : MonoBehaviour, IMiniSceneManager, ISystem
{
    public Context Context { get; set; }

    [Header("관리 캔버스")]
    [SerializeField] private PackCanvasManager CanvasManager_Pack;

    public void Init()
    {
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
    }
}
