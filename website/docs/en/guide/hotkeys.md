# Keyboard shortcuts

A **Keyboard shortcut** button sends keys to **the window that has focus** on the PC. Type them in
**Keys** using the Windows *SendKeys* syntax, plus `#` for the Windows key.

## Common examples

| Keys | Sends |
|---|---|
| `^c` | Ctrl+C |
| `^+s` | Ctrl+Shift+S |
| `%{F4}` | Alt+F4 |
| `#d` | Win+D (show the desktop) |
| `#+s` | Win+Shift+S (screenshot tool) |
| `^+m` | Ctrl+Shift+M (mute in many meeting apps) |
| `{MEDIA_PLAY_PAUSE}` | The media play/pause key |

## Syntax

| Symbol | Key | Example |
|---|---|---|
| `^` | Ctrl | `^c` → Ctrl+C |
| `+` | Shift | `^+s` → Ctrl+Shift+S |
| `%` | Alt | `%{F4}` → Alt+F4 |
| `#` | Windows | `#d` → show desktop |
| `( )` | Hold modifiers for a group | `+(abc)` → ABC |
| `~` | Enter | `^a~` |
| `{NAME}` | Special key | `{ENTER}`, `{TAB}`, `{F5}` (see below) |
| `{NAME n}` | Repeat | `{TAB 3}` |
| `{x}` | The character itself | `{+}`, `{^}`, `{%}`, `{#}`, `{~}`, `{(}`, `{{}` |

Any other character is typed as is. With Ctrl, Alt or Windows, letters ignore case: `^C` is Ctrl+C;
use `+` for Shift.

??? note "All special key names"
    `{ENTER}`, `{TAB}`, `{ESC}`, `{SPACE}`, `{F1}`…`{F24}`, `{UP}`, `{DOWN}`, `{LEFT}`, `{RIGHT}`,
    `{HOME}`, `{END}`, `{PGUP}`, `{PGDN}`, `{INS}`, `{DEL}`, `{BS}`, `{PRTSC}`,
    `{NUMPAD0}`…`{NUMPAD9}`, `{VOLUME_UP}`, `{VOLUME_DOWN}`, `{VOLUME_MUTE}`,
    `{MEDIA_PLAY_PAUSE}`, `{MEDIA_NEXT}`, `{MEDIA_PREV}`.

Keys are sent with their hardware scan codes and held for a moment, so most games that read the
keyboard directly also receive them.

!!! warning "Limitations"
    - Windows blocks input to apps running **as administrator** unless DroidDeck also runs as administrator.
    - Games with anti-cheat may ignore simulated keys.
    - Some combinations are reserved by Windows and can't be simulated, such as Ctrl+Alt+Del and Win+L.
      To lock the PC, use the [Windows](actions.md#windows) action.

!!! info "Created on the PC only"
    Keys can do anything you could do at the keyboard, so keyboard-shortcut buttons can only be created
    or changed in the [configurator](../getting-started/configurator.md) on the PC. The phone runs them
    normally.

To send a sequence (keys, a pause, more keys), use a [Multi-action](multi-action.md), or record it
with [Record from keyboard](multi-action.md#record).
