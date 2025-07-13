using OVFL.ECS;

namespace Minomino
{
    /// <summary>
    /// 줄 클리어 이벤트를 ScoreSystem에 전달하기 위한 Command 컴포넌트
    /// </summary>
    public class LineClearCommand : IComponent
    {
        /// <summary>
        /// 클리어된 줄의 개수
        /// </summary>
        public int LinesCleared;

        /// <summary>
        /// 줄을 클리어한 테트리미노의 Entity ID (점수 계산에 사용)
        /// </summary>
        public int TetriminoEntityId;
    }
}
