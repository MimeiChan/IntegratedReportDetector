using System;
using System.Text;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace IntegratedReportDetector
{
    public class PdfExtractor
    {
        public string ExtractText(string filePath, int maxPages = 3)
        {
            StringBuilder text = new StringBuilder();

            try
            {
                // 標準の抽出方法を試す
                ExtractWithStandardMethod(filePath, maxPages, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"標準のテキスト抽出中にエラーが発生しました: {ex.Message}");
                Console.WriteLine("フォールバック抽出方法を試みます...");
                
                try
                {
                    // フォールバック方法でもう一度試す
                    ExtractWithFallbackMethod(filePath, maxPages, text);
                }
                catch (Exception fallbackEx)
                {
                    throw new Exception($"PDFテキスト抽出中にエラーが発生しました: {fallbackEx.Message}", fallbackEx);
                }
            }

            // テキストが空の場合のフォールバック
            if (string.IsNullOrWhiteSpace(text.ToString()))
            {
                text.AppendLine("テキスト抽出に失敗しました。このPDFは日本語テキストを含んでいる可能性があり、特殊なフォント処理が必要です。");
                text.AppendLine("itext7.asianパッケージが必要な場合があります。");
            }

            return text.ToString();
        }

        private void ExtractWithStandardMethod(string filePath, int maxPages, StringBuilder text)
        {
            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                int pageCount = Math.Min(pdfDoc.GetNumberOfPages(), maxPages);
                
                for (int i = 1; i <= pageCount; i++)
                {
                    var page = pdfDoc.GetPage(i);
                    string pageText = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
                    text.AppendLine(pageText);
                    text.AppendLine("---ページ区切り---");
                }
            }
        }

        private void ExtractWithFallbackMethod(string filePath, int maxPages, StringBuilder text)
        {
            // ロケーションベースの抽出戦略を使用（日本語などで効果的な場合がある）
            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                int pageCount = Math.Min(pdfDoc.GetNumberOfPages(), maxPages);
                
                for (int i = 1; i <= pageCount; i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var strategy = new LocationTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    text.AppendLine(pageText);
                    text.AppendLine("---ページ区切り---");
                }
            }
        }
    }
}
