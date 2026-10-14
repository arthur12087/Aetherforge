# Aetherforge: direção artística v3

## Decisão atual

Em 13/10/2026, o usuário definiu personagens 3D animados em tempo real. Também autorizou cenário 3D no idle, desde que leve, retirando a restrição provisória de cenário 2D. Essa decisão vale para o alvo de produção; o protótipo M0 ainda usa sprites.

O tom continua pesado, sinistro e triste. Frostpunk e Frostpunk 2 orientam a relação entre pessoas vulneráveis e indústria opressiva. Task Bar Hero orienta a apresentação compacta, a marcha e os encontros automáticos. A v3 usa formas 3D simplificadas, materiais foscos e pintura de grandes massas para aproximar conceito e produção.

## O problema da v2

A v2 escureceu a atmosfera, mas ainda dependia de pintura detalhada e de muitas pequenas marcas de superfície. Esse detalhe perde utilidade quando os personagens ocupam poucas dezenas de pixels. Também não mostrava como transformar as imagens em modelos animados.

O painel anterior foi auditado com a [Taste Skill](https://www.tasteskill.dev/), usando o [SKILL.md do repositório oficial](https://github.com/Leonxlnx/taste-skill/blob/main/skills/taste-skill/SKILL.md). O lint registrou 42 ocorrências de falha; o inspector agrupou nove categorias de falha e seis avisos. Entre os problemas: rótulos monoespaçados, números decorativos, muitas linhas finas, textos de 10–14 px, três famílias tipográficas e pouca hierarquia da imagem principal.

Essas contagens avaliam a página de estudo, não a qualidade artística do jogo. A skill é de frontend, explicitamente voltada a páginas públicas. Ela será aplicada à apresentação e à revisão visual; a interface compacta do jogo mantém suas convenções funcionais no fragmento `data-ui`.

## Referências escolhidas

As referências são material de estudo. Não há licença presumida para importar imagens ou modelos de terceiros no runtime.

### Color Hunt

| Paleta publicada | Papel no estudo | Aplicação proposta |
|---|---|---|
| [Carvão, aço, lona e osso](https://colorhunt.co/palette/222831393e46948979dfd0b8): `#222831`, `#393E46`, `#948979`, `#DFD0B8` | Base escolhida | Fundo frio, massas metálicas, roupa do healer e leitura de texto |
| [Oliva e cal](https://colorhunt.co/palette/181c143c3d37697565ecdfcc): `#181C14`, `#3C3D37`, `#697565`, `#ECDFCC` | Alternativa de região | Cor de deterioração e ambiente úmido; não substituir toda a paleta por uma cor de habilidade |
| [Aço, cobre e lona](https://colorhunt.co/palette/2c36393f4e4fa27b5cdcd7c9): `#2C3639`, `#3F4E4F`, `#A27B5C`, `#DCD7C9` | Alternativa de material | Ferragens e mineração, com cobre localizado |

A paleta estabelece relações de valor e proporção; não significa que todas as superfícies sejam quatro cores chapadas. Carvão e aço dominam; lona separa o healer; osso é reservado à leitura. A faixa desgastada do escudo é um acento pontual. Luz quente e éter continuam discretos.

### Dribbble

| Referência e autor | Observação da imagem | O que levar para Aetherforge |
|---|---|---|
| [Steampunk Lab, Roman Klčo](https://dribbble.com/shots/16638158-Steampunk-Lab), [imagem](referencias/dribbble-steampunk-lab-roman-klco.png) | Construção com volumes cilíndricos, tubulações e peças reconhecíveis num pequeno conjunto 3D | Organizar módulos como formas reconhecíveis. Reduzir brilho e saturação para o tom do jogo |
| [Boiler Room, Reijo](https://dribbble.com/shots/15379548-Boiler-Room), [imagem](referencias/dribbble-boiler-room-reijo.png) | Caldeiras e peças agrupadas por função; luz quente concentra o olhar | Um marco por encontro e conexões mecânicas com propósito. Reduzir o detalhe fino e as superfícies luminosas |
| [Den of Summoning, Reijo](https://dribbble.com/shots/15497736-Den-of-Summoning), [imagem](referencias/dribbble-den-of-summoning-reijo.png) | Canto industrial compacto, objetos em camadas e grande massa escura ao redor | Profundidade rasa, áreas de silêncio e um foco luminoso restrito |

As referências mostram composição e linguagem de volumes. Não comprovam desempenho e não são exemplos já importados no Godot. O primeiro candidato pesquisado, Low Poly Factory WIP, não foi usado como prova visual porque sua imagem não pôde ser inspecionada diretamente.

## Identidade proposta

Uma equipe de três sobreviventes atravessa as Galerias da Caldeira. O escudo retangular do Guarda, com uma única faixa de pintura gasta, é a forma central. A leitura começa nessa silhueta; segue para o rifle do DPS e para a lona clara e a ampola do healer.

| Função ou objeto | Regra de forma | Regra de material |
|---|---|---|
| Guarda | Escudo largo, ombros simples, pernas separadas e postura sob carga | Aço fosco com poucas marcas grandes |
| Pistoleiro | Casaco em painéis largos, rifle espesso o suficiente para a barra | Tecido frio, madeira e aço sem brilho intenso |
| Alquimista | Capuz, mochila médica e gesto dirigido ao aliado | Lona mais clara; uma ampola de éter |
| Saqueador | Corpo angular, membro com gancho ou pinça | Sucata e reparos assimétricos |
| Sentinela | Massa larga de caldeira e pistões | Metal escuro; abertura de calor pequena |
| Cenário | Passarela, arco, cano, carrinho e caldeira reutilizáveis | Textura de massas, sem modelar cada marca de desgaste |

Proporções adultas. O baixo número de polígonos deve aparecer como decisão de forma, sem transformar os personagens em brinquedos. Ruído de ferrugem, pequenos rebites e costuras não devem competir com escudo, arma e gesto.

## Pranchas novas

- [Painel visual v3](index.html): inclui prévia conceitual da faixa em 800/960 pixels, estados compacto/expandido, imagens ampliáveis e referências creditadas.
- [Cena idle em 3D leve](conceitos/05-idle-3d-leve-v3.png): conceito panorâmico da etapa Trilhos Partidos. Foi refinado para combinar com a linguagem facetada do kit, mantendo as três funções, dois inimigos e o chão contínuo.
- [Kit de modelagem](conceitos/06-kit-modelagem-3d-v3.png): vistas dos três heróis, módulos de cenário e dois inimigos essenciais. Serve para orientar massas, materiais e proporções.
- [Prompts GPT Images v3](Prompts_GPT_Images_v3.md): geração integrada e refinamento com referência explícita ao kit. Não foi usado o CLI/API de imagens.

As imagens são rasters gerados com GPT Images. Não são GLB, malhas, rigs nem animações utilizáveis. A cena panorâmica indica aparência e enquadramento; o kit indica simplificação para a produção. As diferenças entre vistas ainda precisam de normalização por quem modelar.

## Câmera e cenário 3D do idle

Começar com câmera lateral fixa em três quartos, preferencialmente ortográfica, com pequena inclinação para mostrar piso, pés e faces laterais. A equipe avança num corredor raso. O jogador deve perceber passagem entre módulos durante a marcha e estabilidade durante o combate.

A prancha panorâmica tem proporção próxima de 3:1 e enquadramento mais próximo para avaliar materiais. A faixa de produção deve ser recomposta na câmera, com altura dos atores medida no viewport. Não recortar essa imagem para 800 × 180 como se fosse um asset final; a comparação de escala do painel preserva a imagem e serve de inspeção conceitual.

Manter apenas o trecho visível e pequenos trechos adjacentes. Reutilizar passarela, arcos e canos, alterando um marco de cenário por etapa. Evitar cidade completa, geometria de interiores invisíveis, cabelo complexo, roupa simulada e partículas contínuas. A animação do próprio rig transmite o peso.

O ambiente não precisa de uma textura cinematográfica para parecer triste: cabides vazios, um carrinho parado, uma roda quebrada e uma lanterna isolada fazem esse trabalho. Os atores devem ter um fundo de valor diferente atrás da silhueta.

## Envelope inicial de produção

**Todas as quantidades abaixo são propostas não medidas.** Devem ser revistas após o primeiro personagem e o primeiro trecho rodarem no Godot. Cumprir uma contagem de polígonos não garante uso de CPU, GPU ou memória.

| Recurso | Envelope inicial | Verificação |
|---|---|---|
| FPS do idle | 30 ativo; modo reduzido a 15 | Frame time e regularidade ao lado de outros aplicativos |
| Atores visíveis | Três integrantes e até três inimigos; chefe substitui o grupo comum | Custo de animação e skinning |
| Triângulos | Até 120 mil visíveis como teto inicial de estudo | Medir cena completa; priorizar redução onde houver custo |
| Draw calls | Até 80 como alvo inicial | Registrar materiais, superfícies e instâncias reais |
| Herói | 2–4 mil triângulos por LOD inicial | Avaliar deformação e leitura em 48–64 px |
| Inimigo comum | 1–3 mil triângulos | Manter diferença de silhueta |
| Chefe | 6–10 mil triângulos | Caber inteiro; tamanho horizontal e gesto legível |
| Texturas | 512/1024 como ponto de partida, reaproveitadas por materiais | Medir resolução, mipmaps, compressão e memória total |
| Luz e sombra | Luz de ambiente, cor pintada e sombra de contato simples; poucas luzes dinâmicas | Comparar custo e legibilidade; testar no Compatibility |
| Memória/CPU | Manter metas anteriores de 250 MB residentes e média de CPU abaixo de 3% em dez minutos | Documentar hardware, Windows, GPU, DPI e método de cálculo; metas ainda não garantidas |

Godot documenta custo de transparências, luzes, skinning e estratégias de LOD. Seu instancing automático é específico do Forward+; não assumir que ele resolve draw calls no Compatibility. [Documentação de otimização 3D](https://docs.godotengine.org/en/stable/tutorials/performance/optimizing_3d_performance.html).

## Pipeline e migração

1. Preparar um integrante com escala, orientação, malha, materiais e rig. Exportar glTF/GLB e verificar transformações e pose de descanso.
2. Validar repouso, caminhada, ataque, habilidade, dano e queda, com pés no chão e sem clipping do equipamento.
3. Montar um pequeno corredor modular e comparar iluminação simples, sombra e ausência de transparências excessivas.
4. Medir a equipe completa com inimigos e efeitos. Registrar custo, depois ajustar os limites do envelope.
5. Integrar a nova apresentação ao idle, preservando domínio compartilhado, API, persistência e testes de equipamento.

[Orientações oficiais de exportação](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_3d_scenes/model_export_considerations.html). Originais, materiais, texturas, rigs, clips, `.uid`, `.import` e licenças serão versionados conforme as regras do projeto. O bake de sprites permanece disponível para reproduzir o protótipo histórico.

## Taste Skill no painel de estudo

Inventário que orienta a apresentação: nome Aetherforge, Galerias da Caldeira, Guarda da Caldeira, Pistoleiro, Alquimista do Éter, equipe de três, cinco etapas, escudo retangular, trilhos partidos, mineração e faixa junto à barra.

Argumento: mostrar a cena compacta; provar a linguagem 3D no kit; justificar cores e volumes com fontes; separar conceito de orçamento medido; encaminhar modelagem e profiling. Não há conteúdo de marketplace, clientes fictícios ou depoimentos.

Direção sorteada e escolhida: **page-is-the-object**, seed **1337574584**. Ideia: o registro de expedição das Galerias da Caldeira organiza o estudo. O elemento interativo compara a largura real da faixa conceitual em 800/960 pixels e a apresentação compacta/expandida.

Dois redraws registrados: o hero de convite exigia conteúdo e fotografia de casamento ausentes; foi substituído por uma composição de objeto ao lado da imagem. A lista de convidados em proof foi substituída por tabela de referências/créditos, porque convidados não fazem parte do conteúdo. Paleta mantida e vinculada às fontes do Color Hunt. A fonte sorteada é Archivo.

O painel foi refinado por Local Codex no projeto visual existente. [Crítica e verificação final](critica-taste-v3.md): lint sem findings, inspector sem FAIL e dois WARN defendidos. A nota 7.90 é autoavaliação do painel, não aprovação do usuário nem prova da qualidade dos modelos. A demonstração de escala usa a imagem estática; não substitui um renderer 3D nem serve de benchmark. As pranchas v1 e v2 permanecem como histórico.
