using OVFL.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Minomino
{
    public class ApartmentPanelManager : MonoBehaviour, ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        [SerializeField] private TMP_Text Text_Stage;
        [SerializeField] private TMP_Text Text_Limit;
        [SerializeField] private RectTransform RectTransform_ApartmentPanel;
        [SerializeField] private GridLayoutGroup GridLayout_Layout;
        [SerializeField] private GameObject Prefab_Plot;
        [SerializeField] private GridLayoutGroup GridLayout_Board;
        [SerializeField] private MinoView Prefab_Mino;
        [SerializeField] private RectTransform RectTransform_Reward;
        [SerializeField] private GridLayoutGroup GridLayout_Reward;
        [SerializeField] private RewardView Prefab_Reward;

        private MinoView[,] minoViews;
        private List<RewardView> rewardViews;

        public void Setup()
        {
            Text_Stage.gameObject.SetActive(false);
            Text_Limit.gameObject.SetActive(false);
        }

        public void Show()
        {

        }

        public void Tick()
        {
            if (!gameObject.activeSelf) return;

            var start = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (start.Count > 0)
            {
                var player = Context.GetPlayer();
                Text_Stage.text = $"{player.Round}-{player.Stage}\n단지\n부지";
                Text_Stage.gameObject.SetActive(true);
                Text_Limit.gameObject.SetActive(true);

                RectTransform_ApartmentPanel.sizeDelta = new Vector2(GlobalSettings.Instance.MinoWidth * GlobalSettings.Instance.SafeWidth + 10,
                                                                   GlobalSettings.Instance.MinoHeight + (GlobalSettings.Instance.MinoHeight - 5) * (GlobalSettings.Instance.SafeHeight - 1) + 10);

                RectTransform_Reward.sizeDelta = new Vector2(GlobalSettings.Instance.MinoWidth, GlobalSettings.Instance.MinoHeight * GlobalSettings.Instance.SafeHeight + 15);
                GridLayout_Reward.cellSize = new Vector2(GlobalSettings.Instance.MinoWidth, GlobalSettings.Instance.MinoHeight);
                rewardViews = new List<RewardView>();
                for (int i = 0; i < GlobalSettings.Instance.SafeHeight; i++)
                {
                    var rewardView = Instantiate(Prefab_Reward, RectTransform_Reward.transform);
                    rewardView.name = $"RewardView_{i}";
                    rewardViews.Add(rewardView);
                    rewardView.SetTransparency(true);
                }

                GridLayout_Layout.cellSize = new Vector2(GlobalSettings.Instance.MinoWidth, GlobalSettings.Instance.MinoHeight);

                for (int h = 0; h < GlobalSettings.Instance.BoardHeight; h++)
                {
                    for (int w = 0; w < GlobalSettings.Instance.SafeWidth; w++)
                    {
                        Instantiate(Prefab_Plot, GridLayout_Layout.transform);
                    }
                }

                GridLayout_Board.cellSize = new Vector2(GlobalSettings.Instance.MinoWidth, GlobalSettings.Instance.MinoHeight);
                minoViews = new MinoView[GlobalSettings.Instance.SafeWidth, GlobalSettings.Instance.BoardHeight];


                for (int h = 0; h < GlobalSettings.Instance.BoardHeight; h++)
                {
                    for (int w = 0; w < GlobalSettings.Instance.SafeWidth; w++)
                    {
                        var minoView = Instantiate(Prefab_Mino, GridLayout_Board.transform);
                        minoView.name = $"MinoView_{w}_{h}";
                        minoViews[w, h] = minoView;
                    }

                    if (GlobalSettings.Instance.RewardLines.Contains(h))
                    {
                        // 보상 라인에 해당하는 경우
                        var rewardView = rewardViews[h];
                        rewardView.SetTransparency(false);
                    }
                }
            }

            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;

            var board = Context.GetBoard();

            for (int h = 0; h < GlobalSettings.Instance.BoardHeight; h++)
            {
                for (int w = 0; w < GlobalSettings.Instance.SafeWidth; w++)
                {
                    var minoID = board.Board[w, h];

                    if (minoID == 0)
                    {
                        minoViews[w, h].Refresh(null);
                    }
                    else
                    {
                        var minoView = minoViews[w, h];

                        var minoEntity = Context.FindEntityByID(minoID);
                        minoView.Refresh(minoEntity);
                    }
                }
            }

            var rewards = Context.GetEntitiesWithComponent<RewardComponent>();
            foreach (var rewardEntity in rewards)
            {
                var rewardComponent = rewardEntity.GetComponent<RewardComponent>();
                var rewardView = rewardViews[rewardComponent.Line];
                rewardView.Refresh(rewardComponent.IsReceived);
            }
        }

        public void Hide()
        {
            Text_Stage.gameObject.SetActive(false);
            Text_Limit.gameObject.SetActive(false);

            minoViews = null;

            // GridLayout_Board의 자식들 안전하게 제거
            var layoutChildCount = GridLayout_Board.transform.childCount;
            for (int i = layoutChildCount - 1; i >= 0; i--)
            {
                var child = GridLayout_Board.transform.GetChild(i);
                Destroy(child.gameObject);
            }

            // GridLayout_Layout의 자식들 안전하게 제거
            var minoChildCount = GridLayout_Layout.transform.childCount;
            for (int i = minoChildCount - 1; i >= 0; i--)
            {
                var child = GridLayout_Layout.transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var rewardChildCount = RectTransform_Reward.transform.childCount;
            for (int i = rewardChildCount - 1; i >= 0; i--)
            {
                var child = RectTransform_Reward.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            rewardViews.Clear();
        }
    }
}