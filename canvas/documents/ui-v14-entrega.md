# V14 — fontes, menus e HUD implementados

**Estado: implementado e verificado no protótipo Godot; acabamento sujeito à revisão visual do usuário.** O usuário autorizou seguir as recomendações e implementar o [plano V14](ui-v14-fontes-menus-hud.md). A base de personagens Brass v12 e a paleta quente v10 permanecem; esta entrega não é uma nova direção de arte nem o M1 completo.

## O que o jogador vê

- **Limelight** na marca e títulos principais. **Source Sans 3 400/600 reais** em informação, botões, nomes e números; valores de Status em 28, nomes de integrantes 18–20, corpo 14, metadados 13, números de combate 18 a 100%. Fontes locais, licenciadas e cacheadas, sem instalação global.
- **Status / Mesa:** topo reto, placas claras de Vida/Ataque/Defesa, escudo com destaque vinho e estado real `Pronto`, `Protegido` ou `Recarga`.
- **Herói / Estojo:** coroa em arco, retrato v12, três seletores com ícones, bolsa de sete colunas/21 espaços indisponíveis. A seleção também identifica o integrante no palco.
- **Expedição / Telégrafo:** topo escalonado, cinco trechos com forma e texto para estado, bilhete atual e journal limitado aos acontecimentos relevantes.
- Cabeçalhos e ações **fixos**, somente o miolo rolável. `Recuar` fica separado da pausa. Fechar um instrumento não pausa a rota; foco visível e retorno ao controle anterior.
- **HUD conciso:** vida com trilho e preenchimento de 52 × 7 de base, ícones de função e nomes condicionais sobre placas opacas. Se os três indicadores não couberem sobre a formação, entram numa faixa compacta de equipe, na ordem Alquimista/Pistoleiro/Guarda, sem esconder a vida de um aliado para ganhar espaço.
- Identificação selecionada pode ficar abaixo dos pés quando não há espaço acima. Em escala alta/área apertada, textos secundários cedem antes das barras; os valores completos continuam nos instrumentos. `Detalhes` alterna a apresentação de verdade, sem modificar combate ou alvos.
- **Chefe** com nome/vida num componente dedicado durante combate. Números de dano/cura em espaço de tela, semibold, sinais `−`/`+` e marcas distintas além da cor; sem duplicação por `Label3D`.
- Seis SVGs originais: escudo, rifle, aplicador, pausa, retorno e rota. Latão em detalhes, vinho em seleção/ação e placas claras para leitura — não bordas douradas em todos os controles.

## Capturas reais

São viewports do Godot, não mockups, novas pinturas, texturas de runtime ou prova de arte 3D final. Cópias byte a byte, sem corte/upscale; dimensões e SHA-256 no [manifesto](../../assets/concepts/brass-v14-ui/provenance.json).

| Captura | O que verificar |
|---|---|
| [Prova de leitura](../../assets/concepts/brass-v14-ui/typography.png) | Jost e Source Sans 3 com textos iguais, 13/14/18/26 e pesos 400/600; valores demonstrativos identificados como espécime, não sessão |
| [Palco compacto](../../assets/concepts/brass-v14-ui/compact.png) | 1000 × 320, modo normal; vida dos três e identificação selecionada |
| [Combate / Overview](../../assets/concepts/brass-v14-ui/combat.png) | 1160 × 880, hierarquia dos três instrumentos e feedback real |
| [Chefe / Overview](../../assets/concepts/brass-v14-ui/boss.png) | Componente dedicado do Capataz; compare com a [captura V13](../../assets/concepts/brass-v13-godot/boss.png) |
| [Status nativo](../../assets/concepts/brass-v14-ui/status.png) | Vida/ataque/defesa e ações fixas; escudo ativo durante pausa |
| [Herói nativo](../../assets/concepts/brass-v14-ui/hero.png) | Retrato v12 e seleção fixa; bolsa no conteúdo rolável |
| [Expedição nativa](../../assets/concepts/brass-v14-ui/route.png) | Cinco estados reais, rolagem do conteúdo e ações fixas |
| [Palco a 150%](../../assets/concepts/brass-v14-ui/compact-150.png) | Janela continua 1000 × 320; barra de vida não é sacrificada para caber a fonte maior |
| [Status a 150%](../../assets/concepts/brass-v14-ui/status-150.png) | 570 × 664 na área útil desta máquina; cabeçalho/ações acessíveis e corpo rolável |

O Overview é um arranjo para revisão, **não** o layout normal do desktop. Com texto a 150% ele prioriza ações e rolagem, mostrando menos conteúdo por vez. A prova A/B é um espécime tipográfico nativo, não duas skins completas do mesmo combate. A comparação V13/V14 inclui mudanças de fonte, HUD e layout, não isola apenas uma variável.

## Plano executado e decisões de integração

| Etapa | Resultado |
|---|---|
| P1 — prova | Comparação nativa Jost/Source Sans 3 e capturas antes/depois. Aplicada a família recomendada; não é declaração de aprovação artística final |
| P2 — kit | Fontes/cores/espaçamento/escala, estados, foco por contorno, ícones e três chassis |
| P3 — HUD | Projeção, colisões/bordas/cabeças, faixa compacta de segurança, chefe e pool de números |
| P4 — instrumentos | Três janelas nativas, conteúdo rolável e cabeçalhos/rodapés fixos |
| P5 — QA | Build, entrada, três escalas, fixtures de bounds, capturas, rota e regressões. DPI físico, gamepad e benchmark continuam pendentes |

### Correções encontradas executando, não só compilando

1. O arquivo Source Sans 3 tem peso-base **200**. Além disso, nesta integração a chave textual `wght` ficava no objeto C#, mas não chegava ao cache nativo. A solução usa a tag numérica OpenType `0x77676874`. O QA consulta os RIDs no TextServer e exige coordenadas efetivas **400/600**, sem emboldening sintético. O nome interno `Source Sans 3 ExtraLight` é metadado da face-base, não o peso usado no desenho.
2. Jost solicita `tnum` por tag numérica; Source Sans 3 já tem avanços tabulares por padrão. As larguras de `111111`, `888888` e `000000` são comparadas no Godot.
3. O fechar foi para a linha superior, liberando toda a placa para o título em escala alta. Nas janelas nativas, a altura respeita a faixa do palco quando há espaço para cabeçalho, ações e conteúdo legível; não exige fonte menor. Em desktops baixos, janelas continuam independentes e podem se sobrepor — não há promessa de expor três painéis altos e palco ao mesmo tempo.
4. O HUD passou a ter fallback de faixa de equipe para não perder um indicador de vida quando placas/atores disputam espaço. Números e nomes cedem espaço a rostos, bordas e identificação relevante.
5. Atalhos dos instrumentos passam por um nó de entrada do próprio viewport; não dependem só do sinal nativo `WindowInput`. Os testes cobrem janela embutida e nativa.

Transições ornamentais de 120–160 ms e uma nova opção de redução de movimento/flash **não** foram introduzidas. Os estados de menu são imediatos; o feedback de combate preexistente permanece. Preferiu-se consolidar leitura, layout e ações reais antes de acrescentar animações decorativas.

## Verificação executada

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-UiV14.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Expedition.ps1 -Screenshots
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Integration.ps1
```

Todos passaram na rodada final:

- Builds cliente/API/checks com **0 avisos / 0 erros**, importação Godot concluída.
- Entrada/layout headless em **100/125/150%** e teste gráfico das **três janelas nativas distintas (IDs 1/2/3)** nas mesmas escalas.
- Mouse nativo: automação Win32, com posição do cursor restaurada ao final; teclado: eventos `Viewport.PushInput` no Godot. Headless usa eventos de mouse/teclado no viewport raiz e encaminhamento a janelas embutidas. **Não são apenas chamadas `Pressed`, mas também não são um playtest humano de teclado/mouse físico.** O teste gráfico movimenta temporariamente o cursor e muda foco das janelas do jogo.
- Toggle Detalhes, três seletores, Enter/Tab, pausa/retomada, Escape/abrir, escudo com P, recuo e reinício. Pausa conserva tempo/distância/feedback; reset remove números antigos. Três vidas verificadas visíveis na apresentação compacta.
- Fixtures da **área útil** 1366 × 728, 1920 × 1040 e 2560 × 1400, combinadas com as três escalas. Usam o mesmo cálculo de layout e verificam cabeçalhos/rodapés/miolo; **não simulam o DPI do Windows nem substituem os três monitores físicos**.
- Quinze execuções na suíte V14: três headless, três nativas e nove capturas. Seus quinze `.err.log` finais estão vazios. Os logs gerais, JSONs e capturas adicionais estão em `artifacts/ui-v14/`; diagnósticos intermediários permanecem como histórico, fora das referências selecionadas.
- Rota completa preservada: **137,45 s simulados**, **5 vitórias**, seis tipos, **108 unidades** de marcha/câmera, sem deslocamento em combate; HP final **108/180**, **52/100**, **110/110**.
- HUD na rota: dano **e** cura visíveis; pico observado de **4 números ativos**, no máximo **2 por ator**. Capacidade fixa 10, vida .65 s no tempo da simulação; não é benchmark de GPU/CPU ou prova de ausência de qualquer colisão perceptual em todas as situações.
- Legados: API, dois clientes de rede, colisão, transferência do mesmo item, defesa 13, dano 17 e persistência após reinício. Saves de teste isolados em `artifacts/`; nenhum save manual editado.
- Hashes de `Expedition.cs` e `ExpeditionActor.cs` iguais aos da entrada desta implementação. Não foram alterados regras, progressão, atores, API, persistência ou multiplayer para estilizar a UI.

Evidências locais: `artifacts/ui-v14/{build-final.log,test-ui-final.log,test-expedition.log,test-legacy.log,test-integration.log,final-audit.json}` e `artifacts/studio-godot/qa/route.json`.

## Como abrir

- **Jogar Expedicao.cmd:** palco compacto e instrumentos por botão.
- **Revisar Expedicao.cmd:** Overview, somente para revisão.
- **Prova de Fontes.cmd:** comparação tipográfica nativa.
- Fonte maior, sem alterar configuração global do Windows:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Run.ps1 -Mode Expedition -UiScale 1.25
# Ou -UiScale 1.5; -HudDetails inicia o HUD detalhado.
```

Espaço pausa/continua; P escudo; R reinicia; Escape fecha o instrumento ou sai quando não consumido. M exige foco no palco para o áudio, como antes. Abrir/fechar instrumentos não pausa. A rota continua local/finita, sem equipamentos, XP ou recompensas persistentes inventados.

## Organização e limites

Contrato e caminhos de código: [UI-V14-IMPLEMENTACAO](../../docs/studio-godot/UI-V14-IMPLEMENTACAO.md). Três workers reais com arquivos exclusivos (tema/HUD/menus) concluíram em `workspace-write --ephemeral`, sem build concorrente ou commits. Prompts em `docs/studio-godot/tasks/ui-v14-*.txt`, eventos/retornos em `artifacts/ui-v14/agents/`. O orquestrador integrou e corrigiu fontes, entrada/layout nativo, fallback do HUD e verificações. `Start-UiV14Workers.ps1` não é comando de jogar: relança agentes e consome uso.

Permanecem sem verificação: DPI físico 125/150% do SO, múltiplos monitores, teclado físico/humano, gamepad, outras GPUs/máquinas, áudio perceptivo e benchmark. Tamanhos maiores exibem menos texto por vez; nomes opcionais podem ceder por falta de espaço. Não há certificação de acessibilidade. Modelos/cenário continuam técnicos; rigs/texturas/ambientes finais e M1 completo não foram entregues por esta revisão de UI.

Nenhuma nova pintura/chamada a gerador de imagens, importação de arte de pesquisa ou migração/validação nova do cadastro Studio ocorreu. Fonte e ícones: [proveniência de runtime](../../src/Aetherforge.Client/Assets/Brass/PROVENANCE.md). A aceitação visual final desta V14 ainda cabe ao usuário.
