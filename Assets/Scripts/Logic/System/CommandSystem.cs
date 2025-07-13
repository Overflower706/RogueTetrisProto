using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class CommandSystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var queueEntities = context.GetEntitiesWithComponent<CommandRequestComponent>();
            if (queueEntities.Count == 0)
            {
                Debug.LogWarning("CommandQueueComponent가 있는 엔티티가 없습니다.");
                return; // 명령 큐가 없으면 아무것도 하지 않음
            }
            else if (queueEntities.Count > 1)
            {
                Debug.LogWarning("CommandQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return; // 여러 엔티티가 있으면 경고 후 종료
            }

            var commandQueue = queueEntities[0].GetComponent<CommandRequestComponent>();

            // 한 틱당 하나의 명령만 처리 (순서 보장)
            if (commandQueue.Requests.Count > 0)
            {
                var request = commandQueue.Requests.Dequeue();

                // 명령을 엔티티로 생성하여 다른 시스템들이 소비할 수 있도록 함
                var commandEntity = context.CreateEntity();

                // 명령 타입에 따라 적절한 컴포넌트 추가
                AddCommandComponent(request, commandEntity);
            }
        }

        public void Cleanup(Context context)
        {
            var commandEntities = context.GetEntitiesWithComponent<CommandMarkerComponent>();

            foreach (var entity in commandEntities)
            {
                context.DestroyEntity(entity);
            }
        }

        private void AddCommandComponent(CommandRequest request, Entity entity)
        {
            switch (request.Type)
            {
                case CommandType.StartGame:
                    var startCommand = new StartGameCommand();
                    startCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(startCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.EndGame:
                    var endCommand = new EndGameCommand();
                    endCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(endCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.MoveLeft:
                    var moveLeftCommand = new MoveLeftCommand();
                    moveLeftCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(moveLeftCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.MoveRight:
                    var moveRightCommand = new MoveRightCommand();
                    moveRightCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(moveRightCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.SoftDrop:
                    var softDropCommand = new SoftDropCommand();
                    softDropCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(softDropCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.HardDrop:
                    var hardDropCommand = new HardDropCommand();
                    hardDropCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(hardDropCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.RotateClockwise:
                    var rotateClockwiseCommand = new RotateClockwiseCommand();
                    rotateClockwiseCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(rotateClockwiseCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.RotateCounterClockwise:
                    var rotateCounterClockwiseCommand = new RotateCounterClockwiseCommand();
                    rotateCounterClockwiseCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(rotateCounterClockwiseCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.Hold:
                    var holdCommand = new HoldTetriminoCommand();
                    holdCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(holdCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.GenerateTetrimino:
                    var generateCommand = new GenerateTetriminoCommand();
                    generateCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(generateCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                case CommandType.LineClear:
                    var lineClearCommand = new LineClearCommand();
                    // PayLoad에서 줄 클리어 정보 추출
                    if (request.PayLoad is (int linesCleared, int tetriminoEntityId, TetriminoColor[][] completedLinesColors))
                    {
                        lineClearCommand.LinesCleared = linesCleared;
                        lineClearCommand.TetriminoEntityId = tetriminoEntityId;
                        lineClearCommand.CompletedLinesColors = completedLinesColors;
                    }
                    entity.AddComponent(lineClearCommand);
                    entity.AddComponent<CommandMarkerComponent>();
                    break;

                // 향후 다른 명령 타입들 추가
                default:
                    Debug.LogWarning($"알 수 없는 명령 타입: {request.Type}");
                    break;
            }
        }
    }
}
