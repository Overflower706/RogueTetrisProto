using OVFL.ECS;

namespace Minomino
{
    /// <summary>
    /// 게임의 점수 배율 및 레벨 정보를 담는 컴포넌트
    /// </summary>
    public class ScoreMultiplierComponent : IComponent
    {
        /// <summary>
        /// 현재 점수 배율
        /// </summary>
        public float ScoreMultiplier = 1.0f;

        /// <summary>
        /// 현재 레벨
        /// </summary>
        public int Level = 1;

        /// <summary>
        /// 연속 줄 클리어 카운트 (콤보)
        /// </summary>
        public int ComboCount = 0;

        /// <summary>
        /// 콤보 배율
        /// </summary>
        public float ComboMultiplier = 1.0f;
    }
}
