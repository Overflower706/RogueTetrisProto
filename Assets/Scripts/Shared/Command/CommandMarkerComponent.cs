using OVFL.ECS;

namespace Minomino
{
    public class CommandMarkerComponent : IComponent
    {
        // 이 컴포넌트는 명령어를 표시하기 위한 마커 역할을 합니다.
        // 실제 명령어 데이터는 CommandRequestComponent에 저장됩니다.
        // 이 컴포넌트는 명령어가 처리되었음을 나타내기 위해 사용될 수 있습니다.
    }
}