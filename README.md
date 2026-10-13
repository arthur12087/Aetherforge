# Aetherforge

Protótipo de RPG idle em C# e Godot .NET para Windows. **O foco atual é desenvolver o idle primeiro:** combate, progressão, equipamento, rotas de caça e mineração. O protótipo multiplayer e a integração de equipamento permanecem no projeto para uma etapa posterior.

## Executar em outro PC

Requisitos: Windows x64, Git, PowerShell e internet. Após clonar o repositório, execute na raiz:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1
```

Depois, abra **Jogar Idle.cmd**. As ferramentas ficam em `.tools/`, sem instalação global. Os modelos e sprites necessários já estão versionados. O setup usa Godot 4.7.2 .NET e SDK .NET 10.0.401.

O idle atual apresenta tank, DPS e healer com combate animado. Para combate controlado pelo serviço e recompensa persistida, inicie a API em um terminal separado antes de abrir o idle:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Run.ps1 -Mode Api
```

Sem API, há apenas animação local, sem gerar itens. **Barra / janela** alterna a apresentação compacta e a janela convencional; **×** fecha o cliente. A API escuta em `127.0.0.1:5080`, com uma conta de teste e sem autenticação.

## Estado e próximos passos

A base M0 inclui janela idle, sprites direcionais, API, persistência JSON, dois clientes de mapa e transferência de uma armadura entre idle e MMO. M1 ainda não está implementado: rotas completas, níveis e XP persistidos, mineração, durabilidade e progresso offline seguem pendentes. Login, PostgreSQL e economia comercial pertencem a etapas futuras.

O save local fica em `data/integration-state.json` e não é enviado ao GitHub. Para levar esse progresso a outro PC, feche os clientes e a API e copie o arquivo separadamente. Sem ele, o novo PC começa com um estado novo.

## Estrutura

```text
Aetherforge.slnx               Solução com os cinco projetos .NET
src/
  Aetherforge.Client/          Cliente Godot: idle e protótipo do mapa
  Aetherforge.Domain/          Regras compartilhadas
  Aetherforge.Api/             Serviço local
  Aetherforge.Persistence/     Persistência do protótipo
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
```

O primeiro script verifica domínio, API e rede; o segundo verifica combate, transferência, equipamento e persistência após reinício. Adicione `-Screenshots` para capturas em um ambiente gráfico. Logs e saves de teste ficam em `artifacts/`.

## Documentação

- [Desenvolvimento, publicação no GitHub e continuidade em outro PC](docs/Desenvolvimento.md)
- [Backlog e critérios de aceitação](docs/POC_Producao.md)
- [Game Design Document](docs/GDD_Aetherforge.md)
- [Stack e arquitetura](docs/Stack_Tecnologica.md)
- [Economia v0.3](docs/economia/Economia_Aetherforge_POC.xlsx)
- [Validação M0 e pendências](docs/Validacao_M0.md)
- [Teste de equipamento compartilhado](docs/Teste_Equipamento.md)
- [Referências e licenças dos assets](docs/Referencias_M0.md)

Para abrir o editor, importe `src/Aetherforge.Client/project.godot` no Godot .NET de `.tools/godot/`. Depois de editar C#, execute `Build.ps1`. Use `Build.ps1 -BakeSprites` para regenerar os atlas.

Os atalhos **Jogar Multiplayer.cmd** e **Testar Integração.cmd** continuam disponíveis. O primeiro inicia um servidor local e duas janelas de mapa; o segundo abre idle, mapa e serviços para demonstrar a transferência de equipamento. [Instruções do teste integrado](docs/Teste_Equipamento.md).
