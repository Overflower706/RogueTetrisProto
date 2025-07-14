using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class CurrentTetriminoSystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                Debug.Log("게임 시작. CurrentTetriminoSystem에서 처리");
                GenerateTetrimino();
            }
        }

        public void Cleanup()
        {
            var state = GetState();

            if (state.CurrentState != GameState.Playing) return;

            // 현재 테트리미노가 없으면 새로 생성
            var currentTetrimino = GetCurrentTetrimino();
            if (currentTetrimino == null)
            {
                GenerateTetrimino();
            }
        }

        private GameStateComponent GetState()
        {
            var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();

            if (stateEntities.Count == 0)
            {
                Debug.LogWarning("게임 상태 컴포넌트가 없습니다.");
                return null;
            }

            if (stateEntities.Count > 1)
            {
                Debug.LogWarning("게임 상태 컴포넌트가 여러 개 있습니다.");
                return null;
            }

            return stateEntities[0].GetComponent<GameStateComponent>();
        }

        private BoardComponent GetBoard()
        {
            var boardEntities = Context.GetEntitiesWithComponent<BoardComponent>();

            if (boardEntities.Count == 0)
            {
                Debug.LogWarning("보드 컴포넌트가 없습니다.");
                return null;
            }

            if (boardEntities.Count > 1)
            {
                Debug.LogWarning("보드 컴포넌트가 여러 개 있습니다.");
                return null;
            }

            return boardEntities[0].GetComponent<BoardComponent>();
        }

        private BoardTetriminoComponent GetCurrentTetrimino()
        {
            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();

            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (tetriminoComponent.State == BoardTetriminoState.Current)
                {
                    return tetriminoComponent;
                }
            }
            return null;
        }

        private BoardTetriminoComponent GetHoldTetriminoComponent()
        {
            var holdEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();

            foreach (var entity in holdEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (tetriminoComponent.State == BoardTetriminoState.Hold)
                {
                    return tetriminoComponent;
                }
            }

            return null;
        }

        private TetriminoQueueComponent GetTetriminoQueue()
        {
            var queueEntities = Context.GetEntitiesWithComponent<TetriminoQueueComponent>();
            if (queueEntities.Count == 0)
            {
                Debug.LogWarning("TetriminoQueueComponent가 있는 엔티티가 없습니다.");
                return null; // 큐가 없으면 null 반환
            }
            else if (queueEntities.Count > 1)
            {
                Debug.LogWarning("TetriminoQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null; // 여러 엔티티가 있으면 경고 후 null 반환
            }
            return queueEntities[0].GetComponent<TetriminoQueueComponent>();
        }

        private void GenerateTetrimino()
        {
            var queueEntity = GetTetriminoQueue();

            if (queueEntity.TetriminoQueue.Count > 0)
            {
                var nextTetrimino = queueEntity.TetriminoQueue.Dequeue();
                var boardTetriminoComponent = nextTetrimino.AddComponent<BoardTetriminoComponent>();
                boardTetriminoComponent.State = BoardTetriminoState.Current;
                boardTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                boardTetriminoComponent.Rotation = 0; // 초기 회전 상태

                Debug.Log($"새로운 Current Tetrimino 생성: Type: {nextTetrimino.GetComponent<TetriminoComponent>().Type}, Position: {boardTetriminoComponent.Position}");
            }
            else // TetriminoQueue가 비었다면, Hold로부터 꺼내온다.
            {
                var holdTetrimino = GetHoldTetriminoComponent();
                if (holdTetrimino != null)
                {
                    // Hold 상태의 Tetrimino를 Current로 변경
                    holdTetrimino.State = BoardTetriminoState.Current;
                    holdTetrimino.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                    holdTetrimino.Rotation = 0; // 초기 회전 상태
                }
                else // 그런데 Hold조차 비었다면, 로그만 남기고 종료
                {
                    Debug.LogWarning("Queue도 비었으며 Hold Tetrimino 또한 비어 있습니다. 새로운 Tetrimino를 생성할 수 없습니다.");
                    return;
                }
            }
        }
    }
}