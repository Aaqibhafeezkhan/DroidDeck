# Seu primeiro botão

Com o celular [pareado](pairing.md), crie um botão que toca e pausa o que estiver tocando no PC. Leva
um minuto e mostra o ciclo inteiro: edita no PC, toca no celular.

## 1. Abra o configurador

No PC, clique com o botão direito no ícone do DroidDeck na bandeja → **Abrir no navegador** (ou dê
um clique duplo no ícone). O [configurador](configurator.md) abre em `http://localhost:4787/` e entra
sozinho.

![O configurador: perfis e ferramentas à esquerda, a grade de botões do celular no meio e o guia do botão escolhido à direita](../../assets/screenshots/configurator-pt.png){ .dd-shot width="1440" height="820" loading="lazy" }
O configurador tem três painéis: **Perfis** à esquerda, a **grade** de botões no meio e as
**propriedades** do botão selecionado à direita.

## 2. Crie o botão

1. Clique numa **casa vazia** da grade. O painel de propriedades mostra um botão novo.
2. Em **Ação**, escolha **Mídia**.
3. Deixe o **Comando** em **Play / Pause**. O **Rótulo** pode ficar vazio: botões de mídia se preenchem
   sozinhos.
4. Clique em **Salvar**.

O botão aparece no celular na hora; não tem nada para sincronizar.

## 3. Toque no celular

Comece uma música ou um vídeo no PC (Spotify, YouTube no navegador, qualquer player) e toque no botão
novo no celular. A reprodução pausa, e o ícone alterna entre ▶ e ⏸ para acompanhar.

!!! tip "Prefere editar no celular?"
    Toque numa casa vazia no celular para criar um botão ali, e segure um botão para editá-lo. Botões
    que enviam teclas, abrem programas ou rodam scripts só podem ser criados no PC; veja
    [Configurador](configurator.md#editando-no-celular).

## E agora

- [Botões e ações](../guide/actions.md): todos os tipos de ação, de atalhos de teclado a timers.
- [Integrações](../guide/actions.md#streaming): Discord, OBS, Spotify, lâmpadas inteligentes e mais.
- [Automações](../guide/automations.md): botões que se apertam sozinhos num horário ou quando um app abre.
- [Importar do Stream Deck](../guide/import-share.md#stream-deck): traga seus perfis da Elgato.
- [Galeria de perfis](https://me.gf2.in/DroidDeck/gallery/): decks prontos para começar.
