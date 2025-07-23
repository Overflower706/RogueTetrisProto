using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class MinoSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                var minoEntities = Context.GetEntitiesWithComponent<MinoComponent>();
                foreach (var mino in minoEntities)
                {
                    var minoComponent = mino.GetComponent<MinoComponent>();
                    minoComponent.State = MinoState.Empty; // 초기 상태를 Empty로 설정
                }
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                var minoEntities = Context.GetEntitiesWithComponent<MinoComponent>();
                Debug.Log($"게임 종료 명령을 받았습니다. 모든 Mino 상태를 Empty로 설정합니다. 총 {minoEntities.Count}개");
                foreach (var mino in minoEntities)
                {
                    var minoComponent = mino.GetComponent<MinoComponent>();
                    minoComponent.State = MinoState.Empty; // 초기 상태를 Empty로 설정
                }
            }

            var completedLineEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            if (completedLineEntities.Count > 0)
            {
                var board = Context.GetBoard();
                var notifyQueue = Context.GetNotifyQueue();
                // board에서 해당 height에 해당하는 모든 int 값을 가져온다.
                // 해당 int 값은 Mino Entity의 ID이다.
                var completedLineComponent = completedLineEntities[0].GetComponent<CompletedLineComponent>();
                // 왼쪽에서부터 순서대로 가져와서, MinoComponent의 State를 Linving으로 바꾼다.
                foreach (var line in completedLineComponent.CompletedLine)
                {
                    for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
                    {
                        int entityID = board.Board[x, line];
                        if (entityID != 0)
                        {
                            var minoEntity = Context.FindEntityByID(entityID);
                            var minoComponent = minoEntity.GetComponent<MinoComponent>();
                            minoComponent.State = MinoState.Living;

                            // 이제 notify 보내기만 하면된다.
                            notifyQueue.Notifies.Enqueue(new Notify
                            {
                                Type = NotifyType.MinoStateChanged,
                                PayLoad = minoEntity.ID
                            });
                            Debug.Log($"높이 {line}의 Mino ID {entityID}의 상태를 Living으로 변경했습니다.");
                        }
                    }
                }
            }
        }
    }
}