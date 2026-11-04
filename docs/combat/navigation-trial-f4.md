## C3-A — corretivo F4 após o parecer GAMELAB-70

Corretivo do autor para avaliação do Director, no mesmo PR #7 e branch
`agent/gameplay-engineer/08687138e308`, base `e54971d616eac6900a218a4051f8dad4921daea8`
/ PR #6. Entrada: `d094d4c9357f9700f21c8b0c2b3879e22cc2f000`.
Runtime corrigido: `e0d62bab63ffffb5402d5156ba5d6d5aa04049bf`.
Código e testes executados: `855f7c538a754af3eaf28ffab0e4471b7eba43e2`;
o commit posterior desta entrega acrescenta somente este relatório.

O parecer70 desfavorável permanece histórico válido. Seu ZIP foi baixado pelo
CLI: SHA-256 `d770403afedcd6d3c3b7d50fe26df9d068c4919942433315e590e0075675e7d1`,
290/290 arquivos do manifesto conferidos. Foram lidos o relatório integral e as
reproduções PublicRestore, RestoreProbe e Harness. F4-A é ocorrência natural;
as corrupções F4-B são injeções sintéticas, sem nova ocorrência natural alegada.

### Antes/depois e implementação

| Caso | Entrada d094d4c | Corretivo e resultado do autor |
|---|---|---|
| F4-A, API pública aos 21,20/21,25/21,30 s | Três recusas reproduzidas; controles 21,15/21,35 s aceitos | Cinco aceites, fingerprint idêntico, sufixos particionados até depois da conclusão |
| Todos os checkpoints naturais N/S a cada 50 ms | Parecer70: 3/616 N recusados, 0/598 S | Probe original: 0/616 e 0/598; gate ampliado inclui t=0: 617 N e 599 S, todos aceitos |
| Heap apagado com trabalho aberto; root destacado; custos/closed/conectores | Aceites indevidos no relatório70 | Rejeição atômica; cobertura da fronteira, custos e vínculos reconstruídos antes da publicação |
| GoalX15 com rota terminando em X12; rota unitária com chegada falsa | Aceites indevidos no relatório70 | Rejeição atômica; endpoint/Goal, índice, busca e chegada conferidos |
| Matriz de coesão zerada | Relatório70: aceita e permite 7,90→8,04 m em 50 ms | Restore recusa. Consumidor também ignora cache sem certificado válido; reprodução ao vivo conserva ≤8 m |
| Pending com heap vazio no limite do orçamento | Estado válido a preservar | Busca real de um nó restaura Pending/heap vazio e conclui na próxima expansão |
| Cauda replanejada/reversão | Exceções legítimas ao endpoint antigo | Aceitas; restore no endpoint enquanto a cauda está Pending, publicação posterior e chegada no novo Goal |
| State desconhecido/RequestKey máximo | Observações sintéticas de endurecimento | State desconhecido e identidade máxima recusados; exaustão explícita antes do incremento, sem wraparound |

`TrialSearch.cs:51` usa o clearance contratual de **450000 µm**, também nos
conectores dos certificados. Não muda mapa, posições, máscara ou predicado real
de colisão; a igualdade continua válida. O acréscimo conservador de comprimento
do certificado permanece separado do raio geométrico.

`TrialSearch.cs:175` reconstrói conectores de origem/destino com área e reservas,
exige parents fechados e custos de arestas reais, confere relaxamento dos nós
fechados e uma entrada atual no heap para cada nó aberto. Duplicatas obsoletas
legítimas continuam possíveis. Best/Cost/status e o caminho Found são conferidos.
Uma fila vazia é aceita quando não há trabalho aberto: terminar o último pop
no limite de orçamento pode deixar Finish para a próxima expansão.

`TrialMotion.cs:112,339,415` vincula o cache numérico de coesão a certificados
imutáveis com extremos racionais e limite emitido por testemunha geométrica ou
transporte Lipschitz. Checkpoint copia a matriz; os valores imutáveis podem ser
compartilhados. Restore confere limite e extremos, além da recertificação pontual;
MoveInterval só consome cache com essa procedência. Zeros simétricos isolados não
são prova de distância. Isso preserva o valor legítimo do cache e o determinismo
do sufixo, sem substituí-lo por uma estimativa nova durante o restore.

`TrialMotion.cs:48,233,445` impede chegada prematura enquanto a cauda está
Pending e permite sua publicação mesmo depois de atingir o endpoint antigo.
Reversão e cauda em andamento podem conservar endpoint antigo, mas não declarar
Complete nele com outro Goal. Identidade usa incremento verificado e erro
`Navigation request identity exhausted`; `long.MaxValue` não é identidade válida
de checkpoint. Essas observações não aumentam a severidade dos achados naturais.

### Gates, reproduções e oráculos

Godot.NET 4.7.2, cliente net8.0, domínio netstandard2.1, SDK portátil 10.0.401.
Comandos na raiz do checkout:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Setup.ps1 -SkipAssets
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Integration.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -Interpolated -SkipChecks -SkipBuild -Compact -OutputDirectory artifacts/f4/north-compact
# Repetir com -South e/ou sem -Compact para as demais três combinações.
```

Build/import, Test padrão com NavigationChecks e smokes headless N/S, regressões
legadas/equipamento, API/rede e integração/persistência passaram. O sandbox
recusou acesso ao NuGet no primeiro build e MoveFile na persistência dos primeiros
Test/Integration; as repetições autorizadas fora dele passaram. Logs de ambas
as tentativas foram retidos. Não houve alteração de scripts/CI ou do save manual.

O gate padrão chama `RestoreChecks.Run` e inclui **56.156 asserções** no total.
Não se toma a contagem isolada como prova: JSON identifica os timestamps,
snapshots/sufixos, 23 novas corrupções nomeadas, três estados válidos dirigidos
e as 13 corrupções originais em CorrectiveChecks. Todas as rejeições conferem
imutabilidade tanto da origem quanto da entrada corrompida. Checkpoint/restore
Pending, N, Hold, resíduos, pausa e terminal continuam exercitados.

`RestoreOracle.cs` usa o complemento literal dos sete retângulos do piso,
clipping segmento/retângulo e movimento relativo dos discos em double, tolerância
2 µm, sem chamar os predicados/certificados do produto. Confere todos os spans
dos sufixos. Para coesão, constrói uma polilinha livre e estende seu limite pelo
deslocamento dos dois atores, provando o intervalo inteiro. Não usa distância
euclidiana através de obstáculo. Nos cinco timestamps, há também testemunha de
grade independente em C erodido fechado, passando por Z=20,05 m; máximo observado
4,778478 m nos controles, 4,498482 m nas três antigas recusas, sempre abaixo de8.
Essa prova contínua dos sufixos não substitui os testes específicos da grade.

Jornadas N/S conservam 30,80/29,90 s no orçamento padrão512, 1.666/1.583 spans,
clearance mínimo 0,450000/0,848528 m e separação mínima 0,827647/0,960851 m.
O Harness70 copiado também passou: zero falhas, incluindo F1 repetido e F2/F3.
A única adaptação de expectativa é aceitar a rejeição do restore na reprodução
do cache falso; originalmente esse caso prosseguia após o aceite para medir8,04m.
Fontes originais e adaptadas, JSONs e diff seguem no pacote.

### Custo e apresentação

Medição final pelo mesmo Harness70: cronômetro ao redor de Advance inteiro,
grade nova por jornada, sem oráculo dentro do tempo. Debug, processos curtos,
sem isolamento estatístico de JIT/carga; outliers e outra repetição preservados.

| Caso | Amostras | Construção ms | Advance p50/p95/p99/máximo ms |
|---|---:|---:|---|
| N | 616 | 544,036 | 0,0875 / 10,866 / 26,391 / 32,878 |
| S | 598 | 542,399 | 0,0847 / 10,828 / 26,394 / 34,186 |

Outra execução atingiu40,289 ms em S. Clone+Restore,30 amostras: Pending
p50/p99 2,132/2,926 ms; N1,990/2,675 ms. Parecer70 observou45,193 ms em Advance;
esses resultados não eliminam o risco nem isolam causalidade do delta.

Quatro execuções gráficas sintéticas16.667 µs passaram: N/S em1136×780 e640×180,
pausa/checkpoint/restore visível e transições resize/Tab/reset/restore. Capturas
restored N compacto, S compacto fase6, N normal fase7 e restore S normal foram
inspecionadas. A medição é do runner original preservado, não uma nova validação
gráfica independente. Margens mesh/quad normais≈49,69/17,40 px; compactas≥3,9998 px
para alvo4 px/tolerância0,01 px. Câmera completa p99 1,77–1,97 ms, máximos
13,27–14,66 ms. Algumas sessões gráficas se sobrepuseram; sem benchmark isolado.

Não há certificação30/60FPS, glifos, percepção humana, DPI ou natural1×.
976×180 permanece falha histórica. Nada mudou na apresentação ou na barra nativa.

### Entrega e fronteira

Pacote final anexado pelo CLI contém JSONs/logs/fontes/capturas, metadados dos
SHAs, patch e manifesto. PR#7 continua draft, sem merge/retarget/publicação.
O resultado é correção do autor aguardando avaliação do Director e posterior
validação independente dirigida; não substitui nem altera o parecer70.

GAMELAB-64/61 permanecem in_progress;69/70 in_review. Mantidos7–13 dias
provisórios. Seleção/fallback macro, FirstContact temporal, recuperação geral,
posto alternativo/retorno automático deN e postos/giro continuam pendentes.
C3-B/C4/C5, Stage/Vitals/HUD e30-C humano seguem sem liberação. Checkpoint é
volátil; leash de fixture não certifica inimigos/chefe. C1.1/C2.1 e procedência,
equipamento/saves/VFX e os gates posteriores29/30 continuam preservados.
