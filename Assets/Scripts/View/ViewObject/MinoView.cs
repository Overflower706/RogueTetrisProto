using OVFL.ECS;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [SerializeField] private Image Image_Minue;
        [SerializeField] private Image Image_Background;
        private MinoComponent _minoComponent;

        public void Refresh(MinoComponent minoComponent = null)
        {
            if (minoComponent == null)
            {
                _minoComponent = null;
                Image_Background.sprite = null;
                Image_Background.color = new Color(0, 0, 0, 0); // 투명하게 설정
                return;
            }

            // 새로운 엔티티가 설정되면 업데이트
            _minoComponent = minoComponent;
            Image_Background.color = new Color(1, 1, 1, 1); // 불투명하게 설정
            GetSpriteByState(_minoComponent.State);
        }

        private void GetSpriteByState(MinoState state)
        {
            switch (state)
            {
                case MinoState.None:
                    Debug.LogWarning($"None state MinoComponent");
                    break;
                case MinoState.Empty:
                    Image_Background.sprite = SpriteSettings.Instance.Sprites_Empty[0];
                    break;
                case MinoState.Living:
                    Image_Background.sprite = SpriteSettings.Instance.Sprites_Living[0];
                    break;
                default:
                    Debug.LogWarning($"Unknown MinoState: {state}");
                    break;
            }
        }
    }
}