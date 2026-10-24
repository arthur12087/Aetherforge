# Desenvolvimento e continuidade em outro PC

## Prioridade atual

A partir de 13/10/2026, desenvolver o **idle primeiro**. O protótipo de MMO e a integração existente ficam preservados para retomada posterior. O GDD descreve a visão completa; nem tudo ali faz parte da próxima entrega.

Sequência do roadmap aprovada em 24/10/2026, preservando Herói/Status e a integração legada:

1. Fechar regras de troca entre encontros, integridade dos itens e contrato técnico.
2. Implementar equipamento funcional e comparação sobre a V19, com efeito em Status e combate.
3. Salvar e retomar a sessão, preservando IDs, ganhos e compatibilidade com o legado.
4. Definir metas, repetição da rota e gestão de itens excedentes; revisar economia antes de ativar durabilidade.
5. Especificar habilidades/pontos e, após continuidade, repetição e economia, progresso offline.

Essas etapas são trabalho a executar, não funcionalidades concluídas. Os critérios de produto M1–M5 continuam no [backlog](POC_Producao.md), incluindo energia, mineração, durabilidade e reparos. O M1 completo permanece pendente. A [primeira fatia de expedição Godot](studio-godot/README.md) já valida marcha/cenário/combate/menus, com cinco trechos e chefe. A V19 acrescenta XP e ganhos locais, ainda sem salvamento da nova rota nem integração com o save legado.

A entrada padrão já abre a expedição 3D com modelos procedurais articulados de protótipo, baseados em Brass v12 e na paleta quente aceita. O alvo de produção continua sendo personagens com rig e animação em tempo real, com cenário 3D leve: ART-03/ART-04 ainda exigem validar um integrante final, um trecho modular e seu desempenho. O [estudo v3](visual/Direcao_Artistica_v3.md) registra a direção e o orçamento inicial, não uma medição do runtime atual. Os sprites do M0 permanecem no modo Idle legado; a V19 ainda não usa rigs/assets finais.

Os registros V14–V18 abaixo são históricos: descrevem o que cada entrega continha, inclusive a bolsa indisponível antes da V19. O estado corrente de XP/loot está no parágrafo V19 e no [guia dessa versão](visual/UI_Progressao_v19.md).

A [UI V14 foi implementada na expedição](../canvas/documents/ui-v14-entrega.md): Source Sans 3/Limelight, HUD em tela e três instrumentos com conteúdo rolável e ações fixas. A escala interna 100/125/150% e as interações automatizadas passaram; DPI físico/multimonitor e acabamento artístico final continuam pendentes. **Prova de Fontes.cmd** abre o comparativo; `Run.ps1 -Mode Expedition -UiScale 1.25` amplia a UI. Não altera progressão, saves, API nem o M1 pendente.

A [V15 detalha os componentes do Status e adiciona microinterações](../canvas/documents/ui-v15-componentes-microinteracoes.md): placa gravada, sockets de atributos e fichas de equipe selecionáveis, após pesquisa de três jogos e comparação de três tratamentos. Foco/clique têm transições curtas; a vida mostra rastro de perda sem atrasar o valor. **Reduzir movimento da UI** fica no corpo rolável do Status, por execução. Em 150% ou áreas curtas, as fichas podem exigir rolagem. Não altera regras, modelos ou progressão. `Test-UiV15.ps1 -Screenshots` verifica input e componentes com saída isolada em `artifacts/ui-v15/`.

A [exploração V16 de layouts](../canvas/documents/ui-v16-layouts-exploratorios.md) registra a preferência pela **B — Ficha de campo** e quatro estruturas estáticas, com nova pesquisa de menus/HUD. Naquela rodada o runtime ainda era V15 A; os estudos permanecem como histórico. [Galeria comparativa](visual/conceitos-v16/index.html).

Você escolheu **01 — Dossiê de turno**, agora [implementado no Godot como V17](../canvas/documents/ui-v17-dossie-implementado.md): Status editorial com régua compartilhada e retratos da equipe; Herói mais largo com retrato/bolsa lado a lado e empilhamento em largura curta; Expedição como bilhete de cinco etapas. Janelas continuam independentes, ações fixas e bolsa 7×3 indisponível. Fontes, seleção, microinterações e redução de movimento preservadas. A 150% nesta altura, equipe/bolsa exigem rolagem; não é encaixe universal nem teste de DPI físico. **24 casos UI**, build, expedição, legados e integração passaram, com capturas próprias em `artifacts/ui-v17/`. Regras, palco/HUD, modelos, saves e M1 pendente não mudaram.

A [V18 implementa o Status de integrante](visual/UI_Status_v18.md), após aprovação da adaptação de Taskbar Hero: retrato e contexto compartilhados com Herói/palco, resumo com dados reais, atributos detalhados em grupos e explicação selecionável. Escape volta em camadas; Enter/Espaço ativam o controle focado. A antiga régua do Status cede lugar à lista compacta; Herói e Expedição conservam o desenho V17. O corpo rola quando necessário em 100/125/150%, com ações fixas. Não adiciona XP, pontos ou habilidades ainda inexistentes; cura, dano e escudo mantêm os valores anteriores, agora expostos pelo domínio para o detalhe. Evidências em `artifacts/ui-v18/qa/`; [pesquisa completa](referencias/taskbar-hero/README.md).

A [V19 adiciona nível, XP e loot da sessão](visual/UI_Progressao_v19.md): barra real no Status, crescimento linear até nível 20, recompensas por vitória e bolsa com itens inspecionáveis no Herói. As ações de pausa/recuo/reinício ficam em Expedição, liberando espaço na ficha. Reiniciar conserva os ganhos; fechar o jogo os perde. Equipar e aplicar bônus, persistir e integrar esses itens continuam pendentes. É uma primeira parte de IDL-02, não o M1 completo.

As outras telas estão propostas no [protótipo navegável do idle](visual/prototipo/index.html), com [mapa dos fluxos](visual/Fluxos_Idle_Prototipo.md). O estudo usa dados fictícios e ações de demonstração para revisar UI-01; inclui o resumo offline de UI-02 como prévia futura. Não implementa esses marcos no cliente Godot, nem altera a API ou o save.

## Organização

| Caminho | Conteúdo |
|---|---|
| `src/Aetherforge.Client/` | Godot .NET: expedição 3D técnica como entrada padrão; idle com sprites e mapa integrado preservados |
| `src/Aetherforge.Domain/` | Regras C# compartilhadas, sem dependência do Godot |
| `src/Aetherforge.Api/` | API local e combate do protótipo integrado |
| `src/Aetherforge.Persistence/` | Persistência JSON do protótipo |
| `tests/Aetherforge.Checks/` | Verificações executáveis de domínio e persistência |
| `scripts/` | Setup, build, execução e testes no Windows |
| `scripts/docs/` | Geradores opcionais de documentos de planejamento |
| `docs/` | Design, arquitetura, backlog, evidências e instruções |
| `docs/economia/` | Planilha e imagens de planejamento econômico v0.3 |
| `.tools/`, `artifacts/`, `outputs/` | Downloads e saídas locais, ignorados pelo Git |
| `data/` | Save local, ignorado pelo Git |

Preservar os modelos, texturas, sprites, arquivos `.uid`, `.import` e licença dos assets no Git. O cache `.godot/` é recriado no build. Os assets atuais são pequenos o suficiente para serem versionados diretamente.

## Primeiro clone no Windows

Requisitos: Windows x64, Git, Windows PowerShell 5.1 ou PowerShell 7 e acesso à internet para downloads e NuGet. Não é necessário instalar Godot ou .NET globalmente.

```powershell
git clone <URL_DO_REPOSITORIO>
cd <PASTA_DO_REPOSITORIO>
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1
```

`-SkipAssets` usa os assets já versionados. Sem essa opção, o setup baixa novamente o pacote KayKit fixado e copia os modelos. `Build.ps1 -BakeSprites` regenera os atlas; não é necessário no primeiro clone quando os PNGs já estão presentes.

Abra **Jogar Expedicao.cmd** para a expedição 3D local. O comando abaixo é equivalente: `Run.ps1` e `Main.cs` usam Expedition como padrão. Para revisão, use **Revisar Expedicao.cmd** ou o modo Overview:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Expedition -Overview
```

A expedição V19 não exige API. **Abrir Godot.cmd** abre o editor .NET portátil. A preparação para mudar o tipo do projeto no OhMyGame, ainda não aplicada, está no [guia Studio Godot](studio-godot/README.md).

**Jogar Idle.cmd** ou `Run.ps1 -Mode Idle` mantém o fluxo legado com sprites. Para testar esse combate com recompensa persistida, rode a API em outro terminal antes de abrir o idle:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Api
```

Sem API, o idle legado exibe a animação local e não gera itens. O save da API fica em `data/integration-state.json`. Para continuar com o mesmo equipamento legado em outro PC, feche os clientes e a API e copie esse arquivo separadamente para o mesmo caminho do clone. O Git não sincroniza saves; sem o arquivo, a API começa com um estado novo. Não copie os arquivos `.lock`. Esse save não contém XP, recursos, loot ou checkpoint da V19: reiniciar a rota preserva os ganhos da sessão, mas fechar o jogo os perde. Salvamento/retomada e offline são entregas futuras distintas.

## Verificações

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Expedition.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV14.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV15.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV17.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV18.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV19.ps1 -Screenshots
```

Os testes usam saves isolados em `artifacts/` e encerram seus processos no final. `-Screenshots` adiciona capturas e exige ambiente gráfico. Nas suítes V14/V15/V17, também automatiza o mouse do Windows, mudando foco temporariamente e restaurando a posição do cursor; não usar durante outra interação manual no desktop. Para medir o idle, use `scripts/Measure-Idle.ps1`; a matriz de DPI, monitores e consumo continua pendente conforme o [relatório M0](Validacao_M0.md).

O workflow do GitHub executa build e as duas verificações sem capturas em Windows. O setup usa as versões fixadas no projeto: SDK .NET 10.0.401 e Godot 4.7.2 .NET; cliente `net8.0`, backend `net10.0`, domínio `netstandard2.1`.

## Publicar o repositório

Crie um repositório vazio no GitHub, sem README ou licença automáticos. Na raiz local:

```powershell
git status --short
git add .
git diff --cached --stat
git commit -m "Organiza prototipo Aetherforge e prioriza desenvolvimento idle"
git remote add origin <URL_DO_REPOSITORIO>
git push -u origin main
```

Não foi escolhida uma licença para o código do projeto. A licença CC0 incluída junto aos assets KayKit se aplica a esses assets.

Para alternar entre PCs, faça commit e push antes de sair; no outro PC, use `git pull --ff-only` antes de editar. Após receber alterações de C#, execute `Build.ps1` novamente. Nunca envie `.tools/`, caches, saves ou credenciais.

## Ferramentas opcionais de documentação

`scripts/docs/build_gdd.py` é um gerador legado do documento v0.1 e usa `python-docx`. Ele escreve em `outputs/`; não substitui o [GDD atual em Markdown](GDD_Aetherforge.md).

`scripts/docs/build_economy.mjs` registra como a planilha v0.3 foi produzida e depende de `@oai/artifact-tool`, disponível no ambiente de geração original. Essa dependência não faz parte do setup do jogo. A planilha e suas imagens já estão versionadas em `docs/economia/`; novas saídas do gerador vão para `outputs/aetherforge-v03/`.
