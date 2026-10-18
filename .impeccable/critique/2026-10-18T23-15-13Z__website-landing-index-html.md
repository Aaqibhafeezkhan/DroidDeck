---
target: landing page
total_score: 27
max_score: 36
na_heuristics: 7
p0_count: 0
p1_count: 2
target_identity: "file:G:\\ggfto\\AnyDeck\\website\\landing\\index.html"
target_fingerprint: "sha256:3e756da9cb6d9945379fb85a8f8c30fe602a653db6de134d6b0a1d45faab3156"
target_path: "G:\\ggfto\\AnyDeck\\website\\landing\\index.html"
timestamp: 2026-10-18T23-15-13Z
slug: website-landing-index-html
---
# Critique 2 — Site DroidDeck (landing, galeria, compartilhar)

Method: dual-agent. Nota 27/36 (75%, Bom; heurística 7 n/a), antes 19/32 (59%).

| # | Heurística | Nota | Problema |
|---|---|---|---|
| 1 | Visibilidade | 3 | Versão ao vivo; botões preparando |
| 2 | Mundo real | 3 | "Apache 2.0", "tray", "replay buffer" |
| 3 | Controle | 3 | Idioma lembrado |
| 4 | Consistência | 3 | Mesma ação com 4 nomes; larguras diferentes entre páginas |
| 5 | Prevenção | 3 | SmartScreen, "nada é apagado" |
| 6 | Reconhecimento | 3 | Downloads numerados |
| 7 | Flexibilidade | n/a | Persuade |
| 8 | Estética | 3 | Recursos com 14 linhas iguais |
| 9 | Erros | 3 | /share/ vazio diz "corrompido" |
| 10 | Ajuda | 3 | Docs onde as decisões acontecem |

Detector: URL 0 achados; arquivos: design-system-font (Consolas, Material Symbols) falsos; desvios menores do DESIGN.md (raio 14px, tamanhos fora da escala).

## Prioridades
1. [P1] Download sem botão principal; "Baixar para Windows" só rola (clarify/bolder)
2. [P1] iPhone sem saída: botão leva a downloads inúteis no aparelho (adapt)
3. [P2] Fonte de ícones da galeria pode mostrar nomes crus (harden/optimize)
4. [P2] Recursos em parede de 14 linhas; animação perdida no celular (distill/animate)
5. [P3] Mesma ação com quatro nomes (clarify)
