---
target: app Flutter (deck + configurador)
total_score: 22
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 3
target_identity: "file:G:\\ggfto\\AnyDeck\\app\\lib\\src\\stream_deck\\stream_deck_page.dart"
target_fingerprint: "sha256:8f426daa72465fd71047e34e12e6196110169aae11766972e14f3f84ca389e47"
target_path: "G:\\ggfto\\AnyDeck\\app\\lib\\src\\stream_deck\\stream_deck_page.dart"
timestamp: 2026-10-18T21-41-49Z
slug: app-lib-src-stream-deck-stream-deck-page-dart
---
# Critique — App DroidDeck (Flutter: deck + configurador)

Method: design review only (Flutter canvas; detector não se aplica). Nota 22/40 (Aceitável).

| # | Heurística | Nota | Problema |
|---|---|---|---|
| 1 | Visibilidade | 2 | Indicador de conexão é ponto de 12px de debug ("Conn Error", lastDebugData); botões sem estado pressionado |
| 2 | Mundo real | 3 | "Grade do celular… (definida pelo aparelho)", automações pedem "obs64" e "1-5" |
| 3 | Controle | 2 | "Limpar botão" sem confirmação/desfazer; troca de botão descarta edição; toque longo abre editor |
| 4 | Consistência | 2 | ?deck abre em Áudio; "Exportar… para importar use o PC" acima de "Importar backup" |
| 5 | Prevenção | 2 | Lixeira fixa ao lado de cada perfil; campos livres das automações sem validação |
| 6 | Reconhecimento | 2 | 5 ícones sem rótulo na barra; dropdown de ~28 ações sem busca; pasta sem rótulo |
| 7 | Flexibilidade | 3 | Arrastar, gestos, multi-ação, automações; falta atalho de teclado/duplicar |
| 8 | Estética | 2 | Grade limitada a 800px, rótulos 10px cortados; 25 de 40 "+" no deck ao vivo |
| 9 | Erros | 2 | Fallback mostra exceção crua; "Não deu para salvar." |
| 10 | Ajuda | 3 | Boas-vindas ótimas; sem ajuda no configurador |

## Prioridades
1. [P1] Edições perdidas/apagadas sem aviso (harden)
2. [P1] ?deck abre em Áudio e Áudio estoura em retrato 412px (adapt)
3. [P1] Status de conexão é artefato de debug (clarify)
4. [P2] Configurador desperdiça a tela; 5 ícones sem rótulo; sem colapso <1100px (layout)
5. [P2] Seletor de ação: lista de 28 sem busca (distill)

## Personas
- Jordan: ícone da bandeja escondido; puzzle/paleta/ponto sem rótulo; "obs64".
- Casey: sem estado pressionado; "+" abre editor fora do modo edição; Áudio cortado em retrato.
- Sam: zero Semantics no app; estado só por cor; rótulos 10px fixos.

## Menores
Splash azul vs logo amarelo; flash branco em /#/config; "localhost" como nome do PC; NET sem unidade.

## Perguntas
- Amarelo dos hexágonos como acento e linguagem de tecla?
- "+" só no modo de edição?
- Abrir a página do QR sozinho na primeira execução?
