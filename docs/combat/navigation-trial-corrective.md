## C3-A corretivo — GAMELAB-64 / PR #7

Correções F1–F6 autorizadas pelo Director na resposta da GAMELAB-69 de 03/11/2026.
Código: `fb9a07262d97f6257da886812967d65f6f9c7dad`, sobre o candidato rejeitado
`975d7719f0df455452f2724e8b345d0aee485d7b`. Mesmo PR/branch, base `e54971d`/PR #6.
O commit posterior deste relatório só documenta resultados. C1.1/C2.1 e produção
permanecem intactas; a autorização F5 permite alterar scripts/CI existentes.

### Matriz de correção, teste e limite

| Achado | Correção/local | Prova executável | Limite residual |
|---|---|---|---|
| F1/P1 | `TrialMotion.cs:165`: a barreira de Hold zera o saldo da chamada que a atravessou | API pública: 3 s versus parcelas até Hold a 2 s; Resume/Advance(0) não anda. Ambos os Holds, checkpoint/restore, resíduo novo de 17.321 µs e partições distintas | Marcha/pausa preservam resíduos; não implementa scheduler de combate |
| F2/P1 | `TrialMotion.cs:229`: ordem física/entrada; token só é confirmado após aceitar separação/coesão | Fixture do parecer: DPS avança a X=22,94 e recebe `[1]`; Guarda permanece X=22,4 sem token. Espelho oeste, vários ocupantes sem ordem por papel, intenção recusada e checkpoints | Sem solver FirstContact; intenção ainda é recusada conservadoramente |
| F3/P2 | Mesmo método: espera antes do papel fora de N; direção/spacing/saída simétricos | Encontro frontal favorece DPS que espera; leste/oeste liberam no último ator; ida completa/reunião e retorno explícito com ordem `[2,...]`, sufixo restaurado com parcelas 123+49.877 µs | Retorno comandado na fixture não é recuperação geral/retorno automático de N |
| F4/P2 | `TrialMotion.cs:195,386`; `TrialSearch.cs:175`: cancelar antes de expandir/publicar; nova busca quando necessária; validar candidato destacado antes de retornar | Invalidação não lança nem publica rota antiga. 13 corrupções rejeitadas sem alterar a simulação original; Pending, N, Hold, resíduos e estado terminal válido restauram | Checkpoint em memória, no mesmo grafo. Não é protocolo de save nem entrada de serialização arbitrária |
| F5/P2 | `Build.ps1`, `Test.ps1`, `Test-NavigationTrial.ps1`, `checks.yml` | Build compila NavigationChecks; Test padrão executa asserts e smokes headless N/S. Exige exit/marcador/JSON e presença dos achados/jornadas. CI guarda evidências com `always()` | CI externo do novo head é informado separadamente; sucesso do autor não concede aceite técnico |
| F6/P2 | `TrialGroupCamera.cs:28`; `NavigationTrial.cs:79,80`: medir meshes/yaw e quads gerados na pose apresentada; margem 4 px; bind/reset e resize | Quatro jornadas gráficas N/S normal/640×180, com interpolação, pausa, resize, Tab/reset e restore, quads/corpos separados | Quads não são pixels de glifos. Sem percepção humana/DPI/natural 1×; 976×180 continua falha histórica sem nova prova |

`CorrectiveChecks.cs` usa fixtures independentes injetadas, identificadas no JSON;
não adiciona ganchos de teste à produção. `ContinuousChecks.cs` adapta, com crédito,
o oráculo independente do Technical Lead: tabela literal de C1, complemento do piso,
distância segmento–retângulo e movimento relativo de discos, sem chamar os predicados
do produto. Os mesmos percursos N/S conservam **1.666/1.583 spans**, clearance mínimo
**0,450000/0,848528 m** e separação **0,827647/0,960851 m**, tolerância **2 µm**.
São provas dessas trajetórias, não equivalência universal nem passe dos 48 casos.

### Comandos e resultados

PowerShell, Windows, SDK portátil 10.0.401, .NET 10.0.12, Godot mono 4.7.2,
cliente net8.0/domínio netstandard2.1; Ryzen 7 7700 / RTX 4070 SUPER / OpenGL.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -Interpolated -SkipChecks -SkipBuild -OutputDirectory artifacts/navigation/final-north-normal
# Repetir com -Compact, -South e ambos, em diretórios distintos.
```

Build/import aprovados; **21.017 asserções** no gate final; smokes N/S com
`NAVIGATION TRIAL RENDER PASS`, checkpoints/transições verificados. Regressões
legadas, equipamento (254 asserts), API/rede e integração/persistência aprovadas.
Os testes usam estados isolados; nenhum save manual foi modificado.

Probe de seis comportamentos, mesma fonte compilada contra domínio antigo e novo:
**baseline: exit 1 / seis falhas; corretivo: exit 0 / zero falhas**. Fontes dos dois
projetos, JSONs e códigos de saída estão no pacote (`defectprobe/`). Não confundir
o PASS da suíte original com essa prova de regressão. Os logs/JSONs das tentativas
intermediárias malsucedidas também estão preservados: guarda de saída oeste,
inicialização de Label3D, clamp de float e import impedido pelo sandbox. Os erros
de permissões foram reexecutados com autorização; não foram ocultados como passes.

### Custo comparável

Mede toda a chamada Advance, incluindo consultas de coesão/coerência, em grades
novas. Oráculo executado fora do cronômetro. Debug, processos curtos; mesma máquina,
sem controle estatístico de carga/JIT. JSON de referência: `independent-review.json`
da revisão69 autenticada; final: `gate-final-terminal/checks.json`.

| Jornada | Referência69 p50/p95/p99/máx. (ms) | Corretivo p50/p95/p99/máx. (ms) | Construção fria (ms) |
|---|---|---|---|
| N, 616 chamadas | 0,062 / 10,005 / 24,342 / 29,941 | 0,065 / 10,374 / 25,166 / 30,066 | 566,88 |
| S, 598 chamadas | 0,058 / 10,040 / 24,626 / 29,999 | 0,062 / 10,058 / 24,960 / 32,268 | 573,31 |

Chegada N/S mantém **30,8/29,9 s**; máximo **512 expansões/rodada**. Contagens
permanecem N=58.545 consultas/842 predicados/95.773 hits; S=56.339/206/88.801.
Outra repetição preservada do gate chegou a **37,21 ms** em S: a cauda não é um
limite garantido. Construção anterior69: 641,14 ms, em processo diferente.

Restore Pending agora valida estrutura/caminhos/coesão: 30 amostras,
p50 **1,792 ms**, p99/máx. **2,497 ms**, contra p99 **0,0194 ms** na revisão69.
Restore em N/S: **1,723/1,727 ms**. Esse aumento é esperado pelo novo validador,
não custo ordinário por frame. A câmera com medição completa custa aproximadamente
**1–2 ms** por frame, superior ao contador antigo por pontos (<0,04 ms p99).
Percentis e outliers completos estão nos JSONs gráficos; a medição é incluída no custo.

| Caso gráfico final | Menor margem mesh / quad (px) | Câmera p99 / máximo (ms) | Overflow mesh / quad (frames) |
|---|---|---|---|
| N 1136×780 | 49,6889 / 17,4037 | 1,8856 / 14,9446 | 0 / 0 |
| N 640×180 | 3,9999 / 3,9998 | 1,7668 / 13,2899 | 0 / 0 |
| S 1136×780 | 49,6889 / 17,4037 | 1,8957 / 13,7182 | 0 / 0 |
| S 640×180 | 3,9999 / 3,9998 | 1,8102 / 14,1048 | 0 / 0 |

As capturas usam passos sintéticos de **16.667 µs** e PresentationPosition, incluindo
yaw; não são execução natural 1×. Os quatro casos exigem margem geométrica de 4 px
(tolerância numérica de 0,01 px), sem vértices/quads fora do quadro. Os maiores
deltas iniciais de frame, em torno de 400 ms, permanecem nos relatórios. Não se
certifica 30/60 FPS, GPU, consumo energético, sessão longa ou leitura humana.

### Entrega e decisão pendente

Anexo Multica contém JSONs/capturas/logs/fontes e manifesto SHA-256, com identificação
do commit e dos documentos normativos. Sem novo PR, merge, retarget ou migração
de Stage/Vitals/HUD. GAMELAB-64/61 continuam in_progress; GAMELAB-69 in_review.
Solicitar ao Director revisão independente dirigida deste candidato antes de aceite.

Seleção/fallback macro, FirstContact temporal e recuperação/posto alternativo/retorno
geral de N continuam pendentes e sem liberação neste corretivo. C3-B/C4/C5 e 30-C
humano continuam fechados/pendentes conforme a decisão vigente. A referência de
**7–13 dias** do restante permanece provisória, excluindo C4/C5/assets/gates humanos;
estes testes não fundamentam reduzir essa faixa nem comprometer prazo do conjunto.
