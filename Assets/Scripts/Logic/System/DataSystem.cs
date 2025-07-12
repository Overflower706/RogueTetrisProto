using OVFL.ECS;

namespace Minomino
{
    public class DataSystem : ISetupSystem
    {
        public Context Context { get; set; }
        public void Setup(Context context)
        {
            var gameEntity = context.CreateEntity();
            gameEntity.AddComponent<GameStateComponent>();
            gameEntity.AddComponent<ScoreComponent>();
            gameEntity.AddComponent<BoardComponent>();

            var playerEntiy = context.CreateEntity();
            playerEntiy.AddComponent<PlayerComponent>();

            var commandReuqestEntty = Context.CreateEntity();
            commandReuqestEntty.AddComponent<CommandRequestComponent>();
        }
    }
}