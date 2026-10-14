# Aetherforge: protótipos das telas idle

## Abrir e revisar

Abra **Ver Prototipo Idle.cmd**, na raiz do repositório, ou o [protótipo HTML](prototipo/index.html) no navegador. Não precisa instalar dependências nem iniciar a API. Também está disponível na cópia local do projeto visual, pela navegação **Telas do idle** do dossiê.

Esta entrega preserva a direção artística v3 aprovada. É um protótipo de interface com dados fictícios em memória, separado da apresentação Godot. Fechar ou recarregar restaura o estado inicial. O save do jogo não é utilizado.

## Telas e transições

| Tela | Decisão principal | O que experimentar |
|---|---|---|
| Expedição | Entender atividade, etapa, trio e próximo obstáculo | Faixa compacta, situação de combate, mineração e recuperação |
| Rotas | Escolher caça ou mineração | Confirmação, alteração pendente, cancelar e aplicar entre encontros |
| Equipe | Entender o papel e equipamento dos três integrantes | Fichas individuais, atributos, habilidades automáticas e comparação por slot |
| Inventário | Avaliar uma peça | Filtros, comparação, incompatibilidade, troca programada e desmontagem confirmada |
| Oficina | Decidir se e quando reparar | Custo, saldo após reparo, bônus inativo e automático opcional |
| Registro | Entender os acontecimentos | Histórico de ações, resultado de chefe, recuo e prévia offline M2 |
| Configurações | Ajustar a apresentação | Compacta, movimento reduzido e preferência de reparo |

Os diálogos completam os fluxos de mudança de missão, reparo, desmontagem, vitória do chefe e retorno offline. A próxima região explica o desbloqueio, mas seu botão permanece desativado porque o conteúdo ainda não foi especificado.

O [mapa de fluxos](Fluxos_Idle_Prototipo.md) contém as regras, os exemplos numéricos e a cobertura atual separada das pendências.

## Controles de demonstração

A faixa clara **Protótipo · dados fictícios** fica fora do HUD do jogo. Seus controles permitem abrir os estados Combate, Mineração, Chefe, Derrota, Offline, Sem moedas e Rota concluída. **Concluir encontro**, **Concluir ciclo** e **Avançar recuperação** são comandos de revisão; não representam ações manuais exigidas para produzir no idle.

Sugestões de revisão:

1. Na Oficina, reparar a Couraça de caldeira: 78 → 100 custa 44 moedas; a carteira fictícia passa de 640 para 596. Cancelar ou Escape mantém o saldo.
2. No Inventário, comparar o Rifle de pressão com o Guarda e depois com o Pistoleiro. A primeira opção explica a incompatibilidade; a segunda programa a troca. A ficha só muda após o intervalo.
3. Em Rotas, programar mineração e concluir o encontro. Ao voltar à caça durante uma extração, o diálogo avisa que somente o ciclo incompleto será descartado.
4. Abrir **Sem moedas** para observar a ação de reparo bloqueada com motivo. Abrir **Derrota** para observar recuo, equipe caída e recuperação.
5. Abrir **Chefe**, concluir o encontro e ver a próxima rota liberada enquanto a atual reinicia. Abrir **Offline** e reabrir o relatório para confirmar que o lote fictício não se repete.

## Decisões visuais

- Carvão e aço sustentam a interface; osso identifica seleção e ação. Os textos secundários usam uma cor com contraste suficiente sobre as duas superfícies.
- Archivo permanece local. Os ícones são desenhos SVG funcionais; as artes são os [dois conceitos v3 existentes](Direcao_Artistica_v3.md), gerados anteriormente com GPT Images.
- A Expedição preserva a imagem inteira com `contain`. As fichas usam recortes CSS da prancha original para destacar o integrante selecionado; a prancha completa permanece acessível.
- O desktop usa navegação lateral. Nas larguras menores, todos os destinos aparecem em linhas, sem exigir uma lista horizontal cortada.
- A faixa compacta foi medida em **960 × 216 px** no desktop. O botão para expandir fica fora dessa caixa. Em telas menores, a largura se adapta ao espaço disponível.

A [Taste Skill v2](referencias/taste-skill/SKILL.md) declara que não se aplica a dashboards ou telas de aplicativo. Esta entrega mantém o contrato visual aprovado e utiliza inspeção de capturas, contraste e interação; não reivindica aprovação do inspector de sites nem atribui uma nota de landing page ao HUD.

## Verificação realizada

- `node --check` passou para o JavaScript do protótipo.
- Capturas das sete telas em 1440, 768 e 390 px: 21 apresentações verificadas automaticamente quanto a largura e imagens carregadas. Nenhum transbordamento da página, imagem quebrada ou exceção de JavaScript.
- 36 verificações de fluxo passaram: navegação, confirmação/cancelamento, Escape e foco, reparo, saldo insuficiente, compatibilidade, troca entre encontros, desmontagem, mudança de missão, frações, chefe, recuperação, offline sem duplicação, compacto e preferências.
- Oito verificações adicionais passaram: teto fictício de 500 moedas, equipamentos distintos em chefes repetidos, dimensões compactas em quatro larguras e diálogos de chefe/offline em 390 px.
- Abertura direta por arquivo local e execução pelo endereço da cópia do projeto visual foram verificadas. No endereço do projeto, as 21 capturas e as 36 verificações de fluxo também passaram.
- Links locais e `git diff --check` verificados. Não houve mudança no runtime; os testes de Godot e integração não foram executados para esta entrega de interface.

As evidências de revisão ficam em `artifacts/idle-ui-qa/`, ignoradas pelo Git. As imagens foram examinadas para corrigir navegação cortada no telefone, foco visual da ficha de personagem, contraste do texto pequeno e altura excedente da faixa compacta.

O MCP do projeto visual retornou **Transport closed** nesta sessão. A entrega foi sincronizada por arquivos com o projeto local existente, e o endereço servido pelo projeto foi aberto e testado. O dossiê anterior e seus conceitos permanecem disponíveis.

## Limites e passagem para produção

Não há modelos 3D, rigs, animações ou combate executável nestas telas. A mineração demonstra ciclos e conservação de frações, sem sorteio de emboscadas; o inventário lista equipamentos, enquanto materiais aparecem na Expedição. O relatório offline usa uma amostra fixa de 12 h de ausência e 8 h contabilizadas, sem cálculo autoritativo de XP e desgaste.

Autenticação, compartilhamento, MMORPG e marketplace permanecem fora desta revisão. A implementação posterior continua seguindo [M1 e os critérios de produção](../POC_Producao.md), preservando o multiplayer existente. A prova de personagem e cenário 3D leve continua dependendo de ART-03/ART-04 e medições no motor.
