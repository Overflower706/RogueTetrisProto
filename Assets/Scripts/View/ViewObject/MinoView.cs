using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [SerializeField] private Image Image_Background;
        [SerializeField] private List<Sprite> Images;

        public void Refresh(MinoComponent minoComponent = null)
        {
            if (minoComponent == null)
            {
                Image_Background.sprite = null;
                Image_Background.color = new Color(0, 0, 0, 0); // 투명하게 설정
                return;
            }

            Image_Background.color = new Color(1, 1, 1, 1); // 불투명하게 설정
            GetSpriteByState(minoComponent.State);
        }

        private void GetSpriteByState(MinoState state)
        {
            switch (state)
            {
                case MinoState.None:
                    Debug.LogWarning($"None state MinoComponent");
                    break;
                case MinoState.Empty:
                    Image_Background.sprite = Images[0];
                    break;
                case MinoState.Living:
                    int randomIndex = Random.Range(1, 3);
                    Image_Background.sprite = Images[randomIndex];
                    break;
                default:
                    Debug.LogWarning($"Unknown MinoState: {state}");
                    break;
            }
        }
    }
}