using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Code-behind for MainWindow.xaml.
    /// Strictly handles UI events — all chatbot logic lives in ChatbotEngine.
    /// Visual language: light "security console" cards with a coloured
    /// left-accent bar identifying the speaker, rather than full-colour
    /// terminal-style bubbles.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Core objects ───────────────────────────────────────────────────
        private readonly MemoryStore   _memory = new();
        private readonly ChatbotEngine _engine;
        private          SoundPlayer?  _player;

        // ── Typing-animation timer ─────────────────────────────────────────
        private readonly DispatcherTimer _typingTimer  = new();
        private string    _pendingBotText = string.Empty;
        private int       _typingIndex    = 0;
        private TextBlock? _typingBlock   = null;

        // ── Colour palette (mirrors App.xaml design tokens) ─────────────────
        private static readonly SolidColorBrush BrushCardBg     = new(Color.FromRgb(0xFF, 0xFF, 0xFF));
        private static readonly SolidColorBrush BrushUserAccent = new(Color.FromRgb(0x25, 0x63, 0xEB)); // BrandBlue
        private static readonly SolidColorBrush BrushBotAccent  = new(Color.FromRgb(0x0E, 0xA5, 0xA0)); // BrandTeal
        private static readonly SolidColorBrush BrushErrorAccent= new(Color.FromRgb(0xDC, 0x26, 0x26)); // BrandRed
        private static readonly SolidColorBrush BrushTextPrimary= new(Color.FromRgb(0x0F, 0x17, 0x2A));
        private static readonly SolidColorBrush BrushMuted      = new(Color.FromRgb(0x94, 0xA3, 0xB8));
        private static readonly SolidColorBrush BrushAmber      = new(Color.FromRgb(0xD9, 0x77, 0x06));
        private static readonly SolidColorBrush BrushBorder     = new(Color.FromRgb(0xE2, 0xE8, 0xF0));

        private int _messageCount = 0;

        // ──────────────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            _engine = new ChatbotEngine(_memory);
            SetupTypingTimer();
            Loaded += MainWindow_Loaded;
        }

        // ══════════════════════════════════════════════════════════════════
        //  STARTUP
        // ══════════════════════════════════════════════════════════════════
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PlayGreeting();
            ShowBotMessage(_engine.GetGreetingMessage());
            SetStatus("Awaiting your name…");
        }

        // ══════════════════════════════════════════════════════════════════
        //  EVENT HANDLERS
        // ══════════════════════════════════════════════════════════════════
        private void BtnSend_Click(object sender, RoutedEventArgs e)         => SendMessage();
        private void BtnPlayGreeting_Click(object sender, RoutedEventArgs e) => PlayGreeting();
        private void BtnClearChat_Click(object sender, RoutedEventArgs e)    => ClearChat();

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            int len = TxtInput.Text.Length;
            SetStatus(len > 0 ? $"Typing… ({len} chars)" : "Ready — type a message or choose a quick topic");
        }

        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                TxtInput.Text = tag;
                SendMessage();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  MESSAGE DISPATCH
        // ══════════════════════════════════════════════════════════════════
        private void SendMessage()
        {
            string input = TxtInput.Text;
            TxtInput.Clear();

            if (!string.IsNullOrWhiteSpace(input))
                ShowUserMessage(input);

            string response = _engine.GetResponse(input);

            bool isError = response.Contains("Please enter a message") ||
                           response.Contains("stumped");

            ShowBotMessage(response, isError);
            UpdateSidePanel();
            SetStatus("Ready");
        }

        // ══════════════════════════════════════════════════════════════════
        //  CHAT CARD RENDERING
        // ══════════════════════════════════════════════════════════════════

        /// <summary>Builds the soft drop-shadow shared by every chat card.</summary>
        private static DropShadowEffect CardShadow() => new()
        {
            Color       = Color.FromRgb(0x0B, 0x1F, 0x3A),
            Opacity     = 0.07,
            BlurRadius  = 12,
            ShadowDepth = 2,
            Direction   = 270
        };

        private void ShowUserMessage(string text)
        {
            _messageCount++;

            var container = new Border
            {
                Background          = BrushCardBg,
                BorderBrush          = BrushBorder,
                BorderThickness      = new Thickness(0, 0, 0, 0),
                CornerRadius         = new CornerRadius(10),
                Padding              = new Thickness(14, 10, 14, 10),
                Margin               = new Thickness(70, 5, 4, 5),
                HorizontalAlignment  = HorizontalAlignment.Right,
                MaxWidth             = 540,
                Effect               = CardShadow()
            };

            // Left accent bar identifies the speaker without colouring the whole card
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var accent = new Border { Background = BrushUserAccent, CornerRadius = new CornerRadius(2), Margin = new Thickness(0, 0, 12, 0) };
            Grid.SetColumn(accent, 0);

            var stack = new StackPanel();
            Grid.SetColumn(stack, 1);

            var label = new TextBlock
            {
                Text       = $"You  •  {DateTime.Now:HH:mm}",
                Foreground = BrushMuted,
                FontSize   = 10.5,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                Margin     = new Thickness(0, 0, 0, 4)
            };

            var msg = new TextBlock
            {
                Text         = text,
                Foreground   = BrushTextPrimary,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 13.5,
                TextWrapping = TextWrapping.Wrap
            };

            stack.Children.Add(label);
            stack.Children.Add(msg);
            grid.Children.Add(accent);
            grid.Children.Add(stack);
            container.Child = grid;
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

        private void ShowBotMessage(string text, bool isError = false)
        {
            _messageCount++;

            var accentBrush = isError ? BrushErrorAccent : BrushBotAccent;
            var speakerLabel = isError ? "ShieldBot · Notice" : "ShieldBot";

            var container = new Border
            {
                Background          = BrushCardBg,
                CornerRadius        = new CornerRadius(10),
                Padding             = new Thickness(14, 10, 14, 10),
                Margin              = new Thickness(4, 5, 70, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth            = 600,
                Effect              = CardShadow()
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var accent = new Border { Background = accentBrush, CornerRadius = new CornerRadius(2), Margin = new Thickness(0, 0, 12, 0) };
            Grid.SetColumn(accent, 0);

            var stack = new StackPanel();
            Grid.SetColumn(stack, 1);

            var label = new TextBlock
            {
                Text       = $"{speakerLabel}  •  {DateTime.Now:HH:mm}",
                Foreground = BrushMuted,
                FontSize   = 10.5,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                Margin     = new Thickness(0, 0, 0, 4)
            };

            var msgBlock = new TextBlock
            {
                Foreground   = BrushTextPrimary,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 13.5,
                TextWrapping = TextWrapping.Wrap
            };

            stack.Children.Add(label);
            stack.Children.Add(msgBlock);
            grid.Children.Add(accent);
            grid.Children.Add(stack);
            container.Child = grid;
            ChatPanel.Children.Add(container);

            StartTypingEffect(text, msgBlock);
            ScrollToBottom();
        }

        // ══════════════════════════════════════════════════════════════════
        //  TYPING ANIMATION
        // ══════════════════════════════════════════════════════════════════
        private void SetupTypingTimer()
        {
            _typingTimer.Interval = TimeSpan.FromMilliseconds(14);
            _typingTimer.Tick    += TypingTimer_Tick;
        }

        private void StartTypingEffect(string text, TextBlock block)
        {
            _typingTimer.Stop();
            if (_typingBlock != null)
                _typingBlock.Text = _pendingBotText;

            _pendingBotText = text;
            _typingIndex    = 0;
            _typingBlock    = block;
            _typingTimer.Start();
        }

        private void TypingTimer_Tick(object? sender, EventArgs e)
        {
            if (_typingBlock == null || _typingIndex >= _pendingBotText.Length)
            {
                _typingTimer.Stop();
                return;
            }

            int charsToAdd = Math.Min(2, _pendingBotText.Length - _typingIndex);
            _typingBlock.Text += _pendingBotText.Substring(_typingIndex, charsToAdd);
            _typingIndex      += charsToAdd;
            ScrollToBottom();
        }

        // ══════════════════════════════════════════════════════════════════
        //  SIDE PANEL ("Inspector")
        // ══════════════════════════════════════════════════════════════════
        private void UpdateSidePanel()
        {
            TxtUserName.Text     = _memory.HasName          ? $"Name: {_memory.UserName}"        : "Name: —";
            TxtUserTopic.Text    = _memory.HasFavouriteTopic ? $"Topic: {_memory.FavouriteTopic}" : "Topic: —";
            TxtMessageCount.Text = $"Messages: {_memory.MessageCount}";

            var sa = new SentimentAnalyzer();
            TxtSentiment.Text = sa.GetSentimentLabel(_engine.LastSentiment);

            TxtSentiment.Foreground = _engine.LastSentiment switch
            {
                Sentiment.Worried    => BrushErrorAccent,
                Sentiment.Frustrated => BrushAmber,
                Sentiment.Curious    => BrushUserAccent,
                _                    => BrushMuted
            };
        }

        // ══════════════════════════════════════════════════════════════════
        //  VOICE GREETING
        // ══════════════════════════════════════════════════════════════════
        private void PlayGreeting()
        {
            try
            {
                string[] candidates =
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "greeting.wav"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "greeting.ogg"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Audio", "greeting.wav"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Audio", "greeting.ogg"),
                };

                string? found = candidates.FirstOrDefault(File.Exists);

                if (found != null && found.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    _player?.Dispose();
                    _player = new SoundPlayer(found);
                    _player.Play();
                    SetStatus("🔊 Playing voice greeting…");
                }
                else if (found != null)
                {
                    SetStatus("ℹ️  Voice file found (OGG). Convert to WAV for Windows playback.");
                    AppendSystemNote("ℹ️  Voice greeting found but requires WAV format. Convert Audio/greeting.ogg → Audio/greeting.wav to enable playback.");
                }
                else
                {
                    SetStatus("⚠️  Audio file not found — running without sound.");
                    AppendSystemNote("⚠️  No audio file detected. Place greeting.wav inside the Audio folder.");
                }
            }
            catch (Exception ex)
            {
                SetStatus("⚠️  Audio playback failed — continuing normally.");
                AppendSystemNote($"⚠️  Audio error: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  UTILITY HELPERS
        // ══════════════════════════════════════════════════════════════════
        private void ClearChat()
        {
            ChatPanel.Children.Clear();
            _memory.ClearAll();
            _messageCount = 0;
            UpdateSidePanel();
            ShowBotMessage(_engine.GetGreetingMessage());
            SetStatus("Chat cleared — ready to start again.");
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }

        private void SetStatus(string message) => TxtStatus.Text = message;

        private void AppendSystemNote(string note)
        {
            var tb = new TextBlock
            {
                Text         = note,
                Foreground   = BrushAmber,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 11.5,
                TextWrapping = TextWrapping.Wrap,
                Margin       = new Thickness(8, 4, 8, 4),
                FontStyle    = FontStyles.Italic
            };
            ChatPanel.Children.Add(tb);
            ScrollToBottom();
        }
    }
}
