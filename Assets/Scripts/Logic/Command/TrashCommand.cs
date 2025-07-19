namespace Minomino
{
    public class TrashCommand : ICommand
    {
        public int Index;

        public object PayLoad { get; set; }
    }
}