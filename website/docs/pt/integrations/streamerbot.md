# Streamer.bot

O DroidDeck dispara ações do [Streamer.bot](https://streamer.bot) pelo **WebSocket Server** dele.

## Configuração

1. No Streamer.bot, abra **Servers/Clients → WebSocket Server**, marque **Auto Start** e clique em
   **Start Server**. O padrão é `127.0.0.1`, porta `8080`.
2. Abra **Configuração → Integrações → Streamer.bot** no DroidDeck. O endereço já vem preenchido
   (`ws://127.0.0.1:8080/`); toque em **Conectar**.
3. Se ligou **Authentication** no Streamer.bot, preencha a mesma senha (fica cifrada).

Streamer.bot em outro PC: lá, troque o endereço do servidor para `0.0.0.0`; no DroidDeck, use o IP daquele
PC (`ws://192.168.0.20:8080/`).

## O que dá para fazer

Crie um botão do tipo **Streamer.bot** e escolha a **Ação** na lista (as ações ativas, com o grupo). Também
dá para digitar o nome da ação.

Os **Argumentos** (opcionais) viram variáveis `%chave%` na ação: um por linha no formato `chave=valor`, ou
um objeto JSON (`{"usuario": "gabriel", "quantidade": 3}`).
