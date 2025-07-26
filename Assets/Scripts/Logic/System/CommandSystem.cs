using System;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class CommandSystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var commandQueue = Context.GetCommandRequest();

            // 한 틱당 하나의 명령만 처리 (순서 보장)
            if (commandQueue.Requests.Count > 0)
            {
                var request = commandQueue.Requests.Dequeue();

                // 명령을 엔티티로 생성하여 다른 시스템들이 소비할 수 있도록 함
                var commandEntity = Context.CreateEntity();

                // 명령 타입에 따라 적절한 컴포넌트 추가
                commandEntity.AddComponent<CommandMarkerComponent>();
                AddCommandComponent(request, commandEntity);
            }
        }

        public void Cleanup()
        {
            var commandEntities = Context.GetEntitiesWithComponent<CommandMarkerComponent>();

            foreach (var entity in commandEntities)
            {
                Context.DestroyEntity(entity);
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
                    break;

                case CommandType.EndGame:
                    var endCommand = new EndGameCommand();
                    endCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(endCommand);
                    break;

                case CommandType.MoveLeft:
                    var moveLeftCommand = new MoveLeftCommand();
                    moveLeftCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(moveLeftCommand);
                    break;

                case CommandType.MoveRight:
                    var moveRightCommand = new MoveRightCommand();
                    moveRightCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(moveRightCommand);
                    break;

                case CommandType.SoftDrop:
                    var softDropCommand = new SoftDropCommand();
                    softDropCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(softDropCommand);
                    break;

                case CommandType.HardDrop:
                    var hardDropCommand = new HardDropCommand();
                    hardDropCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(hardDropCommand);
                    break;

                case CommandType.RotateClockwise:
                    var rotateClockwiseCommand = new RotateClockwiseCommand();
                    rotateClockwiseCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(rotateClockwiseCommand);
                    break;

                case CommandType.RotateCounterClockwise:
                    var rotateCounterClockwiseCommand = new RotateCounterClockwiseCommand();
                    rotateCounterClockwiseCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(rotateCounterClockwiseCommand);
                    break;

                case CommandType.Hold:
                    var holdCommand = new HoldTetriminoCommand();
                    holdCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(holdCommand);
                    break;

                case CommandType.GenerateTetrimino:
                    var generateCommand = new GenerateTetrominoCommand();
                    generateCommand.PayLoad = request.PayLoad;
                    entity.AddComponent(generateCommand);
                    break;

                // 향후 다른 명령 타입들 추가
                default:
                    Debug.LogWarning($"알 수 없는 명령 타입: {request.Type}");
                    break;
            }
        }
    }
}
