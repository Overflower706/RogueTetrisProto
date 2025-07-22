using System;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class ScoreSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var score = Context.GetScore();

            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                var player = Context.GetPlayer();

                Debug.Log($"게임 시작, 플레이어 정보: 라운드 {player.Round}, 스테이지 {player.Stage}");
                Debug.Log($"플레이어의 라운드 보너스: {GlobalSettings.Instance.RoundBonus}, 스테이지 보너스: {GlobalSettings.Instance.StageBonus}");

                score.CurrentScore = 0;
                score.TargetScore = (int)(Math.Pow(player.Round, GlobalSettings.Instance.RoundBonus) * player.Stage * GlobalSettings.Instance.StageBonus);
                Debug.Log($"게임 시작, 초기 점수 설정: {score.CurrentScore}, 목표 점수: {score.TargetScore}");
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                Debug.Log("게임 종료, 최종 점수: " + score.CurrentScore);
                score.CurrentScore = 0; // 게임 종료 시 점수 초기화
                score.TargetScore = 0; // 목표 점수도 초기화
                return; // 게임 종료 시 더 이상 점수 계산하지 않음
            }

            var completedLineComponentEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            if (completedLineComponentEntities.Count > 0)
            {
                var board = Context.GetBoard();
                var completedLineEntity = completedLineComponentEntities[0];
                var completedLineComponent = completedLineEntity.GetComponent<CompletedLineComponent>();

                foreach (var line in completedLineComponent.CompletedLine)
                {
                    for (int x = 0; x < BoardComponent.WIDTH; x++)
                    {
                        int entityID = board.Board[x, line];
                        if (entityID != 0)
                        {
                            var minoEntity = Context.FindEntityByID(entityID);
                            var minoComponent = minoEntity.GetComponent<MinoComponent>();

                            if (minoComponent.State == MinoState.Living)
                            {
                                score.CurrentScore += 1;
                                Debug.Log($"{line}번째 줄에서 Mino ID {entityID}의 상태를 Living으로 변경하고 점수에 1점 추가했습니다. 현재 점수: {score.CurrentScore}");
                            }
                            Debug.Log($"높이 {line}의 Mino ID {entityID}의 상태를 Living으로 변경했습니다.");
                        }
                    }
                }

                Context.DestroyEntity(completedLineEntity);
            }
        }
    }
}