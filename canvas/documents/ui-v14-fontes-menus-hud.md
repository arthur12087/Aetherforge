# V14 — plano de fontes, menus e HUD

**Estado atualizado: plano autorizado e implementado no protótipo.** Ver [entrega V14, capturas, decisões e testes](ui-v14-entrega.md). O conteúdo abaixo preserva o planejamento original: expressões como “ainda não implementado”, “futuro” e “não feito nesta rodada” descrevem aquela rodada de planejamento, não o runtime após a implementação. Aprovação visual final continua com o usuário. V14 é uma revisão de UI; Brass v12 GPT continua sendo a base de personagens e a paleta quente v10 permanece aceita.

## 1. Objetivo e direção

**Elegância industrial: instrumentos de oficina bem desenhados, com informação clara — não uma coleção de caixas marrons com bordas douradas.**

O jogador deve entender primeiro quem está em perigo, o que está acontecendo e qual ação está disponível. Depois percebe o acabamento de latão, esmalte, papel e os detalhes do Arco Partido. A interface deve parecer um produto do mundo de Aetherforge, sem competir com os personagens.

Preservar:

- Palco compacto junto à barra, não HUD de MMO em tela cheia.
- Status, Herói e Expedição como três janelas independentes; Overview apenas para revisão.
- Alquimista atrás/esquerda, Pistoleiro no meio e Guarda à frente/direita; inimigos mais à direita.
- Sete colunas de bolsa, três integrantes e cinco trechos. Equipamentos seguem indisponíveis nesta fatia.
- Tom adulto, industrial e melancólico; paleta quente, latão contido e éter localizado.
- Marcha real, eventos e valores da simulação. Nenhuma nova barra de energia, crítico, nível, XP, dinheiro ou recompensa fictícia.

Não entram nesta etapa: refazer personagens/cenários, produzir novas pinturas, mudar regras, progressão, saves, API, multiplayer ou cadastro do Studio.

## 2. Diagnóstico do estado atual

Referências inspecionadas: [captura enviada pelo usuário](../../assets/concepts/ui-v14-planejamento/referencia-hud-usuario.png), [menus reais do protótipo](../../assets/concepts/brass-v13-godot/boss.png) e [estudo de cor v10](../../assets/concepts/brass-v10/01-menus-cor.png).

| Observação | Efeito | Resposta proposta |
|---|---|---|
| Nomes finos em caixa-alta diretamente sobre tubulações e alvenaria | Cenário e identificação competem; leitura frágil na miniatura | Fonte funcional mais robusta, fundo local de contraste e menos rótulos permanentes |
| Barras muito finas sob os nomes | Parecem sublinhados, não indicadores de vida | Maior espessura, trilho escuro e preenchimento claramente separado |
| Quase todos os controles têm a mesma linguagem de retângulo e borda | Falta hierarquia entre ação principal, navegação e informação | Estados e níveis de importância distintos, sem emoldurar cada linha |
| Três menus usam o mesmo chassi no runtime | Mesa, estojo e telégrafo perdem identidade de instrumento | Mesmo sistema de materiais, três silhuetas/cabeçalhos discretamente diferentes |
| Nome do herói, título da janela e bilhete usam a mesma voz decorativa | A fonte de marca invade a leitura recorrente | Reservar o desenho Art Déco aos títulos principais; dados em fonte funcional |

Conferência no código, além da imagem:

- `BrassUi.cs`: Jost é a fonte padrão, com tamanho-base 14; vários elementos em `ExpeditionView.cs` usam 10–12. Limelight aparece também em nomes e bilhetes.
- `ExpeditionVitals.cs`: nomes desenhados a 10 e barras de 4, com largura normal 40. O HUD já é projetado em espaço de tela; **não é necessário refazê-lo como texto 3D**.
- `ExpeditionStage.cs`: os números de dano/cura usam `Label3D`, `FontSize = 48` e `PixelSize = .007`, sem fonte explícita naquele construtor. Esse 48 não equivale a 48 pixels finais na tela e não acompanha automaticamente o tema dos menus.

O recorte enviado mede 520 × 177; ele não permite concluir sozinho qual era a resolução/DPI da sessão. Os tamanhos acima vêm do código atual, não de estimativa sobre a imagem.

## 3. Tipografia recomendada

### Duas famílias, com funções claras

1. **Limelight Regular — identidade.** Manter na marca e no título principal de cada instrumento, em tamanho confortável. Retirar de nomes de integrantes, nomes de trechos, números, descrições e textos de botões. Não simular negrito numa família sem esse peso.
2. **Source Sans 3 — informação.** Candidata recomendada para substituir Jost em corpo, botões, nomes do HUD e números. Provar Regular/Medium e Semibold: mais peso nos nomes/ações, menos nas explicações.

Source Sans 3 já está no repositório em `docs/visual/assets/fonts/v5/SourceSans3-Variable.ttf`, com licença SIL OFL. Seu hash confere com `sources.json`. Ainda **não** foi importada nesta rodada para o cliente nem validada no novo HUD. Não é necessário comprar fonte ou instalar nada globalmente. A origem histórica desse arquivo num estudo anterior não traz consigo a paleta nem o layout daquele estudo.

Fazer uma prova A/B de **Jost ajustada versus Source Sans 3**, com o mesmo conteúdo, tamanhos e dois pesos por família. A recomendação é Source Sans 3, mas a decisão final vem da leitura no tamanho real, não do nome da fonte. Não reabrir três novas direções artísticas completas.

### Escala inicial a testar

Unidades de UI a 100%, não pontos de impressão; valores de partida, não resultados já aprovados:

| Uso | Família/peso | Tamanho inicial |
|---|---|---|
| Marca compacta | Limelight Regular | 20–22 |
| Título principal do instrumento | Limelight Regular | 24–26 |
| Nome do integrante / trecho atual | Source Sans 3 Semibold | 18–20 |
| Valores principais de Status | Source Sans 3 Semibold | 26–30 |
| Botões e conteúdo principal | Source Sans 3 Medium/Semibold | 14–16 |
| Descrições e linhas de journal | Source Sans 3 Regular | 14 |
| Metadados secundários | Source Sans 3 Regular/Medium | 12–13 |
| Nomes sobre atores, quando visíveis | Source Sans 3 Semibold | 13–14 |
| Números de combate | Source Sans 3 Semibold | 17–20 em espaço de tela |

Regras: evitar textos críticos abaixo de 13 no novo alvo; usar caixa normal nos nomes (`Guarda`, `Sentinela`), não tudo em caixa-alta. Caixa-alta curta fica para placas/títulos. Entrelinha de corpo perto de 1,25–1,35; grade de espaçamento 4/8/12/16/24. Validar os glifos `Éter`, `Pressão`, `Alquimista`, `ç`, `ã`, `0 O`, `1 I l`, sinais e percentuais. Testar dígitos tabulares no arquivo/importação; não presumir suporte só porque Jost passou no estudo antigo.

## 4. Estilização dos menus

### Sistema comum

Três planos de valor: **rebaixo de fuligem escuro → carcaça tabaco → placa clara de leitura**. Vinho concentra seleção e ação; latão aparece em encaixes, cantos e divisórias importantes, não no contorno de todo componente. Superfícies majoritariamente quietas, com relevo curto nas bordas e sombra discreta; nada de textura suja por baixo das letras, brilho dourado excessivo ou gradiente azul.

- Arco Partido como assinatura do cabeçalho, sem repeti-lo em cada slot.
- Botões com área útil confortável, alvo inicial de 34–40 de altura e respiro entre ações.
- Ação principal: esmalte vinho, texto linho, ícone próprio e estado explícito. Ações secundárias: rebaixo escuro. Recuar: separado da ação frequente para reduzir enganos.
- Hover, foco de teclado, pressionado, selecionado, desabilitado e recarga precisam ser distinguíveis; foco não depende só de mudar de cor. Desabilitado deve explicar por quê.
- Ícones vetoriais autorais da mesma família: escudo, rifle, aplicador/cura, pausa, retorno e rota. Base 24, traço coerente e duas massas de valor. Sem emoji e sem copiar ícones de pesquisa.
- Transições curtas, aproximadamente 120–160 ms para foco/abertura e sem elasticidade de brinquedo. Prever redução de movimento/flash, sem alterar a velocidade das regras.

### Status — Mesa de Regulagem

Cabeçalho reto, chapa técnica e três mostradores numéricos bem alinhados. Destacar vida atual/máxima e o integrante selecionado. A equipe aparece em três linhas claras. Escudo vira o componente de ação de referência: ícone, nome, estado `Pronto` / `Protegido` / recarga real. Cura automática é informação, não botão que parece manual. Explicações longas ficam em ajuda/tooltip acessível; ações principais continuam à vista.

### Herói — Estojo de Expedição

Cabeçalho/encaixe em arco e retrato v12 com mais área de respiro, sem nova pintura. Nome do integrante em fonte funcional; três seletores com ícone/retrato e estado selecionado. Bolsa de sete colunas com slots mais coerentes e quase quadrados, mas discretos enquanto indisponíveis. Não decorar vazios como se já contivessem itens utilizáveis.

### Expedição — Telégrafo de Rotas

Cabeçalho escalonado e uma faixa de papel para o bilhete atual. Cinco nós com estados por **forma e texto**, além de cor: futuro, atual, concluído. O trecho atual e `Em marcha` / `Em combate` têm prioridade; journal é secundário e não disputa com a rota. Sem novas abas Mineração/Oficina/MMO não implementadas.

### Espaço e janelas

Usar 340 × 540 como referência inicial do instrumento, não tamanho fixo inviolável. Em telas baixas, limitar pela área útil e pelo palco, manter rodapé de ações acessível e rolar a área de conteúdo. Não resolver falta de espaço reduzindo a fonte para 9–10. Em 1366 × 768, três menus altos mais o palco podem não caber sem sobreposição: testar menus abertos individualmente e uma disposição utilizável, sem prometer que tudo ficará simultaneamente exposto. O modo Overview não é a prova de ergonomia das janelas nativas.

## 5. HUD de combate: menos ruído, mais presença

### Apresentação concisa, recomendada como padrão

- Aliados: barra de vida compacta e pequeno identificador de função; nome completo na inspeção por hover/foco/seleção do integrante. Evitar repetir três nomes longos permanentemente sobre a formação conhecida.
- Inimigos comuns: vida legível; nome curto quando houver espaço ou ao inspecionar/ser atingido. Não criar um comando de alvo manual — é apresentação da informação que já existe.
- Chefe: nome e vida em um componente compacto dedicado no topo do palco durante o encontro, sem banner gigante nem HP inventado. Evitar duplicar simultaneamente a mesma identificação acima da cabeça.
- Contexto do palco: uma linha limpa de região/trecho/estado; botões de menus bem alinhados, sem repetir todos os dados em duas barras.

Prever uma opção futura de leitura detalhada com nomes e valores mais persistentes. Essa opção só entra quando funcional e sem capturar atalhos globais do desktop.

### Barras e posicionamento

Testar barras normais com 48–56 de largura e 6–7 de altura, trilho de fuligem e enchimento de valor bem separado; números exatos ficam disponíveis no detalhe. Não resolver baixa leitura apenas tornando a barra mais brilhante.

Rótulos visíveis recebem uma pequena placa opaca de contraste, cantos discretamente cortados e margens internas. Medir seus retângulos em pixels, reservar uma faixa para números e tratar colisões entre labels, bordas e cabeças. Se faltar espaço: priorizar o ator relevante e ocultar texto secundário; **não** encolher todas as fontes. Testar especialmente vizinhos próximos, o Vigia alto e o Drone suspenso.

Manter tamanho do texto em espaço de tela e projetar apenas a âncora 3D. Revisar clamp/visibilidade nas bordas: não desenhar placas cortadas nem mover rótulos a ponto de parecerem pertencer ao vizinho.

### Dano, cura e escudo

- Unificar a fonte dos números com a fonte funcional. Avaliar mover o texto de combate para uma camada de UI projetada, preservando âncora, trajetória curta e eventos reais.
- Dano e cura com sinais/forma além da cor; cura sempre com `+`. Shield ativo usa ícone e `Protegido`, **não** `Bloqueado`, pois a mitigação atual não é imunidade.
- Primeiro alvo: até dois números simultâneos por ator e um limite global a validar. Dano e cura não devem se cancelar visualmente num único saldo; agrupamento, se necessário, é somente visual.
- Testar vida visual de 500–750 ms e subida curta. Números não atravessam nomes/barras nem encobrem rostos. Sem novos rótulos de crítico/combo/loot que a simulação não produz.
- Feedback de combate respeita pausa, reset e tempo da simulação; interações dos menus continuam responsivas. Reutilizar elementos, evitar nós ilimitados e não recalcular todo o menu a cada frame.

## 6. Paleta e contraste

Manter `#BFB49E` papel/pedra, `#241F1A` fuligem, `#40332B` tabaco, `#6C3A32` vinho, `#B99B63` latão, `#E4D8B7` linho e `#97A581` éter. Éter sinaliza cura/vida de modo localizado, não vira brilho em toda a interface. Cor não é o único identificador de função/estado.

Conferência matemática de luminância relativa sRGB dos pares **sólidos** existentes:

| Par | Contraste aproximado |
|---|---|
| Linho sobre fuligem | 11,51:1 |
| Linho sobre tabaco | 8,58:1 |
| Papel sobre tabaco | 5,93:1 |
| Latão sobre tabaco | 4,59:1 |
| Vinho sobre papel | 4,47:1 |

Meta de leitura para texto funcional: pelo menos 4,5:1. O último par fica abaixo: usar fuligem para texto pequeno sobre papel, não vinho. Latão tem margem pequena; reservar preferencialmente ao ornamento. Esses cálculos não certificam acessibilidade do jogo: alfa, antialiasing, tamanho, movimento e o fundo real ainda precisam ser avaliados no Godot.

## 7. Plano de execução, por prioridade

| Etapa | Entrega futura | Critério para seguir |
|---|---|---|
| **P1 — prova tipográfica e HUD** | Comparação A/B Jost/Source Sans 3 com o mesmo recorte real, nomes, HP e dano/cura a 100%; um componente de escudo | Escolher a combinação por leitura e personalidade; nenhuma prancha ampliada substitui o teste pequeno |
| **P2 — kit visual único** | Tokens de fonte/cor/espaçamento, pesos reais, botões/estados, barras, placa e seis ícones | Um conjunto aprovado aplicado a um instrumento-piloto: Status |
| **P3 — HUD no Godot** | Barras, nomes condicionais, números e chefe com posicionamento consistente | Marcha/combate/cura/escudo/morte sem sobreposição ou informação falsa |
| **P4 — três instrumentos** | Propagar o kit a Status/Herói/Expedição, com cabeçalhos distintos e rodapé utilizável | Mesmas regras e janelas independentes; mais legibilidade sem inflar o palco |
| **P5 — acabamento e regressão** | Capturas nativas, revisão em movimento, foco, escalas e relatório | Critérios abaixo cumpridos; falhas remanescentes documentadas |

**Próxima entrega recomendada: P1, não o redesenho inteiro.** Mostrar a mesma situação de combate antes/depois e um pequeno recorte do novo Status; aprovar isso antes de fabricar uma biblioteca completa de molduras/ícones. Texto deve continuar nativo/editável, não pintado por gerador de imagens.

### Divisão futura de trabalho

Delegação proposta, **não executada nesta rodada**:

- Tipografia/tema: `BrassUi.cs`, novos tokens e fontes/licenças em `Assets/Brass/`. Centralizar tamanhos e estados; evitar dezenas de overrides conflitantes.
- HUD: `ExpeditionVitals.cs` e a parte de números/projeção em `ExpeditionStage.cs`. Contrato de tokens acordado antes; sem tocar no domínio.
- Menus: `ExpeditionView.cs`, usando o tema acordado. Não mudar regras nem retirar janelas independentes.
- Orquestrador/QA: integrar em sequência, adaptar checks se necessário e coordenar build único. Workers não compilam simultaneamente nem editam arquivos de outra frente.

A simulação continua avançando apenas no dono atual, `ExpeditionView._Process`. Atualização do conteúdo da UI pode ser por eventos/alterações; âncoras sobre atores continuam acompanhando movimento. O trabalho de estilização não autoriza uma segunda simulação ou uma revisão da API de domínio.

## 8. Aprovação e testes futuros

- Leitura em tamanho real: palco 1000 × 320 e Overview 1160 × 880. A imagem 520 × 177 enviada é referência de diagnóstico, não uma nova resolução obrigatória do produto.
- Matriz desktop: 1366 × 768, 1920 × 1080 e 2560 × 1440; escalas 100%, 125% e 150%. Separar DPI do SO de uma eventual opção de escala do texto; testar a combinação sem cortar cabeçalhos, foco ou rodapé.
- Cenas: marcha, grupos próximos, Drone/Vigia, chefe, dano e cura no mesmo intervalo, escudo pronto/ativo/recarga, pausa, derrota, recuo e reset. Usar eventos/fixtures de QA, não alterar saves manuais ou inventar regras para produzir screenshots.
- Legibilidade: HP reduzido perceptível sem depender de cor; números não parecem nomes; texto crítico em alto contraste; detalhes aparecem por mouse **e** foco/seleção; nada fora da área útil.
- Interação: mouse, Tab/setas/Enter/Escape; foco visível e retorno ao botão de origem. Preservar abertura/fechamento sem parar a rota nem tomar foco continuamente. Gamepad é validação adicional quando houver dispositivo, não suporte já comprovado.
- Técnica: fontes locais com licença, sem fallback silencioso nos glifos usados; UI não recebe escala da câmera nem estica glyphs; árvore de nós/contagem de números limitada numa rota inteira.
- Regressão após implementar: build, `Test-Expedition.ps1 -Screenshots`, `Test.ps1` e `Test-Integration.ps1`, além de cliques/foco reais e inspeção em movimento. Headless sozinho não aprova estética/DPI.

## 9. O que esta rodada fez e o que não fez

Feito: inspeção da imagem enviada, imagens atuais e código; inventário/verificação da fonte candidata e licença; contraste dos pares sólidos; recomendação visual e backlog de aprovação; referência e plano registrados no Canvas.

Não feito: novo mockup/prancha, importação de fonte no runtime, implementação de tema/HUD, execução de subagentes, geração de imagens, testes no Godot ou benchmark. Nenhum código do jogo, gameplay, save ou documento histórico foi alterado para executar esta proposta.
