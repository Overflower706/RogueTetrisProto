using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class ScoreSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var score = GetScoreComponent();

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                score.CurrentScore = 0;
                score.TargetScore = 1000; // 예시로 1000점으로 

                Debug.Log("게임 시작, 초기 점수 설정: " + score.CurrentScore);
            }
        }

        private ScoreComponent GetScoreComponent()
        {
            var scoreEntities = Context.GetEntitiesWithComponent<ScoreComponent>();

            if (scoreEntities.Count == 0)
            {
                Debug.LogWarning("ScoreComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (scoreEntities.Count > 1)
            {
                Debug.LogWarning("ScoreComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return scoreEntities[0].GetComponent<ScoreComponent>();
        }
    }
}