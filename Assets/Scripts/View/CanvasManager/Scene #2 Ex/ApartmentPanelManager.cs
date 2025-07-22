using OVFL.ECS;
using TMPro;
using UnityEngine;

namespace Minomino
{
    public class ApartmentPanelManager : MonoBehaviour, ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        [SerializeField] private TMP_Text Text_Stage;

        public void Setup()
        {
            Text_Stage.gameObject.SetActive(false);
        }

        public void Show()
        {

        }

        public void Tick()
        {
            var start = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (start.Count > 0)
            {
                var player = Context.GetPlayer();
                Text_Stage.text = $"{player.Round}-{player.Stage}\n단지\n예정 부지";
                Text_Stage.gameObject.SetActive(true);
            }

            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;
        }

        public void Hide()
        {
            Text_Stage.gameObject.SetActive(false);
        }
    }
}