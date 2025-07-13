using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class PlayerSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var player = GetPlayerComponent();

            var commandEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                player.Currency = 0;
                Debug.Log("게임 시작, 초기 현금 설정: " + player.Currency);
            }
        }

        private PlayerComponent GetPlayerComponent()
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
    }
}