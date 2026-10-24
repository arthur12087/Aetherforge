# Catálogo de dados do TaskbarHero

Extração estática da instalação fornecida pelo usuário, realizada em 2026-10-23 com UnityPy 1.24.2. São **59 TextAssets CSV, 43.952 registros**, sem linhas com número de colunas incompatível. Os originais permanecem intactos. Nenhuma imagem, música ou sprite foi exportado por esta rotina.

Este catálogo cobre integralmente esses TextAssets, não todo o código ou comportamento do jogo. `rowCount` conta registros CSV; não significa quantidade disponível no servidor, objetos equipáveis simultaneamente nem conteúdo desbloqueado. `ItemInfoData` contém 4.364 registros marcados `IsDeletedInServer=True`; suas 7.031 linhas não podem ser anunciadas como 7.031 itens atualmente obtíveis.

As evidências abaixo usam **arquivo + pathID + chave/linha**, sob o ID **D01**. Todos os TextAssets são de `TaskBarHero_Data/sharedassets0.assets`; os CSV extraídos ficam em `artifacts/taskbar-hero-rea/current/tables/`, como suplemento local ignorado pelo Git. Linha 1 é cabeçalho. Valores são literais: escala de percentuais, duração, unidades e política para campos vazios não foram deduzidas do tamanho do número. O resumo portátil [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json) registra hashes do CSV UTF-8 sem BOM, campos, contagens e limites. Não depende da presença dos CSV para leitura do relatório.

O resumo interpretado está em [Sistemas.md](Sistemas.md). O extrator reproduzível está em [Inspect-TaskbarHero-Data.py](../../../scripts/research/Inspect-TaskbarHero-Data.py).

## Inventário

| Tabela | pathID | Registros | Colunas |
|---|---:|---:|---:|
| [EventBannerInfoData](#eventbannerinfodata) | 2481 | 5 | 8 |
| [BuffGroupInfoData](#buffgroupinfodata) | 2482 | 18 | 2 |
| [AttributeGroupInfoData](#attributegroupinfodata) | 2483 | 8 | 2 |
| [InventoryInfoData](#inventoryinfodata) | 2484 | 260 | 2 |
| [RuneLevelInfoData](#runelevelinfodata) | 2485 | 858 | 6 |
| [ReliquaryInfoData](#reliquaryinfodata) | 2486 | 382 | 5 |
| [DropInfoData](#dropinfodata) | 2487 | 20311 | 7 |
| [StatViewInfoData](#statviewinfodata) | 2488 | 17 | 5 |
| [ItemTypeScaleInfoData](#itemtypescaleinfodata) | 2489 | 2 | 4 |
| [StageLevelInfoData](#stagelevelinfodata) | 2490 | 170 | 5 |
| [PcSkinInfo](#pcskininfo) | 2491 | 100 | 8 |
| [GearTypeInfoData](#geartypeinfodata) | 2492 | 16 | 5 |
| [PassiveSkillInfoData](#passiveskillinfodata) | 2493 | 108 | 5 |
| [StatModGroupInfoData](#statmodgroupinfodata) | 2494 | 474 | 5 |
| [StorageInfoData](#storageinfodata) | 2495 | 101 | 2 |
| [ModRuleInfoData](#modruleinfodata) | 2496 | 80 | 8 |
| [GearInfoData](#gearinfodata) | 2497 | 5954 | 13 |
| [EventPeriodInfoData](#eventperiodinfodata) | 2498 | 1 | 4 |
| [StatPoolInfoData](#statpoolinfodata) | 2499 | 90 | 7 |
| [CubeLevelInfoData](#cubelevelinfodata) | 2500 | 100 | 2 |
| [RelicInfoData](#relicinfodata) | 2501 | 360 | 5 |
| [AttunementCostInfoData](#attunementcostinfodata) | 2502 | 20 | 6 |
| [RuneInfoData](#runeinfodata) | 2503 | 253 | 8 |
| [SynthesisRecipeInfoData](#synthesisrecipeinfodata) | 2504 | 605 | 13 |
| [CurrencyInfoData](#currencyinfodata) | 2505 | 1 | 5 |
| [GearTypeScaleInfoData](#geartypescaleinfodata) | 2506 | 20 | 4 |
| [StageInfoData](#stageinfodata) | 2507 | 192 | 29 |
| [SkillInfoData](#skillinfodata) | 2508 | 128 | 25 |
| [StatModInfoData](#statmodinfodata) | 2509 | 985 | 7 |
| [StatusEffectInfoData](#statuseffectinfodata) | 2510 | 6 | 9 |
| [MaterialInfoData](#materialinfodata) | 2511 | 189 | 4 |
| [MonsterInfoData](#monsterinfodata) | 2512 | 81 | 13 |
| [GradeInfoData](#gradeinfodata) | 2513 | 10 | 18 |
| [SoundInfoData](#soundinfodata) | 2514 | 324 | 4 |
| [CommonStatModInfoData](#commonstatmodinfodata) | 2515 | 12 | 6 |
| [StashInfoData](#stashinfodata) | 2516 | 131 | 2 |
| [ItemInfoData](#iteminfodata) | 2517 | 7031 | 22 |
| [ItemGroupInfoData](#itemgroupinfodata) | 2518 | 2922 | 3 |
| [CubeRecipeInfoData](#cuberecipeinfodata) | 2519 | 10 | 5 |
| [HeroInfoData](#heroinfodata) | 2520 | 6 | 29 |
| [OfflineRewardInfoData](#offlinerewardinfodata) | 2521 | 116 | 5 |
| [UniqueModInfoData](#uniquemodinfodata) | 2522 | 132 | 22 |
| [AttributeInfoData](#attributeinfodata) | 2523 | 132 | 8 |
| [TradingStashInfoData](#tradingstashinfodata) | 2524 | 10 | 2 |
| [SkillLevelInfoData](#skilllevelinfodata) | 2525 | 360 | 3 |
| [SkinInfoData](#skininfodata) | 2526 | 18 | 4 |
| [ExtractionCostInfoData](#extractioncostinfodata) | 2527 | 90 | 5 |
| [CraftingRecipeInfoData](#craftingrecipeinfodata) | 2528 | 63 | 6 |
| [SynthesisDropInfoData](#synthesisdropinfodata) | 2529 | 239 | 6 |
| [PartyModInfoData](#partymodinfodata) | 2530 | 63 | 19 |
| [PetInfoData](#petinfodata) | 2531 | 10 | 8 |
| [PetStatInfoData](#petstatinfodata) | 2532 | 13 | 4 |
| [ResonanceInfoData](#resonanceinfodata) | 2533 | 63 | 3 |
| [ItemLevelScaleInfoData](#itemlevelscaleinfodata) | 2534 | 19 | 4 |
| [LevelInfoData](#levelinfodata) | 2535 | 100 | 2 |
| [EventStageInfoData](#eventstageinfodata) | 2536 | 3 | 6 |
| [CubeSubRecipeInfoData](#cubesubrecipeinfodata) | 2537 | 89 | 10 |
| [BuffInfoData](#buffinfodata) | 2538 | 31 | 5 |
| [PlagueIntensityInfoData](#plagueintensityinfodata) | 2539 | 60 | 4 |

## Dicionário por tabela

Os exemplos são o primeiro registro do respectivo CSV, na linha 2. Campos vazios são preservados; eles não implicam zero ou falso. Chaves repetidas podem representar relações de um para muitos.

### EventBannerInfoData

Fonte D01: `sharedassets0.assets`, pathID `2481`; 5 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/EventBannerInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`EventBannerKey`, `Location`, `Priority`, `StartDateUtc`, `EndDateUtc`, `IsEnabled`, `ImagePath`, `LinkUrl`.

Exemplo da linha 2: `EventBannerKey=10001; Location=SettingGeneral; Priority=0; StartDateUtc=2026-10-14 07:00:00; EndDateUtc=2026-10-29 07:00:00; IsEnabled=True; ImagePath=AdBanner_E7Wish_2610; LinkUrl=https://store.steampowered.com/app/5019180/?utm_source=tbh&utm_medium=display&utm_campaign=tbh_cf&utm_content=Option`.

Limites numéricos presentes: `EventBannerKey`: 10001 a 10005; `Priority`: 0 a 0.

### BuffGroupInfoData

Fonte D01: `sharedassets0.assets`, pathID `2482`; 18 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/BuffGroupInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`BuffGroupKey`, `BuffKeys`.

Exemplo da linha 2: `BuffGroupKey=10501; BuffKeys=105011`.

Limites numéricos presentes: `BuffGroupKey`: 10501 a 4.61e+06.

### AttributeGroupInfoData

Fonte D01: `sharedassets0.assets`, pathID `2483`; 8 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/AttributeGroupInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`AttributeGroupKey`, `RequiredAllocatedPoint`.

Exemplo da linha 2: `AttributeGroupKey=10001; RequiredAllocatedPoint=0`.

Limites numéricos presentes: `AttributeGroupKey`: 10001 a 10008; `RequiredAllocatedPoint`: 0 a 70.

### InventoryInfoData

Fonte D01: `sharedassets0.assets`, pathID `2484`; 260 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/InventoryInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Index`, `CostForUnlock`.

Exemplo da linha 2: `Index=0; CostForUnlock=0`.

Limites numéricos presentes: `Index`: 0 a 259; `CostForUnlock`: 0 a 1.024e+07.

### RuneLevelInfoData

Fonte D01: `sharedassets0.assets`, pathID `2485`; 858 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/RuneLevelInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`LevelKey`, `Level`, `CostItemKey`, `CostValue`, `STATTYPE`, `Value`.

Exemplo da linha 2: `LevelKey=1; Level=1; CostItemKey=100001; CostValue=100; STATTYPE=AllHeroAttackDamage; Value=1`.

Limites numéricos presentes: `LevelKey`: 1 a 1.902e+07; `Level`: 1 a 10; `CostItemKey`: 100001 a 100001; `CostValue`: 100 a 1e+09; `Value`: 1 a 50000.

### ReliquaryInfoData

Fonte D01: `sharedassets0.assets`, pathID `2486`; 382 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ReliquaryInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemKey`, `ResonanceKeys`, `InherentPoolKey`, `InherentStatCount`, `SlotIndex`.

Exemplo da linha 2: `ItemKey=790001; InherentPoolKey=1001; InherentStatCount=1`.

Limites numéricos presentes: `ItemKey`: 790001 a 799063; `ResonanceKeys`: 20101 a 20520; `InherentPoolKey`: 1001 a 1010; `InherentStatCount`: 1 a 3.

### DropInfoData

Fonte D01: `sharedassets0.assets`, pathID `2487`; 20311 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/DropInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`DropKey`, `DropType`, `REWARDTYPE`, `RewardKey`, `HeroKeyCondition`, `Weight`, `EventKey`.

Exemplo da linha 2: `DropKey=9100111; DropType=EachDropOneWeight_DLCVariant; REWARDTYPE=ITEMGROUP; RewardKey=1000010; Weight=900194`.

Limites numéricos presentes: `DropKey`: 3.11101e+06 a 9.41201e+06; `RewardKey`: 130001 a 1.9019e+08; `HeroKeyCondition`: 0 a 601; `Weight`: 0 a 9.6657e+07; `EventKey`: 50001 a 50001.

### StatViewInfoData

Fonte D01: `sharedassets0.assets`, pathID `2488`; 17 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StatViewInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`STATTYPE`, `SectionType`, `Order`, `IsShowStatus`, `IsShowZero`.

Exemplo da linha 2: `STATTYPE=FullHpTargetCriticalChance; SectionType=Offense; Order=101; IsShowStatus=FALSE; IsShowZero=FALSE`.

Limites numéricos presentes: `Order`: 0 a 105.

### ItemTypeScaleInfoData

Fonte D01: `sharedassets0.assets`, pathID `2489`; 2 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ItemTypeScaleInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemType`, `AlchemyGoldScale`, `CubeExpScale`, `ContaminScale`.

Exemplo da linha 2: `ItemType=GEAR; AlchemyGoldScale=1000; CubeExpScale=1000; ContaminScale=1000`.

Limites numéricos presentes: `AlchemyGoldScale`: 1000 a 12000; `CubeExpScale`: 1000 a 12000; `ContaminScale`: 1000 a 1000.

### StageLevelInfoData

Fonte D01: `sharedassets0.assets`, pathID `2490`; 170 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StageLevelInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StageLevel`, `MonsterAtkDmgMultiplier`, `MonsterHpMultiplier`, `MonsterGoldMultiplier`, `MonsterExpMultiplier`.

Exemplo da linha 2: `StageLevel=1; MonsterAtkDmgMultiplier=100; MonsterHpMultiplier=100; MonsterGoldMultiplier=100; MonsterExpMultiplier=100`.

Limites numéricos presentes: `StageLevel`: 1 a 170; `MonsterAtkDmgMultiplier`: 100 a 3.23966e+07; `MonsterHpMultiplier`: 100 a 1.32077e+06; `MonsterGoldMultiplier`: 100 a 134160; `MonsterExpMultiplier`: 100 a 2.6726e+06.

### PcSkinInfo

Fonte D01: `sharedassets0.assets`, pathID `2491`; 100 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PcSkinInfo.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PcSkinKey`, `PartsCategory`, `DecorableType`, `GroupKeys`, `Cost`, `HasUpperLayer`, `IsDefaultUnlocked`, `IconPath`.

Exemplo da linha 2: `PcSkinKey=10001; PartsCategory=Body; DecorableType=Body; Cost=0; IsDefaultUnlocked=True; IconPath=Sprites/Icon/PcSkin/Body_10001`.

Limites numéricos presentes: `PcSkinKey`: 10001 a 25003; `Cost`: 0 a 150.

### GearTypeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2492`; 16 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/GearTypeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`GearType`, `BaseStat1_STATTYPE`, `BaseStat1_MODTYPE`, `BaseStat2_STATTYPE`, `BaseStat2_MODTYPE`.

Exemplo da linha 2: `GearType=SWORD; BaseStat1_STATTYPE=AttackDamage; BaseStat1_MODTYPE=FLAT; BaseStat2_STATTYPE=AttackSpeed; BaseStat2_MODTYPE=FLAT`.


### PassiveSkillInfoData

Fonte D01: `sharedassets0.assets`, pathID `2493`; 108 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PassiveSkillInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PassiveSkillKey`, `SkillNameKey`, `STATTYPE`, `MODTYPE`, `Value`.

Exemplo da linha 2: `PassiveSkillKey=101001; SkillNameKey=Passive_AttackDamage; STATTYPE=AttackDamage; MODTYPE=FLAT; Value=1`.

Limites numéricos presentes: `PassiveSkillKey`: 101001 a 601082; `Value`: 1 a 200.

### StatModGroupInfoData

Fonte D01: `sharedassets0.assets`, pathID `2494`; 474 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StatModGroupInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StatModGroupKey`, `GearGroup`, `StatModKey`, `MinTier`, `MaxTier`.

Exemplo da linha 2: `StatModGroupKey=1100011; GearGroup=WEAPON; StatModKey=102501; MinTier=2; MaxTier=2`.

Limites numéricos presentes: `StatModGroupKey`: 1.10001e+06 a 1.39001e+06; `StatModKey`: 100101 a 105901; `MinTier`: 1 a 10; `MaxTier`: 1 a 10.

### StorageInfoData

Fonte D01: `sharedassets0.assets`, pathID `2495`; 101 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StorageInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Index`, `CostForUnlock`.

Exemplo da linha 2: `Index=0; CostForUnlock=0`.

Limites numéricos presentes: `Index`: 0 a 100; `CostForUnlock`: 0 a 3100.

### ModRuleInfoData

Fonte D01: `sharedassets0.assets`, pathID `2496`; 80 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ModRuleInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ModName`, `StackRule`, `StackParam`, `IdentityParam`, `MainValueParam`, `StackLimit`, `ExclusiveGroup`, `Priority`.

Exemplo da linha 2: `ModName=CooldownReduceAllSkills; StackRule=Additive; StackParam=1; IdentityParam=0; MainValueParam=1; StackLimit=600`.

Limites numéricos presentes: `StackParam`: 0 a 2; `IdentityParam`: 0 a 1; `StackLimit`: 600 a 1000; `Priority`: 1 a 20.

### GearInfoData

Fonte D01: `sharedassets0.assets`, pathID `2497`; 5954 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/GearInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`GearKey`, `BaseStat1_Value`, `BaseStat2_Value`, `InherentStat1_STATTYPE`, `InherentStat1_MODTYPE`, `InherentStat1_Value`, `InherentStat2_STATTYPE`, `InherentStat2_MODTYPE`, `InherentStat2_Value`, `InherentStat3_STATTYPE`, `InherentStat3_MODTYPE`, `InherentStat3_Value`, `UniqueModKey`.

Exemplo da linha 2: `GearKey=300001; BaseStat1_Value=1; BaseStat2_Value=20; InherentStat1_STATTYPE=NONE; InherentStat1_MODTYPE=FLAT; InherentStat1_Value=0; InherentStat2_STATTYPE=NONE; InherentStat2_MODTYPE=FLAT; InherentStat2_Value=0; InherentStat3_STATTYPE=NONE; InherentStat3_MODTYPE=FLAT; InherentStat3_Value=0`.

Limites numéricos presentes: `GearKey`: 300001 a 639193; `BaseStat1_Value`: 0 a 21862; `BaseStat2_Value`: 0 a 180; `InherentStat1_Value`: 0 a 12930; `InherentStat2_Value`: 0 a 13992; `InherentStat3_Value`: 0 a 12930; `UniqueModKey`: 0 a 1.00101e+06.

### EventPeriodInfoData

Fonte D01: `sharedassets0.assets`, pathID `2498`; 1 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/EventPeriodInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`EventKey`, `StartDateUtc`, `EndDateUtc`, `IsEnabled`.

Exemplo da linha 2: `EventKey=50001; StartDateUtc=2026-09-18 00:00:00; EndDateUtc=2026-11-11 23:59:59; IsEnabled=True`.

Limites numéricos presentes: `EventKey`: 50001 a 50001.

### StatPoolInfoData

Fonte D01: `sharedassets0.assets`, pathID `2499`; 90 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StatPoolInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StatPoolKey`, `STATTYPE`, `MODTYPE`, `MinValue`, `MaxValue`, `Interval`, `Weight`.

Exemplo da linha 2: `StatPoolKey=1001; STATTYPE=AttackDamage; MODTYPE=ADDITIVE; MinValue=50; MaxValue=100; Interval=10; Weight=100`.

Limites numéricos presentes: `StatPoolKey`: 1001 a 1010; `MinValue`: 4 a 700; `MaxValue`: 8 a 800; `Interval`: 1 a 10; `Weight`: 100 a 100.

### CubeLevelInfoData

Fonte D01: `sharedassets0.assets`, pathID `2500`; 100 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CubeLevelInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Level`, `ExpForLevelUp`.

Exemplo da linha 2: `Level=1; ExpForLevelUp=15`.

Limites numéricos presentes: `Level`: 1 a 100; `ExpForLevelUp`: 15 a 1e+09.

### RelicInfoData

Fonte D01: `sharedassets0.assets`, pathID `2501`; 360 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/RelicInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemKey`, `RelicType`, `InherentStatModKey`, `InherentStatTier`, `AdditionalMod`.

Exemplo da linha 2: `ItemKey=710001; RelicType=Red; InherentStatModKey=108001; InherentStatTier=1`.

Limites numéricos presentes: `ItemKey`: 710001 a 759002; `InherentStatModKey`: 108001 a 108036; `InherentStatTier`: 1 a 10.

### AttunementCostInfoData

Fonte D01: `sharedassets0.assets`, pathID `2502`; 20 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/AttunementCostInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`AttunementCostKey`, `PARTS`, `GRADE`, `BaseCost`, `CostIncrease`, `MaxCount`.

Exemplo da linha 2: `AttunementCostKey=10101; PARTS=RELIQUARY; GRADE=COMMON; BaseCost=100; CostIncrease=120; MaxCount=19`.

Limites numéricos presentes: `AttunementCostKey`: 10101 a 21001; `BaseCost`: 50 a 1.22e+07; `CostIncrease`: 60 a 1.464e+07; `MaxCount`: 19 a 19.

### RuneInfoData

Fonte D01: `sharedassets0.assets`, pathID `2503`; 253 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/RuneInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`RuneKey`, `NameKey`, `MaxLevel`, `PrevNodeRequiredLevel`, `NextRuneKey`, `PreviewRuneKey`, `LevelDataKey`, `IconPath`.

Exemplo da linha 2: `RuneKey=1; NameKey=RuneName_AllHeroAttackDamage; MaxLevel=1; NextRuneKey=10 20; LevelDataKey=1; IconPath=AllHeroAttackDamage`.

Limites numéricos presentes: `RuneKey`: 1 a 1.902e+07; `MaxLevel`: 1 a 10; `PrevNodeRequiredLevel`: 1 a 5; `LevelDataKey`: 1 a 1.902e+07.

### SynthesisRecipeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2504`; 605 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SynthesisRecipeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SynthesisRecipeKey`, `RecipeTier`, `MinMaterialTier`, `MinResultLevel`, `MaxResultLevel`, `ItemSynthesisType`, `GRADE`, `MaterialAmount`, `MinMaterialAverageLevel`, `LevelWeight1`, `LevelWeight2`, `LevelWeight3`, `LevelWeight4`.

Exemplo da linha 2: `SynthesisRecipeKey=10100110; RecipeTier=1; MinMaterialTier=1; MinResultLevel=1; MaxResultLevel=1; ItemSynthesisType=Gear; GRADE=COMMON; MaterialAmount=9; MinMaterialAverageLevel=1; LevelWeight1=100; LevelWeight2=0; LevelWeight3=0; LevelWeight4=0`.

Limites numéricos presentes: `SynthesisRecipeKey`: 1.01001e+07 a 1.0501e+07; `RecipeTier`: 1 a 9; `MinMaterialTier`: 1 a 8; `MinResultLevel`: 1 a 90; `MaxResultLevel`: 1 a 90; `MaterialAmount`: 9 a 9; `MinMaterialAverageLevel`: 0 a 90; `LevelWeight1`: 50 a 900; `LevelWeight2`: 0 a 700; `LevelWeight3`: 0 a 300; `LevelWeight4`: 0 a 0.

### CurrencyInfoData

Fonte D01: `sharedassets0.assets`, pathID `2505`; 1 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CurrencyInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`CurrencyKey`, `CurrencyNameStringKey`, `Description`, `InitialAmount`, `IconPath`.

Exemplo da linha 2: `CurrencyKey=100001; CurrencyNameStringKey=CurrencyName_100001; Description=기본화폐; InitialAmount=100; IconPath=UI/Icon/Item/Item_100001`.

Limites numéricos presentes: `CurrencyKey`: 100001 a 100001; `InitialAmount`: 100 a 100.

### GearTypeScaleInfoData

Fonte D01: `sharedassets0.assets`, pathID `2506`; 20 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/GearTypeScaleInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`GearType`, `AlchemyGoldScale`, `CubeExpScale`, `ContaminScale`.

Exemplo da linha 2: `GearType=SWORD; AlchemyGoldScale=1000; CubeExpScale=1000; ContaminScale=1000`.

Limites numéricos presentes: `AlchemyGoldScale`: 700 a 4000; `CubeExpScale`: 700 a 4000; `ContaminScale`: 700 a 4000.

### StageInfoData

Fonte D01: `sharedassets0.assets`, pathID `2507`; 192 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StageInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StageKey`, `StageNameKey`, `PortalStageNameKey`, `STAGETYPE`, `STAGEDIFFICULITY`, `Act`, `StageNo`, `StageLevel`, `PrevStageKey`, `NextStageKey`, `WaveAmount`, `WaveMonsterAmount`, `Monsters`, `MonsterDropItemKey`, `MonsterDropItemRateM`, `BossDropItemKey`, `BossDropItemRateM`, `FirstClearDropKey`, `BossMonsterKey`, `BossDamageMultiplier`, `BossHpMultiplier`, `BossGoldMultiplier`, `BossExpMultiplier`, `BossScale`, `SoulstoneItemKey`, `SoulstoneAmount`, `IsDemo`, `BGMSoundKey`, `rewardString`.

Exemplo da linha 2: `StageKey=1101; StageNameKey=StageName_1101; STAGETYPE=NORMAL; STAGEDIFFICULITY=NORMAL; Act=1; StageNo=1; StageLevel=1; NextStageKey=1102; WaveAmount=10; WaveMonsterAmount=1; Monsters=10011_1000 10021_1000; MonsterDropItemKey=910011; MonsterDropItemRateM=160000; BossDropItemKey=920011; BossDropItemRateM=1000000; FirstClearDropKey=9200010; BossMonsterKey=10022; BossDamageMultiplier=2000; BossHpMultiplier=3000; BossGoldMultiplier=2000; BossExpMultiplier=3000; BossScale=3; IsDemo=True; BGMSoundKey=11011`.

Limites numéricos presentes: `StageKey`: 1101 a 201420; `Act`: 1 a 23; `StageNo`: 1 a 20; `StageLevel`: 1 a 95; `PrevStageKey`: 2110 a 4310; `NextStageKey`: 1102 a 201420; `WaveAmount`: 10 a 31; `WaveMonsterAmount`: 1 a 24; `MonsterDropItemKey`: 910011 a 940201; `MonsterDropItemRateM`: 600 a 160000; `BossDropItemKey`: 920011 a 941201; `BossDropItemRateM`: 20000 a 1e+06; `FirstClearDropKey`: 9.20001e+06 a 9.20052e+06; `BossMonsterKey`: 10022 a 5.0001e+06; `BossDamageMultiplier`: 2000 a 3000; `BossHpMultiplier`: 3000 a 6000; `BossGoldMultiplier`: 2000 a 3000; `BossExpMultiplier`: 3000 a 15000; `BossScale`: 3 a 3; `SoulstoneItemKey`: 190001 a 190104; `SoulstoneAmount`: 1 a 1; `BGMSoundKey`: 11011 a 13101.

### SkillInfoData

Fonte D01: `sharedassets0.assets`, pathID `2508`; 128 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SkillInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SkillKey`, `SkillNameKey`, `SkillDescriptionKey`, `ACTIVATIONTYPE`, `ActivationValue`, `SLOTTYPE`, `SkillTag`, `SkillBuffType`, `BuffGroupKey`, `Param1`, `Param2`, `Param3`, `Param4`, `Param5`, `Range`, `Order`, `DamageType`, `DamageDeliveryType`, `Value`, `SkillLevelKey`, `AnimClipPath1`, `AnimClipPath2`, `AnimClipPath3`, `AttributeKey`, `SoundKey`.

Exemplo da linha 2: `SkillKey=10001; SkillNameKey=SkillName_10001; ACTIVATIONTYPE=BASEATTACK; ActivationValue=0; SLOTTYPE=BASEATTACK; SkillTag=Attack; SkillBuffType=Normal; Param1=600; Param2=200; Param3=200; Range=140; Order=999; DamageType=Physical; DamageDeliveryType=Melee; Value=1000; AnimClipPath1=Animation/Skill/Skill_10001; AnimClipPath2=Animation/Skill/Skill_10001_2; AnimClipPath3=Animation/Skill/Skill_10001_3; SoundKey=1100011`.

Limites numéricos presentes: `SkillKey`: 10001 a 5.0001e+07; `ActivationValue`: 0 a 50; `BuffGroupKey`: 10501 a 2.20902e+06; `Param1`: 2 a 3500; `Param2`: 4 a 500; `Param3`: 3 a 200; `Param4`: 8 a 8; `Param5`: 235 a 235; `Range`: 120 a 1650; `Order`: 200 a 999; `Value`: 800 a 2300; `SkillLevelKey`: 10101 a 60601; `AttributeKey`: 101003 a 601043; `SoundKey`: 1.10001e+06 a 1.30905e+07.

### StatModInfoData

Fonte D01: `sharedassets0.assets`, pathID `2509`; 985 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StatModInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StatModKey`, `Tier`, `STATTYPE`, `MODTYPE`, `MinValue`, `MaxValue`, `Interval`.

Exemplo da linha 2: `StatModKey=100101; Tier=1; STATTYPE=AttackDamage; MODTYPE=FLAT; MinValue=1; MaxValue=2; Interval=1`.

Limites numéricos presentes: `StatModKey`: 100101 a 108036; `Tier`: 1 a 10; `MinValue`: 0 a 5200; `MaxValue`: 0 a 10000; `Interval`: 1 a 10.

### StatusEffectInfoData

Fonte D01: `sharedassets0.assets`, pathID `2510`; 6 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StatusEffectInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StatusEffectKey`, `StatusEffectType`, `Duration`, `BuffKeys`, `OverrideType`, `Param0`, `Param1`, `Param2`, `Param3`.

Exemplo da linha 2: `StatusEffectKey=101; StatusEffectType=Chill; Duration=400; BuffKeys=1011 1012; OverrideType=InitDuration`.

Limites numéricos presentes: `StatusEffectKey`: 101 a 106; `Duration`: 150 a 700; `Param0`: 30 a 800; `Param1`: 2 a 100.

### MaterialInfoData

Fonte D01: `sharedassets0.assets`, pathID `2511`; 189 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/MaterialInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemKey`, `MATERIALTYPE`, `StatModGroupKey`, `IsMaterialStashSlot`.

Exemplo da linha 2: `ItemKey=110001; MATERIALTYPE=DECORATION; StatModGroupKey=1100011; IsMaterialStashSlot=True`.

Limites numéricos presentes: `ItemKey`: 110001 a 190104; `StatModGroupKey`: 1.10001e+06 a 1.39001e+06.

### MonsterInfoData

Fonte D01: `sharedassets0.assets`, pathID `2512`; 81 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/MonsterInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`MonsterKey`, `MonsterNameStringKey`, `MONSTERTYPE`, `RewardGold`, `RewardExp`, `SkillKey`, `AttackDamage`, `AttackSpeed`, `MaxLife`, `MovementSpeed`, `DeadSoundKey`, `PrefabPath`, `AnimatorPath`.

Exemplo da linha 2: `MonsterKey=10011; MonsterNameStringKey=MonsterName_10011; MONSTERTYPE=MONSTER; RewardGold=10; RewardExp=10; SkillKey=100111; AttackDamage=10; AttackSpeed=40; MaxLife=50; MovementSpeed=110; DeadSoundKey=21001111; PrefabPath=Prefab/Monster/Monster_10011; AnimatorPath=Animation/Monster/10011/Monster_10011`.

Limites numéricos presentes: `MonsterKey`: 10011 a 5.0001e+06; `RewardGold`: 0 a 500; `RewardExp`: 0 a 46; `AttackDamage`: 8 a 406; `AttackSpeed`: 40 a 260; `MaxLife`: 15 a 1.3845e+06; `MovementSpeed`: 80 a 570; `DeadSoundKey`: 2.10011e+07 a 2.30111e+07.

### GradeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2513`; 10 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/GradeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`GRADE`, `Lower2GradeWeight`, `Lower1GradeWeight`, `SameGradeWeight`, `Higher1GradeWeight`, `Higher2GradeWeight`, `InherentSlotAmount`, `ExtraSlotAmount_Decoration`, `ExtraSlotAmount_Engraving`, `ExtraSlotAmount_Inscription`, `BaseAlchemyGold`, `BaseCubeExp`, `BaseContamin`, `BaseContamin_Material`, `SlotAmount_Reliquary`, `Relic_SameGradeWeight`, `Relic_Higher1GradeWeight`, `Relic_Higher2GradeWeight`.

Exemplo da linha 2: `GRADE=COMMON; Lower2GradeWeight=0; Lower1GradeWeight=0; SameGradeWeight=0; Higher1GradeWeight=100000; Higher2GradeWeight=5000; InherentSlotAmount=0; ExtraSlotAmount_Decoration=0; ExtraSlotAmount_Engraving=0; ExtraSlotAmount_Inscription=0; BaseAlchemyGold=10; BaseCubeExp=2; BaseContamin=1; BaseContamin_Material=2; SlotAmount_Reliquary=0; Relic_SameGradeWeight=0; Relic_Higher1GradeWeight=90000; Relic_Higher2GradeWeight=10000`.

Limites numéricos presentes: `Lower2GradeWeight`: 0 a 0; `Lower1GradeWeight`: 0 a 0; `SameGradeWeight`: 0 a 100000; `Higher1GradeWeight`: 0 a 160400; `Higher2GradeWeight`: 0 a 5000; `InherentSlotAmount`: 0 a 3; `ExtraSlotAmount_Decoration`: 0 a 2; `ExtraSlotAmount_Engraving`: 0 a 2; `ExtraSlotAmount_Inscription`: 0 a 2; `BaseAlchemyGold`: 10 a 355607; `BaseCubeExp`: 2 a 71089; `BaseContamin`: 1 a 500; `BaseContamin_Material`: 2 a 30; `SlotAmount_Reliquary`: 0 a 8; `Relic_SameGradeWeight`: 0 a 70000; `Relic_Higher1GradeWeight`: 0 a 95000; `Relic_Higher2GradeWeight`: 0 a 10000.

### SoundInfoData

Fonte D01: `sharedassets0.assets`, pathID `2514`; 324 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SoundInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SoundKey`, `SoundType`, `Volume`, `AudioClipPath`.

Exemplo da linha 2: `SoundKey=10001; SoundType=BGM; Volume=70; AudioClipPath=Sounds/BGM/title_bgm`.

Limites numéricos presentes: `SoundKey`: 10001 a 2.30901e+07; `Volume`: 15 a 70.

### CommonStatModInfoData

Fonte D01: `sharedassets0.assets`, pathID `2515`; 12 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CommonStatModInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`CommonStatModKey`, `STATTYPE`, `MODTYPE`, `Value`, `MaxValue`, `Interval`.

Exemplo da linha 2: `CommonStatModKey=4510001; STATTYPE=MaxHp; MODTYPE=FLAT; Value=1000`.

Limites numéricos presentes: `CommonStatModKey`: 4.51e+06 a 4.51001e+06; `Value`: 1 a 2000; `MaxValue`: 2 a 200; `Interval`: 1 a 1.

### StashInfoData

Fonte D01: `sharedassets0.assets`, pathID `2516`; 131 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/StashInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Index`, `CostForUnlock`.

Exemplo da linha 2: `Index=0; CostForUnlock=0`.

Limites numéricos presentes: `Index`: 0 a 130; `CostForUnlock`: 0 a 5.12e+06.

### ItemInfoData

Fonte D01: `sharedassets0.assets`, pathID `2517`; 7031 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ItemInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemKey`, `ITEMTYPE`, `GRADE`, `CONTENTTYPE`, `PARTS`, `GEARTYPE`, `GearGroup`, `ItemSynthesisType`, `NameKey`, `DescriptionKey`, `GearKey`, `DropKey`, `DropCooldown`, `Level`, `ContaminOverride`, `MaxStack`, `IsSteamItem`, `IconPath`, `IsBucketBox`, `IsDeletedInServer`, `IsCanExchangeMarketable`, `IsTempMarketableFalse`.

Exemplo da linha 2: `ItemKey=910011; ITEMTYPE=STAGEBOX; GRADE=COMMON; NameKey=Normal Monster Box 1; DropKey=9100111; DropCooldown=120; IsSteamItem=True; IconPath=Item_910011; IsBucketBox=True`.

Limites numéricos presentes: `ItemKey`: 110001 a 941201; `GearKey`: 300001 a 639193; `DropKey`: 9.10011e+06 a 9.41201e+06; `DropCooldown`: 3 a 3600; `Level`: 1 a 100; `ContaminOverride`: 4 a 128; `MaxStack`: 5 a 999.

### ItemGroupInfoData

Fonte D01: `sharedassets0.assets`, pathID `2518`; 2922 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ItemGroupInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ItemGroupKey`, `GroupName`, `ItemKey`.

Exemplo da linha 2: `ItemGroupKey=1000010; GroupName=커먼 주무기 1레벨; ItemKey=300001`.

Limites numéricos presentes: `ItemGroupKey`: 170500 a 1.9019e+08; `ItemKey`: 110001 a 799063.

### CubeRecipeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2519`; 10 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CubeRecipeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`CubeKey`, `RECIPETYPE`, `Index`, `IsDefaultUnlocked`, `TooltipStringKey`.

Exemplo da linha 2: `CubeKey=100001; RECIPETYPE=SYNTHESIS; Index=0; IsDefaultUnlocked=True; TooltipStringKey=Cube_RecipeTooltip_Synthesis`.

Limites numéricos presentes: `CubeKey`: 100001 a 900001; `Index`: 0 a 9.

### HeroInfoData

Fonte D01: `sharedassets0.assets`, pathID `2520`; 6 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/HeroInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`HeroKey`, `HeroNameKey`, `DescriptionKey`, `ClassType`, `MainWeaponGearType`, `SubWeaponGearType`, `SkillKey`, `AttackDamage`, `AttackSpeed`, `CastSpeed`, `CriticalChance`, `CriticalDamage`, `MaxHp`, `Armor`, `CooldownReduction`, `MovementSpeed`, `UnlockCost`, `IsMeleeHero`, `IsAvailable`, `IsFirstAvailable`, `SelectSoundKey`, `DeadSoundKey`, `DLCAppId`, `DLCBitIndex`, `HasDLCDrop`, `IconPath`, `DeadIconPath`, `PrefabPath`, `AnimatorPath`.

Exemplo da linha 2: `HeroKey=101; HeroNameKey=HeroName_101; DescriptionKey=HeroDescription_101; ClassType=Knight; MainWeaponGearType=SWORD; SubWeaponGearType=SHIELD; SkillKey=10001; AttackDamage=2; AttackSpeed=80; CastSpeed=100; CriticalChance=25; CriticalDamage=1400; MaxHp=130; Armor=45; CooldownReduction=0; MovementSpeed=950; UnlockCost=500; IsMeleeHero=True; IsAvailable=True; IsFirstAvailable=True; SelectSoundKey=30501; DeadSoundKey=2100011; DLCAppId=0; DLCBitIndex=0; HasDLCDrop=False; IconPath=UI/Icon/Hero/Hero_101; DeadIconPath=UI/Icon/Hero/HeroDead_101; PrefabPath=Prefab/Hero/Hero_101; AnimatorPath=Animation/Hero/101/Hero_101`.

Limites numéricos presentes: `HeroKey`: 101 a 601; `SkillKey`: 10001 a 60001; `AttackDamage`: 1 a 2; `AttackSpeed`: 50 a 100; `CastSpeed`: 100 a 100; `CriticalChance`: 20 a 50; `CriticalDamage`: 1400 a 1800; `MaxHp`: 50 a 130; `Armor`: 5 a 45; `CooldownReduction`: 0 a 0; `MovementSpeed`: 770 a 950; `UnlockCost`: 500 a 500; `SelectSoundKey`: 30501 a 30506; `DeadSoundKey`: 2.10001e+06 a 2.30001e+06; `DLCAppId`: 0 a 4.42743e+06; `DLCBitIndex`: 0 a 1.

### OfflineRewardInfoData

Fonte D01: `sharedassets0.assets`, pathID `2521`; 116 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/OfflineRewardInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StageLevel`, `BaseGold`, `BaseExp`, `KillCount`, `ClearCount`.

Exemplo da linha 2: `StageLevel=1; BaseGold=1; BaseExp=1; KillCount=180; ClearCount=20`.

Limites numéricos presentes: `StageLevel`: 1 a 116; `BaseGold`: 1 a 1110; `BaseExp`: 1 a 21898; `KillCount`: 180 a 6300; `ClearCount`: 10 a 20.

### UniqueModInfoData

Fonte D01: `sharedassets0.assets`, pathID `2522`; 132 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/UniqueModInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`UniqueModKey`, `UniqueMod`, `Param1ExchangeType`, `Param1`, `Param1_Max`, `Param1_Interval`, `Param2ExchangeType`, `Param2`, `Param2_Max`, `Param2_Interval`, `Param3ExchangeType`, `Param3`, `Param3_Max`, `Param3_Interval`, `Param4ExchangeType`, `Param4`, `Param4_Max`, `Param4_Interval`, `Param5ExchangeType`, `Param5`, `Param5_Max`, `Param5_Interval`.

Exemplo da linha 2: `UniqueModKey=10001; UniqueMod=ShieldChargeKillCooldown`.

Limites numéricos presentes: `UniqueModKey`: 10001 a 1.00204e+06; `Param1_Max`: 2 a 5000; `Param1_Interval`: 1 a 100; `Param2_Max`: 500 a 500; `Param2_Interval`: 10 a 10.

### AttributeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2523`; 132 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/AttributeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`AttributeKey`, `HeroKey`, `GroupKey`, `ATTRIBUTETYPE`, `Value`, `RequiredPoint`, `MaxLevel`, `AvailableDemo`.

Exemplo da linha 2: `AttributeKey=101001; HeroKey=101; GroupKey=10001; ATTRIBUTETYPE=PASSIVESKILL; Value=101001; RequiredPoint=1; MaxLevel=3; AvailableDemo=TRUE`.

Limites numéricos presentes: `AttributeKey`: 101001 a 601072; `HeroKey`: 101 a 601; `GroupKey`: 10001 a 10008; `Value`: 10101 a 601072; `RequiredPoint`: 1 a 1; `MaxLevel`: 3 a 10.

### TradingStashInfoData

Fonte D01: `sharedassets0.assets`, pathID `2524`; 10 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/TradingStashInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Index`, `CostForUnlock`.

Exemplo da linha 2: `Index=0; CostForUnlock=0`.

Limites numéricos presentes: `Index`: 0 a 9; `CostForUnlock`: 0 a 0.

### SkillLevelInfoData

Fonte D01: `sharedassets0.assets`, pathID `2525`; 360 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SkillLevelInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SkillLevelKey`, `Level`, `Value`.

Exemplo da linha 2: `SkillLevelKey=10101; Level=1; Value=2500`.

Limites numéricos presentes: `SkillLevelKey`: 10101 a 60601; `Level`: 1 a 10; `Value`: 3 a 9800.

### SkinInfoData

Fonte D01: `sharedassets0.assets`, pathID `2526`; 18 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SkinInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SkinKey`, `SetId`, `IsDefault`, `IsFree`.

Exemplo da linha 2: `SkinKey=25101001; IsDefault=True`.

Limites numéricos presentes: `SkinKey`: 2.5101e+07 a 2.5601e+07; `SetId`: 2.5001e+07 a 2.5001e+07.

### ExtractionCostInfoData

Fonte D01: `sharedassets0.assets`, pathID `2527`; 90 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ExtractionCostInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ExtractionKey`, `GearGroup`, `MATERIALTYPE`, `Tier`, `Cost`.

Exemplo da linha 2: `ExtractionKey=10101; GearGroup=WEAPON; MATERIALTYPE=DECORATION; Tier=1; Cost=100`.

Limites numéricos presentes: `ExtractionKey`: 10101 a 30310; `Tier`: 1 a 10; `Cost`: 100 a 200000.

### CraftingRecipeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2528`; 63 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CraftingRecipeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`CraftingRecipeKey`, `ItemCraftingType`, `RecipeTier`, `Material`, `MaterialIndex`, `DropKey`.

Exemplo da linha 2: `CraftingRecipeKey=6001001; ItemCraftingType=MainWeapon; RecipeTier=1; Material=140003_1; MaterialIndex=0; DropKey=3601001`.

Limites numéricos presentes: `CraftingRecipeKey`: 6.001e+06 a 6.00901e+06; `RecipeTier`: 1 a 9; `DropKey`: 3.601e+06 a 3.60901e+06.

### SynthesisDropInfoData

Fonte D01: `sharedassets0.assets`, pathID `2529`; 239 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/SynthesisDropInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`SynthesisDropKey`, `ItemLevel`, `RecipeTier`, `ItemSynthesisType`, `GRADE`, `DropKey`.

Exemplo da linha 2: `SynthesisDropKey=10010001; ItemLevel=1; RecipeTier=1; ItemSynthesisType=Gear; GRADE=UNCOMMON; DropKey=3111011`.

Limites numéricos presentes: `SynthesisDropKey`: 1.001e+07 a 5.01e+07; `ItemLevel`: 1 a 90; `RecipeTier`: 1 a 9; `DropKey`: 3.11101e+06 a 3.70511e+06.

### PartyModInfoData

Fonte D01: `sharedassets0.assets`, pathID `2530`; 63 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PartyModInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PartyModKey`, `PartyModType`, `PartyMod`, `Param1ExchangeType`, `Param1`, `Param1_Max`, `Param1_Interval`, `Param2ExchangeType`, `Param2`, `Param2_Max`, `Param2_Interval`, `Param3ExchangeType`, `Param3`, `Param3_Max`, `Param3_Interval`, `Param4ExchangeType`, `Param4`, `Param4_Max`, `Param4_Interval`.

Exemplo da linha 2: `PartyModKey=20101; PartyModType=UniqueMod; PartyMod=None; Param1ExchangeType=Divided; Param1=1001010`.

Limites numéricos presentes: `PartyModKey`: 20101 a 20520; `Param1`: 1 a 1.00204e+06; `Param1_Max`: 80 a 700; `Param1_Interval`: 10 a 10; `Param2`: 1 a 2000; `Param2_Max`: 1 a 1000; `Param2_Interval`: 1 a 10; `Param3`: 30 a 60; `Param3_Max`: 50 a 100; `Param3_Interval`: 1 a 1.

### PetInfoData

Fonte D01: `sharedassets0.assets`, pathID `2531`; 10 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PetInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PetKey`, `NameKey`, `DescriptionKey`, `StatDataKey`, `UnlockCondition`, `Param1`, `Param2`, `EventKey`.

Exemplo da linha 2: `PetKey=1001; NameKey=PetName_1001; DescriptionKey=PetDescription_1001; StatDataKey=1001; UnlockCondition=KillMonster; Param1=10031; Param2=5000`.

Limites numéricos presentes: `PetKey`: 1001 a 6003; `StatDataKey`: 1001 a 6003; `Param1`: 10031 a 5.0001e+06; `Param2`: 500 a 5000; `EventKey`: 50001 a 50001.

### PetStatInfoData

Fonte D01: `sharedassets0.assets`, pathID `2532`; 13 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PetStatInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PetStatKey`, `STATTYPE`, `MODTYPE`, `Value`.

Exemplo da linha 2: `PetStatKey=1001; STATTYPE=DropChanceNormalChestPercent; MODTYPE=FLAT; Value=100`.

Limites numéricos presentes: `PetStatKey`: 1001 a 6003; `Value`: 10 a 200.

### ResonanceInfoData

Fonte D01: `sharedassets0.assets`, pathID `2533`; 63 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ResonanceInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`ResonanceKey`, `Condition`, `PartyModKey`.

Exemplo da linha 2: `ResonanceKey=20101; Condition=Blue_2; PartyModKey=20101`.

Limites numéricos presentes: `ResonanceKey`: 20101 a 20520; `PartyModKey`: 20101 a 20520.

### ItemLevelScaleInfoData

Fonte D01: `sharedassets0.assets`, pathID `2534`; 19 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/ItemLevelScaleInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Level`, `AlchemyGoldScale`, `CubeExpScale`, `ContaminScale`.

Exemplo da linha 2: `Level=1; AlchemyGoldScale=1000; CubeExpScale=1000; ContaminScale=1000`.

Limites numéricos presentes: `Level`: 1 a 90; `AlchemyGoldScale`: 1000 a 1.227e+06; `CubeExpScale`: 1000 a 3.06e+06; `ContaminScale`: 1000 a 1000.

### LevelInfoData

Fonte D01: `sharedassets0.assets`, pathID `2535`; 100 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/LevelInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`Level`, `ExpForLevelUp`.

Exemplo da linha 2: `Level=1; ExpForLevelUp=30`.

Limites numéricos presentes: `Level`: 1 a 100; `ExpForLevelUp`: 30 a 1.99777e+09.

### EventStageInfoData

Fonte D01: `sharedassets0.assets`, pathID `2536`; 3 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/EventStageInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`StageKey`, `EventKey`, `UnlockStageKey`, `EntryTicketItemKey`, `EntryTicketAmount`, `MaxBoxDrop`.

Exemplo da linha 2: `StageKey=2901; EventKey=50001; UnlockStageKey=2310; EntryTicketItemKey=170101; EntryTicketAmount=1; MaxBoxDrop=10`.

Limites numéricos presentes: `StageKey`: 2901 a 4901; `EventKey`: 50001 a 50001; `UnlockStageKey`: 2310 a 4310; `EntryTicketItemKey`: 170101 a 170103; `EntryTicketAmount`: 1 a 1; `MaxBoxDrop`: 10 a 10.

### CubeSubRecipeInfoData

Fonte D01: `sharedassets0.assets`, pathID `2537`; 89 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/CubeSubRecipeInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`CubeSubRecipeKey`, `RECIPETYPE`, `RecipeTier`, `DefaultUnlockWhenMainRecipeOpen`, `UnlockCubeLevel`, `UnlockCost`, `Material`, `TriggerGoldCost`, `DropKey`, `SubRecipeNameStringKey`.

Exemplo da linha 2: `CubeSubRecipeKey=100011; RECIPETYPE=SYNTHESIS; RecipeTier=1; DefaultUnlockWhenMainRecipeOpen=True; UnlockCubeLevel=1; UnlockCost=0; SubRecipeNameStringKey=SubRecipeName_100011`.

Limites numéricos presentes: `CubeSubRecipeKey`: 100011 a 900011; `RecipeTier`: 1 a 9; `UnlockCubeLevel`: 1 a 90; `UnlockCost`: 0 a 50000; `Material`: 1.60001e+06 a 1.6211e+06; `TriggerGoldCost`: 10 a 910; `DropKey`: 3.7e+06 a 3.71e+06.

### BuffInfoData

Fonte D01: `sharedassets0.assets`, pathID `2538`; 31 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/BuffInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`BuffKey`, `BuffType`, `STATTYPE`, `MODTYPE`, `Value`.

Exemplo da linha 2: `BuffKey=1011; BuffType=Debuff; STATTYPE=AttackSpeed; MODTYPE=MULTIPLICATIVE; Value=300`.

Limites numéricos presentes: `BuffKey`: 1011 a 2.20902e+07; `Value`: 20 a 1000.

### PlagueIntensityInfoData

Fonte D01: `sharedassets0.assets`, pathID `2539`; 60 registros. CSV suplementar local: `artifacts/taskbar-hero-rea/current/tables/PlagueIntensityInfoData.csv`. Esquema versionado em [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json).

Campos, na ordem original:

`PlagueIntensity`, `STATTYPE`, `MODTYPE`, `Value`.

Exemplo da linha 2: `PlagueIntensity=1; STATTYPE=MovementSpeed; MODTYPE=ADDITIVE; Value=600`.

Limites numéricos presentes: `PlagueIntensity`: 1 a 20; `Value`: 50 a 405000.

## Localização e resolução de chaves

Evidência **L01**, com [resumo portátil](evidencias/Localizacao_Resumo.json). O Unity Localization separa o identificador numérico da chave humana. A rotina leu `m_TableData` das StringTables e resolveu `m_Id` em `m_Entries` de SharedTableData. A ligação usa `m_SharedData.m_PathID`; o vínculo não foi inferido apenas pelo nome do arquivo.

| Coleção | Fonte/objeto | Registros | Sem chave no SharedTableData |
|---|---|---:|---:|
| ItemTable_en-US | `localization-string-tables-english(unitedstates)(en-us)_assets_all.bundle`, pathID `-3122350057523193882` | 1504 | 0 |
| StringTable_en-US | `localization-string-tables-english(unitedstates)(en-us)_assets_all.bundle`, pathID `7152259898081180557` | 1793 | 65 |
| StringTable_pt-BR | `localization-string-tables-portuguese(brazil)(pt-br)_assets_all.bundle`, pathID `4428710360693310810` | 1775 | 47 |
| ItemTable_pt-BR | `localization-string-tables-portuguese(brazil)(pt-br)_assets_all.bundle`, pathID `8769652279597820803` | 1508 | 4 |

As coleções compartilhadas vêm de `localization-assets-shared_assets_all.bundle`: `ItemTable Shared Data`, pathID `-1933338285342846232`; `StringTable Shared Data`, pathID `6792816966776794831`. Os bundles locais ficam em `TaskBarHero_Data/StreamingAssets/aa/StandaloneWindows64/`.

Os JSON individuais em `artifacts/taskbar-hero-rea/current/localization/` preservam IDs, texto e chave nula para entradas sem correspondência. `en-US.keys.json` e `pt-BR.keys.json` são mapas convenientes apenas das chaves resolvidas. Uma chave ausente nesse mapa não comprova ausência de tradução em tempo de execução: pode existir texto órfão, fallback, geração dinâmica ou outro recurso.

Os textos são evidência do que o jogo descreve, não prova de que cada regra esteja executada. Exemplos úteis: `Tutorial_CanLearnSkill`, `Tutorial_EquipGear`, `UI_Status_DetailedStats`, `NormalAttackDPS`, `Cube_RecipeTooltip_*`, `Tooltip_Stat_*`, `AccountStat_*`, `PartyMod_*` e `UniqueMod_*`.

## Cuidado com contagens e relações

- `DropInfoData`: 20.311 linhas agrupadas em 380 `DropKey`; não são 20.311 sorteios independentes. `REWARDTYPE` determina se `RewardKey` aponta a um item ou grupo.
- `ItemGroupInfoData`: várias linhas por `ItemGroupKey`; preservar agrupamento e ordem.
- `RuneLevelInfoData`: chave composta `LevelKey + Level`; `StatModInfoData`: `StatModKey + Tier`; `SkillLevelInfoData`: `SkillLevelKey + Level`.
- `BuffGroupInfoData`: `BuffGroupKey=60601` ocorre duas vezes, uma com `606011` e outra vazia. O tratamento de duplicidade depende do carregador e não foi estabelecido.
- Todas as 7.031 linhas de `ItemInfoData` têm `IsSteamItem=True`. Há 4.364 com `IsDeletedInServer=True`, 1.113 com `IsCanExchangeMarketable=True`, 50 com `IsTempMarketableFalse=True`. Esses campos não validam disponibilidade nem transação atual na Steam.
- `StageLevelInfoData` contém níveis 1–170, mas as 192 fases de `StageInfoData` referem `StageLevel` 1–95. `OfflineRewardInfoData` cobre 1–116. A existência de linhas adicionais não prova conteúdo alcançável.
- `LevelInfoData` e `CubeLevelInfoData` contêm linhas 1–100. A linha 100 ainda tem `ExpForLevelUp`; o limite efetivo de nível precisa do código ou execução.

## Reprodução

Na raiz do repositório, com o ambiente já preparado:

```powershell
.\.tools\taskbar-hero-rea\venv\Scripts\python.exe -X utf8 scripts\research\Inspect-TaskbarHero-Data.py --installation 'C:\Program Files (x86)\Steam\steamapps\common\TaskbarHero' --output artifacts\taskbar-hero-rea\current --evidence docs\referencias\taskbar-hero\evidencias
```

A rotina requer Python com UnityPy 1.24.2, não instala dependências e só grava nos diretórios de saída explícitos. Os dois resumos portáteis contêm metadados; a extração completa fica fora do runtime do Aetherforge e do versionamento. O caminho de instalação pode ser substituído pelo local escolhido pelo leitor.
