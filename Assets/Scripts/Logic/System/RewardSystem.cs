using OVFL.ECS;
using UnityEngine;
using System.Collections.Generic;

namespace Minomino
{
    public class RewardSystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var start = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (start.Count > 0)
            {
                foreach (var rewardLine in GlobalSettings.Instance.RewardLines)
                {
                    var rewardEntity = Context.CreateEntity();
                    var reward = rewardEntity.AddComponent<RewardComponent>();
                    reward.Line = rewardLine;
                    reward.IsReceived = false;
                    reward.Rewards = GetRandomRewards(3);

                    Debug.Log($"{rewardLine}째 줄에 보상 생성: {string.Join(", ", reward.Rewards)}");
                }
            }

            var end = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (end.Count > 0)
            {
                // 게임 종료시 모든 보상들을 제거합니다.
                var rewards = Context.GetEntitiesWithComponent<RewardComponent>();
                foreach (var reward in rewards)
                {
                    Context.DestroyEntity(reward);
                }
            }

            var state = Context.GetGameState();
            if (state.CurrentState != GameState.Playing) return;

            var completedLineEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            if (completedLineEntities.Count > 0)
            {
                var completedLines = Context.GetCompletedLine().CompletedLine;
                foreach (var completedLine in completedLines)
                {
                    if (GlobalSettings.Instance.RewardLines.Contains(completedLine))
                    {
                        // 해당 줄에 대한 보상을 생성합니다.
                        var rewardEntity = Context.GetEntitiesWithComponent<RewardComponent>()
                            .Find(r => r.GetComponent<RewardComponent>().Line == completedLine);

                        var reward = rewardEntity.GetComponent<RewardComponent>();
                        if (reward.IsReceived) continue; // 이미 보상을 받은 경우는 건너뜁니다.
                        reward.IsReceived = true; // 보상을 받았음을 표시

                        Debug.Log($"{completedLine}째 줄에 보상 : {string.Join(", ", rewardEntity.GetComponent<RewardComponent>().Rewards)}");
                    }
                }
            }
        }

        public void Cleanup()
        {
            var completedLineEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            foreach (var completedLineEntity in completedLineEntities)
            {
                Context.DestroyEntity(completedLineEntity);
            }
        }

        private List<string> GetRandomRewards(int count)
        {
            var allDescriptions = GlobalSettings.Instance.SpecialMinueDescriptions;
            var selectedRewards = new List<string>();

            if (allDescriptions == null || allDescriptions.Count == 0)
            {
                Debug.LogWarning("SpeicialMinueDescriptions가 비어있습니다.");
                return selectedRewards;
            }

            // 사용 가능한 설명의 인덱스 리스트 생성
            var availableIndices = new List<int>();
            for (int i = 0; i < allDescriptions.Count; i++)
            {
                availableIndices.Add(i);
            }

            // 요청된 개수만큼 무작위로 선택 (중복 없음)
            int selectCount = Mathf.Min(count, allDescriptions.Count);
            for (int i = 0; i < selectCount; i++)
            {
                int randomIndex = Random.Range(0, availableIndices.Count);
                int selectedIndex = availableIndices[randomIndex];
                selectedRewards.Add(allDescriptions[selectedIndex]);
                availableIndices.RemoveAt(randomIndex);
            }

            return selectedRewards;
        }
    }
}