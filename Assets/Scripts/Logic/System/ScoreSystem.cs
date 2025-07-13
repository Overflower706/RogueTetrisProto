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

            var commandEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                score.CurrentScore = 0;
                score.TargetScore = 500; // 예시로 500점으로 

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
        /// 줄 클리어 이벤트 처리 및 점수 계산 (새로운 색상 연속 규칙)
        /// </summary>
        private void ProcessLineClear(ScoreComponent score, LineClearCommand lineClearCommand)
        {
            if (lineClearCommand.LinesCleared <= 0 || lineClearCommand.CompletedLinesColors == null) return;

            Debug.Log($"=== 줄 클리어 점수 계산 시작 ===");
            Debug.Log($"완성된 줄 수: {lineClearCommand.LinesCleared}개");

            // 1. 각 줄의 색상 연속 점수 계산
            int totalColorScore = 0;
            for (int lineIndex = 0; lineIndex < lineClearCommand.CompletedLinesColors.Length; lineIndex++)
            {
                int lineScore = CalculateLineColorScore(lineClearCommand.CompletedLinesColors[lineIndex], lineIndex);
                totalColorScore += lineScore;
            }

            // 2. 줄 배수 계산 (1줄→1배, 2줄→2배, 3줄→4배, 4줄→8배)
            int lineMultiplier = GetLineMultiplier(lineClearCommand.LinesCleared);

            // 3. 최종 점수 계산
            int finalScore = totalColorScore * lineMultiplier;
            score.CurrentScore += finalScore;

            Debug.Log($"색상 연속 총합: {totalColorScore}점");
            Debug.Log($"줄 배수: {lineMultiplier}배 ({lineClearCommand.LinesCleared}줄 완성)");
            Debug.Log($"최종 점수: {totalColorScore} × {lineMultiplier} = {finalScore}점");
            Debug.Log($"총 누적 점수: {score.CurrentScore}점");
            Debug.Log($"=== 줄 클리어 점수 계산 완료 ===");
        }

        /// <summary>
        /// 한 줄의 색상 연속 점수 계산
        /// </summary>
        private int CalculateLineColorScore(TetriminoColor[] lineColors, int lineIndex)
        {
            Debug.Log($"--- 줄 {lineIndex} 분석 시작 ---");

            // 색상 배열을 문자열로 변환해서 로그에 출력
            string colorPattern = "";
            for (int i = 0; i < lineColors.Length; i++)
            {
                colorPattern += GetColorName(lineColors[i]);
                if (i < lineColors.Length - 1) colorPattern += "-";
            }
            Debug.Log($"색상 패턴: {colorPattern}");

            int totalScore = 0;
            int currentCombo = 1;
            TetriminoColor currentColor = lineColors[0];
            string scoreBreakdown = "";

            // 첫 번째 블록: 1점
            totalScore += currentCombo;
            scoreBreakdown += $"{GetColorName(currentColor)}1개={currentCombo}";

            // 나머지 블록들 처리
            for (int x = 1; x < lineColors.Length; x++)
            {
                if (lineColors[x] == currentColor)
                {
                    // 같은 색이면 콤보 증가
                    currentCombo++;
                    totalScore += currentCombo;
                }
                else
                {
                    // 색이 다르면 콤보 마무리하고 새로 시작
                    if (currentCombo > 1)
                    {
                        scoreBreakdown += $"(+{currentCombo - 1}={currentCombo * (currentCombo + 1) / 2})";
                    }

                    currentColor = lineColors[x];
                    currentCombo = 1;
                    totalScore += currentCombo;
                    scoreBreakdown += $" + {GetColorName(currentColor)}1개={currentCombo}";
                }
            }

            // 마지막 콤보 정리
            if (currentCombo > 1)
            {
                scoreBreakdown += $"(+{currentCombo - 1}={GetComboTotal(currentCombo)})";
            }

            Debug.Log($"점수 계산: {scoreBreakdown}");
            Debug.Log($"줄 {lineIndex} 총점: {totalScore}점");
            Debug.Log($"--- 줄 {lineIndex} 분석 완료 ---");

            return totalScore;
        }

        /// <summary>
        /// 색상 이름을 한글로 반환
        /// </summary>
        private string GetColorName(TetriminoColor color)
        {
            switch (color)
            {
                case TetriminoColor.Red: return "빨강";
                case TetriminoColor.Green: return "초록";
                case TetriminoColor.Blue: return "파랑";
                case TetriminoColor.Yellow: return "노랑";
                default: return "?";
            }
        }

        /// <summary>
        /// 콤보 총점 계산 (1+2+...+n)
        /// </summary>
        private int GetComboTotal(int comboCount)
        {
            return comboCount * (comboCount + 1) / 2;
        }

        /// <summary>
        /// 줄 수에 따른 배수 계산
        /// </summary>
        private int GetLineMultiplier(int linesCleared)
        {
            switch (linesCleared)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 4;
                case 4: return 8;
                default: return linesCleared; // 4줄 이상인 경우 선형 증가
            }
        }

        /// <summary>
        /// 클리어된 줄 수에 따른 기본 점수 계산 (테트리스 표준 점수) - 더 이상 사용되지 않음
        /// </summary>
        [System.Obsolete("새로운 색상 연속 점수 시스템으로 대체됨")]
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
        /// 줄 클리어 이벤트 처리 및 점수 계산 (향상된 버전) - 더 이상 사용되지 않음
        /// </summary>
        [System.Obsolete("새로운 색상 연속 점수 시스템으로 대체됨")]
        private void ProcessLineClearAdvanced(ScoreComponent score, LineClearCommand lineClearCommand)
        {
            // 이 메서드는 더 이상 사용되지 않습니다.
            // 새로운 ProcessLineClear 메서드를 사용하세요.
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