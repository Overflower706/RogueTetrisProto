using UnityEngine;

namespace Minomino
{
    public class GlobalSettings : MonoSingleton<GlobalSettings>
    {
        [Header("보드")]
        public int BoardWidth = 10;
        public int BoardHeight = 20;

        [Header("홀드")]
        public int HoldSize = 1;

        [Header("목표 점수")]
        public int RoundBonus = 10;
        public int StageBonus = 100;

        [Header("보상")]
        public int CurrencyBase = 100;
        public int CurrencyBonusPerDeck = 1;

        [Header("Player Component를 확인하세요")]
        public int BaseBakeCount = 3;
        public int BaseTrashCount = 3;

        [Header("점수")]
        public int CookieScore = 10;
        public int BreadScore = 50;
        public int IcingCookieScore = 50;
        public int ChocoSoraBreadScore = 100;

        [Header("UI Sprite")]
        public Sprite[] tetriminoSprites;
    }
}