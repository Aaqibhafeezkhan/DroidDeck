---
target: app Flutter (deck + configurador)
total_score: 28
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 2
target_identity: "file:G:\\ggfto\\AnyDeck\\app\\lib\\src\\stream_deck\\stream_deck_page.dart"
target_fingerprint: "sha256:e0a982ee89d1eabdde51e8e887f4eb64269ad2f3127c32ff0444c8b9961f1757"
target_path: "G:\\ggfto\\AnyDeck\\app\\lib\\src\\stream_deck\\stream_deck_page.dart"
timestamp: 2026-10-18T23-15-13Z
slug: app-lib-src-stream-deck-stream-deck-page-dart
---
# Critique 2 — App DroidDeck (Flutter)

Method: design review only (Flutter canvas). Nota 28/40 (Bom), antes 22/40.

| # | Heurística | Nota | Problema |
|---|---|---|---|
| 1 | Visibilidade | 3 | Selo some em tela cheia; primeira conexão aparece "Reconectando…" |
| 2 | Mundo real | 3 | "Opções do perfil" vago |
| 3 | Controle | 3 | Desfazer, modo de edição separado |
| 4 | Consistência | 2 | Snackbar claro; pílula vs 12px; ícones só no deck |
| 5 | Prevenção | 3 | Casas vazias quietas no uso |
| 6 | Reconhecimento | 3 | Ações do deck só ícone (sem dica no toque) |
| 7 | Flexibilidade | 3 | Gestos, busca, importação |
| 8 | Estética | 2 | Navegador em pé quebrado; rótulos cortados no configurador a 1000px |
| 9 | Erros | 3 | Cartão offline e selo explicam |
| 10 | Ajuda | 3 | Boas-vindas e guia no configurador |

## Prioridades
1. [P1] Deck no navegador usa grade 10x4 do celular em qualquer tela; teclas ~43px em pé (adapt)
2. [P1] Snackbar sem tema e instrução errada em tela cheia (polish)
3. [P2] Estado some em tela cheia; "Conectando" vs "Reconectando" (harden)
4. [P2] Configurador corta rótulos e não usa a altura (layout)
5. [P3] "Novo perfil" em pílula; barra de baixo padrão no celular deitado (polish)

## Personas
- Jordan: ações só ícone; "Reconectando…" na primeira abertura.
- Casey: teclas pequenas em pé; tela cheia sem estado; snackbar cobre o botão.
- Sam: estado das teclas não é lido; medidores sem valor; foco do configurador fraco.
