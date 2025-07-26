using Codice.Client.BaseCommands;
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
            ObserveStartGame();
        }

        public void Cleanup()
        {
            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;
            var score = Context.GetScore();

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

            var board = Context.GetBoard();

            for (int y = GlobalSettings.Instance.SafeHeight; y < GlobalSettings.Instance.BoardHeight; y++)
            {
                for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
                {
                    if (board.Board[x, y] != 0)
                    {
                        // 검증
                        var minoEntity = Context.FindEntityByID(board.Board[x, y]);
                        var minoComponent = minoEntity.GetComponent<MinoComponent>();
                        var tetriminoEntity = Context.FindEntityByID(minoComponent.ParentID);
                        var boardComponent = tetriminoEntity.GetComponent<BoardTetrominoComponent>();
                        if (boardComponent != null && boardComponent.State == BoardTetrominoState.Current)
                        {
                            continue; // 현재 테트리미노는 버퍼 존에 있어도 괜찮음
                        }

                        // 버퍼 존에 블록이 있는 경우 게임 오버 상태로 전환
                        state.CurrentState = GameState.GameOver;
                        Debug.Log("게임 종료, 버퍼줄에 블록이 있습니다. 게임 오버 상태로 전환합니다.");

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

        private void ObserveStartGame()
        {
            var state = Context.GetGameState();

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                state.CurrentState = GameState.Playing;
                Debug.Log("게임 시작");
            }
        }
    }
}