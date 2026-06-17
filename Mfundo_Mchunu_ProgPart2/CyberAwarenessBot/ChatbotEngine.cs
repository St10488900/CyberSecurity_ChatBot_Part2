namespace CyberAwarenessBot
{
    /// <summary>
    /// Core engine that orchestrates MemoryStore, ResponseManager,
    /// SentimentAnalyzer, and the delegate pipeline to generate responses.
    /// </summary>
    public class ChatbotEngine
    {
        // ── Dependencies ───────────────────────────────────────────────────
        private readonly MemoryStore       _memory;
        private readonly ResponseManager   _responses;
        private readonly SentimentAnalyzer _sentiment;

        // ── Delegates configured at construction ───────────────────────────
        private readonly FormatBotMessage     _formatter;
        private readonly ValidateUserInput    _validator;
        private readonly LogConversationEntry _logger;

        // ── Sentiment exposed for UI display ──────────────────────────────
        public Sentiment LastSentiment { get; private set; } = Sentiment.Neutral;

        // ──────────────────────────────────────────────────────────────────
        public ChatbotEngine(MemoryStore memory)
        {
            _memory    = memory;
            _responses = new ResponseManager();
            _sentiment = new SentimentAnalyzer();

            // Connect delegates
            _formatter = DelegateHandlers.PersonalisedBotFormatter;
            _validator = DelegateHandlers.EmptyInputValidator;
            _logger    = DelegateHandlers.DebugLogger;
        }

        // ══════════════════════════════════════════════════════════════════
        //  MAIN ENTRY POINT
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Accepts raw user input and returns the bot's formatted reply.
        /// </summary>
        public string GetResponse(string rawInput)
        {
            // 1. Validate via delegate
            string? validationError = _validator(rawInput);
            if (validationError != null)
            {
                string errMsg = _memory.HasName
                    ? $"{validationError} {_memory.UserName}."
                    : validationError;
                return Format(errMsg);
            }

            // 2. Trim and log
            string input = rawInput.Trim();
            _memory.PushUserMessage(input);
            _memory.AddToHistory($"User: {input}");
            _logger("User", input);

            // 3. Detect sentiment
            LastSentiment = _sentiment.DetectSentiment(input);

            // 4. Name collection stage
            if (!_memory.HasName)
                return HandleNameCollection(input);

            // 5. Topic collection stage
            if (!_memory.TopicAsked)
                return HandleTopicCollection(input);

            // 6. Standard response flow
            return HandleNormalInput(input);
        }

        // ══════════════════════════════════════════════════════════════════
        //  GREETING  (called once on startup)
        // ══════════════════════════════════════════════════════════════════
        public string GetGreetingMessage()
        {
            _memory.GreetingDone = true;
            return Format("Hey there! Welcome to ShieldBot — your personal cybersecurity guide. I'm here to help you navigate the digital world safely. What should I call you?");
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private string HandleNameCollection(string input)
        {
            string name = StripNamePrefix(input);

            if (name.Length < 1 || name.Length > 40)
            {
                _memory.NameAsked = true;
                return Format("Hmm, I didn't catch that. Could you share your name with me?");
            }

            _memory.RememberName(name);
            _memory.NameAsked = true;

            string response = $"Great to meet you, {_memory.UserName}! Which cybersecurity topic would you like to explore first? "
                            + "(Choose from: passwords, phishing, safe browsing, scams, malware, privacy, 2FA, or updates)";
            _memory.AddToHistory($"Bot: {response}");
            _logger("Bot", response);
            return Format(response);
        }

        private string HandleTopicCollection(string input)
        {
            string? topicResponse = _responses.GetKeywordResponse(input);
            string  topic         = DetectTopicLabel(input);

            _memory.TopicAsked = true;

            if (!string.IsNullOrWhiteSpace(topic))
            {
                _memory.RememberFavouriteTopic(topic);
                string response = $"Noted! I'll keep {topic} in mind throughout our chat, {_memory.UserName}. "
                               + (topicResponse ?? _responses.GetRandomTip())
                               + $"\n\nI'll circle back to {topic} as we go. What would you like to know next?";
                return Format(response);
            }

            _memory.RememberFavouriteTopic(input.Length > 30 ? input[..30] : input);
            return Format($"Thanks for sharing that, {_memory.UserName}! Feel free to ask me anything about staying safe online. "
                        + "Popular topics include passwords, phishing, scams, and safe browsing.");
        }

        private string HandleNormalInput(string input)
        {
            string lower = input.ToLower();

            // ── Predefined conversational queries ─────────────────────────
            if (ContainsAny(lower, "how are you", "how r u", "how are u"))
                return Format($"All systems running and threats being monitored! 😊 How can I assist you today, {_memory.UserName}?");

            if (ContainsAny(lower, "what's your purpose", "your purpose", "what do you do", "why are you here"))
                return Format("🛡️ My job is to raise cybersecurity awareness — helping you understand threats like phishing, weak passwords, and online scams so you can stay protected!");

            if (ContainsAny(lower, "what can i ask", "what can you do", "help", "topics"))
                return Format("💬 Here's what you can ask me about:\n• Passwords & passphrases\n• Phishing & scam emails\n• Safe browsing & HTTPS\n• Online scams & fraud\n• Malware, viruses & ransomware\n• Privacy & data protection\n• Two-factor authentication (2FA)\n• Software updates & patches");

            if (ContainsAny(lower, "tell me more", "more info", "give me another tip", "explain more", "what else"))
                return HandleFollowUp();

            // ── Keyword + sentiment response ───────────────────────────────
            string? keywordResponse = _responses.GetKeywordResponse(input);

            if (keywordResponse != null)
            {
                string sentimentOpener = _sentiment.GetSentimentAcknowledgement(LastSentiment, _memory.UserName);
                string favouriteSuffix = BuildFavouriteSuffix();

                bool isPhishing = ContainsAny(lower, "phish", "phishing");
                string topicContent = isPhishing ? _responses.GetRandomPhishingTip() : keywordResponse;

                string full = string.IsNullOrEmpty(sentimentOpener)
                    ? $"{topicContent}{favouriteSuffix}"
                    : $"{sentimentOpener}\n\n{topicContent}{favouriteSuffix}";

                return Format(full);
            }

            // ── Sentiment without keyword ──────────────────────────────────
            if (LastSentiment != Sentiment.Neutral)
            {
                string ack = _sentiment.GetSentimentAcknowledgement(LastSentiment, _memory.UserName);
                string tip = _responses.GetRandomTip();
                return Format($"{ack}\n\n{tip}");
            }

            // ── Gibberish check ────────────────────────────────────────────
            if (IsGibberish(input))
                return Format("🤔 That one's got me stumped. Could you try rephrasing? Ask about passwords, phishing, or safe browsing.");

            // ── Default fallback ───────────────────────────────────────────
            return Format($"I'm not quite sure what you mean, {_memory.UserName}. Try asking about passwords, phishing, scams, malware, privacy, 2FA, or safe browsing!");
        }

        private string HandleFollowUp()
        {
            if (_memory.HasFavouriteTopic)
                return Format($"Since you're keen on {_memory.FavouriteTopic}, {_memory.UserName}, here's an extra tip: "
                            + _responses.GetRandomTip());

            return Format($"Here's a bonus security tip for you, {_memory.UserName}:\n\n{_responses.GetRandomTip()}");
        }

        // ──────────────────────────────────────────────────────────────────
        private string Format(string message)
        {
            string formatted = _formatter(message, _memory.UserName);
            _memory.AddToHistory($"Bot: {message}");
            _logger("Bot", message);
            return formatted;
        }

        private string BuildFavouriteSuffix()
        {
            if (!_memory.HasFavouriteTopic) return string.Empty;
            return $"\n\n📌 Keeping your interest in {_memory.FavouriteTopic} in mind — I'll flag related tips!";
        }

        private static bool ContainsAny(string input, params string[] terms)
            => terms.Any(t => input.Contains(t, StringComparison.OrdinalIgnoreCase));

        private static string StripNamePrefix(string input)
        {
            string[] prefixes = { "i'm ", "i am ", "my name is ", "it's ", "its ", "call me ", "name's " };
            string lower = input.ToLower().Trim();
            foreach (var p in prefixes)
            {
                if (lower.StartsWith(p))
                    return input[p.Length..].Trim();
            }
            return input.Trim();
        }

        private static string DetectTopicLabel(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains("password"))                              return "passwords";
            if (lower.Contains("phish"))                                 return "phishing";
            if (lower.Contains("scam") || lower.Contains("fraud"))       return "scams";
            if (lower.Contains("malware") || lower.Contains("virus"))    return "malware";
            if (lower.Contains("privacy"))                               return "privacy";
            if (lower.Contains("2fa") || lower.Contains("two factor") || lower.Contains("mfa")) return "2FA";
            if (lower.Contains("update") || lower.Contains("patch"))     return "software updates";
            if (lower.Contains("brows") || lower.Contains("https"))      return "safe browsing";
            return string.Empty;
        }

        private static bool IsGibberish(string input)
        {
            if (input.Length < 3) return true;
            int vowels  = input.Count(c => "aeiouAEIOU".Contains(c));
            int letters = input.Count(char.IsLetter);
            if (letters < 3) return true;
            double ratio = letters > 0 ? (double)vowels / letters : 0;
            return ratio < 0.10 && letters > 4;
        }
    }
}
