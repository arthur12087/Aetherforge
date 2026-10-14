# Aetherforge: crítica da v3 com Taste Skill v2

## Método e versão

Skill exato: taste-reference/SKILL.md, copiado da versão completa indicada pelo usuário. Scripts sem modificação: lint.mjs e inspect.mjs. Referências lidas: copy.md, critique.md, type.md, colour.md, layout.md, motion.md e imagery.md (fontes/licenças e fragmentos data-ui).

Direção page-is-the-object, seed 1337574584, Archivo, --keep palette. Sheet preservado em .taste-skill/direction.json.

Redraws já fornecidos:
- Hero invitation substituído por striped-card-beside-room: exigia casamento/retrato inexistentes.
- Proof guest list substituído por recognition-table: o conteúdo real são créditos de fontes.

Adaptações de conteúdo, sem sorteio adicional:
- Hero abre a prancha 3:1 em largura total para cumprir a prioridade explícita de imagem dominante e íntegra; uma metade vertical cortaria a cena ou reduziria sua presença.
- A faixa vem do escudo; não se repete como ornamento de casamento.
- Fichas são arquivos de expedição preenchidos; não uma papelaria nupcial. Conteúdo de produção substitui objetos sem relação.
- Referências têm suas proporções reais, sem cortes; suas tiras inferiores têm créditos, não labels fictícios.
- Verso do dossiê contém arquivos de handoff; não endereços, selos ou contatos inventados.

## Diagnóstico anterior

Inspector baseline em 1440, 768 e 390 px: 9 grupos FAIL e 6 WARN. Falhas: mono, numbering, micro-label, hairline, outlined-box, body-size, small-text, still-page e eyebrow. Avisos: card-rows, font-count, size-count, first-action, no-footage, measure.

Capturas examinadas: todos os folds e sheets em .taste-skill/baseline/. O painel mantém informação, mas a imagem só surge após muita navegação no telefone, tem miudezas de 10-14 px e três fontes. O novo objeto editorial deve substituir a ornamentação por escala e conteúdo.

ROUND 0 (baseline v2)
Seen in one second: Aetherforge em texto, depois duas pranchas de mesmo peso.
Idea (one sentence): Painel de comparação de arte, ainda sem objeto específico organizando o conteúdo.
Live thing: Ampliar imagens, sem ensaio de estado e largura.
Layout words by section: hero=split / pranchas=dupla / materiais=grid / escala=split / referências=grid / handoff=trio.
Scores: design 6.3 · usability 6.5 · creativity 6.0 · content 8.0 → overall 6.43, limitado a 6.0.
Caps applied: FAIL vigente: máximo 6.0; mono, labels e hairlines: máximo 6.5.
Top three problems (most visible first), each with the screenshot it shows in and the fix:
1. mobile-fold.jpg: navegação e labels empurram a arte para baixo. Corrigir com uma linha de navegação, título curto e cena dominante.
2. desktop-page-01.jpg: miudezas, linhas e caixas competem com a pintura. Eliminar essa ornamentação e subir o texto para 16-18 px.
3. desktop-page-02.jpg: encaminhamento ainda fala de atlas/sprites. Substituir pela decisão vigente: GLB, rig e perfil 3D real.

## Referências e proveniência

ROUND 1 (v3 estática)
Seen in one second: A cena industrial planar domina o dossiê no desktop.
Idea (one sentence): O registro das Galerias da Caldeira organiza as pranchas, decisões e arquivos de produção.
Live thing: Largura 800/960, compacto/expandido e equipe do HUD; controles CSS preservam funcionamento sem JS.
Layout words by section: hero=prancha-dossie / faixa=palco / modelagem=fichas / referências=prints / origem=tabela / produção=lista / footer=verso.
Scores: design 7.8 · usability 7.1 · creativity 7.5 · content 8.4 → overall 7.57, limitado a 6.0.
Caps applied: FAIL covered-text e still-page; score máximo 6.0 até corrigir.
Top three problems (most visible first), each with the screenshot it shows in and the fix:
1. tablet-page-02.jpg: o rodapé sticky aparece sob conteúdo e tem o título escondido. Retirar a sobreposição e usar entrada do verso por transform, mantendo o sentido de curtain-footer sem cobrir texto.
2. mobile-fold.jpg: a cena larga vem depois de título, parágrafo e ação. Colocar a arte primeiro no telefone, sem cortar seu 3:1.
3. mobile-page-01.jpg: rota e etapa se encostam no HUD. Separar os spans em linhas.

Também corrigir a declaração CSS de font-weight para 800 (o eixo variable estava correto, mas o peso calculado era 700) e construir a motion editorial em partes, com o conteúdo sempre visível por padrão. Não são animações de jogo.

ROUND 2
Seen in one second: A cena planar abre o registro no telefone; no desktop é a massa dominante.
Idea (one sentence): A galeria e seu trio dão conteúdo específico às fichas de expedição, não a uma landing page.
Live thing: Preview raster com quatro combinações de largura/estado e informações de equipe, sem jogo simulado.
Layout words by section: hero=prancha-dossie / faixa=palco / modelagem=fichas / referências=prints / origem=tabela / produção=lista / footer=verso.
Scores: design 7.9 · usability 8.0 · creativity 7.7 · content 8.5 → overall 7.99, limitado a 6.0.
Caps applied: Inspector ainda apontou still-page (7 elementos amostrados); não declarar aprovação.
Top three problems (most visible first), each with the screenshot it shows in and the fix:
1. desktop-page-03.jpg e medição de motion: entradas por Web Animations terminam antes da amostra do inspector, e a camada ficou abaixo do gate. Usar keyframes CSS para as mesmas entradas reais, mantendo variação e movimento contido, e medir novamente.
2. desktop-page-01.jpg: a tabela de geometria chega antes da ressalva de orçamento. Acrescentar a ressalva visível ao lado da tabela.
3. mobile-page-01.jpg: a apresentação de tabela vira blocos, e o inspector confunde tabelas/prints com cards. O orçamento permanece uma tabela sem ornamentos; defender o aviso se não houver repetição de widgets de marketing.

| Arquivo local | Título | Autor | Licença / origem | Crédito na página |
| --- | --- | --- | --- | --- |
| conceitos/05-idle-3d-leve-v3.png | Galerias da Caldeira v3 | GPT Images, fornecido | Conceito existente; nenhum novo asset gerado | Abertura |
| conceitos/06-kit-modelagem-3d-v3.png | Kit de modelagem v3 | GPT Images, fornecido | Raster conceitual; não contém GLB/rig | Ficha Identidade |
| referencias/dribbble-steampunk-lab-roman-klco.png | Steampunk Lab | Roman Klčo | https://dribbble.com/shots/16638158-Steampunk-Lab ; licença de reutilização não presumida | Referências e tabela |
| referencias/dribbble-boiler-room-reijo.png | Boiler Room | Reijo | https://dribbble.com/shots/15379548-Boiler-Room ; licença de reutilização não presumida | Referências e tabela |
| referencias/dribbble-den-of-summoning-reijo.png | Den of Summoning | Reijo | https://dribbble.com/shots/15497736-Den-of-Summoning ; licença de reutilização não presumida | Referências e tabela |
| referencias/frostpunk-2-oficial-01.jpg | Frostpunk 2, cidade | 11 bit studios | https://store.steampowered.com/app/1601580/Frostpunk_2/ ; direitos dos titulares | Referências e tabela |
| referencias/frostpunk-oficial-02.jpg | Frostpunk, posto isolado | 11 bit studios | https://store.steampowered.com/app/323190/Frostpunk/ ; direitos dos titulares | Origem |
| referencias/taskbar-hero-oficial-01.jpg | Task Bar Hero, arena/inventário | Nugem Studio | https://www.nugemstudio.com/en/games/taskbar-hero ; direitos dos titulares | Origem |
| assets/fonts/Archivo-variable.ttf | Archivo variable | Archivo Project Authors / Omnibus-Type | SIL OFL 1.1, assets/fonts/OFL-Archivo.txt ; https://github.com/google/fonts/tree/main/ofl/archivo | Tabela de origem |

Fontes da paleta: https://colorhunt.co/palette/222831393e46948979dfd0b8 . Alternativas anteriores registradas em brand-spec.md, não aplicadas.

Imagens externas são referências de estudo, sem licença presumida e sem importação de assets no runtime. Todos os originais foram mantidos. Nenhuma imagem foi gerada nesta revisão.

## Limitações reais e revisão de produção

As pranchas são estáticas. Nenhum footage válido do projeto existe neste material, portanto não inventar um vídeo de jogo. A motion editorial serve orientação e feedback, não demonstra animações de personagens.

Os orçamentos são propostos e não medidos. 30/15 FPS, até seis atores, menos de 120 mil triângulos, menos de 80 draw calls, envelopes de malha e texturas 512/1024 precisam de perfil no Godot Compatibility. 250 MB/CPU 3% continuam metas anteriores não garantidas. Esta revisão não alterou código do jogo nem implementou migração.

## Crítica final e evidência

ROUND 3
Seen in one second: A prancha das Galerias da Caldeira, com três adultos e ameaças mecânicas.
Idea (one sentence): O registro de expedição das Galerias da Caldeira é o objeto que agrupa cena, fichas de construção, fontes e encaminhamento.
Live thing: Preview raster estático em 800/960 px e compacto/expandido, com HUD de rota/etapa/equipe e disclosure funcional; nenhuma simulação de combate.
Layout words by section: hero=prancha-dossie / faixa=palco / modelagem=fichas / referências=prints / origem=tabela / produção=lista / footer=verso.
Scores: design 7.8 · usability 8.0 · creativity 7.6 · content 8.6 → overall 7.90.
Caps applied: Nenhum FAIL final. Sem cap por motion: 53 elementos com entradas em desktop e tablet, 45 em mobile. A inspiração de pasta/dossiê é conhecida; criatividade deliberadamente abaixo de 8.0.
Top three problems (most visible first), each with the screenshot it shows in and the fix:
1. mobile-fold.jpg: a arte 3:1 ocupa só 114 px de altura quando inteira. A escolha preserva a obra; a ampliação funciona, mas a revisão de silhuetas em pixels reais deve ser feita no desktop ou no motor. Não recortar a cena para preencher uma tela vertical.
2. desktop-page-01.jpg: no compacto, a arte precisa de margens porque sua proporção diverge de 800 × 180. O preview revela esse problema em vez de escondê-lo. A correção é de câmera/composição 3D futura, não de crop no painel.
3. desktop-fold.jpg: a roda monumental continua competindo com o trio. As notas mandam provar a função da peça ou simplificá-la na produção; não alterar arbitrariamente o raster fornecido.

### Os quatorze passes

| Passagem | Observação das capturas finais |
| --- | --- |
| Um segundo | Cena industrial e escudo, com uma ação de ver a faixa. |
| Squint | Fundo carvão, folha em ferro e imagem larga formam massas claras; não um mosaico de widgets. |
| Cobrir a marca | Trio, escudo marcado, galeria e saqueadores situam o objeto em Aetherforge. |
| Bordas | Margem externa, borda da folha e coluna interna são reutilizadas; sem grid desenhado. |
| Espaço | Separação entre capítulos maior que os intervalos internos. Fichas se agrupam; nenhum segundo repouso vazio foi medido. |
| Tipo | Archivo carregada 100%; cinco tamanhos no desktop, corpo 18 px, 16 px no telefone. Todos os títulos ficam em até duas linhas. |
| Cor | Color Hunt mantida; carvão e ferro dominam, texto integral claro. Ação em lona e marca do escudo restritas. |
| Imagens | Duas pranchas atuais e seis referências visíveis, mais histórico recolhido. Arte íntegra; sem placeholders e sem footage inventado. |
| Repetição | Prancha, palco, arquivo em fichas, prints de pesquisa, tabela e lista de entrega têm funções e geometrias distintas. |
| Telefone | Arte vem primeiro; HUD quebra rota/etapa, controles cabem e orçamento vira blocos legíveis. |
| Densidade | Cada seção entrega a informação pedida. Relatório e fontes ficam em documentos, sem marketing ou métricas fictícias. |
| Substância | Direção full 3D vigente, limitações de raster, geometria/material/rig propostos e migração pendente estão expressos. |
| Colisões | Zero overflow, zero texto coberto, zero imagens quebradas nas três larguras. O final do telefone foi examinado adicionalmente. |
| Concorrente e live thing | O mecanismo de arquivo é familiar, mas a cena e seu handoff pertencem a este projeto. A largura/estado responde ao limite real de taskbar. |

### Gate final

- Lint exato: zero findings em index.html. Evidência: .taste-skill/lint-final.json.
- Inspector exato, sem --allow e sem alteração de regras: 0 FAIL, 2 WARN. Evidência: .taste-skill/round-3/report.json.
- Larguras: 1440, 768 e 390 px; overflow 0 nas três; console sem erros.
- Títulos: hero 2/1/2 linhas; todos os títulos até duas linhas.
- Fonte: Archivo 100%, peso de display 800, eixo wdth 62.
- Tamanhos desktop: 16/18/24/56/88 px.
- Motion: entradas de título/texto, registros laterais, imagens com escala .985 → 1 e verso subindo. Sem loop ou animação de atores. Conteúdo visível por padrão; redução de movimento elimina as entradas.
- Contraste do texto principal: 9.78:1 no carvão e 7.10:1 no ferro. A ação usa tinta mais escura derivada em OKLch; hover troca foreground/background juntos e aumenta contraste.
- Verificação adicional com a biblioteca CDP do mesmo skill: 79 checks aprovados, incluindo links locais, IDs únicos, quatro estados do preview, contain, disclosure, 13 ampliações, Escape/foco, arquivo direto, no-JS e reduced-motion. Evidência: .taste-skill/local-verification.json.
- Capturas examinadas: todos os folds e sheets de cada round, mais .taste-skill/mobile-end.jpg. O inspector limita a caminhada a 12 telas; o encerramento mobile foi aberto separadamente para não presumir o conteúdo abaixo desse limite.
- A versão anterior está em index-v2.html; pranchas antigas e arquivos prévios preservados. Sem código de jogo modificado.

### WARNs defendidos

- **no-footage:** este pedido é revisão estática de conceitos raster. Não existe clip válido fornecido do projeto; acrescentar footage genérico ou inventado confundiria intenção artística com jogo implementado.
- **card-rows no mobile:** as ocorrências ficam nas tabelas reais de orçamento e créditos (três/quatro linhas alternadas preenchidas), não em cards de marketing. São os dados pedidos, com leitura móvel e sem ornamentos; linhas de table são uma exceção documentada do próprio skill.

### Decisões e pendências

O curtain-footer sticky foi tentado e rejeitado pelo inspector por esconder o título. O verso agora entra em fluxo normal com transform de 32 px, sem sobreposição. As fichas também ficam em fluxo para preservar leitura. A galeria tem quatro obras principais em vez de inflar o arquivo a 8-16 tiles; as demais referências têm papel próprio na tabela e na comparação. As imagens ficam no enquadramento original, mesmo quando a proporção não coincide com o blueprint. São adaptações ao conteúdo e às prioridades explícitas, sem novo sorteio de direção.

O gate aprovado verifica frontend. Não comprova peso da cena, qualidade de rig, FPS ou custo de motor. A próxima entrega é GLB + rig com pose de descanso, material compartilhado e um trecho real de perfil. A roda, a câmera, as diferenças entre vistas e a presença do chefe devem ser resolvidas nessa produção.

O raster compacto preserva a arte 3:1 por contain e deixa margens. A câmera realtime terá de ser recomposta para heróis em 48-64 px. A roda monumental do fundo precisa de função ou simplificação. Chefe completo, rig, pesos, UVs e GLBs são trabalho de produção pendente.

Conteúdo demonstrativo: o HUD mostra a etapa Trilhos Partidos (identificada na direção v3 do projeto) e o trio existente apenas para estudo espacial. Não há combate, progressão, mineração ou métricas simuladas.
