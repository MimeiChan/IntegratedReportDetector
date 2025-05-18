using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntegratedReportDetector
{
    public class IntegratedReportScorer
    {
        private readonly OllamaClient _ollamaClient;
        private readonly string _promptTemplate;

        public IntegratedReportScorer(OllamaClient ollamaClient)
        {
            _ollamaClient = ollamaClient;
            _promptTemplate = @"
あなたはPDFが「統合報告書」であるかどうかを判断する専門家です。
以下のテキストはPDFの最初の数ページから抽出されたものです。
このテキストを分析し、このPDFが企業の「統合報告書」である可能性を0～100のスコアで評価してください。

判断基準：
- 財務情報と非財務情報の統合
- ESG、サステナビリティに関する言及
- 中長期的な価値創造に関する記述
- ステークホルダーへの包括的な情報提供
- 企業のビジョン・戦略・ガバナンスの説明

抽出テキスト：
{0}

回答形式：
スコア: [0-100の数値]
理由: [簡潔な判断理由]
";
        }

        public async Task<(int Score, string Reason)> ScorePdfText(string pdfText)
        {
            var prompt = string.Format(_promptTemplate, pdfText);
            var response = await _ollamaClient.GenerateCompletion(prompt);
            
            return ParseScoreAndReason(response);
        }

        private (int Score, string Reason) ParseScoreAndReason(string response)
        {
            int score = 0;
            string reason = string.Empty;

            try
            {
                // スコア行を探す
                var scoreMatch = Regex.Match(response, @"スコア:\s*(\d+)");
                if (scoreMatch.Success && scoreMatch.Groups.Count > 1)
                {
                    int.TryParse(scoreMatch.Groups[1].Value, out score);
                }
                else
                {
                    // 数字のみの検索も試す
                    var numericMatch = Regex.Match(response, @"(\d{1,3})\s*\/\s*100");
                    if (numericMatch.Success && numericMatch.Groups.Count > 1)
                    {
                        int.TryParse(numericMatch.Groups[1].Value, out score);
                    }
                }

                // 理由行を探す
                var reasonMatch = Regex.Match(response, @"理由:\s*(.+?)(\r|\n|$)");
                if (reasonMatch.Success && reasonMatch.Groups.Count > 1)
                {
                    reason = reasonMatch.Groups[1].Value.Trim();
                }
                else
                {
                    // 理由が複数行にわたる場合や形式が異なる場合の対応
                    var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("理由:") || lines[i].Contains("理由："))
                        {
                            reason = lines[i].Replace("理由:", "").Replace("理由：", "").Trim();
                            
                            // 次の行も含める可能性がある場合
                            if (i + 1 < lines.Length && !lines[i + 1].Contains("スコア:") && !lines[i + 1].Contains("スコア："))
                            {
                                reason += " " + lines[i + 1].Trim();
                            }
                            break;
                        }
                    }
                }

                // 理由が見つからない場合、レスポンス全体から推測
                if (string.IsNullOrWhiteSpace(reason) && !string.IsNullOrWhiteSpace(response))
                {
                    // スコア行を除いた最初の段落を理由と仮定
                    var paragraphs = response.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var paragraph in paragraphs)
                    {
                        if (!paragraph.Contains("スコア:") && paragraph.Length > 10)
                        {
                            reason = paragraph.Trim();
                            if (reason.Length > 200) reason = reason.Substring(0, 197) + "...";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"スコア解析エラー: {ex.Message}");
                score = 0;
                reason = "スコアの解析に失敗しました";
            }

            // スコアが範囲外の場合は修正
            if (score < 0) score = 0;
            if (score > 100) score = 100;

            return (score, reason);
        }
    }
}
