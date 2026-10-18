# MQTT

Um botão MQTT genérico: publica uma mensagem num tópico do **seu** broker — Mosquitto, o add-on do Home
Assistant, EMQX, Node-RED, Zigbee2MQTT, aparelhos com Tasmota...

## Configuração

Abra **Configuração → Integrações → MQTT** e preencha:

- **Broker**: `192.168.0.10`, `broker.local:1884`, `mqtt://host:1883`, ou `mqtts://host:8883` para TLS.
  Sem porta, vale a 1883 (ou a 8883 com TLS).
- **Usuário** e **Senha**, se o broker pedir.

Toque em **Conectar**. O PC fica conectado e reconecta sozinho (espaçando até uma vez por minuto). A senha
fica cifrada (`mqtt.json`).

## O que dá para fazer

| Campo | Significado |
|---|---|
| Tópico | Onde publicar, ex.: `casa/sala/luz/set`. Curingas (`#`, `+`) não são aceitos |
| Mensagem | Texto ou JSON; pode ficar vazia |
| QoS | 0 — no máximo uma vez · 1 — pelo menos uma vez · 2 — exatamente uma vez |
| Reter (retain) | O broker guarda a última mensagem do tópico para quem assinar depois |

O DroidDeck só publica; não assina nada, então o botão não mostra o estado de um aparelho.
