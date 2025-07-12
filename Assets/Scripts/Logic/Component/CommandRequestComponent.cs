using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class CommandRequestComponent : IComponent
    {
        public Queue<CommandRequest> Requests = new Queue<CommandRequest>();
    }

    public struct CommandRequest
    {
        public CommandType Type;
        public object PayLoad;
    }

    public enum CommandType : ushort
    {
        StartGame = 1,
        // 추후 다른 메시지들 추가 예정
        // MoveLeft = 2,
        // MoveRight = 3,
        // Rotate = 4,
    }
}