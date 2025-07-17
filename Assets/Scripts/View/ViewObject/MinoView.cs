using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [SerializeField] private Image Image_Background;

        public void Refresh(TetriminoColor color)
        {
            Image_Background.color = GetColorFromEnum(color);
        }

        private Color GetColorFromEnum(TetriminoColor tetriminoColor)
        {
            return tetriminoColor switch
            {
                TetriminoColor.Red => new Color(0.8f, 0.2f, 0.2f),      // 빨간색
                TetriminoColor.Green => new Color(0.2f, 0.8f, 0.2f),    // 초록색
                TetriminoColor.Blue => new Color(0.2f, 0.2f, 0.8f),     // 파란색
                TetriminoColor.Yellow => new Color(0.8f, 0.8f, 0.2f),   // 노란색
                _ => Color.white                                        // 기본색
            };
        }
    }
}