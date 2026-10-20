# Monitores e informação

Um botão pode mostrar informação ao vivo. No editor, abra **Mostrar no botão** e escolha uma
**Informação**. O toque continua executando a ação do botão, se houver uma (um botão de CPU pode abrir
o Gerenciador de Tarefas, por exemplo).

## Monitores do PC

| Informação | Mostra |
|---|---|
| **Uso da CPU** | Uso total do processador, em um medidor. |
| **Uso da memória** | Uso de RAM, com "usado/total GB". |
| **Uso da GPU** | Uso do motor 3D da GPU, em um medidor. |
| **Rede** | Velocidade de download ↓ e upload ↑ somando todos os adaptadores de rede ativos (K = KB/s, M = MB/s). |

Os medidores mudam de cor conforme a carga: **verde** até 60 %, **laranja** acima de 60 %,
**vermelho** acima de 85 %.

- Os valores são atualizados **a cada segundo** enquanto houver um celular conectado. O PC para de
  coletá-los quando nenhum celular está conectado.
- O uso da GPU vem dos contadores de desempenho do Windows e exige um driver de vídeo razoavelmente
  recente. Quando não está disponível, aparece 0 %.

## Informação

| Informação | Mostra | Opções |
|---|---|---|
| **Relógio** | Hora e data do celular. | 24 horas, segundos, data. |
| **Clima** | Temperatura e ícone do tempo, pelo [Open-Meteo](https://open-meteo.com) (sem conta). Atualiza a cada 15 min. | Cidade; Fahrenheit. |
| **Tocando agora** | Capa, faixa e artista do que toca no Windows (Spotify, navegador, players). | — |
| **Saída de um comando (PC)** | O que um comando do `cmd` escreve. | Comando, modelo, intervalo. |
| **Valor de uma URL (PC)** | A resposta de um endereço, ou um campo do JSON dela. | URL, campo do JSON, modelo, intervalo. |
| **Notificações do Windows** | Um sino com o número de notificações na central de notificações do Windows. Atualiza a cada 3 s. | Só do app (opcional). |

- **Modelo**: `{value}` é trocado pelo valor. Ex.: `Dia {value}`, `{value} ★`.
- **Intervalo**: de quantos em quantos segundos o PC busca de novo (mínimo 5, padrão 60).
- **Campo do JSON**: um caminho com pontos e índices, como `current.temp` ou `items[0].name`.
  Vazio = a resposta inteira.
- O comando tem até 10 segundos para terminar. Exemplos: `powershell -c "(Get-Date).DayOfYear"`,
  `echo %TIME:~0,5%`.
- Quando algo falha, o botão mostra um aviso amarelo com o motivo (cidade não encontrada, campo que não
  existe, HTTP 404...).

!!! info "Comando e URL são criados no PC"
    Como os atalhos de teclado, botões que rodam um comando ou leem uma URL no PC só podem ser criados
    ou alterados no [configurador](../getting-started/configurator.md).

### Notificações do Windows { #notifications }

Conta as notificações esperando na central de notificações do Windows. **Só do app (opcional)** limita a
um app: digite parte do nome dele como o Windows mostra nas notificações (`Discord`, `WhatsApp`,
`Outlook`). Vazio = todos os apps.

Na primeira vez, o Windows pede permissão para ler as notificações. Se o botão mostrar **Sem
permissão**, libere em **Configurações → Privacidade e segurança → Notificações** do Windows e espere
alguns segundos. Limpar as notificações no PC diminui o número.

## Uso de IA { #ai-usage }

**Uso de IA (Claude, Codex)** mostra quanto você usou do Claude Code ou do Codex. Escolha a
**Ferramenta** e o que mostrar:

| Mostrar | O que aparece | De onde vem |
|---|---|---|
| **Limites do plano** | Uma barra para a janela de **5 horas** e outra para a **semana**, com a porcentagem já usada. Em botão mais alto aparece também quando cada uma zera ("zera 17:40", "zera seg 10:00"). | **Claude**: a mesma consulta que o `/usage` do Claude Code faz, com o login que ele salvou no PC. **Codex**: o último registro de limites que o Codex gravou no PC. |
| **Tokens** | O total de **hoje** e dos **últimos 7 dias** ("187 mi", "7 dias: 1,1 bi"). | Os registros que a ferramenta grava no PC: `~/.claude/projects` e `~/.codex/sessions`. |

- Os tokens atualizam a cada minuto; os limites do Claude, a cada 5 minutos (a Anthropic recusa consultas
  frequentes e, quando recusa, o botão mantém o último valor e espera mais). As barras mudam de cor como os medidores: verde, laranja acima de 60 %,
  vermelho acima de 85 %.
- Os tokens somam entrada, saída e cache (como o `ccusage`). O cache relido costuma ser a maior parte,
  então o número fica bem maior que o texto trocado de fato.
- "Hoje" começa à meia-noite do PC; "7 dias" conta hoje e os seis dias anteriores.
- O Claude Code precisa estar instalado e com login no PC onde o DroidDeck roda. O DroidDeck **só lê**
  o login; nunca o renova, para não derrubar a sua sessão. Se ele expirar, o botão pede para abrir o
  Claude Code. Nada além das porcentagens sai do PC para o celular.
- O Codex só atualiza os limites quando roda: o botão mostra o último valor que ele gravou.
- Pastas diferentes: o DroidDeck respeita `CLAUDE_CONFIG_DIR` e `CODEX_HOME`.

!!! warning "Consulta não oficial"
    Os limites do Claude vêm de uma consulta interna, a mesma que o Claude Code usa, sem documentação
    pública. Se a Anthropic mudar, o botão passa a mostrar um aviso; os tokens continuam funcionando.

## Controle deslizante { #volume-slider }

O **Controle deslizante (volume, brilho, música)** transforma o botão num fader: arraste o dedo nele
para mudar o valor.

| Controla | O que muda |
|---|---|
| **Som do PC (saída padrão)** | O volume do dispositivo de saída padrão do Windows. |
| **Microfone (entrada padrão)** | O nível do dispositivo de entrada padrão. |
| **Um aplicativo** | O volume de um app (**Processo do aplicativo**: o nome do processo sem `.exe`, ex.: `Spotify`, `chrome`, `Discord`). Todos os fluxos de áudio do app mudam juntos. |
| **Volume de uma fonte do OBS** | O volume de uma fonte de áudio do [OBS](../integrations/obs.md), na mesma curva do fader do OBS. |
| **Volume de uma pessoa no Discord** | O volume de quem está na call com você, de 0 a 200 % (100 % é o normal). O botão mostra o nome da pessoa. Precisa do [Discord](../integrations/discord.md) conectado. |
| **Brilho do monitor** | O brilho de todos os monitores juntos. |
| **Posição da música** | Em que ponto está a faixa que toca no Windows; o botão mostra o tempo e arrastar pula para outro ponto. |
| **Brilho de luz** (Smart Life/Tuya, Home Assistant, Philips Hue, Govee, Elgato Key Light) | O brilho de uma lâmpada (no Hue, também de um cômodo inteiro). Arrastar até zero apaga a luz. |

- O controle fica **em pé** quando o botão é mais alto que largo e **deitado** no contrário. Dê mais
  casas a ele em [Tamanho](actions.md#size) (1 × 3 ou 3 × 1, por exemplo) para ter mais precisão.
- Fontes do OBS, pessoas na call e lâmpadas aparecem numa lista que vem do PC. Se a lista vier vazia
  (integração desconectada, ninguém na call), dá para digitar o valor ou recarregar.
- Som e microfone seguem sempre o dispositivo padrão *atual*, então continuam funcionando depois de
  trocar de fone.
- O nível também muda por fora (teclas do PC, outro botão, o próprio app); o controle acompanha em
  poucos segundos. O preenchimento fica mais apagado enquanto o alvo está mudo.
- O controle de um app só encontra o app enquanto ele aparece no mixer de volume do Windows, na saída padrão.
- **Monitor**: monitores externos precisam do **DDC/CI** ligado no menu do próprio monitor; o de notebook
  usa o brilho do Windows.
- **Discord**: o Discord não informa o volume de cada pessoa a qualquer momento. Se você mudar o volume
  de alguém direto no Discord, o botão pode demorar a acompanhar.
- **Lâmpadas Smart Life**: só aparecem as que têm controle de brilho; interruptores e tomadas não.
- Quando o alvo não responde (OBS fechado, luz desligada da rede), o botão mostra o motivo no lugar do nome.
- O toque continua executando a ação do botão, se houver; segurar abre o editor. No configurador do PC
  o controle só mostra o nível, para você poder arrastar o botão de lugar.
