using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoIconView : MonoBehaviour
    {
        [Header("미노 아이콘")]
        [SerializeField] private Image Image_MinoIcon;

        public void Refresh(MinoComponent mino = null)
        {
            if (mino == null)
            {
                Image_MinoIcon.color = Color.clear;
                return;
            }
            else
            {
                // #a8a678 -> RGB(168, 166, 120)
                Image_MinoIcon.color = new Color32(0xa8, 0xa6, 0x78, 0xFF);
            }
        }
    }
}