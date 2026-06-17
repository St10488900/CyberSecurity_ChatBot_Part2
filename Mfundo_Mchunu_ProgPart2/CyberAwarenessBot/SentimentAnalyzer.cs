namespace CyberAwarenessBot
{
    /// <summary>
    /// Analyses user message tone and surfaces an empathetic acknowledgement
    /// before the chatbot's main response.
    /// </summary>
    public enum Sentiment
    {
        Neutral,
        Worried,
        Frustrated,
        Curious
    }

    public class SentimentAnalyzer
    {
        // ── Keyword sets per sentiment ─────────────────────────────────────
        private static readonly string[] WorriedWords    = { "worried", "scared", "afraid", "anxious", "nervous", "concerned", "fear", "terrified", "unsafe" };
        private static readonly string[] FrustratedWords = { "frustrated", "annoyed", "angry", "confused", "tired", "overwhelmed", "useless", "stupid", "hate", "difficult" };
        private static readonly string[] CuriousWords    = { "curious", "interested", "tell me more", "learn", "want to know", "how does", "explain", "teach me", "what is", "show me" };

        // ──────────────────────────────────────────────────────────────────

        /// <summary>Analyses input and returns the most likely sentiment.</summary>
        public Sentiment DetectSentiment(string input)
        {
            string lower = input.ToLower();
            if (ContainsAny(lower, WorriedWords))    return Sentiment.Worried;
            if (ContainsAny(lower, FrustratedWords)) return Sentiment.Frustrated;
            if (ContainsAny(lower, CuriousWords))    return Sentiment.Curious;
            return Sentiment.Neutral;
        }

        /// <summary>
        /// Produces an empathetic opening line tailored to the detected sentiment.
        /// </summary>
        public string GetSentimentAcknowledgement(Sentiment sentiment, string? userName = null)
        {
            string name = !string.IsNullOrWhiteSpace(userName) ? $" {userName}" : string.Empty;

            return sentiment switch
            {
                Sentiment.Worried    => $"😌 It's completely understandable to feel concerned{name} — the online world can seem daunting. Here's something practical that can help right now:",
                Sentiment.Frustrated => $"🤝 I hear you{name}, this stuff can feel overwhelming. Let me break it down simply:",
                Sentiment.Curious    => $"🌟 Excellent{name}! Curiosity is the best first step to staying safe. Here's what you need to know:",
                _                    => string.Empty
            };
        }

        /// <summary>Returns a display label for the UI sentiment indicator.</summary>
        public string GetSentimentLabel(Sentiment sentiment) => sentiment switch
        {
            Sentiment.Worried    => "😟 worried",
            Sentiment.Frustrated => "😤 frustrated",
            Sentiment.Curious    => "🤔 curious",
            _                    => "😐 neutral"
        };

        // ──────────────────────────────────────────────────────────────────
        private static bool ContainsAny(string input, string[] words)
            => words.Any(w => input.Contains(w));
    }
}
