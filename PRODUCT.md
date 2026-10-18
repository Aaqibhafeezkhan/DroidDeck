# Product

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

O deck e o configurador são um único app Flutter: Android (APK) e navegador (iPhone, iPad, tablet, outro PC, e o próprio PC como configurador). A interface deve respeitar as convenções de cada sistema onde roda. Há também a parte do PC: um app de bandeja do Windows (menu da bandeja e janela de pareamento em WinForms) e o servidor local.

## Users

Principal: streamers e gamers de PC com Windows que querem um Stream Deck sem comprar um. O celular velho fica ao lado do monitor durante a live ou o jogo, muitas vezes num quarto escuro, e é usado com uma mão, de relance, sem tirar a atenção da tela principal. Muitos não são técnicos: chegam pelo APK baixado ou pelo site, sem ler o README.

Secundários, confirmados pelo uso do produto: quem usa o deck para reuniões, música, produtividade e casa inteligente; e power users que automatizam o PC (scripts, API REST, MQTT, Home Assistant).

Idiomas: português do Brasil (origem do projeto), inglês e espanhol no app, no site e na bandeja; a documentação existe em inglês e português.

## Product Purpose

Transformar um celular Android parado, ou qualquer aparelho com navegador, num painel de botões para o PC: atalhos, áudio e mixer, mídia, monitores ao vivo, Discord, OBS, soundboard, luzes e casa inteligente, automações. O PC executa as ações; o celular é o painel. Sucesso é o usuário instalar as duas partes, parear, criar o primeiro botão e usar o deck durante a live ou o jogo sem pensar nele.

## Positioning

- Grátis e de código aberto (Apache 2.0): sem conta, sem assinatura, sem hardware para comprar.
- Qualquer aparelho: reaproveita o celular da gaveta; iPhone e iPad pelo navegador; tablet ou outro PC também viram deck.
- Local e seguro: celular e PC conversam direto pela rede local ou pelo cabo USB, com chave de pareamento; ações que executam código (abrir programa, atalho, script, HTTP) só são criadas ou alteradas no próprio PC.
- Integrações nativas prontas, sem caçar plugins: Discord, OBS, Spotify, Twitch, YouTube, Home Assistant, Tuya/Smart Life, Voicemeeter, Elgato Key Light, Philips Hue, Govee, MQTT, Streamer.bot, VTube Studio, soundboard (MyInstants), Steam.

Existe outro projeto chamado DroidDeck (roda jogos da Steam no Android). Toda superfície pública precisa deixar claro, sem alarde, que este controla o PC com Windows.

## Operating Context

- Duas partes obrigatórias: o DroidDeck do Windows (instalador DroidDeck-Setup.exe, fica na bandeja, atualiza sozinho) e o app do celular (APK) ou o deck no navegador.
- Pareamento por QR na janela da bandeja; conexão pela rede Wi-Fi ou pelo cabo USB (adb reverse).
- O configurador (editor de 3 painéis) abre no navegador do PC em localhost:4787; o celular também edita botões no modo de edição.
- A grade de botões é definida pelo tamanho da tela do celular; perfis, pastas, botões grandes, dois estados, gestos, automações e importação de perfis do Stream Deck da Elgato.
- O deck é usado durante a live: interrupções, pressa, ambiente escuro, olhar rápido.

## Capabilities and Constraints

- Windows 10/11 no PC; Android 7+ no celular; navegador moderno para o deck web (iPhone sem app nativo: publicar na App Store exige conta paga, decisão adiada).
- Sem servidor na nuvem: tudo roda no PC do usuário.
- Ações privilegiadas (programa, atalho, janela, script, HTTP, comando, URL) só no configurador do PC; conexões do celular, inclusive pelo USB, contam como remotas.
- Linux e macOS no PC: fora do escopo por enquanto.
- Distribuição: GitHub Releases, site me.gf2.in/DroidDeck, winget (em revisão). Play Store e F-Droid ainda não (exigem cadastro).

## Brand Commitments

- Nome: DroidDeck. Logo: três hexágonos amarelos.
- Voz: direta, concreta, informal (você), sem jargão para quem não é técnico; frases como "Dê um novo trabalho àquele celular antigo" e "Pareado, não aberto".
- O amarelo do logo é o destaque da marca em todas as superfícies (decisão do usuário em 2026-10-18).

## Evidence on Hand

- Gravação real do deck com um perfil de demonstração: website/docs/assets/demo.gif e website/landing/img/demo.webm/mp4.
- Imagem de compartilhamento: website/landing/og.png.
- Galeria com cinco perfis prontos: website/landing/gallery/.
- Não há depoimentos, números de usuários, imprensa nem selos: não inventar.

## Product Principles

1. O primeiro uso decide: instalar, parear e o primeiro botão funcionando precisam ser óbvios para quem não é técnico.
2. O deck ao vivo é sagrado: nada de surpresas durante a live (editar só no modo de edição, estado sempre visível, feedback a cada toque).
3. Local e seguro por padrão: o que executa código no PC nasce no PC; nunca expor chave, IP ou dados pessoais.
4. Funciona no aparelho que a pessoa já tem: Android, navegador, cabo ou Wi-Fi.
5. Grátis de verdade: nenhum recurso atrás de conta ou pagamento.

## Accessibility & Inclusion

- Leitor de tela nos botões do deck e estados que não dependem só de cor.
- Uso com uma mão e de relance: alvos de toque grandes, feedback visual e por vibração.
- Três idiomas no app (pt-BR, en, es).
