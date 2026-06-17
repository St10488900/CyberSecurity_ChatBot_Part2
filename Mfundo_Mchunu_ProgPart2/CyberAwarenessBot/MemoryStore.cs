namespace CyberAwarenessBot
{
    /// <summary>
    /// Holds all user-specific data for the current chat session.
    /// Uses a Queue for ordered conversation history and a Stack for
    /// quick access to the most recent user messages.
    /// </summary>
    public class MemoryStore
    {
        // ── Identity ───────────────────────────────────────────────────────
        public string UserName  { get; private set; } = string.Empty;
        public bool   HasName   => !string.IsNullOrWhiteSpace(UserName);

        // ── Preference ─────────────────────────────────────────────────────
        public string FavouriteTopic    { get; private set; } = string.Empty;
        public bool   HasFavouriteTopic => !string.IsNullOrWhiteSpace(FavouriteTopic);

        // ── Conversation stage flags ───────────────────────────────────────
        public bool GreetingDone { get; set; } = false;
        public bool NameAsked    { get; set; } = false;
        public bool TopicAsked   { get; set; } = false;

        // ── History: Queue (FIFO) — keeps last N exchanges in order ────────
        private readonly Queue<string> _history = new();
        private const int MaxHistory = 50;

        // ── Recent messages: Stack (LIFO) — most recent message on top ─────
        private readonly Stack<string> _recentUserMessages = new();
        private const int MaxStack = 10;

        // ──────────────────────────────────────────────────────────────────

        public void RememberName(string name)          => UserName       = name.Trim();
        public void RememberFavouriteTopic(string topic) => FavouriteTopic = topic.Trim();

        /// <summary>Returns a personalised name prefix when the name is known.</summary>
        public string GetNamePrefix()
            => HasName ? $"{UserName}, " : string.Empty;

        /// <summary>Appends an entry to the conversation history Queue.</summary>
        public void AddToHistory(string entry)
        {
            if (_history.Count >= MaxHistory)
                _history.Dequeue(); // Drop oldest when full
            _history.Enqueue(entry);
        }

        /// <summary>Pushes the latest user message onto the recent-messages Stack.</summary>
        public void PushUserMessage(string message)
        {
            if (_recentUserMessages.Count >= MaxStack)
            {
                var temp = _recentUserMessages.ToArray();
                _recentUserMessages.Clear();
                foreach (var item in temp.Take(MaxStack - 1))
                    _recentUserMessages.Push(item);
            }
            _recentUserMessages.Push(message);
        }

        /// <summary>Reads the most recent user message without removing it.</summary>
        public string PeekLastUserMessage()
            => _recentUserMessages.Count > 0 ? _recentUserMessages.Peek() : string.Empty;

        public IReadOnlyCollection<string> GetHistory() => _history;

        public int MessageCount => _history.Count;

        public void ClearAll()
        {
            UserName       = string.Empty;
            FavouriteTopic = string.Empty;
            GreetingDone   = false;
            NameAsked      = false;
            TopicAsked     = false;
            _history.Clear();
            _recentUserMessages.Clear();
        }
    }
}
