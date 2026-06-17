# 🛡️ ShieldBot — Cybersecurity Awareness Chatbot (Part 2 / WPF GUI)

An interactive WPF chatbot that raises cybersecurity awareness among South African users, covering phishing, passwords, scams, malware, and more — built with a light, professional "security console" interface.

---

## 📋 Features

| Feature | Status |
|---|---|
| WPF GUI — modern light "console" theme | ✅ |
| Custom voice greeting on launch | ✅ |
| Drawn vector shield brand mark (no emoji/ASCII art) | ✅ |
| Name personalisation | ✅ |
| Favourite topic memory | ✅ |
| Keyword recognition (8+ topics) | ✅ |
| Sentiment detection (worried / frustrated / curious) | ✅ |
| Generic collections (Dictionary, List, Queue, Stack) | ✅ |
| Delegate pipeline (FormatBotMessage, ValidateUserInput, LogEntry) | ✅ |
| Robust error handling (empty input, gibberish, missing audio) | ✅ |
| Typing animation for bot responses | ✅ |
| Quick-topic sidebar panel | ✅ |
| CI/CD via GitHub Actions | ✅ |

---

## 🎨 Visual Design

ShieldBot moved away from the dark "hacker terminal" look used in earlier drafts to a **light, professional security-dashboard aesthetic** — the kind of interface you'd expect from an enterprise security product, not a green-on-black console.

**Design tokens**

| Token | Value | Use |
|---|---|---|
| Surface (page) | `#F4F7FB` | App background |
| Surface (card) | `#FFFFFF` | Chat cards, inspector cards |
| Brand Navy | `#0B1F3A` | Header bar |
| Brand Blue | `#2563EB` | Primary actions, user accent |
| Brand Teal | `#0EA5A0` | Bot accent, positive state |
| Brand Amber | `#D97706` | Frustrated mood indicator |
| Brand Red | `#DC2626` | Errors only |
| Text Primary | `#0F172A` | Body text |
| Text Secondary | `#475569` | Labels, captions |

**Typography:** Segoe UI for all interface text (clean, native, professional), with Cascadia Mono reserved only for the small session message-count stat — not used throughout, so the UI doesn't read as a terminal.

**Signature element:** a hand-drawn vector shield-and-checkmark mark in the header, built directly in XAML (`Path` geometry), replacing the previous block-letter ASCII art banner. Chat messages use white cards with a soft drop shadow and a thin coloured accent bar on the left edge (blue for the user, teal for ShieldBot, red for error/notice messages) instead of full-colour bubble backgrounds — a quieter, more confident way to distinguish speakers.

---

## 🏗️ Project Structure

```
CyberAwarenessBot/
├── App.xaml / App.xaml.cs          — Application entry point & design-token styles
├── MainWindow.xaml                  — GUI layout (XAML)
├── MainWindow.xaml.cs               — UI event handlers only
├── ChatbotEngine.cs                 — Central bot logic & response orchestration
├── SentimentAnalyzer.cs             — Detects worried / frustrated / curious moods
├── MemoryStore.cs                   — Stores name, topic, history (Queue + Stack)
├── ResponseManager.cs               — Keyword Dictionary + random tip Lists
├── DelegateHandlers.cs              — Delegate definitions & implementations
├── Audio/
│   └── greeting.wav                 — Voice greeting (WAV format required)
├── .github/workflows/
│   └── ci.yml                       — GitHub Actions CI workflow
└── README.md
```

---

## 🚀 Getting Started

### Requirements
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Visual Studio 2022 (with .NET Desktop workload) **or** VS Code with C# Dev Kit

### Steps

1. **Clone the repo**
   ```bash
   git clone https://github.com/YOUR_USERNAME/CyberAwarenessBot.git
   cd CyberAwarenessBot
   ```

2. **Open in Visual Studio**
   - Open `CyberAwarenessBot.sln`
   - Press **F5** to build and run

3. **Command-line build**
   ```bash
   dotnet build
   dotnet run
   ```

4. **Voice greeting**
   - `greeting.wav` already sits in the `Audio/` folder
   - The app handles a missing file gracefully without crashing

---

## 🧠 Generic Collections

| Collection | File | Why |
|---|---|---|
| `Dictionary<string, string>` | `ResponseManager.cs` | O(1) keyword lookups — fastest as topic set grows |
| `List<string>` | `ResponseManager.cs` | Random-index access for varied tip delivery |
| `Queue<string>` | `MemoryStore.cs` | FIFO conversation history — oldest messages dequeued first |
| `Stack<string>` | `MemoryStore.cs` | LIFO recent-message access — most recent always on top |

---

## 🔗 Delegate Pipeline

Defined in `DelegateHandlers.cs`, wired in `ChatbotEngine.cs`:

| Delegate | Role |
|---|---|
| `FormatBotMessage` | Adds timestamp and name prefix to every bot reply |
| `ValidateUserInput` | Rejects empty input before it reaches the engine |
| `LogConversationEntry` | Writes each exchange to the Debug output |

---

## 💬 Sample Conversation

```
ShieldBot: Hey there! Welcome to ShieldBot. What should I call you?
User:      Mfundo
ShieldBot: Great to meet you, Mfundo! Which cybersecurity topic interests you most?
User:      I'm worried about online scams
ShieldBot: 😌 It's completely understandable to feel concerned Mfundo...
           ⚠️ Scammers rely on pressure and panic. Pause, verify...
User:      Tell me more
ShieldBot: Since you're keen on scams, Mfundo, here's an extra tip: [random tip]
User:      [empty]
ShieldBot: Please enter a message so I can assist you.
```

---

## ✅ CI Status

![CI](https://github.com/YOUR_USERNAME/CyberAwarenessBot/actions/workflows/ci.yml/badge.svg)

---

## 📦 Releases

| Tag | Description |
|---|---|
| `v1.0` | WPF GUI with keyword recognition and memory |
| `v1.1` | Sentiment detection, delegates, and enhanced error handling |
| `v1.2` | Light "security console" redesign + custom voice greeting |

---

*The Independent Institute of Education — 2026*
