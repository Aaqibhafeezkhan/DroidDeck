# Elgato Key Light

As luzes Elgato **Key Light**, **Key Light Air**, **Key Light Mini** e **Light Strip** têm uma pequena API
HTTP na rede local (porta `9123`). O DroidDeck usa essa API direto — o Elgato Control Center não precisa
estar aberto, e não tem conta.

## Configuração

Cada botão leva o **IP da luz**. Ele aparece no Elgato Control Center → configurações da luz, ou no seu
roteador. Dê um IP fixo para a luz (reserva de DHCP no roteador) para os botões continuarem funcionando.
Porta diferente vai depois de dois-pontos: `192.168.0.50:9123`.

## O que dá para fazer

| Operação | Parâmetros |
|---|---|
| Liga/desliga / Ligar / Desligar | — |
| Definir o brilho | 1 a 100 % (também acende a luz) |
| Brilho (+/-) | Passo de -100 a 100 % |
| Definir a temperatura | 2900 a 7000 K |
| Temperatura (+/-) | Passo em K; positivo deixa a luz mais fria (azulada) |

Com várias luzes no mesmo aparelho, todas mudam. A Light Strip tem cor em vez de temperatura: nela, use
liga/desliga e brilho.
