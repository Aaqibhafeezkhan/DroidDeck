# Importar e compartilhar

Três jeitos de trazer botões para o DroidDeck além de montar você mesmo: importar um perfil do app
Stream Deck da Elgato, abrir um perfil que alguém compartilhou como link, ou partir da galeria de
perfis. Todos entram como **perfis novos**; nada do seu deck é sobrescrito. Para levar *tudo* de um PC
para outro, use o [backup](profiles.md#backup).

## Importar do Stream Deck { #stream-deck }

O DroidDeck lê os arquivos `.streamDeckProfile` exportados pelo app Stream Deck da Elgato (no app
Stream Deck: lista de perfis → **Export**).

1. Abra o configurador no PC (`http://localhost:4787/`).
2. Na barra lateral, clique em **Importar do Stream Deck** e escolha o arquivo.

Cada página do perfil do Stream Deck vira um perfil do DroidDeck, com o nome dele (`Nome`,
`Nome (2)`…). Pastas viram perfis abertos por um botão [Abrir perfil](actions.md#open-profile-and-back),
com o nome `Nome — <botão da pasta>`. No fim, uma mensagem diz quantos perfis entraram e quais ações
ficaram sem equivalente.

| Ação do Stream Deck | Vira |
|---|---|
| Hotkey, Hotkey Switch | [Atalho de teclado](hotkeys.md) (teclas convertidas para a sintaxe do DroidDeck) |
| Text | Atalho que digita o texto (mais Enter, se estava marcado) |
| Open, Website | [Abrir app](actions.md#launch-app) |
| Multimedia | Tocar/pausar, próxima, anterior; aumentar/diminuir volume e mudo da saída padrão |
| Create Folder / Parent Folder | Abrir perfil / Voltar |
| Multi Action | [Multiação](multi-action.md); passos de *Delay* viram a espera do passo anterior |
| Plugin do Discord (oficial): mudo, ensurdecer | [Discord](../integrations/discord.md) alternar mudo / ensurdecer |
| Plugin do OBS (oficial): gravar, transmitir, replay buffer, câmera virtual, cena | Os equivalentes do [OBS](../integrations/obs.md) |

- **Imagens**: a imagem própria de cada botão vem junto, reduzida para 144 px (GIFs animados de até
  2 MB vêm como estão). O título vira o rótulo.
- **Segundo estado**: o rótulo e a imagem do segundo estado de uma tecla viram o rótulo e o ícone
  *ativos* do botão; um atalho ou app com dois estados vira um botão de [dois estados](actions.md#two-states).
- **O resto** (outros plugins, Soundboard, timers, troca de página...) fica com o título e a imagem, mas
  sem ação. A mensagem lista esses botões para você configurar à mão.
- **Tamanho da grade**: se o perfil do Stream Deck cabe na sua grade, os botões ficam nas mesmas
  posições. Se for maior (um Stream Deck XL num celular, por exemplo), os botões são reorganizados em
  ordem de leitura, da esquerda para a direita e de cima para baixo; os que ainda assim não couberem
  ficam de fora.

!!! info "Atalhos e apps precisam do PC"
    Importe pelo configurador no PC. Feita de um navegador em outro aparelho, a importação traz os botões
    Hotkey, Text, Open e Website (e as multiações com eles) sem a ação (a mensagem diz quantos), pela
    mesma regra das [ações privilegiadas](actions.md).

!!! warning "Suporte ao formato"
    A Elgato não documenta o formato `.streamDeckProfile`. O importador foi escrito a partir da estrutura
    conhecida (layouts do Stream Deck 4/5 e 6+) e testado com arquivos sintéticos, não com todas as
    versões do app. Se um perfil não importar direito, abra uma
    [issue](https://github.com/ggfto/DroidDeck/issues) — anexar o arquivo ajuda.

## Compartilhar um perfil por link { #share }

Abra as configurações do perfil — o ícone de controles na barra do deck, ou o lápis acima da grade no
configurador — e toque em **Compartilhar**. Você recebe:

- um link para `me.gf2.in/DroidDeck/share/…` (**Copiar link** ou **Enviar**), e
- um QR code, quando o perfil é pequeno o bastante para caber em um.

O perfil vai **dentro do link** (comprimido, depois do `#`); ele não é enviado para nenhum servidor.
Quem abre vê uma prévia dos botões e clica em **Importar no DroidDeck deste PC**. Isso abre o
configurador dessa pessoa em `http://localhost:4787/?import=…`, que pergunta antes de adicioná-lo como
um perfil novo.

- **Imagens próprias não vão** (o link ficaria grande demais); ícones embutidos, rótulos, cores e ações
  vão. Para levar as imagens também, use o [backup](profiles.md#backup).
- A troca automática pelo app e a marca de "perfil padrão" não são compartilhadas.
- Como na importação do Stream Deck, ações privilegiadas só entram quando a importação é feita no
  próprio PC: o DroidDeck precisa estar aberto no computador onde o link é aberto.

!!! warning "Confira antes de compartilhar"
    O link leva os parâmetros de todos os botões: URLs de webhook, cabeçalhos HTTP, comandos, entidades
    do Home Assistant... Tire o que for particular (tokens, endereços da sua rede) antes de enviar.

## Galeria de perfis { #gallery }

A [galeria de perfis](https://me.gf2.in/DroidDeck/gallery/) tem decks prontos para começar (Streamer,
Meetings, Music, Productivity, Gaming). **Importar** funciona como um link compartilhado: abre o
DroidDeck deste PC e adiciona o perfil. Depois, ajuste nomes, cenas e atalhos ao seu setup.

Fez um bom? Compartilhe e poste o link nas [Discussions](https://github.com/ggfto/DroidDeck/discussions).
