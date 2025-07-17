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
            Systems.AddSystem<DataSystem>();
            Systems.AddSystem<CommandSystem>();

            Systems.AddSystem<PlayerSystem>();
            Systems.AddSystem<GameStateSystem>();
            Systems.AddSystem<ScoreSystem>();

            Systems.AddSystem<TetriminoSystem>();

            Systems.AddSystem<CurrentTetriminoSystem>();

            Systems.AddSystem<BoardSystem>(); // Board측에서 Hold에 대해 지우고나서 HoldSystem이 BoardTetriminoState를 바꿔야함
            Systems.AddSystem<HoldSystem>();


            Systems.AddSystem<ExampleInputSystem>();

            // View 관련

            var panelSceneManager = FindFirstObjectByType<PanelSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(panelSceneManager);

            var titleSceneManager = FindFirstObjectByType<TitleSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(titleSceneManager);
            var gameSceneManager = FindFirstObjectByType<GameSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(gameSceneManager);
            var packSceneManager = FindFirstObjectByType<PackSceneManager>(FindObjectsInactive.Include);
            Systems.AddSystem(packSceneManager);

            var titleCanvasManager = FindFirstObjectByType<TitleCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(titleCanvasManager);

            var bannerCanvasManager = FindFirstObjectByType<BannerCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(bannerCanvasManager);
            var gameCanvasManager = FindFirstObjectByType<GameCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(gameCanvasManager);
            var shopCanvasManager = FindFirstObjectByType<ShopCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(shopCanvasManager);
            var stageCanvasManager = FindFirstObjectByType<StageCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(stageCanvasManager);

            var packCanvasManager = FindFirstObjectByType<PackCanvasManager>(FindObjectsInactive.Include);
            Systems.AddSystem(packCanvasManager);

            var ShopPanelManager = FindFirstObjectByType<ShopPanelManager>(FindObjectsInactive.Include);
            Systems.AddSystem(ShopPanelManager);
        }
    }
}