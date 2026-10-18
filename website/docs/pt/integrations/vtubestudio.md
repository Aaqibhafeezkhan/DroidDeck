# VTube Studio

O DroidDeck conecta no [VTube Studio](https://denchisoft.com) como um plugin, pela API pública (WebSocket,
porta `8001` por padrão).

## Configuração

1. No VTube Studio, abra as **Configurações** (engrenagem) e ligue **Iniciar API (permitir plugins)**.
2. No DroidDeck, abra **Configuração → Integrações → VTube Studio** e toque em **Conectar**.
3. O VTube Studio pergunta se permite o plugin **DroidDeck**. Clique em **Permitir**.

O DroidDeck guarda o token que o VTube Studio entrega (cifrado, `vtubestudio.json`) e usa em toda
reconexão — você só aprova uma vez. Se negar o pedido, ou revogar o plugin depois no VTube Studio, o
DroidDeck não pergunta de novo sozinho: toque em **Conectar** quando quiser.

## O que dá para fazer

| Operação | Parâmetros |
|---|---|
| Disparar atalho | Um atalho (hotkey) do modelo carregado agora (expressões, animações...). Também aceita o nome digitado |
| Carregar modelo | Um modelo da lista |

A lista de atalhos vem do modelo que está carregado quando você abre o editor.
