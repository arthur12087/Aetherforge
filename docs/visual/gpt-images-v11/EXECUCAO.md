# Execução GPT Images v11

**Continuação posterior:** após nova orientação do usuário para se inspirar mais em BioShock 1 e 2, a revisão [v12](../gpt-images-v12/EXECUCAO.md) gerou e importou três novas pranchas. Este relatório preserva o histórico da v11 e do estudo R03.

**Três pinturas iniciais geradas, inspecionadas e importadas. Após a revisão do usuário, a direção estética está em revisão: as imagens foram consideradas realistas demais.** Foi produzida uma nova candidata estilizada da equipe, G01-r03, descrita ao final. Total atual: seis chamadas; aguardar orientação antes de novas gerações. A execução inicial ocorreu em 20/10/2026 à noite em São Paulo, entre 00:51 e 00:57 UTC de 21/10/2026, em cinco chamadas: G01, correção de G01, G02, correção de G02, G03. A tabela seguinte preserva a seleção inicial.

## Arquivos selecionados

| Job | Pintura original selecionada | Dimensão real | Estado |
|---|---|---|---|
| G01 — equipe | [01-equipe-r02.png](../../../assets/concepts/brass-v11-gpt/generated/01-equipe-r02.png) | 1536 × 1024 | Gerado, corrigido e inspecionado |
| G02 — bestiário I | [02-bestiario-1-r02.png](../../../assets/concepts/brass-v11-gpt/generated/02-bestiario-1-r02.png) | 1672 × 941 | Gerado, corrigido e inspecionado |
| G03 — bestiário II | [03-bestiario-2.png](../../../assets/concepts/brass-v11-gpt/generated/03-bestiario-2.png) | 1672 × 941 | Gerado e inspecionado |

As dimensões são as dos PNGs retornados pela ferramenta. Não houve upscale, deformação ou recorte para alegar 2K/16:9. G01 usa a paisagem 3:2 permitida pelo handoff; G02/G03 têm proporção aproximadamente 16:9. Todas preservam papel quente, fuligem, tabaco, vinho oxidado, linho/porcelana e latão pontual, sem campo azul/petróleo dominante.

## Ferramenta e proveniência

- Ferramenta real: `image_gen.imagegen`, binding `image_gen__imagegen`, integração nativa OpenAI / GPT Images desta sessão. A versão/identificador do modelo e o request ID do provedor **não foram expostos**. Não se atribui uma versão GPT Image específica.
- Origem: **geração externa na CLI**. Não foi usado `generate_canvas_media`; a geração externa não comprova reparo da conexão do editor.
- [Registro completo](../../../assets/concepts/brass-v11-gpt/provenance.json): prompts efetivamente enviados, horários antes/depois de cada chamada, referências anexadas, nomes/identificadores de arquivo retornados, dimensões, SHA256 dos PNGs e dos prompts, seleção e limitações. Nenhuma credencial incluída.
- Os três prompts iniciais foram enviados integralmente, sem duplicar o brief comum: [G01](01-equipe.txt), [G02](02-bestiario-1.txt), [G03](03-bestiario-2.txt). Os hashes dos arquivos conferiram com o [plano](execution-plan.json). Apenas espaços/quebras de linha finais da saída de leitura foram removidos na submissão; o texto submetido e seu hash UTF-8 constam no registro.
- Prompts de correção completos: [G01-r02](01-equipe-r02.txt), [G02-r02](02-bestiario-1-r02.txt). Cada correção recebeu somente seu próprio PNG inicial como alvo. As gerações iniciais não receberam anexos raster.
- Foram abertas e consultadas as seis imagens v10 indicadas no handoff: menus, painel de pesquisa e quatro referências de Colin Fix/James Ku. Nenhuma imagem de terceiros ou maquete v6/v9 foi anexada, recortada ou composta nas pinturas.
- Os cinco PNGs retornados foram copiados binariamente para `assets/concepts/brass-v11-gpt/generated/`, preservando tanto os primeiros resultados quanto as correções. Os originais na pasta padrão da ferramenta também foram mantidos.

## Inspeção e correções

As saídas foram vistas na conversa e abertas a partir dos arquivos locais. O visualizador mostrou apenas uma faixa superior ao abrir diretamente estes PNGs; cópias recodificadas com `System.Drawing`, em [qa/](../../../assets/concepts/brass-v11-gpt/qa/), permitiram abrir integralmente e revisar as três seleções. São derivados exclusivos de inspeção, sem repintura, substituição da geração ou uso como fonte do canvas. Os PNGs nativos e seus metadados permanecem intactos em `generated/`.

**G01:** três adultos inteiros e distintos, na ordem editorial Pistoleiro, Guarda, Alquimista. Rifle com madeira/latão, faixa diagonal clara, escudo retangular com uma faixa ocre, face humana do Guarda, capuz/capa clara e satchel do Alquimista. A primeira versão acrescentou cápsulas e reservatórios cilíndricos; a correção deixou uma cápsula discreta no aplicador, pack plano e fundo sem arcos grandes. Sem membro fundido ou texto legível identificado na seleção. Limitações: parte do pack e a pega do escudo ficam ocultas; muitas fivelas/desgastes; o motivo Arco Partido ainda varia entre placas. Conexões internas não foram validadas.

**G02:** exatamente Rato, Saqueador e Drone, com corpos/cauda/gancho/rotores inteiros e separados. A correção baixou e alongou o roedor e reduziu a fileira de cristais a poucos depósitos; preservou o gancho assimétrico, avental e sino com dois rotores protegidos e lente inferior. Sem texto legível identificado. Limitações: o Rato ocupa menos área pela postura baixa; bigodes e rotor direito têm margem curta, embora não cortados; a engenharia dos rotores é apenas sugestão pintada.

**G03:** exatamente Sentinela larga/baixa com venezianas e punhos, Vigia de porcelana com três apoios legíveis e Capataz largo com braço-martelo assimétrico e fornalha pequena. Apoios e ferramentas dentro do quadro, sem texto legível identificado. Limitações: Capataz tem maior área editorial, e a margem esquerda da Sentinela é curta. Articulações, conexões e colisões foram avaliadas visualmente, sem rig ou prova técnica.

Não foi feita a prova opcional a 64–96 px por sujeito. Pinturas detalhadas ainda precisarão de simplificação e validação em volume antes de eventual uso no palco pequeno. Não há aprovação de cânone, personagem final, modelo, rig, animação, arena ou gameplay.

## Importação e verificação do canvas

Importado no board `ab163f7a-13a0-4f45-b5c5-ba7638f4083d`, com novos assets e nós `type: "asset"`, `mediaType: "image"`:

- `v11-gpt-equipe` → `generated/01-equipe-r02.png`;
- `v11-gpt-bestiario-1` → `generated/02-bestiario-1-r02.png`;
- `v11-gpt-bestiario-2` → `generated/03-bestiario-2.png`.

Títulos identificam **conceito GPT gerado na CLI**. Descrições apontam para a proveniência e registram aprovação pendente. O manifesto e o board passaram de 43 para 46 entradas cada. Nenhum nó/asset anterior foi alterado; prompts, configurações e histórico v10 foram preservados. `canvas/jobs.json` e `canvas/index.json` não foram editados.

Índice, manifesto, board e schemas foram relidos imediatamente antes da importação. A edição acrescentou somente as novas entradas, preservando o restante do texto. Comparações dos bytes antes da escrita e leitura posterior não detectaram mudanças concorrentes durante a operação.

`canvas_check` não existe entre as ferramentas desta sessão e não foi simulado como comando. A validação local passou: JSON sem chaves duplicadas, contratos dos schemas locais de índice/manifesto/board, unicidade de IDs, referências de nós/assets/documentos, edges e existência dos caminhos. [Evidência com contagens e hashes](canvas-validation.json). O helper local usado foi `artifacts/complete-v11.py`; não faz parte do runtime.

## Estados finais

- **Gerado:** G01, G02 e G03 com arquivos reais.
- **Corrigido:** G01 e G02, uma correção dirigida por job; originais preservados.
- **Inspecionado:** três seleções abertas integralmente e revisadas; limitações acima.
- **Importado:** três assets e três nós novos, com validação local aprovada.
- **Pendente:** escolha/aprovação artística do usuário e eventual validação específica dentro do editor.
- **Bloqueado:** nenhum job obrigatório de geração ou importação. A conexão antiga do editor não foi diagnosticada nem declarada corrigida.

Sem alterações no runtime, domínio, API, persistência, multiplayer ou saves. Não foi necessário compilar/testar gameplay. Não foram alteradas permissões globais, ACLs, sandbox, antivírus ou credenciais.

## Revisão estética — G01-r03

Feedback posterior do usuário: “as imagens estao boas mas estao realistas demais, dando a cara de AI Slop”. A inspeção técnica inicial não significou aprovação do acabamento artístico.

A **sexta chamada real** produziu [01-equipe-r03-estilizada.png](../../../assets/concepts/brass-v11-gpt/generated/01-equipe-r03-estilizada.png), PNG nativo de **1672 × 941**. Foi gerada do zero, sem anexar a versão realista, com [prompt revisado completo](01-equipe-r03-estilizada.txt). A ferramenta não expôs versão do modelo nem request ID. Os horários, nome de arquivo retornado e hashes constam na proveniência. Nenhum resultado anterior foi sobrescrito.

Mudança de linguagem: proporções intencionais, rostos angulares, mãos e botas mais pesadas, poucas massas de valor, pinceladas amplas, menor número de fivelas e menor microtextura. Paleta quente v10 e os identificadores funcionais dos três personagens preservados. O reservatório é plano, o escudo é retangular com uma faixa ocre, o rifle tem madeira/latão e o Alquimista usa uma cápsula jade única.

Inspeção: imagem gerada vista integralmente e [prévia JPEG local](../../../assets/concepts/brass-v11-gpt/qa/01-equipe-r03-preview.jpg) aberta no visualizador. A prévia é apenas derivado de inspeção, reduzido para 1254 × 706; o canvas usa o PNG original. Há três adultos inteiros e separados, sem texto legível identificado. Limitações: margem superior curta no capacete e capuz, sem corte; ainda existe desgaste pintado; pega do escudo oculta. Não há prova de rig, anatomia 3D ou leitura no runtime.

Importação comparativa: novo asset/nó **`v11-gpt-equipe-r03-estilizada`**, ao lado da equipe anterior, com título de conceito GPT gerado na CLI e aprovação pendente. Índice, manifesto, board e schemas foram relidos antes da escrita. IDs, assets e nós anteriores preservados; sem mudanças em `jobs.json`, runtime ou permissões. [Validação local desta revisão](canvas-validation-r03.json): contratos, IDs, referências, caminhos e preservação do conteúdo anterior. `canvas_check` continua indisponível.

**Estado atual:** equipe R03 gerada, inspecionada e importada como candidata; aprovação estética pendente. Os dois bestiários continuam na linguagem anterior, cuja revisão estética está pendente. Limite de seis chamadas do handoff atingido; nenhuma geração adicional foi iniciada.
