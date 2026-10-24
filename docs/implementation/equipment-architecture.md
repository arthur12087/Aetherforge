# Arquitetura de equipamento, identidade e continuidade da expedição

GAMELAB-5 · Technical Lead · 24/10/2026 · versão 1 para revisão do Game Director. Definição técnica para implementação posterior; as interfaces, arquivos novos e schema propostos abaixo ainda não existem no runtime. Nenhum save é criado ou migrado por esta entrega.

## Entrada, revisão de Design e base integrável

Entrada normativa: [equipment-contract.md](equipment-contract.md), versão 1 de GAMELAB-3, com E01–E15/F01–F10. O Director aceitou seu comportamento em 24/10/2026, conforme descrição atual de GAMELAB-5 e comentário `01a16deb-aeab-75b2-aba1-bfdb247030eb` em GAMELAB-1. Revisão técnica: **contrato implementável, sem alteração das regras de Design**. As decisões de representação, invalidação, atomicidade e retomada neste documento preenchem as responsabilidades técnicas deixadas pelo contrato. Os 25 cenários são critérios futuros; não há certificação de equipamento implementado.

O contrato foi baixado pelo CLI autenticado, anexo `01a16de9-65f8-732b-b4ea-2534ea6aa0a1`; patch `01a16de9-66db-73ce-947c-33e5870989e8`. Aplicação com `git am` preserva autor, data de autoria, mensagem e coautoria do commit original `21349e4f99fde30bbd65002c1c2ca42ade9cf942`. O arquivo incorporado é byte a byte igual ao anexo, SHA-256 `01cfd22541d21d5ffee686e9b1515e6a93962c3fc80fba8f80fa9e3a883002db`. Não foi criada outra versão comportamental.

Checkout desta task: `agent/technical-lead/dcbd6bff353d`. A consulta a `origin/HEAD` e `origin/main` confirmou `main` em `aca2d622ae590e0044a8e744f392fd6ed787161d`; o patch passou em `git apply --check` nessa base. O PR #1, head documental `3ea5a8b20ae2c149239d4ca463c352c032c58670`, estava aberto e sem merge na consulta à tabela de PRs de GAMELAB-4. Sua head não foi adotada como base integrada. Conferir novamente a base ao integrar; não retornar à revisão da auditoria se novas entregas já estiverem em main.

Evidências consultadas nesta base:

| Local | Estado observado / consequência |
|---|---|
| `global.json:2`; `src/Aetherforge.Client/Aetherforge.Client.csproj:1`; `src/Aetherforge.Domain/Aetherforge.Domain.csproj:2` | SDK fixado 10.0.401, Godot.NET.Sdk 4.7.2, cliente net8.0 e domínio netstandard2.1. API, persistência e checks usam net10.0 nos respectivos projetos. |
| `scripts/Setup.ps1`; `scripts/Environment.ps1` | Ferramentas portáteis fixadas. Neste checkout `.tools` está ausente e `dotnet --list-sdks` não listou SDK; não se afirma uma versão instalada/resolvida aqui. O roadmap registra execução anterior de Godot 4.7.2/.NET 10.0.401, não repetida nesta entrega. |
| `src/Aetherforge.Domain/Party.cs:17`; `src/Aetherforge.Domain/Progression.cs:30` | Atributos por nível, HP por nível preservando vida faltante; não há loadout. Manter defaults vazios para consumidores legados de PartyMember. |
| `src/Aetherforge.Domain/Expedition.cs:77`, `:118`, `:133`, `:223` | Infusão derivada separadamente; reinício de tentativa; scheduler determinístico e crédito no fim do encontro. Equipamento deve entrar nesses pontos sem substituir o simulador. |
| `src/Aetherforge.Domain/ExpeditionRewards.cs:43` | Bolsa/pendentes, contador local, RNG e créditos por etapa privados. São estado necessário, não reconstruível apenas da bolsa. |
| `src/Aetherforge.Client/ExpeditionView.cs:371`, `:382`, `:392` | View avança a simulação, compartilha seleção e trata pausa/reinício. Precisa encaminhar comandos e invalidar intenções; janela não é autoridade de regras. |
| `src/Aetherforge.Client/ExpeditionLootView.cs:46`; `src/Aetherforge.Client/ExpeditionStatusPanel.cs:152` | Bolsa atualiza por contagem e Status consulta o domínio. Substituição exige revisão de conteúdo e atualização imediata após sucesso. |
| `src/Aetherforge.Domain/Integration.cs`; `src/Aetherforge.Persistence/IntegrationStore.cs:17`, `:65`, `:110` | Modelo legado distinto, GUIDs, recibos por operação, JSON com lock/temp/flush e sem versão/backup. Não contém sessão V19. |
| `src/Aetherforge.Api/Program.cs`; `src/Aetherforge.Client/Main.cs:48` | API local única para integração; Expedition é o modo padrão e não usa essa API para sua simulação. |
| `tests/Aetherforge.Checks/ProgressionChecks.cs`; `tests/Aetherforge.Checks/ExpeditionChecks.cs` | Oráculos existentes de crescimento, caídos, recompensas, reinício e tempo. Regressões a preservar, sem cobertura do equipamento futuro. |

## Decisões de autoridade e fronteiras

**A1 — Autoridade V19 local no agregado ExpeditionSimulation**, com comandos serializados. Mantém a arquitetura já em uso, permite checks sem Godot e não introduz dependência de rede para trocar peças. A API/IntegrationStore continua autoridade exclusiva do inventário legado. Não enviar `local-*` a `/integration/*`, não executar os dois modelos sobre o mesmo item, não adicionar autenticação, serviço remoto ou banco neste recorte. Autoridade local não oferece garantia contra edição pelo dono da máquina; futura transferência a conta/mercado exige projeto e aprovação próprios.

**A2 — Um dono para cada mutação.** ExpeditionSimulation coordena fase, Party, Rewards e equipamento. Um componente de inventário da expedição, composto em Rewards, mantém cadastro de peças, ordem da bolsa/pendência e loadouts. Helpers de regras calculam/preparam, mas não têm cópias mutáveis independentes. Não usar `EquipmentRules.Equip` do legado: seus scopes e vínculo são outro contrato.

**A3 — Cliente apresenta e solicita.** Seleção, foco, janelas, intenção de comparação e deduplicação de feedback ficam no controlador de apresentação. Os controles não atribuem Health, bônus, posição de item, Paused ou créditos diretamente no fluxo novo. Chamadas de domínio e ticks usam a mesma fila local, na thread principal atual. Não é necessário introduzir workers, barramento distribuído ou refazer Main.

**A4 — Persistência desacoplada e posterior.** Snapshot é DTO sem Nodes. Um port de checkpoint recebe o candidato inteiro e devolve confirmação; o adaptador em memória basta para equipamento. Para o futuro save, propor `src/Aetherforge.Expedition.Persistence/` como biblioteca net8.0 com System.Text.Json/IO do runtime, referenciando Domain. Client net8.0 e checks net10.0 podem consumi-la; não referenciar a biblioteca atual net10.0 pelo cliente net8.0. Esse pequeno adaptador separado evita reescrever/multitargetar IntegrationStore e não adiciona pacote externo. Sua criação pertence à task de save.

| Camada / localização proposta | Responsabilidade e contrato |
|---|---|
| Domain: `ExpeditionEquipment.cs` | Loadout, propriedade, compatibilidade, plano de movimento, validação de IDs/capacidade. Sem IO, relógio real ou Godot. |
| Domain: `ExpeditionStats.cs` | Cálculo puro único de atributos e bônus finais; reutilizado por PartyMember, infusão e comparação. |
| Domain: `ExpeditionCommands.cs` | Envelopes, códigos, resultados e recibos de operações da sessão. |
| Domain: `ExpeditionSnapshot.cs` | Captura/restauração validada de estado; port `IExpeditionCheckpointSink`. Codec JSON fica fora do domínio. |
| Client: controlador de comparação, junto a ExpeditionView | Contexto de seleção e intenção; gera OperationId, revalida intenção ativa, confirma explicitamente, renderiza resultado atual. |
| Client: LootView / StatusPanel / HUD | Projeções imutáveis; atualização por revisão; saída de gerenciamento acessível. Preservar Herói/Status e palco. |
| Adaptador net8.0 futuro | JSON versionado, lease, gerações e recuperação; caminho recebido da composição do cliente. Não chama regras de concessão ao carregar. |

Nomes de tipos/arquivos são a base de implementação acordada nesta proposta, não obrigação de gerar infraestrutura além do necessário. Preservar APIs de leitura atuais por projeções enquanto os consumidores são adaptados. Encapsular os caminhos novos; não usar esta task para reformular o protótipo multiplayer ou CombatPreview.

## Modelo de identidade, loadout e invariantes

Identidade de uma peça V19 é o par `(SessionId, ItemId)`, comparado ordinalmente. `ItemId` conserva exatamente `local-000001` etc.; D6 é largura mínima, não limite de seis dígitos. SessionId é um identificador opaco novo na criação/reset da sessão, distinto de OperationId e do GUID de item legado. Uma string local nunca é convertida em GUID de item. RestartRoute conserva SessionId e IDs; incrementa AttemptId monotônico. Reset cria outra SessionId, contador/RNG/ganhos/slots vazios e invalida a sessão anterior.

Para implementar sem ambiguidade: novas SessionId e OperationId usam UUID v4 em formato N (32 caracteres hexadecimais minúsculos), gerados uma vez na sua criação; rejeitar outra forma no protocolo novo. Isso não altera o parser legado nem o formato de ItemId. AttemptId começa em 1; AttemptId, revisões, sequências de recompensa/item e geração de checkpoint são long não negativos com incremento checked. IDs de evento mantêm int checked como na API atual, sem wrap ou reutilização dentro da tentativa.

Nesta formação fixa, MemberId estável é `tank`, `dps`, `healer`, mapeado explicitamente a PartyRole e aos índices existentes 0/1/2. Não usar nome traduzido, NodePath, índice da bolsa ou retrato como identidade. O modelo não permite dois membros do mesmo papel; expansão de elenco requer outro contrato/versionamento.

Representação proposta:

```text
ExpeditionItems
  ItemsById: ItemId -> {Role, Slot, Rarity}   # cadastro único, imutável
  BagIds: lista ordenada de ItemId, até 21
  PendingIds: fila ordenada de ItemId por concessão
  Loadouts: MemberId -> {Weapon: ItemId?, Armor: ItemId?, Accessory: ItemId?}
```

O cadastro descreve a instância; somente as três coleções de localização representam propriedade. Bolsa compacta conserva ordem; substituição troca o ID no mesmo índice; desequipar com vaga anexa ao fim. `Rewards.Inventory`, `PendingDrops` e slots expõem projeções de leitura desse estado. Não manter simultaneamente listas editáveis de objetos e um segundo mapa de localização.

| Invariante | Validação obrigatória |
|---|---|
| I1 · partição | Cada ID cadastrado ocorre exatamente uma vez em BagIds, PendingIds ou um dos nove slots; todas as referências resolvem; não existem IDs vazios/duplicados. |
| I2 · compatibilidade | Slot igual a LootSlot e Role igual a PartyRole do destino. Três slots por membro, inicialmente null; não há equipamento inicial/importado. |
| I3 · conservação | Equipar/desequipar preservam o conjunto de IDs e a contagem total, inclusive recuperação FIFO. Só vitória pode acrescentar IDs; este recorte não destrói/consome peças. |
| I4 · capacidade | Bolsa ≤21; slots não ocupam bolsa; pendência não é origem de equipar; retorno da antiga tem prioridade sobre FIFO. |
| I5 · histórico | Recibo de vitória referencia DropId e valores concedidos. Referência histórica/comparação não é nova localização nem nova instância possuída. |
| I6 · recursos | Equipamento não modifica XP, carteira, seed/estado RNG, contador de itens, créditos, tentativa, relógios ou recargas. Só HP pode cair ao limitar ao novo máximo. |
| I7 · publicação | Uma revisão publicada contém bolsa/slots/pendentes/atributos/HP coerentes. Nunca publicar o espaço intermediário da substituição. |
| I8 · limites | Nível 1–20; HP finito em [0, máximo]; enums conhecidos; contadores não negativos e sem overflow. Dado inválido é erro, não oportunidade de corrigir apagando peça. |

`ExpeditionReward.Drop` pode continuar como projeção de leitura para clientes antigos. No snapshot novo, guardar `DropId` e metadados históricos necessários; resolver a peça pelo cadastro. Não desserializar o objeto de cada recibo para dentro da bolsa. Nesta versão sem destruição, cada DropId de recibo deve resolver para a mesma peça possuída; recibos sem drop usam null. Os recibos legados continuam em seu arquivo/modelo sem essa conversão.

## Cálculo único e política de HP

Port conceitual `CalculateStats(baseStats, level, loadoutItems) -> EffectiveStats` é puro. `EffectiveStats` contém MaxHealth, Attack, Defense e InfusionPotential (aplicável ao Healer), além da decomposição Base/Nível/Equipamento para a UI. `Compare` chama a mesma função para estado atual e loadout candidato; combate, descanso e Status leem seus resultados. Não guardar/atualizar um acumulador de bônus ao clicar.

Para as bases inteiras atuais, `Scale(B,p,L) = floor(B * (100 + p*(L-1)) / 100)`; `MaxHealth = Scale(BaseHealth,10,L)+HealthBonus`; `Attack = Scale(BaseAttack,6,L)+AttackBonus`; `Defense = Scale(BaseDefense,5,L)+DefenseBonus`; `InfusionPotential = Scale(42,8,L)+FocusAttackBonus`. Apenas Weapon do Healer contribui à infusão; não usar ataque total nem escalar equipamento por nível. Valores atuais de bônus vêm de ExpeditionDrop: arma/armadura 2/3/4; acessório 10/15/22. Validar slot/raridade antes de consultar, sem transformar enum desconhecido em Rare pelo fallback atual.

Reutilizar a semântica de CharacterProgression.ScaleBase, calculando a partir da base original com intermediário double (ou inteiro largo para as bases inteiras do catálogo). O atalho atual no nível 1 equivale à fórmula para essas bases. Não alterar a aceitação de bases fracionárias de consumidores legados incidentalmente. Vida/dano continuam float e tempo double; não arredondar HP fracionário de escudo/descanso para inteiro ao trocar ou salvar. A UI pode arredondar sua apresentação, nunca o estado. Recursos, sequência e contador de item continuam long com soma checked; overflow recusa o candidato inteiro, sem wrap, novo ID ou crédito parcial.

`ClampAfterEquipment(hpBefore,newMaximum) = min(hpBefore,newMaximum)` para estado previamente válido. Não chamar GainExperience, Heal, RestartRoute ou Reset para aplicar o loadout. Aumento de máximo não cura; zero continua zero. Ganho real de nível conserva a vida faltante dos vivos com os máximos equipados antes/depois, como hoje; crédito de vitória termina antes da prévia de gerenciamento. Descanso trata pelo máximo vigente somente no seu término real, com healer e alvo vivos.

Oráculos de referência: armadura comum Guarda Nv1 dá DEF 12 e dano `max(1,17-12)=5`; arma comum→incomum Pistoleiro dá ATK 20→21, delta +1; foco comum dá ataque 8/infusão 44 no Nv1 e infusão 47 no Nv2; acessório raro→comum com Guarda 202/202 dá 190/190 e voltar ao raro deixa 190/202; subir de nível com 100/180 dá 118/198, então equipar acessório comum dá 118/208. Os cálculos não certificam runtime futuro.

## Interfaces de consulta, intenção e confirmação

Assinaturas abaixo são contratos lógicos, a traduzir para classes/structs compatíveis com netstandard2.1; não pressupõem records ou serializer no domínio.

```text
ReadView() -> ExpeditionReadModel
Compare(ComparisonQuery) -> EquipmentPreview
Execute(EquipmentCommand) -> CommandResult
Execute(RouteCommand) -> CommandResult
Advance(delta) -> AdvanceResult
CaptureCheckpoint() -> ExpeditionSnapshot
RestoreValidated(snapshot) -> ExpeditionSimulation
IExpeditionCheckpointSink.Commit(candidate) -> CommitOutcome
```

`ExpeditionReadModel` é cópia/projeção imutável com SessionId, AttemptId, StateRevision, InventoryRevision, fase/Paused/Management, integrantes/atributos/HP, bolsa/slots/pendentes, carteira e posição no ledger. Nenhum consumidor recebe a coleção interna mutável.

| DTO | Campos necessários |
|---|---|
| ComparisonQuery | SessionId, AttemptId, MemberId, Slot, CandidateItemId opcional, ação Equip/Unequip. Para desequipar, a peça atual é a origem; não confundir null com “remover qualquer peça”. |
| EquipmentPreview | Identidades acima; ExpectedStateRevision; ExpectedEquippedItemId (null explícito); origem observada; totais/decomposição antes/depois, HP antes/depois, delta, DPS teórico; CanConfirm e BlockReason. Sem reserva/mutação. |
| EquipmentCommand | OperationId; SessionId; AttemptId; ação; MemberId; Slot; ItemId opcional; ExpectedEquippedItemId; ExpectedStateRevision. Não aceita bônus, máximo ou fase fornecidos pela UI. |
| RouteCommand | OperationId, SessionId, AttemptId, ação e payload explícito; pausa usa SetPaused(bool), nunca Toggle. Request/Cancel/Finish são ações distintas. Reinício/Reset recebem identidades da sessão/tentativa de origem. |
| CommandResult | OperationId; Outcome=Applied/NoChange/Rejected/Replay; código; AppliedRevision opcional; IDs afetados; membro/slot; pendentes recuperados; resultado original se Replay; Durability=Volatile ou Checkpointed(Generation). LatestReadModel vem do estado atual. |
| AdvanceResult | Tempo consumido e descartado; StopReason (horizonte, pausa, gerenciamento, terminal, falha de checkpoint); revisão final e eventos novos. Preservar wrapper void atual se útil aos consumidores. |

O controlador mantém `SelectionEpoch` e uma intenção ativa: referência à prévia imutável + epoch + ação. Seleção em Herói, Status ou palco, troca de peça, cancelar, voltar ou fechar a comparação invalidam essa intenção, mesmo que o jogador volte ao alvo original. Um callback deve apresentar a mesma intenção ainda ativa; o controlador recusa StaleComparison antes de enviar ao domínio se ela expirou. Não confiar no alvo capturado por uma closure antiga, nem fabricar nova prévia e confirmar automaticamente.

O domínio não possui foco/janelas: independentemente do controlador, revalida SessionId/AttemptId, revisão, origem, destino, compatibilidade e fase. Mudanças em HP, nível, propriedade, autorização/fase, tentativa ou tempo consumido invalidam a revisão comparada. Uma inspeção em combate pode continuar aberta e ser redesenhada; entrar em gerenciamento exige prévia nova e outro clique. Observar/recalcular não reserva item nem grava comando.

Uma consulta Equip pode inspecionar a peça que já ocupa o mesmo slot, para apresentar NoChange. Após validar contexto/fase/revisão/compatibilidade, essa coincidência é a única exceção à origem bolsa e retorna sem plano de movimento. Unequip sobre slot vazio validado também retorna antes de exigir vaga. Nenhuma dessas exceções permite aceitar um callback antigo: revisão e destino esperado precisam corresponder ao estado atual.

StateRevision é monotônica por sessão, incrementada em cada commit de mutação do agregado, incluindo avanço real de tempo e comandos de rota; não incrementa por leitura, rejeição, replay ou no-op. Um Advance pode produzir vários commits internos, publicados para UI apenas ao retornar. InventoryRevision muda quando conteúdo/localização/ordem muda, incluindo substituição 21→21 e concessão, não apenas contagem. Reentrada por callbacks de observadores durante Execute/Advance é proibida; solicitações entram depois na fila.

### Operações e códigos

| Operação | Regra / efeito |
|---|---|
| Equip | Somente bolsa→slot compatível; troca inclui devolução da antiga à posição do candidato. Mesmo ID já no mesmo slot, com prévia atual válida: NoChange; não mover de outro slot/pendência. |
| Unequip | Somente slot indicado→bolsa; exige vaga prévia; slot já vazio com prévia atual: NoChange. |
| RequestManagement | March/Loot: ativa imediatamente; Combat: solicita parada única sem pausar; terminal: NoChange com contexto terminal. Já solicitado/ativo: NoChange. |
| CancelManagementRequest | Limpa apenas Requested; ausente: NoChange. Não conclui gerenciamento nem altera Paused. |
| FinishManagement | Active→None, preservando Paused e deadlines; ausente: NoChange. Sem chamada interna a Advance. |
| SetPaused | Estado explícito; rejeitar alteração enquanto gerenciamento Active. Repetir o mesmo valor: NoChange. Pausa em Combat nunca autoriza Equip. |
| Retreat | Terminal imediato se rota ativa, limpa gerenciamento/pedido, invalida prévia; conserva crédito já concluído. Terminal: NoChange. |
| RestartRoute | Ação explícita já existente, preservando sua disponibilidade atual (inclusive atalho); nova AttemptId, conserva XP/saldos/peças/loadouts/seed/contador/RNG/recibos; HP até máximo equipado e tempos novos. Nenhuma troca a invoca. |
| ResetSession | Operação explícita de reset já existente/testes; nova SessionId, estado inicial e slots vazios. Não acrescentar reset implícito a fluxo de equipamento nem nova ação de UI nesta definição. |

Não transformar dois cliques em Equip e Unequip por alternância. OperationId é novo identificador opaco por ação explícita; retry de transporte usa o mesmo envelope imutável. Recibos de operações e recibos de vitória são coleções diferentes.

Ordem de validação para equipamento: validar forma do envelope/OperationId → conferir SessionId → consultar recibo da operação e colisão de payload → conferir AttemptId/membro/slot/enums → conferir fase segura → revisão e destino esperado → origem/compatibilidade/capacidade → plano de cálculo/invariantes → commit. Assim Combat atual devolve InCombat mesmo com fase/revisão antiga; numa fase segura, intenção antiga devolve StaleComparison. O bloqueio do controlador por mudança de seleção ocorre antes de entrar no domínio. Sessão antiga nunca ganha acesso pelo recibo de outra sessão.

| Código estável | Resultado / texto ou ação de apresentação |
|---|---|
| InvalidCommand / InvalidOperationId | Envelope/enum/identificador inválido; nenhuma mutação. |
| SessionMismatch / AttemptMismatch | Sessão/tentativa terminou ou foi reiniciada; descartar intenção, sem retry automático. |
| OperationConflict | Mesmo OperationId de operação aplicada, payload diferente; recusar, sem reinterpretar. |
| MemberNotFound | Alvo não existe nesta formação. |
| InCombat | “Em combate”; aplica inclusive em Paused. |
| ManagementRequired | “Ative o gerenciamento entre encontros”; March/Loot sem Active. |
| ManagementActive | Ação de pausa indisponível; apresentar Concluir gerenciamento. |
| StaleComparison | “Comparação desatualizada; confira os valores novamente”; nova prévia e nova ação. |
| ItemNotAtExpectedLocation | “Item não está na bolsa/slot esperado”; inclui ID inexistente ou de outro membro/pendência. |
| RoleMismatch / SlotMismatch | “Função incompatível” / “Slot incompatível”; manter alvo selecionado. |
| BagFull | “Bolsa cheia para desequipar”; nenhuma drenagem de pendentes. |
| NoChange | “Sem alteração”; IDs distintos com atributos iguais são Applied, não NoChange. |
| InvalidState / NumericOverflow | Falha de invariante/capacidade numérica; preservar estado e registrar diagnóstico técnico fora da UI de decisão. |
| CheckpointFailed / CommitIndeterminate | Apenas quando houver adaptador durável: não afirmar “salvo”; suspender mutações até recuperação definida adiante. |

Rejeição, NoChange e Replay deixam o snapshot autoritativo inteiro igual ao começo desse comando. Não registram novo recibo, evento, RNG, revisão ou drenagem FIFO. Registrar somente operações **aplicadas**, incluindo comandos de rota, em mapa `OperationId -> payload completo + resultado original`. Consultar esse mapa antes de revisão/fase da operação original impede repetir uma troca já aceita ou solicitar outra parada depois de a anterior ser atendida. Resultado Replay não representa sucesso novo; retornar separadamente a view atual para nunca redesenhar um snapshot histórico sobre o presente.

Não persistir rejeições/no-ops permite essa igualdade estrita. O protocolo proíbe reutilizar seus OperationIds; para tentar de novo após falha é preciso nova prévia/ação explícita. Não há retry automático de erro de regra. O domínio garante deduplicação das operações aplicadas, não um cache histórico de toda consulta recusada. Guardar o mapa durante toda a sessão (inclusive após RestartRoute) e no futuro save, sem poda silenciosa; eventual compactação exige protocolo de expiração próprio. Reset invalida tudo pela SessionId nova.

ResetSession é a exceção à conservação do mapa: o comando pertence à SessionId que termina e a nova sessão começa com mapa vazio. Retry com a identidade antiga recebe SessionMismatch, sem executar outro reset. RestartRoute permanece na mesma sessão e seu resultado aplicado é deduplicado mesmo quando o envelope contém a AttemptId anterior.

Feedback de equipamento é publicado somente em Applied de Equip/Unequip, uma vez por `(SessionId, OperationId)`, após view coerente. Replay, NoChange, falha, cancelamento e carga não emitem cue de troca. Observadores que falharem não desfazem commit nem disparam operação outra vez.

## Transação de propriedade e publicação

Cada Execute lê um estado estável e prepara um candidato por cópia dos dados afetados; não altera listas expostas nem reaplica bônus incrementais. O agregado é o único ponto de commit. Não usar serialização JSON como mecanismo obrigatório de clone no domínio.

1. Validar a intenção e todo o estado relevante na ordem acima. Calcular loadout final, EffectiveStats e HP limitado. Sem custo/recurso/recarga.
2. Equip em slot vazio: remover candidato da bolsa e instalar. Substituição: remover candidato no índice i, instalar, inserir antiga em i, sem ocupar essa vaga com pendente. Unequip: exigir bolsa <21 antes de retirar e anexar peça ao fim.
3. Após o movimento principal, transferir PendingIds FIFO apenas enquanto bolsa <21. Não executar esta etapa em leitura, erro, no-op ou replay.
4. Validar I1–I8, revisão e conservação do conjunto; acrescentar recibo da operação aplicada no mesmo candidato. Se houver falha, descartar candidato, mantendo também contadores/recibos originais.
5. Pelo port, confirmar candidato em memória agora; no futuro adaptador durável, confirmar checkpoint antes de publicar Applied durável. Trocar a raiz de estado uma vez, incrementar revisões pertinentes e emitir um resultado/view. O refresh após sucesso é imediato, não espera os 100 ms da atualização ordinária.

Exemplos de capacidade: substituição com bolsa 21 mantém 21 e não drena pendentes; equipar slot vazio com 21 e pendentes abre uma vaga definitiva e recupera um; desequipar com 20 ocupa a última vaga antes da FIFO; com 19 e pendentes, devolve antiga e recupera exatamente um. Nenhuma operação move item para pendência como solução para falta de espaço.

## Scheduler e gerenciamento

`Management = None | Requested | Active` é independente de ExpeditionPhase e Paused. Requested só permanece em Combat; Active só em March/Loot. Terminais limpam ambos e permitem preparação sem relógio ativo. Paused fica intocado ao entrar/concluir gerenciamento; não copiar/alternar esse bool para representar a trava. Bloqueio efetivo: `Paused || Management==Active || Terminal`.

Serialização: um comando ou Advance por vez; a ordem real de enfileiramento é a ordem autoritativa. Comando antes do tick observa o estado anterior; comando depois observa o resultado. Não há execução de UI no meio de um crédito/transação. Não antecipar um clique que chegou depois de Advance nem abrir gerenciador retroativamente numa vitória anterior.

```text
Advance(delta):
  rejeitar delta negativo/NaN/infinito antes de qualquer early return
  se delta == 0 ou Paused ou Active ou Terminal: retornar sem consumir
  enquanto houver horizonte e não houver trava/terminal/falha:
    escolher próxima fronteira como hoje (até 50 ms/deadline exato)
    preparar passo, validar HP e resolver ações em ordem V19
    ao reconhecer vitória: executar bloco indivisível abaixo
    confirmar passo/fronteira; reavaliar trava antes de outro passo
  descartar delta restante ao parar; nunca acumular catch-up
```

Bloco da vitória, dentro de TickCombat, antes de inimigos/infusão posteriores à condição já resolvida:

1. Manter desempate Guarda→Pistoleiro→Alquimista→inimigos e cancelar ataque do inimigo morto. Vitória reconhecida uma vez; crédito pelo par AttemptId/StageIndex, não pela leitura do último evento.
2. No mesmo candidato: incrementar Victories, XP/níveis e HP de nível (inclusive XP dos caídos), recursos, RNG/drop/contador, destino bolsa/pendência, recibo e chave creditada. Emitir reward/level/drop somente após esse commit. Não permitir estado “creditado sem recibo” ou “moeda antes de RNG”.
3. Se StageIndex=4: Complete, limpar gerenciamento/pedido, sem descanso/tratamento final. Parar mesmo com delta sobrando.
4. Caso contrário: Loot, phaseBeganAt=Elapsed, phaseEndsAt=Elapsed+2. Se Requested, consumir pedido e tornar Active no mesmo candidato, com dois segundos íntegros restantes.
5. Se Active, sair do laço já nessa fronteira, sem EndRest ou ação extra. Sem pedido, consumir normalmente o restante de delta. EndRest cura uma vez, só vivos/healer vivo, incrementa etapa e inicia marcha como V19.

Manter _phaseBeganAt/_phaseEndsAt/_marchFrom, _heroNext/_enemyNext/_enemySwings e deadlines de escudo/infusão. Gerenciamento congela Elapsed e, por consequência, todos esses tempos restantes; trocar/concluir não os recalcula, zera ou estende. Um descanso com 0,75 s restante continua com 0,75 s após espera humana. O estado não acumula tempo real parado; somente novos deltas após concluir progridem.

Retreat limpa Requested/Active e termina no ponto serial atual, sem crédito parcial. Se crédito já foi confirmado antes, permanece. Derrota limpa pedido sem inventar vitória. RestartRoute conserva ganhos/loadout/RNG/contador/recibos e começa nova tentativa; restaura HP e temporizadores porque é reinício explícito. ResetSession é a única operação que começa outra sessão. Nenhum desses efeitos é atalho de Equip.

## Snapshot e checkpoint para a retomada posterior

**S1 — Checkpoint é estado exato num limite de commit**, inclusive quando a fase é Combat; “limite seguro” significa ausência de transação parcial, não permissão para equipar. Retomada não refaz RestartRoute, CreditVictory, BeginAttempt, BeginMarch ou sorteio. A criação por factory/restauração valida e injeta estado sem efeitos colaterais de construtor que publica reset/recompensa.

Schema proposto: envelope JSON `Format="aetherforge.expedition"`, `SchemaVersion=1`, `RulesetId="v19-equipment-1"`, `Generation` monotônica, `SavedAtUtc` informativo, `Payload` e checksum SHA-256 dos bytes UTF-8 do payload armazenado. O codec grava o payload como bloco/string de bytes canônicos e calcula/verifica esse bloco exato; não comparar hash de JSON reordenado. Hash detecta corrupção acidental, não autentica jogador. Serializer invariante com precisão round-trip de float/double, sem NaN/Infinity. Persistir valores de enum como códigos estáveis nomeados, sem ordinal implícito.

| Bloco de Payload | Estado necessário e correspondência atual |
|---|---|
| Identidade | SessionId, AttemptId, StateRevision, InventoryRevision, RulesetId/versão do conteúdo. IDs não mudam ao restaurar. |
| Party | MemberId/Role/ordem estáveis; bases ou versão explícita do catálogo; nível e XP restante, HP absoluto float. Validar XP/nível, não chamar GainExperience na carga. Derivar máximos/ataque/defesa/infusão do nível+loadout. |
| Propriedade | ItemsById com Role/Slot/Rarity e IDs originais; BagIds em ordem; nove slots; PendingIds em ordem FIFO. Sem copiar propriedade de recibos. |
| Recompensas | Coins/Scrap/Components; seed original; RandomState uint; algoritmo `lcg32-rejection-v1`; contador de itens **LastIssuedItemSequence**, equivalente a `_nextItem` (próximo é +1); NextRewardSequence; todos os recibos. |
| Créditos | Recibos com Sequence, AttemptId, StageIndex, recursos concedidos, ExperienceGranted e LevelsGained por MemberId, DropId opcional. Chaves históricas `(AttemptId,StageIndex)`, CurrentAttemptCredited[5] equivalente a `_credited`; sequência global por sessão. Validar unicidade e coerência, sem reconstruir ganhos por soma/reexecução de recibos. |
| Rota | Phase, StageIndex, Victories, Elapsed, Distance, EncounterAt; _phaseBeganAt/_phaseEndsAt/_marchFrom. Validar combinações de fase/etapa/créditos, inclusive Complete com quinta vitória e Loot após crédito. |
| Combate | Inimigos em ordem com kind/nome/base máxima/HP atuais, _heroNext[3], _enemyNext[2], _enemySwings[2]. Contadores de golpes preservam alvo especial a cada terceiro ataque. Guardar também inimigos já preparados na marcha. |
| Recargas | _shieldUntil, _shieldReadyAt, _healReadyAt absolutos em segundos de Elapsed; não guardar apenas bool pronto. Deadlines passados/zero são válidos; não transformar em novos cooldowns na carga. |
| Travas | Paused e Management (incluindo Requested antes da vitória); fase/estado coerentes. O contexto de UI e sua comparação não são persistidos. |
| Operações | Mapa de operações aplicadas: OperationId, envelope/payload original completo e resultado/revisão original. Preservar após nova tentativa e carga; reset é outra SessionId. |
| Apresentação/eventos | _nextEventId, LastEvent e buffer limitado de eventos já resolvidos ou seu marcador. Nunca reemitir ataques/cura/troca/recompensas na carga. Consumidores começam no marcador atual; feedback antigo não é reproduzido. |

Não derivar o contador de itens do tamanho da bolsa, não zerá-lo no reinício, não reusar ID porque está equipado/pendente. Validar `LastIssuedItemSequence >= maior sufixo emitido` e preservar exatamente seu valor, sem renumerar. Prefixo/formato desconhecido é incompatibilidade a tratar, não convite à conversão. O contador pode ultrapassar seis dígitos. Seed original, RandomState e algoritmo são necessários: seed sozinho repetiria sorteios e perderia o ponto atual.

Preservar o algoritmo atual: `random = unchecked(random*1664525u+1013904223u)` e rejeição de viés `threshold=unchecked(0u-bound)%bound`. Na vitória comum, sortear chance; chefe dispensa esse sorteio; se houver drop, sortear raridade, função e slot nessa ordem. Reset volta à seed original; reinício/Equip/carga não avançam RNG. Alterar algoritmo/ordem exige RulesetId novo e estratégia explícita, nunca migração silenciosa.

**S2 — Frequência futura de gravação.** Checkpoint obrigatório para cada operação aplicada, bloco completo de vitória (incluindo sua fase/gerenciamento de saída), transição de descanso e terminal. Durante marcha/combate, checkpoint periódico máximo de 5 s reais enquanto a aplicação avança, mais saída limpa. Capturar somente entre passos/commits, nunca no meio do golpe/crédito. Não introduzir IO por cada subpasso de 50 ms. Se um Advance atravessa vários encontros, cada vitória passa pela barreira durável antes de prosseguir; UI só publica depois. No estágio de equipamento, o sink em memória mantém tudo Volatile e o aviso de perda ao fechar permanece.

**S3 — Sem progresso offline.** SavedAtUtc serve a diagnóstico, não a delta. Ao carregar, instalar uma trava de apresentação `ResumeHold` até o jogador continuar, sem alterar Paused/Management do snapshot nem consumir tempo. Se havia Active ou Paused, continuam exigindo suas ações próprias. Não somar tempo de parede, curar, zerar recarga ou recreditar ao abrir. O port devolve Generation confirmada para só então a UI poder dizer “salvo”.

## Compatibilidade, gravação e recuperação

Arquivo futuro V19 em `user://saves/expedition/slot-0/`, resolvido pelo cliente e passado como caminho ao adaptador. **Não tocar `data/integration-state.json`** nem seu lock/recibos. O save legado permanece sem versão (v0) no leitor existente, com IDs GUID, Items, Scope, Bound, IntroDropGranted, Commands, Health, LastDamage e HitCount intactos. Sem importação automática, merge de carteiras, transferência ou alteração de scopes. Isso preserva compatibilidade sem precisar migrar um modelo para o outro.

Não existia save V19 versionado na base; não inventar migração de uma sessão em memória já encerrada nem alegar recuperar ganhos que não foram gravados. JSON legado encontrado no caminho V19 é WrongFormat e não é sobrescrito. Se uma task futura precisar versionar o legado, deverá usar migração explícita `aetherforge.integration` v0→v1 que envolva o payload inteiro preservando todos os campos/IDs/recibos e backup original; não é condição para o save separado da expedição.

Pipeline do novo leitor: identificar Format/SchemaVersion → verificar integridade → migrar em cópia com funções puras N→N+1, conservando original → validar referências, números e invariantes → reconstruir agregado sem concessões → publicar. SchemaVersion ausente não assume 1; versão futura, RulesetId desconhecido ou enum desconhecido recusam escrita e preservam bytes. Não inicializar automaticamente partida vazia sobre erro. Mudança em fórmula/catálogo exige versão de regras além da versão estrutural, para não recalcular silenciosamente equipamento/HP antigos por novas regras.

Protocolo proposto para Windows, a implementar/verificar na task de save:

1. Lease exclusivo por slot (se outro processo detém, SlotInUse, sem segundo escritor). Usar geração monotônica/CAS esperado para não substituir estado mais recente. Processos V19 não disputam lock do legado.
2. Serializar candidato completo, validar e gravar `generation-N.tmp` no mesmo diretório/volume; Flush(true), fechar, reler e verificar checksum/schema/invariantes. Preservar último checkpoint confirmado.
3. Promover para `generation-N.json` com nome novo que não sobrescreve outra geração. Validar arquivo promovido. Publicar commit no agregado somente após confirmação; manter as duas gerações confirmadas anteriores. O leitor escolhe a maior geração **completa e válida**; `.tmp` nunca é promovido pelo leitor. Não depender de promessa de atomicidade de rename-overwrite do Windows.
4. A cada commit bem confirmado, rotação só elimina gerações antigas fora das duas anteriores; nunca antes da promoção válida. Primary truncado/corrompido pode cair para anterior válida, informando geração/possível perda desde aquele checkpoint. Versão futura válida é incompatibilidade, não corrupção: não voltar e gravar por cima dela.
5. Se nenhuma geração é válida, preservar arquivos e bloquear continuação/escrita no slot; apresentar recuperação/nova sessão em outro slot com ação explícita. Não remover peça/recibo inconsistente para “consertar” automaticamente. O limite atual é rollback ao último checkpoint validado; não se promete recuperação de progresso ainda não gravado nem garantia contra falha de disco além dos arquivos sobreviventes.

Falha de gravação antes da promoção descarta o candidato e devolve CheckpointFailed, sem Applied/sucesso. Numa chamada Advance que já confirmou fronteiras anteriores, elas permanecem; parar no último estado confirmado em memória antes da fronteira falha, descartar o restante de delta e bloquear novos ticks por erro de armazenamento. Passos ordinários desde o último checkpoint podem estar apenas em memória; a falha não transforma esses passos em duráveis. Não reiniciar todo o Advance/rota nem repetir os créditos anteriores. O adapter deve distinguir falha certa de CommitIndeterminate (promoção pode ter ocorrido, mas confirmação se perdeu); neste último caso suspender comandos, reler gerações e reconciliar pelo identificador/revisão do commit antes de qualquer retry. Nunca fabricar um segundo sorteio/OperationId para resolver incerteza.

Recuperar uma geração anterior restaura **o conjunto completo daquela geração** — itens, saldos, IDs, RNG, créditos e operações — sem combinar campos da geração mais nova. Um recibo de equipamento confirmado em arquivo, seguido de queda antes da resposta à UI, será encontrado na retomada e retry retornará Replay. Um bloco de vitória persistido também já contém crédito e fase de saída, portanto Restore nunca chama CreditVictory outra vez. Rollback informado pode perder trabalho posterior à geração recuperada; não duplica o ledger dentro do estado restaurado. Exportação/sincronização entre máquinas e reconciliação de saves concorrentes ficam fora do recorte.

## Matriz QA: cenários, portas e estado observável

As fixtures devem criar estado pela factory de domínio/harness isolado, sem editar save manual em data/. Snapshot de igualdade inclui propriedade, HP/atributos, XP/saldos, RNG/contador/créditos, recibos/operações, revisões, fase e deadlines; contexto visual fica fora. Rejeição/no-op/replay comparam antes/depois do comando, não tempo normalmente passado em outro Advance.

| Caso de Design | Porta(s) | Oráculo/estado a conferir |
|---|---|---|
| E01 | Compare, Execute(Equip/Unequip), CalculateStats, Advance | DEF 10→12→10, dano 7→5, mesmo ID e remoção integral do bônus. |
| E02 | Compare, Execute | IDs A/B distintos com delta 0: Applied e troca; B já no mesmo slot com prévia atual: NoChange, sem cue. |
| E03 | CalculateStats, Compare, Advance | ATK 20→21, delta +1; DPS teórico usa intervalo existente, sem acúmulo. |
| E04 | Compare, Execute | Redução 22→20 permitida por confirmação explícita, sem autoequipar melhor. |
| E05 | Compare, Execute | RoleMismatch/SlotMismatch; alvo mantido; snapshot idêntico. |
| E06 | CalculateStats, Execute, Advance | Foco aplica ataque e infusão; Nv1 44/Nv2 47; retirada remove ambos, cura limitada. |
| E07 | Compare, Execute | 100/180→100/190→100/180; cheia 180→180/190 sem cura. |
| E08 | Compare, Execute | 202/202→190/190→190/202; repetição não aumenta HP. |
| E09 | Execute, Advance, RestartRoute | Caído 0→0, terminal persiste; só reinício explícito recupera ao máximo equipado. |
| E10 | Execute, ReadView | 21→21, antiga no índice do candidato, pendentes intactos; InventoryRevision muda e UI atualiza imediatamente. |
| E11 | Execute, ReadView | Slot vazio com 21: recuperar exatamente primeiro pendente; conjunto de IDs conservado. |
| E12 | Execute(Unequip) | Com 21: BagFull e snapshot igual; com 20/19: antiga primeiro, FIFO apenas nas vagas finais. |
| E13 | Controlador de intenção, Compare, Execute | Mudança de seleção invalida epoch; callback antigo StaleComparison, peça ainda inspecionada; cancelar não afeta Management. |
| E14 | Execute, ReadView | Revisão/slot/tentativa antiga recusa; envelope já aplicado retorna Replay antes de stale check; sem bônus, peça ou cue repetidos. |
| E15 | Advance, Compare, Execute | Crédito/nível antes de gerenciar; 100/180→118/198→118/208; caído não revive. |
| F01 | Compare, RequestManagement, Execute | Abrir menu não pausa; ManagementRequired até Active; distância/deadlines congelados após comando. |
| F02 | SetPaused, Request/CancelManagement, Execute, Advance | Combat pausado mantém InCombat; pedido não atende nem retoma; cancelamento não muda Paused. |
| F03 | RequestManagement, Advance(1000) | Primeira vitória: um crédito novo, Loot com 2 s e Active; restante descartado, sem cura/outro encontro. |
| F04 | CancelManagementRequest, Advance | Pedido cancelado não para; pedido após Advance observa fase atual e não reabre crédito. |
| F05 | Request/FinishManagement, Advance | Descanso com 0,75 s: após concluir, 0,74 não cura e 0,01 encerra uma vez; máximos atuais. |
| F06 | Advance, Execute | Deltas particionados equivalentes sem intervenção; fase revalidada no clique; sem retroação da prévia. |
| F07 | CaptureCheckpoint, Execute, Advance | Elapsed/deadlines/recargas/escudo preservados na trava; negativos/NaN/infinito sempre rejeitados; delta zero não atende pedido. |
| F08 | Advance, Retreat, Execute | Chefe Complete sem descanso; derrota/recuo limpam pedido; terminais permitem equipar sem crédito/HP/tempo extra. |
| F09 | RestartRoute, ResetSession, Execute | Nova tentativa conserva itens/IDs/RNG/ganhos e loadout; nova sessão zera; callback antigo inválido. |
| F10 | SetPaused, Request/FinishManagement, controlador | Pausa prévia permanece; fechar janela/Voltar não libera trava; HUD oferece concluir/reabrir. |

Checks técnicos adicionais para as etapas correspondentes:

| ID | Porta / evidência exigida |
|---|---|
| T01 · partição | Após toda sequência de operações/concessões: cada ID em uma localização; contar recibo DropId não aumenta propriedade. Enum/ID inválido e falha injetada preservam snapshot. |
| T02 · operações | Mesmo OperationId/payload aplicado em retry imediato, após RestartRoute e após futura carga retorna Replay; payload diferente dá OperationConflict; SessionId diferente nunca acessa recibos. |
| T03 · UI | Substituição com contagem igual atualiza bolsa/slot/Status/comparação; callback após sair e voltar ao mesmo alvo é inválido; Applied delta zero tem um cue, replay/redraw nenhum. |
| T04 · restauração | Futuro save/load em March, Combat (terceiro golpe especial/escudo/infusão), Loot com resto parcial, Active, Requested e cada terminal; mesmo próximo evento e ledger ao continuar, sem cue histórico. |
| T05 · recompensa e RNG | Snapshot antes/depois de vitória e crash após gravação/antes da resposta: um crédito por tentativa/etapa, mesmo próximo drop/ID; contador além de 999999 e overflow checked. |
| T06 · recuperação | Futuro adapter: falhas injetadas antes/durante/depois de flush/promoção/confirmação; lock concorrente; truncamento/hash/refs inválidos; fallback de geração inteira e erro de versão futura sem sobrescrita. |
| T07 · compatibilidade | Corpus legado com mesmo ID, Scope/Bound, saúde e Commands antes/depois; repetir comando legado retorna recibo anterior; trocar V19 não altera arquivo/lock legado; nenhum local-* vira GUID. |
| T08 · regressão | Suites atuais de crescimento/rota/integração e V19: sem rebalancear XP/cura/escudo/energia/encontros; loadout vazio mantém os resultados anteriores. |

Na implementação de domínio, estender os checks reais em `tests/Aetherforge.Checks/` com cenários E/F e invariantes; rodar `scripts/Test.ps1`. Na integração de UI/cliente, rodar também `scripts/Build.ps1`, `scripts/Test-Integration.ps1`, `scripts/Test-Expedition.ps1` e `scripts/Test-UiV19.ps1`, adaptando o harness às novas ações sem enfraquecer os oráculos existentes. Interação gráfica/física, DPI/áudio e medição de desempenho precisam de ensaios próprios; passes headless não os certificam. T04–T07 de persistência pertencem à task de save, não bloqueiam a especificação documental.

## Sequência de integração e limites desta entrega

1. Director revisa este documento junto ao contrato incorporado e libera dependentes pelo fluxo existente. Não há novo despacho de agentes nesta task.
2. Domínio implementa cálculo/loadout/comandos/transação/gerenciamento e snapshots de dados necessários, com sink volátil; UI/UX usa as mesmas portas para comparação e controle explícito, preservando componentes atuais. Comparação não pode antecipar campos diferentes da API de domínio.
3. Integração atualiza Herói/Status/bolsa e feedback, depois revisão técnica/QA executam E/F e T01–T03/T08. Aviso de ganhos da sessão continua até save efetivo.
4. Task posterior de save implementa adapter/schema/restauração/recuperação e T04–T07, passando a confirmar durabilidade. Não ativar offline nem integrar inventário remoto como efeito dessa etapa.

Nenhuma decisão técnica aqui exige novo serviço, custo, rede ou acesso. Escalar ao Director/Arthur apenas se surgir necessidade de importar itens/carteiras, autoridade online, alterar regras aceitas ou ampliar visão/infraestrutura; não bloquear as escolhas locais já justificadas. Capacidade humana da bolsa sem descarte, utilidade de peças inferiores e retenção continuam decisões/validações posteriores do roadmap, não justificativa para introduzir desmontagem/economia nesta fatia.

Validação desta entrega é documental: correspondência dos 25 casos às portas/estado, conferência de fontes/links, identidade do contrato anexado, aplicação do patch sem conflito e `git diff --check`. Conforme AGENTS.md, não há recompilação para Markdown. Builds/testes citados como futuros ou históricos não foram executados como prova desta arquitetura. A entrega não implementa domínio, UI, save, migração, asset ou integração de rede.
