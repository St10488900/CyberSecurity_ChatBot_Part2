namespace CyberAwarenessBot
{
    /// <summary>
    /// Manages keyword-to-response mappings using a Dictionary&lt;string, string&gt;
    /// and maintains Lists of tip variations for random response selection.
    ///
    /// WHY Dictionary&lt;string, string&gt;?
    ///   O(1) average-case lookup means keyword matching stays fast even as the
    ///   topic set grows — far more efficient than scanning a List or array.
    ///   The List&lt;string&gt; collections enable random-index access for
    ///   varied, non-repetitive responses.
    /// </summary>
    public class ResponseManager
    {
        // ── Primary keyword dictionary ─────────────────────────────────────
        private readonly Dictionary<string, string> _keywordResponses;

        // ── Phishing tip pool (random selection) ───────────────────────────
        private readonly List<string> _phishingTips;

        // ── General cybersecurity tip pool ─────────────────────────────────
        private readonly List<string> _generalTips;

        private readonly Random _rng = new();

        // ──────────────────────────────────────────────────────────────────
        public ResponseManager()
        {
            _keywordResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"]        = "🔑 Every account deserves its own unique password — at least 14 characters mixing letters, numbers and symbols. A password manager takes the hassle out of remembering them all!",
                ["passwords"]       = "🔑 Every account deserves its own unique password — at least 14 characters mixing letters, numbers and symbols. A password manager takes the hassle out of remembering them all!",
                ["passphrase"]      = "🔑 Every account deserves its own unique password — at least 14 characters mixing letters, numbers and symbols. A password manager takes the hassle out of remembering them all!",

                ["phish"]           = "🎣 Phishing messages are designed to look legitimate. Always verify the sender's real email address and never click links in unexpected messages — go directly to the official website instead.",
                ["phishing"]        = "🎣 Phishing messages are designed to look legitimate. Always verify the sender's real email address and never click links in unexpected messages — go directly to the official website instead.",
                ["scam email"]      = "🎣 Phishing messages are designed to look legitimate. Always verify the sender's real email address and never click links in unexpected messages — go directly to the official website instead.",

                ["safe browsing"]   = "🌐 Always confirm the padlock icon in your browser's address bar. Avoid entering personal details on HTTP-only sites, and steer clear of public Wi-Fi when accessing banking or email.",
                ["browsing safely"] = "🌐 Always confirm the padlock icon in your browser's address bar. Avoid entering personal details on HTTP-only sites, and steer clear of public Wi-Fi when accessing banking or email.",
                ["https"]           = "🌐 Always confirm the padlock icon in your browser's address bar. Avoid entering personal details on HTTP-only sites, and steer clear of public Wi-Fi when accessing banking or email.",

                ["scam"]            = "⚠️ Scammers rely on pressure and panic. Pause, verify using an official contact number you look up yourself, and never transfer money or gift cards to anyone you haven't met.",
                ["scams"]           = "⚠️ Scammers rely on pressure and panic. Pause, verify using an official contact number you look up yourself, and never transfer money or gift cards to anyone you haven't met.",
                ["fraud"]           = "⚠️ Scammers rely on pressure and panic. Pause, verify using an official contact number you look up yourself, and never transfer money or gift cards to anyone you haven't met.",

                ["malware"]         = "🦠 Malicious software can steal data or hold your files hostage. Keep antivirus software current, only download from trusted sources, and back up your data at least weekly.",
                ["virus"]           = "🦠 Malicious software can steal data or hold your files hostage. Keep antivirus software current, only download from trusted sources, and back up your data at least weekly.",
                ["trojan"]          = "🦠 Malicious software can steal data or hold your files hostage. Keep antivirus software current, only download from trusted sources, and back up your data at least weekly.",
                ["ransomware"]      = "🦠 Malicious software can steal data or hold your files hostage. Keep antivirus software current, only download from trusted sources, and back up your data at least weekly.",

                ["privacy"]         = "🔒 Audit your app permissions every few months. Share the minimum information necessary and tighten social media privacy settings so strangers can't profile you.",
                ["private"]         = "🔒 Audit your app permissions every few months. Share the minimum information necessary and tighten social media privacy settings so strangers can't profile you.",
                ["data protection"] = "🔒 Audit your app permissions every few months. Share the minimum information necessary and tighten social media privacy settings so strangers can't profile you.",

                ["two factor"]      = "🔐 Two-factor authentication is one of the strongest defences you can enable. It stops 99.9% of account takeover attempts — turn it on wherever available.",
                ["2fa"]             = "🔐 Two-factor authentication is one of the strongest defences you can enable. It stops 99.9% of account takeover attempts — turn it on wherever available.",
                ["mfa"]             = "🔐 Two-factor authentication is one of the strongest defences you can enable. It stops 99.9% of account takeover attempts — turn it on wherever available.",

                ["update"]          = "🔄 Unpatched software is an open door for attackers. Enable automatic updates for your OS, browser, and all installed apps to stay protected.",
                ["updates"]         = "🔄 Unpatched software is an open door for attackers. Enable automatic updates for your OS, browser, and all installed apps to stay protected.",
                ["patch"]           = "🔄 Unpatched software is an open door for attackers. Enable automatic updates for your OS, browser, and all installed apps to stay protected.",
            };

            // Multiple phishing tip variations for randomisation
            _phishingTips = new List<string>
            {
                "🎣 Watch out for emails demanding immediate action — 'Your account has been suspended!' is a classic phishing hook. Slow down and verify.",
                "🎣 Hover over any link before clicking. A URL like 'nedbank-secure.fakesite.com' is NOT Nedbank — check carefully.",
                "🎣 Scammers spoof logos and email layouts perfectly. Always check the actual sender domain, not just the display name.",
                "🎣 No legitimate financial institution will ever request your PIN, OTP, or full password via email or WhatsApp.",
                "🎣 When in doubt, go directly to the official website by typing the URL yourself rather than following any link.",
            };

            // General random tips
            _generalTips = new List<string>
            {
                "💡 Tip: A VPN encrypts your traffic on public Wi-Fi — consider using one when away from home.",
                "💡 Tip: Visit haveibeenpwned.com to check whether your email has appeared in any data breach.",
                "💡 Tip: Lock every device with biometrics or a PIN — even a few seconds of unattended access can be harmful.",
                "💡 Tip: Schedule a weekly or monthly backup of important files to an encrypted external drive or cloud service.",
                "💡 Tip: Switch off Bluetooth and Wi-Fi when you're not using them to reduce your exposure to wireless attacks.",
                "💡 Tip: Before installing any app, read the permissions it requests — some ask for far more than they need.",
                "💡 Tip: A reputable password manager like Bitwarden generates and stores strong, unique passwords for every site.",
                "💡 Tip: Social engineering exploits trust — always double-check unexpected requests, even from known contacts.",
            };
        }

        // ──────────────────────────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Scans user input for known keywords and returns the matching response,
        /// or null if no keyword is recognised.
        /// </summary>
        public string? GetKeywordResponse(string input)
        {
            string lower = input.ToLower();

            string[] multiWord = { "safe browsing", "browsing safely", "scam email",
                                   "two factor", "data protection" };
            foreach (var key in multiWord)
            {
                if (lower.Contains(key))
                    return _keywordResponses[key];
            }

            foreach (var kvp in _keywordResponses)
            {
                if (lower.Contains(kvp.Key))
                    return kvp.Value;
            }

            return null;
        }

        /// <summary>Returns a randomly selected phishing tip.</summary>
        public string GetRandomPhishingTip()
            => _phishingTips[_rng.Next(_phishingTips.Count)];

        /// <summary>Returns a randomly selected general tip.</summary>
        public string GetRandomTip()
            => _generalTips[_rng.Next(_generalTips.Count)];

        /// <summary>Returns supported topic keywords for help display.</summary>
        public IEnumerable<string> GetTopicKeywords()
            => new[] { "password", "phishing", "safe browsing", "scam", "malware", "privacy", "2fa", "update" };
    }
}
