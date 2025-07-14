using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class PlayerSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        public void Setup()
        {
            var player = GetPlayer();
            player.Stage = 1;
            player.Round = 1;
            player.Currency = 0;
        }

        public void Tick()
        {
            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                var state = GetState();
                var player = GetPlayer();
                if (state.CurrentState == GameState.Victory)
                {
                    player.Stage++;
                    if (player.Stage > 3)
                    {
                        player.Stage = 1;
                        player.Round++;
                    }
                }
                else if (state.CurrentState == GameState.GameOver)
                {
                    // 게임 오버 상태에서 플레이어가 게임을 종료할 때의 로직
                    player.Round = 1;
                    player.Stage = 1;
                }
                else
                {
                    Debug.LogWarning("게임 종료 명령이 있지만 현재 상태가 게임 오버나 승리 상태가 아닙니다.");
                }
            }
        }

        private PlayerComponent GetPlayer()
        {
            var playerEntities = Context.GetEntitiesWithComponent<PlayerComponent>();
            if (playerEntities.Count == 0)
            {
                Debug.LogWarning("PlayerComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (playerEntities.Count > 1)
            {
                Debug.LogWarning("PlayerComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return playerEntities[0].GetComponent<PlayerComponent>();
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
    }
}