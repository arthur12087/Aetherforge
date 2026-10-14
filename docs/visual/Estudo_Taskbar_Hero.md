# Aetherforge: estudo visual do idle

Data: 13/10/2026. Exploração conceitual; não representa funcionalidades implementadas nem altera os critérios de M1.

**Atualização artística:** por orientação do usuário, a [direção v2](Direcao_Artistica_v2.md) substitui o acabamento fofo e brilhante da primeira proposta. Frostpunk e Frostpunk 2 orientam o tom pesado, sinistro e triste; Task Bar Hero permanece como referência de formato e apresentação. As descrições e pranchas v1 abaixo ficam como histórico da exploração inicial.

## Referência observada

Fontes primárias: [página oficial de Task Bar Hero](https://www.nugemstudio.com/en/games/taskbar-hero) e [Steam](https://store.steampowered.com/app/3678970/TBH__Task_Bar_Hero/). Foram inspecionadas as três primeiras screenshots da galeria oficial. As imagens são material de referência de terceiros, não assets do jogo Aetherforge. Permanecem sob os direitos de seus respectivos titulares.

| Screenshot | Observação direta | Aplicação proposta em Aetherforge |
|---|---|---|
| [01](referencias/taskbar-hero-oficial-01.jpg) | Arena compacta com plataforma de grama própria, personagens pequenos com contornos marcados e barras de vida. Painel de herói/inventário abre acima do combate. | Uma arena que comunica mundo e ação na própria faixa; gerenciamento abre sob demanda. |
| [02](referencias/taskbar-hero-oficial-02.jpg) | Painéis separados de status, herói e portal. Rota desenhada em mapa com nós; arena de deserto abaixo. Projéteis, golpes e notificação de equipamento ocupam a área de combate. | Distinguir cenário percorrido de mapa de progressão; usar efeitos breves e evitar sobreposição excessiva. |
| [03](referencias/taskbar-hero-oficial-03.jpg) | Forma muito pequena junto à barra de tarefas, no canto inferior direito; a arena aparece parcialmente no limite inferior da captura. | Verificar recorte e leitura no tamanho real, com espaço reservado a efeitos e interface. |

O grande papel de parede de floresta/deserto das screenshots não deve ser confundido com o mapa da arena. O combate tem seu próprio chão em uma região pequena. A composição observada lembra um pequeno palco lateral; a direção do deslocamento e a duração exata das animações não podem ser confirmadas por imagens estáticas.

A descrição oficial confirma combate automático de heróis em pixel art e execução junto à barra de tarefas. As capturas confirmam poses expressivas e efeitos, mas este estudo não é uma análise quadro a quadro do jogo em execução.

## Direção proposta

Transferir o princípio do pequeno palco vivo para o universo steampunk já definido no [GDD](../GDD_Aetherforge.md): cenário de profundidade rasa, câmera lateral em três quartos e personagens 2.5D de proporções compactas. O primeiro conceito usa acabamento ilustrado próximo a sprites pré-renderizados; pixel art pode ser explorada numa variante futura. Essa é uma proposta visual, não uma substituição aprovada da direção do GDD.

O cenário deve ter três camadas: arquitetura distante de baixo contraste, estruturas intermediárias e chão que recebe as sombras dos personagens. A equipe caminha para a direita; inimigos chegam pela direita e enfrentam a equipe. Healer na retaguarda, DPS no centro e tank na frente. Essa orientação é uma escolha para Aetherforge, não uma afirmação sobre a referência.

Fluxo visual: marcha → chegada do encontro → combate → recompensa → marcha. Durante a marcha, o cenário desliza; durante o combate, a câmera estabiliza. Cinco vitórias avançam uma etapa conforme o GDD. A passagem entre etapas muda elementos do mapa, luz e inimigos. Derrota deve comunicar recuperação e recuo, sem apresentar avanço como vitória.

## Paleta e hierarquia

| Uso | Cor inicial | Regra |
|---|---|---|
| Base industrial | Grafite `#19252A` | Fundo e molduras discretas |
| Materiais | Latão `#C7A36B`, cobre `#A85F3D` | Identidade das máquinas e equipamentos |
| Éter e cura | Ciano `#70D6CF` | Efeito curto associado ao healer |
| Leitura | Marfim `#EFE6CF` | Rosto, tecido e texto |
| Ataque hostil | Laranja/vermelho | Avisos acompanhados de forma e gesto |

Reservar contraste alto para personagens, alvos e avisos. Evitar iluminar todo o cenário durante cada cura. Funções e perigo precisam ser identificáveis também pela silhueta, ícone e movimento.

## Equipe e bestiário de M1

| Personagem | Silhueta | Animação que comunica a função |
|---|---|---|
| Guarda da Caldeira | Corpo largo, escudo mecânico, armadura pressurizada | Passo pesado; antecipação do escudo; impacto; vapor curto ao proteger |
| Pistoleiro | Corpo estreito, casaco, arma curta e direção de mira clara | Mirar; recuo do disparo; clarão breve; recuperação |
| Alquimista do Éter | Capuz claro, mochila de frascos, foco pequeno | Erguer foco; canalizar; pulso direcionado ao aliado; retorno |
| Rato de fuligem | Baixo, quadrúpede, cauda longa | Correr; preparar salto; morder; recuar |
| Saqueador mecânico | Magro, assimétrico, ferramenta como arma | Movimento irregular; investida dirigida; golpe |
| Sentinela a vapor | Corpo robusto, cilindro e pistões | Pressurizar; anunciar golpe; bater; liberar vapor |
| Drone | Corpo suspenso, rotor e lente | Flutuar; orientar lente; preparar rajada; disparar |
| Vigia | Corpo vertical, cabeça/lente distinta | Observar; travar alvo; atacar |
| Capataz de Ferro | Maior, largo, núcleo de fornalha e ferramenta pesada | Aviso de dois segundos; golpe pesado; impacto em área; recuperação |

Estes inimigos vêm da primeira rota do GDD. A mineração reutiliza o bestiário; o Guardião do Veio é conteúdo posterior dentro de M1, conforme o backlog. A identidade visual do Guardião ainda precisa de conceito próprio.

## Cenários

1. Galerias da Caldeira: entrada de serviço, trilhos partidos, câmara de pressão, ninho de engrenagens e fornalha central. Manter a mesma família modular de túneis e modificar marcos visuais por etapa.
2. Veio da Chaminé Velha: cobre, trilhos de transporte, perfuratriz e cristais de éter. A máquina de extração deve tornar a mineração reconhecível imediatamente.
3. Fornalha central: arena do Capataz, com fundo quente e chão livre para ler o ataque pesado. O chefe cresce horizontalmente, evitando corte no topo da janela.

## Escala e movimento a validar

Proposta inicial de estudo: faixa de 800 × 180 pixels lógicos e alternativa de 960 × 216; não são tamanhos finais. Separar uma linha discreta de estado, área de cenário e barra de ações. Personagens de 48–64 pixels de altura e chefe de 80–100, sujeitos a avaliação em 100%, 125%, 150% e 200% de DPI.

Produção futura: testar repouso, caminhada, ataque, habilidade, dano e queda para cada herói; repouso, movimento, antecipação, ataque, dano e derrota para cada inimigo. Pranchas de poses geradas por IA são estudos de movimento, não atlas prontos nem animações funcionais. Exigem limpeza, alinhamento de pivô e validação de consistência antes de importar no Godot.

Critérios de avaliação: distinguir as três funções em tamanho real; reconhecer pelo menos quatro silhuetas inimigas; preservar chão e sombras; não sobrepor números aos rostos; perceber a troca de etapa; evitar flash contínuo; permitir reduzir movimento.

## Entregáveis e limites

- [Conceito de cenários v1](conceitos/01-cenarios-v1.png): gerado com GPT Images pela ferramenta integrada. Painel com caça, mineração e chefe; detalhes são exploratórios.
- [Equipe, inimigos e poses v1](conceitos/02-equipe-inimigos-v1.png): três poses por herói e seis silhuetas inimigas. São estudos estáticos; não constituem animações prontas.
- [Prompts finais](Prompts_GPT_Images.md): especificações usadas nas duas gerações pela ferramenta integrada.
- O estudo e o preview são independentes do runtime. Multiplayer, regras, integração e save manual permanecem preservados.
- A próxima implementação deve usar [Desenvolvimento.md](../Desenvolvimento.md) e os critérios [M1](../POC_Producao.md), sem apresentar mockups como progresso funcional.
