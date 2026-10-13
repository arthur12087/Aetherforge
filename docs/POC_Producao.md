# Aetherforge: backlog de produção da POC

Referência: GDD v0.3 e Stack_Tecnologica.md. A base do M0 está implementada: ferramentas portáteis, domínio compartilhado, API, renders direcionais, idle animado e dois clientes com movimento autoritativo. A matriz completa de desktop permanece em validação. Os marcos M1 a M5 ainda não estão implementados. Evidências e pendências em [Validacao_M0.md](Validacao_M0.md).

**Prioridade atual (13/10/2026): desenvolver o idle primeiro.** Executar M1 antes de ampliar o MMORPG. Persistência e offline entram depois de consolidar o ciclo idle; os marcos de MMO e marketplace permanecem como planejamento futuro. [Ordem de trabalho atual](Desenvolvimento.md).

## M0: provar as escolhas técnicas

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| TEC-01 | Fixar Godot .NET estável, SDK cliente, .NET 10 servidor e targets do domínio | — | Cliente e API compilam e usam a mesma biblioteca sem depender de Nodes |
| TEC-02 | Prototipar janela idle transparente, foco e posição junto à barra | TEC-01 | Passa pela matriz Windows/DPI/monitores descrita na stack, com dados de consumo registrados |
| ART-00 | Buscar assets gratuitos de personagens, ambiente, UI e áudio; registrar origem e licença | — | Catálogo com links, licença e atribuição; placeholders separados dos assets finais |
| ART-01 | Renderizar modelo pronto ou importar sprites prontos de um personagem em oito direções | TEC-01, ART-00 | Câmera rotacionável mantém direção, pivô e tamanho; registrar uso de textura |
| NET-01 | Conectar dois clientes a um processo Godot headless | TEC-01 | Ambos veem movimento validado pelo servidor; desconexão não trava a instância |

Se TEC-02 falhar após ajustes, comparar shell WPF e medir de novo. Se NET-01 falhar por incompatibilidade de runtimes, corrigir versões antes de produzir conteúdo.

## M1: idle jogável e regras verificáveis

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| IDL-01 | Implementar DPS, tank, healer e prioridades | TEC-01 | Tank atrai ataques comuns; drone pode focar healer; cura respeita energia e HP máximo |
| IDL-02 | Implementar níveis, XP, três slots e raridade | IDL-01 | Limite de nível 20; bônus e XP conforme GDD; função incompatível não equipa arma |
| IDL-03 | Construir Galerias da Caldeira e Capataz | IDL-02 | Cinco vitórias avançam etapa; chefe desbloqueia próxima rota; derrota recua com mínimo 1 |
| IDL-04 | Construir mineração, acumuladores e emboscadas | IDL-03 | Unidades fracionárias se acumulam; troca de missão descarta apenas ciclo incompleto |
| IDL-05 | Implementar durabilidade e reparo | IDL-02 | Item a zero perde bônus; custo por ponto/peça confere; saldo insuficiente não causa débito parcial |
| UI-01 | Mostrar equipe, rota, etapa, recursos e ações na janela | TEC-02, IDL-04, IDL-05 | Trocar missão/equipamento entre encontros, entender derrota e abrir gerenciamento |
| ART-02 | Adaptar assets prontos para os três integrantes e inimigos essenciais | ART-01, IDL-03 | Silhuetas legíveis no tamanho da barra; atlas no orçamento de memória |

Verificação significativa: simulação sem interface cobre morte do tank, falta de energia, vitória, derrota na etapa 1, bônus zerado e conservação das frações de mineração. Playtest ajusta os números para que a primeira barreira não seja um bloqueio permanente.

## M2: conta, persistência e offline

Incremento antecipado para validar a integração: uma conta local de teste já possui equipamento persistido em JSON, transferência idempotente e aplicação de defesa no MMO. [Teste_Equipamento.md](Teste_Equipamento.md) registra a evidência. Isso não fecha SRV-01/SRV-02 de produção: PostgreSQL, autenticação, catálogo completo e offline continuam pendentes.

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| SRV-01 | Criar API, banco, migrations e autenticação | TEC-01 | Conta de teste recebe identidade distinta; endpoints rejeitam conta de outro jogador |
| SRV-02 | Persistir inventário, carteira e registro econômico | SRV-01, IDL-02 | Quantidades e saldos não negativos; IDs únicos e histórico de origem |
| SRV-03 | Executar idle no worker com checkpoints | SRV-02, IDL-04, IDL-05 | Uma expedição por conta; reiniciar preserva estado e não repete recompensas |
| SRV-04 | Recuperar até oito horas offline | SRV-03 | Online e offline com mesma semente produzem o mesmo resultado no período equivalente |
| SRV-05 | Compartilhar lotes com limites por data UTC | SRV-02, SRV-04 | Conta única, cap combinado minério/sucata, excesso permanece idle, quota não duplica na repetição |
| UI-02 | Login e resumo offline nos dois clientes | UI-01, SRV-04 | Mostra período efetivo, teto aplicado, recursos e desgaste sem confiar no relógio local |

Verificação significativa: reconectar após 0, 1, 8 e 12 horas; alterar relógio local; abrir ambos os clientes; cruzar 00:00 UTC; interromper worker depois de um commit; repetir o mesmo comando. Confirmar que itens, moedas e tempo processado são conservados.

## M3: trecho MMORPG e quest

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| MMO-01 | WASD, câmera isométrica e sprites direcionais | ART-01, NET-01 | Movimento e direção coerentes ao girar câmera; colisão validada no servidor |
| MMO-02 | Implementar Guarda e Pistoleiro jogáveis | MMO-01, IDL-01 | Duas classes concluem conteúdo solo e cooperativo; healer NPC não exige terceira classe jogável |
| MMO-03 | Cidade, dois mapas e dungeon em greybox | MMO-02 | Brumaférrea, Trilhos Baixos, Pátio de Purga e Estação conectados; saída/retorno funcionam |
| MMO-04 | Supervisor de Pressão e duas válvulas | MMO-03 | Avisos legíveis, interrupção por cenário e conclusão por dois jogadores |
| QST-01 | Indícios, diálogos, escolha e estado narrativo | MMO-03, SRV-02 | Quest retoma após sair; escolha tem consequência visual por conta/instância |
| QST-02 | Alternativa de projeto e materiais no MMORPG | QST-01, SRV-05 | Pode concluir sem idle; concessão única cobre faltantes sem duplicar recompensas |
| INT-01 | Receita e instalação única do Regulador | MMO-04, QST-02 | Craft debita tudo numa transação e bônus +4% aparece nas duas experiências |

Verificação significativa: inventário cheio, repetir conclusão/craft, obter projeto pelas duas rotas, chefe repetido e grupo com escolhas distintas. Poder final equivalente nas duas escolhas e nenhum bloqueio permanente.

## M4: economia e marketplace de teste

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| ECO-01 | Instrumentar geração, consumo, transferências e durabilidade | SRV-03, INT-01 | Eventos permitem separar produção nova de movimentação interna |
| ECO-02 | Comparar telemetria com a planilha econômica | ECO-01 | Explicar diferença de taxa teórica e real, incluindo derrotas/chefes/reparos |
| MKT-01 | Custódia, listagem e cancelamento | SRV-05 | Equipado/vinculado não entra; cancelar devolve o mesmo item |
| MKT-02 | Compra com premium fictício e taxa comum | MKT-01 | Compra atômica, saldo não negativo e registro econômico completo |
| MKT-03 | Disputas e repetição de pedidos | MKT-02 | Dois compradores concorrentes: uma venda; repetir operação não cobra duas vezes |

Nenhuma integração de pagamento real faz parte de M4. Os saldos premium de teste são injetados pelo ambiente de desenvolvimento e identificados como tais.

## M5: validar o ciclo completo

| ID | Tarefa | Depende de | Aceitação |
|---|---|---|---|
| QA-01 | Sessão guiada de até 30 minutos | INT-01, UI-02 | Jogador encontra projeto, investiga, conclui dungeon, fabrica e reconhece melhoria no idle |
| QA-02 | Sessão sem orientação | QA-01 | Registrar onde o jogador hesita e se explica a utilidade de cada experiência |
| QA-03 | Teste de estabilidade e recuperação | MKT-03, SRV-04 | Falhas/reinícios não corrompem quest, propriedade ou carteira |
| QA-04 | Rever valores e definir próximo escopo | ECO-02, QA-02, QA-03 | Atualizar balanceamento e planilha com medições; identificar o que funcionou e o que exige revisão |

Saída da POC: build Windows dos dois clientes, serviços reproduzíveis, ciclo de integração completo, dados de teste, checklist com evidências e problemas conhecidos. Arte final ampla, guildas, housing, pagamentos, multirregião e milhares de jogadores simultâneos dependem de uma etapa posterior.

## Próximo incremento: idle jogável

A base técnica do primeiro incremento já existe; suas pendências continuam no relatório M0. Iniciar IDL-01, evoluir níveis e equipamento em IDL-02 e entregar a rota de caça IDL-03. Em seguida, mineração, durabilidade, reparo e gerenciamento. Preservar os testes existentes de integração enquanto o desenvolvimento se concentra no idle. Os assets atuais são provisórios e não bloqueiam essa etapa.
