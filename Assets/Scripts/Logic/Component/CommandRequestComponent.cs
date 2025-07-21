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
        EndGame = 2,
        // 테트리미노 이동 및 조작 명령
        MoveLeft = 3,
        MoveRight = 4,
        SoftDrop = 5,
        HardDrop = 6,
        RotateClockwise = 7,        // 시계 방향 회전 (X키)
        RotateCounterClockwise = 8, // 반시계 방향 회전 (Z키)
        Hold = 9,                   // 홀드 (방향키 위)
        GenerateTetrimino = 10, // 테트리미노 생성
        LineClear = 11,
    }
}