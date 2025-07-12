using System.Collections.Generic;
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
        }
    }
}