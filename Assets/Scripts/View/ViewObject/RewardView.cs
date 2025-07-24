using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class RewardView : MonoBehaviour
    {
        [Header("이미지")]
        [SerializeField] private Image Image_Reward;
        [SerializeField] private GameObject Object_Alert;
        private bool isActiveReward = false;

        public void SetTransparency(bool isTransparent)
        {
            if (Image_Reward == null) return;

            Color color = Image_Reward.color;
            color.a = isTransparent ? 0f : 1f; // 투명도 설정
            isActiveReward = !isTransparent; // 활성화 상태 업데이트
            Image_Reward.color = color;
        }

        public void Refresh(bool isReceived)
        {
            if (isReceived)
            {
                Image_Reward.sprite = GlobalSettings.Instance.Sprite_Reward_Received;
            }
            else
            {
                Image_Reward.sprite = GlobalSettings.Instance.Sprite_Reward_Alert;
            }
        }

        public void OnPointerEnter()
        {
            if (!isActiveReward) return;
            Object_Alert.SetActive(true);
        }

        public void OnPointerExit()
        {
            if (!isActiveReward) return;
            Object_Alert.SetActive(false);
        }
    }
}