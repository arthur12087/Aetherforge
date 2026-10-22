# Desenvolvimento e continuidade em outro PC

## Prioridade atual

A partir de 13/10/2026, desenvolver o **idle primeiro**. O protótipo de MMO e a integração existente ficam preservados para retomada posterior. O GDD descreve a visão completa; nem tudo ali faz parte da próxima entrega.

Ordem de trabalho:

1. Combate idle verificável com tank, DPS e healer (IDL-01).
2. Níveis, XP, equipamento e progressão (IDL-02).
3. Rota de caça, etapas, chefe, derrota e recuo (IDL-03).
4. Mineração, durabilidade, reparo e gerenciamento da equipe (IDL-04, IDL-05, UI-01).
5. Evoluir salvamento e progresso offline depois de consolidar o ciclo jogável.

Os critérios detalhados continuam no [backlog](POC_Producao.md). O M1 completo permanece pendente. Foi adicionada uma [primeira fatia de expedição Godot](studio-godot/README.md), autorizada à parte para validar marcha/cenário/combate/menus antes da progressão: cinco trechos e chefe, sem XP/economia/salvamento da nova rota. Ela não altera a ordem do restante do backlog nem substitui o protótipo integrado.

Direção visual atual: personagens 3D com rig e animação em tempo real; o idle pode usar cenário 3D desde que leve. O [estudo v3](visual/Direcao_Artistica_v3.md) define linguagem e orçamento inicial. Validar um integrante e um trecho modular em ART-03/ART-04, mantendo o protótipo integrado existente até a migração passar nas verificações. Os sprites do M0 permanecem no modo Idle legado, não como alvo de produção. A entrada padrão agora abre a expedição com modelos procedurais articulados de protótipo, baseados em Brass v12 e na paleta quente aceita; ainda não são rigs/assets finais.

A [UI V14 foi implementada na expedição](../canvas/documents/ui-v14-entrega.md): Source Sans 3/Limelight, HUD em tela e três instrumentos com conteúdo rolável e ações fixas. A escala interna 100/125/150% e as interações automatizadas passaram; DPI físico/multimonitor e acabamento artístico final continuam pendentes. **Prova de Fontes.cmd** abre o comparativo; `Run.ps1 -Mode Expedition -UiScale 1.25` amplia a UI. Não altera progressão, saves, API nem o M1 pendente.

A [V15 detalha os componentes do Status e adiciona microinterações](../canvas/documents/ui-v15-componentes-microinteracoes.md): placa gravada, sockets de atributos e fichas de equipe selecionáveis, após pesquisa de três jogos e comparação de três tratamentos. Foco/clique têm transições curtas; a vida mostra rastro de perda sem atrasar o valor. **Reduzir movimento da UI** fica no corpo rolável do Status, por execução. Em 150% ou áreas curtas, as fichas podem exigir rolagem. Não altera regras, modelos ou progressão. `Test-UiV15.ps1 -Screenshots` verifica input e componentes com saída isolada em `artifacts/ui-v15/`.

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

Abra **Jogar Expedicao.cmd** para a nova fatia 3D local ou **Revisar Expedicao.cmd** para os três menus embutidos. **Abrir Godot.cmd** abre o editor .NET portátil. A preparação para mudar o tipo do projeto no OhMyGame, ainda não aplicada, está no [guia Studio Godot](studio-godot/README.md).

**Jogar Idle.cmd** mantém o fluxo legado. Para testar esse combate com recompensa persistida, rode a API em outro terminal antes de abrir o idle:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Api
```

Sem API, o idle exibe a animação local e não gera itens. O save da API fica em `data/integration-state.json`. Para continuar com o mesmo equipamento em outro PC, feche o idle e a API e copie esse arquivo separadamente para o mesmo caminho do clone. O Git não sincroniza saves; sem o arquivo, a API começa com um estado novo. Não copie os arquivos `.lock`.

## Verificações

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Expedition.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV14.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV15.ps1 -Screenshots
```

Os testes usam saves isolados em `artifacts/` e encerram seus processos no final. `-Screenshots` adiciona capturas e exige ambiente gráfico. Nas suítes V14/V15, também automatiza o mouse do Windows, mudando foco temporariamente e restaurando a posição do cursor; não usar durante outra interação manual no desktop. Para medir o idle, use `scripts/Measure-Idle.ps1`; a matriz de DPI, monitores e consumo continua pendente conforme o [relatório M0](Validacao_M0.md).

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
