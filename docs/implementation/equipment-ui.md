# Equipamento em Herói e Status — especificação de UI/UX

GAMELAB-6 · UI/UX Designer · 24/10/2026 · versão 1 para revisão do Game Director e implementação posterior em GAMELAB-8. Este documento especifica comportamento, controles e aceite; não adiciona interface, assets, domínio ou save ao runtime. Todos os ensaios de interação e layout abaixo são requisitos futuros, salvo as verificações documentais declaradas ao final.

## 1. Entrada e rastreabilidade

Fontes normativas: [contrato comportamental](equipment-contract.md), E01–E15/F01–F10, e [arquitetura aceita](equipment-architecture.md), incluindo suas portas, códigos e ordem de validação. Em conflito, esses contratos governam as regras; esta especificação governa apresentação e navegação. Não cria outra API nem amplia a autoridade local V19 para inventário remoto.

Base obtida por `multica repo checkout https://github.com/arthur12087/Aetherforge --ref agent/technical-lead/dcbd6bff353d`: head `947298b199cd71d0ed7f543cfbbbc7cfe9578819`, contendo o contrato incorporado `0686d28fb83b59860fbc2ec91df113fc38dedfb4`. Na consulta deste trabalho, `origin/main` era `aca2d622ae590e0044a8e744f392fd6ed787161d` e ancestral dessa head. A branch técnica é uma entrada documental aceita, ainda não presumida como main. Preservar avanços posteriores na integração.

Entradas de coordenação lidas: descrição atual de GAMELAB-6; entrega GAMELAB-5 no comentário `01a16dfc-a029-7e9e-9d68-11229fe54a0f`; aceite do Director `01a16dff-d0c1-7429-8ffb-68acc0df08a7`; parecer UI/UX `01a16cbd-354d-71dc-bee1-604c42d1692b` e demais pareceres do fio de direção visual; relatório `diagnostico-roadmap-aetherforge-aca2d62.md`, anexo autenticado `01a16ccf-69bc-7cae-8741-9bb9e02a178d`. Nenhum gesto ou timing desta proposta é atribuído a Taskbar Hero: a [adaptação aprovada](../referencias/taskbar-hero/Adaptacao_Aetherforge.md) orienta a hierarquia, com interações próprias do Aetherforge.

| Evidência no checkout | Implicação para esta entrega |
|---|---|
| `src/Aetherforge.Client/Aetherforge.Client.csproj`; `global.json`; `src/Aetherforge.Client/project.godot` | Godot.NET.Sdk 4.7.2, cliente net8.0, SDK fixado 10.0.401; a configuração declara Godot 4.7/C#. Não houve execução do motor nesta task. |
| `src/Aetherforge.Client/ExpeditionView.cs`, `BuildMenu`, `SetMenuVisible`, `HandleKey`, `WireFocus` | Três instrumentos nativos e modo de revisão embutido; cabeçalho/rodapé fixos, corpo rolável, Escape e restauração de foco. Há alterações necessárias nesses pontos na integração futura. |
| `src/Aetherforge.Client/ExpeditionLootView.cs`, `Inspect`, `UpdateState`, `Describe` | Bolsa 7×3, seleção por ID, descrição rolada para a área visível; atualização atual por contagem precisa passar a conteúdo/revisão. A inspeção atual não equipa. |
| `src/Aetherforge.Client/ExpeditionStatusPanel.cs`, `UpdateState`, `TryGoBack`, `UpdateExplanation` | Resumo → detalhes → atributo; seleção compartilhada; explicações atuais de equipamento ausente devem mudar somente quando a função existir. |
| `src/Aetherforge.Client/BrassUi.cs`; `BrassTypography.cs`; `BrassDossier.cs` | Dossiê de turno, Source Sans 3 400/600, Limelight, paleta quente e foco delineado existentes. Sem reformular fonte, moldura ou produzir assets. |
| [Status V18](../visual/UI_Status_v18.md); [progressão V19](../visual/UI_Progressao_v19.md) | Evidência histórica de mouse/teclado e escalas internas 100/125/150%; não certifica este fluxo novo, DPI físico, gamepad ou toque. |

## 2. Hierarquia e disposição

**Herói responde “o que está equipado e guardado?”; Status responde “qual é o efeito neste integrante?”.** Ambos exibem o mesmo loadout e abrem a mesma apresentação de comparação. Um controlador junto a ExpeditionView mantém seleção/intenção; cada janela conserva sua navegação e retorno de foco. Não criar uma quarta janela nativa nem uma segunda seleção de alvo.

Manter as áreas `FixedHeader`, `ScrollableBody` e `FixedFooter` de cada instrumento. A comparação é uma página interna que substitui o corpo do instrumento de origem; não se acumula abaixo de toda a bolsa/retrato. As medidas a seguir usam unidades lógicas antes de `BrassUi.Scale`, com Containers/âncoras, sem coordenadas absolutas novas.

| Área | Herói — visão principal | Status — resumo e detalhe | Comparação / resultado no instrumento de origem |
|---|---|---|---|
| Cabeçalho fixo | Título/fechar existentes; linha compacta de alvo compartilhado | Mesmo alvo do resumo, anterior/próximo e condição | “Comparar equipamento” ou “Resultado”; alvo, função, nível, condição e slot sempre visíveis |
| Corpo rolável | Retrato existente; **Equipamento** com três linhas; recursos, capacidade/pendentes, bolsa 7×3 e inspeção | Resumo existente; três linhas de equipamento após os valores principais; detalhes/composição preservados | Equipada/Nova; comparação de totais/deltas e HP; composição; capacidade e destino das peças; explicação de bloqueio ou resultado |
| Rodapé fixo | Seletores de integrante existentes; acesso ao gerenciamento | Atalho Expedição/escudo existentes; acesso ao gerenciamento | Voltar/Cancelar; confirmar ou ação de gerenciamento conforme estado; mensagem curta de bloqueio/resultado. Nunca esconder confirmar no corpo rolável |

Cabeçalho de alvo: `Alvo: {nome} · {função} · Nv. {nível}` + `{condição}`; comparação acrescenta `Slot: {Arma/Foco/Armadura/Acessório}`. Na área curta, usar nome completo com quebra e seletor compacto anterior/próximo; retrato grande/legenda secundária cedem espaço. Não reduzir os números ou retirar nome/função para manter a arte. Vida fica no corpo e é repetida no aviso fixo quando houver corte de HP ou alvo caído.

Três slots por integrante, inicialmente **Vazio**, em ordem Arma/Foco → Armadura → Acessório. Cada linha é um botão com rótulo do slot, nome da peça ou Vazio, raridade por extenso e bônus. Exemplo: `Armadura · Couraça de Latão · Comum · +2 defesa`. No Alquimista, Weapon é rotulado **Foco**; não adicionar um quarto slot. Slots ocupados mostram **Equipada**; gravuras/ícones existentes são complementares ao texto. A bolsa mostra **Guardada na bolsa**, função e raridade completas na inspeção; C/I/R na grade não substituem a descrição.

Layout da comparação, com números ilustrativos de E03:

```text
[fixo] HERÓI                                     [×]
       Alvo: Pistoleiro · DPS · Nv. 1 · Vivo
       Slot: Arma
----------------------------------------------------
[rola] Equipada: Rifle dos Trilhos · Comum · +2 ataque
       Nova:     Rifle dos Trilhos · Incomum · +3 ataque

       Ataque
         Total atual: 20 → Total após troca: 21
         Diferença: +1 · Ganho
       DPS básico teórico: 16 → 16,8 / s · +0,8 / s
       Antes da defesa; intervalo 1,25 s inalterado.
       Defesa e vida máxima: sem alteração.

       Composição do ataque após troca:
         Base 18 + ganho de nível 0 + equipamento 3 = 21
       Bolsa: antiga volta à posição liberada pela nova.
----------------------------------------------------
[fixo] Equipamento e ganhos desta sessão: perdidos ao fechar.
       [Cancelar / Voltar à bolsa] [Equipar no Pistoleiro]
       [Concluir gerenciamento]   (se ativo)
```

Em largura suficiente, totais podem formar colunas Atual/Após/Diferença. Em área estreita ou texto ampliado, empilhar como no desenho; repetir rótulos por atributo e manter unidades. Não exigir rolagem horizontal. A bolsa conserva 21 posições e 7×3 quando couber; se sua largura mínima real não couber após escala, refluí-la para menos colunas, mantendo ordem por ID, quantidade/capacidade e vizinhos de foco. Não esticar a janela além da área útil para preservar sete colunas.

## 3. Seleção, inspeção e caminhos de entrada

Mapeamento único de identidade: `tank` → Guarda da Caldeira / índice legado 0; `dps` → Pistoleiro / 1; `healer` → Alquimista do Éter / 2. Nome, retrato e índice de bolsa não são IDs de comando. Selecionar via Herói, Status ou palco chama o caminho compartilhado de seleção; uma peça inspecionada continua visível mesmo incompatível. Não selecionar automaticamente o dono compatível.

Preservar neste recorte as ordens visuais atuais de seletores (Herói cura/DPS/tank; Status tank/DPS/cura); mudar ordem seria trabalho adicional. A diferença deve ser compensada por nome/função, marcador textual **Selecionado**, borda e leitura do mesmo MemberId. Nunca mapear pela posição visual. A troca anterior/próximo segue os índices compartilhados, sem modificar o equipamento.

| Entrada e ação explícita | Próxima apresentação | Mutação permitida |
|---|---|---|
| Clique simples / Enter / Espaço numa peça da bolsa | Inspeção com ID, função, raridade, bônus, localização e botão **Comparar**; manter alvo | Nenhuma; hover também não muda alvo nem equipa |
| **Comparar** na inspeção | Página de comparação Equip para o slot intrínseco da peça e o alvo atual; gerar consulta nova | Apenas consulta `Compare` |
| Ativar slot vazio em Herói/Status | Detalhe `Vazio · bônus 0`, botão **Escolher na bolsa** | Nenhuma |
| **Escolher na bolsa** / **Comparar outra peça** | Abrir Herói na bolsa, mantendo alvo e registrando slot/origem para retorno; focar primeira peça compatível, ou corpo se não houver | Sem selecionar/equipar peça implicitamente; demais peças continuam inspecionáveis |
| Ativar slot ocupado | Detalhe da peça equipada; **Comparar outra peça** e **Desequipar…** | Nenhuma; inspecionar o ID já nesse destino pode apresentar NoChange |
| **Desequipar…** | Página de comparação Unequip: Equipada = peça atual, Após = Vazio, perda completa de bônus | Só consulta; remoção exige **Desequipar de {nome}** |
| Trocar integrante durante qualquer comparação | Invalidar intenção anterior, conservar peça inspecionada, consultar novamente para o novo alvo | Nunca enviar comando ao alvo anterior; incompatibilidade permanece visível |
| Cancelar, Voltar ou × | Abandonar intenção e retornar/fechar em camadas | Não mover itens, pausar, cancelar pedido, concluir gerenciamento ou reservar vaga |

Ao escolher outra peça a partir de Status, Herói guarda uma origem de navegação com membro/slot e janela de retorno. Voltar da comparação retorna primeiro à bolsa; Voltar da bolsa aberta por esse atalho retorna ao slot de Status, se ainda existir. Abrir Herói diretamente não cria esse retorno cruzado. Fechar qualquer janela invalida sua intenção; fechar/reabrir não restaura uma confirmação válida antiga.

## 4. Comparação e estados de decisão

Dados numéricos vêm de `EquipmentPreview`, calculado pela mesma função usada pelo domínio/Status/combate. Não calcular bônus no callback do botão. Para cada atributo afetado, exibir **Total atual / Total após troca / Diferença** e a decomposição **Base + ganho de nível + equipamento**. “Ganho de nível” é o acréscimo sobre a base, após arredondamento, evitando somar duas vezes a base escalada. A composição deve estar disponível por seleção, sem depender de hover.

| Estado | Conteúdo e confirmação |
|---|---|
| Nenhuma peça selecionada | “Selecione um item para inspecionar”; sem confirmar. Bolsa vazia: “Nenhum equipamento na bolsa”. Slots permanecem inspecionáveis |
| Slot vazio / candidata compatível | Equipada: **Vazio · bônus 0**; Nova com bônus integral; totais do integrante e ganho explícito. Confirmar somente se `CanConfirm` |
| Ganho | Sinal `+` e texto **Ganho** por atributo; confirmar Equipar. Não equipar automaticamente por raridade |
| Igual, IDs distintos | `Diferença: 0 · Sem mudança nos atributos`; confirmar **Trocar peça (mesmos atributos)** continua permitido; resultado Applied, IDs de fato trocados |
| Perda | Sinal `−` e texto **Perda**; confirmar explicitamente **Equipar mesmo com perda** ou **Desequipar de {nome}**. Não acrescentar uma segunda modal obrigatória; a comparação já é a confirmação |
| Mesma peça já no mesmo slot / remoção de vazio | “Sem alteração”; ação de confirmar desabilitada. Se um callback válido ainda chegar, respeitar NoChange sem cue de sucesso |
| Função/slot incompatível | “Compatível com {função da peça}. Alvo atual: {nome/função}” ou “Esta peça ocupa {slot da peça}, não {slot solicitado}”; bloquear confirmação, manter alvo. Não fabricar totais de um loadout ilegal: mostrar totais atuais e “Após: indisponível” |
| Item pendente / em slot de outro integrante | Localização e motivo: “A peça precisa estar na bolsa para equipar”; sem transferência direta, retirada manual ou descarte. Pendentes só entram por FIFO após operação aplicada com vaga |
| Bloqueio por fase / bolsa cheia / intenção antiga | Mostrar motivo específico e saída conforme §§5/7. Valor comparável pode continuar visível como inspeção; nunca insinuar autorização |
| Enviando | Desabilitar confirmação da intenção em execução; impedir nova ação pelo mesmo gesto/duplo clique. Cancelar posterior ao envio não é rollback: o resultado aplicado deve ser apresentado |
| Aplicado / rejeitado / NoChange / Replay | Apresentação e foco de resultado conforme §7; nenhuma recusa aparece como troca aceita |

Weapon do Healer compara **Ataque** e **Potência da infusão**, separadamente: comum Nv1 `6 → 8 (+2)` e `42 → 44 PV (+2)`, Nv2 infusão `45 → 47 PV`. Texto: “Cura real limitada à vida faltante; não ressuscita”. Não nomear infusão como ataque total. DPS: ataque / intervalo atual, rótulo **DPS básico teórico, antes da defesa**, sem prometer dano causado. Defesa/intervalo/vida não afetados ficam “Sem alteração”; nenhum bônus de velocidade/energia é inventado.

Acessório compara vida máxima **e HP atual/máximo antes/depois**. Exemplo de E08: `202/202 → 190/190 PV`; `Máximo: −12 · Perda`; `Vida atual: −12 PV, limitada ao novo máximo`. Voltar ao raro: `190/190 → 190/202`, `Máximo: +12`, `Vida atual: sem alteração — equipar não cura`. Para caído, `0/180 → 0/190` e aviso fixo “Continua caído até reiniciar a rota”. Não usar animação/som de cura para essa atualização.

Valores inteiros sem casas; DPS/intervalo usam até duas casas e vírgula decimal, com unidade. HP principal pode manter o teto inteiro da V19; se isso ocultar corte fracionário, a comparação acrescenta antes/depois e perda com precisão suficiente, usando “menos de 0,01 PV” quando necessário. Nunca mostrar uma perda real como `0 / Sem alteração`, nem arredondar o estado para combinar com o texto. Barras usam valores reais. O máximo cresce sem encher a barra por cura fictícia.

Mostrar no rodapé antes da confirmação: **“Equipamento e ganhos desta sessão: perdidos ao fechar.”** O sucesso Volatile repete o limite; não usar “salvo”, ícone de disco ou promessa de continuidade. Checkpointed(Generation) só poderá justificar “salvo” após implementação/retorno real do adaptador futuro; nesta etapa é um estado contratual, não funcionalidade disponível.

## 5. Fase, gerenciamento e saída permanente

Autorização final é `CanConfirm` + revalidação em `Execute`, não a janela aberta nem `Paused`. Inspeção/abertura não pede gerenciamento e não congela a rota. Há um controle explícito **Gerenciar entre encontros** no rodapé de Herói/Status e nas ações da Expedição; todos refletem um único estado de domínio.

| Estado autoritativo | Indicador fixo e ações | Confirmação de equipamento |
|---|---|---|
| March/Loot, Management=None, com ou sem pausa | “Ative o gerenciamento para trocar”; **Gerenciar entre encontros** envia RequestManagement, ativa imediatamente | Bloqueada até prévia nova após ativação; pausa sozinha não basta |
| Combat sem pedido, ativo ou pausado | “Em combate — troca indisponível”; **Gerenciar após a vitória** envia RequestManagement | Bloqueada; pedido não guarda peça/alvo nem enfileira troca |
| Combat, Requested | “Gerenciamento após a vitória”; **Cancelar pedido** envia CancelManagementRequest | Bloqueada; se pausado, também “Retome o combate para alcançar a vitória” com acesso a Expedição |
| March/Loot, Active | “Gerenciamento ativo — rota parada”; **Concluir gerenciamento** envia FinishManagement | Permitida com contexto válido. Pausa genérica indisponível enquanto Active |
| Active com Paused=true preservado | Acrescentar “Ao concluir, a rota continuará pausada” | Permitida; concluir não despausa |
| Complete / Defeat / Retreated | “Rota concluída” / “Equipe derrotada” / “Recuo concluído”; preparação permitida; **Reiniciar rota** separado | Permitida após crédito/terminal; equipamento não retoma, revive ou reinicia |

Ao atender pedido na vitória não final, atualizar XP/nível/HP/recompensas antes da comparação; o domínio para com os 2 s de descanso íntegros e descarta o delta restante. Mostrar “Gerenciamento ativo — descanso suspenso”. Não mostrar recompensa extra por parar. Vitória final limpa pedido e vai para Complete sem descanso final. Derrota/recuo limpam pedido. Não criar contagem regressiva visual que continue enquanto o relógio do domínio está parado.

Comparação aberta em Combat **não passa a estar confirmada** ao chegar a Active/terminal. Invalidar intenção antiga, solicitar nova prévia e exibir “Condições atualizadas; confira os valores antes de confirmar”. Novo clique/ativação é obrigatório; evento já em fila não pode consumir a prévia nova. O mesmo vale para alteração de HP/nível, fase, item, sessão ou tentativa.

**Saída sem depender de janela aberta:** HUD/palco e Expedição exibem estado Requested/Active e ação Cancelar pedido/Concluir gerenciamento, além de **Abrir Herói**. Esse grupo não deve ficar apenas no diário rolável nem sob uma janela fechada. Fechar Herói e Status mantém Active e sua saída visível. Voltar/Cancelar não concluem. Concluir não fecha a comparação automaticamente: invalida a autorização, atualiza o bloqueio se a rota volta a avançar e mantém a navegação. Espaço fora de botões, enquanto Active, não conclui gerenciamento nem alterna Paused; a rota informa que é preciso usar Concluir gerenciamento.

## 6. Controles e callbacks para implementação

IDs abaixo são **nomes lógicos de controles propostos**, não métodos novos de domínio. Integrar nos componentes existentes; manter APIs de regra exatamente nas portas aceitas de GAMELAB-5. O fluxo não depende de um barramento/evento de domínio ainda inexistente.

| Controle / texto | Callback local e porta aceita | Atualização / identidade |
|---|---|---|
| `EquipmentMemberPrevious/Next`, seletores existentes | Seleção compartilhada de ExpeditionView; incrementar SelectionEpoch, invalidar intenção e `Compare` se houver item | MemberId explícito; atualizar Herói/Status/palco; não mutar loadout |
| `EquipmentSlot-Weapon/Armor/Accessory` | Abrir inspeção do slot a partir de `ReadView()` | Binding de MemberId + slot, nunca índice de bolsa |
| `InspectCandidate`, `CompareCandidate` | Inspecionar por `(SessionId, ItemId)`; depois `Compare(ComparisonQuery)` com ação Equip | Guardar origem para foco; não reservar item |
| `ChooseBagCandidate` | Navegar para Herói/bolsa; manter alvo e retorno de Status quando aplicável | Ler localização atual ao abrir; seleção por ID |
| `PreviewUnequip` — “Desequipar…” | `Compare(ComparisonQuery)` com ação Unequip e slot/peça esperada | Prévia nova; não enviar null genérico para remover qualquer peça |
| `ConfirmEquipment` — texto conforme estado | `Execute(EquipmentCommand)` montado da **mesma intenção ativa** | OperationId novo por ação explícita; SessionId, AttemptId, ação, MemberId, Slot, ItemId, ExpectedEquippedItemId, ExpectedStateRevision; não enviar totais/fase/bônus |
| `RequestEquipmentManagement` | `Execute(RouteCommand)` / RequestManagement | Fase observada pelo domínio; nova consulta depois do resultado |
| `CancelEquipmentManagementRequest` | `Execute(RouteCommand)` / CancelManagementRequest | Não usar FinishManagement para cancelar pedido |
| `FinishEquipmentManagement` | `Execute(RouteCommand)` / FinishManagement | Preservar Paused; não chamar Advance na conclusão |
| `ComparisonBack`, `ComparisonCancel`, fechar instrumento | Invalidar intenção, incrementar SelectionEpoch e voltar/fechar | Sem comando de regra; não alterar Requested/Active |
| Pausar/retomar, recuar, reiniciar existentes | `Execute(RouteCommand)` / SetPaused(bool), Retreat, RestartRoute | R permanece reinício de rota como hoje; não confundir com ResetSession nem adicionar reset de sessão na UI |

O controlador conserva intenção imutável = prévia + SelectionEpoch + ação. Cada ativação só pode usar a intenção para a qual o controle foi habilitado. Mudança de alvo/peça, Voltar, Cancelar, fechamento ou revisão invalidante a abandona, inclusive se o usuário voltar ao mesmo alvo. Callback antigo recebe StaleComparison no controlador antes de enviar; o domínio ainda revalida sessão/tentativa/fase/revisão/destino/origem/compatibilidade/espaço, na ordem da arquitetura. Não recriar uma prévia dentro de Confirm para confirmar sem nova ação.

Após Applied, consumir `CommandResult.LatestReadModel` e atualizar bolsa, slots, pendentes, comparação, HP/atributos e Status imediatamente, na mesma ação, sem aguardar os 100 ms ordinários da view. Redesenho da bolsa depende de `InventoryRevision`/conteúdo/localização/ordem, inclusive 21→21. Seleção e foco são restaurados por ID. A integração pode conservar o refresh centralizado para avanços normais e reagir ao retorno de Advance/Execute; não adicionar polling por frame em cada painel nem duplicar cálculo no cliente.

Replay lê a view **atual**, não repõe o estado histórico do resultado original. Deduplique feedback por `(SessionId, OperationId)`. IDs de operações rejeitadas/NoChange não se reutilizam; nova tentativa exige consulta e ação explícitas. Retry de transporte da mesma operação aplicada conserva o envelope/OperationId. Observer/redraw nunca chama Equip novamente. A arquitetura é síncrona/local neste recorte; “Enviando” é proteção de reentrada, sem spinner ou rede fictícia.

## 7. Resultado, bloqueios e textos

| Resultado / código aceito | Texto e recuperação |
|---|---|
| Applied Equip / Unequip | “{peça} equipada em {nome}” / “{peça} devolvida à bolsa”; atributo e delta; “Atributos atualizados; usados nas próximas ações ao continuar”. Manter alvo e mostrar slot final. Som/cue somente após Applied, uma vez, respeitando preferências existentes |
| Applied sem delta, IDs diferentes | “Peça trocada. Sem mudança nos atributos”; identificar antiga e nova. É sucesso de movimento, não ganho numérico |
| NoChange | “Sem alteração”; sem cue, sem evento de troca |
| Replay | “Esta operação já foi aplicada”; apresentar estado atual sem novo cue/sucesso de troca |
| InCombat | “Em combate. Gerencie após a vitória”; botão de pedido, sem confirmar |
| ManagementRequired | “Ative o gerenciamento entre encontros para trocar”; oferecer ação explícita |
| ManagementActive, em ação de pausa | “Conclua o gerenciamento para controlar a pausa”; manter Concluir acessível |
| RoleMismatch / SlotMismatch | “Função incompatível: requer {função}” / “Slot incompatível: requer {slot}”; alvo intacto e troca bloqueada |
| BagFull ao remover | “Bolsa cheia (21/21). Abra uma vaga antes de desequipar”; se houver slot vazio compatível, explicar que equipar uma peça da bolsa pode abrir vaga, dependendo dos pendentes. Sem essa possibilidade, oferecer Voltar, sem prometer liberação de espaço; nenhuma drenagem na recusa |
| ItemNotAtExpectedLocation | “Item não está na bolsa ou no slot esperado. Selecione a peça novamente”; reler view, não confirmar outro ID |
| StaleComparison | “Comparação desatualizada; confira os valores novamente”; nova prévia e ativação, sem retry automático |
| SessionMismatch / AttemptMismatch | “A sessão ou tentativa mudou. Abra uma nova comparação”; abandonar contexto antigo, voltar à visão atual |
| OperationConflict / InvalidCommand / InvalidOperationId | “Não foi possível validar a ação. Abra uma nova comparação”; manter estado e disponibilizar Voltar. Diagnóstico técnico em log, sem expor GUIDs/stack na decisão |
| MemberNotFound | “Integrante indisponível. Selecione um integrante da equipe”; atualizar seleção pela view atual, sem encaminhar a operação antiga |
| InvalidState / NumericOverflow | “Não foi possível aplicar a troca. O estado foi preservado”; bloquear confirmação dessa intenção, registrar diagnóstico e reler view |
| CheckpointFailed / CommitIndeterminate, somente com save futuro | “Não foi possível confirmar a gravação” / “Verificando o estado da gravação”; seguir trava/recuperação da arquitetura, sem afirmar salvo, Applied ou tentar de novo com outro ID |

Capacidade/destino fazem parte da comparação e do resultado: **substituição** devolve antiga na vaga da candidata, mesmo 21/21; **slot vazio** libera uma vaga e pode recuperar o pendente mais antigo; **desequipar** exige vaga anterior e dá prioridade à peça removida. Exibir `Bolsa {n}/21 · {p} pendentes` e `Recuperados da pendência: {k}` quando k>0. Slots equipados não contam na bolsa. Nenhum aviso oferece venda, descarte, desmontagem ou movimentação direta de pendentes.

Depois de sucesso, a página de resultado mantém alvo e peça no destino final; desabilita a confirmação consumida e foca **Voltar à bolsa/equipamentos**, sem mudar automaticamente a ação para desequipar. Ao voltar da remoção, selecionar a peça devolvida pelo ID na bolsa. Ao voltar de Equip, manter o ID agora equipado inspecionável e expor o slot final, sem tratar o novo conteúdo da antiga célula da bolsa como a mesma seleção. Recuo/reinício invalidam contexto; atualizar fase/HP sem som de troca ou recompensa repetida.

## 8. Mouse, teclado, foco e retorno

Mouse: clique simples seleciona/inspeciona; **Comparar**, **Equipar/Desequipar** e controles de gerenciamento exigem ativação própria. Duplo clique em célula não equipa, drag/drop não movimenta, botão direito não é caminho obrigatório. Hover pode explicar, mas toda informação de compatibilidade, bloqueio e composição também existe por foco/seleção.

Teclado: Tab/Shift+Tab percorrem controles visíveis/habilitados da janela ativa; Enter/Espaço ativam botão focado; corpo rolável recebe setas/Page Up/Page Down quando ele tem foco. Não capturar setas globalmente: elas navegam vizinhos na grade/slots; o foco move o item visível via FollowFocus/EnsureControlVisible. Na grade, esquerda/direita seguem células ocupadas na linha e cima/baixo a mesma coluna quando possível; bordas não saltam para outra janela. Tab sempre permite sair da grade para as ações fixas. Células vazias não recebem foco; slots vazios de equipamento recebem, pois têm ação de inspeção/escolha.

| Transição | Foco inicial / restaurado |
|---|---|
| Abrir Herói diretamente | Seletor do integrante atual, preservando comportamento existente; Tab chega a slots/bolsa |
| Abrir Status diretamente | Anterior integrante, como InitialFocus atual; slots entram na ordem do resumo |
| Abrir inspeção de peça/slot | Botão Comparar ou Escolher na bolsa; se item já equipado, Comparar outra peça; não ativar por key repeat |
| Abrir comparação | **Cancelar/Voltar**, sempre habilitado; confirmar só por deslocamento e ativação próprios. A tecla que abriu a página não pode confirmar no mesmo evento |
| Prévia atualizada por domínio/gerenciamento | Conservar foco se o controle permanece válido; se Confirm ficou bloqueado, mover a Voltar e mostrar motivo; não levar foco automaticamente a Confirm |
| Trocar alvo no seletor | Conservar foco no seletor acionado; recalcular a comparação e invalidar callback antigo |
| Applied | Voltar no resultado; sem salto para outro integrante ou botão de mutação |
| Rejected / NoChange / Replay | Voltar ou controle explicativo habilitado; motivo fixo, sem prender foco num botão desabilitado |
| Voltar da comparação | Célula pelo ID se ainda está na bolsa; senão slot correspondente se equipada; senão célula ocupada mais próxima na ordem anterior, depois corpo rolável. Remoção seleciona o ID devolvido |
| Fechar instrumento | Controle que o abriu se válido/visível/habilitado, incluindo janela nativa de origem; fallback launcher Status/Herói/Expedição no palco |

Ordem da comparação: fechar → seletores de alvo/anterior/próximo → corpo rolável e links de composição → Voltar → Confirm (se habilitado) → gerenciamento (se disponível) → fechar. Corpo pode ser um único ponto de foco para rolar texto estático; labels não precisam gerar dezenas de paradas. Explicação permanente permite ler motivo mesmo sem foco em Confirm bloqueado. Recalcular vizinhos quando muda visibilidade, tamanho, origem, revisão ou resultado; preservar foco por ID antes de recriar controles. Borda de foco completa deve continuar distinta do marcador de item/alvo selecionado.

Escape e botão Voltar consomem **uma camada por evento**: explicação interna da comparação → comparação/resultado → inspeção → página de origem; no Status, depois atributo → detalhes → resumo → fechar, conforme camada aberta. Não fechar a janela no mesmo evento que fecha comparação, nem usar Escape para concluir gerenciamento/cancelar pedido/reiniciar. × fecha a janela e descarta intenções pendentes de envio, preservando estado de rota. O Escape do palco sem instrumento continua sob a política existente de Main.

Espaço num botão ativa esse botão; fora de botões mantém pausa comum apenas onde permitida pelo contrato. Enquanto Active, consumir o atalho sem SetPaused/FinishManagement. P continua escudo com validações atuais; R continua reinício explícito da rota e invalida tentativa/intenção antes de qualquer callback posterior. Não reaproveitar R como “remover”. Trocar foco entre instrumentos nativos não fecha nem confirma páginas; só ação explícita abre/eleva janela, sem roubo de foco em refresh.

## 9. Escala, área útil e linguagem visual

Preservar Source Sans 3 400 para corpo, 600 para valores/botões e Limelight apenas nos títulos existentes. Usar os tamanhos existentes de BrassTypography: corpo 14, texto crítico mínimo 13, botões mínimo 36 lógicos, todos com S(). Quebrar texto/empilhar ações antes de reduzir fonte. Números tabulares existentes, sinais +/−, unidade PV/s e nomes completos devem sobreviver à escala. Redução de movimento elimina transições não essenciais; resultado e foco nunca dependem de animação/som.

Comparação e resultado reservam cabeçalho com alvo e rodapé de ações fora do ScrollContainer. Em área curta, substituir seletores grandes/retrato por controles compactos de alvo com o mesmo nome/função; empilhar confirmar/voltar/gerenciar quando não couberem lado a lado. Se o rodapé completo não couber na altura útil, manter **Voltar**, **Confirmar** (ou bloqueio) e **Concluir gerenciamento** prioritários, e usar um botão **Ações da rota** que abre Expedição para os demais comandos. O estado Active continua no HUD. Texto longo de ajuda pode rolar; o motivo curto do bloqueio e aviso de corte/caído ficam fixos com quebra. Não mover ações críticas para baixo do retrato/bolsa.

A aceitação mínima exige corpo rolável utilizável, pelo menos 40 unidades lógicas de altura, e nenhum alvo/controle crítico cortado. Se uma área útil menor não permitir sequer essa estrutura, limitar ao tamanho que cabe, mostrar uma página compacta com alvo, motivo e **Voltar / Abrir Expedição**; não deixar um botão Equipar parcialmente visível ou uma janela fora da tela. A matriz abaixo define as áreas curtas obrigatórias, sem prometer suporte irrestrito a qualquer tamanho.

**Contraste dos novos textos comuns: mínimo 4,5:1**, inclusive mensagens críticas, valores, bloqueios e estados de botão. Cálculo documental por luminância relativa sRGB, tokens opacos de BrassUi, arredondado a duas casas:

| Texto / fundo | Razão calculada | Uso especificado |
|---|---:|---|
| Soot `#241F1A` / Linen `#E4D8B7` | 11,51:1 | Texto/valores/deltas padrão |
| Soot / Paper `#BFB49E` | 7,96:1 | Células/botões secundários novos |
| Tobacco `#40332B` / Linen | 8,58:1 | Ajuda, avisos de sessão e bloqueios |
| Tobacco / Paper | 5,93:1 | Texto de ação desabilitada em superfície opaca |
| Wine `#6C3A32` / Linen, ou inverso | 6,47:1 | Avisos em Linen; botão primário Linen sobre Wine |
| Wine / Paper | **4,47:1** | Não usar para novos textos comuns; raridade rara na nova UI usa Soot + “Raro” |
| Brass `#B99B63` / Linen; Ether `#97A581` / Linen | 1,87:1; 1,85:1 | Ornamento/preenchimento, não texto novo informativo |

Para controles novos, usar texto Soot sobre Paper nos estados secundários e Linen sobre Wine sólido nos estados primários, Tobacco sobre Paper no desabilitado, sem reduzir opacidade. Hover/pressed/focus usam borda e as mesmas combinações opacas; caso se reutilize fill alterado de Styles, medir a combinação efetiva e ajustar **apenas o novo controle** se falhar. O contorno Wine já existente pode servir de foco no papel; exigir pelo menos 3:1 contra a superfície adjacente e separação geométrica do marcador de seleção. Não recolorir o tema global nesta task.

Esses cálculos não são auditoria de pixels de runtime: transparência, fundos reais, hover/pressed, texto recortado e foco devem ser conferidos nas capturas futuras. Ganho/perda/igual/incompatível/bloqueado/aplicado têm palavras e sinais, além de cor. Não exigir percepção de vermelho/verde, hover, som ou movimento para entender/confirmar.

## 10. Aceite de integração e protocolo de QA

Todos os itens desta matriz estão **não executados nesta entrega documental**. Instrumentar fixtures isoladas, sem editar saves em data/. Teste de callback direto não substitui caminho real por input. Os cenários completos de domínio continuam em equipment-contract.md; esta tabela acrescenta observação de UI.

| Caso UI / vínculo contratual | Procedimento e evidência de aceite |
|---|---|
| U01 · E01/E03/E04 | Comparar vazio, melhor e pior; Guarda DEF 10→12, Pistoleiro 20→21 (+1) e 22→20 (−2). Inspeção não equipa; só confirmação muda slot/Status; mesma composição do domínio; resultado identifica alvo |
| U02 · E02/E14 | Comuns A/B: troca aceita com delta 0 e IDs distintos; confirmar A/B não vira alternância. Repetir operação aplicada retorna Replay, sem segunda troca/cue; mesmo ID no destino mostra Sem alteração |
| U03 · E05/E13 | Inspecionar foco com Guarda; manter alvo/incompatível e bloquear. Abrir comparação para Pistoleiro, mudar alvo via Status/palco e voltar: callback anterior recusa, nova ativação obrigatória |
| U04 · E06 | Foco compara ataque e infusão; 42→44 Nv1, 45→47 Nv2, remoção tira ambos; cura real não excede falta de vida. Texto de apoio e DPS têm unidades/rótulos corretos |
| U05 · E07/E08/E09 | Acessório 100/180→100/190; raro→comum 202/202→190/190; voltar 190/202; caído 0/190. Mostrar corte/ausência de cura, sem cue de cura; HP fracionário não é ocultado como zero |
| U06 · E10 | Bolsa 21, substituição com pendentes: antiga volta à vaga da candidata, contagem 21 e pendentes iguais. Grade atualiza por revisão, seleção não migra ao ID que ocupou a célula antiga |
| U07 · E11/E12 | Equipar vazio recupera primeiro pendente; remover com 21 recusa; com 20 devolve antiga sem drenar; com 19 devolve antiga antes de FIFO. Capacidade e quantidade recuperada corretas, nenhuma peça some |
| U08 · E13/F10 | Cancelar/Voltar/×, incluindo ida de Status à bolsa: snapshot de regra intacto; foco restaurado; pedido/Active/Paused não mudam. Fechar ambas as janelas mantém saída no HUD/Expedição |
| U09 · E14/F01/F06 | Abrir em marcha/combat, avançar fase antes do clique e ativar gerenciamento: comparação antiga nunca autoriza confirmação; não executar Equip automaticamente com a prévia nova |
| U10 · E15 | Vitória com nível: prévia usa 118/198 e depois acessório 118/208; sem cura extra ou contagem dupla. XP/recibo recebidos antes da comparação |
| U11 · F02/F04 | Combate pausado: confirmar bloqueado, pedir/cancelar não retoma. Retomar explicitamente permite vitória; sem pedido a rota segue, sem parada tardia retroativa |
| U12 · F03/F05/F07 | Pedido + Advance(1000) para na primeira vitória não final; UI mostra Active/2 s, valores atualizados uma vez. Descanso suspenso em 0,75 s e recargas não contam durante espera; concluir preserva restante, não cura imediatamente |
| U13 · F08/F09 | Complete/Defeat/Retreated permitem preparar; pedido limpo, sem reviver/recreditar. Reinício explícito mantém loadout/ganhos e restaura máximo equipado; callback da tentativa anterior recusa. ResetSession só no harness invalida sessão, sem criar botão novo |
| U14 · F10 | Pausado antes de Active: concluir mantém Paused. Espaço fora de botão durante Active não conclui/despausa. Fechar instrumento mantém indicador e Concluir acessíveis |
| U15 · input/foco | Sem mouse: abrir, selecionar alvo, rolar bolsa, inspecionar, comparar, confirmar, cancelar, voltar e fechar. Input sintetizado por Viewport.PushInput, sem emitir Pressed diretamente; Enter que abre não confirma e Escape consome uma camada |
| U16 · conteúdo/foco | Redraw por swap 21→21, remoção, FIFO, resize e mudanças de alvo: foco visível por ID ou fallback documentado; Tab sai da grade; não há foco invisível/desabilitado nem roubo entre janelas |
| U17 · layout/contraste | Matriz de viewports/escalas abaixo, texto longo de item/alvo/motivo, HP/deltas, bolsa cheia/pendentes; capturar topo/meio/fim da rolagem e Confirm focado. Alvo/ações/saída visíveis, sem rolagem horizontal, texto novo ≥4,5:1 em cada estado |
| U18 · resultado/durabilidade | Applied uma vez; NoChange/Rejected/Replay/cancelamento/redraw sem cue novo. Aviso de perda ao fechar antes/depois, nunca “salvo” em Volatile. Redução de movimento mantém toda informação |

Executar U15–U18 em 1920×1080 a **100/125/150%** e 1366×900 a **100/125/150%** de escala interna; registrar escala/viewport reais nas evidências. Adicionalmente limitar janelas às áreas úteis virtuais **1366×728, 1920×1040 e 2560×1400**, nas três escalas, validando o modo curto mesmo com os três instrumentos visíveis. Ensaiar modo embutido e janelas nativas: o primeiro valida layout/input sintetizado; o segundo precisa confirmar foco/retorno entre janelas e limites da área útil. Scroll deve chegar a todos os atributos e às 21 posições, sem tirar alvo/confirmar/concluir da área fixa.

Usar controles acionados por mouse real e teclado físico em smoke manual, além dos checks automatizados; registrar diferenças de entrada sintetizada e humana. DPI físico Windows/multimonitor, gamepad e toque ficam sem certificação até ensaio específico; não anunciar suporte novo. Não é necessário exigir gamepad/toque para aceitar este recorte Windows/mouse/teclado.

Na implementação, executar Build/Test/Test-Integration/Test-Expedition e os checks UI pertinentes, conforme AGENTS.md e scripts disponíveis naquele momento, com evidências novas. Os passes históricos V18/V19 não validam estes estados. Documentar passos/expected/actual, capturas e revisões de domínio/cliente usados, sem afirmar gameplay integrado apenas por esta especificação.

## 11. Fronteiras, lacunas e entrega documental

Não há discrepância normativa identificada entre as portas aceitas e o fluxo proposto. **Há lacunas de implementação verificadas na base**, a resolver em GAMELAB-8 após domínio e revisão do Director: inserir o controlador de intenção/SelectionEpoch; alimentar os painéis por ReadView/Compare/CommandResult; atualizar grade por revisão; adaptar a precedência de Escape; impedir que o atalho genérico de pausa altere Paused durante Active; e fornecer saída de gerenciamento no HUD/Expedição. Os DTOs/portas da arquitetura ainda são especificações nessa base, não APIs já compiladas. Se GAMELAB-7 mudar seus nomes/semântica, registrar a discrepância ao integrar, sem inventar adapter concorrente ou mudar este contrato silenciosamente.

Mantidos fora do recorte: produção de assets, novas fontes/molduras, aparência 3D por peça, save real, progresso offline, novas ações de reset, transferência para API/legado, descarte/desmontagem, economia/rebalanceamento e suporte de dispositivo novo. Enquanto o save não existir, a informação de sessão é obrigatória. A hipótese de UX é permitir que o jogador identifique alvo, antecipe delta/HP, faça/cancele a troca e reconheça o efeito ao continuar; compreensão/erros/fricção devem ser observados em playtest, sem alegação de retenção demonstrada.

Validação desta entrega: leitura dos contratos e fontes/captura histórica V19; correspondência documental dos 25 cenários; caminhos/links locais, identificação das portas/códigos, cálculo de contraste dos tokens e `git diff --check`. Conforme AGENTS.md, mudança só de Markdown não exige recompilação. Sem build, testes de gameplay, captura nova ou certificação de layout/input nesta rodada. A aceitação de GAMELAB-6 é revisão da especificação; implementação e QA integrado são etapas posteriores.

## 12. Tradução de integração GAMELAB-8

O texto acima conserva a especificação e sua proveniência documental. A implementação passa a usar as assinaturas publicadas em `c8fcff01f5c15390c2c190108cee73c888c753d8`, conforme tradução aceita pelo Director:

- `Advance(double)` continua wrapper void; `AdvanceDetailed(double)` oferece resultados/eventos. O cliente não calcula tempo de gerenciamento.
- `ComparisonQuery` fica em `EquipmentCommand.Query`. `preview.Confirm(operationId)` cria o envelope imutável; não se consulta novamente ao confirmar. Unequip usa candidata null e captura o ID equipado esperado.
- `CommandResult.Original` é o recibo original imutável; `LatestReadModel` é o presente, inclusive em Replay. Apenas `Outcome == Applied` gera feedback, deduplicado por sessão/operação.
- `IExpeditionCheckpointSink.Commit(snapshot)` confirma uma barreira em memória, com `Durability="Volatile"`. Nenhum adaptador de disco foi conectado.
- `EffectiveStats.LevelAttack/LevelDefense/LevelHealth` já incluem a base. O ganho de nível apresentado é a diferença entre esse valor e a base.

Fontes e protocolo de revisão: [integração cliente](equipment-client-handoff.md). A revisão técnica e o QA independente continuam etapas posteriores; o relatório de execução anexado à issue identifica o commit e os testes efetivamente realizados.
