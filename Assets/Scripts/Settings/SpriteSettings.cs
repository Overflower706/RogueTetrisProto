using UnityEngine;

namespace Minomino
{
    public class SpriteSettings : MonoSingleton<SpriteSettings>
    {
        [Header("게임 내 공통 색")]
        public Color WhiteCream = new Color32(0xFD, 0xFC, 0xEE, 0xFF); // #fdfcee
        public Color Butter = new Color32(0xFC, 0xFB, 0xDC, 0xFF); // #fcfbdc
        public Color Beige = new Color32(0xEE, 0xED, 0xC0, 0xFF); // #eeedc0
        public Color MinueBody = new Color32(0xC7, 0xC5, 0x97, 0xFF); // #c7c597
        public Color Shadow = new Color32(0xA8, 0xA6, 0x78, 0xFF); // #a8a678
        public Color Dark = new Color32(0x6C, 0x6C, 0x51, 0xFF); // #6c6c51
        public Color Font = new Color32(0x43, 0x42, 0x34, 0xFF); // #434234

        [Header("간단한 미노 이미지들")]
        public Sprite[] Sprites_Empty;
        public Sprite[] Sprites_Living;
    }
}