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

                    // Ollamaでスコアリング
                    Console.WriteLine("LLMによるスコアリング中...");
                    var ollamaClient = new OllamaClient("http://localhost:11434", "gemma3:1b"); // モデル名はllama2などに適宜変更
                    var scorer = new IntegratedReportScorer(ollamaClient);
                    var (score, reason) = await scorer.ScorePdfText(extractedText);

                    // 結果の表示
                    Console.WriteLine("\n===== 分析結果 =====");
                    Console.WriteLine($"スコア: {score}/100");
                    Console.WriteLine($"判断理由: {reason}");
                    Console.WriteLine($"判定: {(score >= 70 ? "統合報告書の可能性が高い" : "統合報告書ではない可能性が高い")}");
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
