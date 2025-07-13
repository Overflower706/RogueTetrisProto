using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class ScoreSystem : ITickSystem
    {
        public Context Context { get; set; }

        // 점수 계산 상수
        private const int BASE_LINE_SCORE = 100;
        private const int SINGLE_LINE = 1;
        private const int DOUBLE_LINE = 3;
        private const int TRIPLE_LINE = 5;
        private const int TETRIS_LINE = 8;

        public void Tick(Context context)
        {
            var score = GetScoreComponent();
            if (score == null) return;

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                score.CurrentScore = 0;
                score.TargetScore = 1000; // 예시로 1000점으로 

                Debug.Log("게임 시작, 초기 점수 설정: " + score.CurrentScore);
            }

            // 줄 클리어 이벤트 처리
            var lineClearEntities = Context.GetEntitiesWithComponent<LineClearCommand>();
            foreach (var entity in lineClearEntities)
            {
                var lineClearCommand = entity.GetComponent<LineClearCommand>();
                ProcessLineClear(score, lineClearCommand);
            }
        }

        private ScoreComponent GetScoreComponent()
        {
            var scoreEntities = Context.GetEntitiesWithComponent<ScoreComponent>();

            if (scoreEntities.Count == 0)
            {
                Debug.LogWarning("ScoreComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (scoreEntities.Count > 1)
            {
                Debug.LogWarning("ScoreComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return scoreEntities[0].GetComponent<ScoreComponent>();
        }

        /// <summary>
        /// 줄 클리어 이벤트 처리 및 점수 계산
        /// </summary>
        private void ProcessLineClear(ScoreComponent score, LineClearCommand lineClearCommand)
        {
            if (lineClearCommand.LinesCleared <= 0) return;

            // 기본 점수 계산
            int baseScore = CalculateBaseScore(lineClearCommand.LinesCleared);

            // 테트리미노 Entity에서 추가 효과 확인 (향후 확장 가능)
            // TODO: TetriminoEntityId를 통해 특수 효과 적용

            // 최종 점수 적용
            score.CurrentScore += baseScore;

            Debug.Log($"줄 클리어! 클리어된 줄: {lineClearCommand.LinesCleared}개, 획득 점수: {baseScore}, 총 점수: {score.CurrentScore}");
        }

        /// <summary>
        /// 클리어된 줄 수에 따른 기본 점수 계산 (테트리스 표준 점수)
        /// </summary>
        private int CalculateBaseScore(int linesCleared)
        {
            switch (linesCleared)
            {
                case 1: return BASE_LINE_SCORE * SINGLE_LINE;
                case 2: return BASE_LINE_SCORE * DOUBLE_LINE;
                case 3: return BASE_LINE_SCORE * TRIPLE_LINE;
                case 4: return BASE_LINE_SCORE * TETRIS_LINE;
                default: return BASE_LINE_SCORE * linesCleared;
            }
        }

        /// <summary>
        /// 줄 클리어 이벤트 처리 및 점수 계산 (향상된 버전)
        /// </summary>
        private void ProcessLineClearAdvanced(ScoreComponent score, LineClearCommand lineClearCommand)
        {
            if (lineClearCommand.LinesCleared <= 0) return;

            // 기본 점수 계산
            int baseScore = CalculateBaseScore(lineClearCommand.LinesCleared);

            // 배율 컴포넌트 가져오기 (선택적)
            var multiplierComponent = GetScoreMultiplierComponent();
            float totalMultiplier = 1.0f;

            if (multiplierComponent != null)
            {
                // 레벨 배율 적용
                totalMultiplier *= multiplierComponent.ScoreMultiplier;

                // 콤보 시스템 업데이트
                multiplierComponent.ComboCount++;
                multiplierComponent.ComboMultiplier = 1.0f + (multiplierComponent.ComboCount * 0.1f);
                totalMultiplier *= multiplierComponent.ComboMultiplier;

                Debug.Log($"콤보 {multiplierComponent.ComboCount}! 배율: {multiplierComponent.ComboMultiplier:F1}");
            }

            // 최종 점수 계산
            int finalScore = Mathf.RoundToInt(baseScore * totalMultiplier);
            score.CurrentScore += finalScore;

            Debug.Log($"줄 클리어! 클리어된 줄: {lineClearCommand.LinesCleared}개, 기본 점수: {baseScore}, 배율: {totalMultiplier:F2}, 최종 점수: {finalScore}, 총 점수: {score.CurrentScore}");
        }

        /// <summary>
        /// ScoreMultiplierComponent 가져오기 (선택적)
        /// </summary>
        private ScoreMultiplierComponent GetScoreMultiplierComponent()
        {
            var multiplierEntities = Context.GetEntitiesWithComponent<ScoreMultiplierComponent>();
            return multiplierEntities.Count > 0 ? multiplierEntities[0].GetComponent<ScoreMultiplierComponent>() : null;
        }
    }
}