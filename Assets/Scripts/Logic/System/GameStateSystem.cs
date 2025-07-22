using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class GameStateSystem : ISetupSystem, ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }
        public void Setup()
        {
            var state = Context.GetGameState();
            state.CurrentState = GameState.Initial;
            state.GameTime = 0f;
        }

        public void Tick()
        {
            var state = Context.GetGameState();

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                state.CurrentState = GameState.Playing;
                Debug.Log("게임 시작");
            }
        }

        public void Cleanup()
        {
            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;

            var score = Context.GetScore();

            // 게임 종료 조건 예시: 점수가 목표 점수에 도달하면 게임 종료
            if (score.CurrentScore >= score.TargetScore)
            {
                state.CurrentState = GameState.Victory;
                Debug.Log("게임 종료, 목표 점수 도달");

                // 게임 종료 명령 생성
                var commandComponent = Context.GetCommandRequest();
                commandComponent.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.EndGame,
                    PayLoad = null
                });

                return;
            }

            var tetriminoQueue = Context.GetTetrominoQueue();
            var boardTetriminoEntities = Context.GetEntitiesWithComponent<BoardTetrominoComponent>();

            int count = 0;
            foreach (var entity in boardTetriminoEntities)
            {
                var boardTetriminoComponent = entity.GetComponent<BoardTetrominoComponent>();
                if (boardTetriminoComponent.State == BoardTetrominoState.Current)
                {
                    count++;
                }

                if (boardTetriminoComponent.State == BoardTetrominoState.Hold)
                {
                    count++;
                }
            }

            if (tetriminoQueue.TetrominoQueue.Count == 0 && count == 0)
            {
                state.CurrentState = GameState.GameOver;
                Debug.Log("게임 종료, 테트리미노를 다 썼지만 점수를 넘지 못했습니다.");

                // 게임 종료 명령 생성
                var commandComponent = Context.GetCommandRequest();
                commandComponent.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.EndGame,
                    PayLoad = null
                });

                return;
            }
        }

    }
}