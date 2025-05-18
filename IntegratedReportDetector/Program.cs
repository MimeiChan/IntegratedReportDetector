using System;
using System.Threading.Tasks;

namespace IntegratedReportDetector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("統合報告書検出ツール - プロトタイプ");
            Console.WriteLine("===============================");

            while (true)
            {
                Console.WriteLine("\nPDFファイルのパスを入力してください (終了するには 'exit' と入力):");
                string? filePath = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(filePath) || filePath.ToLower() == "exit")
                {
                    break;
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("ファイルが見つかりません。正しいパスを入力してください。");
                    continue;
                }

                try
                {
                    // PDFからテキストを抽出
                    Console.WriteLine("PDFからテキストを抽出中...");
                    var pdfExtractor = new PdfExtractor();
                    string extractedText = pdfExtractor.ExtractText(filePath, 3); // 最初の3ページまで抽出

                    Console.WriteLine($"抽出テキスト長: {extractedText.Length} 文字");

                    // Ollamaクライアント初期化
                    var ollamaClient = new OllamaClient("http://192.168.1.234:11434", "gemma3:4b-it-qat"); // モデル名はllama2などに適宜変更
                    var scorer = new IntegratedReportScorer(ollamaClient);

                    // 統合報告書スコアリング
                    Console.WriteLine("LLMによる統合報告書判定中...");
                    var (score, reason) = await scorer.ScorePdfText(extractedText);

                    //// 年度判定
                    //Console.WriteLine("LLMによる当年度判定中...");
                    //var (isCurrentFiscalYear, fiscalYearReason, detectedYear) = await scorer.CheckFiscalYear(extractedText);

                    // 結果の表示
                    Console.WriteLine("\n===== 分析結果 =====");
                    Console.WriteLine($"スコア: {score}/100");
                    Console.WriteLine($"判断理由: {reason}");
                    Console.WriteLine($"統合報告書判定: {(score >= 70 ? "統合報告書の可能性が高い" : "統合報告書ではない可能性が高い")}");
                    
                    //Console.WriteLine("\n===== 年度判定 =====");
                    //Console.WriteLine($"当年度判定: {(isCurrentFiscalYear ? "当年度の文書です" : "当年度の文書ではありません")}");
                    //Console.WriteLine($"検出された年度: {detectedYear}");
                    //Console.WriteLine($"判断理由: {fiscalYearReason}");
                    
                    // 総合判定
                    bool isRelevantDocument = score >= 70;
                    Console.WriteLine("\n===== 総合判定 =====");
                    Console.WriteLine($"結果: {(isRelevantDocument ? "当年度の統合報告書です" : "当年度の統合報告書ではありません")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラーが発生しました: {ex.Message}");
                }
            }

            Console.WriteLine("プログラムを終了します。");
        }
    }
}
