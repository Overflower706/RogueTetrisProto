using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class GameStateSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }
        public void Setup(Context context)
        {
            var state = GetState();
            state.CurrentState = GameState.Initial;
            state.GameTime = 0f;
        }

        public void Tick(Context context)
        {
            var state = GetState();

            var commandEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                state.CurrentState = GameState.Playing;
                Debug.Log("게임 시작");
            }

            var score = GetScore();

            if (state.CurrentState == GameState.Playing)
            {
                // 게임 종료 조건 예시: 점수가 목표 점수에 도달하면 게임 종료
                if (score.CurrentScore >= score.TargetScore)
                {
                    state.CurrentState = GameState.Victory;
                    Debug.Log("게임 종료, 목표 점수 도달");
                }
            }
        }

        private GameStateComponent GetState()
        {
            var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();
            if (stateEntities.Count == 0)
            {
                Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (stateEntities.Count > 1)
            {
                Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return stateEntities[0].GetComponent<GameStateComponent>();
        }

        private ScoreComponent GetScore()
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