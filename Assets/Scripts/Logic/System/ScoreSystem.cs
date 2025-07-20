using System;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class ScoreSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var score = GetScore();

            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                var player = GetPlayer();

                Debug.Log($"게임 시작, 플레이어 정보: 라운드 {player.Round}, 스테이지 {player.Stage}");
                Debug.Log($"플레이어의 라운드 보너스: {GlobalSettings.Instance.RoundBonus}, 스테이지 보너스: {GlobalSettings.Instance.StageBonus}");

                score.CurrentScore = 0;
                score.TargetScore = (int)(Math.Pow(player.Round, GlobalSettings.Instance.RoundBonus) * player.Stage * GlobalSettings.Instance.StageBonus);
                Debug.Log($"게임 시작, 초기 점수 설정: {score.CurrentScore}, 목표 점수: {score.TargetScore}");
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                Debug.Log("게임 종료, 최종 점수: " + score.CurrentScore);
                score.CurrentScore = 0; // 게임 종료 시 점수 초기화
                score.TargetScore = 0; // 목표 점수도 초기화
                return; // 게임 종료 시 더 이상 점수 계산하지 않음
            }

            var completedLineComponentEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            if (completedLineComponentEntities.Count > 0)
            {
                var board = GetBoard();
                var completedLineEntity = completedLineComponentEntities[0];
                var completedLineComponent = completedLineEntity.GetComponent<CompletedLineComponent>();

                foreach (var line in completedLineComponent.CompletedLine)
                {
                    for (int x = 0; x < BoardComponent.WIDTH; x++)
                    {
                        int entityID = board.Board[x, line];
                        if (entityID != 0)
                        {
                            var minoEntity = FindEntityByID(entityID);
                            var minoComponent = minoEntity.GetComponent<MinoComponent>();

                            if (minoComponent.State == MinoState.Living)
                            {
                                score.CurrentScore += 1;
                                Debug.Log($"{line}번째 줄에서 Mino ID {entityID}의 상태를 Living으로 변경하고 점수에 1점 추가했습니다. 현재 점수: {score.CurrentScore}");
                            }
                            Debug.Log($"높이 {line}의 Mino ID {entityID}의 상태를 Living으로 변경했습니다.");
                        }
                    }
                }

                Context.DestroyEntity(completedLineEntity);
            }

            #region 줄 클리어 이벤트. 주석 처리 안해도 되지만 일단은
            // 줄 클리어 이벤트 처리
            // var lineClearEntities = Context.GetEntitiesWithComponent<LineClearCommand>();
            // foreach (var entity in lineClearEntities)
            // {
            //     var lineClearCommand = entity.GetComponent<LineClearCommand>();
            //     ProcessLineClear(score, lineClearCommand);
            // }
            #endregion
        }

        private GameStateComponent GetState()
        {
            var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();
            if (stateEntities.Count == 0)
            {
                Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (stateEntities.Count > 1)
            {
                Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return stateEntities[0].GetComponent<GameStateComponent>();
        }

        private BoardComponent GetBoard()
        {
            var boardEntities = Context.GetEntitiesWithComponent<BoardComponent>();
            if (boardEntities.Count == 0)
            {
                Debug.LogWarning("BoardComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (boardEntities.Count > 1)
            {
                Debug.LogWarning("BoardComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return boardEntities[0].GetComponent<BoardComponent>();
        }

        private CommandRequestComponent GetCommandRequest()
        {
            var commandEntities = Context.GetEntitiesWithComponent<CommandRequestComponent>();
            if (commandEntities.Count == 0)
            {
                Debug.LogWarning("CommandRequestComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (commandEntities.Count > 1)
            {
                Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return commandEntities[0].GetComponent<CommandRequestComponent>();
        }

        private ScoreComponent GetScore()
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

        private PlayerComponent GetPlayer()
        {
            var playerEntities = Context.GetEntitiesWithComponent<PlayerComponent>();
            if (playerEntities.Count == 0)
            {
                Debug.LogWarning("PlayerComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (playerEntities.Count > 1)
            {
                Debug.LogWarning("PlayerComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return playerEntities[0].GetComponent<PlayerComponent>();
        }

        private Entity FindEntityByID(int id)
        {
            var entities = Context.GetEntities();
            foreach (var entity in entities)
            {
                if (entity.ID == id)
                {
                    return entity;
                }
            }
            return null;
        }

        private int ProcessBake(int[] row)
        {
            TetriminoColor[] colors = new TetriminoColor[row.Length];

            // Entity ID를 색상으로 변환
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == 0) // 빈 칸
                {
                    colors[i] = TetriminoColor.None;
                }
                else
                {
                    var entity = GetEntityByID(row[i]);
                    if (entity != null)
                    {
                        var component = entity.GetComponent<TetriminoComponent>();
                        colors[i] = component.Color;
                    }
                    else
                    {
                        colors[i] = TetriminoColor.None;
                    }
                }
            }

            Debug.Log($"=== 베이킹 점수 계산 시작 ===");
            string colorPattern = string.Join("-", System.Array.ConvertAll(colors, GetIngredientName));
            Debug.Log($"재료 패턴: {colorPattern}");

            int totalScore = CalculateRecipeScore(colors);

            Debug.Log($"최종 베이킹 점수: {totalScore}점");
            Debug.Log($"=== 베이킹 점수 계산 완료 ===");

            return totalScore;
        }

        /// <summary>
        /// 레시피 기반 점수 계산 (높은 점수 레시피부터 우선 처리)
        /// </summary>
        private int CalculateRecipeScore(TetriminoColor[] colors)
        {
            // 재료 개수 카운트
            int dough = 0;    // 반죽 (Red)
            int yeast = 0;    // 이스트 (Green)
            int syrup = 0;    // 시럽 (Blue)

            foreach (var color in colors)
            {
                switch (color)
                {
                    case TetriminoColor.Red: dough++; break;
                    case TetriminoColor.Green: yeast++; break;
                    case TetriminoColor.Blue: syrup++; break;
                }
            }

            Debug.Log($"재료 보유량: 반죽 {dough}개, 이스트 {yeast}개, 시럽 {syrup}개");

            int totalScore = 0;
            string recipeLog = "";

            // 1. 초코 소라빵 (반죽 + 이스트 + 시럽) = 100점 (최우선)
            int chocoSorabread = System.Math.Min(System.Math.Min(dough, yeast), syrup);
            if (chocoSorabread > 0)
            {
                totalScore += chocoSorabread * GlobalSettings.Instance.ChocoSoraBreadScore;
                dough -= chocoSorabread;
                yeast -= chocoSorabread;
                syrup -= chocoSorabread;
                recipeLog += $"초코 소라빵 {chocoSorabread}개 = {chocoSorabread * 100}점, ";
                Debug.Log($"초코 소라빵 {chocoSorabread}개 제작 (+{chocoSorabread * 100}점)");
            }

            // 2. 식빵 (반죽 + 이스트) = 50점
            int bread = Math.Min(dough, yeast);
            if (bread > 0)
            {
                totalScore += bread * GlobalSettings.Instance.BreadScore;
                dough -= bread;
                yeast -= bread;
                recipeLog += $"식빵 {bread}개 = {bread * 50}점, ";
                Debug.Log($"식빵 {bread}개 제작 (+{bread * 50}점)");
            }

            // 3. 아이싱 쿠키 (반죽 + 시럽) = 50점
            int icingCookie = Math.Min(dough, syrup);
            if (icingCookie > 0)
            {
                totalScore += icingCookie * GlobalSettings.Instance.IcingCookieScore;
                dough -= icingCookie;
                syrup -= icingCookie;
                recipeLog += $"아이싱 쿠키 {icingCookie}개 = {icingCookie * 50}점, ";
                Debug.Log($"아이싱 쿠키 {icingCookie}개 제작 (+{icingCookie * 50}점)");
            }

            // 4. 쿠키 (반죽) = 10점 (최후)
            if (dough > 0)
            {
                totalScore += dough * GlobalSettings.Instance.CookieScore;
                recipeLog += $"쿠키 {dough}개 = {dough * 10}점, ";
                Debug.Log($"쿠키 {dough}개 제작 (+{dough * 10}점)");
            }

            // 남은 재료 로그
            if (yeast > 0 || syrup > 0)
            {
                Debug.Log($"사용되지 않은 재료: 이스트 {yeast}개, 시럽 {syrup}개");
            }

            if (!string.IsNullOrEmpty(recipeLog))
            {
                recipeLog = recipeLog.TrimEnd(' ', ',');
                Debug.Log($"레시피 결과: {recipeLog}");
            }

            return totalScore;
        }

        /// <summary>
        /// 색상을 재료 이름으로 변환
        /// </summary>
        private string GetIngredientName(TetriminoColor color)
        {
            switch (color)
            {
                case TetriminoColor.Red: return "반죽";
                case TetriminoColor.Green: return "이스트";
                case TetriminoColor.Blue: return "시럽";
                default: return "없음";
            }
        }

        private Entity GetEntityByID(int entityID)
        {
            // 모든 Entity에서 ID로 검색
            var allEntities = Context.GetEntities();
            foreach (var entity in allEntities)
            {
                if (entity.ID == entityID)
                {
                    return entity;
                }
            }

            Debug.LogWarning($"Entity ID {entityID}를 찾을 수 없습니다.");
            return null;
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
                case 3: return 3;
                case 4: return 4;
                default: return linesCleared; // 4줄 이상인 경우 선형 증가
            }
        }
    }
}