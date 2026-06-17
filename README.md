# ShieldBot - Cybersecurity Awareness Chatbot (Part 2)

This is Part 2 of my PROG6221 POE. It builds on the console chatbot from Part 1 and turns it into a proper WPF desktop app with a graphical interface, memory, sentiment detection, and a few other things the brief asked for.

The bot's name is ShieldBot. It's meant to help people (mostly aimed at South African users) understand basic cybersecurity stuff like phishing, password safety, scams, and so on, in a casual chat format rather than a wall of text.

## What it does

- Greets you by voice when the app opens (there's a wav file that plays)
- Asks for your name and what topic you're interested in, then remembers both for the rest of the session
- Recognises keywords like "password", "phishing", "scam", "malware", "privacy", "2fa", "update" even if you type a full sentence around them
- Picks up on your tone - if you say you're "worried" or "frustrated" it responds a bit differently before giving the actual tip
- Gives random phishing tips instead of repeating the same one every time
- Doesn't crash if you send an empty message or type gibberish, it just asks you to rephrase
- Has a sidebar showing your name, your topic, your current mood, and how many messages you've sent

## Why it looks the way it does

I didn't want the typical dark "hacker" look with green text on black that a lot of these projects end up with. I went for something closer to a normal security app you'd actually trust - light background, navy header, blue and teal accents, a shield icon I drew directly in XAML instead of using ASCII art or an emoji. Messages show up as white cards with a thin coloured line on the left to show who's talking (blue for you, teal for the bot, red if something went wrong).

## Project layout

```
CyberAwarenessBot/
├── App.xaml / App.xaml.cs      -> styles and colours for the whole app
├── MainWindow.xaml              -> the actual window layout
├── MainWindow.xaml.cs           -> button clicks, chat bubbles, etc (UI only)
├── ChatbotEngine.cs              -> the main brain, decides what to reply
├── ResponseManager.cs           -> keyword dictionary + the tip lists
├── SentimentAnalyzer.cs         -> figures out if you're worried/frustrated/curious
├── MemoryStore.cs                -> keeps your name, topic, and chat history
├── DelegateHandlers.cs          -> the delegate stuff (formatting, validation, logging)
├── Audio/greeting.wav            -> the voice greeting file
└── README.md
```

## Running it

You'll need:
- Windows (it's WPF so no Mac/Linux)
- .NET 8 SDK
- Visual Studio 2022 with the desktop development workload, or VS Code with the C# extension

Clone it, open `CyberAwarenessBot.sln`, hit F5. That's it. Or if you'd rather use the terminal:

```
dotnet build
dotnet run
```

The greeting.wav file is already in the Audio folder, so the voice greeting should just work when you launch it. If for some reason the file is missing or won't play, the app doesn't crash, it just shows a little warning in the chat and carries on.

## Collections and delegates (since the brief specifically asks about these)

I used a Dictionary for keyword lookups in ResponseManager because checking a string against a dictionary key is basically instant compared to looping through a list every time someone types something. There are two Lists as well, one for phishing tip variations and one for general tips, both used with Random so you don't get the exact same response twice in a row.

For memory I went with a Queue to store the conversation history since it naturally keeps things in order and old messages drop off the front once it hits the limit. There's also a Stack holding the most recent user messages, mainly there to show LIFO access since that's a requirement.

Three delegates are defined in DelegateHandlers.cs:
- FormatBotMessage - adds the timestamp and name to every bot reply
- ValidateUserInput - checks if what you typed is empty before it even reaches the engine
- LogConversationEntry - writes each message to the debug output

These get wired up once in the ChatbotEngine constructor, so if I wanted to change how messages are formatted later I'd only need to swap out one delegate instead of rewriting the engine.

## A quick example of how a chat goes

```
ShieldBot: Hey there! Welcome to ShieldBot - your personal cybersecurity guide. What should I call you?
You: Mfundo
ShieldBot: Great to meet you, Mfundo! Which cybersecurity topic would you like to explore first?
You: I'm worried about scams
ShieldBot: It's completely understandable to feel concerned, Mfundo - the online world can feel daunting.
           Scammers rely on pressure and panic. Pause, verify using a number you look up yourself...
You: tell me more
ShieldBot: Since you're keen on scams, Mfundo, here's an extra tip: ...
```

## Releases

- v1.0 - basic WPF GUI, keyword recognition, memory working
- v1.1 - added sentiment detection, delegates, and proper error handling
- v1.2 - redesigned the whole UI to look more like an actual product, swapped in a real voice recording for the greeting

---
Submitted as part of PROG6221 - The Independent Institute of Education, 2026
