# Documento de Design do Jogo Aetherforge

**Versão:** 0.3  
**Status:** design da POC detalhado; parâmetros iniciais sujeitos a playtest  
**Nome do projeto:** provisório  
**Gênero:** MMORPG com jogo idle integrado  
**Temática:** steampunk fantástico

## Resumo executivo

Aetherforge é um projeto que combina duas experiências no mesmo universo:

1. Um RPG idle completo, inspirado no formato de *The Taskbar Hero*, executado na barra de tarefas e baseado em progressão contínua por mapas.
2. Um MMORPG inspirado na estrutura de *Ragnarok Online*, com mapas e personagens 3D animados em tempo real.

O jogador envia uma equipe para missões contínuas de caça ou mineração. A equipe avança por etapas até enfrentar o chefe final da região, evolui de nível, equipa seus próprios itens e continua participando da missão mesmo sem intervenção constante. Parte dos equipamentos, moedas e recursos obtidos alimenta o MMORPG e sua economia global.

O princípio central de produto é:

> Dois jogos completos, uma progressão compartilhada.

## 1. Visão geral

### 1.1 Proposta do produto

As duas experiências compartilham conta, mundo, inventário selecionado e parte da economia. O jogo idle mantém o universo ativo em sessões curtas ou durante a ausência do jogador. O MMORPG concentra exploração, combate, narrativa e interação social em tempo real.

A proposta não é oferecer dois jogos independentes ligados apenas por recompensas. Cada experiência deve produzir oportunidades que ganham significado na outra. A integração precisa ser útil e desejável, mas não obrigatória em todos os momentos.

### 1.2 Proposta em uma frase

> Dois RPGs steampunk conectados: uma equipe idle que luta e evolui continuamente na barra de tarefas e um MMORPG com exploração, combate, quests elaboradas e uma economia global compartilhada.

### 1.3 Plataformas

- PC como plataforma inicial.
- Jogo idle executado diretamente na barra de tarefas.

### 1.4 Direção visual

- Tom pesado, sinistro e triste. Frostpunk e Frostpunk 2 são referências de atmosfera; Task Bar Hero continua orientando o formato do idle. A [direção artística v3](visual/Direcao_Artistica_v3.md) reúne a revisão com Taste Skill, Color Hunt e Dribbble.
- Personagens 3D com rig e animação em tempo real nos dois jogos, conforme orientação do usuário em 13/10/2026. A representação por sprites pré-renderizados deixa de ser o alvo de produção.
- Cenários 3D, com câmera isométrica ou semi-isométrica no MMORPG. No idle, cenário 3D leve, modular e de profundidade rasa, com câmera lateral fixa em três quartos. A exigência provisória de cenário 2D foi retirada pelo usuário.
- Leitura visual inspirada em MMORPGs clássicos, com silhuetas claras e animações expressivas.
- Personagens de proporções adultas, equipamento de trabalho e sobrevivência, materiais gastos e máquinas industriais opressivas. Cores frias e desbotadas, calor localizado e éter de brilho contido.
- Cidades industriais, dirigíveis, autômatos, locomotivas, minas e ruínas movidas a éter.
- Paleta baseada em aço escuro, latão e cobre gastos, fuligem, vapor, tecidos desbotados e energia etérea de brilho contido.

### 1.5 Assets da prova de conceito

A POC pode usar assets prontos encontrados na internet para popular personagens, mapas, interface, efeitos e áudio. A prioridade é testar o gameplay e a integração, sem exigir arte autoral ou consistência visual final nessa fase. Esta autorização vale para a etapa de desenvolvimento; o levantamento e a importação ainda não foram executados.

Preferir assets gratuitos com licença clara para uso no protótipo. Registrar origem, autor, licença e atribuição exigida num catálogo do projeto. Manter placeholders identificados e separar arquivos originais das adaptações, para facilitar a substituição pela direção steampunk final. Se algum asset exigir compra, escolher uma alternativa gratuita enquanto não houver orçamento definido.

Mesmo com arte provisória, a próxima validação visual precisa usar personagens 3D com rig e animações, junto a um cenário 3D simples no idle. Modelos prontos licenciados podem validar o pipeline antes de produzir arte própria. O protótipo M0 ainda usa sprites; a migração permanece pendente e deve preservar sua integração e as regras compartilhadas.

## 2. Fantasia do jogador

O jogador começa como um aventureiro contratado por uma guilda de exploradores e engenheiros. Ele não é apenas um combatente: também atua como líder de expedição, inventor, comerciante e descobridor de tecnologias esquecidas.

Durante a progressão, o jogador poderá:

- Explorar cidades, minas, ruínas e ilhas flutuantes.
- Enfrentar criaturas mecânicas e monstros alterados pelo éter.
- Construir e administrar uma equipe de expedição.
- Fabricar armas, autômatos e dispositivos steampunk.
- Participar de uma economia controlada pelos jogadores.
- Conquistar prestígio dentro de guildas e facções industriais.

**Fantasia central:** sou um aventureiro e inventor construindo um império de exploração.

## 3. Pilares de design

### 3.1 Mundo persistente

O universo continua produzindo acontecimentos e descobertas mesmo quando o jogador não está conectado.

### 3.2 Progressão complementar

Cada jogo fortalece o outro, mas ambos precisam continuar divertidos e compreensíveis separadamente.

### 3.3 Economia significativa

Itens têm origem, utilidade, desgaste e destino. Recursos entram e saem da economia por crafting, reparos, impostos, consumíveis e melhorias.

### 3.4 Identidade visual forte

Tecnologia industrial, magia etérea, moda vitoriana e criaturas mecânicas devem tornar o projeto reconhecível imediatamente.

### 3.5 Socialização

Guildas, comércio, crafting especializado e expedições coletivas fazem parte da experiência central.

### 3.6 Quests elaboradas

As quests do MMORPG devem ser produzidas como conteúdo autoral de alta qualidade, com contexto, personagens, escolhas, acontecimentos e consequências memoráveis. Elas serão um diferencial central do jogo, e não apenas tarefas repetitivas de coleta ou eliminação de inimigos.

## 4. Jogo idle

### 4.1 Função

O idle deve funcionar como um RPG completo, mesmo para quem decidir jogar apenas por meio da barra de tarefas. Ele possui campanha própria, mapas, progressão de nível, equipamentos, atributos, inimigos e chefes. Sua integração com o MMORPG aumenta o valor da experiência, mas não é necessária para que o idle seja divertido e tenha objetivos próprios.

### 4.2 Loop principal

1. Montar e equipar uma equipe de aventureiros.
2. Escolher uma missão de caça a monstros ou mineração automatizada.
3. Enviar a equipe para a primeira etapa do mapa selecionado.
4. Acompanhar a equipe avançar automaticamente de etapa em etapa.
5. Enfrentar inimigos, coletar recursos, ganhar experiência e obter equipamentos.
6. Derrotar o chefe final para concluir o mapa e liberar progressão adicional.
7. Caso a equipe inteira seja derrotada, retornar para a etapa anterior e continuar o ciclo.
8. Melhorar níveis, atributos e equipamentos para superar regiões mais difíceis.
9. Manter a equipe participando continuamente até o jogador alterar a missão ou a composição.

### 4.3 Atividades

| Atividade | Resultado esperado |
|---|---|
| Mineração automatizada | Extração de minérios, cristais de éter e sucata |
| Caça a monstros | Experiência, moedas, equipamentos e materiais de criaturas |

### 4.4 Profundidade desejada

O idle segue uma estrutura semelhante à de *The Taskbar Hero*. O jogador escolhe uma missão e a equipe passa a participar dela continuamente, sem precisar receber um novo comando depois de cada combate.

Cada missão é dividida em uma sequência de etapas. A equipe avança de mapa em mapa, enfrenta inimigos e coleta recompensas até alcançar o chefe final daquela rota. Ao derrotar o chefe, a equipe conclui a região e pode liberar mapas ou dificuldades superiores.

Se todos os integrantes morrerem, a equipe retorna para a etapa anterior, recupera-se e tenta avançar novamente. Esse recuo cria um loop contínuo: a equipe progride até encontrar uma barreira, acumula níveis e equipamentos nas etapas que consegue vencer e, com o tempo, torna-se forte o suficiente para superar o ponto onde havia sido derrotada.

### 4.5 Progressão da equipe

Cada integrante da equipe idle possui sistemas próprios de RPG:

- Nível e experiência.
- Atributos de combate.
- Equipamentos próprios.
- Qualidade e raridade dos itens.
- Durabilidade dos equipamentos.
- Função dentro da equipe.
- Possíveis habilidades ou especializações.

A composição, os níveis e os equipamentos determinam até onde a equipe consegue avançar. O jogador pode intervir para trocar itens, reorganizar funções, alterar a missão ou escolher outra região, mas a equipe continua ativa enquanto nenhuma mudança for feita.

### 4.6 Composição definida

A equipe idle possui exatamente **três integrantes: um DPS, um tank e um healer**. Cada vaga tem uma função fixa na POC. Trocar equipamentos e prioridades altera a build, sem remover a necessidade de qualquer uma das três funções.

| Vaga | Personagem inicial | Responsabilidade | Prioridade automática |
|---|---|---|---|
| DPS | Pistoleiro | Eliminar inimigos e superar a regeneração ou o limite de tempo dos chefes | Atacar o alvo marcado pelo tank; priorizar o inimigo que interrompe a cura |
| Tank | Guarda da Caldeira | Atrair ataques, reduzir dano e proteger a equipe | Provocar inimigos; usar proteção quando houver ataque pesado |
| Healer | Alquimista do Éter | Sustentar a equipe e administrar energia de cura | Curar o aliado vivo com menor percentual de vida; proteger a si mesmo quando necessário |

O Alquimista existe como integrante NPC do idle na POC. As duas classes jogáveis do MMORPG continuam sendo Guarda da Caldeira e Pistoleiro. Isso permite validar as três funções sem ampliar o escopo de classes jogáveis.

As especificações de combate, rotas e economia estão na seção 15. São propostas de implementação e balanceamento, sujeitas a playtest.

## 5. MMORPG principal

O MMORPG é o centro da presença social, da habilidade e da descoberta em tempo real.

Suas atividades incluem:

- Exploração em tempo real.
- Combate e progressão de classes.
- Quests elaboradas e narrativa autoral.
- Dungeons e chefes mundiais.
- Guildas e atividades cooperativas.
- Sem PvP na versão inicial.
- Crafting avançado.
- Comércio entre jogadores.
- Housing ou oficina pessoal.

### 5.1 Estrutura do mundo

Para a fase inicial, o mundo deve ser formado por mapas conectados em vez de um grande mundo aberto contínuo. Essa estrutura reduz o custo da prova de conceito, facilita o controle de conteúdo e permite regiões mais densas e memoráveis.

### 5.2 Quests como diferencial

Além da integração com o idle, o principal diferencial do MMORPG será a qualidade das quests. Elas devem evitar a estrutura genérica de apenas eliminar uma quantidade de inimigos ou transportar objetos sem contexto.

As quests prioritárias devem incluir personagens marcantes, conflitos locais, investigação, exploração, acontecimentos roteirizados e recompensas relacionadas à história. Mesmo quando uma tarefa utilizar combate ou coleta, essas ações precisam servir a uma situação narrativa clara.

## 6. Integração entre os jogos

A integração é o principal diferencial do projeto. O idle encontra recursos e oportunidades; o MMORPG atribui contexto, esforço e valor final a essas descobertas.

### 6.1 Ciclo de integração

```text
Idle evolui e gera recursos compartilháveis
        ↓
MMORPG utiliza ou negocia esses recursos
        ↓
MMORPG também gera recompensas elegíveis para o idle
        ↓
As duas experiências alcançam desafios mais difíceis
```

### 6.2 Modelo de transferência recomendado

| Recurso obtido no idle | Utilidade no MMORPG |
|---|---|
| Minérios e cristais de éter | Crafting, manutenção e melhorias |
| Sucata e componentes mecânicos | Construção, reparo e oficina pessoal |
| Materiais de monstros | Receitas, consumíveis e equipamentos temáticos |
| Moedas elegíveis | Uso na economia compartilhada dentro dos limites definidos |
| Equipamentos elegíveis | Uso, restauração ou negociação no MMORPG |
| Cosméticos | Uso compartilhado pela conta |

### 6.3 Exemplo de integração

A equipe idle encontra o núcleo quebrado de um autômato. No MMORPG, o jogador precisa obter peças, localizar um engenheiro ou concluir uma dungeon para restaurá-lo.

Depois disso, o autômato pode ser usado como companheiro no mundo principal ou enviado de volta às expedições idle.

### 6.4 Restrição fundamental

A transferência ilimitada de ouro ou de equipamentos completos deve ser evitada. Caso contrário, o idle poderá se tornar a forma dominante de jogar, aumentar a inflação e incentivar bots e múltiplas contas.

### 6.5 Durabilidade dos equipamentos

Equipamentos não são destruídos permanentemente. O uso reduz sua durabilidade e, ao chegar ao limite mínimo, o item perde eficiência ou fica temporariamente indisponível até ser reparado.

O reparo consome moedas ou materiais e funciona como uma saída constante de recursos da economia. Itens raros podem custar mais para manter, mas o jogador nunca perde definitivamente um equipamento por desgaste comum.

## 7. Economia e RMT

A economia será global, compartilhada entre todos os jogadores e integrada às duas experiências. O modelo de RMT será inspirado na economia de *Warframe*: jogadores compram uma moeda premium oficial e podem usá-la para adquirir itens elegíveis de outros jogadores dentro do marketplace.

Não haverá saque oficial da moeda premium para dinheiro real. O fluxo de valor ocorre da compra da moeda premium para a negociação de itens dentro do ecossistema do jogo.

### 7.1 Fluxo do marketplace

1. Um jogador compra moeda premium da empresa.
2. Outro jogador obtém um item elegível jogando o idle ou o MMORPG.
3. Os jogadores negociam o item pelo marketplace oficial.
4. O vendedor recebe a moeda premium dentro da conta.
5. A empresa controla a infraestrutura, as regras e a segurança da transação.

### 7.2 Regras iniciais

- Apenas itens marcados como negociáveis podem entrar no marketplace.
- Equipamentos vinculados, recompensas narrativas e itens essenciais de progressão podem permanecer fora do mercado.
- A moeda premium é adquirida oficialmente e circula entre os jogadores.
- A empresa pode cobrar uma taxa em moeda comum ou outro custo econômico por transação.
- O histórico de origem, propriedade e negociação de cada item deve ser rastreável.
- O marketplace deve possuir limites e mecanismos contra manipulação, fraude, bots e múltiplas contas.
- O sistema não oferece conversão oficial da moeda premium para dinheiro real.

### 7.3 Economia global

Todos os jogadores participam da mesma economia, mesmo que a infraestrutura técnica utilize diferentes regiões ou instâncias de servidor. Preços, oferta, procura e histórico do marketplace são globais.

Essa decisão aumenta a liquidez do mercado, mas também exige controle rigoroso de inflação, geração de itens, diferenças regionais de preço, automação e exploração de contas.

## 8. Combate

### 8.1 Direção inicial

- Movimento por WASD.
- Câmera isométrica rotacionável.
- Ataques básicos automáticos ou semiautomáticos.
- Habilidades ativas com tempo de recarga.
- Ênfase em posicionamento, alcance e áreas de efeito.
- Poucas habilidades equipadas ao mesmo tempo para preservar a clareza visual.
- Combate fácil de entender, com profundidade criada por builds, equipamentos e sinergias.

Uma barra com aproximadamente seis habilidades ativas, consumíveis e ataques contextuais tende a combinar melhor com a proposta do que dezenas de atalhos simultâneos.

## 9. Classes

| Classe | Papel e identidade |
|---|---|
| Mecânico | Constrói torretas, minas e pequenos autômatos |
| Pistoleiro | Combate à distância, mobilidade e munições especiais |
| Guarda da Caldeira | Tank com armadura pressurizada e escudos mecânicos |
| Alquimista do Éter | Suporte, cura, venenos e manipulação de energia |
| Caçador de Relíquias | Mobilidade, armadilhas, críticos e exploração |
| Tecnomante | Combina magia com máquinas antigas e eletricidade |

A progressão pode começar com classes básicas e depois abrir especializações. Um Mecânico, por exemplo, poderia evoluir para:

- Engenheiro de Cerco.
- Mestre dos Autômatos.

## 10. Mundo e narrativa

O mundo utiliza uma substância chamada **Éter Negro**, capaz de alimentar máquinas e alterar organismos. Grandes cidades industriais dependem dela, mas sua extração está desestabilizando estruturas antigas e despertando ameaças subterrâneas.

### 10.1 Facções possíveis

| Facção | Identidade |
|---|---|
| Consórcio de Latão | Magnatas, comerciantes e industriais |
| Liga dos Cartógrafos | Exploradores e caçadores de relíquias |
| Ordem da Caldeira | Militares e defensores das cidades |
| Círculo Etéreo | Cientistas e estudiosos do sobrenatural |
| Ferrugem Livre | Rebeldes, contrabandistas e engenheiros independentes |

### 10.2 Regiões possíveis

- Metrópole industrial inicial.
- Minas tomadas por criaturas.
- Ferrovia atravessando um deserto tóxico.
- Floresta contaminada por vapor e éter.
- Cemitério de dirigíveis.
- Arquipélago de ilhas flutuantes.
- Ruínas de uma civilização mecânica.

## 11. Escopo da prova de conceito

A prova de conceito deve validar o diferencial do projeto antes da tentativa de construir um MMORPG completo.

O objetivo principal é demonstrar que encontrar algo no idle e transformá-lo no MMORPG produz uma experiência clara, recompensadora e repetível.

### 11.1 Conteúdo mínimo

#### Idle

- Uma equipe de três personagens: DPS, tank e healer.
- Uma missão de caça a monstros.
- Uma missão de mineração automatizada.
- Uma rota com várias etapas e um chefe final.
- Recuo automático para a etapa anterior quando a equipe for derrotada.
- Progressão contínua em loop.
- Níveis, atributos e equipamentos próprios da equipe.
- Progressão offline.
- Inventário compartilhado.

#### MMORPG

- Uma cidade.
- Dois mapas de combate.
- Uma dungeon.
- Duas classes.
- Um chefe.
- Movimento por WASD.
- Uma quest principal elaborada para validar a qualidade narrativa.
- Crafting simples.
- Inventário compartilhado.
- Sem PvP.

#### Integração

1. O idle encontra um projeto e materiais.
2. O jogador acessa o MMORPG.
3. O MMORPG exige um componente ou desafio adicional.
4. O jogador fabrica o item.
5. O item melhora o personagem e a equipe idle.

### 11.2 Critério principal de validação

O jogador deve entender por que vale a pena participar das duas experiências e sentir que cada uma oferece decisões próprias, sem perceber o idle como uma obrigação ou o MMORPG como uma etapa burocrática para resgatar recompensas.

## 12. Riscos e proteções

| Risco | Impacto | Proteção inicial |
|---|---|---|
| Inflação | Moeda e materiais perdem valor | Limites, taxas, crafting, reparos e consumíveis |
| Pay to win | Poder passa a depender de gasto real | Definir itens negociáveis, proteger recompensas narrativas e manter rotas de obtenção por gameplay |
| Bots | Automação domina a coleta de recursos | Telemetria, limites, desafios contextuais e origem rastreável |
| Múltiplas contas | Jogadores contornam limites de produção | Vínculo de recursos, verificação e análise comportamental |
| Fraude | Chargebacks e itens ilícitos afetam o mercado | Retenção, autenticação forte e histórico de transações |
| Idle cansativo | O loop contínuo vira uma tarefa diária sem decisões relevantes | Progressão offline, intervenções significativas e ausência de obrigação diária |
| MMORPG esvaziado | O idle substitui a atividade principal | Itens incompletos e valor final obtido no mundo principal |

### 12.1 Proteções econômicas essenciais

- Limite diário ou semanal de transferência do idle.
- Recursos parcialmente vinculados à conta.
- Taxas no marketplace.
- Durabilidade e reparo de equipamentos.
- Custos de fabricação.
- Itens consumíveis.
- Detecção de bots e múltiplas contas.
- Autenticação forte para negociações valiosas.
- Histórico de origem dos itens.
- Período de retenção para transações suspeitas.

## 13. Decisões consolidadas

| Tema | Decisão |
|---|---|
| Papel do idle | Jogo completo com progressão própria |
| Plataforma do idle | Barra de tarefas no PC |
| Atividades idle | Caça a monstros e mineração automatizada |
| Estrutura idle | Avanço contínuo por etapas até um chefe final |
| Derrota no idle | Retorno para a etapa anterior e continuidade automática |
| Progressão idle | Níveis, atributos e equipamentos próprios |
| Composição idle | Três integrantes: um DPS, um tank e um healer |
| RMT | Compra de moeda premium e marketplace entre jogadores, inspirado em *Warframe* |
| Economia | Global |
| Movimento do MMORPG | WASD |
| Personagens | Modelos 3D com rig e animação em tempo real |
| Cenário do idle | 3D leve, modular, câmera fixa e custo validado por medição |
| PvP | Não estará presente inicialmente |
| Durabilidade | Equipamentos perdem durabilidade, mas não quebram permanentemente |
| Diferencial adicional | Quests bem escritas, elaboradas e memoráveis |

## 14. Direção recomendada

A direção mais segura para a primeira versão é:

- MMORPG cooperativo com mapas conectados.
- Câmera isométrica.
- Personagens 3D animados em tempo real e cenário 3D leve no idle.
- Crafting como sistema central.
- Idle completo, executado na barra de tarefas, com caça, mineração, níveis e equipamentos próprios.
- Loop idle contínuo com avanço por etapas, chefe final e recuo ao morrer.
- Economia global compartilhada entre o idle e o MMORPG.
- Marketplace oficial com moeda premium negociada por itens elegíveis.
- Equipamentos perdem durabilidade, mas não são destruídos permanentemente.
- Movimento do MMORPG por WASD e ausência de PvP na versão inicial.
- Quests elaboradas como diferencial central do MMORPG.

O primeiro protótipo deve provar um único ciclo completo:

1. Escolher uma missão de caça ou mineração na barra de tarefas.
2. Observar a equipe avançar automaticamente pelas etapas.
3. Ser derrotado, recuar uma etapa e continuar acumulando progressão.
4. Melhorar os equipamentos até derrotar o chefe final.
5. Transferir ou negociar uma recompensa elegível.
6. Usar a recompensa no MMORPG.
7. Concluir uma quest elaborada que aproveite o item ou recurso obtido.

Se esse ciclo for divertido, compreensível e desejável, haverá uma base sólida para expandir classes, regiões, economia e conteúdo social.

## 15. Design detalhado da POC

As próximas etapas da versão 0.2 foram desenvolvidas abaixo. A composição de três integrantes e suas funções foi definida pelo usuário. Nomes, números, habilidades, narrativa e solução técnica são propostas iniciais para a POC. O design está especificado; os sistemas ainda precisam ser implementados e testados.

### 15.1 Regras comuns das expedições

- Uma única equipe e uma única atividade ativa por conta: caça ou mineração. Os dois clientes abertos não duplicam produção.
- Uma rota possui cinco etapas. Cada uma das quatro primeiras exige cinco encontros vencidos para avançar; a quinta contém um chefe.
- Ao vencer o chefe, desbloquear a próxima rota e repetir a rota atual. Mudar automaticamente de região é uma preferência desativada por padrão.
- Se os três integrantes caírem, recuar uma etapa, com mínimo na etapa 1. Recuperar vida e energia por dez segundos e reiniciar os cinco encontros dessa etapa.
- Um integrante derrotado permanece caído até o fim do encontro. Ao vencer, recuperar a equipe antes do próximo encontro. A POC não tem ressurreição em combate.
- Recompensas são registradas apenas por encontros vencidos e extrações concluídas. Uma derrota não remove recompensas anteriores.
- Troca de equipamento ou missão entra em vigor entre encontros; mudar de missão cancela progresso parcial de extração e não concede recompensa parcial.
- O servidor guarda a missão, etapa, progresso, equipamento, semente aleatória, versão do balanceamento e instante do último processamento.
- Progresso offline: produzir por até oito horas após a última presença online confirmada pelo servidor, com a mesma eficiência online. O worker pode processar esse intervalo em vários blocos; seus checkpoints não renovam o teto. Ao reconectar, retomar a produção a partir do instante atual, sem recuperar o tempo excedente da ausência.
- Ao entrar, mostrar tempo contabilizado, vitórias, derrotas, recursos e reparos pendentes. Relógio local não influencia produção.
- Progressão offline respeita chefes, derrotas, energia e desgaste. Não multiplicar uma taxa máxima ignorando a sobrevivência da equipe.

### 15.2 Primeira rota de caça: Galerias da Caldeira

A rota atravessa túneis industriais abandonados sob a cidade de Brumaférrea. Os habitantes acreditam que os ruídos são falhas na tubulação; a equipe encontra máquinas que continuam obedecendo a uma ordem antiga.

| Etapa | Encontro e leitura | Pressão sobre a equipe | XP por integrante / vitória | Moedas da equipe / vitória | Material garantido / vitória |
|---|---|---|---:|---:|---|
| 1. Entrada de serviço | Dois ratos de fuligem | Aprender ataque e provocação | 12 | 8 | 1 sucata |
| 2. Trilhos partidos | Dois saqueadores mecânicos | Um tenta atingir o healer | 16 | 10 | 1 sucata |
| 3. Câmara de pressão | Dois sentinelas a vapor | Ataque pesado exige proteção do tank | 20 | 12 | 2 sucatas |
| 4. Ninho de engrenagens | Um drone e um vigia | Drone reduz a próxima cura se permanecer vivo | 24 | 14 | 2 sucatas |
| 5. Fornalha central | Chefe: Capataz de Ferro | Alternar dano no tank e dano em área | 50 | 35 | 5 sucatas e 1 componente bruto |

**Parâmetros iniciais dos inimigos:** vida total por encontro de 90 / 150 / 220 / 300 / 1.200; defesa de 2 / 3 / 4 / 5 / 7. Nas etapas comuns, distribuir a vida entre os inimigos. Ataques individuais de 8 / 11 / 14 / 17 de dano bruto a cada dois segundos. O chefe usa ataque básico de 24 a cada dois segundos e ataque em área de 18 por integrante a cada dez segundos. O ataque em área substitui o ataque básico daquele instante.

**Capataz de Ferro:** aos 50% de vida, passa a usar o ataque em área a cada oito segundos. Seu golpe pesado tem aviso de dois segundos e pode ser mitigado pela habilidade defensiva automática. Após 90 segundos de combate, aumenta o dano em 50%. O chefe não exige comandos manuais no idle.

**Objetivo do balanceamento:** uma equipe nível 1 vence a entrada; a primeira barreira aparece nas etapas 3 ou 4; níveis 3 a 5 e equipamento melhorado permitem vencer o chefe. Esses são alvos de playtest, não resultados de simulação já comprovados.

**Drops de equipamento:** chance inicial de 10% por vitória comum e 100% no chefe, com um item para uma das três funções. Quando houver drop, sortear raridade conforme 15.4. O projeto do Regulador de Éter é garantido na primeira conclusão da etapa 2, uma vez por conta. Esse projeto é narrativo e vinculado.

### 15.3 Primeira rota de mineração: Veio da Chaminé Velha

A mineração usa os mesmos três integrantes. O tank protege a escavadora, o DPS elimina ameaças e o healer estabiliza o sistema de éter e recupera a equipe. Equipamentos de combate também importam para sobreviver às emboscadas.

Cada ciclo tem 60 segundos de extração. Há 20% de chance de emboscada após a extração, acrescentando inicialmente 30 segundos esperados de combate. Cada etapa comum avança após cinco ciclos concluídos e eventuais emboscadas vencidas. Na etapa 5, cinco ciclos são seguidos pelo Guardião do Veio, que libera a próxima rota ao ser derrotado. Depois, a rota atual reinicia.

| Etapa | Local | Minério / hora de extração | Sucata / hora de extração | Cristais / hora de extração |
|---|---|---:|---:|---:|
| 1 | Escavação rasa | 30 | 8 | 0 |
| 2 | Filão de cobre | 42 | 10 | 0 |
| 3 | Veio etéreo | 54 | 12 | 2 |
| 4 | Bolsão instável | 66 | 14 | 4 |
| 5 | Câmara cristalina | 78 | 16 | 6 |

Essas taxas consideram apenas tempo de extração. Com ciclos de 60 segundos, emboscada de 20% e duração esperada de 30 segundos, o ciclo médio é 66 segundos. Na etapa 3, isso significa aproximadamente 49,09 minérios, 10,91 sucatas e 1,82 cristais por hora real antes de derrotas, chefes e reparos.

Usar acumuladores fracionários no servidor: a taxa de 30 minérios/hora produz um minério a cada dois ciclos, sem arredondar cada ciclo para zero. Creditar apenas unidades inteiras e conservar o restante. Uma extração concluída permanece creditada se a emboscada seguinte causar derrota.

Emboscadas usam inimigos da etapa equivalente da caça e não concedem moedas, materiais ou equipamento adicionais; concedem metade da XP de caça, arredondada para baixo. O Guardião do Veio usa os parâmetros do Capataz, concede 50 XP por integrante e desbloqueio, sem lote adicional de recursos. Assim, as taxas de mineração não escondem outra fonte de produção.

Ao derrotar toda a equipe em uma emboscada, recuar e recuperar-se conforme 15.1. Reparos e os encontros de chefe reduzem a produção efetiva. A planilha calcula um cenário de permanência em uma etapa, não a simulação completa da rota.

### 15.4 Atributos, experiência e equipamentos

#### Atributos iniciais no nível 1

| Função | Vida | Ataque | Defesa | Energia | Intervalo do ataque |
|---|---:|---:|---:|---:|---:|
| Tank | 180 | 8 | 10 | 40 | 1,5 s |
| DPS | 100 | 18 | 4 | 40 | 1 s |
| Healer | 110 | 6 | 5 | 60 | 2 s |

- Dano efetivo: `max(1, ataque bruto - defesa do alvo)`. A defesa não pode tornar alguém invulnerável.
- Crítico: 5% de chance, multiplicador 1,5. Aplicar o multiplicador ao ataque bruto antes da defesa e arredondar o dano final para baixo.
- Energia recupera dois pontos por segundo até o máximo. Prioridade de habilidades é avaliada a cada segundo.
- Tank: provocação sem custo a cada seis segundos, com duração de seis segundos. Ataques comuns escolhem o tank vivo sob provocação, exceto inimigos com ataque direcionado explícito.
- Tank: Escudo de Pressão custa dez de energia, tem recarga de 12 segundos e reduz em 40% o dano recebido por quatro segundos. Priorizar golpe pesado avisado, depois vida abaixo de 50%.
- DPS: Tiro Perfurante custa oito de energia, tem recarga de oito segundos e causa 180% do ataque bruto; priorizar drone, depois alvo do tank. O tiro substitui o ataque básico naquele instante.
- Healer: Infusão Etérea custa oito de energia, tem recarga de quatro segundos e cura 30 de vida no nível 1. Usar quando algum aliado vivo estiver abaixo de 75% de vida. Cura não excede vida máxima.
- Na morte do tank, inimigos comuns passam a atacar o aliado vivo com menor defesa. Não reviver automaticamente no meio do encontro.

**Progressão:** nível máximo 20 na POC. XP para passar do nível `L` ao seguinte: `60 + 30 × (L - 1)`. Não há perda de XP na derrota. Todos os integrantes recebem integralmente a XP indicada por vitória, inclusive quem caiu no encontro. No nível máximo, não acumular níveis futuros.

Por nível, aumentar vida-base em 10%, ataque-base em 6%, defesa-base em 5% e cura-base em 8% do valor de nível 1, de forma linear: `base × (1 + ganho × (nível - 1))`. Arredondar para baixo após calcular. Energia máxima e intervalos permanecem fixos. Equipamentos entram depois desses cálculos.

**Slots por integrante:** arma/foco, armadura e acessório. A POC tem nove peças equipadas ao todo. Arma de cada função não pode ser equipada em outra função; armadura e acessório indicam sua compatibilidade.

| Raridade | Probabilidade quando há drop | Bônus principal por peça | Uso na POC |
|---|---:|---:|---|
| Comum | 75% | +2 ataque na arma; +2 defesa na armadura; +10 vida no acessório | Equipamento inicial e substituições |
| Incomum | 20% | +3 ataque; +3 defesa; +15 vida | Primeiro salto de progressão |
| Raro | 5% | +4 ataque; +4 defesa; +22 vida | Recompensa desejável, sem necessidade para concluir a quest |

O foco do healer adiciona o bônus de ataque também à cura. Na POC, a raridade altera esses valores fixos; não há afixos aleatórios, set bônus ou reroll. Desmontar uma peça concede 1 / 2 / 4 sucatas por raridade. O modelo econômico trata essa entrada separadamente e usa zero como padrão até haver telemetria.

**Durabilidade:** máximo de 100 por peça; perder um ponto por encontro de combate vencido e dois por derrota da equipe. Extração sem emboscada não desgasta peças. Todas as peças equipadas sofrem desgaste; peças guardadas não sofrem.

- De 1 a 100, o bônus funciona integralmente. Em zero, o bônus fica inativo, mas atributos-base e habilidades continuam funcionando.
- Reparar custa duas moedas comuns por ponto restaurado por peça. Não consumir materiais na POC, para evitar dupla contabilização no primeiro modelo.
- Reparo automático é opcional, desativado inicialmente. Se ativado, reparar entre encontros ao atingir 20, até 100, apenas se houver saldo para a peça inteira. Ordem: armadura do tank, foco do healer, arma do DPS, demais peças por identificador.
- Equipamento nunca é destruído por desgaste. Exibir alerta quando o bônus ficar inativo ou faltarem moedas para reparar.

### 15.5 Transferência, vínculo e marketplace

O inventário é único por conta. "Transferir" significa alterar a disponibilidade de um lote do escopo idle para o escopo compartilhado. A operação não cria uma cópia e não gera recursos novos. Moedas comuns já pertencem à carteira única da conta.

| Categoria | Compartilhar com MMORPG | Negociar no marketplace | Regra inicial |
|---|---|---|---|
| Minério e sucata coletados | Sim | Sim, após liberar escopo compartilhado | Até 200 unidades combinadas/dia por conta |
| Cristal bruto | Sim | Sim, após liberar escopo compartilhado | Até 12 unidades/dia por conta |
| Material de criatura | Sim | Sim se marcado negociável | Até 40 unidades/dia por conta; sem fonte na primeira rota |
| Moedas comuns de caça | Já na carteira compartilhada | Não vender diretamente por premium | Emissão até 500/dia por conta via idle |
| Equipamento comum e incomum | Sim, se compatível | Sim se não equipado | Até duas peças/dia; vincula ao equipar |
| Equipamento raro da POC | Sim, se compatível | Não | Vinculado à conta na obtenção |
| Projeto do Regulador, componente da quest e Regulador final | Sim | Não | Progressão narrativa vinculada |
| Cosmético elegível futuro | Sim | Conforme origem | Sem fonte ou venda na POC |
| Moeda premium | Carteira central | Meio de pagamento | Nunca produzida por caça, mineração ou XP |

Os limites de compartilhamento reiniciam às 00:00 UTC e são consumidos quando o lote muda de escopo. Lotes excedentes ficam no idle, utilizáveis localmente; não somem. Ao atingir o limite de emissão de moedas do idle, continuar XP e drops, sem novas moedas naquele dia. Isso protege a carteira já compartilhada sem criar uma conversão artificial entre duas moedas comuns.

Equipar bloqueia a peça para transferências enquanto estiver em uso. Compatibilidade com o MMORPG exige uma definição de item aprovada pelo servidor; não converter automaticamente uma arma idle em qualquer arma de classe. A primeira integração efetiva usa materiais e o Regulador, reduzindo o catálogo necessário.

**Marketplace da POC:** ambiente de teste com moeda premium fictícia, sem pagamentos reais. Compra e venda usam saldo de teste fornecido pelo ambiente de desenvolvimento, sem misturá-lo à emissão por gameplay. Saldo inicial em cenários econômicos: zero, até o teste injetar uma quantidade explícita.

- Preço em unidades inteiras de premium, mínimo uma unidade.
- Taxa de venda em moedas comuns: `max(5, arredondar_para_cima(preço_premium × 0,5))`, paga pelo vendedor ao concluir a venda. Esses coeficientes não estabelecem câmbio ou conversibilidade entre moedas.
- Item listado fica em custódia. Cancelar devolve o mesmo item, sem taxa de venda.
- Compra debita premium do comprador e credita exatamente a mesma quantidade ao vendedor; a taxa consome moeda comum do vendedor. Exigir saldo para os dois débitos antes de concluir.
- Custódia, propriedade, saldos, taxa e histórico mudam numa transação atômica e idempotente. Compras simultâneas não podem vender a mesma unidade duas vezes.
- Pagamentos reais, retenções por fraude e operação comercial entram após validação técnica e econômica; não são dependências do primeiro ciclo de gameplay.

### 15.6 Modelo econômico global

A [planilha econômica](economia/Economia_Aetherforge_POC.xlsx) permite editar população, duração das sessões, recompensas, custos, taxas e limites. Todos os valores são hipóteses de design, não métricas observadas nem preços de mercado.

O cenário inicial divide as oito horas de idle por conta em quatro horas de caça e quatro de mineração. Isso respeita uma equipe única: as atividades não acontecem simultaneamente. Para caça, usar inicialmente 24 encontros comuns vencidos por hora, etapa média com dez moedas por vitória e nenhum chefe no cenário estacionário. É uma aproximação para testar fluxos; a simulação real substituirá essa hipótese.

**Entradas:** moedas de caça limitadas a 500/dia, moedas de recompensas do MMORPG, produção de minério/sucata/cristais e desmontagem explícita. **Saídas:** reparos, crafting, consumíveis e taxas de venda. **Transferências internas:** compartilhar materiais e trocar premium entre jogadores; nenhuma aumenta a oferta global.

**Receita do Regulador:** 20 minérios, cinco sucatas, dois cristais, um projeto, um Núcleo de Pressão obtido na dungeon e 120 moedas comuns. Craft de melhoria simples para o modelo recorrente: dez minérios, duas sucatas, um cristal e 60 moedas. A planilha mostra os dois consumos separadamente, usando zero Reguladores/dia no estado recorrente porque a quest é única.

**Indicadores:** geração bruta e líquida de moeda comum por conta/dia e no mundo; consumo como fração da emissão; produção total de materiais, parcela compartilhada e saldo após consumo. Saldo de materiais do modelo representa o conjunto idle + compartilhado; não pressupõe que todo minério produzido possa ser vendido.

**Premium:** entrada apenas por injeção explícita de teste na POC; no produto, compra oficial confirmada. Negociação entre jogadores é redistribuição. Uma eventual compra em loja oficial seria saída e terá seu modelo próprio quando o catálogo existir. Não misturar premium com moedas comuns nas somas.

**Regra de avaliação:** crescimento líquido positivo pode ser adequado à progressão inicial; torna-se um sinal a investigar se continuar sem demanda no jogo maduro. A planilha não comprova estabilidade de preços. Medir emissão/consumo por fonte, estoque por conta, concentração de riqueza, velocidade das vendas, reparos ignorados e rendimento real por rota.

### 15.7 Primeira quest principal: O Coração que Ainda Bate

**Premissa:** as máquinas sob Brumaférrea estão desviando energia do hospital para manter uma antiga instalação funcionando. A solução envolve descobrir quem reativou a rede e decidir como estabilizá-la sem deixar uma parte da cidade sem energia.

**Personagens:** engenheira Iara Voss, que tenta manter o hospital aberto; maquinista Bento, acusado de sabotar a rede; e o autômato C-17, que obedece à ordem "preservar os vivos", mas interpreta motores em funcionamento como seres vivos.

**Entrada:** o idle encontra o projeto do Regulador na etapa 2 da caça. A quest oferece a alternativa de recuperar o mesmo projeto num armário de manutenção do MMORPG, durante a investigação. Ambas as rotas avançam a mesma flag da conta e nunca duplicam a recompensa. Assim, jogar apenas o MMORPG continua viável.

| Ato | Objetivo jogável | História e evento | Condição de conclusão |
|---|---|---|---|
| 1. Um projeto impossível | Levar o projeto à oficina de Iara | Ela reconhece o selo de um engenheiro dado como morto | Conversa e inspeção do projeto |
| 2. Vapor sem destino | Investigar três indícios nos mapas Trilhos Baixos e Pátio de Purga | Um manômetro invertido, um recibo de energia e a ferramenta de Bento contradizem a acusação | Examinar indícios; combate apenas bloqueia acesso |
| 3. A ordem antiga | Entrar na dungeon Estação de Condensação | C-17 recita a ordem; Bento revela que ligou a estação para salvar trabalhadores presos | Abrir a válvula de acesso e ouvir os dois relatos |
| 4. O coração de ferro | Enfrentar o Supervisor de Pressão | Alternar duas válvulas durante avisos de sobrecarga permite interromper o chefe | Derrotar chefe e receber Núcleo de Pressão uma vez |
| 5. Para quem vai o calor? | Escolher priorizar hospital ou bairro operário | Iara alerta sobre pacientes; Bento descreve famílias sem aquecimento | Escolha registrada na conta |
| 6. Uma máquina com propósito | Fabricar e instalar o Regulador | C-17 recebe uma ordem que distingue vidas de motores | Consumir receita e instalar na oficina |

**Exemplos de diálogo:**

> Iara: "Essa assinatura não aparece num projeto há vinte anos. E ontem alguém usou esse mesmo circuito para desligar o hospital."

> Bento: "Eu abri a válvula. Tinha gente lá embaixo. Se quer me prender por isso, primeiro me ajuda a tirar os que ficaram."

> C-17: "Preservar os vivos. Há quarenta e dois motores ativos. Não posso permitir que parem."

**Escolha:** as duas opções dão a mesma recompensa de poder. Priorizar o hospital mantém sua ala ativa e muda o agradecimento de Iara; priorizar o bairro mostra aquecedores funcionando e muda o diálogo de Bento. As mudanças são vistas apenas pela conta ou grupo na instância, evitando escolhas contraditórias no mundo compartilhado. Uma futura quest pode recuperar essa decisão.

**Recompensa:** desbloquear o Regulador na oficina da conta e conceder +4% à vida-base do personagem MMORPG e dos três integrantes idle. O Regulador é uma instalação única vinculada, não duas cópias de um item equipável. Pode beneficiar ambas as experiências ao mesmo tempo porque é um bônus de conta.

**Materiais:** o jogador pode obter a receita inteira pela caça/mineração idle ou por fontes equivalentes nos dois mapas do MMORPG. Na primeira quest, armários de manutenção garantem os 20 minérios, cinco sucatas e dois cristais faltantes; marcar cada concessão para impedir repetição. Essa alternativa é vinculada, não negociável. As 120 moedas podem vir da caça ou de adiantamento único de Iara; pagar apenas a diferença que faltar para fabricar.

**Recuperação:** sair da dungeon preserva indícios e escolha; repetir o chefe não concede outro Núcleo narrativo. Craft e conclusão têm chave idempotente. Se a mochila estiver cheia, manter recompensa pendente no servidor, sem perdê-la. Nenhuma opção de diálogo pode bloquear permanentemente a quest.

**Primeiros trinta minutos, alvo de teste:** 0–5 montar a equipe e escolher rota; 5–10 encontrar o projeto e observar a primeira barreira; 10–17 investigar no MMORPG; 17–25 dungeon e escolha; 25–30 fabricar, instalar e perceber o benefício ao voltar ao idle. Disponibilizar a alternativa de materiais do MMORPG para que o tempo de mineração não torne a demonstração uma espera obrigatória.

**Critério narrativo:** o jogador entende o problema antes do chefe, consegue explicar a motivação de Bento e reconhece uma consequência da escolha. A quest usa investigação e acontecimentos, sem uma exigência artificial de matar dez inimigos.

### 15.8 Produção e critérios de aceitação

O backlog completo e suas dependências estão em [POC_Producao.md](POC_Producao.md). A prioridade atual está em [Desenvolvimento.md](Desenvolvimento.md): desenvolver o idle primeiro. A sequência abaixo registra o plano completo da POC:

1. Validar a janela idle no Windows e migrar a apresentação para modelos 3D animados, medindo o cenário compacto.
2. Implementar a simulação de três integrantes, rota de caça, mineração e avanço offline.
3. Persistir conta, carteira, equipamentos e inventário único com operações idempotentes.
4. Construir cliente MMORPG e servidor de mapa para duas pessoas jogarem juntas.
5. Implementar a quest, receita e bônus de conta nas duas experiências.
6. Validar marketplace com saldo fictício, concorrência e custódia.
7. Fazer playtest do ciclo completo e substituir hipóteses da planilha por telemetria.

**POC aprovada quando:** dois jogadores percorrem a dungeon juntos; a equipe idle progride e recua sem intervenção; uma ausência de oito horas gera recompensas coerentes com a simulação; reiniciar ou repetir pedidos não duplica recursos; o mesmo inventário aparece nos dois clientes; a quest termina e altera o comportamento das duas experiências; uma compra concorrente vende cada item uma única vez.

O desenho da POC está concluído nesta versão. Nenhum sistema listado acima deve ser marcado como implementado apenas por constar no documento.

## 16. Stack e arquitetura propostas

A recomendação é **Godot 4 .NET + C# para os dois clientes, Godot headless para mapas online, ASP.NET Core para serviços da conta e PostgreSQL para persistência**. O detalhamento, alternativas e fontes estão em [Stack_Tecnologica.md](Stack_Tecnologica.md).

Essa escolha prioriza mapas e personagens 3D, uma base de linguagem comum e custo inicial controlável. O idle usa um pequeno trecho 3D com orçamento de malhas, materiais, texturas e animações. A transparência, integração com a barra e consumo da nova apresentação precisam de validação; os conceitos gerados não constituem benchmark.

Usar uma janela transparente sem bordas posicionada junto à barra de tarefas, com controles acessíveis e integração nativa quando necessário. Essa é a proposta técnica para a POC; não pressupõe que o Windows ofereça um espaço nativo para renderizar o jogo dentro da barra.

As simulações e recompensas que alimentam a economia são autoritativas no servidor. Clientes mostram animações e enviam intenções, mas não definem drops, saldos, tempo offline ou propriedade. O núcleo de regras C# não depende de cenas Godot. A compatibilidade de framework entre clientes e serviços será fixada no teste técnico inicial.

Para a POC, manter serviços da conta em um monólito modular, banco único e processos de mapa separados. Redis, Kubernetes, divisão em microserviços e infraestrutura multirregional são opções de expansão condicionadas a medições. Economia global não exige colocar todos os jogadores em um único mapa ou servidor.

## 17. Próximos passos de execução

Os oito itens de design da versão 0.2 foram especificados nas seções 4.6 e 15, na planilha econômica e no backlog. A base do M0 já existe: cliente idle com combate animado, sprites em oito direções, cenário 3D, dois clientes conectados e compatibilidade da biblioteca C# com cliente e API. A apresentação deve mostrar movimento, ataques, projéteis, cura e impactos, conforme a referência adicional do usuário.

Esse pipeline de sprites registra o estado do protótipo existente. A direção atual usa personagens e cenário 3D; validar primeiro um integrante com rig, depois o trecho de cenário e a equipe completa. A migração da apresentação ainda não foi implementada.

O próximo incremento é M1: implementar as regras completas da rota de caça, XP e equipamento, chefe e recuo. A matriz de DPI, monitores e comportamento do desktop continua necessária para fechar TEC-02. O combate local demonstrativo do M0 não implementa ainda a economia persistente ou o progresso offline.

Também foi implementado um teste de integração antecipado: a primeira vitória do idle concede uma Couraça de Latão incomum; o jogador envia o mesmo ID ao inventário compartilhado, equipa no MMO e reduz o dano de um canhão de teste de 20 para 17 por meio do bônus de defesa +3. O equipamento persiste após reinício. O [passo a passo](Teste_Equipamento.md) e o atalho `Testar Integração.cmd` permitem reproduzir esse ciclo. A conta local e o armazenamento JSON desse teste serão substituídos pelos serviços completos nas etapas seguintes.
