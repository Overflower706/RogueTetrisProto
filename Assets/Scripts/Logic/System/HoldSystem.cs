using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class HoldSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var state = GetState();

            if (state.CurrentState != GameState.Playing)
            {
                return;
            }

            // Hold 명령을 감지
            var holdCommandEntities = Context.GetEntitiesWithComponent<HoldTetriminoCommand>();
            if (holdCommandEntities.Count > 0)
            {
                ProcessHoldCommand();
            }
        }

        /// <summary>
        /// Hold 명령 처리
        /// </summary>
        private void ProcessHoldCommand()
        {
            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();
            BoardTetriminoComponent holdTetriminoComponent = null;

            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (tetriminoComponent.State == BoardTetriminoState.Hold)
                {
                    holdTetriminoComponent = tetriminoComponent;
                    break;
                }
            }

            BoardTetriminoComponent currentTetriminoComponent = null;

            foreach (var entity in tetriminoEntities)
            {
                var TetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (TetriminoComponent.State == BoardTetriminoState.Current)
                {
                    currentTetriminoComponent = TetriminoComponent;
                    break;
                }
            }

            if (holdTetriminoComponent == null)
            {
                var tetriminoQueue = GetTetriminoQueue();
                if (tetriminoQueue.TetriminoQueue.Count == 0)
                {
                    Debug.Log("유일한 테트리미노이므로 Hold를 하지 않습니다.");
                    return;
                }
                else
                {
                    currentTetriminoComponent.State = BoardTetriminoState.Hold;
                }
            }
            else
            {
                holdTetriminoComponent.State = BoardTetriminoState.Current;
                holdTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                holdTetriminoComponent.Rotation = 0; // 초기 회전 상태
                currentTetriminoComponent.State = BoardTetriminoState.Hold;
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