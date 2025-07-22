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
            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;

            // 현재 테트리미노가 없으면 새로 생성
            var currentTetrimino = GetCurrentTetrimino();
            if (currentTetrimino == null)
            {
                GenerateTetrimino();
            }
        }

        private void GenerateTetrimino()
        {
            var queueEntity = Context.GetTetrominoQueue();

            if (queueEntity.TetrominoQueue.Count > 0)
            {
                var nextTetrimino = queueEntity.TetrominoQueue.Dequeue();
                var boardTetriminoComponent = nextTetrimino.AddComponent<BoardTetrominoComponent>();
                boardTetriminoComponent.State = BoardTetrominoState.Current;
                boardTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                boardTetriminoComponent.Rotation = 0; // 초기 회전 상태

                Debug.Log($"새로운 Current Tetrimino 생성: Type: {nextTetrimino.GetComponent<TetrominoComponent>().Type}, Position: {boardTetriminoComponent.Position}");
            }
            else // TetriminoQueue가 비었다면, Hold로부터 꺼내온다.
            {
                var holdQueue = Context.GetHoldQueue();
                if (holdQueue.HoldQueue.Count > 0)
                {
                    var holdTetrimino = holdQueue.HoldQueue.Dequeue();
                    var holdTetriminoComponent = holdTetrimino.GetComponent<BoardTetrominoComponent>();
                    // Hold 상태의 Tetrimino를 Current로 변경
                    holdTetriminoComponent.State = BoardTetrominoState.Current;
                    holdTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                    holdTetriminoComponent.Rotation = 0; // 초기 회전 상태

                    Debug.Log($"Hold에서 Current Tetrimino로 변경: Type: {holdTetrimino.GetComponent<TetrominoComponent>().Type}, Position: {holdTetriminoComponent.Position}");
                }
                else // 그런데 Hold조차 비었다면, 로그만 남기고 종료
                {
                    Debug.LogWarning("Queue도 비었으며 Hold Tetrimino 또한 비어 있습니다. 새로운 Tetrimino를 생성할 수 없습니다.");
                    return;
                }
            }
        }

        private BoardTetrominoComponent GetCurrentTetrimino()
        {
            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetrominoComponent>();

            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetrominoComponent>();
                if (tetriminoComponent.State == BoardTetrominoState.Current)
                {
                    return tetriminoComponent;
                }
            }
            return null;
        }
    }
}