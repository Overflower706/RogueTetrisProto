using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class PlayerSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        public void Setup()
        {
            var player = Context.GetPlayer();
            player.Stage = 1;
            player.Round = 1;
            player.Currency = 0;

            Debug.Log($"플레이어 설정 완료: 스테이지 {player.Stage}, 라운드 {player.Round}, 통화 {player.Currency}");
        }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                var player = Context.GetPlayer();
                player.BakeCount = GlobalSettings.Instance.BaseBakeCount;
                player.TrashCount = GlobalSettings.Instance.BaseTrashCount;
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                var state = Context.GetGameState();
                var player = Context.GetPlayer();
                if (state.CurrentState == GameState.Victory)
                {
                    player.Stage++;
                    if (player.Stage > 3)
                    {
                        player.Stage = 1;
                        player.Round++;
                    }
                    player.Currency += GlobalSettings.Instance.CurrencyBase + Context.GetTetriminoQueue().TetriminoQueue.Count * GlobalSettings.Instance.CurrencyBonusPerDeck;
                    Debug.Log($"게임 승리! 현재 스테이지: {player.Stage}, 라운드: {player.Round}, 통화: {player.Currency}");
                }
                else if (state.CurrentState == GameState.GameOver)
                {
                    // 게임 오버 상태에서 플레이어가 게임을 종료할 때의 로직
                    player.Round = 1;
                    player.Stage = 1;
                    player.Currency = 0;
                }
                else
                {
                    Debug.LogWarning("게임 종료 명령이 있지만 현재 상태가 게임 오버나 승리 상태가 아닙니다.");
                }
            }
        }
    }
}