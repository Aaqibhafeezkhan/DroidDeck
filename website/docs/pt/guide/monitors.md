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

## Controle deslizante de volume { #volume-slider }

O **Controle deslizante de volume** transforma o botão num fader: arraste o dedo nele para mudar o
volume.

| Controla | O que muda |
|---|---|
| **Som do PC (saída padrão)** | O volume do dispositivo de saída padrão do Windows. |
| **Microfone (entrada padrão)** | O nível do dispositivo de entrada padrão. |
| **Um aplicativo** | O volume de um app (**Processo do aplicativo**: o nome do processo sem `.exe`, ex.: `Spotify`, `chrome`, `Discord`). Todos os fluxos de áudio do app mudam juntos. |

- O controle fica **em pé** quando o botão é mais alto que largo e **deitado** no contrário. Dê mais
  casas a ele em [Tamanho](actions.md#size) (1 × 3 ou 3 × 1, por exemplo) para ter mais precisão.
- Ele segue sempre o dispositivo padrão *atual*, então continua funcionando depois de trocar de fone.
- O nível também muda por fora (teclas do PC, outro botão); o controle acompanha em poucos segundos. O
  preenchimento fica mais apagado enquanto o alvo está mudo.
- O controle de um app só encontra o app enquanto ele aparece no mixer de volume do Windows, na saída padrão.
- O toque continua executando a ação do botão, se houver; segurar abre o editor. No configurador do PC
  o controle só mostra o nível, para você poder arrastar o botão de lugar.
