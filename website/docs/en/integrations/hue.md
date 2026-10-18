# Philips Hue

DroidDeck controls Philips Hue through the **Hue Bridge** on your network (local API v1), with no Hue
account or cloud.

## Setup (pairing)

1. Open **Settings → Integrations → Philips Hue** (configurator: **Integrations** in the sidebar).
2. Type the bridge **IP** (Hue app: **Settings → Bridges**) or leave it empty to search for it
   (uses `discovery.meethue.com`, so it needs internet).
3. Press the **round button on top of the Hue Bridge**.
4. Within 30 seconds, tap **Pair**.

The bridge hands DroidDeck a user, stored encrypted on the PC (`hue.json`). If the button wasn't pressed,
DroidDeck asks you to press it and tap **Pair** again.

## What you can do

| Operation | Parameters |
|---|---|
| Toggle / On / Off | A light, or a room/zone (or **All lights**). A room is "on" when any light in it is on |
| Set brightness | 1 to 100 % (turns it on) |
| Brightness (+/-) | Step from -100 to 100 %. Dimming doesn't turn on a light that is off |
| Activate scene | A scene from the list (shown with its room) |

Lights, rooms and scenes are listed from the bridge. The PC checks the bridge every few minutes; if it stops
answering, the editor warns that Hue isn't connected.
