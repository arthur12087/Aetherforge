# Interface e fluxos

[Índice](README.md) · [Status em detalhe](Status.md) · [Arquitetura](Arquitetura.md). Evidências U01, M01 e V01 conforme o [registro](Evidencias.md).

## Modelo de apresentação

A cena `level0` separa mundo, gerenciamento, tooltips, popups e logs em raízes diferentes. Foram decodificados 12.698 GameObjects e seus transforms, dos quais 12.637 usam `RectTransform`. Há nove componentes Canvas. A quantidade representa a cena serializada inteira, inclusive objetos inativos e templates; não é quantidade de widgets visíveis nem custo por frame. [U01]

| Raiz | Papel sugerido pelo conteúdo observado |
|---|---|
| `World` | Heróis, monstros, posições de spawn e elementos do palco |
| `Canvas_Stage` | WorldUI, painéis do combate e transições de etapa |
| `Canvas_Main` | Áreas esquerda, centro e direita de gerenciamento |
| `Canvas_Tooltip` | Tooltips de itens, habilidades, runas e stages; avisos transitórios |
| `Canvas_Popup` | Avisos e operações pendentes |
| `Canvas_LogUI` | Janela de logs |
| `Canvas_MoveingSlot` | Representação visual de slot em movimento |
| `Canvas_FirstSetting` | Configuração inicial |
| `Canvas_FaddedUi`, `Canvas_QA` | Elementos auxiliares; a função completa não foi rastreada |

Os nomes sugerem papéis, mas só as estruturas foram lidas. A presença de um canvas auxiliar não prova que o jogador tenha acesso direto a ele.

## Gerenciamento em três áreas

| Área | Conteúdos presentes na cena | Consequência para leitura |
|---|---|---|
| Esquerda | `UI_Status`, `UI_Stash_Remake` | Personagem/build ou armazenamento |
| Centro | `UI_Hero`, configurações, correio, loja, trading stash, recompensa offline e popups | Centro de gerenciamento e acesso a outros sistemas |
| Direita | `UI_Portal`, `UI_RemakeCube` | Escolha de conteúdo ou transformação de itens |
| Área própria no Main | `UI_Rune` | Progressão de conta por runas |

Todos esses objetos existem no arquivo. Não se demonstrou quais se substituem, quais coexistem ou quais dependem de desbloqueio. A captura histórica V01 comprova visualmente a coexistência de Status, Herói e Portal naquela captura; não de todas as combinações da versão 1.3.0.

O gerenciamento é organizado dentro de canvases Unity. Não há evidência suficiente para chamar cada painel de janela nativa independente. O controle desktop deve ser analisado separadamente no `WindowManager`.

## Herói, equipamento e inventário

`UI_Hero` é o GameObject **3054** em `Canvas_Main/Main/Center`. Sua declaração preserva referências a ouro, nome/nível, EXP, gear slots, active skill slots, scroll de inventário, formação, skins, relíquias e atalhos para estatísticas/configurações/logs. [U01, M01:T744]

| Região | Evidência | O que podemos afirmar |
|---|---|---|
| Identidade | `HeroPanel`, `SelectUnitName`, `HeroFullIllustPanel`, campos de nome/nível | Existe uma apresentação central do personagem selecionado |
| Equipamento | `gearSlots`, `EquipPanel`, `HeroGear` | Há slots dedicados a equipamento |
| Inventário | `ui_Inventory`, `m_inventoryScrollRect`, prefab de slot, sorting | Bolsa e ordenação têm componentes próprios |
| Formação | `ArrangeOptionPanel`, `ui_Arrange` | Formação aparece como subárea de gerenciamento |
| Progressão adicional | `UI_Relic`, `PetPanel`, `SkinSetList` | Sistemas de relíquia, pet e cosméticos têm apresentação nominal |
| Atalhos | `BottomButton_Layout`, `TopRight Button Panel` | Grupos distintos de ações globais e navegação |
| Feedback | `ItemEquipGuide`, `ItemEquipNoticePos`, efeitos de compra/equipar | Há elementos de orientação e feedback de alteração |

A captura V01 mostra equipamento ao redor do personagem, retratos pequenos de equipe e uma grade compacta de itens. É possível observar cores de fundo e badges, mas sua semântica exata deve vir dos dados/código; não assumir que todo badge seja raridade ou comparação de melhoria. A grade histórica não define a capacidade completa da bolsa: os dados da versão instalada incluem expansão e scrolling. [V01, D01]

`HeroArrowSwitchPanel` declara botões anterior/próximo. `HeroSlot` possui ações nomeadas de seleção e eventos de pointer. `HeroSmallSlot` declara bloqueio e indicador de pontos de habilidade. Isso sustenta a seleção entre personagens e a apresentação de estados, sem comprovar cada atalho ou animação. [M01:T737, T771, T773]

Os botões inferiores da cena se chamam `Button_BottemMenu` e variantes numeradas. A sequência exata de ícones/rotas não foi recuperada por um binding validado. Não deduzir a rota de cada botão apenas pelo número ou pela posição no screenshot antigo.

## Portal e escolha de conteúdo

`UI_Portal`, GameObject **9673**, fica à direita e contém: [U01]

- Seletor de dificuldade (`Difficulty Panel`).
- Título de ato, grupo de abas rolável e holder de conteúdo.
- Área de conteúdo rolável com o nome histórico `Scroll View_Inventory`.
- Estados de bloqueio, `PlagueLock` e elementos específicos de Plaguelands.
- Botão de fechar, bloqueadores e popup de aviso.

A captura V01 mostra abas de atos, nome de região, caminho de nós, estágios numerados e marcador de posição atual. As tabelas `StageInfoData` e `StageLevelInfoData` sustentam regiões/fases e escalonamento no conteúdo distribuído. O nome `Scroll View_Inventory` dentro do Portal ilustra por que nomes isolados não bastam para determinar função: o contexto de hierarquia é necessário. [V01, D01, U01]

O fluxo **escolher dificuldade → ato/etapa → iniciar ou repetir** é uma interpretação funcional sustentada pela interface e pelos contratos, ainda sem sequência de inputs observada na 1.3.0. Consumo de entradas, condições de desbloqueio e derrota precisam ser lidos junto dos dados e depois testados, não deduzidos de um mapa estático.

## Faixa de combate e HUD

`Canvas_Stage/WorldUI`, GameObject **2403**, possui `sizeDelta` local 366 × 172 e elementos de painel inferior, posição de logs superior/inferior, bloqueio de raycast e efeitos de fade de início/fim. Há também regiões de batalha superior/inferior com altura local 172 no `Canvas_Main`. O tamanho físico e o encaixe na barra de tarefas dependem do controlador de janela e da escala em execução. [U01]

Na captura histórica, o combate ocupa uma faixa horizontal abaixo das janelas: equipe e inimigos têm barras de vida, projéteis e números de dano, e um aviso de item obtido aparece acima da faixa. A imagem evidencia a composição visual naquele instante; não mede frequência de loot, densidade média de efeitos ou comportamento quando outro programa cobre a janela. [V01]

O metadata de `DamageTexture` declara cores próprias para crítico, cura e elementos, além de materiais/feedback. O arquivo de cena contém objetos de transição e slots de feedback. A duração de hit-stop, shake, animações e sons de impacto não foi medida. [M01:T111, U01]

## Tooltips e operações pendentes

Há tooltips distintos para item, item equipado, habilidade, habilidade no Status, stage, Plague stage, runa e informações normais. A separação entre tooltip do item observado e do equipado é compatível com comparação de peças, mas a lógica de comparação e seus sinais não foram recuperados. [U01]

`Canvas_Popup` contém `OneButtonNoticePopup`, `TwoButtonNoticePopup`, `BoxPendingPopup`, `EnchantPendingPopup` e `SynthesisPending`. Isso demonstra componentes dedicados a mensagens e ações pendentes. Não prova, por si só, que uma operação seja transacional, remota ou segura contra repetição. [U01, M01]

## Configurações

`UI_Setting`, GameObject **8793**, separa áreas roláveis de áudio, geral e gráfico. A cena inclui controles nominais para: [U01]

| Grupo | Exemplos de controles recuperados | Limites |
|---|---|---|
| Áudio | Master, BGM, GameSFX, UISFX e BattleLog | Intervalos e curvas de volume não decodificados |
| Desktop | AlwaysOnTop, AlwaysTryOnTop, reset de posição, ZoomMode, MaxFPS | API de janela, DPI e aplicação efetiva não observados |
| Legibilidade | Mostrar dano/cura, nível de item e cooldown de skill | Persistência e atualização efetiva não testadas |
| Gestão | Opções de repetição, logs, tooltips e abertura de baús declaradas no save | A existência do campo não garante acesso em toda fase do jogo |

O `WindowManager` declara presets de canto/centro superior/inferior e opções nominais de zoom 1, 1,25, 1,5, 2 e 3. Não há medição de CPU/GPU, consumo minimizado, click-through, autostart ou compatibilidade multimonitor. Ver [Arquitetura](Arquitetura.md).

## Padrões de interação que merecem adaptação

As propostas abaixo são interpretações para Aetherforge:

1. Manter o combate como contexto contínuo enquanto o gerenciamento abre acima dele.
2. Dar papéis estáveis às áreas: Status explica personagem/build; Herói gerencia posse/equipamento; Expedição escolhe conteúdo.
3. Mostrar resumo antes do detalhe e manter o personagem selecionado entre as áreas.
4. Usar tooltips para significados e modificadores, reservando o detalhe para decisões.
5. Sinalizar seleção, bloqueio, pontos disponíveis e alterações com ícone/texto além da cor.
6. Tratar zoom, altura disponível e rolagem como parte do fluxo desde a primeira implementação.

Os fluxos de mouse/teclado, foco, hover e cancelamento ainda precisam de observação direta. A [lista de lacunas](Lacunas.md) impede confundir esse mapa de estrutura com uma especificação comprovada de todos os gestos.
