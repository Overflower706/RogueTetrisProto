using DG.Tweening;
using OVFL.ECS;
using UnityEngine;

public class TitleSceneManager : MonoBehaviour, IMiniSceneManager, ISystem
{
    public Context Context { get; set; }

    [Header("관리 캔버스")]
    [SerializeField] private TitleCanvasManager CanvasManager_Title;

    public void Init()
    {
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
    }
}