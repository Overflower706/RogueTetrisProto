namespace Minomino
{
    public class StartGameCommand : ICommand
    {
        public CommandType Type { get; set; }
        public object PayLoad { get; set; }
    }
}