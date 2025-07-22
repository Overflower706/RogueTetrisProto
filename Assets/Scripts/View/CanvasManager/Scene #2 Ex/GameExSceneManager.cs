using DG.Tweening;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class GameExSceneManager : MonoBehaviour, IMiniSceneManager, ISystem
    {
        public Context Context { get; set; }

        [Header("관리 캔버스")]
        [SerializeField] private GameExCanvasManager Canvas_GameEx;

        public void Init()
        {
            gameObject.SetActive(false);

            Canvas_GameEx.Init(this);
        }

        public void Clear()
        {
            Canvas_GameEx.Clear();
            gameObject.SetActive(false);
        }

        public Tween LoadScene()
        {
            gameObject.SetActive(true);

            Sequence sequence = DOTween.Sequence();

            return sequence.Append(Canvas_GameEx.Show());
        }

        public Tween UnloadScene()
        {
            return Canvas_GameEx.Hide()
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}