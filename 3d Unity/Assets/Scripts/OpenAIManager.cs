using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class OpenAIManager : MonoBehaviour
{
    [SerializeField] private string apiKey = "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 "; // Store this securely!
    [SerializeField] private string model = "gpt-3.5-turbo"; // Can change to gpt-4 if you have access
    private const string API_URL = "https://api.openai.com/v1/chat/completions";

    [Header("Fact Variety Settings")]
    [SerializeField] private bool useSeedForVariety = true;
    [SerializeField] private float baseTemperature = 0.5f; // Reduced temperature for more consistent output
    [SerializeField] private int maxResponseTokens = 60; // Limit response length

    // Store previously shown facts to avoid repetition
    private List<string> previousFacts = new List<string>();

    // Singleton pattern for easy access
    public static OpenAIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to get a Jupiter fact from OpenAI
    public void GetJupiterFact(Action<string> onFactReceived, int factIndex = 0)
    {
        StartCoroutine(RequestJupiterFact(onFactReceived, factIndex));
    }

    private IEnumerator RequestJupiterFact(Action<string> onFactReceived, int factIndex)
    {
        Debug.Log("OpenAIManager: Getting Jupiter fact #" + factIndex);

        // Create a unique seed based on the fact index
        int seed = factIndex + 1000;

        // Vary temperature slightly based on fact index to get more diversity
        // Using smaller variations to prevent extreme outputs
        float temperature = baseTemperature + (factIndex * 0.02f) % 0.2f;

        // Create request body with very specific format instructions
        string systemPrompt = "You are Captain Cosmic, an educational space guide for children ages 8-10. " +
            "FORMAT REQUIREMENT: Every response MUST start with exactly 'Analysing data: ' followed by a single interesting fact about Jupiter. " +
            "Do not add any other phrases or words at the start or end. " +
            "Keep facts concise (10-15 words) and engaging but not too long. " +
            "Use standard spelling and clear language appropriate for school children. " +
            "Never elongate words or use weird formatting." +
            "Never use emoji's or anything that is not text.";

        // Add specific themes based on the fact index
        switch (factIndex % 5)
        {
            case 0:
                systemPrompt += " Focus on Jupiter's SIZE or MASS in your fact.";
                break;
            case 1:
                systemPrompt += " Focus on Jupiter's MOONS in your fact.";
                break;
            case 2:
                systemPrompt += " Focus on Jupiter's STORMS or ATMOSPHERE in your fact.";
                break;
            case 3:
                systemPrompt += " Focus on Jupiter's CORE in your fact.";
                break;
            case 4:
                systemPrompt += " Focus on Jupiter's MAGNETIC FIELD or unique properties in your fact.";
                break;
        }

        // Example response to further clarify the format
        systemPrompt += " Example good response: 'Analysing data: Jupiter is so huge that more than 1,300 Earths could fit inside it!' " +
            "Do NOT repeat these facts: " + string.Join(", ", previousFacts);

        string jsonRequest = "{" +
            "\"model\": \"" + model + "\"," +
            "\"messages\": [" +
                "{\"role\": \"system\", \"content\": \"" + systemPrompt + "\"}," +
                "{\"role\": \"user\", \"content\": \"Tell me a Jupiter fact for children.\"}" +
            "]," +
            "\"max_tokens\": " + maxResponseTokens + "," +
            "\"temperature\": " + temperature + "," +
            "\"presence_penalty\": 0.5," + // Added to discourage repetitive patterns
            "\"frequency_penalty\": 0.5"; // Added to encourage more varied word choice

        // Add seed for consistent but different results if enabled
        if (useSeedForVariety)
        {
            jsonRequest += ",\"seed\": " + seed;
        }

        jsonRequest += "}";

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log("OpenAIManager: Sending request for fact #" + factIndex);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("OpenAIManager: API Error: " + request.error);
                Debug.LogError("OpenAIManager: Response: " + request.downloadHandler.text);
                onFactReceived?.Invoke("Analysing data: My cosmic database seems to be offline. Let's try again later!");
            }
            else
            {
                Debug.Log("OpenAIManager: Response received for fact #" + factIndex);
                string responseJson = request.downloadHandler.text;

                try
                {
                    // Extract content
                    string content = ExtractContentFromResponseV2(responseJson);
                    Debug.Log("OpenAIManager: Extracted content: " + content);

                    // Verify and fix the format if needed
                    string verifiedContent = VerifyAndFixFactFormat(content);

                    // Add to previous facts list to avoid repetition
                    // Only store the actual fact part without the prefix
                    string factOnly = verifiedContent.StartsWith("Analysing data: ") ?
                        verifiedContent.Substring("Analysing data: ".Length) : verifiedContent;

                    previousFacts.Add(factOnly);
                    if (previousFacts.Count > 10) // Keep list manageable
                    {
                        previousFacts.RemoveAt(0);
                    }

                    onFactReceived?.Invoke(verifiedContent);
                }
                catch (Exception e)
                {
                    Debug.LogError("OpenAIManager: Error parsing response: " + e.Message);
                    onFactReceived?.Invoke("Analysing data: My cosmic fact database needs recalibration!");
                }
            }
        }
    }

    // Verify the format and fix it if needed
    private string VerifyAndFixFactFormat(string content)
    {
        // Default fallback fact if there are serious issues
        string fallbackFact = "Analysing data: Jupiter is the largest planet in our solar system, with a diameter of 86,881 miles!";

        // Reject empty or very short content
        if (string.IsNullOrEmpty(content) || content.Length < 20)
        {
            Debug.LogWarning("OpenAIManager: Content too short or empty, using fallback");
            return fallbackFact;
        }

        // Check if content contains problematic patterns
        if (ContainsProblematicPatterns(content))
        {
            Debug.LogWarning("OpenAIManager: Received potentially problematic content, using fallback");
            return fallbackFact;
        }

        // Clean up content of any unusual characters that might cause display issues
        content = CleanupContent(content);

        // Check for the duplicate prefix issue
        if (content.StartsWith("Analysing data: Analysing data:", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("OpenAIManager: Fixing duplicated prefix");
            content = "Analysing data:" + content.Substring("Analysing data: Analysing data:".Length);
        }

        // Ensure it starts with the required prefix (only if it doesn't already)
        if (!content.StartsWith("Analysing data:", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("OpenAIManager: Adding missing prefix to: " + content);
            content = "Analysing data: " + content;
        }

        // Normalize capitalization of the prefix
        if (content.StartsWith("analysing data:", StringComparison.OrdinalIgnoreCase))
        {
            content = "Analysing data:" + content.Substring("analysing data:".Length);
        }

        // Ensure consistent space after colon
        if (content.StartsWith("Analysing data:") && !content.StartsWith("Analysing data: "))
        {
            content = "Analysing data: " + content.Substring("Analysing data:".Length).TrimStart();
        }

        // Limit length to prevent UI problems
        if (content.Length > 150)
        {
            content = content.Substring(0, 147) + "...";
        }

        return content;
    }

    // Helper method to clean up content
    private string CleanupContent(string content)
    {
        // First replace common problematic sequences
        content = content.Replace("\n", " ")
                         .Replace("\r", " ")
                         .Replace("\t", " ")
                         .Replace("\\", "")
                         .Replace("  ", " ");  // Replace double spaces

        // Then build a clean string with only allowed characters
        StringBuilder cleanContent = new StringBuilder();
        foreach (char c in content)
        {
            // Only allow standard ASCII printable characters, excluding control characters
            if ((c >= 32 && c <= 126) || c == ' ')
            {
                cleanContent.Append(c);
            }
        }

        return cleanContent.ToString();
    }

    // Expanded check for problematic patterns
    private bool ContainsProblematicPatterns(string text)
    {
        // Check for any character repeated more than 3 times in a row (except periods)
        for (int i = 0; i < text.Length - 3; i++)
        {
            if (text[i] != '.' && text[i] == text[i + 1] && text[i] == text[i + 2] && text[i] == text[i + 3])
                return true;
        }

        // Check for very short response which might indicate an error
        if (text.Length < 20)
            return true;

        // Check for non-ASCII characters which might cause rendering issues
        foreach (char c in text)
        {
            if (c > 126 || c < 32)
                return true;
        }

        // Check for JSON formatting errors that might have slipped through
        if (text.Contains("{") || text.Contains("}") || text.Contains("\\\""))
            return true;

        // Check for English language
        string[] commonEnglishWords = { "the", "is", "a", "are", "and", "in", "of", "to", "has", "with", "jupiter" };
        string lowerText = text.ToLower();
        bool containsEnglish = false;

        foreach (string word in commonEnglishWords)
        {
            if (lowerText.Contains(" " + word + " ") || lowerText.StartsWith(word + " ") || lowerText.EndsWith(" " + word))
            {
                containsEnglish = true;
                break;
            }
        }

        return !containsEnglish;
    }

    private string ExtractContentFromResponseV2(string jsonResponse)
    {
        // Find the specific pattern in the OpenAI response
        int choicesIndex = jsonResponse.IndexOf("\"choices\"");
        if (choicesIndex < 0)
        {
            Debug.LogError("OpenAIManager: No 'choices' field found in response");
            return "Analysing data: Jupiter has a mysterious data structure!";
        }

        int messageIndex = jsonResponse.IndexOf("\"message\"", choicesIndex);
        if (messageIndex < 0)
        {
            Debug.LogError("OpenAIManager: No 'message' field found in response");
            return "Analysing data: Jupiter's data signals are scrambled!";
        }

        int contentIndex = jsonResponse.IndexOf("\"content\"", messageIndex);
        if (contentIndex < 0)
        {
            Debug.LogError("OpenAIManager: No 'content' field found in response");
            return "Analysing data: Jupiter's information packets are corrupted!";
        }

        // Find the start of the actual content (after the "content": ")
        int contentValueStart = jsonResponse.IndexOf('"', contentIndex + 10) + 1;
        if (contentValueStart <= 0)
        {
            Debug.LogError("OpenAIManager: Content value start not found");
            return "Analysing data: Jupiter is sending incomplete transmissions!";
        }

        // Find the end of the content (the next unescaped quote)
        int contentValueEnd = contentValueStart;
        bool escaped = false;

        while (contentValueEnd < jsonResponse.Length)
        {
            char c = jsonResponse[contentValueEnd];
            if (c == '\\')
            {
                escaped = !escaped;
            }
            else if (c == '"' && !escaped)
            {
                break;
            }
            else
            {
                escaped = false;
            }
            contentValueEnd++;
        }

        if (contentValueEnd >= jsonResponse.Length)
        {
            Debug.LogError("OpenAIManager: Content value end not found");
            return "Analysing data: Jupiter's message was cut off unexpectedly!";
        }

        // Extract the content
        string content = jsonResponse.Substring(contentValueStart, contentValueEnd - contentValueStart);

        // Unescape JSON string escapes
        content = content.Replace("\\\"", "\"")
                         .Replace("\\n", "\n")
                         .Replace("\\r", "\r")
                         .Replace("\\t", "\t")
                         .Replace("\\\\", "\\");

        return content;
    }

    // Method to use local facts when API fails
    public void UseLocalFacts(Action<string> onFactReceived, int factIndex = 0)
    {
        string[] jupiterFacts = new string[]
        {
            "Analysing data: Jupiter is so massive that it's more than twice as heavy as all the other planets combined!",
            "Analysing data: The Great Red Spot on Jupiter is a giant storm that's been raging for over 300 years!",
            "Analysing data: Jupiter has at least 79 moons orbiting around it - that's like having 79 friends following you everywhere!",
            "Analysing data: A day on Jupiter only lasts about 10 hours, making it the fastest spinning planet in our solar system!",
            "Analysing data: Jupiter's stripes are actually massive storms with fierce winds that reach speeds of over 400 miles per hour!",
            "Analysing data: Jupiter acts like a cosmic vacuum cleaner, protecting Earth by attracting comets and asteroids with its strong gravity!",
            "Analysing data: If Jupiter had been about 80 times more massive, it would have become a star instead of a planet!",
            "Analysing data: Jupiter gives off more heat than it receives from the Sun - it's like a mini-star that never quite ignited!",
            "Analysing data: The strongest lightning in our solar system is on Jupiter - it's thousands of times more powerful than Earth's lightning!"
        };

        // Use the factIndex to select a specific fact, or fall back to random if out of range
        int index = factIndex % jupiterFacts.Length;
        string fact = jupiterFacts[index];
        onFactReceived?.Invoke(fact);
    }
}