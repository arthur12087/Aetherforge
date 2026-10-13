# Aetherforge

## Prioridade

Desenvolver o idle primeiro. A ordem atual está em `docs/Desenvolvimento.md`; os critérios de M1 estão em `docs/POC_Producao.md`. Preservar o protótipo multiplayer e sua integração existente. Ampliações do MMORPG e marketplace pertencem a etapas posteriores, salvo orientação do usuário.

## Estrutura e regras

- Apresentação Godot em `src/Aetherforge.Client/`.
- Regras compartilhadas em `src/Aetherforge.Domain/`, sem dependência de Nodes ou APIs do Godot; manter compatibilidade com `netstandard2.1`.
- Serviço local em `src/Aetherforge.Api/` e persistência JSON em `src/Aetherforge.Persistence/`.
- Verificações executáveis em `tests/Aetherforge.Checks/`.
- Documentação em `docs/`. Geradores de documentos são opcionais e não fazem parte do runtime do jogo.
- Versionar assets originais, sprites, `.uid`, `.import` e licenças. Excluir ferramentas baixadas, caches, logs e saves locais conforme `.gitignore`.
- Não tratar o protótipo atual como implementação completa das regras do GDD. Registrar funcionalidades pendentes com clareza.

## Ambiente Windows

Usar as ferramentas portáteis fixadas pelo projeto. Na raiz:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build.ps1
```

Para mudanças de comportamento, executar as verificações pertinentes:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-Integration.ps1
```

`-Screenshots` exige ambiente gráfico. Não alterar o save manual em `data/` para validar mudanças; os testes usam arquivos isolados em `artifacts/`. Alterações apenas de documentação não exigem recompilação; verificar caminhos e links.
