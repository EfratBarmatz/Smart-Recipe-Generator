using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Services
{
    public class ImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<ImageService> _logger;
        
        public ImageService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ImageService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<string> GenerateRecipeImageAsync(string recipeDescription)
        {
            var apiKey = _config["HuggingFace:ApiKey"];
            var HUGGINGFACE_API_URL =_config["HuggingFace:Endpoint"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("HuggingFace API key not configured. Skipping image generation.");
                return null;
            }

            try
            {
                var prompt = CreateImagePrompt(recipeDescription);
                _logger.LogInformation("Generating image with prompt: {Prompt}", prompt);

                var client = _httpClientFactory.CreateClient("image-client");
                using var httpReq = new HttpRequestMessage(HttpMethod.Post, HUGGINGFACE_API_URL);

                httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var body = new
                {
                    inputs = prompt,
                    options = new { wait_for_model = true }
                };

                httpReq.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.SendAsync(httpReq);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Image generation failed with status {StatusCode}: {Error}",
                        response.StatusCode, errorText);
                    return null;
                }

                // קריאת התמונה כ-byte array
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                // המרה ל-Base64 string
                var base64Image = Convert.ToBase64String(imageBytes);
                var imageUrl = $"data:image/png;base64,{base64Image}";

                _logger.LogInformation("Image generated successfully, size: {Size} bytes", imageBytes.Length);

                return imageUrl;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while generating image");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating image");
                return null;
            }
        }

        private string CreateImagePrompt(string recipeDescription)
        {
            // ניקוי תיאור המתכון מטקסט עברי (המודל עובד טוב יותר עם אנגלית)
            // אפשר להוסיף תרגום אוטומטי אם צריך

            var cleanDescription = recipeDescription?.Trim() ?? "delicious food";

            // יצירת פרומפט איכותי
            return $"professional food photography of {cleanDescription}, " +
          "ultra detailed, 8k resolution, gourmet presentation, " +
          "dramatic lighting, shallow depth of field, " +
          "vibrant colors, appetizing, mouth-watering, " +
          "restaurant quality plating, fresh ingredients, " +
          "sharp focus, bokeh background, masterpiece";
        }
    }
}