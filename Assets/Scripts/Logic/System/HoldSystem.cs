using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class HoldSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            // Hold 명령을 감지
            var holdCommandEntities = context.GetEntitiesWithComponent<HoldTetriminoCommand>();
            if (holdCommandEntities.Count > 0)
            {
                ProcessHoldCommand(context);
            }
        }

        /// <summary>
        /// Hold 명령 처리
        /// </summary>
        private void ProcessHoldCommand(Context context)
        {
            // 현재 테트리미노 엔티티 찾기
            var currentTetriminoEntities = context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
            if (currentTetriminoEntities.Count == 0)
            {
                Debug.LogWarning("Hold: 현재 테트리미노가 없습니다.");
                return;
            }

            var currentTetriminoEntity = currentTetriminoEntities[0];
            var currentTetrimino = currentTetriminoEntity.GetComponent<CurrentTetriminoComponent>();
            var currentTetriminoComponent = currentTetriminoEntity.GetComponent<TetriminoComponent>();

            if (currentTetrimino == null || currentTetriminoComponent == null)
            {
                Debug.LogWarning("Hold: 현재 테트리미노 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            // 이미 홀드된 테트리미노가 있는지 확인
            var holdEntities = context.GetEntitiesWithComponent<HoldTetriminoComponent>();

            if (holdEntities.Count > 0)
            {
                // 이미 홀드된 테트리미노가 있는 경우 - 교체
                ProcessHoldSwap(context, currentTetriminoEntity, holdEntities[0]);
            }
            else
            {
                // 홀드된 테트리미노가 없는 경우 - 새로 홀드
                ProcessFirstHold(context, currentTetriminoEntity);
            }
        }

        /// <summary>
        /// 첫 번째 홀드 (홀드된 테트리미노가 없을 때)
        /// </summary>
        private void ProcessFirstHold(Context context, Entity currentTetriminoEntity)
        {
            Debug.Log("첫 번째 홀드 실행");

            // 현재 테트리미노를 홀드로 변경
            // CurrentTetriminoComponent 제거
            currentTetriminoEntity.RemoveComponent<CurrentTetriminoComponent>();

            // HoldTetriminoComponent 추가 (이미 존재하지 않는 경우에만)
            if (!currentTetriminoEntity.HasComponent<HoldTetriminoComponent>())
            {
                currentTetriminoEntity.AddComponent<HoldTetriminoComponent>();
            }

            // 새로운 테트리미노 생성 요청
            RequestNewTetrimino();
        }

        /// <summary>
        /// 홀드 교체 (이미 홀드된 테트리미노가 있을 때)
        /// </summary>
        private void ProcessHoldSwap(Context context, Entity currentTetriminoEntity, Entity holdTetriminoEntity)
        {
            Debug.Log("홀드 교체 실행");

            // 홀드된 테트리미노의 컴포넌트들 가져오기
            var holdTetriminoComponent = holdTetriminoEntity.GetComponent<TetriminoComponent>();
            if (holdTetriminoComponent == null)
            {
                Debug.LogWarning("홀드된 테트리미노의 TetriminoComponent를 찾을 수 없습니다.");
                return;
            }

            // 1. 현재 테트리미노의 TetriminoComponent 정보 저장
            var currentTetriminoComponent = currentTetriminoEntity.GetComponent<TetriminoComponent>();
            var currentType = currentTetriminoComponent.Type;

            // 2. 홀드된 테트리미노를 현재 테트리미노로 변경
            // 홀드 엔티티에서 HoldTetriminoComponent 제거하고 CurrentTetriminoComponent 추가
            holdTetriminoEntity.RemoveComponent<HoldTetriminoComponent>();
            var newCurrentComponent = holdTetriminoEntity.AddComponent<CurrentTetriminoComponent>();

            // 홀드된 테트리미노를 기본 스폰 위치와 회전으로 리셋
            newCurrentComponent.Position = new UnityEngine.Vector2Int(5, 18); // 스폰 위치
            var holdTetrimino = holdTetriminoEntity.GetComponent<TetriminoComponent>();
            holdTetrimino.Rotation = 0; // 회전 초기화

            // 3. 현재 테트리미노를 홀드로 변경
            currentTetriminoEntity.RemoveComponent<CurrentTetriminoComponent>();
            currentTetriminoEntity.AddComponent<HoldTetriminoComponent>();

            // 현재 테트리미노의 회전도 초기화
            currentTetriminoComponent.Rotation = 0;

            Debug.Log($"홀드 교체 완료: {holdTetrimino.Type} ↔ {currentType}");
        }

        /// <summary>
        /// 새로운 테트리미노 생성 요청
        /// </summary>
        private void RequestNewTetrimino()
        {
            var commandRequest = GetCommandRequest();
            if (commandRequest != null)
            {
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.GenerateTetrimino,
                    PayLoad = null
                });
                Debug.Log("새로운 테트리미노 생성 요청됨");
            }
        }

        private CommandRequestComponent GetCommandRequest()
        {
            var commandRequestEntities = Context.GetEntitiesWithComponent<CommandRequestComponent>();
            if (commandRequestEntities.Count == 0)
            {
                Debug.LogWarning("CommandRequestComponent가 있는 엔티티가 없습니다.");
                return null; // 명령 요청이 없으면 null 반환
            }
            else if (commandRequestEntities.Count > 1)
            {
                Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null; // 여러 엔티티가 있으면 경고 후 null 반환
            }

            return commandRequestEntities[0].GetComponent<CommandRequestComponent>();
        }
    }
}