# Atalhos de teclado

Um botão de **Atalho de teclado** envia teclas para **a janela que está em foco** no PC. Digite-as em
**Teclas** com a sintaxe *SendKeys* do Windows, mais `#` para a tecla Windows.

## Exemplos comuns { #common-examples }

| Teclas | Envia |
|---|---|
| `^c` | Ctrl+C |
| `^+s` | Ctrl+Shift+S |
| `%{F4}` | Alt+F4 |
| `#d` | Win+D (mostrar a área de trabalho) |
| `#+s` | Win+Shift+S (ferramenta de captura) |
| `^+m` | Ctrl+Shift+M (mudo em muitos apps de reunião) |
| `{MEDIA_PLAY_PAUSE}` | A tecla de tocar/pausar mídia |

## Sintaxe { #syntax }

| Símbolo | Tecla | Exemplo |
|---|---|---|
| `^` | Ctrl | `^c` → Ctrl+C |
| `+` | Shift | `^+s` → Ctrl+Shift+S |
| `%` | Alt | `%{F4}` → Alt+F4 |
| `#` | Windows | `#d` → mostrar a área de trabalho |
| `( )` | Segura os modificadores num grupo | `+(abc)` → ABC |
| `~` | Enter | `^a~` |
| `{NOME}` | Tecla especial | `{ENTER}`, `{TAB}`, `{F5}` (veja abaixo) |
| `{NOME n}` | Repetir | `{TAB 3}` |
| `{x}` | O próprio caractere | `{+}`, `{^}`, `{%}`, `{#}`, `{~}`, `{(}`, `{{}` |

Qualquer outro caractere é digitado como está. Com Ctrl, Alt ou Windows, letras ignoram maiúsculas:
`^C` é Ctrl+C; use `+` para o Shift.

??? note "Todos os nomes de teclas especiais"
    `{ENTER}`, `{TAB}`, `{ESC}`, `{SPACE}`, `{F1}`…`{F24}`, `{UP}`, `{DOWN}`, `{LEFT}`, `{RIGHT}`,
    `{HOME}`, `{END}`, `{PGUP}`, `{PGDN}`, `{INS}`, `{DEL}`, `{BS}`, `{PRTSC}`,
    `{NUMPAD0}`…`{NUMPAD9}`, `{VOLUME_UP}`, `{VOLUME_DOWN}`, `{VOLUME_MUTE}`,
    `{MEDIA_PLAY_PAUSE}`, `{MEDIA_NEXT}`, `{MEDIA_PREV}`.

As teclas saem com o scancode do teclado e ficam apertadas por um instante, então a maioria dos
jogos que leem o teclado diretamente também as recebe.

!!! warning "Limitações"
    - O Windows bloqueia entradas para apps rodando **como administrador**, a menos que o DroidDeck também rode como administrador.
    - Jogos com anti-cheat podem ignorar teclas simuladas.
    - Algumas combinações são reservadas pelo Windows e não podem ser simuladas, como Ctrl+Alt+Del e Win+L.
      Para travar o PC, use a ação [Windows](actions.md#windows).

!!! info "Só é criado no PC"
    Teclas fazem tudo o que você faria no teclado, então botões de atalho só podem ser criados ou
    alterados no [configurador](../getting-started/configurator.md), no PC. O celular executa esses
    botões normalmente.

Para enviar uma sequência (teclas, uma pausa, mais teclas), use uma [Multiação](multi-action.md), ou
grave-a com [Gravar do teclado](multi-action.md#record).
