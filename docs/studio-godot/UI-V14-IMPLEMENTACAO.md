# UI V14 — contrato de implementação

**Concluído:** ver [entrega V14](../../canvas/documents/ui-v14-entrega.md). Este contrato foi a base dos três workers; os ajustes abaixo ocorreram na integração, depois de suas entregas.

- Pesos por tags OpenType numéricas e conferência dos caches do TextServer; a chave textual era descartada nesta integração.
- `ExpeditionView.DetailedHud` sincroniza palco/toggle; `ExpeditionStage.Hud` expõe diagnóstico somente de apresentação. `VerifyMenuBounds` é um fixture opt-in, não DPI físico.
- Entrada local por `ExpeditionMenuInput`; cabeçalhos/rodapés fixos, altura nativa adaptada à faixa do palco quando possível. Pool do HUD continua limitado; faixa de equipe preserva três barras quando não cabem sobre as cabeças.
- Orquestrador integrou `Main`, prova tipográfica, `Run -UiScale`, checks, capturas e regressões. Sem nova simulação ou alteração do Domain.

O usuário autorizou implementar as recomendações do plano `canvas/documents/ui-v14-fontes-menus-hud.md`. Esta é uma revisão de apresentação: NÃO alterar Domain, API, persistência, multiplayer, personagens/cenário nem saves. Manter três janelas nativas, três integrantes, sete colunas/21 slots indisponíveis, cinco etapas e marcha real. Paleta quente e v12 preservadas. Não gerar novas pinturas nem importar arte de pesquisa.

## Propriedade exclusiva

- **tema**: `src/Aetherforge.Client/BrassUi.cs`, novos arquivos `BrassTypography.cs`, `BrassIcons.cs`, `BrassTypeProof.cs` e recursos NOVOS `Assets/Brass/Fonts/SourceSans3-Variable.ttf`, licença e `Assets/Brass/Icons/`. Pode atualizar `Assets/Brass/PROVENANCE.md`. Não editar View/Stage/HUD/Main.
- **hud**: `ExpeditionVitals.cs`, `ExpeditionStage.cs` e novos helpers iniciados `ExpeditionHud`. Não alterar geometria/atores ou simulação. Não editar tema/View/Main.
- **menus**: `ExpeditionView.cs` e novos helpers iniciados `ExpeditionMenu`. Não editar tema/HUD/Stage/Main.
- **orquestrador**: Main, scripts, checks/QA, documentação/Canvas, integração e ajustes depois que workers terminarem.

Workers: `codex exec --sandbox workspace-write --ephemeral`, ler fontes antes de editar; sem commits, alterações globais ou bypass de sandbox. Não compilar/importar/executar Godot em paralelo; reportar ao orquestrador. Relatórios em `artifacts/ui-v14/agents/*-last.md`.

## API de tema acordada (worker tema implementa)

Preservar todas as assinaturas e classes existentes de BrassUi. Acrescentar:

```csharp
// Cores existentes permanecem. Limelight só marca e títulos principais.
public static float Scale { get; set; } // default 1; configurar antes de criar UI, 1..1.5
public static int S(float value); // arredondar value * Scale
public static Font BodyFont { get; }   // Source Sans 3 regular/medium
public static Font StrongFont { get; } // Source Sans 3 semibold real, FontVariation wght
public static Font DisplayFont { get; } // Limelight regular
public static Label Emphasis(string text, int size = 18, Color? color = null);
public static Texture2D Icon(string key); // seis SVGs: shield, rifle, heal, pause, return, route
public static void Primary(Button button);
public static void Quiet(Button button); // secundário, sem borda dourada em tudo
```

`Text`, `Wrap`, `Box`, `Margin`, `Meter` e `Button` aplicam Scale INTERNAMENTE a fontes/margens/altura. Chamadores NÃO passam S() nos argumentos dessas factories. Chamadores usam S() em `CustomMinimumSize`/geometria/layout próprios. `Text(..., title:true)` usa DisplayFont; demais usam BodyFont. Emphasis usa StrongFont. Cores e tamanhos centralizados/cached; não carregar recursos por frame.

`BrassChassis` ganha `public int Instrument {get;set;}` (0 Mesa reta, 1 Estojo em arco, 2 Telégrafo escalonado), definido antes de AddChild. Factory Button continua retornando Button, aceita Pressed e foco. Foco visível por forma, não só cor. Ícones vetoriais originais em SVG 24, duas massas linho/latão, não emoji. Baseline corpo 14, dados 26–30, labels críticos 13+, botão mínimo 34, quiet surfaces e estados explícitos. Fonte Source Sans 3 e licença COPIADAS dos arquivos locais em `docs/visual/assets/fonts/v5/`, originais mantidos.

`BrassTypeProof : Control` opcional porém recomendado: prova NATIVA lado a lado de Jost e Source Sans 3 em tamanhos 13/14/18/26 e pesos 400/600, números, acentos, ação de escudo e placa sobre fundo escuro. Não inventar dados de jogo como se fossem sessão; marcar espécime tipográfico. Main encaminhará `--mode=ui-proof`. Não controlar build/quit/captura no proof; o Main cuida de --duration/--screenshot.

## API de palco/HUD acordada (worker hud implementa)

Preservar Stage.Bind e toda a marcha/câmera/nomes dos Nodes usados por checks. Acrescentar em ExpeditionStage:

```csharp
public bool DetailedHud { get; set; } // default false, repassar ao vitals
public int SelectedMemberIndex { get; set; } // default 0, nomes via seleção/foco
```

A view controla essas propriedades; NÃO chamar Advance no palco ou HUD. Vitals lê BrassUi.BodyFont/StrongFont/Scale, reutiliza recursos, projeta âncoras para coordenadas locais. Não usar nova fonte pixelada nem reescalar pela câmera.

HUD: barras 48–56 × 6–7 (base), roles/ícones, nomes em caixa normal 13–14 em placas opacas quando visíveis; default conciso, nome do membro selecionado ou em hover; inimigos com nome quando couber/hover/detalhado. Nome/vida dedicada do chefe no topo do palco durante Combat, sem duplicar na cabeça. Retângulos medidos para evitar colisões/bordas, priorizar ocultar texto secundário a diminuir fontes; não invadir rostos. Tooltip/inspeção de aliado também acessível pela seleção de Herói.

Números: mover os números de Label3D para overlay Control em espaço de tela, fonte Strong 17–20, sinais +cura/-dano e ícones/forma além de cor. Remover o caminho 3D antigo para não duplicar. Consumir Events sem alterar regras; até dois números por ator, limite global (ex. 10); vida de .65 s em Elapsed, sem replay em reset/terminal, sem crescimento de Nodes. Dano e cura não viram saldo líquido. Ancorar perto do ator com canal separado das placas; pausa congela feedback. Fonte/ícones podem ainda estar sendo escritos pelo outro worker: implementar contra contrato, não editar arquivo dele.

## API de view acordada (worker menus implementa)

Preservar pública Simulation/Stage/Overview/ToggleDock/VerifyLayouts/SelectedMemberIndex/InventoryColumns/MemberSelectorCount/RouteNodeCount. Stage criado, Bind antes AddChild. Só View._Process avança simulação. Usar APIs acima e manter sinais de Pressed funcionando.

Menu com cabeçalho e rodapé fixos e SOMENTE miolo rolável, controles legíveis sem reduzir fonte. Menus independentes não param o jogo; abre/fecha/restaura foco por gesto explícito. Títulos principais Limelight; nome do personagem/trecho Source Sans 3 semibold. Dados reais, cura informativa, escudo estado Pronto/Protegido/Recarga, bolsa 7×3 desabilitada, rota cinco estados reais. Não copiar números/XP/itens das pranchas antigas.

Root mantém palco compacto ~1000×320 e Overview ~1160×880. Não aumentar HUD para fullscreen. Adicionar botão toggle REAL `Detalhes` no toolbar, repassando DetailedHud ao Stage. Seleção Herói repassa SelectedMemberIndex. Pode ter Tooltip que explique. Manter textos dos botões antigos (`Status`, `Herói`, `Expedição`, `CURA`, `DPS`, `TANK`, `Pausar`, `Recuar`, `Reiniciar`, `Escudo de Pressão`) ou nomes estáveis para adaptar QA pelo orquestrador. Não alterar classes de checks.

Escala vem de BrassUi.Scale definido por Main via `--ui-scale=1|1.25|1.5` ANTES de criar view. Aplicar layout nativo compatível com bounds: em telas baixas rolar miolo e manter ações; evitar que escala 1.5 force janela fora da área. Não prometer DPI físico do SO apenas por uma opção interna de escala. GuiEmbedSubwindows só em Overview/headless como antes. Fontes maiores não podem inflar Container mínimo a ponto de empurrar palco ou rodapés fora da janela. Sem novos botões sem efeito.

## Verificação do orquestrador

Build e import com ferramentas portáteis; type proof, UI/glyphs e espaço de tela; rota e callbacks headless; teste gráfico de três janelas nativas; capturas compacto/Overview/chefe; escala 1/1.25/1.5 e bounds representativos (fixtures isolados quando necessário). Test/Test-Integration legados. Inspecionar PNGs, reparar colisões/recortes e repetir. Não declarar cliques físicos, DPI, gamepad, consumo ou aprovação artística se não observados.
