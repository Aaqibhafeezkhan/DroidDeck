# MQTT

A generic MQTT button: publishes a message to a topic on **your** broker — Mosquitto, the Home Assistant
add-on, EMQX, Node-RED, Zigbee2MQTT, Tasmota devices...

## Setup

Open **Settings → Integrations → MQTT** and fill in:

- **Broker**: `192.168.0.10`, `broker.local:1884`, `mqtt://host:1883`, or `mqtts://host:8883` for TLS.
  Without a port, 1883 (or 8883 with TLS) is used.
- **Username** and **Password**, if the broker asks for them.

Tap **Connect**. The PC stays connected and reconnects on its own (backing off to once a minute). The
password is stored encrypted (`mqtt.json`).

## What you can do

| Field | Meaning |
|---|---|
| Topic | Where to publish, e.g. `home/living-room/light/set`. Wildcards (`#`, `+`) aren't allowed |
| Message | Text or JSON; can be empty |
| QoS | 0 — at most once · 1 — at least once · 2 — exactly once |
| Retain | The broker keeps the topic's last message for whoever subscribes later |

DroidDeck only publishes; it doesn't subscribe, so the button doesn't show a device's state.
