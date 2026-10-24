# Sistemas, progressão e economia do TaskbarHero

Análise da instalação fornecida pelo usuário, em 2026-10-23. Este documento combina tabelas efetivamente empacotadas com textos de interface. **Não houve execução do jogo nesta frente de investigação**: “configurado” significa presente nos dados; “descrito” significa informado pela localização; “inferência” indica interpretação. As fórmulas do executável nativo e a disponibilidade atual do servidor não foram estabelecidas por essas fontes.

O [catálogo completo](Catalogo.md) enumera 59 TextAssets, seus 43.952 registros, esquemas, limites e exemplos. As referências `Txxxx` abaixo são seletores da evidência **D01**, [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json): `TaskBarHero_Data/sharedassets0.assets`, pathID `xxxx`, TextAsset indicado. O número de linha refere o CSV extraído, com cabeçalho na linha 1. Não são IDs gerados pelo servidor REA. A evidência **L01** está em [Localizacao_Resumo.json](evidencias/Localizacao_Resumo.json). Esses resumos ficam versionados; os CSV e textos completos são suplementos locais em `artifacts/`. As evidências de código e da investigação REA ficam no relatório principal.

## Visão do ciclo

A combinação de `StageInfoData`, `MonsterInfoData`, `ItemInfoData`, `HeroInfoData`, `AttributeInfoData` e das receitas indica este ciclo de design:

1. Formar um grupo de heróis e equipar armas, armaduras, acessórios e habilidades.
2. Enfrentar ondas e chefes em fases; coletar ouro, experiência, caixas e materiais.
3. Gastar pontos de habilidade no personagem e ouro em desbloqueios.
4. Converter e melhorar itens no Cubo; abrir rotas de progressão por runas.
5. Acessar dificuldades e conteúdos adicionais, com pets, relíquias, relicários e ressonâncias.

Isso é uma **reconstrução estrutural**, não uma gravação de uma sessão. O texto `Tutorial_FirstKillMonster` descreve que o chefe aparece quando a barra de progresso da fase enche; `Tutorial_CanLearnSkill` descreve ganhar um ponto ao subir de nível; `Tutorial_EquipGear` descreve equipar por arraste ou clique direito. Fonte dos textos: StringTable, conforme a seção de localização abaixo.

## Personagens, equipamento e pontos

`T2520 HeroInfoData`, linhas 2–7, configura seis classes. Os nomes em português vêm de `HeroName_*` na localização.

| ID | Nome pt-BR | Arma principal / secundária | HP base | Dano base | UnlockCost | DLCAppId | IsFirstAvailable |
|---|---|---|---:|---:|---:|---:|---|
| 101 | Cavaleiro | SWORD / SHIELD | 130 | 2 | 500 | 0 | True |
| 201 | Explorador | BOW / ARROW | 60 | 1 | 500 | 0 | True |
| 301 | Feiticeiro | STAFF / ORB | 50 | 2 | 500 | 0 | True |
| 401 | Sacerdote | SCEPTER / TOME | 95 | 1 | 500 | 4427410 | False |
| 501 | Caçador | CROSSBOW / BOLT | 70 | 2 | 500 | 4427420 | False |
| 601 | Matador | AXE / HATCHET | 115 | 2 | 500 | 4427430 | False |

Todas as seis linhas têm `IsAvailable=True`. `IsFirstAvailable`, `DLCAppId`, `HasDLCDrop` e o custo são condições distintas; os dados não provam que possuir ouro basta para adquirir uma classe DLC. `HasDLCDrop=True` aparece em Caçador e Matador. Não foi feita aquisição nem alteração de conta.

`T2523 AttributeInfoData` contém 22 nós por classe: 16 passivos e 6 ativos, totalizando 132. Todos têm `RequiredPoint=1`; os máximos configurados por nó são 3, 5, 8 ou 10. O campo `ATTRIBUTETYPE` escolhe o destino de `Value`: habilidade passiva ou ativa. `T2483 AttributeGroupInfoData`, linhas 2–9, define oito grupos com pontos alocados exigidos de 0 a 70, em passos de 10. A associação é `AttributeInfoData.GroupKey → AttributeGroupInfoData.AttributeGroupKey`.

`T2493 PassiveSkillInfoData` contém 108 linhas. Esse número excede os 96 nós passivos da árvore; não se deve assumir uso direto de todas as linhas. Cada passiva descreve `STATTYPE`, `MODTYPE` e `Value`. O nó ativo `AttributeKey=101003`, por exemplo, é referido por `T2508 SkillInfoData`, linha 3, `SkillKey=10101`.

Em equipamento, `T2517 ItemInfoData` define a identidade e a posição, `GEARTYPE`, grupo, raridade, nível, referência de atributos e flags Steam. Há posições para arma principal, secundária, capacete, armadura, luvas, botas, bracelete, anel, amuleto, brinco (`EARING` no dado), relicário e relíquia. Isso não estabelece quantos slots de cada tipo estão visíveis ou abertos.

Exemplo rastreável: `ItemKey=300001` (`T2517`, linha 337) é espada comum, nível 1, arma principal, `GearKey=300001`; `T2497 GearInfoData`, linha 2, fornece valores base 1 e 20. `T2492 GearTypeInfoData`, linha 2, interpreta esses dois campos como `AttackDamage/FLAT` e `AttackSpeed/FLAT`. Logo, o esquema evita repetir o nome de cada atributo em cada linha. A conversão de AttackSpeed para ataques por segundo precisa do código.

## Menu de status: informação que os dados sustentam

Os textos configuram nome da janela, pontos restantes, subir nível, reembolsar pontos, equipar, desequipar, pontos exigidos e um acesso a estatísticas detalhadas. Chaves: `Status`, `Status_RemainedSkillPoint`, `UI_Status_LevelUp`, `UI_Status_Refund`, `UI_Status_Equip`, `UI_Status_Unequip`, `UI_Status_RequiredPoints`, `UI_Status_DetailedStats`. `NormalAttackDPS` diferencia expressamente DPS do ataque básico; não há evidência aqui de que seja o DPS agregado de todas as habilidades.

`T2488 StatViewInfoData` contém 17 regras de apresentação com `SectionType`, `Order`, `IsShowStatus` e `IsShowZero`. Ela parece uma tabela de exceções/ajustes: inclui atributos ocultos e não enumera os atributos base HP, ataque ou armadura. Portanto, não deve ser usada isoladamente como lista completa da janela.

| Grupo | Atributos configurados com IsShowStatus=True | Ordem |
|---|---|---|
| Offense | BossDamage; FullHpTargetDamage; LowHpTargetDamage; OnReviveAttackDamage | 102–105 |
| Defense | OnHitDamageReduction; LowHpDamageReduction; CrowdControlDurationReduction | 101–103 |
| Misc | WaveStartMovementSpeed; GoldAmount; ReviveCooldownReduction | 101–103 |

Todas essas linhas têm `IsShowZero=False`, exceto `GoldAmount=True`. As outras sete linhas escondem `FullHpTargetCriticalChance`, `AttackAndCastSpeed`, `DecreaseFireResistance`, `DecreaseColdResistance`, `DecreaseAttackDamage`, `DecreaseLightningResistance` e `DecreaseChaosResistance`. O efeito real da tabela no layout depende do controlador.

Os tooltips `Tooltip_Stat_*` descrevem armadura, absorção fixa do dano final, resistências com penalidade por dificuldade, esquiva, bloqueio, redução de recarga e redução geral de dano. O texto descreve bloqueio bem-sucedido como metade do dano físico/elemental. Vários limites aparecem apenas como parâmetro `{0}%`; o valor do teto não está na StringTable. A chave histórica `Tooltip_Stat_PoisonResistance` atualmente descreve dano de Caos, o que recomenda usar o conteúdo do texto além do nome da chave.

**Ideia aproveitável para o Aetherforge:** reunir personagem, equipamentos e árvore de habilidades num contexto reconhecível; manter o resumo essencial legível e abrir detalhes agrupados por ataque, defesa e utilidade; esconder modificadores irrelevantes quando zero; oferecer explicação do cálculo/limite junto do atributo. Essa é uma proposta de design, não requisito extraído. Layout, dimensões, cores e fluxo visual são tratados pela investigação de UI.

## Combate, habilidades e modificadores

`T2508 SkillInfoData` tem 128 linhas, incluindo personagens, monstros e conteúdos especiais: 68 `BASEATTACK`, 45 `COOLDOWN`, 13 `BASEATTACK_COUNT`, 2 `CONTINUOUS`. Os tipos de dano presentes são Physical, Fire, Chaos, Cold e Lightning. As tags de entrega incluem Melee, Projectile, AOE, Summon e Trap, inclusive combinações.

Exemplos: `SkillKey=10101`, linha 3, tem `BASEATTACK_COUNT`, `ActivationValue=6`, dano físico, `Melee, AOE` e progressão `SkillLevelKey=10101`; `SkillKey=10501`, linha 7, é `COOLDOWN`, `ActivationValue=17`, Buff, `BuffGroupKey=10501`. Os campos `Param1…Param5`, `Range`, `Order` e `Value` requerem semântica específica por habilidade; não se converteu número em porcentagem/tempo sem código.

`T2525 SkillLevelInfoData` tem 360 linhas e associa chave+nível ao valor. `T2538 BuffInfoData` tem 31 entradas; `T2482 BuffGroupInfoData` tem 18 linhas/17 chaves, ligando grupos a listas de buffs. Existe uma duplicidade real: `BuffGroupKey=60601`, linhas 12 e 13, primeiro com `606011`, depois vazio. Não sabemos se o carregador ignora, sobrescreve ou rejeita a repetição.

`T2510 StatusEffectInfoData`, linhas 2–7, configura Chill, Freeze, Ignite, Shock, Bleed e Stun. Os valores de duração são 400, 150, 400, 600, 700 e 200, respectivamente. A unidade não foi confirmada. `OverrideType` distingue `InitDuration` e `NotOverride`.

Há três camadas complementares de modificadores: `T2509 StatModInfoData` (985 linhas, chave+tier, mínimo/máximo/intervalo), `T2522 UniqueModInfoData` (132 linhas, parâmetros com tipos de conversão) e `T2496 ModRuleInfoData` (80 regras de acúmulo). As regras contêm 44 Custom, 14 Max, 13 Once, 8 Additive e 1 Independent. Exemplo `CooldownReduceAllSkills`, linha 2: `StackRule=Additive`, `StackLimit=600`; não se inferiu “60%” desse literal.

## Fases, dificuldades e loot

`T2507 StageInfoData` tem 192 fases: 108 NORMAL, 12 ACTBOSS, 9 CONTAMINACTBOSS, 60 PLAGUE e 3 EVENT. A dificuldade, campo separado do tipo, distribui-se em 30 NORMAL e 54 em cada uma de NIGHTMARE, HELL e TORMENT. Os `StageLevel` usados vão de 1 a 95. Existem fases com campos de ondas vazios, especialmente chefes; vazio não significa zero ondas executadas.

`StageKey=1101`, linha 2, configura 10 ondas, 1 monstro por onda, composição `10011_1000 10021_1000`, chefe 10022, próxima fase 1102, caixa de monstro 910011, caixa de chefe 920011 e recompensa de primeira vitória 9200010. As taxas literais são `MonsterDropItemRateM=160000` e `BossDropItemRateM=1000000`; a base de normalização e seu processamento não foram confirmados.

`StageKey=1310`, linha 31, é ACTBOSS do ato 3 normal, passa a 2101 e configura `SoulstoneItemKey=190001`, quantidade 1. O paralelo Nightmare `2310`, linha 61, passa a 3101 e usa soulstone 190002. Isso evidencia conexões entre rotas e recursos, sem provar a regra de desbloqueio inteira.

`T2512 MonsterInfoData` tem 81 monstros com recompensas base, habilidade, dano, velocidade, HP e referências de apresentação. `T2490 StageLevelInfoData` tem 170 linhas de multiplicadores de dano, HP, ouro e XP; sua existência até 170 não comprova 170 níveis acessíveis. O nome `Multiplier` não revela sozinho divisor, ordem de operações ou arredondamento.

O caminho de loot é tipado: fase → item/caixa (`T2517`) → `DropKey` (`T2487 DropInfoData`) → item ou grupo (`T2518 ItemGroupInfoData`) → itens. `T2487` tem 20.311 linhas em 380 grupos de sorteio; 17.994 referem ITEMGROUP e 2.317 ITEM. Os tipos de sorteio configurados são `EachDropOneWeight_DLCVariant`, `EachDropOneWeight` e `SelectOneByClass`. `HeroKeyCondition` e `EventKey` podem condicionar candidatos. Não se dividiu um peso pelo total para afirmar uma probabilidade final; filtros, fallback e variação DLC precisam do código.

## Economia e inventários

`T2505 CurrencyInfoData`, linha 2, define uma moeda: `100001`, nome localizado Ouro, `InitialAmount=100`. A ausência de outras linhas nessa tabela não nega outros recursos: soulstones, contaminação e materiais aparecem em outros esquemas.

`T2484`, `T2495`, `T2516` e `T2524` configuram quatro espaços com índices e custos. A relação entre cada linha e a unidade visual comprada deve ser confirmada no controlador.

| TextAsset | Linhas/índices | Linhas de custo zero | Maior CostForUnlock |
|---|---|---:|---:|
| InventoryInfoData | 260; 0–259 | 20 | 10.240.000 |
| StorageInfoData | 101; 0–100 | 39 | 3.100 |
| StashInfoData | 131; 0–130 | 41 | 5.120.000 |
| TradingStashInfoData | 10; 0–9 | 10 | 0 |

Os dados de item contêm 6.696 GEAR, 189 MATERIAL e 146 STAGEBOX. **4.364 das 7.031 linhas têm `IsDeletedInServer=True`**, portanto a contagem bruta inclui entradas retiradas/legadas segundo o próprio campo. Todas têm `IsSteamItem=True`; 1.113 permitem `IsCanExchangeMarketable=True`; 50 marcam `IsTempMarketableFalse=True`. Esses metadados não demonstram fluxo de compra, preço, transação ou disponibilidade atual no mercado.

## Cubo e qualidade de itens

`T2519 CubeRecipeInfoData` configura dez famílias. Somente SYNTHESIS tem `IsDefaultUnlocked=True` preenchido na tabela principal. `T2537 CubeSubRecipeInfoData` traz 89 sub-receitas: 63 Offering, 9 Synthesis, 9 Crafting, 2 Attunement e uma por cada outra família. Existem requisitos de nível do Cubo, custo, material e custo de acionamento; a regra combinada entre receita principal e sub-receita precisa do carregador.

| Família | Função descrita nos textos do jogo | Requisito exemplar em T2537 |
|---|---|---|
| Synthesis | sintetizar 9 itens da mesma qualidade | linha 2: tier 1, nível 1, custo 0 |
| Alchemy | converter 1–9 itens em ouro | linha 11: nível 1, custo 10 |
| Corrosion | obter contaminação | linha 12: nível 30, custo 10.000 |
| Decoration | aplicar atributo em slot de decoração com material | linha 13: nível 8, custo 300 |
| Engraving | aplicar atributo em slot de gravação com material | linha 14: nível 15, custo 1.000 |
| Inscription | aplicar atributo em slot de inscrição com material | linha 15: nível 25, custo 10.000 |
| Crafting | escolher tipo e adicionar materiais exigidos | 9 sub-receitas |
| Offering | oferecer tributo e receber itens | 63 sub-receitas |
| Attunement | sortear novamente opções de relicário/relíquia | linhas 88–89: nível 10 |
| Extraction | remover atributos sem devolver o material aplicado | linha 90: nível 10, custo 1.000 |

Fonte textual: chaves `Cube_RecipeTooltip_*`. A descrição simplifica o sistema de síntese: `T2504 SynthesisRecipeInfoData` tem 605 receitas com tier, tipo, qualidade, quantidade, nível médio mínimo e pesos de níveis resultantes. `T2529 SynthesisDropInfoData` tem 239 combinações e aponta a `DropKey`. Exemplo `SynthesisRecipeKey=10100110`, linha 2: Gear COMMON, 9 materiais, tier mínimo 1, nível médio mínimo 1, resultado nível 1, `LevelWeight1=100`. Não se pode extrapolar essa receita para todo tipo de item.

`T2528 CraftingRecipeInfoData` tem 63 receitas com listas `Material`, índices e sorteio de resultado. Exemplo linha 2: `6001001`, MainWeapon, tier 1, `140003_1`, `DropKey=3601001`. `T2527 ExtractionCostInfoData` contém 90 combinações, tiers 1–10, custo de 100 a 200.000. `T2502 AttunementCostInfoData` contém 20 combinações de RELIQUARY/RELIC com qualidade: `MaxCount=19` em todas, e BaseCost/CostIncrease separados. A fórmula de aplicação não foi deduzida.

`T2513 GradeInfoData` define dez qualidades, nesta ordem: COMMON, UNCOMMON, RARE, LEGENDARY, IMMORTAL, ARCANA, BEYOND, CELESTIAL, DIVINE, COSMIC. Há pesos para manter/subir/descer qualidade, números de slots, bases de ouro/XP/contaminação e pesos específicos de relíquias. Exemplos: COMMON tem zero slots inerentes e base de alquimia 10; COSMIC tem 3 slots inerentes, 2 Decoration, 2 Engraving, 2 Inscription e base de alquimia 355.607. Multiplicadores por nível, tipo de item e tipo de equipamento estão em `T2534`, `T2489` e `T2506`. Sua composição e divisores permanecem não confirmados.

## Runas, níveis e recompensa offline

`T2503 RuneInfoData` descreve 253 nós de um grafo com próximas runas, previews, exigência de nível anterior, máximo e chave da progressão. `T2485 RuneLevelInfoData` contém 858 linhas de custo e efeito por nível. O desenho combina melhorias numéricas e desbloqueios de conveniência/conteúdo, com máximos de 1, 2, 3, 5 ou 10.

Exemplos de desbloqueios, todos usando `CostItemKey=100001` (Ouro):

| RuneKey | Efeito literal | Custo | Referências |
|---|---|---:|---|
| 11001 | UnlockOfflineReward +1 | 100 | T2503 linha 27; T2485 linha 75 |
| 21 | UnlockArrangeSlotCount +1 | 1.000 | T2503 linha 57; T2485 linha 169 |
| 24 | UnlockArrangeSlotCount +1 | 150.000 | T2503 linha 60; T2485 linha 178 |
| 27 | UnlockSkillSlotCount +1 | 50.000 | T2503 linha 63; T2485 linha 185 |

Há efeitos para abrir caixas automaticamente, reduzir tempo de abertura, expandir stash e melhorar ganho offline. Esses nomes/textos sustentam a presença de progressão de automação; frequência, timers e ordem de processamento dependem do código. Não foi encontrada nestes 59 TextAssets uma tabela dedicada chamada rebirth/prestige; isso é somente uma busca delimitada, não prova de ausência da mecânica no executável.

`T2535 LevelInfoData` e `T2500 CubeLevelInfoData` contêm 100 linhas cada. Para `Level=1/10/50/100`, `ExpForLevelUp` é respectivamente 30/74.880/81.322.760/1.997.771.834 no primeiro e 15/2.150/897.450/999.999.999 no Cubo. Como a linha 100 ainda possui requisito de XP, a tabela sozinha não confirma cap efetivo ou se a XP é acumulada ou incremental.

`T2521 OfflineRewardInfoData` tem 116 linhas por StageLevel e campos BaseGold, BaseExp, KillCount, ClearCount. Nível 1, linha 2: 1, 1, 180, 20; nível 116, linha 117: 1.110, 21.898, 6.300, 10. Os textos `AccountStat_UnlockOfflineReward`, `AccountStat_OfflineRewardGoldPercent`, `AccountStat_OfflineRewardExpPercent` descrevem ganho de ouro/XP offline e bônus. **Não há aqui prova de período-base, cap de horas, fórmula por segundo, fase selecionada ou compensação de tempo**; os números não devem ser usados como rendimento horário.

## Plaguelands, relíquias, relicários e ressonância

O texto `Contamination_Tooltip` descreve contaminação como requisito de entrada e recurso consumido durante permanência nas Plaguelands, adquirido fora da área ou por Corrosion. `T2539 PlagueIntensityInfoData` configura 20 intensidades, três modificadores por intensidade, total 60 linhas. São dados de MovementSpeed, MaxHp e AttackDamage; a aplicação a personagens/monstros e a conversão da escala precisam do código.

As 60 fases PLAGUE estão em `T2507`; exemplo linha 122, `StageKey=201201`, Nightmare, StageLevel 40, próxima 201202, 17 ondas, 15 monstros por onda, `rewardString=PlagueLandReward_201201`. Os textos `PlagueLandReward_*` informam aumentos de chance de itens; não foram tomados como probabilidades medidas.

`T2486 ReliquaryInfoData` possui 382 registros, com pools inerentes, quantidade de atributos, índices de slots e ressonâncias. `T2499 StatPoolInfoData` descreve 90 linhas de candidatos com mínimo/máximo/intervalo/peso. `T2501 RelicInfoData` contém 360 relíquias: 90 Red, 70 Blue, 100 Yellow, 80 Purple, 20 White. A relíquia branca final `759002`, linha 361, tem tier inerente 10 e `AdditionalMod=ResonanceDoubleCount`.

`T2533 ResonanceInfoData` tem 63 combinações, como linha 2: `ResonanceKey=20101`, `Condition=Blue_2`, `PartyModKey=20101`. `T2530 PartyModInfoData`, também 63 linhas, distingue tipos e parâmetros de efeitos de grupo. Os textos `PartyMod_*` incluem reviver aliado, compartilhar dano, bônus com slots vazios, líder aleatório, propagação de efeitos e aumento de ressonância. Esses textos demonstram intenções variadas; as condições de aplicação e valores finais não foram executados nesta análise.

## Pets, visuais e eventos

`T2531 PetInfoData` contém dez pets. Cinco usam `UnlockCondition=KillMonster`, alvo específico e quantidade 5.000; três usam DLC 4427390; dois usam evento 50001 e quantidade 500 de monstros do evento. `T2532 PetStatInfoData` contém 13 linhas de atributos por PetStatKey, indicando bônus além de apresentação visual. Os campos de condição foram observados, sem completar objetivos numa conta.

`T2491 PcSkinInfo` contém 100 partes cosméticas, com categoria, agrupamento, custo (0–150), camadas, estado padrão e ícone. `T2526 SkinInfoData` tem 18 registros de skins/sets com IsDefault/IsFree; os dois catálogos não devem ser somados como se fossem a mesma unidade.

`T2498 EventPeriodInfoData`, linha 2, configura evento 50001 de **2026-09-18 00:00:00 a 2026-11-11 23:59:59 UTC**, `IsEnabled=True`. Isso é configuração empacotada, não confirmação da disponibilidade online no dia. `T2536 EventStageInfoData`, linhas 2–4, liga fases 2901/3901/4901 aos desbloqueios 2310/3310/4310, tickets 170101/170102/170103, quantidade 1 e MaxBoxDrop 10.

`T2481 EventBannerInfoData` contém cinco banners com local, prioridade, período UTC, habilitação, imagem e URL. Esses recursos demonstram publicidade/eventos datados na instalação; a análise não abriu links nem fez chamadas aos destinos.

## Localização e limites da evidência

Foram lidos os bundles inglês e português brasileiro e o bundle compartilhado com UnityPy `read_typetree`. Há duas coleções, `ItemTable` e `StringTable`. Os caminhos completos, pathIDs, contagens e registros não resolvidos estão no [catálogo](Catalogo.md#localização-e-resolução-de-chaves). Cada mapa conveniente de idioma contém 3.232 chaves resolvidas.

Para reproduzir a localização de qualquer texto citado acima: executar o [extrator versionado](../../../scripts/research/Inspect-TaskbarHero-Data.py), consultar o suplemento `artifacts/taskbar-hero-rea/current/localization/{en-US,pt-BR}.keys.json`, depois a entrada com a mesma chave em `StringTable_<idioma>.json` ou `ItemTable_<idioma>.json`, que preserva `id`, `source`, `pathID` e SharedTableData. Assim a chave é um seletor verificável, sem depender de número de linha de JSON reformatado. O relatório permanece legível sem o suplemento, e os metadados de origem são mantidos em L01.

Esta frente respondeu à estrutura de dados, referências, catálogo, custos configurados, textos e eixos de progressão. Permanecem sem confirmação: fórmulas de dano/DPS/armadura e arredondamento; tetos de atributos; timers de loot e offline; comportamento do servidor/mercado; carregamento de chaves duplicadas; disponibilidade real de conteúdo legado; condições finais de DLC e eventos; fluxo executado de todos os menus. Elas não podem ser preenchidas por suposição a partir do nome de um campo.
