using OVFL.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class ApartmentPanelManager : MonoBehaviour, ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        [SerializeField] private TMP_Text Text_Stage;
        [SerializeField] private RectTransform RectTransform_ApartmentPanel;
        [SerializeField] private GridLayoutGroup GridLayout_Layout;
        [SerializeField] private GameObject Prefab_Plot;

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
                Text_Stage.text = $"{player.Round}-{player.Stage}\n단지\n부지";
                Text_Stage.gameObject.SetActive(true);

                RectTransform_ApartmentPanel.sizeDelta = new Vector2(GlobalSettings.Instance.MinoWidth * GlobalSettings.Instance.BoardWidth + 10,
                                                                   Mathf.Max(1060, GlobalSettings.Instance.MinoHeight + (GlobalSettings.Instance.MinoHeight - 5) * (GlobalSettings.Instance.BoardHeight - 1) + 10));

                GridLayout_Layout.cellSize = new Vector2(GlobalSettings.Instance.MinoWidth, GlobalSettings.Instance.MinoHeight);

                for (int h = 0; h < GlobalSettings.Instance.BoardHeight; h++)
                {
                    for (int w = 0; w < GlobalSettings.Instance.BoardWidth; w++)
                    {
                        Instantiate(Prefab_Plot, GridLayout_Layout.transform);
                    }
                }
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