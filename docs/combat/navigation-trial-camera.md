## Câmera elevada do ensaio C3-A — evidência do autor

Código executado: **908dfdd418dc9692dd6f61c228b404ac01037753**, incremento visual
posterior a **52267cb833b135778b6b6b7a639eab4c2f0d06ca**, no mesmo PR #7 e branch
`agent/gameplay-engineer/08687138e308`. Base de integração e54971d/PR #6 preservada.
Este relatório é posterior ao código e não altera o binário.

Referência aprovada: Kling4-v3 da GAMELAB-61, anexo
`01a1a3ec-cc0c-7c6c-a8ce-cba7ff8c9193`, SHA256
`7cb84f02fb52f4ad9303a44bc4339f8746e6c03e9d1cafd56f4e2983f8b9b2ce`.
Foi baixada pelo CLI e inspecionada em frames; a vista elevada/estável orienta a
composição. O material gerado não foi usado como evidência do runtime, nem seus
números/animações foram copiados. Sem geração adicional paga ou assets finais.

### Delta e isolamento

- `TrialGroupCamera.cs`: ortográfica real, yaw fixo45°/pitch55°, fit nos eixos
  projetados da câmera e pose apresentada, limite do frustum no chão8–14m,
  margem preferida1,5m/absoluta4px, follow/zoom exponenciais e reset de bind.
- `NavigationTrial.cs`: piso com juntas de chapa em um mesh visual, ferro/fuligem,
  latão e cores provisórias dos papéis. Máquina baixa/translúcida e portais
  continuam legíveis. Não acrescenta corpos físicos ou sólidos lógicos.
- `TrialCameraCheck.cs/.tscn`, seu `.uid` e `Test-TrialCamera.ps1`: check dirigido,
  comparação de candidato e captura real do framebuffer, com relógio natural1×.
- Guia do ensaio: reprodução, leitura do vídeo e contrato da integração futura.

**Diff zero contra52267cb no domínio/restore/F4, normas C1/C2, CI, scripts de
build/teste existentes, Stage/Vitals/HUD, combate/equipamento/save e entrada padrão.**
Os arquivos de cena do ensaio continuam opt-in. A revisão71 permanece atribuída
ao seu SHA fixado, sem certificar estes commits visuais posteriores.

### Comparação verificada

O runner extrai os dois arquivos de apresentação exatos de52267cb, recompila
sem cache incremental e restaura os bytes atuais em finally. Cada prova inclui
hashes dos fontes, driver e binários. Uma verificação externa compara TimeUs,
fase, pausa, ramo, root/viewport e todas as poses apresentadas nas mesmas amostras:
**quatro pares A/B idênticos no trajeto e poses; yaw0° antes e45° depois**.

O fit usa os cantos do AABB local do mesh completo, transformados pela pose,
inclusive a arma; o oráculo separado projeta os vértices reais dos triângulos e
os quads gerados de Label3D pelo Camera3D. Não reduz corpo/arma/pés a pontos
centrais. Os quads reservam os indicadores atuais completos. Tolerância do check
de projeção float:0,02px na margem de4px.

| Caso | Root / viewport útil | Margem mínima corpo antes → depois (px) | Quad depois (px) | Recortes depois |
|---|---|---:|---:|---:|
| N normal | 1160×880 / 1136×780 | 22,516 → 14,572 | 3,99974 | 0 |
| N curto | 1000×320 / 640×180 | 4,000 → 10,263 | 3,99986 | 0 |
| S normal | 1160×880 / 1136×780 | 18,226 → 14,572 | 3,99923 | 0 |
| S curto | 1000×320 / 640×180 | 4,000 → 10,144 | 3,99986 | 0 |

Cada jornada cobre aproximações, curvas, gargalo/contorno e reunião. As transições
usam C/Espaço/R/Tab pelo mesmo handler do cliente: pausa, resize1000×320,
1160×880 e540×280, restore, troca de ramo e R sem checkpoint após Tab. No resize
menor o viewport útil é516×180; o estado pausado fica idêntico. Os checks usam
eventos programáticos, sem alegar input humano. O frustum medido por interseção
independente dos quatro raios com y=0 não excedeu14,000004m (erro float da projeção).

**976×180 continua reprovado**, medido explicitamente, sem troca silenciosa de
dimensão:44 frames com corpo fora da margem e87 com quad fora; mínimos−3,128px
e−18,193px. O JSON desse diagnóstico marca a execução concluída, mas seus
contadores são uma falha de enquadramento; o agregado explicita `framingPassed=false`.
640×180 segue experimento do ensaio, sem aprovação para a barra nativa.

### Runtime natural e custo

Ryzen7 7700/16 threads, RTX4070 SUPER, Windows, Godot mono4.7.2/OpenGL Compatibility.
MP4s reais do framebuffer,20fps/H.264, sem áudio, IA ou aceleração. Duração de
parede inclui a pausa de controle; resize pausado é letterboxed no tamanho fixo
do vídeo. O relógio ativo exclui apenas esse bloco de pausa registrado.

| Jornada natural | Tempo ativo / simulação | MP4 | Frame p50 / p95 / p99 / máximo (ms) | Present p99 (ms) |
|---|---|---|---|---:|
| N normal | 30,8114s / 30,80s | 1160×880,32,10s | 13,332 / 18,604 / 23,188 / 80,906 | 0,3132 |
| S curto | 29,9089s / 29,90s | 1000×320,31,05s | 13,330 / 15,364 / 19,318 / 34,211 | 0,2935 |

Ambos passaram com zero recortes e diferença de relógio final abaixo de12ms;
isso não afirma identidade de resíduos terminais arbitrários. A mensuração de
Present inclui atualização de poses+câmera. Nas comparações fixas, seu p99 foi
2,363–2,537ms antes e0,104–0,121ms depois. O fit conservador por AABB evita
projetar todos os triângulos no caminho normal; o check independente continua
projetando-os para provar o enquadramento. São comparações locais, sem isolar
causalidade geral ou certificar desempenho em outras máquinas.

No natural, Advance teve p99/máximo11,815/38,664ms emN e10,984/28,093ms emS.
Os frame times incluem a medição independente, capturas e encoder. Não certificam
30/60FPS, DPI, percepção humana ou pixels de glifos/VFX futuros. O primeiro sample
`start` usa o relógio de aquecimento; os samples da jornada usam o relógio
reiniciado na partida. Não confundir os dois eixos desse log.

### Validação e revisão

`Build.ps1`, `Test.ps1` e `Test-Integration.ps1` passaram, incluindo **56.156**
asserções da suíte NavigationChecks existente, smokes N/S, domínio/equipamento,
rede/parede e transferência real com defesa13/dano17 e reinício. O novo check
passou em oito execuções fixas A/B e duas naturais. Fontes/hashes, JSONs, logs,
capturas, vídeos e verificador externo estão no pacote anexado à GAMELAB-64.
A tentativa inicial de comparação reutilizou um binário incremental e foi
descartada; somente as provas recompiladas e com ângulo verificado foram entregues.

Limites: bounds atuais são dos placeholders estáticos; quads não são pixels de
glifos e a geometria não mede compreensão. Meshes animados, troca de equipamento,
textos/HP/números e VFX futuros precisam invalidar/reoferecer seus bounds ao
adaptador antes do fit. Casings foram conferidas nas capturas, sem certificado
pixel a pixel de oclusão entre todos os atores/efeitos. A câmera não altera regras.

O aceite delimitado de F4 pelo Director no52267cb não cobre este incremento.
**R71-1 permanece limite conhecido:** resíduos terminais podem diferir em certas
partições; não houve normalização nem correção de domínio nesta rodada. Custos
representativos de restore/Advance e construção continuam riscos de integração,
sem generalizar2–3ms de checkpoints iniciais. Permanecem os pré-requisitos C2,
30-C e a faixa provisória7–13dias; este trabalho não libera C3-B/C4/C5.

Solicita-se revisão própria do delta visual e das duas capturas naturais pelo
Director, mantendo64/61 in_progress. Para a futura fatia, preservar
preparação→contato/chegada→HP/número/som/VFX→recuperação e oferecer seus bounds
à câmera; este ensaio não integra nem certifica essa timeline de combate.
