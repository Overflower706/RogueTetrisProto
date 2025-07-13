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
        // 테트리미노 이동 및 조작 명령
        MoveLeft = 2,
        MoveRight = 3,
        SoftDrop = 4,
        HardDrop = 5,
        RotateClockwise = 6,        // 시계 방향 회전 (X키)
        RotateCounterClockwise = 7, // 반시계 방향 회전 (Z키)
        Hold = 8,                   // 홀드 (방향키 위)
    }
}