# Aetherforge: direção artística v2

**Histórico:** a [direção v3](Direcao_Artistica_v3.md) atualiza a apresentação para modelos 3D animados e cenário 3D leve, conforme nova decisão do usuário. Esta página preserva a exploração pictórica anterior.

**Orientação do usuário, 13/10/2026:** a temática deve ser pesada, sinistra e triste. Frostpunk e Frostpunk 2 são referências de atmosfera. Task Bar Hero continua como referência de formato compacto e apresentação do idle. Esta direção substitui a proposta fofa e brilhante das pranchas v1.

## Estudo das referências

Fontes: [Frostpunk na Steam](https://store.steampowered.com/app/323190/Frostpunk/), [Frostpunk 2 na Steam](https://store.steampowered.com/app/1601580/Frostpunk_2/), [Frostpunk 2 no site da 11 bit studios](https://11bitstudios.com/games/frostpunk-2/) e [relato de Bartosz Sobolewski, artista da 11 bit studios](https://blog.playstation.com/2021/07/07/how-frostpunk-reimagined-its-world-in-upcoming-prequel-expansion-the-last-autumn/).

Sobolewski explica como a arte do primeiro jogo reforça a desolação: azuis frios, cores desbotadas, sujeira, partículas e formas arquitetônicas. A equipe usa variações de luz para oferecer breves momentos de esperança. Essa relação entre arte e emoção é a principal referência para Aetherforge.

As imagens abaixo foram baixadas da galeria oficial fornecida pela API pública da Steam e inspecionadas. Arquivos de terceiros são referências de estudo, não assets redistribuíveis do runtime.

| Material observado | Evidência visual | Tradução proposta |
|---|---|---|
| [Frostpunk: apresentação do Season Pass](referencias/frostpunk-oficial-01.jpg) | Figura adulta equipada para trabalho perigoso; tecidos pesados; atmosfera hostil. A imagem é promocional. | Equipamento de sobrevivência, proporções adultas e postura cansada. |
| [Frostpunk: posto isolado](referencias/frostpunk-oficial-02.jpg) | Estruturas pequenas em um grande espaço frio; focos de calor separados por amplas áreas dessaturadas. | Espaço vazio, silêncio e vulnerabilidade; luz quente localizada. |
| [Frostpunk: edifício industrial e interface](referencias/frostpunk-oficial-03.jpg) | Volumes cobertos de neve, luz quente nas estruturas, contorno claro de seleção e painel de linhas finas. | Destacar objetos relevantes com contorno e legibilidade; manter controles discretos. |
| [Frostpunk 2: cidade](referencias/frostpunk-2-oficial-01.jpg) | Grandes massas industriais, corredores e densidade de infraestrutura cercados por terreno hostil. | Máquinas maiores que o trabalhador, estruturas de peso e escala opressiva. |
| [Frostpunk 2: visualização térmica](referencias/frostpunk-2-oficial-02.jpg) | Interface contida, linhas finas e cores intensas que indicam informação térmica. | Separar cor de atmosfera de cor informativa; o alerta pode ter contraste mesmo em um mundo desbotado. |
| [Frostpunk 2: cidade e retratos](referencias/frostpunk-2-oficial-03.jpg) | Cidade extensa, focos de calor e retratos humanos com roupas pesadas na interface inferior. | Contrastar escala industrial com figuras humanas; identificar funções pelo vestuário e equipamento. |

As interpretações e decisões para Aetherforge são propostas nossas, não descrições de intenção dos autores de Frostpunk. A inspiração não exige neve, inverno global, cidade radial ou novos sistemas de sobrevivência. Esses elementos mudariam o universo ou o escopo do jogo e não foram solicitados.

## Linguagem visual

**Fantasia visual:** trabalhadores descem a uma indústria abandonada para recuperar o que ainda mantém sua comunidade funcionando. A persistência da equipe deve parecer custosa. A tristeza vem dos sinais de ausência e desgaste; o perigo vem do peso das máquinas e da exposição dos corpos.

- Formas: proporções adultas, roupas pesadas, peças funcionais, construção assimétrica por reparo e uso. Simplificar massas para a leitura na barra.
- Materiais: lã, lona, couro ressecado, aço fosco, oxidação, fuligem e concreto úmido. Latão existe em pequenas ferragens, com brilho contido.
- Acabamento dos conceitos: pintura de massas com textura de guache e carvão, bordas seletivas e detalhes concentrados nas silhuetas. As imagens continuam sendo geradas com IA; o acabamento é uma intenção artística.
- Luz: cinza frio difuso com lanternas e brasas pontuais. Éter pálido e breve. Preservar um plano mais claro atrás de personagens escuros.
- Cenário: armários vazios, roupa de trabalho abandonada, trilhos danificados e ferramentas gastas. Poucos marcos narrativos bem escolhidos.
- Interface: tipografia de caráter industrial, marfim gasto, linhas finas e superfícies de carvão. Molduras discretas; texto legível.

## Correções da v1

| Problema observado nas pranchas v1 | Critério para a v2 |
|---|---|
| Cabeças grandes, rostos simpáticos e personagens semelhantes a brinquedos | Adultos vulneráveis, rosto parcialmente protegido, peso do equipamento |
| Metal dourado e limpo em quase toda a imagem | Aço gasto como base; latão pontual |
| Ciano e laranja muito luminosos | Cor restrita a função, perigo e pequenas fontes de calor |
| Ornamentação e muitos mecanismos sem propósito claro | Peças com função: pressurizar, perfurar, vigiar e esmagar |
| Cena de aventura vitoriosa | Trabalho perigoso, ausência, exaustão e solidariedade discreta |

## Paleta inicial

| Uso | Cor | Aplicação |
|---|---|---|
| Fuligem | `#171B1D` | Silhuetas e sombras |
| Ardósia | `#65717A` | Luz ambiente e distância |
| Cal envelhecida | `#D3D2C8` | Parede de contraste e texto |
| Lona gasta | `#776E61` | Roupa do healer e equipamento |
| Óxido | `#775340` | Desgaste, chão e máquinas |
| Calor fraco | `#B58750` | Lanterna, brasa e disparo breve |

O éter usa verde/cinza pálido. Alertas hostis podem usar vermelho de óxido com forma e antecipação de gesto. A atmosfera não deve tornar alvos ou avisos invisíveis.

## Personagens e inimigos

O Guarda da Caldeira passa a usar aço rebitado, proteção respiratória e escudo retangular gasto. O Pistoleiro tem casaco de lã escuro e arma industrial simples. O Alquimista do Éter ganha leitura de socorrista de campo: capuz de lona, bolsa médica e ampola discreta, mantendo sua função de cura e identidade etérea.

Ratos têm silhueta magra e orgânica; saqueadores parecem máquinas remendadas; sentinelas lembram caldeiras de pressão; drones e vigias têm lentes e ferramentas de observação reconhecíveis. O Capataz parece uma prensa industrial reutilizada para esmagar, com pistões e ferramenta pesada. O perigo vem de função e massa.

Marcha com carga e inclinação do corpo; ataque com preparação e recuo curto; proteção com pés firmes e escudo que recebe impacto; cura com gesto cuidadoso dirigido ao aliado. A recuperação da equipe deve comunicar alívio breve. Queda e derrota precisam de poses próprias na produção futura.

## Pranchas v2 e avaliação

- [Painel visual v2 preservado](index-v2.html): exploração anterior com pranchas ampliáveis, referências e avaliação de escala.
- [Cenários sombrios v2](conceitos/03-cenarios-sombrios-v2.png): Galerias da Caldeira, mineração e fornalha, com frio úmido e fuligem. Mantém equipe à esquerda e ameaça à direita.
- [Equipe e inimigos sombrios v2](conceitos/04-equipe-inimigos-sombrios-v2.png): três funções em marcha, preparação e ação; seis inimigos. A cura incorpora um aliado como referência de gesto.
- [Prompts v2](Prompts_GPT_Images_v2.md): ferramenta integrada GPT Images, sem uso do CLI/API.

A v2 responde melhor ao tom solicitado. Ainda contém bastante detalhe fino que pode desaparecer em 48–64 pixels de altura. Na prancha de cenário, o Capataz ultrapassa o limite superior do último quadro: reduzir e recompor a silhueta antes de usá-la na faixa. As poses são estudos estáticos com diferenças de equipamento e direção em alguns inimigos; precisam de normalização para atlas.

Antes de produção: testar em tamanho real, simplificar textura em sprites, manter contraste com o fundo e alinhar pivôs. A prioridade é ler função, alvo e gesto. Não foram implementados novos cenários, animações ou regras no Godot nesta revisão.

## Origem

As quatro pranchas conceituais de Aetherforge foram geradas com GPT Images. As duas pranchas v1 ficam preservadas como histórico, e a v2 passa a ser a referência artística atual deste estudo. Imagens de Frostpunk pertencem aos respectivos titulares e servem apenas à pesquisa.
