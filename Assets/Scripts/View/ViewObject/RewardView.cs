using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class RewardView : MonoBehaviour
    {
        [Header("이미지")]
        public Image Image_Reward;

        public void SetTransparency(bool isTransparent)
        {
            if (Image_Reward == null) return;

            Color color = Image_Reward.color;
            color.a = isTransparent ? 0f : 1f; // 투명도 설정
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
    }
}