# Monitors & info

A button can show live information. In the editor, open **Show on button** and pick an
**Information**. Tapping still runs the button's action, if it has one (a CPU button can open Task
Manager, for example).

## PC monitors

| Information | Shows |
|---|---|
| **CPU usage** | Total processor usage, as a gauge. |
| **Memory usage** | RAM usage, with "used/total GB". |
| **GPU usage** | 3D engine usage of the GPU, as a gauge. |
| **Network** | Download ↓ and upload ↑ speed across all active network adapters (K = KB/s, M = MB/s). |

Gauges change colour with load: **green** up to 60 %, **orange** above 60 %, **red** above 85 %.

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

## Volume slider { #volume-slider }

**Volume slider** turns the button into a fader: drag your finger on it to change the volume.

| Controls | What moves |
|---|---|
| **PC sound (default output)** | The volume of Windows' default output device. |
| **Microphone (default input)** | The level of the default input device. |
| **An app** | One app's volume (**App process**: the process name without `.exe`, e.g. `Spotify`, `chrome`, `Discord`). Every audio stream of the app changes together. |

- The slider is **vertical** when the button is taller than wide and **horizontal** otherwise. Give it
  more cells under [Size](actions.md#size) (1 × 3 or 3 × 1, for example) for more precision.
- It always follows the *current* default device, so it keeps working after you switch headphones.
- The level also changes from outside (the PC's keys, another button); the slider catches up within a
  few seconds. The fill dims while the target is muted.
- An app slider only finds the app while it shows up in Windows' volume mixer, on the default output.
- A tap still runs the button's action, if it has one; holding opens the editor. In the PC
  configurator the slider only shows the level, so you can drag the button around.
