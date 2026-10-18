# Streamer.bot

DroidDeck runs [Streamer.bot](https://streamer.bot) actions through Streamer.bot's **WebSocket Server**.

## Setup

1. In Streamer.bot, open **Servers/Clients → WebSocket Server**, tick **Auto Start** and click
   **Start Server**. The default is `127.0.0.1`, port `8080`.
2. Open **Settings → Integrations → Streamer.bot** in DroidDeck. The address is already filled in
   (`ws://127.0.0.1:8080/`); tap **Connect**.
3. If you turned on **Authentication** in Streamer.bot, fill in the same password (stored encrypted).

Streamer.bot on another PC: there, change the server address to `0.0.0.0`; in DroidDeck use that PC's IP
(`ws://192.168.0.20:8080/`).

## What you can do

Create a button of type **Streamer.bot** and pick the **Action** from the list (enabled actions, with their
group). You can also type an action's name.

**Arguments** (optional) become `%key%` variables in the action: one per line as `key=value`, or a JSON
object (`{"user": "gabriel", "amount": 3}`).
