# Fatia jogável — Galerias da Caldeira / Studio Godot

## Direção e escopo

Pedido atual: implementar um protótipo Godot bem-acabado, com caminhada pelo cenário, novos encontros, combate animado e menus derivados dos conceitos. **Brass v12 GPT é a base de trabalho aceita**, não uma aprovação final de todos os detalhes. Paleta quente v10: papel #BFB49E, fuligem #241F1A, tabaco #40332B, vinho #6C3A32, latão #B99B63, linho #E4D8B7, éter #97A581. Sem azul/petróleo dominante.

Referências: `assets/concepts/brass-v12-gpt/generated/01-equipe-r02.png`, `02-bestiario-1.png`, `03-bestiario-2.png`; menus em `assets/concepts/brass-v10/01-menus-cor.png` e `canvas/documents/revisao-latao-v10.md`. Não reutilizar pesquisa de BioShock como asset de runtime.

Fatia local e independente do serviço: cinco trechos (Entrada, Trilhos, Pressão, Ninho, Fornalha), marcha real ao longo de X, câmera acompanhando e cenário contínuo, encontros distintos e chefe, vitória/derrota/recuo/reinício, pausa e proteção manual. Sem economia persistente inventada. O modo M0 `idle`, API, multiplayer e transferência continuam existentes e separados. Não é M1 completo.

Formação da esquerda para direita: Alquimista do Éter (healer NPC), Pistoleiro, Guarda da Caldeira; inimigos à direita. Guarda humano com escudo retangular/faixa ocre e pack plano; Pistoleiro estreito/casaco tabaco/faixa linho/rifle; Alquimista linho e aplicador de uma cápsula. Os desenhos v12 são pinturas, não meshes/rigs. A tradução procedural articulada inicial é asset técnico de protótipo, não modelo final ou pintura substituta.

## Divisão com propriedade exclusiva

- **Regras**: apenas `src/Aetherforge.Domain/Expedition.cs` e `tests/Aetherforge.Checks/ExpeditionChecks.cs`. Sem alterar Party.cs, persistência ou testes existentes.
- **Cena e animação**: apenas `src/Aetherforge.Client/ExpeditionStage.cs`, `ExpeditionActor.cs` e, se necessário, arquivos novos prefixados Expedition no cliente. Não tocar em Main.cs, WorldBuilder.cs, UI ou domínio.
- **Interface**: apenas `src/Aetherforge.Client/ExpeditionView.cs`, `BrassUi.cs`, novos recursos em `src/Aetherforge.Client/Assets/Brass/`. Não tocar em Main.cs, UI antiga, cena ou domínio.
- **Integração e QA**: orquestrador cuida de Main.cs, entrada .tscn, scripts, registro do Studio, chamada dos checks, compilação e revisão conjunta. Não compilar concorrentemente sem coordenar.

## Contrato público de regras (obrigatório)

Namespace `Aetherforge.Domain`, sem Godot, netstandard2.1.

- `enum ExpeditionPhase { March, Combat, Loot, Complete, Defeat, Retreated }`
- `sealed class ExpeditionEnemy`: `string Kind`, `string Name`, `float MaxHealth`, `float Health`. IDs: rat, scavenger, sentry, drone, watcher, foreman. Vida nunca negativa.
- `sealed class ExpeditionEvent`: `int Id`, `string Kind`, `int Actor`, `int Target`, `float Amount`, `double At`, `string Message`. Actor/Target 0=Guarda, 1=Pistoleiro, 2=Alquimista, 3+i=inimigo. Kind attack/hurt/heal/shield/kill/phase. `At` é tempo simulado. Eventos recentes limitados, IDs monotônicos por sessão.
- `sealed class ExpeditionSimulation`:
  - `List<PartyMember> Party { get; }` com os índices e funções do StarterParty existente.
  - `IReadOnlyList<ExpeditionEnemy> Enemies { get; }`
  - `IReadOnlyList<ExpeditionEvent> Events { get; }`
  - `ExpeditionPhase Phase { get; }`
  - `int StageIndex { get; }` zero-based 0..4; `string StageName { get; }`
  - `double Distance { get; }` posição X absoluta do Guarda; aumenta SOMENTE em March; não zerar entre etapas.
  - `double EncounterAt { get; }` posição X do encontro atual/próximo (inimigos visuais em EncounterAt+3.6+i*1.5).
  - `double Elapsed { get; }`, `double PhaseTime { get; }`
  - `int Victories { get; }`
  - `bool Paused { get; set; }`
  - `double ShieldCooldown { get; }`, `double ShieldRemaining { get; }`, `double HealCooldown { get; }`
  - `string LastEvent { get; }`
  - `void Advance(double delta)` — determinístico, passo interno limitado; rejeita delta inválido/negativo; só UI chama.
  - `bool ActivateShield()` — somente durante combate, vivo e fora de cooldown; mitiga dano de verdade.
  - `void Retreat()` — terminal Retreated, sem recompensas, sem avanço oculto.
  - `void Reset()` — nova rota completa.

Rota deve ser concluível em cerca de 100–160 segundos de simulação automática; descanso curto entre etapas é permitido e declarado. Cura automática condicional pelo healer vivo; ataques apenas por vivos; tank recebe prioridade. Chefes e inimigos distintos. Não inventar integração com API.

## Contrato de apresentação (obrigatório)

- `public partial class ExpeditionStage : SubViewportContainer`
  - `public void Bind(ExpeditionSimulation simulation)` chamado antes de AddChild.
  - Constrói SubViewport/Node3D/câmera/luz/cenário/personagens e atualiza poses em `_Process` lendo a simulação, **sem** chamar Advance.
  - Ocupa o tamanho recebido; `Stretch = true`; MouseFilter Ignore. Mundo +Y acima, +X caminho, câmera lateral vendo rostos/escudos em três-quartos. Projeção ortográfica. Corredor com profundidade, pilares, trilhos, tubos, placas, iluminação quente e variantes por trecho. Movimento real e reciclagem de módulos sem saltos na câmera.
  - Party roots: Guarda em `(Distance,0,0)`, Pistoleiro `(Distance-1.65,0,.12)`, Alquimista `(Distance-3.3,0,.24)`. Inimigos em EncounterAt+3.6+i*1.5. Animações de marcha, espera, ataque, impacto, cura e queda; projéteis e golpes visíveis correspondem aos eventos.
  - Recursos procedurais devem ser reutilizados; não reconstruir meshes/materiais a cada frame. Sem sombras excessivas/pós-processamento pesado; alvo 30 FPS, não validado até medição.
- `public partial class ExpeditionView : Control`
  - `public ExpeditionSimulation Simulation { get; }`
  - `public ExpeditionStage Stage { get; }` após Ready
  - `public bool Overview { get; set; }` antes de Ready; modo de revisão dentro de uma janela para screenshot/Studio.
  - `public void ToggleDock()`
  - `public System.Threading.Tasks.Task VerifyLayouts()` testa componentes/colunas/controles, sem assumir transparência em headless.
  - `_Process` é o único dono de Simulation.Advance(delta). Não processar em dobro.
  - Padrão: palco compacto ~1000x320 junto à barra e janelas nativas independentes Status/Herói/Expedição (abrir/fechar sem parar simulação). Em Overview, mesmas três janelas embutidas numa área ~1180x900 com palco menor abaixo; não HUD MMO fullscreen. Adaptar ao usable rect sem cair fora da tela.
  - Inventário sete colunas, três seletores de integrante, cinco etapas e estado atual real. Retrato derivado da pintura própria v12, não usar sprites KayKit antigos como nova arte. Itens/slots sem implementação devem estar explicitamente indisponíveis, não apresentar economia falsa.
  - Fonte Limelight/Jost reais em `docs/visual/assets/fonts/v5`, copiar licenças. Estética de instrumentos/tabaco/vinho/linho/latão pontual, Arco Partido autoral, ícones desenhados (sem emoji genérico). Sem skin azul antiga.
  - Pausa, escudo de pressão, recuar e reiniciar funcionam de verdade; mostrar atalho/botões. Mineração/oficina/MMO não implementados nesta fatia: não criar botões de mentira.

## Operação dos subagentes

Workers via `codex exec --sandbox workspace-write`, sem desativar sandbox, sem alterar configuração global, sem commit. Ler contratos e fontes atuais antes de escrever. Propriedade exclusiva evita sobreposição. Registrar retorno em `artifacts/studio-godot/agents/*-last.md`; reportar arquivos, decisões, verificações e limitações. Se um contrato não servir, relatar ao orquestrador em vez de modificar outra frente.
