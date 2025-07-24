using OVFL.ECS;

namespace Minomino
{
    public interface ICommand : IComponent
    {
        object PayLoad { get; }
    }
}