# Aetherforge: stack e arquitetura da POC

Versão 0.3, 12 de outubro de 2026. A base técnica do M0 foi implementada com Godot 4.7.2 .NET, SDK 10.0.401, cliente net8.0, API net10.0 e domínio netstandard2.1. Os testes iniciais passaram; critérios de desktop e capacidade de produção ainda exigem validação. Evidências em [Validacao_M0.md](Validacao_M0.md).

Atualização de escopo em 13/10/2026: desenvolver o idle primeiro. A arquitetura abaixo preserva a visão completa; serviços de produção, MMORPG e marketplace serão retomados depois do ciclo idle. [Prioridades e ambiente de desenvolvimento](Desenvolvimento.md).

Estado atual V19: a entrada padrão é a expedição local com geometria procedural articulada e cenário 3D modular. Nível/XP e loot inspecionável pertencem à sessão, sem equipamento funcional, salvamento/retomada, offline ou integração com a API. O pipeline de sprites permanece no idle legado; seu equipamento persistido usa JSON e uma conta local de teste. Rigs/assets finais e desempenho da nova apresentação continuam por validar. [Estado corrente e comandos](Desenvolvimento.md); [direção artística e orçamento inicial](visual/Direcao_Artistica_v3.md).

## Recomendação

| Camada | Tecnologia proposta | Motivo e limite |
|---|---|---|
| Cliente MMORPG | Godot 4 estável, edição .NET, C# | Mapas e personagens 3D com rig, câmera isométrica; validar pipeline antes de produzir o catálogo |
| Cliente idle | Godot 4 .NET, C#, renderizador Compatibility | Personagens 3D animados e cenário modular 3D raso; validar janela transparente, recursos gráficos e consumo |
| Integração Windows | APIs de janela do Godot; adaptador Win32 isolado se necessário | Posicionar próximo à barra, lidar com foco, DPI e múltiplos monitores |
| Simulação compartilhada | Biblioteca C# sem dependência da engine, inicialmente netstandard2.1 | Regras de combate, XP, drops e mineração reutilizadas; confirmar compilação nos dois runtimes |
| Mapas online | Godot .NET exportado para servidor dedicado headless | Reutilizar colisão e dados dos mapas; um processo por instância na POC |
| Transporte de combate | ENet/UDP entre clientes Godot e servidor de mapa | Movimento e snapshots; autoridade do servidor e validação de cada comando |
| Conta, idle, inventário e mercado | ASP.NET Core em .NET 10 LTS | Um monólito modular com worker de simulação e tarefas de persistência |
| API de conta | HTTPS/JSON | Login, estado idle, inventário, receita e marketplace; polling curto na primeira versão |
| Persistência | PostgreSQL, versão estável suportada fixada em M0 | Transações, constraints e bloqueios para propriedade e carteira |
| Acesso ao banco | EF Core + Npgsql com versões compatíveis | Migrations e acesso usual; SQL explícito nas operações que exigirem locks |
| Arte | Blender, glTF/GLB, malhas, rigs, animações e texturas | Exportar modelos animados; preservar escala, orientação, pivôs e clips. Atlas de textura pode ser usado para materiais, sem converter personagens em sprites |
| Produção local | Git, solução .NET, testes do domínio, Docker Compose para serviços | Ambiente reproduzível; exportação Godot separada do processo de build da API |
| Hospedagem do teste | Uma máquina Linux para API, PostgreSQL e processos de mapa | Controle de custo; esta configuração não é garantia de capacidade de produção |

O setup já fixa Godot 4.7.2 .NET e SDK .NET 10.0.401 (`scripts/Setup.ps1`, `global.json`); os projetos fixam cliente `net8.0`, API/persistência/checks `net10.0` e domínio `netstandard2.1`. A tabela acima descreve o alvo de arquitetura: PostgreSQL, EF Core/Npgsql, autenticação e serviços de produção continuam planejados. A exportação release ainda exige validação. A biblioteca de regras precisa continuar compatível com cliente e backend, sem APIs exclusivas do backend.

## Comparação das alternativas

| Alternativa | Vantagem para este jogo | Custo ou dificuldade | Decisão proposta |
|---|---|---|---|
| Godot + C# | Engine única para idle e personagens/cenários 3D; servidor headless disponível | Integração fina com desktop e escala de mapas exigem testes próprios | Primeira opção para a POC |
| Unity + C# | Ecossistema grande de ferramentas e produção 3D | Integração Windows continua exigindo trabalho; condições comerciais precisam ser verificadas se houver migração | Reavaliar se equipe ou assets dependerem de Unity |
| Unreal + C++ | Pipeline 3D robusto | Complexidade maior para um idle compacto e duplicação de linguagens com backend C# | Fora da primeira POC |
| WPF + C# no idle | UI e integração Windows maduras | Segundo pipeline de renderização/animação, restrito a Windows | Plano alternativo se a janela Godot falhar nos critérios |
| Electron/stack web no idle | UI web rápida para inventário e painéis | Outro runtime e pipeline gráfico, sem ganho comprovado para o render 3D do jogo | Não adotar antes de medir uma necessidade concreta |

A comparação é julgamento de adequação ao escopo. Não representa medições de consumo ou custo de licença. Os detalhes usados para recomendar Godot, .NET e PostgreSQL foram conferidos nas fontes oficiais ao final.

## Idle na barra de tarefas

O objetivo visual é manter os três integrantes visíveis junto à barra de tarefas. A abordagem inicial é uma janela transparente sem bordas, posicionada sobre ou imediatamente acima dela, configurável pelo jogador. Isso precisa ser reconhecido no produto: não existe aqui uma prova de incorporação nativa do render do jogo dentro da barra do Windows.

O protótipo deve permitir mover o idle, recolher os controles e abrir uma janela convencional de gerenciamento. O adaptador Windows consulta geometria da área de trabalho e responde a mudanças de DPI, monitor e barra. O jogador pode desativar a permanência sobre outras janelas.

M0 verifica Windows 11 com escala 100%, 150% e 200%, barra com ocultação automática, monitor principal e secundário, tela cheia de outro aplicativo e retorno após suspensão. As áreas transparentes não devem bloquear cliques; clicar numa habilidade deve abrir os controles sem interromper a digitação em outra janela quando não houver interação com o jogo.

Metas iniciais no PC de referência, a documentar: animação de 15–30 FPS, memória residente até 250 MB e uso médio de CPU abaixo de 3% ao longo de dez minutos em repouso animado. Medir hardware, GPU, versão do Windows, resolução e processo de renderização junto aos resultados. Se o limite não for atingido, reduzir FPS e trabalho visual; se a integração continuar inadequada, avaliar o shell WPF preservando a simulação C#.

Godot documenta janelas transparentes e configurações de integração desktop; também registra limitações em certas configurações gráficas. Por isso esta verificação abre o backlog. [Criar aplicações desktop](https://docs.godotengine.org/en/stable/tutorials/ui/creating_applications.html), [DisplayServer](https://docs.godotengine.org/en/stable/classes/class_displayserver.html).

## Fronteiras e autoridade

```text
Cliente idle ── HTTPS ──┐
                       ├── API de conta + worker idle ── PostgreSQL
Cliente MMORPG ─ HTTPS ┘              ↑
        │                              │ canal interno autenticado
        └── ENet/UDP ── Servidor de mapa headless
```

O servidor de mapa controla posição válida, colisão, dano, morte, habilidades e encontros. O serviço da conta controla inventário, carteira, XP persistida, quest, produção idle e negociação. Nenhum cliente decide quantidade de recursos ou resultado de uma compra.

O cliente se autentica via HTTPS e recebe um ticket de acesso de curta duração para a instância. O servidor de mapa valida o ticket junto ao serviço de conta. Não transmitir senha ou token de longa duração por UDP; o transporte de gameplay não será tratado como canal cifrado. Segredos e operações de carteira usam HTTPS.

Na POC, o domínio deve aceitar comandos com identificador de operação e versão do estado. No serviço, gravar resultado e alteração no mesmo commit. Repetir a operação retorna o resultado já registrado. Unicidade e validação de saldo no banco continuam necessárias; chave idempotente isolada não resolve duas compras distintas sobre o mesmo item.

## Simulação idle e offline

A biblioteca de regras processa eventos discretos: ataque, habilidade, cura, morte, extração e transição. O servidor conserva estado e gerador aleatório determinístico. Uma atualização online e a recuperação offline usam a mesma rotina.

Processar o tempo offline em blocos com limite de trabalho por requisição. Se o cálculo for longo, o worker continua e a API informa que o resumo está sendo preparado. Não bloquear a thread de uma conexão HTTP por toda a ausência. Cada commit atualiza o instante processado e o estado; uma falha retoma desse checkpoint.

Aplicar desgaste, reparo, teto de moedas e limites diários na data do evento em UTC, inclusive quando um processamento cruza meia-noite. A quota de compartilhamento só muda na transferência explícita do lote, não durante a geração. Bloquear processamento concorrente da mesma expedição com lease/lock no banco e controle de versão.

Oito horas máximas por intervalo de ausência. Persistir `last_online_at` a partir da presença confirmada pelo servidor, separado de `processed_at`. Sem cliente online, o worker só produz até `last_online_at + 8h`; salvar checkpoints não estende essa janela. Ao reconectar após uma ausência maior, concluir a janela elegível e avançar o relógio de produção para o instante atual, sem creditar a lacuna. Presença confirmada retoma produção futura. Manter uma única expedição por conta. Login dos dois clientes não cria um segundo worker, e reinício não permite receber novamente o intervalo já processado.

## Dados mínimos

| Entidade | Campos essenciais e invariantes |
|---|---|
| Conta e personagem | ID, identidade de login, classe, progresso e versão |
| Expedição | Conta única, missão, etapa, encontro, membros, energia, semente, versão das regras, processed_at e last_online_at |
| Item individual | ID único, definição, dono, raridade, durabilidade, vínculo e estado equipado/listado |
| Lote de recurso | Conta, definição, quantidade, origem, vínculo e escopo idle/compartilhado |
| Carteira | Conta, tipo comum/premium, saldo inteiro não negativo |
| Registro econômico | ID de operação, causa, entrada/saída, saldo resultante, item/lote e instante |
| Limite diário | Conta, categoria, data UTC e quantidade consumida |
| Quest | Conta, flags de indícios, projeto, núcleo, escolha, concessões de materiais e conclusão |
| Listagem | Item/lote em custódia, vendedor, preço inteiro premium e estado |
| Pedido idempotente | Conta, tipo, chave única e resultado persistido |

Compra de mercado: bloquear listagem e carteiras em ordem estável, confirmar elegibilidade/saldo, transferir premium, consumir taxa comum, mover propriedade, concluir listagem e registrar operação numa transação. Tratar deadlocks ou conflitos serializáveis com retry limitado. A regra de taxa é a do GDD, sem taxa extra premium.

O PostgreSQL oferece isolamento e bloqueios; a aplicação ainda precisa escolher o nível, escrever constraints e tratar conflitos. [Isolamento de transações](https://www.postgresql.org/docs/current/transaction-iso.html), [Bloqueios explícitos](https://www.postgresql.org/docs/current/explicit-locking.html).

## Rede e escala inicial

Começar com dois jogadores por grupo e uma instância de mapa. Meta de implementação: simulação a 20 ticks/s e snapshots de estado a dez por segundo, com interpolação no cliente. Os valores serão revistos ao medir responsividade e largura de banda. Predição de movimento só entra após o fluxo autoritativo básico funcionar.

Não usar o protocolo multiplayer de alto nível do Godot diretamente em ASP.NET. Ambos os lados da sessão ENet são Godot; ASP.NET recebe chamadas de serviço independentes. O engine não fornece um MMORPG completo pronto: autenticação, persistência, economia, distribuição de instâncias e limites permanecem trabalho do projeto.

Na POC, banco e economia globais podem atender múltiplas instâncias. Depois, medir jogadores por mapa, tempo do tick, latência regional, crescimento do banco e filas. Só então avaliar Redis para cache/presença, múltiplos hosts, serviços separados ou implantação regional. Backups e restauração são necessários antes de testes persistentes com usuários externos.

## Arte e câmera

Usar assets prontos licenciados na POC. Preparar no Blender malhas simples, rig, materiais e clips de repouso, caminhada, ataque, habilidade, dano e queda; exportar glTF/GLB para o Godot. Os personagens são modelos 3D animados em tempo real. Normalizar escala, transformações, orientação e posição dos pés antes de exportar. [Considerações oficiais de exportação](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/model_export_considerations.html).

Primeiro validar um personagem na câmera fixa do idle, com animação, oclusão e contato com o chão; depois o cenário modular e a equipe. No MMORPG, validar o modelo ao girar a câmera. Preservar o protótipo integrado durante essa migração. O catálogo registra origem, licença e atribuição de malhas, texturas, rigs e animações.

Para o idle, começar com um trecho curto de cenário, materiais compartilhados, texturas pequenas e animação a 30 FPS, reduzível a 15. Preferir sombra de contato simples e luz pintada nas texturas; limitar luzes e sombras dinâmicas. Medir draw calls, triângulos, memória, CPU, GPU e custo de skinning. Evitar muitas superfícies transparentes. Godot documenta LOD e MultiMesh; o instancing automático não se aplica ao Compatibility. [Otimização 3D](https://docs.godotengine.org/en/stable/tutorials/performance/optimizing_3d_performance.html). Os limites propostos no estudo v3 são hipóteses de trabalho, não resultados medidos.

## Fontes oficiais verificadas

- [Godot: C# basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html): edição .NET, SDK e exportação desktop.
- [Godot: multiplayer de alto nível](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html): transportes e ENet.
- [Godot: servidores dedicados](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_dedicated_servers.html): execução headless e exportação sem recursos visuais.
- [Microsoft: suporte do .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core): .NET 10 é LTS, com suporte previsto até novembro de 2028.
- [Microsoft: WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/): UI desktop e restrição a Windows, usada para avaliar a alternativa de shell.

Links apontam para documentação atualizável. Fixar versões, arquivos de lock e evidência de compatibilidade no repositório ao executar M0.
