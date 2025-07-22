using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [SerializeField] private Image Image_Background;

        public void Refresh(MinoState color)
        {
            Image_Background.color = GetColorByState(color);
        }

        private Color GetColorByState(MinoState state)
        {
            return state switch
            {
                MinoState.None => new Color(0.9f, 0.9f, 0.9f),     // 밝은 회색
                MinoState.Empty => new Color(0.2f, 0.2f, 0.2f),    // 회색
                MinoState.Living => new Color(0.2f, 0.8f, 0.2f),   // 초록색
                _ => Color.white                                   // 기본색
            };
        }
    }
}