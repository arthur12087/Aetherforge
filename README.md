# Aetherforge

Protótipo de RPG idle em C# e Godot .NET para Windows. **A entrada padrão é a expedição 3D V19**, com combate, nível/XP e loot inspecionável da sessão. O foco é desenvolver o idle primeiro; o idle com sprites, o mapa multiplayer e a integração de equipamento da API permanecem disponíveis como legado.

## Executar em outro PC

Requisitos: Windows x64, Git, PowerShell e internet. Após clonar o repositório, execute na raiz:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1
```

Depois, abra **Jogar Expedicao.cmd** ou execute o comando padrão:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1
```

O modo explícito é `Run.ps1 -Mode Expedition`. **Revisar Expedicao.cmd** ou `Run.ps1 -Mode Expedition -Overview` reúne os instrumentos Status, Herói e Expedição com o palco em uma janela de revisão. A expedição funciona localmente, sem iniciar a API.

As ferramentas ficam em `.tools/`, sem instalação global. Os assets necessários já estão versionados. O setup usa Godot 4.7.2 .NET e SDK .NET 10.0.401; cliente `net8.0`, domínio `netstandard2.1` e API/persistência/checks `net10.0`.

## Estado atual e limites

A [V19](docs/visual/UI_Progressao_v19.md) tem uma rota técnica de cinco encontros, marcha e combate 3D, chefe, pausa, recuo e reinício. Status mostra nível 1–20, XP e atributos; Herói mostra recursos e bolsa 7×3 com itens inspecionáveis. **Os bônus dos itens ainda não são aplicados:** equipar/desequipar, três slots funcionais por integrante e validação de incompatibilidades estão pendentes.

Reiniciar a rota conserva XP, recursos e loot desta execução; recuar ou perder conserva os ganhos já recebidos. **Fechar o jogo perde esses ganhos.** Não há salvamento/retomada, progresso offline nem transferência dos itens V19 para a API ou o inventário legado.

A apresentação 3D já existe como protótipo: personagens de geometria procedural articulada e cenário modular. Rigs, animações e assets finais de produção, além de validação de desempenho e DPI físico/multimonitor, continuam pendentes. Os sprites e modelos KayKit do legado pertencem a outro fluxo; não são os personagens do palco V19.

M1 permanece **parcial**: a rota técnica tem um encontro por etapa, ainda sem as cinco vitórias por etapa comum, desbloqueios e repetição/recuperação completos previstos no GDD. Equipamento funcional, energia, mineração, durabilidade e reparos também continuam pendentes. O [guia de desenvolvimento](docs/Desenvolvimento.md) registra a sequência de equipamento → salvamento/retomada → metas e excedentes; habilidades e offline vêm após suas dependências. Login, PostgreSQL e economia comercial pertencem a etapas futuras.

## Legado e save da API

**Jogar Idle.cmd** ou o comando abaixo abre o idle legado com sprites, tank, DPS e healer:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Idle
```

Para combate legado controlado pelo serviço e recompensa persistida, inicie a API em outro terminal antes de abrir esse idle:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Api
```

Sem API, esse modo tem apenas animação local, sem gerar itens. **Barra / janela** alterna sua apresentação compacta e a janela convencional; **×** fecha o cliente. A API escuta em `127.0.0.1:5080`, com uma conta de teste e sem autenticação.

A base M0 inclui dois clientes de mapa e transferência de uma armadura entre idle e MMO, com equipamento e bônus persistidos. **Jogar Multiplayer.cmd** inicia servidor e dois clientes de mapa; **Testar Integração.cmd** abre idle, mapa e serviços para demonstrar a transferência. [Instruções do teste integrado](docs/Teste_Equipamento.md).

O save **do legado** fica em `data/integration-state.json` e não é enviado ao GitHub. Para levar esse equipamento a outro PC, feche os clientes e a API e copie o arquivo separadamente para o mesmo caminho do clone, sem os arquivos `.lock`. Sem ele, a API começa com um estado novo. Esse arquivo não contém o progresso V19; copiá-lo não salva nem restaura a expedição.

## Estudos de interface

O [protótipo navegável das telas idle](docs/visual/prototipo/index.html) funciona diretamente no navegador, sem instalar dependências; abra **Ver Prototipo Idle.cmd** na raiz. Usa dados fictícios para revisar expedição, rotas, equipe, inventário, reparos e estados de recuperação. [Mapa de fluxos e regras](docs/visual/Fluxos_Idle_Prototipo.md). É um estudo de interface separado do cliente Godot; suas ações não modificam o save do jogo.

## Estrutura

```text
Aetherforge.slnx               Solução com os cinco projetos .NET
src/
  Aetherforge.Client/          Godot: expedição V19, idle legado e mapa
  Aetherforge.Domain/          Regras compartilhadas
  Aetherforge.Api/             Serviço local
  Aetherforge.Persistence/     Persistência JSON da integração legada
tests/Aetherforge.Checks/      Verificações de regras e persistência
scripts/                      Setup, build, execução e testes
docs/                         Design, arquitetura, backlog e evidências
docs/economia/                Planilha de planejamento e imagens v0.3
.github/workflows/             Verificações automáticas no Windows
```

Downloads, caches, logs, saves e saídas temporárias são ignorados pelo Git. Assets, sprites e sua licença permanecem versionados.

## Verificar

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Expedition.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-UiV19.ps1
```

Os scripts verificam, respectivamente: domínio (incluindo progressão/loot), API e rede; integração/equipamento legado e persistência após reinício; rota da expedição; UI e progressão V19. Adicione `-Screenshots` para capturas em um ambiente gráfico. Logs e saves de teste ficam em `artifacts/`. Testes headless não certificam gráficos, áudio, DPI físico ou desempenho. O CI atual executa build, `Test.ps1` e `Test-Integration.ps1`; não chama as duas suítes de runtime Expedition/V19.

## Documentação

- [Desenvolvimento, publicação no GitHub e continuidade em outro PC](docs/Desenvolvimento.md)
- [Expedição Godot: entrada atual e histórico das entregas](docs/studio-godot/README.md)
- [V19: Status, XP, loot da sessão e limites](docs/visual/UI_Progressao_v19.md)
- [Backlog e critérios de aceitação](docs/POC_Producao.md)
- [Game Design Document](docs/GDD_Aetherforge.md)
- [Stack e arquitetura](docs/Stack_Tecnologica.md)
- [Direção artística v3 e apresentação 3D leve](docs/visual/Direcao_Artistica_v3.md)
- [Artes conceituais das telas do jogo idle](docs/visual/Artes_Telas_Idle_v4.md)
- [Estudo de BioShock, Lies of P e Dishonored; tipografia v5](docs/visual/Tipografia_e_Direcao_v5.md)
- [Novos conceitos de expedição, inventário, habilidades e faixa idle v6](docs/visual/Prototipos_Telas_Idle_v6.md)
- [Reprodução visual de Task Bar Hero: inventário, habilidades e portal v7](docs/visual/Reproducao_Taskbar_v7.md)
- [Fluxo do jogador nas artes de interface](docs/visual/Fluxo_Jogador_Artes_Idle.md)
- [Telas e fluxos do idle — protótipo navegável](docs/visual/prototipo/index.html)
- [Mapa dos fluxos do protótipo](docs/visual/Fluxos_Idle_Prototipo.md)
- [Como revisar as telas e resultados da verificação](docs/visual/Prototipos_Idle.md)
- [Economia v0.3](docs/economia/Economia_Aetherforge_POC.xlsx)
- [Validação M0 e pendências](docs/Validacao_M0.md)
- [Teste de equipamento compartilhado](docs/Teste_Equipamento.md)
- [Referências e licenças dos assets](docs/Referencias_M0.md)

Para abrir o editor, importe `src/Aetherforge.Client/project.godot` no Godot .NET de `.tools/godot/`. Depois de editar C#, execute `Build.ps1`. Use `Build.ps1 -BakeSprites` para regenerar os atlas do legado. Os estudos e registros de versões anteriores preservam o estado de suas próprias entregas; para o estado corrente, consulte V19 e Desenvolvimento.
