# Elgato Key Light

Elgato **Key Light**, **Key Light Air**, **Key Light Mini** and **Light Strip** have a small HTTP API on
the local network (port `9123`). DroidDeck uses it directly — Elgato Control Center doesn't need to be
open, and there's no account.

## Setup

Each button carries the light's **IP**. Find it in Elgato Control Center → the light's settings, or in your
router. Give the light a fixed IP (DHCP reservation in the router) so the buttons keep working. A different
port goes after a colon: `192.168.0.50:9123`.

## What you can do

| Operation | Parameters |
|---|---|
| Toggle / On / Off | — |
| Set brightness | 1 to 100 % (also turns the light on) |
| Brightness (+/-) | Step from -100 to 100 % |
| Set temperature | 2900 to 7000 K |
| Temperature (+/-) | Step in K; positive makes the light cooler (bluer) |

With several lights in one device, all of them change. The Light Strip has colour instead of temperature:
on it, use toggle and brightness.
