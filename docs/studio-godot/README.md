# Aetherforge — primeira fatia Godot / A Promessa do Latão

## Progressão V19 — interface atual

A [V19](../visual/UI_Progressao_v19.md) continua o [Status de integrante V18](../visual/UI_Status_v18.md), com nível, barra de XP e atributos derivados do nível. Herói mostra recursos e bolsa 7×3 com loot recebido e inspecionável. Pausa/recuo/reinício ficam em Expedição; o Status conserva escudo e atalho para a rota.

O reinício restaura a equipe e conserva o progresso desta execução. Fechar perde os ganhos: não há salvamento nem transferência desta rota para a API. Equipar e aplicar os bônus dos itens ainda não estão implementados. As seções V17 e anteriores abaixo são histórico, inclusive as menções à bolsa indisponível.

## Dossiê de turno V17 — histórico

Você escolheu **01 — Dossiê de turno** entre os estudos V16 e autorizou a implementação. A [entrega V17 no Canvas](../../canvas/documents/ui-v17-dossie-implementado.md) registra as novas janelas nativas: Status claro com uma régua de atributos e três fichas com retratos; Herói mais largo, retrato/bolsa lado a lado ou empilhados conforme a largura; Expedição como bilhete de cinco nós. O palco continua independente. Não são PNGs dos estudos usados como UI.

Fontes reais 400/600, seleção compartilhada, ações fixas, rolagem, foco, movimento opcional e bolsa **7×3 indisponível** foram preservados. A 100%, os viewports nativos desta máquina mediram **380×620 / 588×620 / 398×620**; em 150%, a equipe e a terceira linha da bolsa pedem rolagem. O modo Overview é somente revisão, não o desktop normal ou garantia de encaixe em qualquer monitor.

`Test-UiV17.ps1 -Screenshots` reutiliza os checks V14/V15 com saída separada em `artifacts/ui-v17/qa/` e acrescenta três revisões amplas: **24 casos**, todos passados, com stderr vazio. Build final sem avisos/erros; expedição completa, legados e integração passaram. Capturas selecionadas em `assets/concepts/brass-v17-dossie/`. Mouse Win32 automatizado, teclado Godot e fixtures não equivalem a uso humano, DPI físico, gamepad ou benchmark. Regras, API, persistência, palco/HUD e modelos não foram alterados.

## Componentes e microinterações V15 — histórico

A [entrega V15](../../canvas/documents/ui-v15-componentes-microinteracoes.md) acrescenta pesquisa visual de BioShock, Frostpunk e Darkest Dungeon II, três tratamentos estáticos e o piloto **Mesa de regulagem** implementado. Cabeçalhos agora têm placa chanfrada; atributos têm encaixes; fichas da equipe selecionam o mesmo integrante de Herói/palco. A 100%, três fichas cabem no Status 380×620; em 150%/altura curta, usar rolagem.

Botões/fichas têm contato curto de foco/clique e vida tem rastro de perda, sem mover hitboxes ou atrasar dados. **Reduzir movimento da UI**, no corpo rolável de Status, desativa esses efeitos nesta execução; não reduz animações 3D/áudio nem é configuração persistida. `--reduce-ui-motion` ativa a preferência ao iniciar diretamente o runtime.

`Test-UiV15.ps1 -Screenshots` executa a suíte de input/layout compartilhada e fixtures próprias (21 casos), com saída em `artifacts/ui-v15/qa/`. Distingue mouse Win32/teclado Godot de sinais de fixtures isoladas e de playtest humano. Build, expedição, legados e integração passaram novamente; sem novas regras, pinturas, modelos ou certificação de DPI/performance.

## Base V14 implementada — histórico preservado

Fontes, menus e HUD agora usam o kit V14: Source Sans 3 com pesos reais 400/600, Limelight nos títulos, seis ícones autorais, três chassis distintos e cabeçalhos/ações fixos. O corpo dos instrumentos rola; a vida dos três aliados tem uma faixa compacta de fallback quando não cabe sobre a formação. Números de combate são desenhados em tela, com sinais e pool limitado, e o chefe recebe identificação dedicada.

Detalhes, capturas e limites: [entrega V14 no Canvas](../../canvas/documents/ui-v14-entrega.md), [plano preservado](../../canvas/documents/ui-v14-fontes-menus-hud.md) e [contrato técnico](UI-V14-IMPLEMENTACAO.md). O restante deste guia registra a fatia V13 e sua integração; a revisão V14 preserva suas regras e caminhos legados.

- **Prova de Fontes.cmd** abre o espécime nativo Jost/Source Sans 3.
- `Run.ps1 -Mode Expedition -UiScale 1.25` (ou `1.5`) aumenta a UI sem mudar o DPI global do Windows; `-HudDetails` inicia o toggle real Detalhes.
- `Test-UiV14.ps1 -Screenshots` verifica as três escalas, pesos no cache nativo, bounds virtuais, entrada e captura. A parte gráfica automatiza o mouse Win32, restaura o cursor e injeta teclado pelo Godot; não é playtest humano. Requer desktop gráfico disponível e movimenta temporariamente foco/cursor.

Build final sem avisos/erros; suítes V14, expedição, legados e integração passaram. Capturas locais em `assets/concepts/brass-v14-ui/`, logs em `artifacts/ui-v14/`. DPI físico, multimonitor, gamepad e benchmark não foram certificados. Na entrega V14 não entraram transições ornamentais ou nova opção de redução de movimento/flash. A V15 acima adiciona apenas microtransições e redução de movimento da UI; não é uma opção global de flashes.

## Entrega

Protótipo local 3D, independente da API, baseado na **Brass v12 GPT aceita como base de trabalho** e nos menus/paleta quente v10. Não substitui nem conclui todo o M1. O cliente antigo, a API, a persistência e a integração multiplayer continuam disponíveis nos modos anteriores.

- Marcha real por um corredor industrial modular, câmera acompanhando e parada para lutar; não é só uma animação de caminhada no mesmo palco.
- Cinco trechos: Entrada, Trilhos, Pressão, Ninho e Fornalha. Seis tipos: Rato, Saqueador, Sentinela, Drone, Vigia e Capataz.
- Formação: Alquimista atrás, Pistoleiro no meio, Guarda à frente; inimigos à direita. Ataques só de vivos, prioridade comum do tank, DPS, cura condicional e escudo manual com mitigação real.
- Marcha/combate/descanso, vitória final, derrota, recuo, pausa e reinício. A rota é finita: o jogador reinicia após o fim.
- Status, Herói e Expedição em três janelas independentes; retratos dos três integrantes usando a pintura v12; bolsa de **7 × 3** explicitamente indisponível nesta fatia; cinco nós de rota ligados ao estado real.
- Modelos 3D articulados técnicos, movimentos por pivôs/plantio de pés, recuo de rifle, golpes, infusão, impactos, números, barras de vida e queda. Áudio sintetizado original de passos, disparo, metal, cura e caldeira, sem samples de terceiros.

## Executar

Na raiz do repositório:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Run.ps1 -Mode Expedition
```

Ou **Jogar Expedicao.cmd**. O palco abre compacto junto à barra; os botões Status/Herói/Expedição abrem os instrumentos. Eles podem ser movidos pelo cabeçalho e fechados sem interromper a rota.

**Revisar Expedicao.cmd** abre o modo Overview: os mesmos três instrumentos embutidos e o palco abaixo, para revisar/capturar tudo numa janela. Não é um novo HUD fullscreen.

- Espaço: pausar/continuar.
- P: escudo de pressão, durante combate e fora de recarga.
- R: reiniciar a rota.
- M, com foco no palco: silenciar/reativar áudio.
- Escape: fecha o menu com foco; sem menu consumindo a tecla, sai do palco.
- Botões Pausar/Recuar/Reiniciar também chamam as regras reais.
- Detalhes: alterna nomes/valores condicionais no palco; Herói seleciona o integrante inspecionado.

**Jogar Idle.cmd**, `Run.ps1 -Mode Idle`, `-Mode Map`, `-Mode Server` e `-Mode Api` continuam legados. O drop compartilhado permanece no fluxo integrado antigo; esta rota nova **não** envia itens à API nem produz economia persistente.

## OhMyGame / Studio Godot

Projeto Godot: [`../../src/Aetherforge.Client/project.godot`](../../src/Aetherforge.Client/project.godot). Cena inicial: [`Main.tscn`](../../src/Aetherforge.Client/Main.tscn), agora iniciando a expedição por padrão. É necessário **Godot .NET**, não a edição sem C#; o Godot 4.7.2 .NET e o SDK fixado já ficam em `.tools/`.

**Abrir Godot.cmd** abre esse projeto no editor Godot portátil com o ambiente .NET correto, sem trocar o cadastro do OhMyGame. Executado nesta sessão: a janela `Main.tscn - Aetherforge — A Promessa do Latão - Godot Engine` foi confirmada aberta.

O cadastro ativo do OhMyGame ainda é `asset-canvas`. A instalação contém o tipo `godot-game` e o plugin Godot, mas esta sessão não dispõe de uma ferramenta para trocar o tipo em memória ou controlar o Godot por MCP. Não foi declarado que a conexão do Studio está validada.

Foi preparada uma **migração local reversível do cadastro**, sem mover/copiar o repositório e preservando ID, conversa e arquivos Canvas:

1. Termine esta sessão e **feche o OhMyGame**.
2. Execute **Migrar para Studio Godot.cmd**.
3. O script faz backup do cadastro em `artifacts/studio-godot/migration/`, altera somente `type`/`updatedAt` e abre o Godot .NET.
4. Reabra o OhMyGame e o mesmo projeto. Ative o plugin Godot, caso não esteja ativo, e informe o caminho do projeto acima quando a integração solicitar.

Isso é alteração local do cadastro persistido, não uma migração automática pela interface oficial. O script recusa aplicar com o OhMyGame aberto, para não disputar com o estado em memória. **Nesta entrega foi executado apenas o dry run**, não a troca efetiva. Nenhum processo do usuário foi encerrado, nenhum MCP/configuração global/credencial foi alterado.

Comandos equivalentes:

```powershell
# Inspecionar sem modificar cadastro:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Studio-Godot.ps1
# Com OhMyGame fechado:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Studio-Godot.ps1 -Apply -OpenEditor
# Voltar ao tipo Asset Canvas, também com OhMyGame fechado:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Studio-Godot.ps1 -RestoreCanvas -Apply
```

A inspeção estática encontrou `godot-game` no contrato distribuído e o plugin `godot` 0.1.0, com `@coding-solo/godot-mcp@0.1.1`. Isso **não** prova que o MCP esteja instalado/conectado nesta sessão. Evidência REA focada: `ev_84849f7466f3706cb797f4c74a204592d72468fd9cde9ac9bb076f749fe51f89`, em `artifacts/studio-godot/studio-focused-analysis.json`; leitura adicional dos manifests/contratos distribuídos. A análise ampla do ASAR foi interrompida por escopo/custo; não foi feita auditoria integral do aplicativo.

## Orquestração real e skills

Três workers independentes foram executados simultaneamente por `codex exec --sandbox workspace-write`, com contratos e propriedade exclusiva, sem bypass de sandbox e sem commits:

| Frente | Entrega | Prompt reproduzível |
|---|---|---|
| Regras | `Expedition.cs`, `ExpeditionChecks.cs` | [regras.txt](tasks/regras.txt) |
| Cena/animação | `ExpeditionStage.cs`, `ExpeditionActor.cs` | [cena.txt](tasks/cena.txt) |
| Interface | `ExpeditionView.cs`, `BrassUi.cs`, fontes/retrato | [interface.txt](tasks/interface.txt) |
| Orquestrador | integração em Main, áudio, barras, checks de callbacks, correções de runtime/leitura, build/QA e preparação Studio | [contratos](CONTRATOS.md) |

Os três terminaram. Retornos e eventos reais: `artifacts/studio-godot/agents/`. `scripts/Start-StudioWorkers.ps1` é a receita de delegação, **não** deve ser reexecutado para jogar (inicia novos agentes, pode editar código e consumir uso).

Skills do set instalado aproveitadas conforme o assunto: `godot-csharp`, `godot-3d-essentials`, `godot-animation`, `godot-ui-control`, `game-ui-ux`, `game-feel` e `godot-audio`. Não houve migração para GDScript, troca de SDK nem instalação de outro set.

## Verificação efetiva

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-UiV17.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Expedition.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Integration.ps1
```

Resultados desta sessão:

- Build completo e importação Godot passaram; build final do cliente: **0 erros / 0 avisos**.
- Checks da expedição: determinismo com diferentes deltas, grandes deltas, HP, cura, morte, shield versus baseline, pausa, recuo, reset, cinco etapas e eventos limitados. Checks antigos de domínio/persistência também passaram.
- Rota completa dentro do Godot: **137,45 segundos simulados**, cinco vitórias, seis tipos, **108 unidades de translação real do Guarda e da câmera**; poses walk/attack observadas e distância parada em combate. HP final: Guarda 108/180, Pistoleiro 52/100, Alquimista 110/110.
- UI: verificação estrutural 3 janelas/3 seletores/7×3 slots/5 nós; teste dos callbacks reais `Pressed` para pausa/retomada, seleção, fechar/reabrir, escudo, recuo e reinício. O teste gráfico sem Overview abriu os três instrumentos e verificou **três IDs nativos distintos do SO: 1, 2 e 3**, separados do palco. Não é automação de cliques físicos/teclado do Windows.
- Capturas Godot abertas e inspecionadas do palco compacto **1000 × 320** e de marcha, primeiro combate, Pressão, chefe e conclusão em Overview, **1160 × 880**. São capturas reais, não novas pinturas. Corrigidos resize manual indevido do SubViewport, sinal de resize, nullability e leitura do chefe/barras.
- Regressão: API, dois clientes multiplayer, colisão, transferência do mesmo item, defesa 13, dano 17 e persistência após reinício passaram usando saves isolados em `artifacts/`. Nenhum save manual em `data/` foi alterado.
- Evidência executável em `artifacts/studio-godot/qa/`: `route.json`, logs de interações (headless e nativo) e seis PNGs; logs gerais em `artifacts/studio-godot/`. Os nove `.err.log` da rodada final estão vazios. `first-review.err.log` é histórico anterior à correção do SubViewport, não pertence à rodada final e foi preservado como diagnóstico.

## Limites e próximo refinamento

Este é um **primeiro protótipo jogável**, não o acabamento artístico final prometido pela pintura v12. Os modelos são geometria procedural articulada, sem texturas pintadas finais, skinning/Skeleton3D, rigs de produção ou clips importados. O cenário ainda repete um kit simples, embora a marcha/câmera/encontros sejam reais. Não apresentar esta geometria como conversão automática da pintura para um modelo final.

Permanecem: direção/escultura/texturas dos personagens, animação autoral de produção e sincronização mais fina entre preparação/impacto e tick de dano; diferenciação ambiental mais forte por trecho; teste de áudio perceptivo (não realizado), navegação física/foco de janelas, DPI/múltiplos monitores, render em outras máquinas e benchmark de consumo. O limite de 30 FPS é uma configuração, **não** comprovação de orçamento/performance de produção.

A fatia não inclui níveis/XP/equipamento funcional, mineração, oficina, durabilidade, salvamento/offline da nova rota, nem integração da nova simulação com a API. Não mostra valores de progressão fictícios nem habilita botões desses sistemas. A próxima decisão de produção é validar um personagem final e um trecho de cenário contra v12, preservando o loop e os testes já obtidos — sem reiniciar a direção visual ou gerar novas pranchas sem pedido.

Proveniência dos recursos de UI: [`Assets/Brass/PROVENANCE.md`](../../src/Aetherforge.Client/Assets/Brass/PROVENANCE.md). Retrato próprio derivado de v12 e fontes SIL OFL incluídas; nenhuma imagem de pesquisa BioShock entrou no runtime.
