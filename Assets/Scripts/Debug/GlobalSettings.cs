namespace Minomino
{
    public class GlobalSettings : MonoSingleton<GlobalSettings>
    {
        public int BoardWidth = 10;
        public int BoardHeight = 20;
        public int HoldSize = 1;
        public int RoundBonus = 10;
        public int StageBonus = 100;
        public int CurrencyBase = 100;
        public int CurrencyBonusPerDeck = 1;
        public int BaseBakeCount = 3;
        public int BaseTrashCount = 3;
    }
}