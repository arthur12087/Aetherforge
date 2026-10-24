# Contrato de equipamento e gerenciamento da expedição V19

GAMELAB-3 · 24/10/2026 · Game Designer · versão 1, entregue para revisão do Game Director e avaliação técnica subsequente. Especificação de comportamento e oráculos de aceite; este documento não implementa gameplay. “Deve” indica requisito do incremento futuro, não funcionalidade já existente.

## Base, objetivo e limites

Checkout pelo CLI Multica: branch `agent/game-designer/8d50825ce88e`, revisão-base `aca2d622ae590e0044a8e744f392fd6ed787161d`, também a revisão integrada de `origin/main` consultada. Nenhuma entrega posterior estava integrada nessa referência na leitura. Stack declarada: Godot.NET.Sdk 4.7.2, cliente net8.0, domínio netstandard2.1, SDK .NET 10.0.401; sem execução do motor nesta entrega documental.

Entradas: descrição de GAMELAB-3; roadmap `diagnostico-roadmap-aetherforge-aca2d62.md`, anexo `01a16ccf-69bc-7cae-8741-9bb9e02a178d` de GAMELAB-1, baixado pelo CLI; pareceres de Design, UI/UX, Economia, QA e Technical Lead nos fios da mesma issue. Arthur aprovou iniciar o roadmap no comentário `01a16dd9-019f-7912-887d-5d6714242d66`. Este contrato consolida as regras aceitas e resolve suas bordas; revisão do contrato e integração continuam com o Director.

O jogador deve conseguir **obter → inspecionar → comparar → equipar → observar o efeito → continuar a rota**, com tempo para decidir e sem interromper o idle a cada drop. Preservar Herói/Status, seleção compartilhada e linguagem visual atuais. Não exigir novos modelos ou alteração da aparência 3D por peça.

Evidências da base, relativas à raiz do repositório:

| Fonte | Observado na revisão-base |
|---|---|
| `src/Aetherforge.Domain/Expedition.cs`, `Advance`, `TickCombat`, `EndRest` | Marcha, combate e `Loot` (descanso de 2 s); cinco vitórias; terminais `Complete`, `Defeat`, `Retreated`. Pausa comum congela tempo; não existe gerenciamento de equipamento. |
| `src/Aetherforge.Domain/ExpeditionRewards.cs`, `ExpeditionDrop`, `CreditVictory` | Três tipos de peça, compatibilidade por `PartyRole`, bônus fixos; bolsa de 21, pendentes sem retirada; crédito por etapa protegido na tentativa. |
| `src/Aetherforge.Domain/Party.cs`, `PartyMember`, `GainExperience`; `src/Aetherforge.Domain/Progression.cs` | Atributos apenas por nível; subir nível preserva HP faltante dos vivos e não revive caídos. |
| `src/Aetherforge.Client/ExpeditionLootView.cs`, `UpdateState`, `Describe` | Inspeção funcional, bônus ainda não aplicado; atualização da grade depende da contagem. |
| `src/Aetherforge.Client/ExpeditionView.cs`, `SelectMember`, `ContinueOrPause`; `src/Aetherforge.Client/ExpeditionStatusPanel.cs` | Seleção compartilhada, pausa/reinício e explicação de atributos existentes. Abrir menu não pausa. |
| `tests/Aetherforge.Checks/ProgressionChecks.cs`; `tests/Aetherforge.Checks/ExpeditionChecks.cs` | Oráculos existentes de XP, HP por nível, recompensas, caídos, temporização e determinismo; não cobrem o equipamento futuro. |
| `docs/GDD_Aetherforge.md`, §15.4; `docs/visual/UI_Progressao_v19.md` | Slots/crescimento/loot planejados e alcance da V19; GDD completo possui diferenças de combate que não entram neste recorte. |

Autoridade de regras no domínio C#, sem Godot; cliente solicita e apresenta o resultado. Preservar idle/mapa, API, inventário, recibos e saves legados. Os IDs de sessão `local-*` não autorizam importação ou transferência ao legado. Persistência V19, autoridade/checkpoint definitivo, migração, offline, mineração, durabilidade, reparo, descarte, desmontagem, afixos, habilidades novas, repetição automática e rebalanceamento ficam fora desta especificação.

## Slots, propriedade e compatibilidade

Cada um dos três integrantes possui exatamente três slots independentes, inicialmente vazios numa sessão nova: `Weapon` (Arma para Guarda/Pistoleiro; Foco para Alquimista), `Armor` (Armadura) e `Accessory` (Acessório). São nove vagas de equipamento, separadas das 21 posições da bolsa compartilhada. Slot vazio contribui zero. Não criar peças iniciais, não converter itens legados e não preencher automaticamente os slots com drops.

| Integrante / função | Weapon | Armor | Accessory |
|---|---|---|---|
| Guarda da Caldeira / `Tank` | Lâmina da Caldeira de `Tank` | Couraça de Latão de `Tank` | Medalhão de Éter de `Tank` |
| Pistoleiro / `Dps` | Rifle dos Trilhos de `Dps` | Couraça de Latão de `Dps` | Medalhão de Éter de `Dps` |
| Alquimista do Éter / `Healer` | Foco de Éter de `Healer` | Couraça de Latão de `Healer` | Medalhão de Éter de `Healer` |

Uma peça só pode ocupar o slot de mesmo `LootSlot` de um integrante com o mesmo `PartyRole`. Nome, raridade, ícone, nível ou condição de caído não substituem essa validação. Não há requisitos novos de nível. Armaduras/acessórios de mesmo nome continuam restritos à função do drop; indicar essa função em texto. Alvo incompatível permanece selecionado: nunca redirecionar ou equipar no integrante compatível automaticamente.

Cada ID de item possui exatamente um destino de propriedade: bolsa, um slot de um integrante, ou pendentes. Referência em recibo/histórico/comparação não é outro destino. Instâncias de mesmos atributos com IDs diferentes são peças distintas. Quantidade e conjunto de IDs de `bolsa ∪ equipados ∪ pendentes` devem permanecer iguais em trocar/desequipar/cancelar; somente concessões reais acrescentam IDs. Não renumerar IDs ao mover, nem usar índice da grade como identidade.

## Fases, gerenciamento e comandos do jogador

Três conceitos separados: fase de rota; pausa comum (`Paused`); estado de gerenciamento (`livre`, `solicitado`, `ativo`). Estes nomes descrevem comportamento, não obrigam enums ou assinaturas. Gerenciamento ativo só existe em March/Loot; terminais já são estáveis. Inspecionar/comparar é permitido em todas as fases, sem mutação nem congelamento implícito.

**Gerenciar entre encontros** em marcha/descanso ativa gerenciamento imediatamente e congela a simulação no instante em que o comando é processado. Em combate cria apenas um pedido cancelável para a próxima vitória; não reserva item/alvo, não enfileira troca e não congela combate. **Cancelar pedido** remove esse pedido sem mudar a pausa ou o combate. Repetir qualquer um desses comandos não acumula pedidos nem alterna involuntariamente o estado.

| Fase / estado ao receber evento | Abrir ou trocar comparação | Gerenciar entre encontros | Confirmar equipar/desequipar | Continuação / saída |
|---|---|---|---|---|
| March em andamento | Livre; marcha continua | Ativa e congela distância/tempo restantes | Recusa até gerenciamento ativo | Rota segue automaticamente sem pedido |
| Loot em andamento (descanso) | Livre; descanso continua | Ativa; preserva restante dos 2 s | Recusa até gerenciamento ativo | Descanso só cura no seu término real |
| March/Loot em pausa comum | Livre | Ativa e conserva a pausa prévia | Só permite após ativação explícita | Concluir gerenciamento mantém pausa comum prévia |
| March/Loot com gerenciamento ativo | Livre, estado estável | Já ativo, sem efeito adicional | Permite com revalidação | Concluir gerenciamento libera só essa trava; retoma se não houver pausa comum |
| Combat ativo, sem pedido | Livre; combate continua | Cria pedido, exibe “Gerenciamento após a vitória” | Recusa “Em combate” | Pausa comum continua disponível |
| Combat ativo, com pedido | Livre | Mantém um pedido | Recusa “Em combate” | Cancelar pedido permite continuação automática normal |
| Combat em pausa comum, com ou sem pedido | Livre; tempo congelado | Cria/mantém pedido, mas não o atende | Recusa “Em combate”, inclusive pausado | Retomar combate explicitamente para alcançar a vitória; pode cancelar pedido ainda pausado |
| Vitória não final | Atualiza contexto após crédito | Se havia pedido, ativa antes de consumir qualquer descanso | Só após concluir crédito e ativação | Sem pedido, inicia descanso normalmente |
| Complete | Livre | Informa “Rota concluída”; sem pedido/gerenciamento ativo | Permite preparar próxima tentativa | Só “Reiniciar rota” inicia nova tentativa |
| Defeat | Livre, todos podem estar caídos | Informa “Equipe derrotada”; limpa pedido | Permite, sem reviver | Só reinício explícito recupera a equipe |
| Retreated | Livre | Informa “Recuo concluído”; limpa pedido | Permite, sem curar ou premiar encontro parcial | Só reinício explícito inicia outra tentativa |

Enquanto gerencia, trocar repetidamente é permitido, mas não executa ticks, cura ou recargas. Durante a trava, o controle genérico de pausa fica indisponível e não altera a pausa comum prévia: apresentar “Concluir gerenciamento” como ação própria. Se a pausa comum já existia, mostrar que a rota continuará pausada ao concluir. Terminais nunca apresentam “Retomar” como início de combate.

Cancelar/Voltar na comparação descarta apenas a intenção de troca. Não desfaz transação já aceita, não cancela pedido e não encerra gerenciamento. Fechar Herói/Status também não encerra gerenciamento: manter indicação visível no HUD/Expedição e ação de concluir/reabrir para não deixar uma parada sem saída. Escape volta em camadas, preservando a navegação existente. Cancelar pedido e concluir gerenciamento são ações distintas, nomeadas.

Recuar durante marcha, combate, pausa ou gerenciamento mantém o comportamento de recuo V19: terminal imediato na ordem de processamento, sem recompensa parcial. Limpar pedido/gerenciamento e invalidar confirmação antiga, sem alterar equipamento aceito. Recuar após uma vitória já creditada não remove seus ganhos. Em terminal, novo recuo é sem efeito.

Reiniciar rota deve preservar XP, carteira, bolsa, pendentes, IDs, RNG e agora também slots/equipamentos. Restaura HP até os máximos equipados pela regra de reinício existente; limpa pedido, gerenciamento e comparações antigas e reinicia temporizadores/contagem da tentativa como hoje. Isso é efeito de **reiniciar**, não da troca. Reset explícito de sessão continua a zerar progressão/ganhos e recriar slots vazios. Trocar, fechar menu ou concluir gerenciamento jamais chama essas operações.

## Ordem de operações e tempo de simulação

Comandos e passos de simulação devem ser serializados. A confirmação consulta a fase atual no domínio, não a fase da abertura da UI. Se `Advance` já terminou antes de um clique ser processado, o clique observa o estado resultante; um pedido não retroage a uma vitória anterior. Se o pedido chegou antes de `Advance`, o avanço deve respeitar a primeira fronteira adequada mesmo com delta grande. Nenhum comando de UI pode ser intercalado no meio de um crédito/transação.

Na última morte inimiga de um encontro, executar nesta ordem:

1. Resolver o golpe/eventos de morte conforme a V19. Manter desempate Guarda → Pistoleiro → Alquimista → inimigos; inimigo morto não realiza golpe pendente. Não aplicar cura/ataques extras após a condição de vitória já resolvida.
2. Reconhecer a vitória uma vez; incrementar a contagem uma vez. Creditar uma vez XP para todos, inclusive caídos; níveis e seu efeito próprio no HP; carteira; sorteio/drop e seu destino bolsa/pendência; recibo e eventos de recompensa/nível/drop. Usar a proteção por tentativa/etapa existente, sem chamar crédito ao gerenciar/retomar/trocar. Recibo/RNG/contador de itens não avançam num retry de comando.
3. Se é o chefe da quinta vitória, entrar em `Complete` e limpar pedido/gerenciamento. Não iniciar descanso, cura de fim de descanso ou nova marcha. Permitir equipamento no terminal após o crédito.
4. Nas demais vitórias, estabelecer `Loot` com 2 s ainda não consumidos. Se havia pedido, consumi-lo e ativar gerenciamento nesse mesmo instante, antes de qualquer tempo de descanso, tratamento ou próximo encontro. Manter os ganhos já creditados.
5. Sem pedido, seguir o fluxo automático. Com gerenciamento ativo, parar o laço de avanço imediatamente, mesmo que reste delta. Após concluir, o descanso começa/continua a partir do restante conservado; ao terminar, tratar vivos com healer vivo, incrementar etapa e marchar uma vez.

Derrota e recuo limpam pedido e gerenciamento e preservam apenas ganhos já creditados. Não fabricar uma vitória para atender um pedido. Se um comando de recuo foi processado antes do passo que mataria o último inimigo, não há crédito; se veio depois do crédito, esse ganho permanece. Mantém-se a ordem serial, sem resolução simultânea nova.

O relógio autoritativo é `Elapsed`. Gerenciamento congela esse relógio, `Distance`, tempo de fase e todos os tempos restantes: fim da marcha/descanso, próximos ataques, recarga da infusão, duração e recarga do escudo. Não reiniciar deadlines ou converter espera humana em tempo de jogo. Ex.: marchar com 3 s restantes, gerenciar por 60 s reais e concluir deixa os mesmos 3 s de marcha; descanso com 0,75 s restante continua com 0,75 s, não com 2 s nem já tratado.

Uma chamada de `Advance(delta)` deve verificar a trava em cada fronteira interna, além da entrada. O tempo não consumido ao alcançar gerenciamento/terminal é descartado, sem fila de catch-up. Após retomar, somente novos deltas avançam; o tempo de parede durante a parada também é descartado. Isso acrescenta uma parada ao scheduler, sem reduzir a capacidade atual de cruzar múltiplas fases quando não existe pedido. Sem intervenções e com mesma semente, `Advance(1000)` e avanços particionados continuam equivalentes. Com pedido, a equivalência é exigida até a mesma fronteira de parada, sem computar o restante descartado.

Delta negativo, NaN ou infinito continua inválido mesmo pausado, gerenciando ou em terminal. Delta zero não progride nem atende pedido por si só. O pedido imediato em March/Loot é atendido pelo comando, sem depender de um tick. Ao liberar gerenciamento em marcha/descanso, nenhuma ação já resolvida é repetida; escudo só ativa em combate ativo e sem travas, como hoje.

## Atributos, vida e comparação

Usar um cálculo compartilhado para prévia, Status, ataque, defesa e cura. Para nível `L` e atributo base `B`, o valor de nível é `floor(B × (100 + p × (L−1)) / 100)`, com `p=10` para vida, `6` para ataque, `5` para defesa e `8` para infusão. Somar equipamento **depois** do crescimento/arredondamento. Não fazer crescimento em cima do valor já arredondado do nível anterior e não escalar bônus por nível.

| Slot | Comum | Incomum | Raro | Aplicação |
|---|---:|---:|---:|---|
| Weapon | +2 | +3 | +4 | Ataque; no Foco do Alquimista, também soma o mesmo bônus à potência da infusão |
| Armor | +2 | +3 | +4 | Defesa |
| Accessory | +10 | +15 | +22 | Vida máxima |

Não usar ataque total como potência da infusão: `floor(42 × (100 + 8 × (L−1)) / 100) + bônus do foco`. Cura real continua limitada à vida faltante e às condições de uso atuais. Os três slots somam apenas os bônus que definem; não criar velocidade, energia ou outros modificadores.

Na troca/desequipamento, calcular o conjunto final substituindo o slot, sem somar o novo sobre o antigo. Política de HP da transação: `HP_depois = min(HP_antes, MaxHP_depois)`, para estado válido não negativo. Aumento de máximo não cura nem preserva percentual/faltante; redução pode cortar HP. Caído (`HP=0`) permanece caído, inclusive em Complete/Defeat/Retreated. Mostrar HP atual e máximo antes/depois, além do delta de máximo. Alternar acessórios não recupera o corte; equipar não emite evento de cura. Os demais integrantes mantêm HP e equipamentos.

Essa política é exclusiva da troca e não substitui `GainExperience`: crescimento de nível continua preservando HP faltante dos vivos. Numa vitória com nível ganho e pedido, primeiro ocorre o crédito/nível V19; depois a comparação usa esse novo estado. Não aplicar “cura de nível” ao recalcular equipamento, nem dar cura extra ao entrar no gerenciamento. Tratamento de descanso usa o novo máximo quando seu término realmente ocorrer; é um evento de descanso separado da troca.

Apresentar alvo (nome/função/condição), slot e raridade; **Equipada / Nova / Total atual / Total após troca / Diferença** com as mesmas unidades. Slot vazio aparece “Vazio”, bônus zero. Comparar todos os atributos afetados; indicar os demais como inalterados, sem sugerir ganho. Delta positivo, zero e negativo têm sinal/texto, sem depender só de cor. DPS básico teórico = ataque / intervalo atual, com rótulo de estimativa antes da defesa; intervalo não muda. Para desequipar, “Após: Vazio” e retirada integral do bônus, incluindo HP limitado.

Mostrar efeito imediato nos atributos e uso nas próximas ações de simulação após continuar. Isso inclui tratamento ao fim do descanso e o próximo encontro; não promete recalcular um golpe passado. Para caído: “Atributos atualizados; continua caído até reiniciar a rota”. Enquanto não houver persistência, informar ganhos/equipamento da sessão, perdidos ao fechar; nunca “salvo” sem confirmação real.

### Mudança de alvo, dados antigos e cancelamento

Inspecionar uma peça não muda o integrante selecionado. Uma intenção de confirmar vincula item por ID, integrante, slot, peça antiga esperada e revisão do estado comparado. Selecionar outro integrante no Herói, Status ou palco invalida a intenção anterior e recalcula a comparação para o novo alvo com a mesma peça inspecionada. Manter essa peça visível mesmo incompatível; mostrar motivo e bloquear ação. Nunca confirmar uma captura do alvo anterior nem reencaminhar para ele silenciosamente.

Se mudar nível, HP relevante, propriedade, conteúdo do slot, tentativa/sessão ou autorização de fase, atualizar a prévia e invalidar a confirmação antiga. Callback atrasado deve recusar “Comparação desatualizada; confira os valores novamente”; exige nova ação explícita sobre a prévia atual, sem retry automático. Ir de comparação em combate para gerenciamento também exige prévia atual. A UI pode continuar a inspeção em combate; confirmar sem permissão recusa, não cria pedido de troca.

Cancelar, mudar peça, voltar ou fechar janela abandona confirmação sem mover itens, reservar vaga, consumir recursos ou alterar RNG. Após sucesso, manter o alvo e exibir o slot final; a peça agora equipada é inspecionável como equipada, sem nova ação de equipar no mesmo destino. Em desequipamento, selecionar a peça devolvida pelo ID, mesmo que a grade tenha mudado de índice. Trocar de igual valor é permitido entre IDs distintos, mas deve indicar “Sem mudança nos atributos”.

## Transação de equipamento e bolsa cheia

Precondições em toda confirmação: fase segura (gerenciamento ativo em March/Loot, ou terminal); sessão/tentativa e revisão esperadas; integrante existente; item de origem pertencente à bolsa (equipar) ou ao slot daquele integrante (desequipar); compatibilidade e slot; destino antigo esperado; capacidade final. Transferência direta de slot de outro integrante ou de pendência não é uma operação deste recorte. Primeiro é preciso mover legalmente à bolsa. Sem custos, desgaste ou consumo de recursos.

Ordem indivisível do ponto de vista do jogador/observadores:

1. Validar tudo contra o estado atual e preparar o resultado, incluindo atributos e HP. Se falhar, devolver motivo e preservar todo o estado autoritativo como estava no início desse comando; a passagem normal de tempo fora do gerenciamento é independente dessa garantia.
2. Equipar em slot vazio: retirar o candidato da bolsa e colocá-lo no slot. Substituir: retirar o candidato da bolsa, instalar no slot e devolver a antiga **à posição liberada pelo candidato**, mesmo com bolsa cheia. Não exigir vaga adicional, não mandar antiga para pendentes, não descartar peça. Desequipar: retirar do slot e inserir na bolsa apenas se já existe uma vaga; com 21/21 recusar integralmente.
3. Concluir a transação de propriedade/atributos/HP. Só então preencher vagas **realmente livres** da bolsa com pendentes, em FIFO pela ordem de concessão (mais antigo primeiro), até 21. Não ocupar a vaga temporária de uma substituição antes de devolver a antiga. Peça devolvida tem prioridade sobre pendentes. Remover da fila apenas IDs efetivamente inseridos. Equipar pendente diretamente não é permitido; a recuperação aqui é automática após uma transação aceita que deixa espaço. Abrir menu, comparar ou uma operação recusada não drena pendentes.
4. Publicar um resultado coerente com bolsa, slots, pendentes, HP, comparação e Status atualizados na mesma ação, incluindo substituição 21→21. Observadores nunca veem o candidato/antiga duplicado ou uma vaga intermediária. Invalidar a grade por mudança de conteúdo/revisão, além da contagem; mostrar quantos pendentes foram recuperados.

Confirmações repetidas da mesma operação não repetem movimento/bônus/feedback; a proteção concreta de comandos, revisões e recibos é responsabilidade do contrato técnico seguinte. Tentativa de equipar ID já nesse slot é “Sem alteração”; desequipar slot vazio também. Não emitir sucesso/áudio de troca em no-op, rejeição, cancelamento, redraw ou retry; a mesma confirmação aceita gera um único resultado. Não alternar implicitamente equipar/desequipar em duplo clique. Resultado deve distinguir troca aceita sem delta (IDs diferentes) de no-op.

Motivos mínimos distinguíveis em texto: em combate; ativar gerenciamento; função incompatível; slot incompatível; item não está na bolsa/slot esperado; bolsa cheia ao desequipar; comparação/alvo desatualizado; sem alteração. Nenhuma recusa consome ou modifica a peça.

## Oráculos de aceite para implementação e QA

Os casos abaixo são especificações calculadas, não testes executados do equipamento. Salvo indicação, integrante Nv1, sem outro equipamento, fase com gerenciamento ativo; itens de IDs distintos e compatíveis. Instrumentar fixtures técnicas sem editar o save manual. “Snapshot igual” compreende propriedade/IDs, atributos/HP, XP, carteira, RNG, créditos, fase e temporizadores no instante do comando; exclui seleção/explicação visual.

| ID / cenário e ação | Resultado exigido |
|---|---|
| E01 — Slot vazio: equipar armadura comum do Guarda | Defesa 10→12, delta +2; bolsa −1, slot ocupa o mesmo ID; golpe bruto de rato 17 causa 7→5 antes do escudo. Desequipar com vaga restaura defesa 10, sem bônus residual. |
| E02 — Igual: Pistoleiro com arma comum A troca pela comum B | Ataque 20→20, delta 0 explícito; B equipa, A volta à posição de B; quantidade/IDs conservados. Novo comando com B já equipado é no-op sem segunda confirmação de sucesso. |
| E03 — Melhor: Pistoleiro comum→incomum | Ataque 20→21, delta **+1**, não +3 acumulado. Sem arma→comum seria 18→20; DPS teórico 14,4→16 com intervalo de 1,25 s. |
| E04 — Pior: Pistoleiro raro→comum | Ataque 22→20, delta −2; confirmação explícita é permitida, sem equipar automaticamente a melhor peça ou proibir redução. |
| E05 — Incompatível | Foco de Healer com Guarda selecionado mostra motivo, mantém Guarda e snapshot. Armadura/acessório de outra função ou slot de destino incorreto também recusam. |
| E06 — Foco do Alquimista | Comum: ataque 6→8, infusão potencial 42→44; desequipar retira ambos. Nv2: base de infusão floor(45,36)=45, com foco comum 47; não 44 escalado por nível. Cura real não excede vida faltante. |
| E07 — Vida ao aumentar | Guarda 100/180 equipa acessório comum: 100/190, sem cura; retirar volta a 100/180. Guarda cheio 180/180→180/190, não 190/190. |
| E08 — Vida ao reduzir e alternar | Guarda com acessório raro e 202/202 troca pelo comum: 190/190; reequipar raro dá 190/202. Repetir nunca cura. Desequipar de 190/190 dá 180/180. Prévia mostra cada corte. |
| E09 — Caído/derrota | Guarda 0/180 equipa acessório e fica 0/190. Não pode atacar; XP pode ser creditada a caído como hoje. Em Defeat, equipar não muda terminal nem revive. Reinício explícito recupera ao máximo final equipado. |
| E10 — Bolsa cheia, substituição | 21 itens na bolsa, slot ocupado, ≥1 pendente: candidato sai, antiga volta à vaga liberada, bolsa 21, pendentes intactos. Nenhum ID some/duplica; UI atualiza apesar da contagem igual. |
| E11 — Bolsa cheia, slot vazio | 21 na bolsa, ≥2 pendentes: equipa candidato; primeiro pendente ocupa a vaga realmente livre; bolsa volta a 21, fila −1, segundo fica pendente. Equipado não ocupa bolsa. |
| E12 — Desequipar sem/com espaço | 21/21: recusa com snapshot igual, mesmo havendo pendentes. 20/21 e 1 pendente: antiga entra, bolsa 21, pendente permanece; 19/21 e ≥2 pendentes: antiga entra primeiro, somente o pendente mais antigo preenche a vaga restante. |
| E13 — Cancelar e mudar alvo | Comparar arma de Dps para Pistoleiro; selecionar Guarda via Status invalida confirmação antiga, mantém item inspecionado e mostra incompatível. Voltar ao Pistoleiro exige nova ação. Cancelar/fechar não equipa, não muda pedido nem termina gerenciamento. |
| E14 — Comparação antiga/repetição | Alterar o slot/tentativa após gerar a prévia: callback antigo recusa e não toca no novo estado. Executar novamente o comando já aceito não acumula bônus, não devolve outra peça nem repete cue. |
| E15 — Nível antes da troca | Guarda 100/180, Nv1 com 48 XP, vence entrada (+12 XP) com pedido. Crédito leva Nv2 e 118/198 pela regra V19; só então acessório comum leva a 118/208, sem cura extra. Caído continua 0 mesmo ganhando nível. |

| ID / fase, evento e tempo | Resultado exigido |
|---|---|
| F01 — Menu durante marcha | Abrir Herói/Status e comparar não pausa. Confirmar sem gerenciamento recusa. Ativar gerenciamento congela a marcha no ponto atual; confirmar passa a ser possível. |
| F02 — Combat pausado | Pausar combate não libera equipamento. Pedido é aceito como solicitado; `Advance` não progride até retomar combate. Cancelar pedido ainda pausado não retoma nem equipa. |
| F03 — Pedido + delta grande | Em Combat, registrar pedido e chamar `Advance(1000)`: parar na primeira vitória não final, com um único crédito novo, em Loot com 2 s restantes e gerenciamento ativo. Não tratar, marchar, gerar segundo encontro/crédito ou avançar recargas no restante descartado. |
| F04 — Cancelar pedido antes da vitória | Sem pedido ao resolver vitória, a rota segue como V19; não cria gerenciamento. Pedir após a chamada já concluída observa a fase atual, sem reabrir o crédito anterior. |
| F05 — Preservar descanso | Interromper descanso com 0,75 s restante; esperar tempo real; trocar; concluir. Avançar 0,74 s não trata; os 0,01 s seguintes encerram descanso uma vez, com cura só de vivos e healer vivo, usando máximos atuais. |
| F06 — Deltas sem pedido e transições | Com mesma semente e equipamento e sem intervenção, grande delta e pequenos deltas dão mesma rota/ledger. Gerenciar antes do fim da marcha impede começar combate. Se o combate já começou antes do clique, apenas solicita; confirmação antiga recusa. |
| F07 — Relógios, escudo e infusão | Guardar tempo restante/recargas antes do gerenciamento; muitos deltas válidos durante a trava não os alteram. Trocar/concluir não zera recargas nem reativa escudo. Delta inválido continua rejeitado mesmo congelado. |
| F08 — Chefe/terminais | Pedido antes da vitória final: crédito único, Complete, pedido limpo, sem descanso final. Derrota/recuo com pedido: limpar sem crédito parcial. Em todos, equipamento permitido sem retomar, curar, reviver ou recreditar. |
| F09 — Reiniciar/Reset | Reiniciar terminal conserva equipamento/IDs e ganhos, recupera HP e reinicia tentativa/tempos como V19; tentativa seguinte pode creditar cada etapa novamente. Reset de sessão limpa slots/ganhos. Comparação/pedido antigo não atravessa nenhuma dessas ações. |
| F10 — Pausa prévia e saída da UI | Em March/Loot pausado, ativar e concluir gerenciamento mantém pausa. Fechar janela enquanto gerencia mantém trava e indicador/saída acessível; Voltar na comparação não retoma. |

## Invariantes da V19 e entrega seguinte

Este recorte altera somente equipamento, cálculo que passa a incorporá-lo, comparação e a parada explícita para gerenciamento. Preservar as regras abaixo; diferença no tempo para vencer devido a equipamento não é autorização para rebalancear:

| Sistema | Regra a preservar |
|---|---|
| Rota | Cinco encontros/vitórias; distâncias 16/22/18/24/28 e marcha 2,4 unidades/s. Sem repetição automática, recuperação de caído no descanso ou rota de 21 encontros importada do M1. |
| XP/carteira | XP 12/16/20/24/50 integral por integrante inclusive caídos; moedas 8/10/12/14/35; sucatas 1/1/2/2/5; um componente do chefe. Total de rota 122 XP por integrante e 79/11/1 recursos. Nível máximo 20, custo 60+30×(L−1), sem XP acumulada após teto. |
| Cura/HP por nível | Infusão base 42, crescimento 8%, limiar ≤72%, recarga 4 s; healer vivo e alvo vivo de menor proporção de vida; limite do máximo. Descanso de 2 s trata 20% do máximo dos vivos se healer vivo; não revive. Ganho de nível mantém HP faltante dos vivos. |
| Escudo | Ação manual em combate ativo com Guarda vivo, 4 s de duração e 12 s de recarga, multiplicador de dano 0,35 (65% de mitigação), aplicado após defesa. Não importar 40%/custos do GDD. |
| Ataques | Intervalos 1,6/1,25/2,4 s e aberturas/desempates/alvos atuais; dano mínimo 1 antes do escudo; sem crítico/habilidade nova. Equipamento muda apenas os atributos previstos. |
| Drops | Chance comum 10%, chefe 100%; raridades 75/20/5; mesma sequência de RNG e geração de IDs para mesmas vitórias. Movimentar equipamento não sorteia, não gera recibo de vitória e não avança RNG. |
| Energia e economia | Energia/custos, desgaste, reparos e consumos planejados continuam ausentes. Não adicionar débito, sucata por peça, barreira de nível ou recompensa por gerenciamento. |

Decisões fechadas nesta versão: slots fora da bolsa; compatibilidade estrita por função/slot; gerenciamento explícito com pedido apenas de parada; HP absoluto limitado; devolução prioritária e pendentes FIFO; mudança de alvo invalida intenção; terminais permitem preparar sem recuperar; restante de delta descartado ao parar. São requisitos da implementação futura, sujeitos à revisão do Director, não evidência de funcionamento atual.

Riscos e encaminhamento para as etapas já previstas:

- Technical Lead deve definir representação/autoridade dos slots, revisão e idempotência de comandos, atomicidade/rollback e compatibilidade do futuro save/checkpoint, preservando legado. O domínio deve conseguir aplicar todos os casos E/F sem depender de UI; não há autorização aqui para criar uma segunda economia ou migrar saves.
- Gameplay deve adaptar a interrupção dentro de `Advance`, não apenas no retorno ao cliente. Risco principal: delta grande atravessar a janela e creditar/curar de novo. Recibos de vitória e resultados de equipamento têm semânticas distintas.
- UI/UX deve detalhar disposição/foco com os componentes atuais e garantir seleção compartilhada, rejeição explicada, atualização por conteúdo e saída visível do gerenciamento. Conferir mouse/teclado, rolagem e 100/125/150% nos viewports já definidos pela V19; não certificar DPI físico sem ensaio.
- QA deve implementar/verificar a matriz no domínio e pelo fluxo real da UI e manter regressão do legado. Estes oráculos não substituem execução. Não houve novos testes, build, alteração de runtime/assets/save ou certificação gráfica nesta entrega.
- Economia/Director devem avaliar depois a utilidade de repetidas/inferiores e a cadência real de melhorias. FIFO resolve vagas abertas por equipar; não resolve bolsa totalmente equipada e cheia, nem oferece desmontagem/retirada manual. Com bônus fixos, raridade maior domina a menor; isso não demonstra diversidade de builds.
- Hipótese de valor: comparação compreensível e parada sob controle do jogador permitem reconhecer a utilidade do loot e continuar uma tentativa. No playtest, pedir que a pessoa identifique alvo, preveja delta/HP, faça/cancele troca e explique o efeito no próximo combate. Registrar compreensão, erros e fricção; ganho de retenção e equilíbrio do equipamento permanecem não demonstrados.

Validação documental: conferir referências no checkout, fórmulas nos arquivos de domínio e cenários contra as decisões do roadmap; `git diff --check`. Conforme `AGENTS.md`, alteração apenas documental não exige recompilação. O aceite desta issue é revisão da especificação; equipamento funcional e seus passes pertencem às etapas posteriores.
