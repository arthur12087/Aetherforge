## Diagnóstico QA-N01 — GAMELAB-22

A lacuna comprovada no harness era enviar um clique Win32, aguardar um número fixo de frames e prosseguir sem verificar sua entrega. Agora ele verifica janela atingida, hover e geometria estável antes de enviar **um único** down/up, e exige recibos dos eventos reais no botão e exatamente uma ativação. Falhas deixam JSON `passed=false`, trace e capturas, além do stderr. O caminho de mouse continua Win32; teclado continua `Viewport.PushInput`. Regras, apresentação e saves de produção não foram alterados.

Isso melhora a localização da falha e a sincronização do teste. **Não determina retrospectivamente a causa das duas tentativas de QA**, nem encerra o gate QA-N01. A revisão independente deve ser encaminhada pelo Director.

### Revisões e entradas

Partida: `4cc69d0eb4b1c33b96cfef4b4f57a2d4e1c4bd23`, head do PR #3, branch `agent/gameplay-engineer/456de55360c9`. Checkout Multica desta investigação: `agent/gameplay-engineer/ec9591c60121`. Domínio `c8fcff01f5c15390c2c190108cee73c888c753d8` e incorporação UI `282ebeb6f45f23612f34e33d9b1eebacae6d7ddb` são ancestrais. PR #3 permanecia aberto, base `agent/gameplay-engineer/e200c54a9265`/PR #2. Esta correção é empilhada sobre a branch do PR #3, sem retarget de PR existente.

Lidos contratos E/F, arquitetura, U01–U18/§12 e handoffs. Inspecionados matriz atual de QA, logs nativos e capturas parciais do ZIP autenticado `01a16e5a-b306-7372-a653-f232de9ef3e6`, em especial `native-repeat/*/applied-Instrument1.png`. A imagem confirma defesa 12 e resultado aplicado, mas antecede Escape/clique no slot; não mostra o gesto que falhou.

### Cada falha e o que a evidência permite afirmar

| Ponto de QA | Experimento dirigido, antes dos recibos | Conclusão e limite |
|---|---|---|
| `initial fixture pause before combat` | Omitir somente o clique de pausa reproduziu a mensagem com `March/Paused=false`. Esperar o combate por ticks naturais e então entregar o clique reproduziu a mesma mensagem com `Combat/Paused=true`. | O assert combina dois requisitos. O log histórico não registrou qual deles falhou; não permite escolher entre perda do gesto e chegada tardia. Ambos permanecem hipóteses para a tentativa histórica. |
| `enabled visible input PreviewUnequip1` | Após equipar por Enter e voltar, omitir somente o clique em `EquipmentSlot1-Armor` reproduziu a mensagem, com página normal (`page=-1`) e remoção invisível. | A indisponibilidade é esperada quando o slot ainda não foi aberto. A tentativa histórica não tem captura após esse clique nem recibo dele; isso impede distinguir clique perdido, posição/foco/rolagem e eventual defeito de abertura. Não há defeito de produto confirmado. |

Os controles são injeções explícitas no harness opt-in, não reprodução espontânea dos defeitos históricos. Nenhuma falha foi descartada até aparecer um passe. O primeiro caso instrumentado, sem mudança no ritmo dos gestos originais, já completou 415 asserções. Foram usados os mesmos Godot 4.7.2/.NET 10.0.401 e Windows/OpenGL Compatibility/RTX 4070 SUPER dos logs de QA, escala interna 150%, raiz nativa efetiva 1000×320.

Observação concreta de foco: no primeiro passe, em `RoutePause`, o Godot já informava foco/hover corretos enquanto `GetForegroundWindow` ainda diferia de `WindowFromPoint`. Ao receber o down, o Windows ativou a janela atingida e o botão recebeu down/up. Um controle inicial de desenvolvimento que exigia igualdade de foreground **antes** do clique recusou indevidamente essa situação em `receipt-01-native`. Foi corrigido: foreground é diagnóstico; a prontidão exige hit-test da janela, hover do botão e centro estável, e a entrega é comprovada pelos recibos. Não se adicionou clique de ativação nem repetição do gesto.

### Mudança delimitada

- `EquipmentUiCheck.cs`: trace com tempo/frame, fase/pausa/Elapsed, página, rect, janela/posição/tamanho, foco/hover, cursor e HWND de foreground/hit-test; relatório e capturas em falha.
- Preparação do ponteiro limitada a 1,5 s, sem enviar eventos durante a espera. Após um único down, espera até 1 s pelo recebimento; solta em `finally`; exige up e uma ativação. Um gesto não entregue falha no botão de origem, antes de acessar outro controle. Os timeouts não transformam falhas em sucesso.
- O assert original de pausa em marcha e os oráculos de transação/HP/foco/visibilidade continuam. Há uma checagem adicional da página de remoção imediatamente após abrir o slot.
- Cinco gestos completos, derivados da fixture independente de QA: ABA por Status; Escape/reabrir; página de Status substituindo Herói; revisão de HP em fixture; concluir/reativar gerenciamento. Cada um verifica botão realmente pressionado e checkpoint/feedback inalterados após a soltura obsoleta. Reflexão/adaptação de HP continuam restritas ao teste opt-in.
- `Test-EquipmentUi.ps1 -InputFault`: `drop-pause` e `drop-slot` omitem um único down; `late-pause` espera combate natural. Uso diagnóstico explícito, com saída separada. Esses casos devem retornar exit 1, nunca passe.

Não há pausa direta da simulação, callback substituindo input nativo, ajuste de regra E/F/U, nova arte, save/offline ou edição do checkout/saves de QA.

### Inventário completo de tentativas gráficas nesta investigação

Saídas relativas a `artifacts/gamelab-22/` no pacote da issue. Todos os nove ensaios nativos foram sequenciais; consultas de runs irmãos mostraram apenas esta task ativa. Não foi iniciado outro ensaio gráfico durante um ensaio nativo.

| Saída | Revisão de harness | Resultado |
|---|---|---|
| `diagnostic-01` | head original + telemetria | Passe completo, 415 asserções; sem injeção. |
| `diagnostic-02-drop-pause` | telemetria + controle | Exit 1 esperado: assert original da pausa, March/não pausado. |
| `diagnostic-03-drop-slot` | mesma | Exit 1 esperado: `PreviewUnequip1` invisível; slot não ativado. |
| `diagnostic-04-late-pause` | mesma | Exit 1 esperado: assert original da pausa, Combat/pausado; clique entregue. |
| `receipt-01-native` | recibos + cinco gestos; precondição de foreground inadequada | Exit 1 em prontidão RoutePause; documentado acima, sem outro gesto enviado. |
| `receipt-02-native` | harness final | Passe completo, 728 asserções observadas; cinco gestos, remoção pelos dois instrumentos, Tab/Enter/Escape e fronteiras existentes. |
| `control-05-drop-pause` | final | Exit 1 esperado: `native mouse-down delivered RoutePause`. |
| `control-06-drop-slot` | final | Exit 1 esperado: `native mouse-down delivered EquipmentSlot1-Armor`. |
| `control-07-late-pause` | final | Exit 1 esperado no assert original; recibos down/up/ativação presentes. |

Dois passes completos, seis controles negativos esperados e uma recusa de desenvolvimento. Nenhum deles é input humano. `regression-embedded`: passe headless 1366×900/150%, 451 asserções e os mesmos cinco gestos. A contagem nativa inclui checagens por iteração de prontidão; não é um contrato de contagem fixa.

Build inicial restrito falhou na API; Build autorizado completou os três projetos e import Godot. Os rebuilds locais do cliente para cada estágio passaram. Persiste NU1900 (consulta de vulnerabilidades NuGet indisponível). `Test.ps1` passou, incluindo 254 asserções de equipamento, legado/persistência, API e dois clientes de rede. `Test-Integration.ps1` passou: batalha real no serviço, transferência do mesmo item, defesa/dano 17 e persistência após reinício em arquivo isolado. Logs completos constam no manifesto final anexado. Não foi reexecutada toda a matriz de escalas: somente os casos afetados e regressões pertinentes.

### Roteiro para QA independente

Fazer checkout Multica do head final indicado no comentário/manifesto da entrega; conferir ancestralidade e hashes dos dois arquivos de harness. Em desktop disponível, consultar `multica issue runs <issue> --siblings --output json` antes de iniciar. Após Setup/Build, usar uma pasta nova por comando:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Screenshots -Native -OutputDirectory artifacts/qa-n01/native
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Screenshots -Native -InputFault drop-pause -OutputDirectory artifacts/qa-n01/drop-pause
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Screenshots -Native -InputFault drop-slot -OutputDirectory artifacts/qa-n01/drop-slot
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Scale 1.5 -Screenshots -Native -InputFault late-pause -OutputDirectory artifacts/qa-n01/late-pause
```

O positivo precisa exit 0 e JSON `passed=true`. Nos negativos, exigir exit 1, JSON `passed=false` e motivo específico da tabela; uma falha diferente é inconclusiva. Se o positivo falhar, conservar **essa** tentativa: trace diferencia prontidão, down, up/ativação, página incorreta e pausa fora de marcha. Reconferir remoção em Herói/Status e os cinco itens `press-context-release` no JSON. Não repetir até um passe nem usar callback para contornar a falha.

Continuam sem certificação: atribuição causal retrospectiva dos dois erros antigos, input humano, DPI/multimonitor, tecnologias assistivas e compreensão. Um passe automatizado e os controles deste PR não substituem aceite independente nem encerramento pelo Director. Save V19 permanece Volatile; GAMELAB-10 e dependências não foram alteradas.
