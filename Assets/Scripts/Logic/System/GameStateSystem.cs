using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class GameStateSystem : ISetupSystem, ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }
        public void Setup()
        {
            var state = GetState();
            state.CurrentState = GameState.Initial;
            state.GameTime = 0f;
        }

        public void Tick()
        {
            var state = GetState();

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                state.CurrentState = GameState.Playing;
                Debug.Log("게임 시작");
            }
        }

        public void Cleanup()
        {
            var state = GetState();

            if (state.CurrentState != GameState.Playing) return;

            var score = GetScore();

            // 게임 종료 조건 예시: 점수가 목표 점수에 도달하면 게임 종료
            if (score.CurrentScore >= score.TargetScore)
            {
                state.CurrentState = GameState.Victory;
                Debug.Log("게임 종료, 목표 점수 도달");

                // 게임 종료 명령 생성
                var commandComponent = GetCommandRequest();
                commandComponent.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.EndGame,
                    PayLoad = null
                });

                return;
            }

            var tetriminoQueue = GetTetriminoQueue();
            var boardTetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();

            int count = 0;
            foreach (var entity in boardTetriminoEntities)
            {
                var boardTetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (boardTetriminoComponent.State == BoardTetriminoState.Current)
                {
                    count++;
                }

                if (boardTetriminoComponent.State == BoardTetriminoState.Hold)
                {
                    count++;
                }
            }

            if (tetriminoQueue.TetriminoQueue.Count == 0 && count == 0)
            {
                state.CurrentState = GameState.GameOver;
                Debug.Log("게임 종료, 테트리미노를 다 썼지만 점수를 넘지 못했습니다.");

                // 게임 종료 명령 생성
                var commandComponent = GetCommandRequest();
                commandComponent.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.EndGame,
                    PayLoad = null
                });
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

        private CommandRequestComponent GetCommandRequest()
        {
            var commandRequestEntities = Context.GetEntitiesWithComponent<CommandRequestComponent>();
            if (commandRequestEntities.Count == 0)
            {
                Debug.LogWarning("CommandRequestComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (commandRequestEntities.Count > 1)
            {
                Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 첫 번째 엔티티를 사용합니다.");
            }

            return commandRequestEntities[0].GetComponent<CommandRequestComponent>();
        }

        private TetriminoQueueComponent GetTetriminoQueue()
        {
            var queueEntities = Context.GetEntitiesWithComponent<TetriminoQueueComponent>();
            if (queueEntities.Count == 0)
            {
                Debug.LogWarning("TetriminoQueueComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (queueEntities.Count > 1)
            {
                Debug.LogWarning("TetriminoQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return queueEntities[0].GetComponent<TetriminoQueueComponent>();
        }
    }
}