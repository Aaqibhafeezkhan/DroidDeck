---
target: landing page
total_score: 19
max_score: 32
na_heuristics: 7,10
p0_count: 0
p1_count: 2
target_identity: "file:G:\\ggfto\\AnyDeck\\website\\landing\\index.html"
target_fingerprint: "sha256:bd152ff4c4e5c15adf13f26e300eb844a172e305ad7e709f126ad401b7f15087"
target_path: "G:\\ggfto\\AnyDeck\\website\\landing\\index.html"
timestamp: 2026-10-18T21-41-49Z
slug: website-landing-index-html
---
# Critique — Landing DroidDeck (index, galeria, compartilhar)

Method: dual-agent. Nota 19/32 (Aceitável; 7 e 10 n/a em superfície Persuade).

| # | Heurística | Nota | Problema |
|---|---|---|---|
| 1 | Visibilidade | 2 | Sem versão/data; Importar sem feedback com DroidDeck fechado |
| 2 | Mundo real | 3 | "REST + SignalR", localhost:4787, "Tuya-based" |
| 3 | Controle | 3 | Erro do share sem "voltar à galeria" |
| 4 | Consistência | 2 | Galeria/share ignoram ?lang e dd-lang; ES vai para docs em inglês |
| 5 | Prevenção | 2 | CTA com logo Windows no celular; sem aviso de SmartScreen/APK; links vazios até o encode |
| 6 | Reconhecimento | 3 | Passo 3 exige lembrar localhost:4787 |
| 7 | Flexibilidade | n/a | Superfície Persuade |
| 8 | Estética | 3 | Terço inferior do celular vazio; ritmo de cartões repetido |
| 9 | Erros | 1 | share/#lixo: erro + dica de botão removido, sem saída |
| 10 | Ajuda | n/a | Docs são outra superfície |

Detector: low-contrast ×4 (#71717a, real), icon-tile-stack ×9 (real), overused-font (Inter/Space Grotesk), rótulos 9px da galeria (real); falsos: dark-glow do ponto, 10px dos medidores, nested-cards da galeria.

## Prioridades
1. [P1] Download sem reasseguramento e cego ao aparelho (onboard/clarify)
2. [P1] O produto real nunca aparece; galeria escondida no rodapé (layout/bolder)
3. [P2] Idioma inconsistente entre páginas (harden)
4. [P2] Erros e botões mortos na galeria/compartilhar (harden)
5. [P2] Celular de exemplo inacessível (divs) e meio vazio (polish/audit)

## Personas
- Jordan: primeira frase é sobre outro produto; jargão; sem aviso do SmartScreen.
- Riley: share inválido sem saída; Importar antes do encode; JS desligado esconde seções.
- Casey: mockup abaixo da dobra; botão Windows no Android; Importar inútil no celular; rótulos 9px.

## Perguntas
- Captura real do app no lugar da ilustração?
- História diferente por aparelho?
- Galeria como segunda seção da landing?
