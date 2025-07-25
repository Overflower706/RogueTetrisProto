using OVFL.ECS;
using TMPro;
using UnityEngine;

namespace Minomino
{
    public class ScorePanelManager : MonoBehaviour, ITickSystem
    {
        public Context Context { get; set; }

        [SerializeField] private TMP_Text Text_Score;

        public void Tick()
        {
            if (!gameObject.activeSelf) return;

            var score = Context.GetScore();
            Text_Score.text = $"목표 점수: {score.TargetScore}\n" +
                              $"현재 점수: {score.CurrentScore}";
        }
    }
}