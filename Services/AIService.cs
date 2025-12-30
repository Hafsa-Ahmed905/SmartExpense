using System.Text;
using System.Text.Json;

namespace FP.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            if (_configuration["Gemini:ApiKey"] != null)
            {
                _apiKey = _configuration["Gemini:ApiKey"];
            }
            else
            {
                throw new InvalidOperationException("Gemini API key not configured");
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generate?key={_apiKey}";
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                    
                    var responseText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                    if (responseText != null)
                    {
                        return responseText;
                    }
                    else
                    {
                        return "I apologize, but I couldn't generate a response.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                return $"Error generating response: {ex.Message}";
            }
        }
    }

    public class GeminiResponse
    {
        public List<Candidate> Candidates { get; set; } = new();
    }

    public class Candidate
    {
        public Content Content { get; set; } = new();
    }

    public class Content
    {
        public List<Part> Parts { get; set; } = new();
    }

    public class Part
    {
        public string Text { get; set; } = string.Empty;
    }
}
