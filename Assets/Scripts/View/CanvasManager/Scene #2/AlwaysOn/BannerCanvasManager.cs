using UnityEngine;
using DG.Tweening;
using System;

public class BannerCanvasManager : MonoBehaviour, ICanvasManager
{
    [field: SerializeField]
    public IMiniSceneManager SceneManager { get; private set; }
    public Game GameData => SceneManager.GameData;

    [Header("관리 Canvas")]
    [SerializeField] private Canvas Canvas_Banner;

    [Header("관리 패널")]
    [SerializeField] private GameObject Panel_Banner;

    private RectTransform _panelRectTransform;
    private Vector3 _originalPosition;

    public void Init(IMiniSceneManager miniSceneManager)
    {
        SceneManager = miniSceneManager;

        _panelRectTransform = Panel_Banner.GetComponent<RectTransform>();
        _originalPosition = _panelRectTransform.anchoredPosition;

        // 패널 처음에는 비활성화
        Panel_Banner.SetActive(false);
        Canvas_Banner.gameObject.SetActive(false);
    }

    public Tween Show()
    {
        Canvas_Banner.gameObject.SetActive(true);
        Panel_Banner.SetActive(true);

        // 화면 아래로 이동 (원래 위치에서 y축으로 -1000만큼 아래)
        _panelRectTransform.anchoredPosition = new Vector2(_originalPosition.x, _originalPosition.y - 1000f);

        Canvas_Banner.GetComponent<CanvasGroup>().interactable = false;

        // 원래 위치로 애니메이션
        return DOTween.To(() => _panelRectTransform.anchoredPosition,
                   x => _panelRectTransform.anchoredPosition = x,
                   (Vector2)_originalPosition, 0.5f)
               .SetEase(Ease.OutQuart)
               .OnComplete(() =>
               {
                   Canvas_Banner.GetComponent<CanvasGroup>().interactable = true;
               });
    }

    public Tween Hide()
    {
        Canvas_Banner.GetComponent<CanvasGroup>().interactable = false;
        Vector2 targetPosition = new Vector2(_originalPosition.x, _originalPosition.y - 1000f);

        var sequence = DOTween.Sequence();

        sequence.Append(DOTween.To(() => _panelRectTransform.anchoredPosition,
                                  x => _panelRectTransform.anchoredPosition = x,
                                  targetPosition, 0.5f)
                               .SetEase(Ease.InQuart))
               .AppendCallback(() =>
               {
                   Panel_Banner.SetActive(false);
                   Canvas_Banner.gameObject.SetActive(false);
               });

        return sequence;
    }

    public void Clear()
    {
        // 패널 처음에는 비활성화
        _panelRectTransform.anchoredPosition = _originalPosition;
        Panel_Banner.SetActive(false);
        Canvas_Banner.gameObject.SetActive(false);
    }
}