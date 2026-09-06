using Core.DTO;
using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Services
{
    public class GeminiTaskBreakdownService : IAiTaskBreakdownService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiTaskBreakdownService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<TaskBreakdownItemDto>> BreakdownDescriptionAsync(
            string description,
            List<(string Name, string? Specialty)> teamMembers)
        {
            var apiKey = _configuration["Gemini:ApiKey"]!;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={apiKey}";

            var teamSection = teamMembers.Count == 0
                ? "لا يوجد فريق محدد، لا تقترح أي اسم."
                : BuildTeamSection(teamMembers);

            var systemPrompt = $$"""
                أنت مساعد تقني متخصص في إدارة المشاريع. مهمتك تكسير وصف عام لميزة برمجية إلى مهام فرعية واضحة (Tasks).
                {{teamSection}}
                يجب أن يكون الرد بصيغة JSON فقط، بدون أي نص إضافي قبله أو بعده، وبدون علامات markdown مثل ```json.
                الصيغة المطلوبة بالضبط:
                {
                  "tasks": [
                    { "title": "عنوان قصير للمهمة", "description": "وصف تفصيلي", "priority": "Low|Medium|High|Critical", "suggestedAssigneeName": "اسم الشخص المقترح من القائمة أو null إذا لم تحدد" }
                  ]
                }
                قسّم الوصف إلى 3-7 مهام منطقية ومنفصلة، وحدد الأولوية المناسبة لكل مهمة.
                """;

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = $"{systemPrompt}\n\nالوصف المطلوب تكسيره:\n{description}" } } }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    responseMimeType = "application/json"
                }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException("تم تجاوز الحد المسموح لاستخدام خدمة الذكاء الاصطناعي. حاول مرة أخرى بعد قليل.");
                }

                throw new InvalidOperationException($"فشل الاتصال بخدمة الذكاء الاصطناعي: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return ParseGeminiResponse(responseBody);
        }

        private string BuildTeamSection(List<(string Name, string? Specialty)> teamMembers)
        {
            var lines = teamMembers.Select(m =>
                string.IsNullOrWhiteSpace(m.Specialty) ? m.Name : $"{m.Name} (تخصصه: {m.Specialty})");

            return $"فريق العمل المتاح: {string.Join(", ", lines)}. لكل مهمة، اقترح اسم الشخص الأنسب بناءً على تخصصه إن وُجد، أو بناءً على طبيعة المهمة إن لم يُحدد التخصص. لا تقترح أي اسم غير موجود في القائمة.";
        }

        private List<TaskBreakdownItemDto> ParseGeminiResponse(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var generatedText = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(generatedText))
                throw new InvalidOperationException("لم يتمكن الذكاء الاصطناعي من توليد رد صالح.");

            using var tasksDoc = JsonDocument.Parse(generatedText);
            var tasksArray = tasksDoc.RootElement.GetProperty("tasks");

            var result = new List<TaskBreakdownItemDto>();

            foreach (var taskElement in tasksArray.EnumerateArray())
            {
                string? suggestedName = null;
                if (taskElement.TryGetProperty("suggestedAssigneeName", out var nameElement) &&
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    suggestedName = nameElement.GetString();
                }

                result.Add(new TaskBreakdownItemDto
                {
                    Title = taskElement.GetProperty("title").GetString() ?? "بدون عنوان",
                    Description = taskElement.GetProperty("description").GetString() ?? string.Empty,
                    Priority = taskElement.GetProperty("priority").GetString() ?? "Medium",
                    SuggestedAssigneeName = suggestedName
                });
            }

            return result;
        }
    }
}