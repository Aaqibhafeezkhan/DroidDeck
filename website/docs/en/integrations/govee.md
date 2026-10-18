# Govee

Govee lights and strips that support the **LAN API** can be controlled directly on the local network, with
no Govee account.

## Setup

1. In the **Govee Home** app, open each device's settings and turn on **LAN Control**. Not every model has
   it.
2. Create a button of type **Govee**. The **Device** list searches the network when it opens (UDP multicast
   on `239.255.255.250:4001`); if the device doesn't show up, type its IP.

Give the device a fixed IP in your router so the buttons keep working.

## What you can do

| Operation | Parameters |
|---|---|
| Toggle / On / Off | — |
| Set brightness | 1 to 100 % |
| Set color | `#RRGGBB` |
| Set temperature (white) | 2000 to 9000 K |

!!! note "Ports"
    Commands go to the device on UDP port `4003`, and the answers (search and the state that **Toggle**
    needs) come back to the PC on UDP port `4002`. If another program on the PC already uses port 4002,
    Govee buttons can't work. If the search finds nothing but the IP works, a firewall is blocking the
    answers — **On** and **Off** still work, as they don't need an answer.
