# Import & share

Three ways to bring buttons into DroidDeck besides building them yourself: import a profile from the
Elgato Stream Deck app, open a profile someone shared as a link, or start from the profile gallery.
All of them add **new profiles**; nothing in your deck is overwritten. To move *everything* between
PCs, use a [backup](profiles.md#backup) instead.

## Import from Stream Deck { #stream-deck }

DroidDeck reads the `.streamDeckProfile` files exported by Elgato's Stream Deck app (in the Stream Deck
app: profile list → **Export**).

1. Open the configurator on the PC (`http://localhost:4787/`).
2. In the sidebar, click **Import from Stream Deck** and pick the file.

Each page of the Stream Deck profile becomes a DroidDeck profile, named after it (`Name`, `Name (2)`…).
Folders become profiles opened by an [Open profile](actions.md#open-profile-and-back) button, named
`Name — <folder button>`. When it's done, a message says how many profiles came in and which actions had
no equivalent.

| Stream Deck action | Becomes |
|---|---|
| Hotkey, Hotkey Switch | [Hotkey](actions.md#hotkey) (keys converted to DroidDeck's syntax) |
| Text | Hotkey that types the text (plus Enter, if it was set) |
| Open, Website | [Launch app](actions.md#launch-app) |
| Multimedia | Play/pause, next, previous; volume up/down and mute of the default output |
| Create Folder / Parent Folder | Open profile / Back |
| Multi Action | [Multi-action](actions.md#multi-action); *Delay* steps become the delay of the step before |
| Discord plugin (official): mute, deafen | [Discord](../integrations/discord.md) toggle mute / deafen |
| OBS plugin (official): record, stream, replay buffer, virtual camera, scene | [OBS](../integrations/obs.md) equivalents |

- **Images**: each button's custom image comes along, resized to 144 px (animated GIFs up to 2 MB are
  kept as they are). The title becomes the label.
- **Second state**: the label and image of a two-state key become the button's *active* label and
  icon; a hotkey or app with two states becomes a [two-state](actions.md#two-states) button.
- **Anything else** (other plugins, Soundboard, timers, page switching...) keeps its title and image but
  has no action. The message lists them so you can set them up by hand.
- **Grid size**: if the Stream Deck profile fits in your grid, buttons keep their positions. If it's
  bigger (a Stream Deck XL on a phone, say), the buttons are laid out again in reading order, left to
  right and top to bottom; buttons that still don't fit are left out.

!!! info "Hotkeys and apps need the PC"
    Import from the configurator on the PC. Done from a browser on another device, hotkey, text, open
    and website buttons (and multi-actions with them) come in without their action (the message says how many), under the same rule
    as [privileged actions](actions.md).

!!! warning "Format support"
    Elgato doesn't document the `.streamDeckProfile` format. The importer was written from its known
    structure (Stream Deck 4/5 and 6+ layouts) and tested with synthetic files, not with every version of
    the app. If a profile doesn't import well, please open an
    [issue](https://github.com/ggfto/DroidDeck/issues) — attaching the file helps.

## Share a profile by link { #share }

Open the profile's settings — the sliders icon in the deck's top bar, or the pencil above the grid in
the configurator — and tap **Share**. You get:

- a link to `me.gf2.in/DroidDeck/share/…` (**Copy link** or **Send**), and
- a QR code, when the profile is small enough to fit in one.

The profile travels **inside the link** (compressed, after the `#`); it isn't uploaded anywhere. Whoever
opens it sees a preview of the buttons and clicks **Import into DroidDeck on this PC**. That opens their
configurator at `http://localhost:4787/?import=…`, which asks before adding it as a new profile.

- **Custom images are not included** (the link would get too long); built-in icons, labels, colours
  and actions are. To take the images too, use a [backup](profiles.md#backup).
- Automatic switching by app and the "default profile" mark are not shared.
- Like a Stream Deck import, privileged actions only come in when the import is done on the PC itself:
  DroidDeck must be running on the computer where the link is opened.

!!! warning "Check before sharing"
    The link carries every button's parameters: webhook URLs, HTTP headers, commands, Home Assistant
    entities... Remove anything private (tokens, addresses on your network) before sending it.

## Profile gallery { #gallery }

The [profile gallery](https://me.gf2.in/DroidDeck/gallery/) has ready-made decks to start from
(Streamer, Meetings, Music, Productivity, Gaming). **Import** works like a shared link: it opens
DroidDeck on this PC and adds the profile. Then adjust names, scenes and shortcuts to your setup.

Made a good one? Share it and post the link in [Discussions](https://github.com/ggfto/DroidDeck/discussions).
