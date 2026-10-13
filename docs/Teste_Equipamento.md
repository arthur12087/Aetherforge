# Teste de equipamento entre idle e MMO

## Executar manualmente

1. Abra `Testar Integração.cmd`. Ele inicia a API, o servidor de mapa, o MMO e o idle.
2. Aguarde aproximadamente 12–15 segundos pela primeira vitória do idle. O serviço gera uma **Couraça de Latão incomum**, com **+3 de defesa**, originada na Sentinela das Galerias. O drop introdutório é garantido uma vez por conta de teste.
3. Clique **Enviar para o MMO** no idle. O mesmo item sai do escopo idle e passa ao inventário compartilhado. O ID curto mostrado nas duas telas deve ser igual.
4. No MMO, clique **Equipar armadura**. A defesa muda de **10 para 13**, o personagem ganha destaque de latão e sua etiqueta mostra a defesa equipada.
5. Clique **Canhão de teste · dano bruto 30**. O servidor calcula o dano: **20 sem a peça, 17 com ela**, e reduz a vida do personagem. Desequipe e repita para comparar. Ao chegar a zero, o próximo disparo recupera a vida antes de aplicar dano, permitindo repetir o teste.
6. Feche os dois clientes e reabra o atalho. O equipamento e sua posição no inventário permanecem salvos.

O estado da demonstração fica em `data/integration-state.json`. Reabrir não gera outra cópia introdutória. O equipamento vincula à conta ao equipar; desequipar devolve o mesmo ID à mochila e remove seu bônus. A demo trabalha com uma conta e um perfil de personagem locais, não com autenticação de contas reais.

## O que é validado

```text
Vitória calculada pela API
        ↓
Um equipamento com ID único no idle
        ↓  Enviar para o MMO
Mesmo ID no inventário compartilhado
        ↓  Equipar
Mesmo ID no slot de armadura + defesa 13
        ↓  Canhão
Dano recebido 17 em vez de 20
```

O cliente não informa vitória, bônus ou quantidade de drops à API. Ele envia presença e intenções de transferência/equipamento. Um worker calcula o combate; um drop é registrado quando o inimigo morre. Mais clientes idle conectados não criam workers ou equipes adicionais.

A ausência de heartbeat por três segundos pausa a simulação. Isso é comportamento do teste online; o processamento offline de oito horas ainda não foi implementado. Os checkpoints de combate não são persistidos neste incremento; os equipamentos e comandos são persistidos.

Mudanças de escopo, vínculo e equipamento usam um único arquivo de estado, protegido por lock de processo e escrita por arquivo temporário seguido de substituição. Cada comando tem chave idempotente, ação e item. Repetir o mesmo pedido retorna o recibo anterior; usar sua chave para outro pedido é recusado. O snapshot contém uma única instância do item em todos os estágios.

## Evidências executadas

Execute os comandos na raiz do repositório. As evidências em `artifacts/` são locais e não acompanham o clone; os scripts abaixo as regeneram.

```powershell
.\scripts\Build.ps1
.\scripts\Test-Integration.ps1 -Screenshots
```

O teste automatizado inicia API, servidor de mapa e dois clientes Godot reais. Os clientes acionam os mesmos handlers dos botões da interface. O resultado deve conter `INTEGRATION idle PASS` e `INTEGRATION map PASS`, com o mesmo ID, defesa 13 e dano 17. A API é reiniciada e o ID, equipamento e bônus são novamente verificados.

- `artifacts/transfer-idle.log`: confirmação do drop enviado pelo idle.
- `artifacts/transfer-mmo.log`: equipar, dano e resultado no MMO.
- `artifacts/integration-idle.png` e `integration-mmo.png`: telas verificadas visualmente.
- Os testes de domínio/persistência verificam equipar antes de transferir, concorrência, repetição após reinício, segunda transferência, uso indevido de chaves e desequipar.

Os arquivos e portas do teste automatizado são separados dos da demonstração manual. Nenhum pagamento, marketplace ou recurso com valor real faz parte deste teste.

## Próximas extensões

Substituir o drop introdutório pela tabela de loot das rotas, permitir múltiplos itens e slots, validar compatibilidade por classe e durabilidade, e migrar a persistência para PostgreSQL com contas autenticadas. O teste atual valida o ciclo pedido antes de ampliar esses sistemas.
