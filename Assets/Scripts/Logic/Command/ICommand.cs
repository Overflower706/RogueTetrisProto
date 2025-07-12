using OVFL.ECS;

namespace Minomino
{
    public interface ICommand : IComponent
    {
        CommandType Type { get; }
        object PayLoad { get; }
    }
}