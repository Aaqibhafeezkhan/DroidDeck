# Monitors & info

A button can show live information. In the editor, open **Show on button** and pick an
**Information**. Tapping still runs the button's action, if it has one (a CPU button can open Task
Manager, for example).

## PC monitors

| Information | Shows |
|---|---|
| **CPU usage** | Total processor usage, as a ring or a bar. |
| **Memory usage** | RAM usage, with "used/total GB", as a ring or a bar. |
| **GPU usage** | 3D engine usage of the GPU, as a ring or a bar. |
| **CPU and memory** | CPU and RAM on the same button, one bar each (and the GPU, if you turn on **Show the GPU too**). |
| **Disk space** | How much of a drive is used and how much is still free, or every drive, one bar each. |
| **Network** | Download ↓ and upload ↑ speed across all active network adapters (K = KB/s, M = MB/s). |

Gauges change colour with load: **green** up to 60 %, **orange** above 60 %, **red** above 85 %.

- **Ring or bar:** CPU, memory, GPU and a single chosen drive show a ring by default; with **Bar**
  they show the big number with a bar under it.
- **Tall button:** in **CPU and memory** and in **Disk space** with every drive, a button 2 rows tall
  also shows memory "used/total GB" and the free space of each drive. On a small button with many
  drives, the ones that don't fit are left out (in drive-letter order).
- **Disk space** lists only the PC's fixed disks (HDD and SSD); USB sticks and network drives are left
  out. Space is read **every 30 seconds**. If the chosen drive goes away (disk turned off), the button
  shows "No data".

- Values update **every second** while a phone is connected. The PC stops collecting them when no
  phone is connected.
- GPU usage comes from Windows performance counters and needs a reasonably recent graphics driver.
  It shows 0 % when unavailable.

## Information

| Information | Shows | Options |
|---|---|---|
| **Clock** | The phone's time and date. | 24-hour, seconds, date. |
| **Weather** | Temperature and a weather icon, from [Open-Meteo](https://open-meteo.com) (no account). Updates every 15 min. | City; Fahrenheit. |
| **Now playing** | Cover, track and artist of what's playing in Windows (Spotify, browser, players). | — |
| **Command output (PC)** | What a `cmd` command prints. | Command, template, interval. |
| **URL value (PC)** | An address's response, or one field of its JSON. | URL, JSON field, template, interval. |
| **Windows notifications** | A bell with the number of notifications in the Windows notification center. Updates every 3 s. | Only from app (optional). |

- **Template**: `{value}` is replaced by the value. E.g. `Day {value}`, `{value} ★`.
- **Interval**: how often, in seconds, the PC fetches it again (minimum 5, default 60).
- **JSON field**: a path with dots and indexes, like `current.temp` or `items[0].name`. Empty = the
  whole response.
- The command has up to 10 seconds to finish. Examples: `powershell -c "(Get-Date).DayOfYear"`,
  `echo %TIME:~0,5%`.
- When something fails, the button shows a yellow warning with the reason (city not found, missing
  field, HTTP 404...).

!!! info "Command and URL are created on the PC"
    Like hotkeys, buttons that run a command or read a URL on the PC can only be created or changed in
    the [configurator](../getting-started/configurator.md).

### Windows notifications { #notifications }

Counts the notifications waiting in the Windows notification center. **Only from app (optional)** limits
it to one app: type part of its name as Windows shows it in the notifications (`Discord`, `WhatsApp`,
`Outlook`). Empty = all apps.

Windows asks for permission to read notifications the first time. If the button shows **No
permission**, allow it under Windows **Settings → Privacy & security → Notifications** and wait a few
seconds. Clearing notifications on the PC lowers the number.

## AI usage { #ai-usage }

**AI usage (Claude, Codex)** shows how much of Claude Code or Codex you've used. Pick the **Tool** and
what to show:

| Show | What appears | Where it comes from |
|---|---|---|
| **Plan limits** | One bar for the **5-hour** window and one for the **week**, with the percentage used. A taller button also shows when each one resets ("resets 17:40", "resets Mon 10:00"). | **Claude**: the same query Claude Code's `/usage` makes, with the login it saved on the PC. **Codex**: the latest limits Codex logged on the PC. |
| **Tokens** | The total for **today** and the **last 7 days** ("187M", "7 days: 1.1B"). | The logs the tool keeps on the PC: `~/.claude/projects` and `~/.codex/sessions`. |

- Tokens update every minute; Claude's limits every 5 minutes (Anthropic refuses frequent queries, and
  when it does the button keeps the last value and waits longer). The bars change color like the gauges: green, orange above 60 %, red above 85 %.
- Tokens add up input, output and cache (like `ccusage`). Cache reads are usually most of it, so the
  number is much larger than the text actually exchanged.
- **Weekly pace**: a tick on the week bar marks where usage would be now at a steady pace that ends
  the week at 100 %. If you're spending too fast (the week would end above 110 %), the percentage turns
  **orange**; if you're leaving some unused (below 85 %), it turns **blue**. A taller button also shows the
  projection, like "ends at ~141%". The pace doesn't show in the first hours of the week.
- "Today" starts at the PC's midnight; "7 days" counts today and the six days before.
- Claude Code must be installed and signed in on the PC running DroidDeck. DroidDeck **only reads** the
  login; it never renews it, so your session isn't kicked out. If it expires, the button asks you to
  open Claude Code. Only the percentages go from the PC to the phone.
- Codex only updates its limits while it runs: the button shows the last value it logged.
- Different folders: DroidDeck honors `CLAUDE_CONFIG_DIR` and `CODEX_HOME`.

!!! warning "Unofficial query"
    Claude's limits come from an internal query, the same one Claude Code uses, with no public
    documentation. If Anthropic changes it, the button shows a warning; tokens keep working.

## Slider { #volume-slider }

**Slider (volume, brightness, music)** turns the button into a fader: drag your finger on it to change
the value.

| Controls | What moves |
|---|---|
| **PC sound (default output)** | The volume of Windows' default output device. |
| **Microphone (default input)** | The level of the default input device. |
| **An app** | One app's volume (**App process**: the process name without `.exe`, e.g. `Spotify`, `chrome`, `Discord`). Every audio stream of the app changes together. |
| **Volume of an OBS source** | The volume of an [OBS](../integrations/obs.md) audio source, on the same curve as OBS's fader. |
| **Volume of a person on Discord** | The volume of someone in the call with you, 0 to 200 % (100 % is normal). The button shows the person's name. Needs [Discord](../integrations/discord.md) connected. |
| **Monitor brightness** | The brightness of all monitors together. |
| **Song position** | Where the track playing in Windows is; the button shows the time and dragging seeks. |
| **Light brightness** (Smart Life/Tuya, Home Assistant, Philips Hue, Govee, Elgato Key Light) | A bulb's brightness (on Hue, also a whole room). Dragging to zero turns the light off. |

- The slider is **vertical** when the button is taller than wide and **horizontal** otherwise. Give it
  more cells under [Size](actions.md#size) (1 × 3 or 3 × 1, for example) for more precision.
- OBS sources, people in the call and bulbs come in a list from the PC. If the list is empty
  (integration disconnected, nobody in the call), you can type the value or reload.
- Sound and microphone always follow the *current* default device, so they keep working after you
  switch headphones.
- The level also changes from outside (the PC's keys, another button, the app itself); the slider
  catches up within a few seconds. The fill dims while the target is muted.
- An app slider only finds the app while it shows up in Windows' volume mixer, on the default output.
- **Monitor**: external monitors need **DDC/CI** turned on in the monitor's own menu; a laptop panel
  uses Windows brightness.
- **Discord**: Discord doesn't report each person's volume at any time. If you change someone's volume
  directly in Discord, the button may take a while to catch up.
- **Smart Life bulbs**: only the ones with brightness control show up; switches and plugs don't.
- When the target doesn't respond (OBS closed, light off the network), the button shows why instead of
  its name.
- A tap still runs the button's action, if it has one; holding opens the editor. In the PC
  configurator the slider only shows the level, so you can drag the button around.
