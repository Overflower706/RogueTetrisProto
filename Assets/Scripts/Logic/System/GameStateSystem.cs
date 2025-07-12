using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class GameStateSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }
        public void Setup(Context context)
        {
            var state = GetStateComponent(context);
            state.CurrentState = GameState.Initial;
            state.GameTime = 0f;
        }

        public void Tick(Context context)
        {
            var state = GetStateComponent(context);

            if (state == null)
            {
                Debug.LogWarning("GameStateComponent가 없습니다.");
                return;
            }

            var startCommand = GetStartGameCommand(context);
            if (startCommand != null)
            {
                state.CurrentState = GameState.Playing;
                Debug.Log("게임 시작");
            }

            if (state.CurrentState == GameState.Playing)
            {
                state.GameTime += Time.deltaTime;
            }
        }

        private GameStateComponent GetStateComponent(Context context)
        {
            var stateEntities = context.GetEntitiesWithComponent<GameStateComponent>();
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

        private StartGameCommand GetStartGameCommand(Context context)
        {
            var commandEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count == 0)
            {
                Debug.LogWarning("StartGameCommand가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (commandEntities.Count > 1)
            {
                Debug.LogWarning("StartGameCommand가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return commandEntities[0].GetComponent<StartGameCommand>();
        }
    }
}