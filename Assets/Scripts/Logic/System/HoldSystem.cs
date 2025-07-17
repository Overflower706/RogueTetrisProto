using System.Collections.Generic;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class HoldSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                var holdQueue = GetHoldQueue();
                holdQueue.HoldQueue = new Queue<Entity>();
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                var holdQueue = GetHoldQueue();
                holdQueue.HoldQueue.Clear();
                Debug.Log("게임 종료: HoldQueue를 초기화했습니다.");
                return;
            }

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
            // 만일 HoldQueue.Count도 0이고, TetriminoQueue.Count도 0이라면,
            // 유일한 Tetrimino이므로 Hold를 하지 않고, 그냥 Return

            var tetriminoQueue = GetTetriminoQueue();
            var holdQueue = GetHoldQueue();
            if (tetriminoQueue.TetriminoQueue.Count == 0 && holdQueue.HoldQueue.Count == 0)
            {
                Debug.Log("유일한 테트리미노이므로 Hold를 하지 않습니다.");
                return;
            }

            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();
            Entity currentTetrimino = null;

            foreach (var entity in tetriminoEntities)
            {
                var TetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (TetriminoComponent.State == BoardTetriminoState.Current)
                {
                    currentTetrimino = entity;
                    break;
                }
            }

            // holdQueue가 GlobalSettings.Instance.HoldSize보다 작으면,
            // 일단 Current를 HoldQueue에 Enqueue. 그리고 초기 상태로 설정

            if (holdQueue.HoldQueue.Count < GlobalSettings.Instance.HoldSize)
            {
                holdQueue.HoldQueue.Enqueue(currentTetrimino);
                currentTetrimino.GetComponent<BoardTetriminoComponent>().State = BoardTetriminoState.Hold;
                currentTetrimino.GetComponent<BoardTetriminoComponent>().Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                currentTetrimino.GetComponent<BoardTetriminoComponent>().Rotation = 0; // 초기 회전 상태

                Debug.Log("HoldQueue에 Tetrimino를 추가했습니다. 현재 HoldQueue 크기: " + holdQueue.HoldQueue.Count);
            }
            else if (holdQueue.HoldQueue.Count == GlobalSettings.Instance.HoldSize)
            {
                // holdQueue가 GlobalSettings.Instance.HoldSize보다 같거나 크다면,
                // Current를 HoldQueue에 Enqueue하고, HoldQueue의 가장 오래된 요소를 Dequeue해서 Current로 State 변경

                holdQueue.HoldQueue.Enqueue(currentTetrimino);
                currentTetrimino.GetComponent<BoardTetriminoComponent>().State = BoardTetriminoState.Hold;
                currentTetrimino.GetComponent<BoardTetriminoComponent>().Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                currentTetrimino.GetComponent<BoardTetriminoComponent>().Rotation = 0; // 초기 회전 상태

                var oldestHoldEntity = holdQueue.HoldQueue.Dequeue();
                oldestHoldEntity.GetComponent<BoardTetriminoComponent>().State = BoardTetriminoState.Current;
                oldestHoldEntity.GetComponent<BoardTetriminoComponent>().Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
                oldestHoldEntity.GetComponent<BoardTetriminoComponent>().Rotation = 0; // 초기 회전 상태

                Debug.Log("HoldQueue에 Tetrimino를 추가하고, 가장 오래된 Hold를 Current로 변경했습니다. 현재 HoldQueue 크기: " + holdQueue.HoldQueue.Count);
            }
            else
            {
                Debug.LogError("HoldQueue의 크기가 설정 값보다 큽니다. HoldQueue.Count: " + holdQueue.HoldQueue.Count);
                return;
            }

            // var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();
            // BoardTetriminoComponent holdTetriminoComponent = null;

            // foreach (var entity in tetriminoEntities)
            // {
            //     var tetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
            //     if (tetriminoComponent.State == BoardTetriminoState.Hold)
            //     {
            //         holdTetriminoComponent = tetriminoComponent;
            //         break;
            //     }
            // }

            // BoardTetriminoComponent currentTetriminoComponent = null;

            // foreach (var entity in tetriminoEntities)
            // {
            //     var TetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
            //     if (TetriminoComponent.State == BoardTetriminoState.Current)
            //     {
            //         currentTetriminoComponent = TetriminoComponent;
            //         break;
            //     }
            // }

            // if (holdTetriminoComponent == null)
            // {
            //     var tetriminoQueue = GetTetriminoQueue();
            //     if (tetriminoQueue.TetriminoQueue.Count == 0)
            //     {
            //         Debug.Log("유일한 테트리미노이므로 Hold를 하지 않습니다.");
            //         return;
            //     }
            //     else
            //     {
            //         currentTetriminoComponent.State = BoardTetriminoState.Hold;
            //     }
            // }
            // else
            // {
            //     holdTetriminoComponent.State = BoardTetriminoState.Current;
            //     holdTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);
            //     holdTetriminoComponent.Rotation = 0; // 초기 회전 상태
            //     currentTetriminoComponent.State = BoardTetriminoState.Hold;
            // }
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

        private HoldQueueComponent GetHoldQueue()
        {
            var holdQueueEntities = Context.GetEntitiesWithComponent<HoldQueueComponent>();
            if (holdQueueEntities.Count == 0)
            {
                Debug.LogWarning("HoldQueueComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (holdQueueEntities.Count > 1)
            {
                Debug.LogWarning("HoldQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return holdQueueEntities[0].GetComponent<HoldQueueComponent>();
        }
    }
}