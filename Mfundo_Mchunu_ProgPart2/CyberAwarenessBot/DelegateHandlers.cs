namespace CyberAwarenessBot
{
    // ══════════════════════════════════════════════════════════════════════
    //  DELEGATE TYPE DEFINITIONS
    //
    //  Delegates decouple formatting, validation, and logging from the core
    //  engine — each concern can be swapped independently without touching
    //  ChatbotEngine or the UI layer.
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Signature for a function that formats a bot response before display.
    /// Implementations can add timestamps, name tags, emoji, etc.
    /// </summary>
    public delegate string FormatBotMessage(string rawMessage, string? userName);

    /// <summary>
    /// Signature for a function that validates user input.
    /// Returns null if the input is acceptable, or a descriptive error string.
    /// </summary>
    public delegate string? ValidateUserInput(string input);

    /// <summary>
    /// Signature for a function that logs a conversation entry.
    /// </summary>
    public delegate void LogConversationEntry(string role, string message);

    // ══════════════════════════════════════════════════════════════════════
    //  CONCRETE DELEGATE IMPLEMENTATIONS
    // ══════════════════════════════════════════════════════════════════════

    public static class DelegateHandlers
    {
        // ── FormatBotMessage ──────────────────────────────────────────────

        /// <summary>
        /// Basic formatter: prepends a time stamp and the bot label.
        /// </summary>
        public static FormatBotMessage StandardBotFormatter => (message, userName) =>
        {
            string time = DateTime.Now.ToString("HH:mm");
            return $"[{time}] 🤖 ShieldBot: {message}";
        };

        /// <summary>
        /// Personalised formatter: includes the user's name when available.
        /// This is the formatter wired into ChatbotEngine by default.
        /// </summary>
        public static FormatBotMessage PersonalisedBotFormatter => (message, userName) =>
        {
            string time   = DateTime.Now.ToString("HH:mm");
            string prefix = string.IsNullOrWhiteSpace(userName)
                ? "ShieldBot"
                : $"ShieldBot → {userName}";
            return $"[{time}] 🤖 {prefix}: {message}";
        };

        // ── ValidateUserInput ─────────────────────────────────────────────

        /// <summary>
        /// Rejects blank or whitespace-only input with a polite prompt.
        /// Returns null (valid) for any non-empty input.
        /// </summary>
        public static ValidateUserInput EmptyInputValidator => input =>
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please enter a message so I can assist you.";
            return null;
        };

        /// <summary>
        /// Length checker — currently a no-op (always returns null).
        /// Kept as a demonstration that validators can be swapped without
        /// changing ChatbotEngine.
        /// </summary>
        public static ValidateUserInput LengthWarningValidator => input => null;

        // ── LogConversationEntry ──────────────────────────────────────────

        /// <summary>
        /// Writes each exchange to the Debug output stream.
        /// In a production deployment this could be directed to a log file.
        /// </summary>
        public static LogConversationEntry DebugLogger => (role, message) =>
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] {role}: {message}");
        };
    }
}
