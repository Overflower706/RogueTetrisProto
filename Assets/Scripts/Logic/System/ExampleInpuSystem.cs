using OVFL.ECS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minomino
{
    // GameCanvas에서의 Input 처리 예시
    public class ExampleInputSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }
        private Keyboard keyboard;
        private bool isStarted = false; // 게임 시작 여부 확인용.

        public void Setup(Context context)
        {
            keyboard = Keyboard.current;
        }

        public void Tick(Context context)
        {
            var commandEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0 && !isStarted)
            {
                isStarted = true; // 게임 시작 상태로 설정
            }

            if (!isStarted)
            {
                return; // 게임이 시작되지 않은 상태에서는 입력을 처리하지 않음
            }

            // 받아야할 키보드 입력
            // 방향키 좌, 우 (현재 테트리미노 좌, 우 움직이기
            // 방향키 위 (홀드)
            // 방향키 아래 (소프트 드롭)
            // 스페이스바 (하드 드롭)
            // z (회전 반시계 방향)
            // x (회전 시계 방향)

            // CommandRequestComponent 찾기
            var commandRequestEntity = GetCommandRequestEntity(context);

            var commandRequest = commandRequestEntity.GetComponent<CommandRequestComponent>();

            // 방향키 좌 (현재 테트리미노 좌측 이동)
            if (keyboard.aKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 좌측 이동 (←)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.MoveLeft,
                    PayLoad = null
                });
            }

            // 방향키 우 (현재 테트리미노 우측 이동)
            if (keyboard.dKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 우측 이동 (→)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.MoveRight,
                    PayLoad = null
                });
            }

            // 방향키 위 (홀드)
            if (keyboard.wKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 홀드 (↑)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.Hold,
                    PayLoad = null
                });
            }

            // 방향키 아래 (소프트 드롭)
            if (keyboard.sKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 소프트 드롭 (↓)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.SoftDrop,
                    PayLoad = null
                });
            }

            // 스페이스바 (하드 드롭)
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 하드 드롭 (Space)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.HardDrop,
                    PayLoad = null
                });
            }

            // Z키 (회전 반시계 방향)
            if (keyboard.qKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 반시계 회전 (Z)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.RotateCounterClockwise,
                    PayLoad = null
                });
            }

            // X키 (회전 시계 방향)
            if (keyboard.eKey.wasPressedThisFrame)
            {
                Debug.Log("입력: 시계 회전 (X)");
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.RotateClockwise,
                    PayLoad = null
                });
            }
        }

        /// <summary>
        /// CommandRequestComponent가 있는 엔티티를 찾기
        /// </summary>
        private Entity GetCommandRequestEntity(Context context)
        {
            var commandRequestEntities = context.GetEntitiesWithComponent<CommandRequestComponent>();

            if (commandRequestEntities.Count == 0)
            {
                return null;
            }
            else if (commandRequestEntities.Count > 1)
            {
                Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 첫 번째 엔티티를 사용합니다.");
            }

            return commandRequestEntities[0];
        }
    }
}