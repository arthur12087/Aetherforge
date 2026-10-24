# Domínio de equipamento V19 — entrega GAMELAB-7

Implementação sobre `947298b199cd71d0ed7f543cfbbbc7cfe9578819`, que incorpora o contrato GAMELAB-3 em `0686d28` e a arquitetura GAMELAB-5. `origin/main` foi reconfirmado em `aca2d622ae590e0044a8e744f392fd6ed787161d` durante esta entrega. Os dois contratos aprovados foram preservados sem edição. Esta revisão não integra a UI nem grava sessões em disco.

## API concreta para GAMELAB-8

`ExpeditionSimulation` continua independente de Godot e compatível com netstandard2.1. A formação fixa identifica integrantes como `tank`, `dps`, `healer`. `ReadView()` devolve cópia imutável com revisão de estado/inventário, integrantes, slots, bolsa/pendentes, atributos, HP, carteira e estado de gerenciamento. Peças e loadouts são imutáveis; a identidade da peça é `(SessionId, Id)`, sem conversão para GUID legado.

```csharp
var view = simulation.ReadView();
var query = new ComparisonQuery(view.SessionId, view.AttemptId, "dps",
    LootSlot.Weapon, EquipmentAction.Equip, inspectedItemId);
var preview = simulation.Compare(query);
// Guardar esta prévia na intenção ativa; não recriá-la automaticamente ao confirmar.
if (preview.CanConfirm)
{
    var command = preview.Confirm(Guid.NewGuid().ToString("N"));
    var result = simulation.Execute(command);
    // Redesenhar com result.LatestReadModel; cue só para Outcome == Applied.
}
```

Para retirar uma peça, consultar `EquipmentAction.Unequip` com `CandidateItemId=null`. A prévia captura o ID equipado esperado, inclusive null explícito, e a revisão. `Confirm(operationId)` produz envelope imutável; retry de transporte reutiliza exatamente esse envelope. `NoChange`, `Rejected` e `Replay` não geram movimento, revisão, drenagem FIFO ou novo recibo. `Replay.Original` contém o recibo original, enquanto `LatestReadModel` reflete o presente. IDs distintos com os mesmos atributos são uma troca aplicada. A UI decide texto/feedback por `ExpeditionCode` e não por exceções comuns de regra.

`EffectiveStats` expõe bases, valores após nível, bônus e totais, incluindo infusão. A prévia oferece HP antes/depois, deltas de ataque/defesa/máximo e DPS teórico. Combate, descanso, HP por nível e comparação usam o mesmo cálculo. Aumento de máximo não cura; redução limita HP absoluto, preservando frações; caídos continuam em zero.

Comandos de rota usam `new RouteCommand(operationId, sessionId, attemptId, action, paused)` e `Execute`. Ações: `RequestManagement`, `CancelManagementRequest`, `FinishManagement`, `SetPaused`, `Retreat`, `RestartRoute`, `ResetSession`. Em Combat, pedido somente solicita; March/Loot ativam imediatamente; terminais permitem equipamento sem gerenciamento. Pausa em Combat não permite trocar. `FinishManagement` preserva pausa e tempos. Reinício conserva sessão/ganhos/peças/loadouts/recibos/RNG e incrementa tentativa; reset cria sessão nova e limpa tudo. Retry de reinício é deduplicado; retry de reset com sessão antiga recebe `SessionMismatch`.

`Advance(double)` mantém a assinatura void existente. `AdvanceDetailed(double)` retorna tempo consumido/descartado, motivo da parada, revisão e eventos novos. A parada solicitada ocorre dentro da primeira vitória não final, após crédito e nível, com descanso inteiro de 2 s e gerenciamento ativo. Tempo excedente é descartado; não há catch-up. Terminais limpam pedido. Equipar/concluir gerenciamento não reinicializam recargas ou relógios.

O controlador da UI ainda precisa implementar `SelectionEpoch`/intenção ativa e invalidar callbacks ao trocar seleção, cancelar, fechar, voltar ou mudar de peça, inclusive ao voltar ao alvo anterior. O domínio não conhece foco/janelas. Consultas não reservam itens nem pausam; após ativar gerenciamento é necessária uma prévia atual. Atualizar por `InventoryRevision`, inclusive quando uma troca mantém 21 itens. Manter saída de gerenciamento acessível ao fechar a janela. E13 e a parte visual de F10 dependem dessa integração futura.

## Estado e checkpoint desta etapa

O cadastro de peças e suas localizações pertencem a `ExpeditionRewards.Items`, componente interno único: bolsa ordenada, fila pendente e nove slots. Projeções públicas não são editáveis. A substituição devolve a antiga ao índice do candidato antes de recuperar pendentes; retirada exige vaga prévia e anexa a antiga antes da FIFO. Recibos históricos referenciam a mesma instância imutável cadastrada.

Comandos e passos do scheduler preparam cópias, validam e publicam sob uma guarda contra reentrada. Vitória é uma unidade com XP/HP de nível, recursos, RNG, peça, crédito, recibo e fase de saída; overflow descarta o candidato inteiro. O port é chamado antes da publicação dos comandos e nas fronteiras de fase/vitória, não a cada subpasso ordinário. Não há eventos/callbacks externos durante a publicação; o cliente processa comandos e ticks em ordem na mesma thread.

`CaptureCheckpoint()` produz checkpoint destacado **em memória** com identidade/revisões, propriedade, membros/XP/HP, ledger, créditos, seed/estado RNG/contador de itens, recibos de operações, inimigos, eventos e deadlines exatos. `RestoreValidated(snapshot)` aceita checkpoints produzidos pelo domínio e não chama concessão, cura ou reinício. A retomada intermediária de combate é verificada nos checks. A construção a partir de dados externos e suas validações de schema ficam para GAMELAB-11.

**Traduções de assinatura a considerar antes da integração cliente:** `AdvanceDetailed` complementa o wrapper void; `CommandResult.Original` é um recibo imutável com payload/revisão/IDs/recuperados; `ComparisonQuery` agrupa o alvo/ação em `EquipmentCommand.Query`. `IExpeditionCheckpointSink.Commit(snapshot)` retorna bool e nesta etapa confirma apenas memória. Todos os resultados dizem `Durability="Volatile"`. Este port não deve receber um adaptador de disco antes de GAMELAB-11 ampliar o protocolo para gerações, falha indeterminada, suspensão/recuperação e carregamento externo. Codec JSON, nova biblioteca de persistência, gravação, migração e offline não foram implementados.

APIs mutáveis antigas de `Party`/HP/XP são mantidas para os consumidores existentes; mudanças de HP/XP por esses adaptadores invalidam comparações por revisão. O fluxo novo deve usar `ReadView` e comandos, sem editar `Party`, progressão ou inventário. `Rewards.BeginAttempt/Reset` agora são internos ao agregado. Nenhuma API de concessão arbitrária foi criada; os checks usam acesso interno e reflexão somente em fixtures isoladas. O serviço, o arquivo e os IDs do inventário legado permanecem separados.

## Validação e limites

- `scripts/Setup.ps1 -SkipAssets`: ferramentas portáteis .NET SDK 10.0.401 e Godot 4.7.2.
- `scripts/Build.ps1`: cliente, API, checks e importação headless do Godot; compilação sem avisos/erros.
- `EquipmentChecks`: atributos/infusão/DPS/dano antes do escudo, trocas iguais/melhores/piores, nove slots, FIFO/bolsa cheia, conservação de IDs, HP/caídos/níveis, enum e identidade, revisão/replay/conflito, pedidos/cancelamento/pausa, delta grande, descanso parcial, terminais, reinício/reset, snapshot de memória, falhas injetadas, reentrada e overflow. Igualdade de recusas inclui o checkpoint observável inteiro.
- `scripts/Test.ps1`: regressões de domínio, API e rede com dois clientes headless.
- `scripts/Test-Integration.ps1`: fluxo legado de drop/transferência/equipamento/dano e reinício da API com arquivo isolado.

Os comandos completos e resultados finais devem acompanhar o comentário da issue/PR. Testes de persistência/processos precisam de execução com permissões para seus temporários locais; a sandbox negou um move do arquivo temporário legado antes da execução autorizada. Nenhum save manual em `data/` foi editado.

UI/DPI/acessibilidade, gameplay de equipamento integrado, gravação durável e balanceamento não são certificados por estes checks de domínio. Não houve publicação/deploy, offline, energia ou durabilidade de peças. A entrega fica para revisão do Director/Technical Lead e integração posterior, sem merge automático.
