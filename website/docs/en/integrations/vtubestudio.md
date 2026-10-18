# VTube Studio

DroidDeck connects to [VTube Studio](https://denchisoft.com) as a plugin through its public API
(WebSocket, port `8001` by default).

## Setup

1. In VTube Studio, open **Settings** (gear) and turn on **Start API (allow plugins)**.
2. In DroidDeck, open **Settings → Integrations → VTube Studio** and tap **Connect**.
3. VTube Studio asks whether to allow the **DroidDeck** plugin. Click **Allow**.

DroidDeck keeps the token VTube Studio gives it (encrypted, `vtubestudio.json`) and uses it on every
reconnection — you only approve once. If you deny the request, or later revoke the plugin in VTube Studio,
DroidDeck doesn't ask again on its own: tap **Connect** when you want it to.

## What you can do

| Operation | Parameters |
|---|---|
| Trigger hotkey | A hotkey of the model loaded now (expressions, animations, ...). A typed name also works |
| Load model | A model from the list |

The hotkey list comes from the model that is loaded when you open the editor.
