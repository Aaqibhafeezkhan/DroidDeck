# Philips Hue

O DroidDeck controla o Philips Hue pelo **Hue Bridge** da sua rede (API local v1), sem conta Hue nem
nuvem.

## Configuração (pareamento)

1. Abra **Configuração → Integrações → Philips Hue** (no configurador, **Integrações** na barra lateral).
2. Digite o **IP** do bridge (app Hue: **Configurações → Bridges**) ou deixe vazio para procurar
   (usa o `discovery.meethue.com`, então precisa de internet).
3. Aperte o **botão redondo em cima do Hue Bridge**.
4. Em até 30 segundos, toque em **Parear**.

O bridge entrega um usuário ao DroidDeck, guardado cifrado no PC (`hue.json`). Se o botão não foi
apertado, o DroidDeck pede para apertar e tocar em **Parear** de novo.

## O que dá para fazer

| Operação | Parâmetros |
|---|---|
| Liga/desliga / Ligar / Desligar | Uma luz, ou uma sala/zona (ou **Todas as luzes**). A sala está "ligada" com qualquer luz acesa |
| Definir o brilho | 1 a 100 % (acende) |
| Brilho (+/-) | Passo de -100 a 100 %. Diminuir não acende uma luz apagada |
| Ativar cena | Uma cena da lista (com o nome da sala) |

Luzes, salas e cenas vêm do bridge. O PC confere o bridge a cada poucos minutos; se ele parar de responder,
o editor avisa que o Hue não está conectado.
