using System;
using OVFL.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class RewardPanelManager : MonoBehaviour, ISetupSystem
    {
        public Context Context { get; set; }


        [Header("선택지")]
        [SerializeField] private Button Button_Choice_1;
        [SerializeField] private TMP_Text Text_Choice_1;
        [SerializeField] private Button Button_Choice_2;
        [SerializeField] private TMP_Text Text_Choice_2;
        [SerializeField] private Button Button_Choice_3;
        [SerializeField] private TMP_Text Text_Choice_3;

        [Header("패스")]
        [SerializeField] private Button Button_Close;

        public void Setup()
        {
            Button_Choice_1.interactable = false;
            Button_Choice_2.interactable = false;
            Button_Choice_3.interactable = false;

            Button_Close.onClick.AddListener(OnCloseButtonClicked);
            Button_Close.interactable = false;
        }

        public void Show(RewardComponent reward)
        {
            gameObject.SetActive(true);
            Button_Choice_1.interactable = true;
            Button_Choice_2.interactable = true;
            Button_Choice_3.interactable = true;
            Button_Close.interactable = true;

            Refresh(reward);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Button_Choice_1.interactable = false;
            Button_Choice_2.interactable = false;
            Button_Choice_3.interactable = false;
            Button_Close.interactable = true;
        }

        public void Refresh(RewardComponent reward)
        {
            for (int i = 0; i < reward.Rewards.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        Text_Choice_1.text = reward.Rewards[i];
                        Button_Choice_1.onClick.RemoveAllListeners();
                        Button_Choice_1.onClick.AddListener(() => OnChoiceButtonClicked(reward.Line, 1));
                        break;
                    case 1:
                        Text_Choice_2.text = reward.Rewards[i];
                        Button_Choice_2.onClick.RemoveAllListeners();
                        Button_Choice_2.onClick.AddListener(() => OnChoiceButtonClicked(reward.Line, 2));
                        break;
                    case 2:
                        Text_Choice_3.text = reward.Rewards[i];
                        Button_Choice_3.onClick.RemoveAllListeners();
                        Button_Choice_3.onClick.AddListener(() => OnChoiceButtonClicked(reward.Line, 3));
                        break;
                }
            }
        }

        private void OnChoiceButtonClicked(int line, int choiceIndex)
        {
            Button_Choice_1.interactable = false;
            Button_Choice_2.interactable = false;
            Button_Choice_3.interactable = false;
            Button_Close.interactable = false;

            var commandQueue = Context.GetCommandRequest();
            commandQueue.Requests.Enqueue(new CommandRequest
            {
                Type = CommandType.ChooseReward,
                PayLoad = Tuple.Create(line, choiceIndex)
            });

            commandQueue.Requests.Enqueue(new CommandRequest
            {
                Type = CommandType.EndReward,
                PayLoad = null
            });

            Hide();
        }

        private void OnCloseButtonClicked()
        {
            Button_Choice_1.interactable = false;
            Button_Choice_2.interactable = false;
            Button_Choice_3.interactable = false;
            Button_Close.interactable = false;

            var commandQueue = Context.GetCommandRequest();
            commandQueue.Requests.Enqueue(new CommandRequest
            {
                Type = CommandType.EndReward,
                PayLoad = null
            });

            Hide();
        }
    }
}