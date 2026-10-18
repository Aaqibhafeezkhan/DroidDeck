# Govee

Lâmpadas e fitas Govee com **API LAN** podem ser controladas direto pela rede local, sem conta Govee.

## Configuração

1. No app **Govee Home**, abra as configurações de cada aparelho e ligue o **Controle LAN** (LAN Control).
   Nem todo modelo tem.
2. Crie um botão do tipo **Govee**. A lista **Aparelho** procura na rede ao abrir (multicast UDP em
   `239.255.255.250:4001`); se o aparelho não aparecer, digite o IP dele.

Dê um IP fixo ao aparelho no roteador para os botões continuarem funcionando.

## O que dá para fazer

| Operação | Parâmetros |
|---|---|
| Liga/desliga / Ligar / Desligar | — |
| Definir o brilho | 1 a 100 % |
| Definir a cor | `#RRGGBB` |
| Definir a temperatura (branco) | 2000 a 9000 K |

!!! note "Portas"
    Os comandos vão para o aparelho na porta UDP `4003`, e as respostas (a busca e o estado que o
    **Liga/desliga** precisa) voltam ao PC na porta UDP `4002`. Se outro programa do PC já usa a porta
    4002, os botões Govee não funcionam. Se a busca não acha nada mas o IP funciona, um firewall está
    bloqueando as respostas — **Ligar** e **Desligar** continuam funcionando, porque não esperam resposta.
