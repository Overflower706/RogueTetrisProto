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
            Systems.AddSystem<GameStateSystem>();
            Systems.AddSystem<CommandSystem>();

            // View 관련

            var panelSceneManager = FindFirstObjectByType<PanelSceneManager>();
            Systems.AddSystem(panelSceneManager);

            var titleSceneManager = FindFirstObjectByType<TitleSceneManager>();
            Systems.AddSystem(titleSceneManager);
            var gameSceneManager = FindFirstObjectByType<GameSceneManager>();
            Systems.AddSystem(gameSceneManager);
            var packSceneManager = FindFirstObjectByType<PackSceneManager>();
            Systems.AddSystem(packSceneManager);

            var titleCanvasManager = FindFirstObjectByType<TitleCanvasManager>();
            Systems.AddSystem(titleCanvasManager);

            var bannerCanvasManager = FindFirstObjectByType<BannerCanvasManager>();
            Systems.AddSystem(bannerCanvasManager);
            var gameCanvasManager = FindFirstObjectByType<GameCanvasManager>();
            Systems.AddSystem(gameCanvasManager);
            var shopCanvasManager = FindFirstObjectByType<ShopCanvasManager>();
            Systems.AddSystem(shopCanvasManager);
            var stageCanvasManager = FindFirstObjectByType<StageCanvasManager>();
            Systems.AddSystem(stageCanvasManager);

            var packCanvasManager = FindFirstObjectByType<PackCanvasManager>();
            Systems.AddSystem(packCanvasManager);
        }
    }
}