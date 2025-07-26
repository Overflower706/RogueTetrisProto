using UnityEngine;
using OVFL.ECS;

namespace Minomino
{
    public class ECSRunner : MonoBehaviour
    {
        public Context Context;
        public Systems Systems;

        private void Start()
        {
            Initialize();

            Systems.Setup();
        }

        private void Update()
        {
            Systems.Tick();
            Systems.Cleanup();
        }

        private void OnDestroy()
        {
            Systems.Teardown();
        }

        private void Initialize()
        {
            Context = new Context();
            Systems = new Systems();

            // Context를 Systems에 설정
            Systems.SetContext(Context);

            // 순서 중요
            Systems.AddSystem<UniqueComponentSystem>();
            Systems.AddSystem<CommandSystem>();

            Systems.AddSystem<PlayerSystem>();
            Systems.AddSystem<GameStateSystem>();

            Systems.AddSystem<TetrominoSystem>();

            Systems.AddSystem<CurrentTetriminoSystem>();

            Systems.AddSystem<BoardSystem>(); // Board측에서 Hold에 대해 지우고나서 HoldSystem이 BoardTetriminoState를 바꿔야함
            Systems.AddSystem<HoldSystem>();
            Systems.AddSystem<MinoSystem>(); // GameState의 게임 오버 검증 이후 MinoSystem 업데이트

            Systems.AddSystem<ScoreSystem>();

            Systems.AddSystem<NotifySystem>();
            Systems.AddSystem<ExampleInputSystem>();

            // View 관련

            var panelSceneManager = FindFirstObjectByType<PanelSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(panelSceneManager);

            var titleSceneManager = FindFirstObjectByType<TitleSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(titleSceneManager);
            var packSceneManager = FindFirstObjectByType<PackSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(packSceneManager);
            var gameExSceneManager = FindFirstObjectByType<GameExSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(gameExSceneManager);

            var titleCanvasManager = FindFirstObjectByType<TitleCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(titleCanvasManager);

            var gameExCanvasManager = FindFirstObjectByType<GameExCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(gameExCanvasManager);

            var apartmentPanelManager = FindFirstObjectByType<ApartmentPanelManager>(FindObjectsInactive.Include);
            Systems.AddSystem(apartmentPanelManager);

            var holdPanelManager = FindFirstObjectByType<HoldPanelManager>(FindObjectsInactive.Include);
            Systems.AddSystem(holdPanelManager);

            var nextQueuePanelManager = FindFirstObjectByType<NextQueuePanelManager>(FindObjectsInactive.Include);
            Systems.AddSystem(nextQueuePanelManager);

            var scorePanelManager = FindFirstObjectByType<ScorePanelManager>(FindObjectsInactive.Include);
            Systems.AddSystem(scorePanelManager);
        }
    }
}