using MangaAPI.DTOs;
using MangaAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MangaAPI.Services
{
    public class PythonTranslationItem
    {
        [JsonPropertyName("pageRegionId")]
        public long PageRegionId { get; set; }

        [JsonPropertyName("translated_text")]
        public string? TranslatedText { get; set; }
    }

    public class PythonTranslationResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("data")]
        public List<PythonTranslationItem>? Data { get; set; }
    }

    public class MangaAiService
    {
        private readonly HttpClient _httpClient;

        public MangaAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Trỏ thẳng sang cổng 8000 của Python
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/"); 
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<MangaRegionResponse?> AnalyzePageAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType!);
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("api/segment", content);
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            return JsonSerializer.Deserialize<MangaRegionResponse>(jsonResult, options);
        }

        public async Task<List<PythonTranslationItem>?> TranslateRegionsAsync(IFormFile file, List<PageRegion> regions)
        {
            using var content = new MultipartFormDataContent();

            // 1. Đóng gói file ảnh gửi đi
            var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType!);
            content.Add(streamContent, "file", file.FileName);

            // 2. Chuyển danh sách vùng thành chuỗi JSON
            var regionsData = regions.Select(r => new {
                pageRegionId = r.PageRegionId,
                x = (int)r.X,
                y = (int)r.Y,
                width = (int)r.Width,
                height = (int)r.Height
            }).ToList();

            string jsonString = JsonSerializer.Serialize(regionsData);
            content.Add(new StringContent(jsonString), "regions_json");

            // 3. Bắn sang API Python (Sử dụng _httpClient được quản lý tập trung)
            var response = await _httpClient.PostAsync("api/translate", content);
            
            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PythonTranslationResponse>(responseString);

            return result?.Status == "success" ? result.Data : null;
        }
    }
}