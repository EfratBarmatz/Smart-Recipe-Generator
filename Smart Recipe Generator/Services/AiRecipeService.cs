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
            // Read AI endpoint and key from configuration
            var endpoint = _config["AI:Endpoint"];
            var apiKey = _config["AI:ApiKey"]; // optional

            // Log provider/endpoint presence (do not log secrets)
            var provider = (_config["AI:Provider"] ?? string.Empty).ToLowerInvariant();
            _logger.LogInformation("AI provider={provider}; endpointConfigured={hasEndpoint}; apiKeyPresent={hasKey}", provider, !string.IsNullOrWhiteSpace(endpoint), !string.IsNullOrWhiteSpace(apiKey));

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogWarning("AI endpoint not configured. Falling back to local placeholder.");
                // Fallback to existing naive generator if no endpoint configured
                var fallback = new RecipeService();
                return await fallback.GenerateRecipeAsync(request);
            }

            // Build a clear prompt asking the model to return JSON in the expected schema
            var prompt = BuildPrompt(request);
            var model = _config["AI:Model"] ?? string.Empty;

            try
            {
                var client = _httpClientFactory.CreateClient("ai-client");

                using var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint);

                if (provider == "huggingface")
                {
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    var body = new { inputs = prompt };
                    var jsonBody = JsonSerializer.Serialize(body);
                    httpReq.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }
                else if (provider == "gemini" || provider == "google" || (!string.IsNullOrEmpty(endpoint) && endpoint.Contains("generativelanguage.googleapis.com")))
                {
                    // Google Generative Language (Gemini) - proper request structure

                    // If API key provided, append it as query parameter
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        var separator = endpoint.Contains("?") ? "&" : "?";
                        httpReq.RequestUri = new Uri(endpoint + separator + "key=" + Uri.EscapeDataString(apiKey));
                    }
                    else
                    {
                        // Try Service Account authentication
                        var saPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

                        if (!string.IsNullOrWhiteSpace(saPath) && File.Exists(saPath))
                        {
                            try
                            {
                                var googleCred = GoogleCredential.FromFile(saPath)
                                    .CreateScoped("https://www.googleapis.com/auth/generative-language.retriever");

                                var underlying = googleCred.UnderlyingCredential;
                                if (underlying != null)
                                {
                                    string accessToken = null;
                                    try
                                    {
                                        accessToken = await underlying.GetAccessTokenForRequestAsync();
                                    }
                                    catch (Exception exToken)
                                    {
                                        _logger.LogWarning(exToken, "Failed to get access token from underlying credential.");
                                    }

                                    if (!string.IsNullOrWhiteSpace(accessToken))
                                    {
                                        httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to acquire access token from Service Account JSON.");
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(apiKey))
                        {
                            _logger.LogWarning("No API key or Service Account found for Gemini; call may fail.");
                        }

                        // Ensure request URI is set
                        if (httpReq.RequestUri == null)
                        {
                            httpReq.RequestUri = new Uri(endpoint);
                        }
                    }

                    // Gemini API uses "contents" structure, not "prompt"
                    var body = new
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

                    var jsonBody = JsonSerializer.Serialize(body);
                    httpReq.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }
                else
                {
                    // Fallback generic: send { inputs: prompt } and use bearer if provided
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    var body = new { inputs = prompt };
                    var jsonBody = JsonSerializer.Serialize(body);
                    httpReq.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                // Log request details for debugging
                _logger.LogInformation("Sending request to: {Endpoint}", httpReq.RequestUri);
                _logger.LogInformation("Request method: {Method}", httpReq.Method);

                var resp = await client.SendAsync(httpReq);

                // Check status before throwing exception
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    _logger.LogError("AI API returned {StatusCode}: {Error}", resp.StatusCode, errorContent);

                    // Fall back to placeholder instead of throwing
                    _logger.LogWarning("Falling back to placeholder due to API error.");
                    var fallback = new RecipeService();
                    return await fallback.GenerateRecipeAsync(request);
                }

                var respText = await resp.Content.ReadAsStringAsync();

                // Try to parse responseText as JSON matching RecipeResponse
                try
                {
                    // Some inference APIs return { "generated_text": "..." } or plain text
                    // If response is wrapped, try to detect and extract JSON inside
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // 1) Try direct deserialization into RecipeResponse
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<RecipeResponse>(respText, options);
                        if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Title))
                        {
                            return parsed;
                        }
                    }
                    catch { /* ignore */ }

                    // 2) Try provider-specific extraction heuristics (e.g., Gemini returns candidates)
                    try
                    {
                        using var doc = JsonDocument.Parse(respText);
                        // look for candidates array
                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
                        {
                            var first = candidates[0];
                            // check possible fields
                            if (first.TryGetProperty("output", out var outp) && outp.ValueKind == JsonValueKind.String)
                            {
                                respText = outp.GetString() ?? respText;
                            }
                            else if (first.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Object)
                            {
                                // Gemini structure: candidates[0].content.parts[0].text
                                if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                                {
                                    var sbc = new StringBuilder();
                                    foreach (var el in parts.EnumerateArray())
                                    {
                                        if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("text", out var t))
                                        {
                                            sbc.Append(t.GetString());
                                        }
                                        else if (el.ValueKind == JsonValueKind.String)
                                        {
                                            sbc.Append(el.GetString());
                                        }
                                    }
                                    if (sbc.Length > 0) respText = sbc.ToString();
                                }
                            }
                            else if (first.TryGetProperty("text", out var tprop) && tprop.ValueKind == JsonValueKind.String)
                            {
                                respText = tprop.GetString() ?? respText;
                            }
                        }
                        // also check top-level fields commonly used
                        else if (doc.RootElement.TryGetProperty("generated_text", out var gen) && gen.ValueKind == JsonValueKind.String)
                        {
                            respText = gen.GetString() ?? respText;
                        }
                        else if (doc.RootElement.TryGetProperty("output", out var outTop) && outTop.ValueKind == JsonValueKind.String)
                        {
                            respText = outTop.GetString() ?? respText;
                        }
                    }
                    catch { /* ignore parse errors and fall back */ }

                    // 3) If the returned text contains JSON, try to extract JSON substring and parse it
                    var firstBrace = respText.IndexOf('{');
                    var lastBrace = respText.LastIndexOf('}');
                    if (firstBrace >= 0 && lastBrace > firstBrace)
                    {
                        var jsonSub = respText.Substring(firstBrace, lastBrace - firstBrace + 1);
                        try
                        {
                            var parsed2 = JsonSerializer.Deserialize<RecipeResponse>(jsonSub, options);
                            if (parsed2 != null)
                            {
                                return parsed2;
                            }
                        }
                        catch { /* ignore */ }
                    }

                    // 4) As a last resort, return the raw text in steps
                    return new RecipeResponse
                    {
                        Title = "תוצאה שנוצרה על ידי AI",
                        Ingredients = request.Ingredients,
                        Steps = new System.Collections.Generic.List<string> { respText },
                        Servings = request.Servings <= 0 ? 1 : request.Servings
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse AI response. Returning raw output.");
                    return new RecipeResponse
                    {
                        Title = "תוצאה שנוצרה על ידי AI",
                        Ingredients = request.Ingredients,
                        Steps = new System.Collections.Generic.List<string> { respText },
                        Servings = request.Servings <= 0 ? 1 : request.Servings
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling AI endpoint. Falling back to placeholder.");
                var fallback = new RecipeService();
                return await fallback.GenerateRecipeAsync(request);
            }
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
            {
                sb.AppendLine("- " + ing);
            }

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