# Evidências, ferramentas e reprodução

[Índice](README.md) · [Cobertura e lacunas](Lacunas.md). Investigação local de 23/10/2026, versão declarada em `Version.txt`: **1.3.0**.

## Identidade da instalação

O [manifesto](evidencias/Instalacao.json) registra caminhos relativos, tamanhos e SHA-256 dos arquivos encontrados na instalação. Não inclui saves do usuário. Os principais marcadores são:

| Arquivo | Bytes | SHA-256 |
|---|---:|---|
| `TaskBarHero.exe` | 672.256 | `f6b9b3eddae8dce1cff501aa389da6584bd58bf7c98a42ac02fb7bb3d77aab7f` |
| `GameAssembly.dll` | 111.653.376 | `f09a3eb6dd3fcb14f9e9f612514d66b3b6d9007d4b90962436493f3f18c3089c` |
| `TaskBarHero_Data/il2cpp_data/Metadata/global-metadata.dat` | 23.036.032 | `85bf394366ce9c6c6969be9731131d5bc87c80bd94fbed3e54b8f50ca785e622` |
| `TaskBarHero_Data/level0` | 7.661.584 | `da9e62b1a6984840f5ac2f056d5cad43142bf62e2bd6c0546f6decb17185c9c9` |
| `TaskBarHero_Data/sharedassets0.assets` | 23.661.588 | `ee1d04adcaeb073013f3dfb5640c1542da2d0734c03c18684246af45ef197f64` |

Quando esses hashes mudarem, a nova instalação deve receber outra rodada de análise. A existência de um arquivo antigo em `artifacts/` não confirma seu alinhamento com outra versão. O snapshot anterior do launcher foi reutilizado somente após o REA aceitar a identidade correspondente ao executável atual.

## Registro das fontes

| Código | Tipo de evidência | Conteúdo e fronteira | Resumo portátil |
|---|---|---|---|
| R01 | REA/Ghidra | Launcher PE x64 aberto com snapshot alinhado; busca de procedimentos e strings já retidas | [REA_Resumo.json](evidencias/REA_Resumo.json) e IDs abaixo |
| R02 | REA managed static | `GameAssembly.dll` identificado como PE x64 **sem CLI/CIL** | [REA_Resumo.json](evidencias/REA_Resumo.json), campo `managed` |
| R03 | REA/Ghidra | Pseudocódigo e instruções do entry do launcher em `0x140001264` | [REA_Resumo.json](evidencias/REA_Resumo.json), campo `launcher_entry` |
| R04 | Retorno de indisponibilidade REA | Ghidra Windows P0 recusa DLL; não se analisaram corpos de gameplay | Limitação literal abaixo |
| U01 | UnityPy 1.24.2 | GameObjects, transforms, layout e cabeçalhos de componentes de `level0`; MonoScripts de `globalgamemanagers.assets` | [UI_Resumo.json](evidencias/UI_Resumo.json) |
| D01 | UnityPy + parser CSV | 59 TextAssets em `sharedassets0.assets`, esquemas, 43.952 registros e valores literais | [Tabelas_Resumo.json](evidencias/Tabelas_Resumo.json), [catálogo](Catalogo.md) |
| L01 | UnityPy, StringTable/SharedTableData | Duas coleções nos idiomas en-US e pt-BR; resolução por ID compartilhado | [Localizacao_Resumo.json](evidencias/Localizacao_Resumo.json) |
| M01 | Parser IL2CPP v31 | Tipos/campos/métodos/nomes/parâmetros com offsets e tokens, sem corpos nativos | [Metadados_Resumo.json](evidencias/Metadados_Resumo.json) |
| M02–M04 | Arquivos de configuração | Assemblies declarados, inicializadores, plugins e boot/app.info | Mesmo resumo de metadados; [Arquitetura](Arquitetura.md) |
| V01 | Imagem histórica local | Captura oficial previamente preservada; inspecionada nesta rodada como referência visual | [Screenshot 02](../../visual/referencias/taskbar-hero-oficial-02.jpg), proveniência no [estudo anterior](../../visual/Estudo_Taskbar_Hero.md) |

U01, D01, L01 e M01 são identificadores de evidências auxiliares desta documentação. **Não são Evidence IDs emitidos pelo servidor REA.** V01 não é captura de runtime feita nesta sessão e não comprova o layout exato da 1.3.0.

### Evidence IDs do REA

- Classificação da DLL: `ev_d294096ce66b7f673b123436c35e72ba48cb3b848d87e813a756d6462872df03`.
- Entry decompilado do launcher: `ev_369f846b075d884d4abe73beead99b88c977620d9c1a1129de8d5f67a065ee84`.

O dossier do entry contém apenas inicialização de runtime (`__security_init_cookie` e `__scrt_common_main_seh`), sem regras de combate ou Status. Essa decompilação não deve ser anunciada como recuperação do gameplay.

### Limites operacionais observados

`open_binary` para a DLL retornou `capability_unavailable`, `target_role_unsupported`: **“Windows Ghidra P0 accepts PE applications, not DLL or non-executable images.”** O provedor disponível é Ghidra 12.1.4. Não foram instalados outros motores nem alterados os binários para contornar essa fronteira.

O Computer Use não autorizou abrir o jogo: **“Computer Use was not approved to use taskbarhero”**. Portanto, não foram feitas navegação por menus, capturas atuais, alocações de pontos ou medições de comportamento.

O servidor REA também bloqueou `export_evidence_bundle` e `close_binary`, inclusive o fechamento sem snapshot, com **“MCP tool call requires approval, but approval policy is never”**. O export autenticado da sessão não foi criado e o encerramento da sessão não pôde ser confirmado. O JSON portátil deste relatório preserva os dois registros retornados inline; ele **não se apresenta como bundle autenticado exportado pelo REA**. O alvo original permaneceu sem alteração.

## Cobertura dos parsers

U01 decodificou todos os 12.698 GameObjects e 12.698 transforms da cena. Todos os 17.972 MonoBehaviours expuseram somente as quatro propriedades do cabeçalho base conhecido. Em 17.874 deles, foi necessário um fallback após erro de leitura integral por dados residuais; os outros 98 não lançaram esse erro, o que não comprova campos customizados decodificados. As strings ASCII residuais são candidatas e não bindings ou textos interpretados. Valores `active_self` não equivalem a visibilidade efetiva, e posições locais não equivalem a pixels físicos. A primeira referência externa da cena foi conferida como `globalgamemanagers.assets`, origem dos MonoScripts usados no resumo.

D01 usa parsing CSV, preserva campos vazios e registra linhas malformadas. Nesta extração não houve linhas malformadas. A duplicidade de chave em `BuffGroupInfoData` permanece como fato, sem presumir a política do carregador.

L01 preserva entradas sem chave compartilhada. Há 65 em StringTable en-US, 47 em StringTable pt-BR e quatro em ItemTable pt-BR. Não foram descartadas da extração integral; apenas não aparecem no mapa conveniente de chaves resolvidas. Cada idioma contém 3.232 chaves resolvidas. A soma bruta de registros não é o tamanho desse mapa.

M01 valida assinatura/versão, limites das tabelas, tamanhos de registros utilizados, nomes, tokens, associação de métodos aos tipos e intervalos de parâmetros. Esses checks validam a interpretação estrutural, não tipos nativos, offsets de objetos em memória ou chamadas efetivas.

## Reproduzir a extração

Os scripts são opcionais para pesquisa; não fazem parte do setup/build/runtime do jogo. A análise usou o Python já presente em `.tools/taskbar-hero-rea/venv/` com UnityPy 1.24.2. Essa pasta é local, ignorada pelo Git e não acompanha um clone. O parser de metadados usa somente a biblioteca padrão do Python; os outros dois exigem UnityPy 1.24.2 no interpretador escolhido.

Executar na raiz do projeto, com uma cópia legitimamente instalada da mesma versão. Os comandos abaixo escrevem somente evidências no repositório:

```powershell
$taskbarRoot = 'C:\Program Files (x86)\Steam\steamapps\common\TaskbarHero'
$taskbarPython = '.\.tools\taskbar-hero-rea\venv\Scripts\python.exe'
$taskbarOutput = '.\artifacts\taskbar-hero-rea\current'

& $taskbarPython .\scripts\research\Inspect-TaskbarHero-Ui.py --game $taskbarRoot --output "$taskbarOutput\ui"
& $taskbarPython .\scripts\research\Inspect-TaskbarHero-Data.py --installation $taskbarRoot --output $taskbarOutput
& $taskbarPython .\scripts\research\Inspect-TaskbarHero-Metadata.py --data-dir "$taskbarRoot\TaskBarHero_Data" --output "$taskbarOutput\metadata"
```

Para atualizar os resumos portáteis, acrescentar `--evidence .\docs\referencias\taskbar-hero\evidencias` aos scripts UI/Data e `--summary .\docs\referencias\taskbar-hero\evidencias\Metadados_Resumo.json` ao de metadata. Revisar qualquer diferença de versão antes de substituir o relatório. Os resumos já versionados são suficientes para consultar os achados sem executar scripts ou ter a instalação.

Os scripts leem recursos distribuídos; não abrem saves, não carregam DLLs de gameplay no processo e não se conectam a serviços do jogo. As extrações completas ficam em `artifacts/`; nenhuma textura, sprite, áudio ou binário do Taskbar Hero foi adicionado aos assets do runtime de Aetherforge.

## Roteiro para uma futura validação interativa

Na conferência final, os 171 links relativos dos nove documentos resolveram; as 59 tabelas tiveram esquema, contagem e hash conferidos, somando 43.952 registros. Os 61 arquivos do manifesto instalado mantiveram seus tamanhos e hashes. Os três parsers foram executados com sucesso e sua sintaxe foi validada. Não houve alteração de comportamento em Aetherforge; build e testes de runtime não foram necessários para esta entrega documental.

Quando houver autorização do ambiente gráfico, observar primeiro um personagem existente sem gastar recursos: abrir Status, registrar resumo e detalhe, alternar integrante, selecionar habilidades, abrir descrições, cancelar e fechar. Depois verificar leitura em diferentes escalas e altura disponível. Gravar separadamente o que foi visto, a versão e as condições.

Testes que alterem build, pontos, itens ou progresso precisam de uma sessão de teste apropriada e registro de estado inicial/final. Não preencher as lacunas atuais com uma experiência presumida nem editar o save manual de Aetherforge para validar uma tela de referência.
