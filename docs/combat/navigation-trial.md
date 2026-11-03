## Ensaio isolado C3-A — GAMELAB-64

Cena explícita `src/Aetherforge.Client/NavigationTrial.tscn`, sobre
`e54971d616eac6900a218a4051f8dad4921daea8` (PR #6 aberto). O PR do ensaio deve
ter como base `agent/gameplay-engineer/c8761626ec35`; não main. O corretivo F1–F6
altera o ensaio e inclui seus checks/smoke nos scripts e CI existentes. `project.godot`, Main, Stage, Vitals, áudio,
equipamento, persistência e evidências anteriores permanecem intactos.

Após `scripts/Setup.ps1 -SkipAssets` e `scripts/Build.ps1`, na raiz:

```powershell
$env:DOTNET_ROOT = (Resolve-Path .tools/dotnet).Path
$godot = (Get-ChildItem .tools/godot -Recurse -Filter '*_console.exe' | Select-Object -First 1).FullName
& $godot --path src/Aetherforge.Client res://NavigationTrial.tscn
# Acrescentar -- --south para executar a testemunha do outro contorno.
```

Espaço pausa; C cria checkpoint volátil; R restaura; Tab reinicia o ensaio no
outro contorno. Esses controles pertencem somente à janela da cena de ensaio.
Não há dano, inimigos de produção, recompensa, save ou troca da entrada normal.

Verificação reproduzível (o runner encerra e propaga falha):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -OutputDirectory artifacts/navigation/north-normal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -Compact -SkipChecks -OutputDirectory artifacts/navigation/north-compact
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -South -SkipChecks -OutputDirectory artifacts/navigation/south-normal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NavigationTrial.ps1 -Screenshots -South -Compact -SkipChecks -OutputDirectory artifacts/navigation/south-compact
```

Sem `-Screenshots`, executa o mesmo runner headless, sem prova gráfica.
O check separado `tests/Aetherforge.NavigationChecks` mede geometria, cápsulas,
busca, clone/restore e fronteiras reais; não invoca o BFS documental de C1.
Desde o corretivo, `Build.ps1` compila o projeto e `Test.ps1` executa a suíte,
smokes headless N/S e exige marcadores/JSONs. O CI guarda `artifacts/navigation/**`
inclusive em falha. JSON `passed=false` e exit não zero são evidência de falha.
`-Interpolated` usa o mesmo `PresentationPosition` interativo com passos sintéticos
de 16.667 µs. Para quatro capturas gráficas, acrescentar essa opção aos comandos
acima. A cena mede vértices reais dos meshes e quads de Label3D com margem de 4 px,
incluindo yaw, pausa/resize, Tab/reset e restore; não mede pixels de glifos/leitura.

Hold descarta somente o saldo do Advance que cruza a barreira, conservando resíduos
de marcha/pausa. Reservas são confirmadas depois de aceitar a intenção; ocupantes,
proximidade da entrada e espera determinam a ordem, nos sentidos leste e oeste.
Pedidos antigos são cancelados antes da expansão/publicação, com nova busca quando
necessária. Restore valida uma cópia completa antes de publicá-la. A ida/volta explícita
dos checks é uma fixture, sem solver de recuperação geral automático.
Matriz, evidências e limites atuais: `navigation-trial-corrective.md`.

### Implementação e provas

`NavigationTrial/TrialGeometry.cs` extrai as bordas externas da união dos sete
retângulos C1. A máscara conserva portais e igualdade de clearance. Arestas
inteiras, produtos largos e cápsulas bloqueiam travessia entre endpoints válidos.
`TrialSearch.cs` usa grade 620×280/origem 0,05 m, oito vizinhos e desempates
f/h/NodeId, conectores verificados, sem alisamento. Reservas de destino por
prioridade impedem que uma chegada obstrua a rota de outro integrante.

`TrialMotion.cs` mantém posições racionais, tempo de segmento, heading lógico,
pedidos, heap/parents/closed, cursor de orçamento, índices e tokens de travessia.
O avanço usa fronteiras de 50 ms subdivididas nos waypoints; apresentação natural
interpola somente subsegmentos já certificados, com um passo de atraso. Render
não escreve no domínio. Pausa conserva inclusive o resíduo de tempo.

A admissão em N mantém ordem/direção e distância longitudinal mínima de 0,90 m;
o predicado de discos é contínuo e conserva o mínimo geral de 0,80 m. A fila
permanece até o último membro sair. Não se inverte dentro da faixa reservada.
Coesão usa testemunhas de caminho da grade e limites superiores conservadores
transportados por comprimento percorrido; nunca distância euclidiana através
de um sólido. Recertifica perto de 7 m, antes do teto de 8 m. Uma fixture atrasa
o suporte, exige espera do líder e verifica retomada sem Hold espúrio.

O leash é demonstrado separadamente: consulta limitada à área erodida, custo
navegável igual/acima de 6 m e movimento conservador que retorna pelo caminho
ao spawn. Os dois observadores ficam imóveis nessa fixture; ela não certifica
perseguição, seleção de alvo, reset de encontro ou HP de inimigo.

Bloqueio controlado inicia replanejamento após 1 s, retração pelo segmento válido
após 2 s (sem inversão de N), reunião por caminho no último R seguro após 5 s,
e Hold técnico após 8 s ativos. Pending e esperas legítimas não contam como stuck.
Retomar faz uma nova tentativa explícita. A fixture usa uma porta de falha do
piloto, não acrescenta um prop à geometria normativa. Não afirma recuperação
universal de qualquer composição de corpos.

Os checks exercitam ambos os contornos até R5, obstáculos, independência de
destinos, ausência de teleporte, separação, coesão, leash, orçamento/Pending,
timeout, checkpoint durante N, suffix idêntico com partições distintas de tempo,
checkpoint destacado e rejeição de geração divergente. O runner gráfico captura
também pausa/restore e fixtures identificadas de bloqueio, Hold e leash.

### Calibração e limites para revisão

O orçamento inicial de 8.192 expansões foi medido: uma fronteira fria excedeu
180 ms. O padrão do piloto foi calibrado para **512 por rodada de 100 ms**, ainda
dentro do teto C2 de 8.192. A alternativa foi verificada com uma grade nova e
termina sem estourar o timeout de Pending de 2 s. `checks.json` conserva a
comparação fria entre 8.192 e 512, percentis e máximos; o custo de construir
máscara/grade aparece separado. Não prometer 30/60 FPS pelo orçamento lógico.

O ensaio torna explícitas estas diferenças que exigem revisão antes de C3-B:

- N e S são testemunhas selecionadas por comando. Ainda falta o selecionador
  macro que verifica disponibilidade, compara 25,6/28 m e troca o grupo de ramo
  automaticamente após falha. Não apresentar `--south` como prova desse fallback.
- O swept-disc rejeita conservadoramente a intenção inteira quando há contato;
  não calcula ainda o último microssegundo válido/normal/ObstacleId de FirstContact.
  Segurança e progresso das fixtures passaram; a equivalência temporal completa
  de C2 continua pendente. Alternativa: manter a rejeição conservadora no piloto
  e implementar o solver de primeiro contato antes de acoplar ações.
- A recuperação recua no máximo o segmento atual, menor que 0,15 m; não implementa
  ainda seleção geral de posto alternativo em até 1,5 m. A reunião está implementada
  fora do corredor; em N bloqueado preserva direção e entra em Hold.
- Checkpoints são clones em memória no mesmo grafo; rejeitam hash/geração ou
  instância divergente. Não são DTO persistente nem migração de save.
- Máscara de navegação do ensaio usa os três raios aliados de 0,35 m. A consulta
  geométrica aceita outros raios e há check de 0,65 m, mas a segunda máscara e
  navegação do Capataz não estão integradas.
- Câmera ortográfica a 55°, zona de 0,75 m, lead até 1 m, suavização 6/s e zoom
  4/s; maior extensão do chão limitada a 8–14 m. Usa viewport real e registra
  objetivos/placas fora do quadro. O ensaio de largura integral 976×180 falhou:
  cortava corpos nas curvas. A solução do piloto mantém 180 px de altura e usa
  viewport 640×180 centralizado na janela 1000×320, com faixas laterais. Não é
  a barra nativa/HUD: sua eventual integração exige decisão de UI/UX. O relatório
  conta corpos e placas separadamente. Não certifica legibilidade, input humano,
  DPI físico ou gate 30-C.

As medições gráficas usam avanço fixo de 50 ms por frame, não tempo natural 1×.
Capturas sincronizadas e aquecimento podem gerar outliers; o JSON descreve o
método e separa a jornada das fixtures de falha. Sem benchmark geral, sessão de
20 minutos, medição de consumo energético ou aceite independente.

### Restante de C3

Referência provisória acolhida pelo Director após GAMELAB-69: **7–13 dias de
engenharia** do restante, incluindo 3–6 dias de pré-requisitos, sujeita a nova
estimativa após a revisão deste corretivo. Não é prazo comprometido do corretivo;
exclui revisão/espera, C4/C5, assets finais e gates humanos. Os gargalos medidos são trabalho
de busca frio e extensão vertical disponível em compacto, não HP/economia.

O piloto fica para revisão na GAMELAB-64, que permanece in_progress. Não libera
C3-B, migração Stage/Vitals, C4/C5 ou os 48 casos completos de C2. Preservação
seletiva de VFX aliado e evento anterior à primeira amostragem continuam como
requisitos da integração futura, junto aos oráculos 29/30 e à revisão humana 30-C.
