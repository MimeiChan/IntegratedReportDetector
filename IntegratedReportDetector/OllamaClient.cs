using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntegratedReportDetector
{
    public class OllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _modelName;

        public OllamaClient(string baseUrl = "http://localhost:11434", string modelName = "llama2")
        {
            _baseUrl = baseUrl;
            _modelName = modelName;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2); // 2分のタイムアウト設定
        }

        public async Task<string> GenerateCompletion(string prompt)
        {
            Console.WriteLine($"LLMモデル '{_modelName}' に送信中...");

            var request = new
            {
                model = _modelName,
                prompt = prompt,
                stream = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                return result?.Response ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ollama API呼び出し中にエラーが発生しました: {ex.Message}", ex);
            }
        }
    }

    public class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;
        
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
        
        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
