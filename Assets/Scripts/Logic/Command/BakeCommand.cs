namespace Minomino
{
    public class BakeCommand : ICommand
    {
        public int Index;

        public object PayLoad { get; set; }
    }
}