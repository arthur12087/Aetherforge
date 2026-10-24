# Taskbar Hero — arquitetura recuperada dos metadados

Análise estática da instalação local em 23/10/2026. Este documento descreve declarações presentes no produto distribuído. Não é código-fonte recuperado e não demonstra a execução dos métodos. Os nomes preservados permitem orientar a leitura das telas e dos dados; vários outros nomes foram ofuscados.

## Evidência e reprodução

**M01** designa o inventário local de `TaskBarHero_Data/il2cpp_data/Metadata/global-metadata.dat` e suas declarações verificadas pelo parser [Inspect-TaskbarHero-Metadata.py](../../../scripts/research/Inspect-TaskbarHero-Metadata.py). O recorte portátil versionado está em [Metadados_Resumo.json](evidencias/Metadados_Resumo.json). As referências `M01:T1723`, por exemplo, identificam o índice global do tipo no inventário, não um endereço de código.

| Referência | Arquivo produzido em `artifacts/taskbar-hero-rea/current/metadata/` | Conteúdo e limite |
|---|---|---|
| M01 | `inventory.json` | Identidade, tabelas, contagens, verificações e limites do parser |
| M01:Tn | `declarations-game.json` | Todos os 2.210 tipos de `Assembly-CSharp.dll`, com nomes, campos, métodos, parâmetros, tokens e offsets no arquivo de metadados |
| M01:Foco | `declarations-focus.json`, `declarations-focus.txt` | Recorte por nomes relacionados a status, heróis, combate, offline, saves e janela; a ausência neste recorte não significa ausência no jogo |
| M02 | `scripting-assemblies.json` | Manifesto `ScriptingAssemblies.json` da instalação |
| M03 | `runtime-initializers-game.json` | Entradas do jogo em `RuntimeInitializeOnLoads.json` |
| M04 | `plugins.json`, `boot-config.json` | Inventário dos arquivos de plugins e configurações `app.info`/`boot.config` |

```powershell
.\.tools\taskbar-hero-rea\venv\Scripts\python.exe .\scripts\research\Inspect-TaskbarHero-Metadata.py
```

O script também aceita `--data-dir`, `--output` e `--summary docs/referencias/taskbar-hero/evidencias/Metadados_Resumo.json` para regenerar o recorte portátil. Usa apenas a biblioteca padrão do Python. Faz leitura da instalação e grava resultados no repositório; não executa o jogo, não acessa saves e não consulta serviços. A pasta `artifacts/` contém evidências locais regeneráveis e pode estar excluída pelo `.gitignore`.

Identidade de M01:

- Tamanho: **23.036.032 bytes**.
- SHA-256: `85bf394366ce9c6c6969be9731131d5bc87c80bd94fbed3e54b8f50ca785e622`.
- Assinatura IL2CPP: `0xFAB11BAF`; versão de metadados **31**.
- 124 imagens de assemblies no metadata; 24.470 tipos, 195.337 métodos, 109.462 campos e 211.314 parâmetros no conjunto inteiro, incluindo bibliotecas e engine.
- `Assembly-CSharp.dll` ocupa os índices de tipo **0 a 2209**. A imagem declarada não significa que uma DLL CIL correspondente esteja disponível no disco: IL2CPP conserva os metadados e compila os corpos para código nativo.

O parser verifica os limites de todas as tabelas do cabeçalho, divisibilidade dos registros utilizados, decodificação dos nomes, tokens de tipo/campo/método/parâmetro, propriedade de cada método pelo tipo e intervalos de parâmetros. As verificações passam para todo o conjunto: 24.470 tipos, 195.337 métodos e 109.462 campos. Isso sustenta a interpretação das declarações, mas não substitui a resolução dos tipos nativos e a análise dos corpos.

## Tecnologia e organização

O produto distribuído utiliza Unity com IL2CPP. O manifesto inclui `Assembly-CSharp`, componentes Unity UI/TextMeshPro, Input System, Addressables, Localization, URP, Cinemachine, UniTask, DOTween e Newtonsoft.Json. Também declara Steamworks.NET, Heathen.Steamworks, Backend e componentes de serialização/obfuscação. **A inclusão de uma biblioteca no build não prova que toda a sua funcionalidade esteja ativa.** [M01, M02]

Os plugins inventariados são `steam_api64.dll`, `lib_burst_generated.dll`, `libonigwrap.dll` e `Steamworks.NET.txt`. `app.info` identifica `TesseractStudio` e `TaskBarHero`. O `boot.config` contém configuração de rendering/threading, `hdr-display-enabled=0`, `gc-max-time-slice=3` e `force-d3d11-bitblt-model=`. O último valor não permite concluir sozinho como a transparência da janela é implementada. [M04]

| Área | Declarações encontradas | O que a evidência estabelece |
|---|---|---|
| Apresentação principal | `UIManager`, `UI_Hero`, `UI_Status`, `UI_Stage`, `UI_Stash`, `UI_Setting` | Existem controladores separados por telas e funções |
| Mundo e unidades | `StageManager`, `Hero`, `Monster`, `Unit`, `CameraFollower` | Há declarações específicas para palco, heróis, monstros e representação de unidades |
| Regras de combate | namespace `TaskbarHero.Combat`, `ActiveSkill`, `SkillBehaviorContext`, `DamageInfo` | O build conserva tipos que representam habilidades, contexto e dano |
| Status | `StatType`, `AccountStatus`, `StatViewInfoData`, `EStatViewSection` | Status individuais, status de conta e organização de exibição têm declarações distintas |
| Dados de conteúdo | namespace `TaskbarHero.Data`, `HeroInfoData`, `SkillInfoData`, classes `*InfoDataMap` | Conteúdo possui estruturas de dados próprias |
| Persistência | `AccountSaveData`, `PlayerSaveData`, `SettingSaveData`, namespace `EasySaveData` | Existem modelos separados para conta, progresso, configurações e dados de entidades |
| Desktop | `WindowManager`, `EWindowLayoutPreset`, `EWindowZoomRate` | O domínio da janela possui controlador, eventos e opções próprias |

Essa é uma organização de declarações. Não foram recuperadas dependências de chamadas entre esses controladores; o fluxo exato entre camadas permanece aberto. [M01]

## Menu de status: estrutura recuperada

`TaskbarHero.UI.UI_Status` é o tipo **1723**, token `0x020006ba`, registro em `0x120a6b4` do arquivo de metadados. Possui **107 campos e 60 métodos**. A maior parte dos métodos está ofuscada; campos de UI preservam muitos nomes úteis. [M01:T1723]

| Bloco | Campos declarados que o sustentam | Interpretação permitida |
|---|---|---|
| Identificação | `text_HeroName`, `text_HeroLevel`, `slider_Exp`, `text_HeroAbilityPoint` | Há componentes para nome, nível, barra de EXP e pontos |
| Resumo | `text_DefaultStat_DPS`, `text_DefaultStat_AttackDmg`, `m_hpStatText`, `text_DefaultStat_MaxHp`, `text_DefaultStat_AttackSpeed`, `text_DefaultStat_CriChance`, `text_DefaultStat_CriDmg`, `text_DefaultStat_CoolTimeReduction`, `text_DefaultStat_MoveSpeed`, `text_DefaultStat_Armor`, `text_DefaultStat_CastSpeed` | A tela possui referências para um resumo de atributos de combate |
| Detalhe ofensivo | Campos `text_Detail_*` para dano, velocidades, crítico, reduções, alcance, elementos, multistrike, projéteis e bônus de estilos | O modelo da UI comporta um conjunto ofensivo detalhado |
| Detalhe defensivo | HP, armadura, resistências, esquiva, bloqueio, regeneração, absorção e redução de dano | O modelo da UI comporta um conjunto defensivo detalhado |
| Recuperação e utilidade | Movimento, área, duração, vida por acerto/abate, roubo de vida, cura, níveis de skill e EXP | Há campos além do ataque/defesa básicos |
| Ajuda contextual | `m_tooltip_Armor`, tooltips de resistências, esquiva, bloqueio, cooldown e redução de dano | Existem referências específicas para explicações de atributos |
| Progressão | `text_AttributeName`, `text_AttributeLevel`, `text_RequiredPoint`, `text_AttributeDescription`, `image_AttributeIcon`, `slider_AllocatePoint`, `text_AllocatePoint` | O painel declara apresentação de atributo e seleção de quantidade de pontos |
| Skills | `attributeLevelSlotParent`, `attributeLevelSlotPrefab`, `activeSkillSlotDetail`, `activeSkillEquipPanel` | Slots e painel de equipamento de habilidades fazem parte dessa tela |
| Equipe | `_teamStatPanel`, `_teamStatusButton` | Há ligação da UI de status a componentes de equipe |
| Cores | `_defaultStatColor`, `_buffStatColor`, `_debuffStatColor` | Três referências distintas de cor foram preservadas; seus valores e aplicação exigem outra evidência |
| Eventos e ciclo | `OnShowStatusUI`; métodos `OnEnable`, `OnDisable`, `OnDestroy`, `LateUpdate` | Existem evento nominal e métodos de ciclo Unity; a frequência real de atualização de cada valor é desconhecida |

O enum `EStatusUIState`, tipo **55**, declara `Default`, `StatDetail`, `AttributeDetail`, `SkillEquip`, `SkillSlot`. Isso fornece nomes de estados contemplados pelo código; não prova todas as transições nem quais estão acessíveis na versão instalada. [M01:T55]

`TeamStatPanel`, tipo **48**, contém `_teamMoveSpeed`. `SkillStatusDescriptionPanel`, tipo **1689**, declara versões normal, detalhada e passiva, com ícone, nível, descrição, nome, cooldown e DPS. `Show(attributeCache, slotRectTransform, showOnRight)` e `Hide()` são métodos nomeados preservados; a posição final do popup e as condições de abertura requerem cena ou observação. [M01:T48, T1689]

### Exibição de atributos orientada por dados

`StatViewInfoData`, tipo **1958**, tem campos `StatType`, `SectionType`, `Order`, `IsShowStatus`, `IsShowZero`. `EStatViewSection`, tipo **1957**, declara `None`, `Offense`, `Defense`, `Misc`. Há, portanto, uma estrutura de conteúdo capaz de classificar, ordenar e configurar a apresentação de cada atributo. **Não foi demonstrado pelo metadata que `UI_Status` lê essa estrutura em todo o seu fluxo.** [M01:T1957–1958]

O enum `StatType`, tipo **1085**, contém 83 campos: um campo interno `value__`, `NONE` e **81 nomes de atributos**. Alguns exemplos:

- Base: `AttackDamage`, `AttackSpeed`, `CriticalChance`, `CriticalDamage`, `MaxHp`, `Armor`, `MovementSpeed`, `CastSpeed`.
- Skills: `CooldownReduction`, `SkillRangeExpansion`, `BaseAttackCountReduction`, `AddAllSkillLevel`, `SkillDurationIncrease`, `SkillHealIncrease`.
- Mitigação: resistências Fire/Cold/Lightning/Chaos, valores máximos de resistências, esquiva/bloqueio físicos e elementais, absorção e reduções.
- Dano: tipos físicos/elementais, adições, bônus de Melee/Projectile/AOE/Summon, multistrike, projéteis adicionais, bônus contra boss e condições de HP.
- Recuperação/progressão: regeneração por segundo, vida por hit/kill, leech, EXP e ouro.

Nomes `PoisonResistance` aparecem em campos de UI, enquanto `ChaosResistance` aparece em `StatType`. A equivalência semântica entre esses termos não pode ser assumida apenas pelos nomes. Também não se deve supor que todo atributo do enum seja mostrado, nem que todo campo de tela esteja preenchido. [M01:T1085, T1723]

## Heróis, habilidades e combate

`HeroInfoData`, tipo **1841**, conserva 29 campos: identificação/localização, classe, tipos de armas principal/secundária, skill, atributos base, custo de desbloqueio, flags de disponibilidade/melee, sons, ícones, prefab/animação e relação nominal com DLC. Esses campos definem o contrato de dados; valores concretos e curvas de crescimento pertencem à análise dos recursos. [M01:T1841]

`Hero`, tipo **635**, possui 96 métodos e 31 campos, incluindo `HeroHud`, efeitos de level-up/skin e `ReviveDelayTime`/`ReviveTime`. `Unit`, tipo **645**, preserva referências nominais a `UnitHealthController`, `AnimationController`, `StatusEffectManager`, `BuffManager`, `EffectController`, estado de unidade/ataque e eventos de morte, mudança de estado, skill e revive. Essa composição sugere separação de responsabilidades, mas sem tipos nativos resolvidos não se estabelece o tipo concreto de cada campo ou quem invoca cada operação. [M01:T635, T645]

Enums preservados:

| Tipo | Membros de conteúdo, excluindo `value__` |
|---|---|
| `EUNITSTATE` (11) | `NONE`, `IDLE`, `MOVE`, `RETURN_TO_POSITION`, `ATTACK`, `REVIVE`, `DIE` |
| `EATTACKSTATE` (12) | `PREDELAY`, `ACTIONTIME`, `POSTDELAY` |
| `EDamageAttribute` (1919) | `Physical`, `Fire`, `Cold`, `Lightning`, `Chaos`, `AllElement`, `None` |
| `EDamageType` (1920) | `None`, `Melee`, `Projectile`, `AOE`, `Summon`, `DOT`, `Trap` |
| `EDamageSource` (642) | `Direct`, `StatusEffect`, `Reflect`, `SpecialDamage`, `ExtraDamage` |
| `ESkillTag` (1921) | `None`, `Attack`, `Buff`, `Heal`, `Revive`, `Movement`, `Summon`, `Trap`, `Zone` |
| `StatusEffectType` (1373) | `Chill`, `Freeze`, `Ignite`, `Shock`, `Bleed`, `Stun` |
| `BuffExpireConditionType` (1382) | `Time`, `BaseAttackCount`, `Stage` |
| `BuffType` (1383) | `Buff`, `Debuff` |
| `BuffStackType` (1384) | `OnlyOne`, `Stackable` |

Esses nomes sustentam a existência de categorias e estados no build, sem demonstrar fórmulas, chances, limites numéricos ou transições. [M01:T11–12, T642, T1373, T1382–1384, T1919–1921]

`DamageInfo`, tipo **643**, conserva `Attacker`, `OriginDamage`, `IsCritical`, `CanCritical`, `DamageAttribute`, `DamageType`, `DamageSource`, flags de texto/feedback/som, efeitos, dano redistribuído e ataque básico. `ActiveSkill`, tipo **2125**, declara 69 métodos e campos de conteúdo/cache/distância/animação. Seu método `AttackDamage` está nomeado; o corpo não foi recuperado. `SkillBehaviorContext`, tipo **2132**, declara `SkillOwner`, `MaxCoolDown`, `ActivationValue`, `SkillCache`, `ActiveSkill` e `PlayerNormalAttackCountWhenInit`. [M01:T643, T2125, T2132]

`StageManager`, tipo **865**, conserva lista de heróis, referências de câmera/spawn/cenário, estado do stage e eventos `OnHeroRemovedFromHeroList`, `OnFirstClearStage`, `OnGetBox`, `OnDeadInfoChange`. Os nomes indicam pontos de extensão do controlador; a ordem de progressão e atribuição de recompensas não foi reconstruída. [M01:T865]

## Progresso da conta e offline

`EAccountStatusCategory`, tipo **1404**, declara `Battle`, `Exploration`, `Reward`. `EAccountStatus`, tipo **1405**, contém 53 nomes de conteúdo relacionados a bônus globais, baús, capacidade, desbloqueios, abertura automática e offline. Entre eles estão `UnlockOfflineReward`, `OfflineRewardGoldPercent`, `OfflineRewardExpPercent`, `MaxInventorySlot`, `UnlockStashPageCount`, `UnlockArrangeSlotCount` e `UnlockSkillSlotCount`. A classe `AccountStatus`, tipo **1406**, declara 40 métodos, quase todos ofuscados. Não foram recuperados custos, limites ou a correspondência de cada método a cada atributo. [M01:T1404–1406]

`OfflineRewardResult`, tipo **832**, declara `HasReward`, `RewardSec`, `StartSteamUnixSec`, `EndSteamUnixSec`, `Gold`, `HeroResults`. `OfflineRewardHeroResult`, tipo **831**, declara `HeroKey`, `StartLevel`, `EndLevel`, `StartExp`, `EndExp`, `StartMaxExp`, `EndMaxExp`, `AddedExp`. Esses contratos permitem representar um resumo global e evolução individual dos heróis. [M01:T831–832]

`UI_OfflineReward`, tipo **54**, declara texto de último login, tempo offline, slider de tempo, ouro, slots de heróis, grupo de canvas e banner. `OfflineRewardInfoData`, tipo **1882**, existe, mas seus cinco campos estão ofuscados. A duração máxima remunerada, a taxa de ganhos, arredondamentos, fontes de tempo confiáveis e a fórmula de EXP permanecem desconhecidos nesta análise. Os identificadores `SteamUnixSec` não provam por si só que o relógio seja consultado na Steam ou que exista proteção eficaz contra alteração de horário. [M01:T54, T831–832, T1882]

## Persistência e sessão

| Contrato | Campos e operações preservados | Limites da conclusão |
|---|---|---|
| `AccountSaveData` (889) | Versão, primeiro uso, último save, playtime, contador de sessão, owner Steam, marcador de último exit com nomes `Encrypted`/`Key`; `GetLastExitSteamUnixSec` e `SetLastExitSteamUnixSec` | Nomes não demonstram algoritmo, robustez ou formato de criptografia |
| `CommonSaveData` (892) | Stage atual/wave, maior concluído, stage anterior, heróis selecionados, pet, tutorial, backups e flags de primeira execução | Não foram lidos arquivos efetivos nem validada política de backup |
| `PlayerSaveData` (899) | Composição de configurações, moedas, heróis, itens, inventários, stash, skills/atributos, pets, runes, cube, relic e operações pendentes; `PreSave()` e `PostLoad()` | Representa o contrato declarado; não prova atomicidade, migração ou integridade |
| `HeroSaveData` (1442) | HeroKey, nível/EXP, desbloqueio, pontos, pontos alocados, IDs de equipamentos, skills, grupos de atributos e skin | Não foram lidos valores de personagens do usuário |
| `SettingSaveData` (900) | Idioma, FPS, janela, logs, repetição de stage, visualização de valores/textos/tooltips e abertura de baús | Persistência e aplicação real de cada opção exigem rastreamento de código/runtime |

Há tipos de máquinas de estado geradas pelo compilador chamados `<AutoSaveAsync>d__68`, `<EnsureFirstSaveAsync>d__43`, `<SaveAsync>d__44` e `<WaitForValidSteamIdAsync>d__51`, índices **1764–1767**. São vestígios de operações assíncronas nomeadas; não estabelecem intervalo de autosave, caminho no disco, ordem de flush ou resistência a falhas. [M01:T1764–1767]

O manifesto de inicialização inclui `TS.AppLifecycle.Initialize`, `Hero.ResetLeader`, `LogRequestQueueManager.ResetState`, `System/Party.ResetSession`, `System/PartyEffect.ResetSession`, `System/Relic.ResetSession` e resets de módulos PartyMod/ModRule. Isso comprova que essas entradas foram registradas no arquivo de inicialização. Não é um log de chamadas executadas nesta investigação. [M03]

O manifesto de assemblies e classes de contratos revelam integração nominal com Steam e Backend, incluindo inventário e correio. Não foram consultados endpoints, autenticação, configurações privadas ou dados remotos. A arquitetura do servidor e as regras autoritativas não podem ser reconstruídas pelo contrato cliente isoladamente. [M01, M02]

## Integração com o desktop

`WindowManager`, tipo **723**, tem 110 métodos e 73 campos. Preserva eventos `OnWindowResolutionChanged`, `OnMonitorInfoChanged`, `OnWorldUIPositionChanged`, `OnWindowLayoutPresetChanged`, `OnWindowPositionChanged`, `OnBlockMoveWindowChanged`, `OnZoomOptionsChanged` e `OnWorldUIOnTopChanged`. Há referências separadas a retângulos de WorldUI, MainUI, LogUI, painel de informação e regiões superior/inferior de logs. Possui métodos Unity `Awake`, `Update`, `OnDisable`, `OnDestroy` e `OnApplicationFocus(hasFocus)`. [M01:T723]

`EWindowLayoutPreset`, tipo **1075**, declara `None`, `LeftBottom`, `CenterBottom`, `RightBottom`, `LeftTop`, `CenterTop`, `RightTop`. `EWindowZoomRate`, tipo **1074**, declara `Zoom1`, `Zoom1_25`, `Zoom1_5`, `Zoom2`, `Zoom3`. Os nomes não bastam para confirmar o multiplicador aplicado a cada valor nem tratamento de DPI. [M01:T1074–1075]

`SettingSaveData` conserva `isAlwaysOnTop`, `maxFps`, `windowZoomRate`, `windowLayoutPreset`, `isAutoStart`, `BoxAlphaValue`, `WindowOpacityValue`, `BoxShowWhenHover`, `ShowQuickMenu`, opções de cooldown/nível/textos e expansão de tooltip. Isso é evidência de opções declaradas para a experiência desktop. Não demonstra click-through, monitoramento de aplicativos, suspensão em fullscreen, registro de autostart, consumo em idle ou APIs Win32 utilizadas. [M01:T900]

## Ideias aplicáveis ao Aetherforge

Estas são propostas de design derivadas da estrutura encontrada, e não comportamento comprovado do Taskbar Hero nem implementação realizada no Aetherforge:

1. Expor nome, nível, EXP e pontos disponíveis junto ao resumo de atributos; manter o detalhe acessível em uma ação curta.
2. Dividir o detalhe em ataque, defesa e utilidade, usando uma definição de dados com ordem, formato, tooltip e regra de ocultar zeros.
3. Representar valor base, bônus e penalidade explicitamente; cores de buff/debuff podem complementar os números.
4. Integrar alocação de pontos e skills sem obrigar a troca de contexto entre várias telas.
5. Apresentar o ganho offline com período, ouro e delta de nível/EXP por personagem; definir fórmula e teto próprios no domínio compartilhado do Aetherforge.
6. Separar regras do jogo, estado salvo e controlador visual; o catálogo de atributos deve residir no domínio compatível com `netstandard2.1`.

O menor passo útil para o menu atual é um resumo legível com detalhe progressivo. As dezenas de atributos encontradas no jogo de referência são um catálogo de possibilidades; a seleção do Aetherforge deve corresponder às regras já implementadas.

## Pendências e nível de confiança

| Pergunta | Situação nesta análise |
|---|---|
| Quais contratos/classes de status, janela, combate, offline e save existem? | Respondida para os nomes preservados do metadata v31 identificado |
| Quais campos e métodos são declarados nesses tipos? | Respondida no catálogo, incluindo offsets/tokens; assinaturas de tipos nativos não resolvidas |
| Qual é a composição exata da cena, posição e aparência dos widgets? | Exige evidência de recursos/cena; o metadata sozinho não responde |
| Quais são as fórmulas de dano, DPS, mitigação, EXP e offline? | Não respondida por esta investigação de metadata |
| Qual é a ordem das chamadas e propagação de eventos? | Não respondida; não há grafo de chamadas nativo recuperado |
| Os estados e opções declarados são todos alcançáveis? | Não respondida sem execução ou rastreamento adicional |
| Qual é o formato/criptografia real dos saves? | Não respondida; saves e dados pessoais não foram acessados |
| Qual é a API desktop e seu comportamento com DPI/múltiplos monitores? | Parcial: contratos e opções encontrados; implementação e comportamento não verificados |

**Confiança alta** na identidade e declarações validadas de M01 e no conteúdo dos manifestos M02–M04. **Confiança limitada** em interpretações de responsabilidades a partir dos nomes. **Sem evidência** para fórmulas, chamadas efetivas, garantias de segurança/backup ou medições de runtime. Nenhum desses vazios foi preenchido com números inventados.
