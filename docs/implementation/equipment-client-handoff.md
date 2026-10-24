# Integração cliente de equipamento — GAMELAB-8

Herói e Status apresentam os três slots do integrante selecionado e a mesma comparação do domínio. Inspecionar preserva o estado; equipar/desequipar exige ação explícita. A bolsa atualiza por `InventoryRevision`, incluindo substituição 21→21 e recuperação FIFO. Os atributos exibidos em Status e nas composições vêm de `ReadView().Members[].Stats`, usados pelo combate e pela cura.

## Revisões e integração

- Main reconfirmada: `aca2d622ae590e0044a8e744f392fd6ed787161d`.
- Domínio: `c8fcff01f5c15390c2c190108cee73c888c753d8`, PR #2, preservado integralmente por fast-forward, incluindo contratos GAMELAB-3/5.
- Especificação UI: cherry-pick de `46878bd4ebb159f5627b0a3d5ca32e0a1a6598da`, preservando autoria, gerou `282ebeb`. Esta é a revisão reunida antes das mudanças no cliente.
- Branch de entrega: `agent/gameplay-engineer/456de55360c9`. O manifesto anexado à issue registra o SHA final e os resultados da execução nessa revisão.
- PR de integração tem base `agent/gameplay-engineer/e200c54a9265` para mostrar somente a especificação UI e o cliente. Integrar/revisar PR #2 primeiro; depois atualizar a base deste PR para main, preservando a ancestralidade. Não reaplicar o domínio por cherry-picks nem fazer squash que duplique os commits compartilhados sem resolver essa base.

## Fontes e garantias

| Fonte | Responsabilidade |
|---|---|
| `src/Aetherforge.Client/EquipmentIntent.cs` | SelectionEpoch, intenção/prévia/envelope imutáveis, consumo único e deduplicação de feedback |
| `src/Aetherforge.Client/ExpeditionView.Equipment.cs` | Páginas internas de inspeção/comparação/resultado, nove slots, comandos e seleção compartilhada, motivos e saída permanente de gerenciamento |
| `src/Aetherforge.Client/EquipmentPresentation.cs` | Formatação PT-BR, totais/deltas/composição, HP/corte/caído, infusão/DPS, motivos e estilos locais com contraste |
| `src/Aetherforge.Client/ExpeditionLootView.cs` | Grade por revisão e identidade, inspeção, foco/vizinhos e reflow de 21 células |
| `src/Aetherforge.Client/ExpeditionStatusPanel.cs` | HP e atributos efetivos, composição sem dupla contagem da base e seleção compartilhada |
| `src/Aetherforge.Client/EquipmentUiCheck.cs` | Fixtures isoladas e gestos por Viewport.PushInput, geometria/contraste, capturas e relatório JSON |

Mudar alvo/peça/contexto, voltar, cancelar, fechar e reiniciar invalida a intenção, mesmo na seleção A→B→A. Confirmar envia o envelope da prévia apresentada; o domínio continua validando revisão, autorização, propriedade e capacidade. O gesto guarda a intenção ao pressionar o botão, impedindo que soltar um gesto antigo consuma uma prévia substituída. Resultados leem `LatestReadModel` e não provocam outro Equip no redraw. Rejeição, cancelamento, NoChange e Replay não geram feedback aplicado.

Pausa, pedido, cancelamento, conclusão, recuo e reinício passam por `RouteCommand`. Active bloqueia pausa genérica; fechar janelas conserva o estado e deixa Concluir/Abrir Herói no HUD. Nenhum cálculo local de catch-up, concessão, HP, XP ou inventário integra o fluxo de produção. Reflexão e adaptação de HP/XP aparecem somente no harness opt-in de teste.

Cabeçalho e ações ficam fora da rolagem. Em área curta a comparação substitui o retrato/bolsa por alvo compacto; ações usam linha compacta e o corpo mantém pelo menos 40 unidades lógicas. As janelas de revisão podem usar mais altura que a reserva visual original do palco para manter ações legíveis. A grade reflui quando sete colunas não cabem; a ordem e as 21 posições permanecem. Escape consome uma camada; Tab sai da grade. O aviso de perda ao fechar permanece explícito; nenhuma mensagem anuncia salvamento.

## Validação reproduzível

Na raiz, com ferramentas fixadas pelo projeto:

```powershell
./scripts/Setup.ps1 -SkipAssets
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Test-Integration.ps1
./scripts/Test-Expedition.ps1
./scripts/Test-UiV19.ps1
./scripts/Test-EquipmentUi.ps1 -Size 1920x1080 -Scale 1 -Screenshots
./scripts/Test-EquipmentUi.ps1 -Size 1366x900 -Scale 1.5 -Screenshots
./scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Screenshots -Native
./scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Reduced
```

Repetir os dois viewports a 1/1.25/1.5. Cada caso também limita os três instrumentos às áreas úteis virtuais 1366×728, 1920×1040 e 2560×1400. O script cria JSON, logs e capturas em artifacts/equipment-ui; esses arquivos são anexados pelo fluxo Multica, não versionados. `-Native` exige `-Screenshots` para usar janelas reais, pois o backend headless sempre incorpora sub-janelas. Capturas nativas incluem cada instrumento separadamente.

| Casos | Evidência automatizada e fronteira |
|---|---|
| E01–E04 / U01–U02 | Defesa 10→12, remoção −2, armas 18→20→20→21, perda −1 explicitamente rotulada; troca entre IDs iguais em atributos. Oráculo raro→comum −2 também no domínio |
| E05/E13/E14 / U03/U08/U09 | Alvo compartilhado incompatível, ida e volta, callback antigo injetado, cancelamento com checkpoint igual, mudança de fase/revisão/tentativa; input por mouse/Enter/Escape |
| E06 / U04 | Prévia e potência real de infusão 42→44; os oráculos Nv2/limites de cura são cobertos no domínio |
| E07–E09 / U05 | Máximo +22 sem cura, 202→190, retorno 190/202, aviso de caído; precisão de HP abaixo de 0,01 tratada pelo formatador. Caído/derrota e HP fracionário têm oráculos adicionais no domínio |
| E10–E12 / U06–U07/U16 | Bolsa cheia, conteúdo 21→21, remoção bloqueada, vaga com FIFO e recibo de recuperados; remoção com 19/20 e conservação de IDs também no domínio |
| E15 / U10 | Vitória real com limiar de nível preparado no harness e nova prévia após crédito/nível; cálculo exato 118/198→118/208 coberto pelo domínio |
| F01–F04/F06 / U09/U11–U12 | Marcha sem gerenciamento, pedido/cancelamento em combate pausado, retomada explícita e parada na primeira vitória com 2 s íntegros. Delta 1000 e equivalência particionada no domínio |
| F05/F07 | Preservação de 0,75 s, recargas e ausência de cura extra nos checks de domínio; cliente usa os mesmos comandos e não avança relógio por troca |
| F08–F10 / U13–U14 | Recuo/reinício por input, loadout conservado, intenção antiga invalidada, pausa prévia preservada e saída no HUD com instrumentos fechados. Complete/Defeat/ResetSession também no domínio; sem botão novo de reset |
| U15–U17 | Tab/Enter/Escape, grade/foco por ID, cabeçalho/rodapé contidos e corpo mínimo, três áreas virtuais, topo/meio/fim da comparação e Confirm focado. Contraste calculado dos estilos opacos de cada estado dos controles novos |
| U18 | Onze transações de equipamento geram onze feedbacks; Replay não repete. Cancelamento/redraw/recusas não contam. Aviso Volatile antes/depois; redução de movimento não remove informação |

O check V19 anterior passou a verificar alcance integral da descrição pela rolagem e ausência de corte horizontal, pois o novo botão Comparar recebe o foco de inspeção e corpos curtos não comportam toda a descrição e ação simultaneamente. Não se exige encolher fonte para caber num único viewport. O script V19 também cria seu diretório quando chamado com ProgressionOnly.

## Revisão humana e limites

No smoke, pedir a uma pessoa para obter uma peça, identificar alvo/função, prever o delta, cancelar e depois equipar; conferir o Status e o próximo combate/cura. Repetir com peça pior, bolsa cheia, pedido em combate pausado e gerenciamento com todas as janelas fechadas. Em janela nativa, verificar foco de retorno entre Status/Herói/Expedição, teclado físico, mouse físico e área útil do monitor. Os testes automatizados usam input sintetizado; não certificam compreensão, DPI físico, multimonitor, acessibilidade assistiva, gamepad ou toque. Contraste de tokens não equivale a auditoria manual de todos os pixels.

O domínio mantém as 254 asserções E/F recebidas, reexecutadas nesta integração. Isso não substitui QA independente nem revisão do Technical Lead/Director. Save/codec/recuperação/offline continuam fora desta entrega. Nenhum save manual em data/ é usado. Há aviso NU1900 quando o serviço de auditoria de pacotes NuGet está indisponível; isso deve constar no manifesto de build, sem ser descrito como build sem avisos.
