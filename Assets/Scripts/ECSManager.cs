using UnityEngine;
using OVFL.ECS;
using Minomino;

namespace Minos
{
    public class ECSManager : MonoBehaviour
    {
        public Context Context;
        public Systems Systems;

        private void Start()
        {
            Initialize();

            Systems.Setup(Context);
        }

        private void Update()
        {
            Systems.Tick(Context);
            Systems.Cleanup(Context);
        }

        private void OnDestroy()
        {
            Systems.Teardown(Context);
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
            Systems.AddSystem<GameStateSystem>();
            Systems.AddSystem<PlayerSystem>();
            Systems.AddSystem<ScoreSystem>();
            Systems.AddSystem<TetriminoSystem>();
            Systems.AddSystem<BoardSystem>();

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
        }
    }
}