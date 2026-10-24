## Contrato de apresentação de inimigos (GAMELAB-25)

Base: `b18ef0010b29edaf1c4b67f04823594283b9f12f`, PR #4. O delta deve ser integrado sobre essa branch; não inclui o manifesto documental de GAMELAB-19.

Transações continuam clonando os DTOs. O palco identifica a tentativa por `(SessionId, AttemptId)` e o encontro por `(SessionId, AttemptId, StageIndex, EncounterAt)`. Dentro do encontro, a composição é a sequência ordenada `(posição, Kind, Name)`. Esses campos são imutáveis no DTO atual; HP e demais valores amostrados não são identidade. Clones com a mesma chave atualizam `_enemyData` sem reconstruir os atores. Mudança da chave, ordem, espécie, nome ou quantidade retira a formação anterior. A rota atual cria uma única formação por etapa/tentativa; se o domínio passar a criar várias formações idênticas nas mesmas coordenadas, precisará expor uma geração de encontro explícita.

O ator Godot é o token de apresentação que Vitals usa para placas/números. DTO novo não invalida esse token. Atores são retirados imediatamente em troca real de composição; números inimigos e efeitos antigos são limpos. Reinício/nova sessão/bind também limpam poses aliadas, feedback e cursor. Bind de uma simulação em andamento amostra o estado atual sem reapresentar o histórico retido. Eventos são consumidos por ID dentro da tentativa; uma troca de composição no mesmo encontro estabelece um piso de eventos para inimigos, sem descartar cura aliada. Eventos inimigos retidos não atingem a formação que já entrou na próxima marcha.

Números usam o mesmo `Elapsed` das poses: duram 0,65 s de simulação, inclusive na fase Loot. Pausa congela idade, transformação e posicionamento; retomada continua. Permanecem os limites anteriores de dez números/dois por ator e a substituição por saturação. O layout pode ocultar um número quando não encontra uma faixa livre; isso é diferente de invalidar sua identidade. Fases terminais congelam `Elapsed`: preserva-se o comportamento anterior de limpar transientes e mostrar cadáver já assentado, sem introduzir relógio de parede ou mudar o scheduler.

## Verificação reproduzível

Após `scripts/Build.ps1`:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Presentation.ps1 -Fast
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Presentation.ps1 -Screenshots -OutputDirectory artifacts/presentation-1x
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Screenshots -Native
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-UiV19.ps1
```

`Test-Presentation` é opt-in e usa somente a simulação em memória da instância de teste. Registra tempos de parede/simulação separados, IDs, HP, cursores de hurt/kill, transforms e retângulos de números. `-Fast` usa passos fixos para correção; não certifica timing de parede. A execução gráfica usa delta real a 1× e captura a queda e o número do segundo inimigo. Não é benchmark de capacidade, prova de DPI físico ou teste de input humano.

As fixtures finais preservam tempo/event IDs iguais entre tentativas/sessões antes de amostrar, para detectar resets que não seriam reconhecidos apenas por relógio regressivo. Também reordenam/removem integrantes nas mesmas coordenadas e fazem bind com histórico. Os oráculos de equipamento E01–E15/F01–F10 e gestos obsoletos permanecem nos testes existentes, sem alteração.

O hash de `ExpeditionStage.cs` e de `ExpeditionVitals.cs` diverge do manifesto de GAMELAB-19 após esse delta; registrar os hashes exatos no relatório da entrega para conciliação. `ExpeditionActor.cs`, `ExpeditionGeometry.cs` e as receitas geométricas não mudam. Não se declara o manifesto atualizado.
