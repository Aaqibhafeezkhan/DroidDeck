---
name: DroidDeck
description: O celular parado vira um Stream Deck para o PC com Windows.
colors:
  deck-yellow: "#FDD400"
  deck-yellow-hi: "#FFE03D"
  ochre-ink: "#7A6000"
  night: "#0D0D0F"
  night-raised: "#141417"
  panel: "#18181C"
  key: "#2A2A30"
  hairline: "#26262C"
  hairline-strong: "#3F3F46"
  chalk: "#F4F4F5"
  ash: "#A1A1AA"
  ink: "#111111"
  live-red: "#B71C1C"
  signal-green: "#4ADE80"
typography:
  display:
    fontFamily: "Space Grotesk, Inter, system-ui, sans-serif"
    fontSize: "clamp(40px, 6vw, 66px)"
    fontWeight: 700
    lineHeight: 1.1
    letterSpacing: "-0.02em"
  headline:
    fontFamily: "Space Grotesk, Inter, system-ui, sans-serif"
    fontSize: "clamp(30px, 4vw, 42px)"
    fontWeight: 700
    lineHeight: 1.1
    letterSpacing: "-0.02em"
  title:
    fontFamily: "Space Grotesk, Inter, system-ui, sans-serif"
    fontSize: "20px"
    fontWeight: 700
    lineHeight: 1.2
  body:
    fontFamily: "Inter, system-ui, sans-serif"
    fontSize: "16px"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Inter, system-ui, sans-serif"
    fontSize: "13px"
    fontWeight: 600
    lineHeight: 1.3
  key-label:
    fontFamily: "Inter, Roboto, system-ui, sans-serif"
    fontSize: "13px"
    fontWeight: 600
    lineHeight: 1.15
rounded:
  key: "8px"
  key-outline: "10px"
  control: "12px"
  card: "18px"
  device: "42px"
  pill: "999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  xxl: "32px"
components:
  button-primary:
    backgroundColor: "{colors.deck-yellow}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "12px 22px"
    height: "48px"
  button-primary-hover:
    backgroundColor: "{colors.deck-yellow-hi}"
    textColor: "{colors.ink}"
  button-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.chalk}"
    rounded: "{rounded.control}"
    padding: "12px 22px"
    height: "48px"
  deck-key:
    backgroundColor: "{colors.key}"
    textColor: "{colors.chalk}"
    rounded: "{rounded.key}"
    typography: "{typography.key-label}"
  deck-key-active:
    backgroundColor: "{colors.live-red}"
    textColor: "{colors.chalk}"
    rounded: "{rounded.key}"
  deck-key-empty:
    backgroundColor: "{colors.night-raised}"
    textColor: "{colors.hairline-strong}"
    rounded: "{rounded.key}"
  connection-chip:
    backgroundColor: "{colors.night-raised}"
    textColor: "{colors.chalk}"
    rounded: "{rounded.pill}"
    padding: "6px 10px"
    typography: "{typography.label}"
  panel-card:
    backgroundColor: "{colors.panel}"
    textColor: "{colors.chalk}"
    rounded: "{rounded.card}"
    padding: "24px"
---

# Design System: DroidDeck

## Overview

**Creative North Star: "O painel na penumbra"**

O DroidDeck é um painel de teclas iluminado ao lado do monitor, num quarto escuro, durante a live ou o jogo. Tudo parte daí: o fundo é quase preto para não brilhar na cara de ninguém, as teclas são blocos sólidos que acendem quando algo está ligado, e o amarelo do logo é a única luz de destaque. A pessoa olha de relance, com uma mão, sem tirar a atenção da tela principal; por isso o sistema prefere estado legível a ornamento.

A densidade muda com a superfície, mas o mundo é um só. O deck do celular é o mais denso e o mais físico: grade de teclas que preenche a tela, rótulo e ícone proporcionais à tecla, toque que afunda. O configurador, o site e a documentação são a mesma noite com mais ar: superfícies em degraus de cinza-escuro, títulos em Space Grotesk, texto em Inter, amarelo para a ação principal e para o que está selecionado. A plataforma é adaptativa: no Android o app segue as convenções do Material; no navegador do iPhone e do iPad respeita os gestos e o recorte de tela de lá. A identidade mora no painel, nas cores e no tom, não em reinventar os controles do sistema.

Rejeições confirmadas: o azul padrão do Material como destaque (substituído pelo amarelo em 2026-10-18), etiquetas acima de títulos, grades de cartões iguais com ícone em quadradinho como estrutura de página, e qualquer imagem com chave de pareamento, IP ou perfis reais de alguém.

**Key Characteristics:**
- Escuro por padrão; o claro existe, mas é a exceção.
- Um único destaque: o amarelo do logo.
- Teclas sólidas que acendem; estado sempre visível, nunca só por cor.
- Plano, com profundidade por degraus tonais; sombra só no que flutua.
- Tátil e direto: alvos grandes, resposta imediata ao toque.

## Colors

Uma noite em degraus de cinza-escuro com uma única luz amarela; vermelho e verde só falam de estado.

### Primary
- **Amarelo DroidDeck** (deck-yellow): o destaque da marca, igual ao logo. Botão principal (texto em Tinta), item selecionado, foco do teclado, links no escuro, o "+" e os ícones de guia. No tema claro, texto amarelo vira Ocre.
- **Amarelo aceso** (deck-yellow-hi): hover do botão principal e do link no escuro.
- **Ocre** (ochre-ink): o amarelo quando ele precisa ser texto sobre branco (links da documentação no tema claro, destaque do app no tema claro). Contraste 6:1.

### Neutral
- **Noite** (night): fundo de tudo no escuro: deck, site, documentação, splash do app.
- **Noite elevada** (night-raised): segundo degrau; faixas de seção, casas vazias do deck, selos.
- **Painel** (panel): terceiro degrau; cartões, barras laterais do configurador, código.
- **Tecla** (key): cor de uma tecla sem cor própria no deck.
- **Fio** (hairline) e **Fio forte** (hairline-strong): divisórias, bordas de cartão, trilho de barra de rolagem.
- **Giz** (chalk): texto principal no escuro.
- **Cinza-cinza** (ash): texto secundário no escuro (7,6:1 sobre Noite). Nunca usar cinzas mais escuros que este para texto.
- **Tinta** (ink): texto sobre o amarelo.

### Estado
- **Vermelho ao vivo** (live-red): tecla ligada por padrão (mudo, gravando, live) quando o usuário não escolheu outra cor de ativo.
- **Verde sinal** (signal-green): conectado, no selo de conexão e no ponto do celular de exemplo.

### Named Rules
**A Regra da Luz Única.** O amarelo é a única cor de marca. Ele marca a ação principal e o que está escolhido; se tudo for amarelo, nada é. As cores das teclas pertencem ao usuário, não ao sistema.

**A Regra do Estado com Palavra.** Ligado, desligado, conectado e offline sempre têm texto ou ícone além da cor (selo "Conectado a PC", rótulo de estado ativo, ícone de mudo).

## Typography

**Display Font:** Space Grotesk (com Inter, system-ui)
**Body Font:** Inter (com system-ui)
**No app:** a fonte do sistema (Roboto no Android, SF no iPhone), seguindo a plataforma; o peso e a escala seguem os papéis abaixo.

**Character:** Space Grotesk dá aos títulos um ar de equipamento, geométrico e firme; Inter some no texto corrido e nos rótulos, que precisam ser lidos de relance.

### Hierarchy
- **Display** (700, clamp(40px, 6vw, 66px), 1.1): só o título do topo do site.
- **Headline** (700, clamp(30px, 4vw, 42px), 1.1): títulos de seção do site; H1 e H2 da documentação.
- **Title** (700, 20px, 1.2): títulos de cartão, passos, nome do perfil no configurador.
- **Body** (400, 16px, 1.6): texto corrido; linhas de 62ch na documentação (cerca de 75 caracteres).
- **Label** (600, 13px): idioma, selos, metadados, rótulos de campo.
- **Rótulo de tecla** (600, entre 10px e 16px, proporcional à tecla, 1.15): texto dentro das teclas do deck; até duas linhas.

### Named Rules
**A Regra do Relance.** Texto dentro de tecla nunca abaixo de 10px e sempre proporcional ao tamanho da tecla; se não couber, a tecla encolhe o conteúdo, nunca corta o rótulo pela metade.

## Layout

O deck é uma grade fixa de colunas por linhas definida pelo celular (quantas teclas de cerca de 96px cabem inteiras na tela), com 8px entre teclas, sem rolagem e centralizada. Botões grandes ocupam várias casas (largura × altura) e nunca cobrem outro botão. Fora do modo de edição, casas vazias ficam vazias; os "+" aparecem só para editar ou num deck ainda vazio.

O configurador tem três faixas: perfis e ferramentas (240px), grade no centro do maior tamanho que couber (teclas de 48px a 150px, 12px entre elas) e propriedades (360px). Abaixo de 1100px de largura, o painel de propriedades passa a flutuar por cima da grade.

O site usa um container de 1120px com margens de 20px e seções de respiro generoso; a documentação usa a coluna do Material com texto limitado a 62ch. Ritmo de espaçamento em 4/8/12/16/24/32px.

## Elevation & Depth

Plano, com camadas tonais: a profundidade vem dos degraus Noite → Noite elevada → Painel → Tecla, não de sombras. Sombra aparece só no que flutua sobre o resto: o celular e a moldura do vídeo no site, diálogos, o painel de propriedades sobreposto no configurador, e uma sombra curta sob as teclas sólidas do deck.

### Shadow Vocabulary
- **Aparelho** (`box-shadow: 0 40px 80px -20px rgba(0,0,0,.8), inset 0 0 0 1px #333`): o celular de exemplo e a moldura do vídeo no site.
- **Tecla** (`box-shadow: 0 2px 4px rgba(0,0,0,.3)`): teclas sólidas do deck.
- **Painel flutuante** (elevação 12 do Material): o painel de propriedades sobre a grade em telas estreitas.

### Named Rules
**A Regra do Chão Plano.** Superfícies em repouso não têm sombra. Sombra só para o que está fisicamente por cima de outra coisa.

## Shapes

Cantos médios e consistentes: teclas com 8px (sólida) ou 10px (contorno), controles com 12px, cartões com 18px, o aparelho com 42px e selos em pílula. O estilo "Moldura" das teclas tem um chanfro no canto inferior direito com um triângulo de cor: é a única geometria fora do retângulo arredondado.

## Components

### Buttons
Tátil e direto.
- **Shape:** cantos de controle (12px), altura mínima de 48px.
- **Primary:** fundo Amarelo DroidDeck, texto Tinta, peso 600.
- **Hover / Focus:** hover para Amarelo aceso; foco com anel de 2px no amarelo, afastado 3px.
- **Ghost:** transparente, borda de Fio, texto Giz; hover só escurece a borda. Uma ação principal por tela; o resto é ghost ou texto.

### Teclas do deck (componente assinatura)
- **Shape:** 8px (sólida), 10px (contorno) ou moldura chanfrada; três estilos escolhidos pelo usuário.
- **Cor:** a do usuário, ou Tecla; ícone e rótulo em Giz (ou preto quando a cor é clara).
- **Ativo:** cor de ativo do usuário, ou Vermelho ao vivo; rótulo e ícone do estado ativo quando definidos.
- **Toque:** afunda para 93% e escurece levemente em 60ms, volta em 140ms; vibra ao encostar. Com "reduzir movimento", sem animação.
- **Informação:** medidores em anel (CPU, GPU, RAM), rede com unidade (KB/s, MB/s), relógio, clima, sino de notificações com número, controle deslizante de volume que preenche a tecla.
- **Vazia:** Noite elevada com borda de Fio e "+" apagado; só no modo de edição.

### Selo de conexão
- **Style:** pílula com fundo do estado a 12%, borda a 45%, ícone e texto: "Conectado a PC" (Verde sinal), "Reconectando…" (âmbar), "PC desligado" (vermelho de erro).
- **Toque:** explica o que fazer; dados técnicos recolhidos.

### Cards / Containers
- **Corner Style:** 18px.
- **Background:** Painel sobre Noite.
- **Shadow Strategy:** nenhuma (ver Elevation & Depth).
- **Border:** 1px de Fio.
- **Internal Padding:** 24px.

### Inputs / Fields
- **Style:** contorno do Material (no app) ou borda de Fio com 12px (no site), fundo Painel.
- **Focus:** borda e anel no amarelo.
- **Error / Disabled:** erro no vermelho de erro do tema, com mensagem que diz o que fazer; desabilitado a 40%.

### Navigation
- **App:** barra inferior Áudio / Deck no celular; abre no Deck e lembra a última aba. No configurador, barra lateral com perfis e ferramentas com rótulo.
- **Site e documentação:** cabeçalho escuro com logo, links em Cinza-cinza que acendem para Giz, seletor EN/PT/ES com o idioma ativo em amarelo.

## Do's and Don'ts

### Do:
- **Do** usar o Amarelo DroidDeck (deck-yellow) para a ação principal e para o que está selecionado, com texto Tinta por cima.
- **Do** mostrar todo estado com palavra ou ícone além da cor (selo de conexão, rótulo ativo).
- **Do** manter o deck escuro por padrão e as superfícies em degraus Noite → Noite elevada → Painel.
- **Do** dimensionar rótulo e ícone pela tecla (rótulo entre 10px e 16px).
- **Do** usar o perfil de demonstração em qualquer imagem do produto.

### Don't:
- **Don't** voltar ao azul padrão do Material como destaque.
- **Don't** usar etiquetas pequenas acima de títulos nem grades de cartões iguais com ícone em quadrado como estrutura de página.
- **Don't** usar cinza mais escuro que Cinza-cinza (ash) para texto no escuro.
- **Don't** pôr sombra em superfície em repouso.
- **Don't** publicar imagens com chave de pareamento, IP, nome do PC ou perfis reais de alguém.
