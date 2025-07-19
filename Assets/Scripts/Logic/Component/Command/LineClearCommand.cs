using OVFL.ECS;

namespace Minomino
{
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

        /// <summary>
        /// 완성된 줄들의 색상 정보 (점수 계산용)
        /// [줄 인덱스][x 좌표] = 색상
        /// </summary>
        public TetriminoColor[][] CompletedLinesColors;
    }
}
