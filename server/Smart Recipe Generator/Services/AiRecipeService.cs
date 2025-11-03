using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Smart_Recipe_Generator.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;

namespace Smart_Recipe_Generator.Services
{
    public class AiRecipeService : IAiRecipeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AiRecipeService> _logger;

        public AiRecipeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AiRecipeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<RecipeResponse> GenerateRecipeAsync(RecipeRequest request)
        {
            var endpoint = _config["AI:Endpoint"];
            var apiKey = _config["AI:ApiKey"];
            var provider = (_config["AI:Provider"] ?? string.Empty).ToLowerInvariant();

            _logger.LogInformation("AI provider={provider}; endpointConfigured={hasEndpoint}; apiKeyPresent={hasKey}",
                provider, !string.IsNullOrWhiteSpace(endpoint), !string.IsNullOrWhiteSpace(apiKey));

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogWarning("AI endpoint not configured. Returning placeholder recipe.");
                return new RecipeResponse
                {
                    Title = "תוצאה לא זמינה",
                    Ingredients = request.Ingredients,
                    Steps = new List<string> { "לא ניתן ליצור מתכון כרגע." },
                    Servings = request.Servings <= 0 ? 1 : request.Servings
                };
            }

            var prompt = BuildPrompt(request);

            try
            {
                var client = _httpClientFactory.CreateClient("ai-client");
                using var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint);

                if (provider == "huggingface")
                {
                    if (!string.IsNullOrWhiteSpace(apiKey))
                        httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                    var body = new { inputs = prompt };
                    httpReq.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                }
                else // Gemini / Google / generic fallback
                {
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        var separator = endpoint.Contains("?") ? "&" : "?";
                        httpReq.RequestUri = new Uri(endpoint + separator + "key=" + Uri.EscapeDataString(apiKey));
                    }
                    else
                    {
                        var saPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                        if (!string.IsNullOrWhiteSpace(saPath) && File.Exists(saPath))
                        {
                            try
                            {
                                var googleCred = GoogleCredential.FromFile(saPath)
                                    .CreateScoped("https://www.googleapis.com/auth/generative-language.retriever");

                                var accessToken = await googleCred.UnderlyingCredential.GetAccessTokenForRequestAsync();
                                if (!string.IsNullOrWhiteSpace(accessToken))
                                    httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to get access token from Service Account.");
                            }
                        }
                    }

                    if (httpReq.RequestUri == null)
                        httpReq.RequestUri = new Uri(endpoint);

                    var body = new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        }
                    };
                    httpReq.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                }

                _logger.LogInformation("Sending request to: {Endpoint}", httpReq.RequestUri);
                var resp = await client.SendAsync(httpReq);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("AI API returned {StatusCode}: {Error}", resp.StatusCode, respText);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1) Try direct deserialization
                try
                {
                    var parsed = JsonSerializer.Deserialize<RecipeResponse>(respText, options);
                    if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Title))
                        return parsed;
                }
                catch { }

                // 2) Try provider-specific extraction (Gemini candidates)
                try
                {
                    using var doc = JsonDocument.Parse(respText);
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
                    {
                        var first = candidates[0];
                        if (first.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts))
                        {
                            var sb = new StringBuilder();
                            foreach (var el in parts.EnumerateArray())
                            {
                                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("text", out var t))
                                    sb.Append(t.GetString());
                            }
                            respText = sb.ToString();
                        }
                    }
                }
                catch { }

                // 3) Try extracting JSON substring
                var firstBrace = respText.IndexOf('{');
                var lastBrace = respText.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    var jsonSub = respText.Substring(firstBrace, lastBrace - firstBrace + 1);
                    try
                    {
                        var parsed2 = JsonSerializer.Deserialize<RecipeResponse>(jsonSub, options);
                        if (parsed2 != null)
                            return parsed2;
                    }
                    catch { }
                }

                // 4) Fallback: return raw text in steps
                return new RecipeResponse
                {
                    Title = "תוצאה שנוצרה על ידי AI",
                    Ingredients = request.Ingredients,
                    Steps = new List<string> { respText },
                    Servings = request.Servings <= 0 ? 1 : request.Servings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling AI endpoint. Returning placeholder.");
            }

            // Final fallback return
            return new RecipeResponse
            {
                Title = "תוצאה לא זמינה",
                Ingredients = request.Ingredients,
                Steps = new List<string> { "לא ניתן ליצור מתכון כרגע." },
                Servings = request.Servings <= 0 ? 1 : request.Servings
            };
        }

        private string BuildPrompt(RecipeRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("את/ה עוזר שף מקצועי. צור מתכון על בסיס רשימת הרכיבים וההעדפות, והחזר את התוצאה כ-JSON בדיוק לפי הסכמה הבאה:");
            sb.AppendLine("{");
            sb.AppendLine("  \"Title\": \"שם המתכון\",");
            sb.AppendLine("  \"Description\": \"תיאור מליצי קצר על המנה (2–3 משפטים)\",");
            sb.AppendLine("  \"Ingredients\": [\"רכיב 1\", \"רכיב 2\"],");
            sb.AppendLine("  \"Steps\": [\"שלב 1\", \"שלב 2\"],");
            sb.AppendLine("  \"Nutrition\": { \"Calories\": 250, \"ProteinGrams\": 10.0, \"FatGrams\": 5.0, \"CarbsGrams\": 40.0 },");
            sb.AppendLine("  \"ImageDescription\": \"תיאור קצר של המראה\",");
            sb.AppendLine("  \"Servings\": 1");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("**חשוב מאוד**: כל הטקסט (Title, Description, Ingredients, Steps, ImageDescription) חייב להיות בעברית!");
            sb.AppendLine("השב רק עם JSON, ללא הסבר נוסף.");
            sb.AppendLine();
            sb.AppendLine("רכיבים זמינים:");
            foreach (var ing in request.Ingredients)
                sb.AppendLine("- " + ing);

            if (request.Preferences != null)
            {
                sb.AppendLine();
                sb.AppendLine("דרישות תזונתיות:");
                if (request.Preferences.Vegetarian)
                    sb.AppendLine("- צמחוני");
                if (request.Preferences.Vegan)
                    sb.AppendLine("- טבעוני (ללא מוצרים מן החי)");
                if (request.Preferences.GlutenFree)
                    sb.AppendLine("- ללא גלוטן");
                if (request.Preferences.MaxCalories.HasValue && request.Preferences.MaxCalories.Value > 0)
                    sb.AppendLine($"- מקסימום {request.Preferences.MaxCalories.Value} קלוריות למנה");
            }

            sb.AppendLine();
            sb.AppendLine($"מספר מנות: {(request.Servings <= 0 ? 1 : request.Servings)}");
            sb.AppendLine();
            sb.AppendLine("צור סט ריאליסטי של שלבים והערכת תזונה פשוטה.");
            sb.AppendLine();
            sb.AppendLine("מותר לך להוסיף עד שני רכיבים משלך אם לדעתך הם משפרים את הטעם, המרקם או הריח, בתנאי שהם מתאימים לסגנון ולמגבלות התזונתיות.");
            sb.AppendLine("הוסף גם תיאור מליצי קצר של המנה (2–3 משפטים) שיהיה מזמין וציורי, מיד אחרי שם המתכון.");
            sb.AppendLine("אל תשתמש ברכיבים חריגים או לא זמינים – רק חומרים נפוצים במטבח ביתי.");
            sb.AppendLine();
            sb.AppendLine("שוב - כל הטקסט בתשובה חייב להיות בעברית בלבד!");

            return sb.ToString();
        }
    }
}
