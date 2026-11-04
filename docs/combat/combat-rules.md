# Regras de combate e continuidade — C1

GAMELAB-62 · Game Designer · 02/11/2026 · C1.1 para revisão do Game Director. Especificação de comportamento para C2/C3, sem código de gameplay. **Decisão** significa requisito desta fatia após aceite; **Inicial** significa valor escolhido para o primeiro ensaio e sujeito à calibração. Nenhum valor novo é apresentado como medido em playtest.

Mapa normativo: [combat-blockout.md](combat-blockout.md), `fornalha-c1`, revisão 1. A equipe explora automaticamente, ocupa posições por função e vence pela preparação. Intervenção é opcional: pausa, retirada, preparação de equipamento nas janelas existentes, escudo manual e escolha de continuar/parar. Sem controle manual de marcha, críticos, esquiva, provocação ativa, interrupção de cura, novos recursos ou árvores de habilidades.

## 1. Base lida e rastreabilidade

Checkout Multica `agent/game-designer/f2649555bbf0`; HEAD e `origin/main` lidos em `aca2d622ae590e0044a8e744f392fd6ed787161d`. `origin/agent/gameplay-engineer/gamelab-25-visual-identity` aponta para `1fae8498af3ac7518fca4d11839366652e93250e`; `git merge-base --is-ancestor aca2d62 1fae849` retornou 0. Não houve merge/cherry-pick. Consulta Multica de PR #5: aberto, `merged_at=null`, snapshot de CI marcado antigo. C2 decide a base integrável; não interpretar esta documentação sobre main como dispensa da cadeia de equipamento.

| Entrada/evidência lida | O que é atual | O que C1 muda/fecha |
|---|---|---|
| `AGENTS.md`; `src/Aetherforge.Client/project.godot`; `Aetherforge.Client.csproj`; `global.json` | Godot .NET 4.7.2, C#, Compatibility, cliente net8.0, domínio netstandard2.1, SDK 10.0.401 | Preservar stack; sem dependências Godot no domínio |
| `src/Aetherforge.Domain/Expedition.cs`, `TickCombat`, `EndRest`, `RestartRoute` | Cinco composições, dano imediato, alvo pelo primeiro vivo, cura sem ocupar ataque, escudo manual, descanso 2 s; terminais param | Ações temporizadas, posições/visão, cura exclusiva, escudo automático, continuidade versionada |
| `src/Aetherforge.Domain/Party.cs`; `ExpeditionRewards.cs` | Atributos iniciais, escalas, crédito por tentativa/etapa, carteira/bolsa/pendentes e RNG de sessão | Preservar fórmulas e recompensas; novas tentativas não resetam sessão |
| `src/Aetherforge.Client/ExpeditionStage.cs:172`, `:175`, `:366` | Raízes/câmera derivadas de Distance/EncounterAt; apresentação procedural 3D | Trocar por posição/orientação de cada entidade; Distance vira apenas projeção legada se necessário |
| `tests/Aetherforge.Checks/ExpeditionChecks.cs` | Oráculos de formação parada, ataque imediato, cura/escudo, terminais, determinismo | Preservar intenção e regressões; expectativas alteradas precisam de versão/novos oráculos |
| Cadeia `1fae849`: `docs/implementation/equipment-contract.md`, `equipment-architecture.md` | Identidades/transações, gerenciamento March/Loot, terminal permite equipar, HP da troca não cura | Não reabrir inventário; estender apenas continuidade e estado espacial |
| GAMELAB-61: plano v2, anexo `01a19e5e-c4c8-7461-aaeb-77c47f0e6b96` e complemento espacial `01a19e5e-c5af-7754-8ad6-bde2d447b038` | Direção aprovada por Arthur no fio `01a19e8f-742e-7dcc-a9d6-4ba0cad9fb4d` | Fonte de escopo desta C1; sobrepõe histórico de backlog/formação linear |
| GAMELAB-63: contrato de impacto v1, anexo `01a19e58-8fbd-7c39-88f2-0c482dfc3949` | Proposta de scheduler/eventos/impacto, ainda sem implementação | Confirmar políticas abaixo; substituições espaciais já determinadas pelo v2 |
| GAMELAB-29 e GAMELAB-30, ambas backlog na consulta | Cobertura de evento retido/cura/composição e fallback de número ainda são gates | C2 registra resolução prévia ou incorporação coordenada; C1 não os executa nem os dispensa |

As skills `router`, `godot-csharp`, `game-ai` e `level-design` orientaram a separação entre decisão, navegação e apresentação e o dimensionamento do blockout. Não se consultou API nova nem se executou motor nesta entrega.

## 2. Estados, decisão e exploração

O estado de rota e o estado primário de cada ator são eixos distintos. Nomes abaixo são semânticos, não APIs C# existentes. Um FSM pequeno basta; não requer framework de behavior tree.

| Estado de rota | Entrada/atividade | Saída autorizada |
|---|---|---|
| Explorar (`March`) | Objetivo local R; cada vivo obtém destino alcançável distinto. Sem ataque/cura de combate nem aggro remoto | Todos os vivos reunidos no R armado → ativar encontro; gerenciamento/pausa congelam; retirada termina |
| Combater (`Combat`) | Um encontro ativo; escolher alvo e posição; até três aliados e dois inimigos | Último inimigo morto → vitória; todos aliados mortos → derrota; falha de navegação persistente → retenção explícita |
| Descansar (`Loot`) | Crédito já confirmado, 2 s; até 0.70 s de acabamento visual dentro desse intervalo | Ao fim, healer vivo trata vivos em 20% do máximo vigente, uma vez; iniciar caminho ao próximo R |
| Resultado (`Complete/Defeat/Retreated`) | Snapshot terminal imutável no instante T; cancela ações. Queda termina visualmente em até 0.70 s | Complete/Defeat podem ter continuação programada de 4 s; Retreated aguarda comando. Sem novo combate no snapshot terminal |
| Recuperar/retornar (continuidade) | Contagem autoritativa de 4 s, pausável, apresentada com motivo e destino. Sem ataque, crédito, cura intermediária ou recarga de combate | Ao fim, criar nova tentativa atomicamente e restaurar os três ao máximo equipado no R escolhido; ou permanecer parado se `Stop`/gerenciamento pendente determinar |
| Reter (`Hold`) | Falha de caminho ou piso sem etapa segura; nenhum tempo de combate/loot é consumido | Ação explícita corrigir/reiniciar/retirar; não tentar infinitamente em silêncio |

`Paused`, gerenciamento e Hold são travas explícitas; inspeção nunca pausa. Caminho/ação/relógios ficam congelados em pausa e gerenciamento ativo. A apresentação congela inclusive números/quedas. O tempo humano de parada não vira catch-up ao concluir. Uma falha de commit não publica ação nem feedback candidato.

Prioridades de decisão por ator: morte/retirada/geração inválida → manter ação comprometida até seu término válido → necessidade de cura do suporte → posição válida para ação → ação pronta → acompanhar objetivo/reunir. Escudo é status paralelo e pode ser ativado sem cortar o golpe do tanque. Reações visuais não deslocam a raiz lógica nem interrompem a ação.

Decisão periódica inicial a cada 0.10 s lógico, mais fronteiras exatas de ação/recarga/morte. Nunca por frame de render. C2 fixa integração de movimento e precisão/reprodução; consultas espaciais em ImpactAt ocorrem depois de integrar até aquele instante. Não arredondar impacto à física de 20 Hz. Guardar alvo/destino/caminho entre decisões; replanejar se destino mudou ≥0.50 m, caminho invalidou ou recuperação de bloqueio pediu. Evitar consulta completa de caminho a cada frame.

Na exploração, os destinos não são offsets de uma distância única do grupo: Guarda busca o centro R pelo caminho mais curto; DPS e suporte escolhem candidatos em anéis de 1 e 1.5 m ao redor de R, a cada 30° a partir de +X, além da própria posição se já satisfizer a reunião. Excluir sólidos, reservas/ocupação, pontos fora do disco de reunião e rotas que violem coesão. DPS prefere visão livre do próximo portal/objetivo; suporte prefere maior cobertura dos vivos em alcance/visão de cura. Depois, menor custo de caminho e índice do candidato. Reservar destinos distintos na ordem Guarda→DPS→suporte, conservando destino válido por 0.60 s; não reservar a posição física atual de outro vivo como destino. Durante o trajeto seguir os portais da escolha de contorno, usando largura disponível; só N impõe ordem longitudinal. Aguardar/reunir por coesão não substitui navegação individual. Não há ataque/cura até ativar o encontro.

## 3. Métricas, posição e validade espacial

Todas as distâncias de ação abaixo são entre **centros no plano XZ**, não entre bordas ou âncoras de arma. Igualdade com alcance é válida; sem tolerância de render. Caminho e coesão usam comprimento navegável; alcance usa distância euclidiana. C2 escolhe representação numérica e regra de fronteira determinística, sem acrescentar bônus secreto de alcance.

| Parâmetro | Inicial | Regra de uso |
|---|---:|---|
| Raio aliado/inimigo comum / Capataz | 0.35 / 0.55 m | Discos sem sobreposição; caídos perdem bloqueio de navegação, permanecem visuais |
| Folga à parede | 0.10 m | Navegador considera raio+folga; geometria compartilhada com visão |
| Separação desejada / mínima, comuns | 0.90 / 0.80 m | Mínima geral = soma dos raios +0.10; não empurrar raiz através de parede |
| Velocidade de todos os aliados | 2.40 m/s | Igualdade de velocidade não implica distância/fila fixa |
| rat / scavenger / sentry / drone / watcher / foreman | 2.6 / 2.2 / 1.8 / 2.6 / 2.2 / 1.6 m/s | Sem corrida/teleporte de catch-up |
| Giro em marcha / preparação | 360 / 540 graus/s | Antes de começar ação, erro angular ≤15°; na preparação pode girar sem transladar |
| Chegada ao destino | ≤0.20 m | Parar sem oscilar; não usar tolerância para ampliar alcance de ação |
| Compromisso com destino válido | 0.60 s | Só trocar antes por morte, perda de caminho/alcance/visão, cura urgente ou ameaça relevante |
| Máxima separação de vivos | 8 m navegáveis | Segurar líder/objetivo, reunir por caminho; não puxar aliados com força nem montar fila permanente |
| Leash inimigo | 6 m navegáveis do spawn, limitado à área do mapa | Não perseguir outra área/encontro |
| Perda de alvo mantida | 1.0 s sem posição de ação alcançável | Depois escolher substituto; ação já iniciada mantém TargetId |

**Visão:** segmento centro→centro no plano contra sólidos do mapa; visão é bloqueada por parede/máquina e pelo exterior do piso. Aliados/inimigos não ocluem tiro/cura nesta primeira fatia, mas continuam volumes de navegação. Não há interceptação amigável nem dano colateral. Contato bloqueado informa o primeiro ponto do sólido; arma/âncora visual não autoriza atravessar parede. Cura também exige visão, explicitamente, para que a máquina tenha o mesmo significado para as duas equipes.

| Função/perfil | Posição desejada e início | Validade no impacto | Reposicionamento |
|---|---|---|---|
| Tanque | Aproximar foco; buscar 1.25 m do alvo comum e 1.45 m do Capataz; só iniciar ≤1.60 m, visão livre, frente alinhada | Alvo vivo, ≤1.60 m e visão livre | Ao perder alcance, próxima tentativa aproxima até a distância desejada; não esticar golpe |
| DPS | Buscar ponto a 3.5–4.5 m, iniciar ≤5.00 m com visão | Mesmo TargetId vivo, ≤5.00 m e visão | Se distância <2.5 m buscar afastamento válido; <2.5 não proíbe tiro caso não haja recuo seguro. Recuar não cancela preparação |
| Suporte, ataque | Ponto seguro a 3–4 m do foco, iniciar ≤4.50 m com visão e sem cura prioritária | Vivo, ≤4.50 m e visão | Não perseguir dano além de perder cobertura de cura |
| Suporte, cura | Vivo com menor %HP entre todos os vivos feridos; aproximar para ≤6.00 m com visão | Vivo, ≤6.00 m e visão | Mirar posto que cubra todos vivos quando possível; se impossível, cobrir o aliado escolhido e ficar no lado seguro da ameaça |
| Inimigo corpo a corpo / pesado | Aproximar a 1.25 /1.45 m, iniciar ≤1.60 m, visão | Vivo, ≤1.60 m, visão; pesado não é cone/AoE | Mesmos limites de preparação e leash |
| drone e watcher, tiro | Buscar 3–4 m, iniciar ≤5.00 m, visão | Vivo, ≤5.00 m, visão | Não kitear perpetuamente: posição válida fica comprometida 0.60 s; limitado à área/leash |

Um destino ofensivo é um ponto alcançável, livre de sólido/ocupação reservada, no anel útil e com visão. Para tornar a escolha repetível, usar candidatos em anéis: tanque distância desejada; DPS 4, 3.5, 4.5 m; suporte ataque 3.5, 3, 4 m; cura 4, 5, 3, 6 m em torno do paciente. Ângulos a cada 30°, começando no vetor alvo→ator atual (vetor nulo usa +X). Filtrar geometria, ocupação, coesão e perfil; preferir destino atual ainda válido dentro do compromisso; depois menor custo de caminho, menor erro para distância preferida, índice do anel/ângulo. C2 pode usar solução contínua equivalente com desempate documentado, sem mudar as prioridades de gameplay.

Para suporte, antes do custo de caminho, maximizar quantidade de vivos em alcance/visão de cura; desempate favorece manter tanque alcançável e distância ≥2.5 m dos hostis, quando houver ponto. Se nenhum candidato mantém todos, priorizar paciente, não atravessar hostis por conveniência e emitir `PartialCoverage` com IDs fora de alcance. Se tanque vivo, posição segura prefere o semiplano atrás dele em relação ao foco; não é slot fixo. Sem tanque, DPS/suporte mantêm distância útil e defendem-se pelo mesmo limite de área. Ausência de posição perfeita não desliga cura alcançável nem concede invulnerabilidade.

## 4. Aquisição, foco e perda de alvo

Só entidades do encontro ativo são elegíveis. Tanque escolhe o primeiro spawn vivo e alcançável; preserva foco até morte, perda persistente de posição alcançável ou reset/leash. Não há tabela de ameaça oculta. O inimigo comum prefere tanque vivo; na ausência, primeiro vivo Guarda→DPS→suporte. Drone direciona cada terceiro **ataque aceito** ao DPS vivo; watcher cada terceiro ao suporte vivo. Contar no início aceito, inclusive se o impacto falhar; rejeições sem início não contam. Fora desse terceiro, usar preferência normal. Preparar o terceiro publica o alvo para leitura. Sem taunt/interrupção real, não mostrar cue dessas mecânicas.

Prioridade do DPS nesta fatia é fixa: inimigo com ação anunciada cujo TargetId é o suporte → foco vivo do tanque → primeiro spawn vivo alcançável. A ameaça muda só a próxima ação ainda não iniciada; alvo de preparação não é trocado. Suporte ao atacar segue foco do tanque, depois primeiro spawn alcançável. Quando tanque cai, DPS mantém alvo atual válido, e suporte prioriza sustentação dos sobreviventes.

Alvo sem visão/alcance imediato pode ser mantido por 1 s enquanto busca posto válido. Se nenhum caminho até qualquer posto válido existir nesse período, tentar outro alvo vivo da mesma prioridade. Se todos falharem, entrar na recuperação de bloqueio. O leash nunca autoriza perseguir outra etapa. Cura escolhe menor percentual global primeiro; empate Guarda→DPS→suporte. Se esse paciente não tem caminho de cura por 1 s, atender o próximo necessitado alcançável e sinalizar o inacessível. Não desperdiçar toda a equipe tentando curar através de uma parede.

Ao sair do leash ou ficar 3 s sem qualquer alvo alcançável, inimigo cancela preparação, retorna ao spawn por caminho e reseta o encontro inteiro se não houver combate válido restante. Reset de leash não conta vitória, não paga nem cura aliados, restaura inimigos e cria nova geração após reunir os vivos. Preserva recargas/HP dos aliados. Dois resets consecutivos sem vitória deixam Hold com `LeashLoop`; nova tentativa exige comando. Não usar reset como modo infinito de chip damage ou de tratamento gratuito.

## 5. Ações, tempos e recargas

Uma ação primária por ator: `Ready→Windup→[Released]→Resolved→Recovery→Ready`. Origem, TargetId e potência são comprometidos em StartedAt. Defesa/status e espaço são consultados no impacto. Durante Windup e Recovery, raiz plantada; girar é permitido. Entre RecoveryEnd e próximo início, pode caminhar. Reação/escudo são camadas visuais, sem impedir nem duplicar ação. Morte tem prioridade visual.

Tempos são offsets em segundos a partir do **início aceito**. Intervalo é início→próximo início mínimo; não é duração do clip. Intervalo/cooldown não cria fila de ataques perdidos: ao ficar bloqueado, há apenas uma próxima ação elegível, sem rajada de catch-up.

| Perfil | Preparação / liberação | Impacto | RecoveryEnd | Próximo início mínimo | Alcance |
|---|---:|---:|---:|---:|---:|
| Guarda básico | 0.16 + golpe até contato; sem Released | 0.24 | 0.54 | 1.60 | 1.60 m |
| Pistoleiro | Released=0.12 | 0.12 | 0.34 | 1.25 | 5.00 m |
| Alquimista ataque | Released=0.20 | 0.20 | 0.44 | 2.40 | 4.50 m |
| Infusão Etérea | 0.22 + gesto; sem voo/liberação física | 0.36 | 0.60 | Cura em 4.00 | 6.00 m |
| rat básico | 0.14 + golpe | 0.24 | 0.49 | 1.80 | 1.60 m |
| scavenger básico | 0.20 + golpe | 0.32 | 0.62 | 2.20 | 1.60 m |
| sentry básico | 0.25 + golpe | 0.40 | 0.75 | 2.40 | 1.60 m |
| drone tiro | Released=0.18 | 0.18 | 0.40 | 1.70 | 5.00 m |
| watcher tiro | Released=0.24 | 0.24 | 0.50 | 2.30 | 5.00 m |
| foreman básico | 0.22 + golpe | 0.38 | 0.73 | 2.50 | 1.60 m |
| foreman pesado | Preparação anunciada 0.90, sem Released | 0.90 | 1.40 | Básico em 2.50; pesado a cada ≥12.00 | 1.60 m |

**Decisão de tiro inicial:** todas as ações de tiro desta tabela são direcionadas instantâneas após preparação, `ReleasedAt=ImpactAt`. Muzzle/traçador/contato mostram o segmento válido no mesmo frame, sem bala viajando depois do dano. A cura é pulso de contato no impacto, sem projétil com voo; o gesto de preparação não altera HP. Isso evita deixar a cura atravessar uma parede durante uma animação de viagem. Perfil balístico futuro exige outro contrato de primeiro contato, não está aprovado aqui.

Ao ativar encontro, primeiro início aliado é não antes de `CombatAt+0.4/+0.6/+0.8` para Guarda/DPS/suporte; hostis `+1.0/+1.35` por spawn. Alcance, alinhamento e caminho podem adiar. Recargas de cura/escudo não zeram ao mudar encontro; atravessam marcha e descanso, congelam em pausa/gerenciamento. Uma nova tentativa de recuperação/repetição restaura como RestartRoute; nenhum uso ao equipar.

Ataque básico consome sua cadência no início. Cura consome sua recarga no início; não avança nem zera o relógio do básico. `NextBasicStart=max(lastBasicStart+interval, primarySlotFree)`; `NextHealStart=max(lastHealStart+4, primarySlotFree)`, mais condições. Se ambos prontos e algum vivo tem HP≤72%, iniciar cura; se cura em recarga, suporte pode atacar. Se cura pronta mas paciente necessita aproximação/visão, buscar posto de cura por até 1 s antes de considerar paciente alternativo; não iniciar ataque enquanto esse esforço de cura prioritária está ativo. Ação já iniciada termina antes de reavaliar. Não cortar um ataque em preparação para curar.

Fórmulas preservadas: dano básico aliado `max(1,AttackSnapshot−Armor(kind))`; armadura sentry 4, foreman 3, scavenger 1, demais 0. Hostil `max(1,EnemyAttack−DefenseAtImpact)`; poderes rat 17, scavenger 22, sentry 25, drone 19, watcher 24, foreman 31. Cura `ScaleBase(42,8)+bônus do foco` da cadeia de equipamento, limitada à vida faltante. Pesado novo usa `max(1,31×1.8−DefenseAtImpact)` antes do escudo, sem arredondamento intermediário novo. Todos são valores iniciais; preservar crescimento/equipamento contratado. Não aplicar mínimo 1 novamente depois do multiplicador do escudo.

| Falha/cancelamento | Resultado e custo |
|---|---|
| Comando/início recusado | Nenhuma ação/recarga consumida; devolver motivo |
| Alvo sai de alcance/visão durante preparação | Não cancelar preventivamente nem trocar alvo; em ImpactAt integrar posições, resolver `OutOfRange`/`Occluded`, dano/cura 0; recuperar até prazo original, sem reembolso |
| Autor morre, retirada ou geração muda antes do impacto | `ActionCancelled` uma vez, sem resolução de efeito; morte/terminal aposenta slot e cancela prazos; sem reembolso |
| Alvo morre antes do impacto | Cancelar por `TargetDead` uma vez, sem redirecionar; autor vivo preserva RecoveryEnd/NextEligible originais antes de novo início |
| Cura encontra alvo cheio | Resolver `NoMissingHealth`, EffectiveAmount 0; recarga e recuperação permanecem; nenhum número positivo |
| Alvo vivo e válido | Uma resolução, dano/cura limitado a HP disponível/faltante; morte decorrente do mesmo delta |
| Reação visual ou escudo ativado | Não cancela primária; não dá segundo ataque |

Motivos de disponibilidade expostos ao read model: `Paused`, `ManagementActive`, `NotInCombat`, `ActorDead`, `NoLivingTarget`, `PrimaryBusy(endsAt)`, `Cooldown(readyAt)`, `OutOfRange(distance,max)`, `Occluded(obstacleId)`, `Turning`, `NoPath`, `WaitingPassage`, `Regrouping`, `NoHealingNeeded`, `ReservedForHeavy(impactAt)` e `ContinuationHeld`. Mostrar a causa prioritária atual e tempo restante quando aplicável; registro pode manter causas secundárias. Esperar posição não é consumir cooldown.

## 6. Escudo e Capataz: automático suficiente

Escudo protege só o Guarda: duração 4 s, recarga 12 s desde ativação, multiplicador de dano 0.35 (65% mitigado). Não é invulnerabilidade, cura ou escudo do grupo. Camada instantânea sem slot primário. Auto e manual usam a mesma operação `TryActivateShield`; uma aceitação marca imediatamente readyAt. Segundo pedido no mesmo timestamp retorna indisponível/replay e não reinicia duração/recarga. Manual pode antecipar o uso; UI mostra que isso pode deixar o próximo pesado sem proteção.

Política automática fixa, ligada nesta fatia:

1. Há pesado anunciado contra Guarda vivo, escudo livre e ImpactAt a ≤0.60 s: ativar no limite `ImpactAt−0.60`, ou imediatamente se o aviso chegou dentro dessa janela. Se escudo já ativo cobre aquele impacto, não fazer outra ativação.
2. Fora dessa janela, emergência: Guarda HP≤35% ou soma de dano **já agendado** contra ele nos próximos 0.60 s é letal sem escudo, e escudo pronto. Se existe pesado conhecido cujo impacto seria antes de `agora+12.60 s`, reservar para ele; só furar reserva se o dano letal confirmado chega antes da próxima cura agendada. Não prever cura ainda não iniciada nem ataques não anunciados como se fossem garantidos.
3. Sem pesado conhecido/previsto nesse intervalo, emergência usa escudo. Sem emergência, conservar. Reserva não impede comando manual elegível.

Capataz: primeiro pesado fica devido em CombatAt+6 s; iniciar no próximo início básico elegível (slot livre, cadência 2.5 s cumprida, alcance/visão), substituindo esse básico. Próximo pesado nunca antes de 12 s após **início aceito** do anterior; vencimento atrasado não acumula cópias. Básicos continuam nos outros espaços da cadência 2.5 s. Anúncio fixa TargetId e ImpactAt, com postura/ícone/linha ao alvo; não exige esquiva. A mira segue giro limitado; dano dirigido exige alcance/visão no contato. Se Guarda morto no início, usa prioridade normal de sobreviventes; escudo não protege substituto e a equipe pode perder. Para reserva automática, o próximo prazo de pesado é conhecido desde a ativação; usar seu impacto mais cedo possível (`heavyDue+0.90`) até existir anúncio com instante exato.

Conferência analítica sem atraso espacial: básicos iniciam 1/3.5 s, pesado 6, depois básicos 8.5/11/13.5/16 e pesado 18.5. Impactos pesados 6.9/19.4; auto ativa 6.3/18.8; cobertura `[6.3,10.3)` e `[18.8,22.8)`, recarga pronta 18.3. Mesmo no limite de dois inícios pesados exatamente 12 s apartados, a segunda ativação coincide com a recarga pronta. Logo o mínimo 12 s permite proteção em todos os pesados de referência. Se o início seguinte for adiado por recuperação/posição, não antecipa a ameaça. Emergência/manual podem gastar a proteção antes: isso é estado de risco explícito, não motivo para reduzir recarga secretamente. A equipe de referência deve passar sem esse gasto conflitante; C5 precisa comprovar.

Cobertura de escudo é intervalo semiaberto: `ActivatedAt≤ImpactAt<ExpiresAt`. Exatamente em ExpiresAt não protege. Por exemplo, escudo em 10 expira 14; impacto 13.999999 recebe mitigação, 14.000000 não. Comando novo aceito em 14 com escudo ainda em recarga até 22 é recusado, não estende cobertura. Se readyAt e ImpactAt forem ambos 22, comando aceito antes dos impactos de 22 protege. Sem comando/auto aceito antes, não há defesa retroativa.

## 7. Ordem autoritativa e apresentação

Confirmar a ordenação v1 com extensão espacial: em T, comandos aceitos por sequência do driver (sem retrodatação) → travas/cancelamentos → integrar movimento até T e reconciliar alcance/ocupação → expirar status e aplicar auto elegível → liberações → impactos → fins de recuperação → novas decisões/inícios. Expiração usa a condição semiaberta, independentemente da ordem interna de remoção. Ativação automática de escudo em T ocorre antes dos impactos de T quando elegível. Se pausa/retirada aceitas impedem avanço, não processar impactos desse avanço.

Dentro de cada grupo temporal: Guarda, DPS, suporte, inimigos por ordem estável de spawn, depois ActionId. Após cada impacto atualizar HP/morte e cancelar dependentes antes do próximo. Se último inimigo morreu, encerrar encontro e não permitir cura/ataque adicional em T, preservando a intenção atual de vitória imediata. Não há dano simultâneo verdadeiro nem morte mútua fabricada. Um inimigo morto pelo DPS no empate não termina seu golpe. Um suporte morto antes do próprio contato não cura.

Publicar deltas confirmados de HP antes de recompensa/nível/fase. Uma ação = no máximo um resultado terminal (cancelamento ou resolução). O ID de entidade inclui geração de encontro; nunca atingir novo ocupante do slot. Guarda potência do início, defesa/status do impacto e posições/ponto de contato daquele T. Callbacks de animação, sinal de partícula e colisão visual jamais causam dano/cura/loot.

Contato, HP principal/texto, reação, número efetivo, VFX e pedido de áudio vêm do mesmo lote. Barra residual é decoração; não atrasar o único HP. Em resolução 0, registrar motivo sem dano/curativo fictício; tiro bloqueado pode mostrar contato no obstáculo. Cues antigos não são reproduzidos em rajada ao retornar de minimização/bind. Pausa/restore precisam conservar ação/posição/cursor. Gates 29/30 continuam donos da validação de identidade/feedback e visibilidade efetiva.

## 8. Bloqueio, perseguição e coesão

Progresso = redução ≥0.10 m no caminho restante em 1 s enquanto deveria andar. Espera autorizada por reserva/ação/pausa não conta como stuck. Ao detectar stuck: liberar só a reserva obsoleta, recalcular caminho uma vez e conceder prioridade a quem já atravessa. Se continuar por mais 1 s, testar alternativa de contorno ou posto de ação; perdedor de disputa recua por caminho até último ponto livre, no máximo 1.5 m. Não diminuir raio nem desativar colisão para passar.

Persistindo por 3 s após essa tentativa, reunir os demais no último R seguro alcançável. Nenhum progresso/rota após total 8 s → Hold `NavigationBlocked`, com ator, destino e obstáculo/reserva. Congelar combate/continuidade nesse Hold técnico para não punir a equipe com dano enquanto a navegação está insolúvel; manter HP, recargas, recibos e ações para diagnóstico. Não conceder vitória nem repetir automaticamente. Retomar tenta uma vez com estado/caminho revalidado; reiniciar é escolha explícita com nova tentativa. Geometria corrigida implica nova MapRevision, nunca teleporte de um vivo.

Em combate, nenhum integrante persegue além do polígono/leash ou viola teto de coesão para manter um foco. DPS/suporte escolhem posto válido mais próximo; tanque espera/reúne se não puder aproximar sem abandonar os demais. Se o alvo móvel torna impossível todo contato por 15 s contínuos sem dano/cura efetivos, aplicar a mesma recuperação e Hold, distinguindo `NoCombatProgress` de derrota. Um encontro geometricamente defeituoso não deve virar farm de derrotas. C2 deve reproduzir decisões de espera/reserva e condições de timeout independentemente do FPS.

## 9. Vitória, repetição, derrota e ganhos

Uma vitória exige todos os spawns do encontro mortos por resoluções válidas. Não depende de chegar em X5, distância acumulada, tempo, caminho escolhido ou sair do quadro. Recompensa permanece por vitória lógica, com tabelas atuais: XP `[12,16,20,24,50]`, moedas `[8,10,12,14,35]`, sucata `[1,1,2,2,5]`, componente 1 no chefe. Mesmas regras de drop/raridade/bolsa 21/pendentes; sem bônus por contorno. XP inclui caídos e nível mantém o efeito próprio já contratado. Derrota, retirada e reset de leash não pagam encontro parcial.

**Identidade:** preservar SessionId, MemberIds, ItemIds, OperationIds/recibos já aplicados, ledger, RNG, XP, níveis, moedas, bolsa, equipados e pendentes. Cada nova tentativa incrementa AttemptId; cada instância de encontro tem EncounterId novo. Crédito protege `(SessionId,AttemptId,logicalStageId)` e o recibo mantém sequência global de sessão. Reprocessar evento/commit não paga de novo; uma vitória real na nova tentativa pode pagar de novo. Não reiniciar RNG nem contador de itens/recibos. `ResetSession` nunca é usado pela continuidade automática.

| Evento | Decisão de continuidade |
|---|---|
| Vitória E1–E4 em modo avançar | Crédito uma vez; Loot 2 s com tratamento de vivos se healer vivo; marcha ao R seguinte. Caídos continuam caídos até nova tentativa |
| Vitória E5 | Complete e crédito; não aplicar descanso/tratamento extra. Padrão repetir mesma rota: contagem 4 s, depois nova tentativa em R0 com HP máximo e temporizadores de reinício |
| “Parar ao final” | Flag explícita idempotente; afeta término da rota, não a próxima vitória comum. Complete fica estável e sem contagem. Desmarcar só programa continuação por comando explícito, sem retroagir |
| Gerenciamento pedido em vitória comum | Atender em Loot com 2 s inteiros restantes, antes de qualquer contagem; concluir conserva pausa prévia e retoma resto |
| Gerenciamento pedido na vitória final | Complete permite equipamento; segurar repetição até “Continuar rota” explícito. Não converter pedido em troca automática |
| Derrota em etapa j (todos mortos) | Guardar falha de j; cancelar ações/pedido. Recuperação 4 s; nova tentativa começa em `max(0,j−1)` no R correspondente, não em etapa zero inexistente |
| Primeira falha em j, modo avançar | Após recuperar, vencer a anterior k e tentar j novamente; vitória em k não apaga falha de j |
| Segunda falha em j sem vitória em j | Entrar em cultivar k=`max(0,j−1)`, sem avançar para j automaticamente. Informar “Cultivando etapa k após duas derrotas em j” |
| Vitória cultivando k | Crédito normal; contagem 4 s e nova tentativa da mesma k, com HP restaurado. Não marcar rota completa nem pagar etapas não combatidas |
| Derrota cultivando k | Primeira: recuperar e repetir k; segunda sem vitória em k: baixar cultivo a k−1. Se k=0, Hold após segunda derrota; exige melhorar preparação/“Tentar avançar” explícito |
| Segunda derrota em j=0 | Hold após acabamento; sem nova tentativa automática. Oferecer preparar e tentar novamente, sem custo novo nem etapa segura fictícia |
| “Tentar avançar” em cultivo/Hold | Agenda nova tentativa após recuperação 4 s, com alvo de avanço guardado j e percurso a partir de k. Zera apenas a série de falhas autorizada pelo comando; ganhos/ledger preservados |
| Retirada manual | Retreated estável, sem recuperação/repetição automática nem recompensa parcial. Reinício explícito preserva ganhos como contrato existente |

Contador `ConsecutiveFailures[stage]` só zera com vitória **nessa etapa**, ou comando explícito de nova tentativa de avanço. Vitória da etapa anterior não zera o bloqueio; subir nível/trocar item não retoma avanço silenciosamente. Em cultivo, uma vitória da própria etapa reinicia sua série local de falhas, mas continua cultivando. Isso mantém farm seguro automático sem tentativas infinitas contra a barreira. Modo avançar/cultivar, alvo de avanço e contadores atravessam tentativas; ResetSession explícito é outra operação.

Recuperação não exige moeda, desgaste, reparo ou item. Não promete save ao fechar; sessão local continua Volatile até a entrega própria de persistência. O retorno para R anterior/R0 é uma **transição declarada de tentativa**, com texto/destino e corpos antigos aposentados, não correção de trajetória durante combate. Nenhuma cura acontece ao comparar/equipar. Só ao término dessa recuperação/reinício os três revivem ao máximo equipado e recargas reiniciam como RestartRoute.

**Compatibilidade com equipamento:** manter permissão apenas em gerenciamento ativo March/Loot ou terminais, inclusive quando pausado; combate pausado nunca permite troca. Prévia continua sujeita à revisão/AttemptId; contagem terminal visível pode iniciar outra tentativa e invalidar confirmação antiga. O jogador pode escolher parar/pausar explicitamente antes de comparar; abrir painel não para. Confirmação e transição são serializadas: equipar antes do limite afeta o máximo do próximo reinício; callback depois recebe AttemptMismatch/StaleComparison, sem retry. `Gerenciar` no terminal continua sem criar pedido; Complete/Defeat permitem troca sem curar. Não alterar HP da troca, propriedade, capacidade/FIFO ou recibos de operações.

“Parar ao final” também segura recuperação após derrota se a intenção é encerrar a atividade; mostrar Defeat estável. Reinício explícito existente continua disponível e preserva sua semântica. Pausa durante contagem congela contagem e acabamento. C2 deve modelar a continuidade no domínio, com um relógio autoritativo e prazos próprios: o snapshot terminal não ganha ticks de ataque/recarga só porque a contagem externa avança. A contagem nunca depende de callback de queda. Ao bater terminal, Advance para nessa barreira; continuação só consome novos deltas após commit e publicação do resultado. Delta restante desse avanço é descartado, sem esconder resultado por cruzar várias tentativas em uma chamada.

## 10. Três trajetos de regra com oráculos

### A. Contorno, gargalo e reunião

1. E2 vence: crédito `(S,A,1)` uma vez; Loot 2 s termina tratamento. Objetivo J0 `(21,14)`. Os três possuem destinos diferentes e nenhum hostil de E3 está ativo.
2. Custos por N/S são 25.6/28 m. Escolher N e guardar BranchChoice. Tanque mais próximo da entrada recebe prioridade; DPS/suporte entram pela distância de caminho e desempate estável. Em N, centros ficam entre Z 20.45 e 21.15 e distam ≥0.80; separação longitudinal desejada 0.90.
3. Um aliado espera reserva por 0.8 s: não marcar stuck. Se perder rota válida, replanejar; se N comprovadamente indisponível antes de entrar, escolher S para o grupo. Nunca atravessar a máquina de J0 para R3. Checkpoint no meio conserva ordem/direção/reservas.
4. Último vivo passa X=31.5 na faixa N; liberar fila temporária e buscar R3. Só com todos a≤2 m de R3 e separados ativar E3. Agora tanque busca 1.25 m da sentry, DPS 3.5–4.5 m e suporte cobertura 6 m; não retornar a uma fila fixa.
5. E3 paga sua identidade lógica uma vez. Retornar por S ou revisitar N não cria outro spawn/crédito de E2/E3 nessa instância.

### B. Alvo móvel, alcance, visão e fronteira do escudo

1. Em T10, Guarda `(10,10)` prepara contra alvo `(11.5,10)`, d=1.50≤1.60; ImpactAt 10.24, RecoveryEnd 10.54, NextEligible 11.60. HP do alvo 100 fica 100 durante preparação.
2. Alvo anda a `(12.1,10)` até 10.24; d=2.10. Resolver `OutOfRange`, HP 100, nenhum número de dano. Guarda fica plantado até 10.54 e depois aproxima para 1.25; não agenda novo básico antes 11.60. Se alvo regressasse até 1.60 em 10.24, o acerto seria válido.
3. Um tiro de `(21,14)` contra `(33,14)` é recusado antes do início: além do alcance e máquina no caminho; não consumir cadência. Para testar perda de visão no próprio blockout, fixture DPS em `(23.9,20.65)` e rat em `(22,19.5)`: o segmento cruza X=23 em Z≈20.105, livre. Durante os 0.12 s de preparação, rat move 0.30 m para `(22,19.2)` (2.50 m/s, dentro de sua velocidade). Agora o segmento cruza X=23 em Z≈19.963, dentro da máquina. Ambos continuam a menos de 2.5 m: resolver `Occluded` no tiro instantâneo, traçador no primeiro sólido e HP preservado. Não é necessário introduzir parede dinâmica.
4. Cura começa com paciente em 5.8 m e HP 50/100. Paciente chega a 6.2 m até StartedAt+0.36: cura 0 por alcance, recarga continua 4 s, próxima decisão aproxima. Se outro efeito o deixou cheio antes do contato, cura 0 por `NoMissingHealth`; se morreu, cancelamento TargetDead, sem ressuscitar/substituir alvo.
5. Pesado contra Guarda chega em 14.000000; escudo `[10,14)` já expirou, portanto usa dano sem mitigação. Com escudo novo válido `[13.4,17.4)`, recebe 35% do dano pós-defesa. No empate entre tiro letal aliado e pesado inimigo, tiro resolve primeiro e morte cancela pesado; não mostrar dano pesado sem resolução.

### C. Derrota, recuperação e repetição com ganhos conservados

1. Sessão S, tentativa A7 venceu E1–E3 e guarda recibos/XP/itens I1, I2; falha pela primeira vez em E4 (j=3). Não pagar E4; terminar quedas≤0.70 s. Recuperação 4 s inicia A8 em R3 (k=2), HP máximo, slots/ItemIds/RNG/ledger conservados.
2. E3 vencido em A8 gera **novo recibo**, pois é nova vitória; não reaproveitar `(S,A7,2)`. Ao entrar E4, `Failures[3]` continua 1. Segunda derrota em E4 marca 2 e recupera A9 cultivando E3. Não alternar automaticamente para E4 após cada vitória em E3.
3. Cada vitória real cultivando E3 gera novo recibo em nova tentativa, sem zerar carteira/XP/equipados. Um evento antigo A7 recebido de novo não paga nem atinge entidades A9. Se cultivo E3 passar a perder duas vezes, baixar para E2; se nada é seguro até E1, parar após segunda queda no piso.
4. Jogador melhora preparação em janela segura e escolhe “Tentar avançar”: autoriza nova série. Ao vencer E4, sua série zera; E5 segue. Ao vencer E5, creditar uma vez e repetir rota após 4 s em nova tentativa R0, salvo “Parar ao final”/gerenciamento pedido/pausa.
5. “Parar ao final” ligado deixa Complete estável, sem cura extra nem novo crédito ao abrir Status. Um reinício explícito restaura equipe e preserva toda a sessão. Nenhuma etapa desse exemplo chama ResetSession ou escreve save legado.

## 11. Mudanças em relação ao contrato v1

| Trecho anterior | Decisão desta C1 |
|---|---|
| Impacto v1 §9, formação lógica parada/sem perseguição; §4 distância visual irrelevante | Já substituído pelo complemento v2: posição autoritativa individual, alcance/visão no impacto, caminho e movimento integrado |
| Exemplo do tiro v1: libera 0.12, impacto 0.23 (voo 110 ms) | Substituído pelo v2 e confirmado aqui: libera=impacto 0.12; contato espacial instantâneo. Não preservar voo cosmético 110 ms |
| Exemplo da cura v1: libera 0.20, chega 0.36 | C1 mantém impacto 0.36 e recuperação 0.60, mas escolhe pulso sem voo e visão obrigatória; não fica trajetória curativa atravessando obstáculo |
| v1 §6 propostas de morte, recarga, empates, escudo semiaberto | Confirmadas; cancelar alvo morto preserva tempo de recuperação do autor vivo. Auto de escudo antes dos impactos explicitado |
| Cura atual fora do slot básico | Mudança deliberada: primária exclusiva e prioridade sobre próximo ataque, sem interromper comprometido |
| Equipamento v1: só reinício explícito sai de Complete/Defeat | **Extensão autorizada pelo plano v2**: continuidade automática com contagem e opção de parar. Janelas/transações/HP da troca preservados; invalidar prévias na nova tentativa |
| v1 §12 terminal não avança scheduler, acabamento≤0.70 s | Mantido para combate e apresentação; continuidade tem prazo autoritativo próprio e cria nova tentativa depois da barreira. Não rodar combate no acabamento |
| Plano v2 pesado 8–12 s; escudo 12 s | Escolher limite 12 s entre inícios, aviso 0.90 e auto 0.60 antes do impacto; 8–11 s não garantem proteção em toda ocorrência com essa recarga |
| Plano v2 repetir E1 após derrotas no piso | Fechado: no máximo duas derrotas sem vitória no piso, depois Hold. Evita ciclo infinito sem etapa segura |

Não foi encontrada necessidade de ampliar o escopo para inventário/save/skills. As incompatibilidades comportamentais listadas são versões novas explícitas, não regressões a esconder. Recomendação ao Director: revisar em especial continuidade/terminal e duração de chefe com os HP atuais; C2 deve versionar RulesetId e adaptar oráculos correspondentes antes de C3. Não há pedido de nova autorização de Arthur para essas decisões já contidas no plano.

## 12. Parâmetros de referência e handoff C2

Fixture de preparação proposta: três integrantes Nv1; cada função com arma/armadura/acessório comum compatível da cadeia de equipamento, recebidos pela fixture (não itens iniciais grátis em produto). Guarda HP 190/ATK 10/DEF 12; DPS 110/20/6; suporte 120/8/7, infusão 44. Tudo cheio, sem status/recarga, semente de recompensa padrão, modo avançar/auto escudo ligado. Controle adicional: starter sem equipamento, 180/8/10, 100/18/4, 110/6/5, infusão 42. Progressão real por vitória continua habilitada. C5 compara essas preparações e registra níveis/itens efetivos ao entrar em cada encontro.

Hipóteses de ritmo herdadas: comuns 12–25 s; chefe 35–60 s; descanso 2 s; recuperação 4 s. Não alterar HP/recompensas para forçar essas faixas antes do ensaio. Pela conta estática de Nv1 equipado, foco no foreman sem movimento/cura oferece `7/1.6+17/1.25+5/2.4≈20.06 HP/s`; 420 HP levariam≈20.94 s de dano sustentado, fora drone, preparação, deslocamento e cura. Isso mostra que 35–60 s pode não se confirmar. Medir por encontro tempo de deslocamento, espera/visão, ação, cura, dano recebido, cobertura de escudo e vencedor; o Idle Economy Designer calibra números com essa evidência sob coordenação do Director, sem abrir nova economia nesta C1. A rota de referência precisa vencer no automático antes de ser aceita; conta de recarga não prova vitória.

| Dado/campo semântico necessário | Obrigação de C2 |
|---|---|
| SessionId/AttemptId/EncounterId/ActorId/ActionId/EventSequence/RulesetId | Estender cadeia existente; ação e recompensa não usam referência DTO/slot como identidade |
| MapId/Revision, sólidos, piso/portais, PositionXZ, heading, raio, velocidade | Compartilhar geometria entre domínio/cliente; escolher caminho determinístico ou adaptador com entradas/resultados registráveis |
| Intent/TargetId, DesiredPosition, path/índice, BranchChoice | Reproduzir por ator; guardar compromisso, aquisição/perda e seleção de candidato |
| Reservas do gargalo, direção, ordem, último progresso/repath, HoldReason | Checkpoint/clone/restore durante passagem não perde exclusão nem teleporta |
| Started/Released/Impact/RecoveryEnd/NextEligible, potência capturada | Scheduler exato, um resultado terminal por ação; integração de movimento nas fronteiras |
| ShieldActive/Expires/Ready, heavyDue/announcedImpact, contadores de swing | Uma ativação auto/manual; projeção correta de janela/reserva e expirado na igualdade |
| Mode(advance/farm), FarmStage, AdvanceTarget, Failures[], StopAtEnd | Atravessar tentativas sem auto liberar barreira; piso/retirada definidos |
| Outcome terminal, ContinuationDue/Held, ReturnAnchor, recuperação | Uma barreira de commit; simulação versus acabamento separados; nova AttemptId sem ResetSession |
| Resultado espacial/motivo, posições/ponto do primeiro contato, HP/Raw/Prevented/Effective | Coordenador apresenta resolução; valores positivos/zero corretos e contato obstruído verificável |

C2 ainda decide base/SHA, tipos/arquivos, representação de tempo/movimento, consultas e garantia de reprodução, orçamento de caminhos, versionamento de snapshot/checkpoint, enquadramento/câmera e tratamento dos gates 29/30. C1 fornece comportamento; não presume interfaces novas já existentes. Sem expandir para framework genérico. Separação de função não cria nova habilidade de proteção/taunt. GAMELAB-17 mantém futuros pontos/habilidades.

Aceite executável a transportar para C2/C3/C5:

- Antes/em/depois de ImpactAt: uma mutação válida; cura/tiro 0 quando fora de alcance/visão; alvo móvel, alvo morto e autor morto sem redirecionamento.
- Escudo na ativação, 1µs antes da expiração, na igualdade; auto/manual no mesmo T; dois pesados 12 s apartados; emergência que consome reserva é sinalizada.
- Cura versus básico prontos, cura durante preparação de ataque, paciente cheio/morto/inacessível; zero cura não mostra ganho fictício.
- Caminhos N/S alcançáveis, reta pela máquina proibida; gargalo/reserva, coesão 8 m, travamento 8 s, leash 6 m e loop sem dano; alvo não é puxado no impacto.
- Checkpoint em movimento/preparação/corredor e restore: mesmas próximas resoluções, reservas e ganhos, dentro da garantia formal de C2; passos/FPS diferentes não mudam sequência semântica.
- Cinco créditos por rota tanto N quanto S, replay 0 créditos extras; farm/new attempt permite só vitórias novas; falhas no piso param; inventário/slots/RNG preservados.
- Gerenciamento pedido em vitória, pause terminal/countdown, equipamento na fronteira de tentativa, retirada antes/depois de crédito e legacy/saves conservados.
- Runtime posterior:20 impactos comuns e 10 pesados/curas, contato/HP/número/VFX dentro de um frame normal; áudio audível medido separado, viewport compacto real e gates 29/30 sem relaxamento.

Estimativa revisada **apenas de C1**:1.5–2.5 dias úteis de esforço para especificação, blockout cotado, revisão de consistência e uma rodada de ajustes; não é medição de tempo deste agente nem SLA. A expansão espacial inclui métricas/portais, reservas, coesão e recuperação que não existiam na estimativa anterior. C1 agora está entregue documentalmente; ajustes de revisão consomem essa margem conforme achados. C2–C5 só podem ser reestimadas após contrato e ensaio de navegação/câmera; 7–12 dias globais anteriores não são válidos.

## 13. Verificação desta entrega e limites

Verificações documentais: leitura das fontes/SHAs e contratos acima; ancestralidade main→cadeia 1fae 849; PR 5 ainda aberto; conferência de fórmulas/atributos/cadências; cálculo de portais/gargalo; checagem geométrica independente com grade de 0.10 m, discos de raio 0.35+folga 0.10, conectividade dos POIs por N e por S e bloqueio da reta pela máquina. Resultado detalhado acompanha `combat-consistency.txt`. Esse cálculo discretizado é uma verificação estática de desenho, não prova do navegador da engine nem de avoidance em runtime.

Os arquivos alterados são apenas documentos em `docs/combat/`. Não houve build/teste de gameplay, playtest, benchmark, mapa em Godot, produção de asset, alteração de save, publicação, merge ou promoção de C2. Entrega por anexos autenticados; não há PR necessário para esta especificação. Revisão do Director pendente; parâmetros de ritmo, preparação e câmera permanecem hipóteses até C3/C5.
