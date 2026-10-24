# Conferência de assets — fatia de equipamento

GAMELAB-19, 24/10/2026. Referência: `b18ef0010b29edaf1c4b67f04823594283b9f12f`, branch de origem `agent/gameplay-engineer/ec9591c60121` (PR #4). Trabalho documental sobre essa pilha; não afirma integração em main. O manifesto adjacente `equipment-assets-manifest.json` identifica cada arquivo por SHA-256, origem, licença/pendência, derivação, aprovação e chamada de uso.

Esta conferência retoma o inventário de 25 arquivos e a auditoria de `aca2d62`, anexos de GAMELAB-1 (`01a16ccc-5939-77e9-9f7b-2489ba48b454` e `01a16ccc-584b-764a-9684-8b7b7dcb2ca0`). Não substitui o histórico nem repete a auditoria visual geral. A entrega GAMELAB-8 foi consultada pelo anexo `equipment-integration-review.md` (`01a16e3b-0361-725b-8bd7-90ea13645392`) e pelas fontes integradas.

## Resultado da classificação

| Grupo do manifesto | Quantidade | Uso demonstrado e tratamento futuro |
|---|---:|---|
| `slice-runtime` | 9 | Pintura equipe-v12, Source Sans 3/Limelight, shield/rifle/heal/pause/return/vital. Candidatos ao pacote da expedição; retrato com decisão de direitos ainda pendente. |
| `proof-only` | 2 | Jost e route.svg disponíveis na Prova de Fontes. Não foi encontrada chamada de route.svg na rota da expedição; os conectores são procedurais. Incluir somente se a prova for distribuída. |
| `legacy-runtime` | 3 | Atlas Knight/Mage/Rogue usados em idle/mapa. Manter se esses modos fizerem parte do pacote. Não são os atores da expedição. |
| `legacy-source` | 6 | GLBs e texturas auxiliares KayKit usados no bake; GLBs também declaram imagem embutida. Fontes preservadas no Git; incluir em distribuição de fontes/bake conforme escopo futuro. |
| `required-notice` | 4 | Três OFL e licença KayKit. Preservar integralmente no repositório e levar os avisos aplicáveis como arquivos legíveis no pacote correspondente. |

Os 24 arquivos de mídia/avisos são os mesmos do inventário anterior; o 25º era `Assets/Brass/PROVENANCE.md`, agora corrigido. Dez receitas C# de UI, atores, cenário, VFX e áudio estão identificadas separadamente por hash. Os 20 `.import` adjacentes permanecem preservados e têm hashes próprios. Eles são metadados de importação, não nova mídia ou autorização de uso.

Não há catálogo novo: o palco cria geometria/animação por código em `ExpeditionActor`/`ExpeditionStage`; a interface usa Controls e SVGs próprios. Checkbox SVG é gerado em memória por `BrassUi`. O áudio é sintetizado em `ExpeditionAudio`, sem samples externos localizados nesse código. A avaliação sonora continua em GAMELAB-20; não houve escuta ou mudança de áudio nesta conferência.

## Origem, geração e aprovação

**Pintura usada nos retratos.** `Assets/Brass/Portraits/equipe-v12.png` é cópia binária de `assets/concepts/brass-v12-gpt/generated/01-equipe-r02.png`, SHA-256 `43b59b500c329fc02f2fd1f712721cb66274187e69e00c250741d45df9312194`, 1672×941. Foi gerada em 21/10/2026 por `image_gen.imagegen`, OpenAI/GPT Images; modelo/versão/request ID não expostos. O manifesto aponta o prompt, seu hash, horários e duas referências efetivamente anexadas a G01-r02. Não atribuir a essa revisão as três entradas da variante rejeitada r01. As regiões integrais/de busto são calculadas em `ExpeditionView.InitializePortraits`, sem editar o PNG.

O contrato posterior `docs/studio-godot/CONTRATOS.md` aceita Brass v12 **como base de trabalho**, sem aprovação final de cada detalhe. O `provenance.json` original, que registra aprovação artística pendente, continua intacto. “Fornecida pelo usuário” foi conciliado em `Assets/Brass/PROVENANCE.md` com a evidência de geração; não significa autoria manual, exclusividade ou liberação jurídica.

**Fontes.** URLs e hashes de aquisição estão em `docs/visual/assets/fonts/v5/sources.json` (14/10/2026). As três fontes e seus avisos coincidem com as cópias dessa pasta. Source Sans 3 usa pesos 400/600 em memória; Limelight é display; Jost permanece na prova A/B. OFL/copyright/nomes reservados permanecem nos textos integrais. Não foi feito download nem revalidação de termos atuais na web; esta conferência se apoia nas evidências arquivadas e na auditoria anterior, que consultou fontes primárias.

**KayKit.** Origem fixada no commit `672074b73ba276876a19e8816ecdc5241817ab47`, autor Kay Lousberg; `scripts/Setup.ps1` e `docs/Referencias_M0.md` registram o pacote. Aviso local declara CC0. Atlas são derivados do bake Godot de `SpriteBaker`, célula 128×144, oito direções, três poses e quatro quadros. Não houve novo download, comparação binária com upstream ou bake nesta rodada.

**Ícones e receitas.** Proveniência local declara desenhos autorais V14/V15 e geometria procedural do projeto. Isso registra origem declarada, sem estender CC0/OFL ao código, às pinturas ou ao restante do projeto. `docs/Desenvolvimento.md` registra que não foi escolhida licença geral de código. Aprovação de integração do protótipo não equivale a aprovação final de arte nem licença pública para redistribuição das fontes.

## Pesquisa separada de distribuição

Excluir por padrão `assets/concepts/**`, `assets/imported/**`, `docs/visual/**`, `docs/referencias/**` e `canvas/**` do futuro pacote de jogo. A exceção é a cópia de runtime explicitamente inventariada no cliente, nunca a pasta de pesquisa inteira. Não apagar esse acervo do Git nesta task. Metadados, prompts, fontes e licenças continuam disponíveis para rastreabilidade.

O manifesto destaca as referências Robb Waters/Colin Fix realmente anexadas à geração do retrato, com hashes, e os conceitos de bestiário/menu que orientaram receitas. Créditos e disponibilidade pública não demonstram licença para redistribuir essas imagens. Screenshots, recursos examinados de Taskbar Hero e referências de BioShock/Frostpunk/Dishonored/NieR/RDR2/Outer Worlds não viram recursos liberados por estarem no repositório. Não foram encontradas cargas diretas dessas pastas no cliente; isso não certifica direitos sobre a geração derivada de referências.

Não há `export_presets.cfg` versionado, pacote exportado ou PCK inspecionado nesta entrega. O manifesto é uma proposta explícita para distribuição futura: cargas dinâmicas devem constar do export, e arquivos TXT de avisos precisam acompanhar o pacote mesmo que o importador não os inclua. Antes de distribuir, conferir conteúdo real contra o manifesto e rever as pendências abaixo. Nenhum pacote foi preparado/publicado.

## Decisão semântica do Guarda

Nesta fatia, `shield.svg` é símbolo contextual de proteção/função e **não identifica sozinho o tipo de equipamento**. `ExpeditionLootView.cs:78` ainda o usa para armadura e para arma do Guarda. Não transformar a Lâmina da Caldeira em escudo nem inferir defesa dessa imagem.

A decisão documental é conservar os ícones e usar os textos/estados já integrados como significado autoritativo:

| Contexto | Significado verificável |
|---|---|
| HUD “Escudo · P” e seletor “TANK” | Proteção manual/função do Guarda; não slot de armadura. |
| Bolsa com shield | Ilustração compacta compartilhada. Tooltip/inspeção identificam nome, slot, função e bônus (`ExpeditionLootView.Describe`). A letra C/I/R expressa raridade, não tipo. |
| Herói/Status | Slots “Arma”, “Armadura”, “Acessório”; “Foco” para a arma do healer (`EquipmentPresentation.Slot`). Estado “Vazio · bônus 0” ou “Equipada”, integrante e item explícitos (`ExpeditionView.Equipment.cs:243`). |
| Comparação/confirmação | Alvo, peça equipada/candidata, valores antes/depois e Ganho/Perda/Sem alteração. Bloqueios vêm do domínio; sucesso aparece após operação aceita. |

A leitura isolada da miniatura permanece ambígua; ela não deve ser usada como prova de compreensão. A redundância textual resolve o contrato de significado nesta entrega documental, sem novo símbolo ou alteração de scripts. Se playtest revelar confusão apesar de inspeção/rótulos, o Director poderá coordenar correção de texto/estado antes de encomendar arte. Não há comprovação humana de compreensão nesta rodada. Equipar não troca o desenho da arma no palco/retrato; o efeito é comunicado pelos atributos e regras de combate.

## Conferências e limites

Executados nesta rodada: checkout Multica, leitura de AGENTS.md/histórico/anexos, `git merge-base --is-ancestor` para `4cc69d0e` e `c8fcff01` em `b18ef001`, avanço apenas da branch desta task para a referência, rastreamento estático de cargas e conferência Python de hashes/PNG/SVG/cópias/avisos/imports. Resultado: **191 conferências documentais passaram**, anexadas como `equipment-assets-validation.json`, com verificador `validate_asset_manifest.py`. Não são testes de gameplay. O verificador roda com `python validate_asset_manifest.py` na pasta contendo checkout `Aetherforge/` e `inputs/inventario-visual-aca2d62.json` baixado pelo CLI; usa somente a biblioteca padrão e Git. Avisos são comparados também pelo blob Git, considerando a conversão CRLF do checkout; seus bytes locais continuam conferidos contra o inventário anterior. `git diff --check` verifica formatação; caminhos e links dos documentos alterados são conferidos localmente. Sem alterações de binários, scripts de produção, `.import`, licenças ou saves.

As execuções de build/UI/combate descritas por GAMELAB-8 pertencem àquela entrega. A revisão do Director libera `b18ef001` no alcance de reprodução nativa automatizada; a causa dos dois erros históricos continua inconclusiva. Não reexecutei build, importação, runtime gráfico, exportação, benchmark ou QA humano/DPI. O desktop foi preservado para a avaliação de áudio em paralelo, conforme instrução da issue.

Pendências explícitas para Arthur/Director antes de distribuição: decisão de uso/direitos da pintura gerada e de suas referências, aprovação artística final dos detalhes v12, licença/condições de distribuição do projeto e escopo exato do pacote (expedição, legado, prova e fontes). A revisão do pacote real e inclusão dos avisos dependem de um export futuro. Essas lacunas não impedem a entrega do manifesto documental; impedem tratá-lo como certificação de direitos ou pacote pronto.
