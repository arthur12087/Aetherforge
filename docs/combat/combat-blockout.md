# Blockout C1 — Fornalha, plano XZ

GAMELAB-62 · 02/11/2026 · versão C1.1 para revisão. Especificação geométrica; não é cena construída nem mapa jogável. Regras associadas: [combat-rules.md](combat-rules.md). Unidades em metros lógicos; `(x,z)` no chão `y=0`, +X para o destino geral, +Z para o norte deste documento. Não converter X em progresso de encontro.

## Geometria normativa

`MapId=fornalha-c1`, `MapRevision=1`. Limites de desenho: `[0,62] × [0,28]`. O piso caminhável é **somente a união** dos retângulos abaixo; fora dela é sólido/intransitável e bloqueia visão. Bordas adjacentes compartilhadas são portais abertos, não paredes sobrepostas. Todas as paredes têm altura bloqueadora lógica de 3 m; altura visual pode diminuir para a câmera sem mudar a consulta.

| ID | Retângulo X × Z | Dimensão | Função |
|---|---|---|---|
| A | `[0,8] × [10,18]` | 8 × 8 | Entrada e retorno de rota |
| B | `[8,23] × [2,26]` | 15 × 24 | Pátio: E1 ao norte, E2 ao sul; acesso aos dois contornos |
| N | `[23,31] × [20,21.6]` | 8 × 1.6 | Passagem norte estreita |
| S | `[23,31] × [4,8]` | 8 × 4 | Passagem sul alternativa larga |
| C | `[31,40] × [2,26]` | 9 × 24 | Pátio leste, saída dos contornos e E3 |
| D | `[40,48] × [8,20]` | 8 × 12 | Reunião e E4 |
| F | `[48,62] × [2,26]` | 14 × 24 | Área do Capataz, E5 |

Máquina central: caixa sólida `[23,31] × [8,20]`, altura 3 m, 8 × 12 m. O espaço `[23,31] × (21.6,28]` também é fechado: a passagem norte não tem um desvio oculto por cima. Não adicionar props com colisão a esta revisão. Chão não contém buracos, rampas, saltos ou vários andares.

Os retângulos definem limites físicos. Para caminho, erodir a união por `raio + folgaParede`; não erodir cada retângulo separadamente, pois isso fecharia os portais. Raio aliado/inimigo comum 0.35 m, folga de parede 0.10 m: centro fica ao menos 0.45 m das bordas externas. Capataz: raio 0.55, folga 0.10. Obstáculos/visão usam a geometria física compartilhada; pathfinding considera volume do agente. Tangência com parede conta como bloqueio para visão. Uma linha que passa exatamente num canto não concede tiro.

```text
Norte (+Z)         x=0       8               23       31        40       48             62
z=26                        +---------------+        +---------+        +--------------+
                            | B: E1         | sólido | C: E3   |        | F: E5        |
z=21.6                      |               +--------+         |        |              |
                            |               | N 1.6m |         +--------+              |
z=20                        |               +--------+         | D: E4  |              |
z=18              +---------+               |máquina |         |        |              |
                  | A       |               | 8×12m  |         |        |              |
z=10              +---------+               |        |         |        |              |
z=8                         | B: E2         +--------+         +--------+              |
                            |               | S 4m   |         |        |              |
z=4                         |               +--------+         |        |              |
z=2                         +---------------+        +---------+        +--------------+
```

O desenho é orientativo; coordenadas/tabelas prevalecem. A entrada em cada pátio exige usar também Z, e os contornos não têm encontro/recompensa próprios.

## Pontos e trajetos de referência

Cada `R` é um centro de reunião, não um slot fixo do tanque. Ao chegar, três vivos ocupam pontos distintos dentro de 2 m de R, com separação mínima de 0.80 m. Uma colocação de referência é `R+(0,0)`, `R+(-1,0.8)`, `R+(-1,-0.8)`; o solucionador por função pode escolher outros pontos válidos. Caídos não bloqueiam a passagem e não contam na condição de reunião.

| ID | Coordenada | Uso |
|---|---|---|
| R0 | `(4,14)` | Spawn/retorno: tanque `(4,14)`, DPS `(3,15)`, suporte `(3,13)` |
| R1 | `(11,18)` | Reunião/ativação E1 |
| R2 | `(17,8)` | Reunião/ativação E2 |
| J0 | `(21,14)` | Escolha de contorno após E2 |
| N0, N1, N2, N3 | `(21,20.8)`, `(23.6,20.8)`, `(30.4,20.8)`, `(33,20.8)` | Contorno norte e pontos fora do gargalo |
| S0, S1, S2, S3 | `(21,6)`, `(23.6,6)`, `(30.4,6)`, `(33,6)` | Contorno sul |
| R3 | `(33,14)` | Reunião após qualquer contorno; ativação E3 |
| R4 | `(41.5,14)` | Reunião/ativação E4 |
| R5 | `(50,14)` | Reunião/ativação E5 |
| X5 | `(59,14)` | Referência visual de conclusão; não concede vitória por atravessar |

Grafo de macro objetivos: `R0→R1→R2→J0→(N0→N1→N2→N3 OU S0→S1→S2→S3)→R3→R4→R5`. Ligações são rotas para o navegador, não autorização para traçar uma reta através de paredes. Referência da bifurcação: polilinhas de J0 a R3 por N0/N3 ou S0/S3. Comprimentos: norte `6.8+12+6.8=25.6 m`; sul `8+12+8=28 m`. Com estado inicial livre, norte vence por menor comprimento; empate de custo prefere N. Se N for marcado inacessível pela geometria/recuperação, sul é alternativa real. Sem RNG nem preferência manual nesta fatia. Todos usam a mesma escolha até R3; não dividir a equipe por caminhos diferentes.

## Gargalo e separação

Em N, largura física 1.60 m. Após folga/raio, centros comuns podem ocupar Z de 20.45 a 21.15: largura útil lateral de 0.70 m. Dois aliados precisam de 0.80 m entre centros; portanto **não cabem lado a lado** respeitando a margem. Fila temporária é necessária só aqui. Em S, faixa útil lateral de 3.10 m permite duas pessoas lado a lado com folga; terceiro escolhe posição livre.

Reserva de travessia N cobre X `[22.5,31.5]`, Z `[20,21.6]`, incluindo aproximação dos portais. Por direção, admitir integrantes em sequência, separados longitudinalmente por pelo menos 0.90 m; não travar o corredor inteiro para um único integrante. Primeiro: menor comprimento restante até a entrada; empate Guarda→DPS→suporte→EntityId. Uma reserva já concedida tem prioridade sobre novas. Não há inimigos/patrulha no corredor na revisão 1. N não admite ultrapassagem nem troca de direção enquanto ocupado. Em retorno comandado, terminar a travessia na direção atual e reunir em N0/N3 antes de inverter.

Quando o último vivo sai para X ≥31.5, liberar reservas; cada um busca R3 por caminho válido e volta ao posicionamento livre. A reunião exige todos os vivos a até 2 m de R3, separação ≥0.80 e sem travessia pendente. Só depois E3 pode ativar. Caminho individual até o objetivo não pode separar vivos mais de 8 m por distância navegável; o líder espera se o próximo movimento violar esse teto. Esse limite evita zoom excessivo; não define uma distância única ou formação de combate.

## Distribuição dos cinco encontros

Somente o próximo encontro elegível tem entidades hostis instanciadas; os demais POIs podem aparecer como marcos inertes. Nunca mais de dois inimigos vivos/ativos. As cinco composições e HP vêm da revisão-base; os nomes/índices não são identidades de spawn.

| Encontro lógico / índice | Reunião | Inimigos em `(x,z)`, HP | Polígono de movimento hostil |
|---|---|---|---|
| E1 Entrada / 0 | R1 | rat `(14,19)`, 100; rat `(16,20)`, 90 | B |
| E2 Trilhos / 1 | R2 | scavenger `(20,6)`, 140; rat `(21,9)`, 100 | B |
| E3 Pressão / 2 | R3 | sentry `(37,14)`, 180; drone `(38,17)`, 120 | C |
| E4 Ninho / 3 | R4 | watcher `(45,12)`, 180; scavenger `(46,16)`, 150 | D |
| E5 Fornalha / 4 | R5 | foreman `(56,14)`, 420; drone `(57,18)`, 100 | F |

Cada inimigo ainda respeita leash navegável de 6 m do próprio spawn, recortado pelo polígono acima. Aliados podem ocupar áreas conectadas vizinhas, sujeitos ao limite de grupo; inimigos não perseguem pelo gargalo nem ativam outro encontro. Destinos ofensivos devem existir dentro da interseção entre polígono, leash e alcance, sem atravessar parede.

Uma **nova instância** de encontro arma o gatilho apenas após o anterior estar creditado/encerrado, descanso e trajeto concluídos. Na recuperação para uma etapa específica, somente ela é armada: não exigir nem recreditar as etapas puladas. Ativação exige todos os vivos dentro do disco de 2 m da reunião correspondente, separados, sem bloqueio de gerenciamento/pausa, e nenhum outro combate ativo. Usa posição XZ e estado lógico; cruzar um limiar X, entrar pela passagem sul ou voltar ao pátio já limpo não ativa nem paga nada.

Ao ativar: fixar geração/IDs dos dois spawns, publicar combate, liberar posições de reunião e escolher destinos por função. Inimigos esperam até a ativação, sem dano de proximidade na marcha. Ao vencer: congelar o resultado, pagar uma vez e manter quedas por até 0.70 s; 2 s de descanso antes de buscar a próxima reunião. Se houver gerenciamento solicitado, atender antes de consumir descanso. O chefe usa a continuidade definida nas regras, sem cura de descanso extra.

Exemplos de posições **possíveis**, sem exigir slots: em E3 tanque `(35.6,14)`, DPS `(32.4,14)`, suporte `(33,16)` diante da sentry `(37,14)`. Distâncias entre tanque/alvo=1.4, DPS/alvo=4.6 e suporte/tanque≈3.28. Em E5 tanque `(54.5,14)`, DPS `(51.5,14)`, suporte `(52,16)`, foreman `(56,14)`: 1.5, 4.5 e 3.20 m respectivamente. Todos os discos cabem no piso; são testemunhas geométricas, não prova de que a IA já chega lá.

## Retorno, câmera e aceite de geometria

Após derrota, o ponto de recuperação é a reunião da etapa de retorno, com nova tentativa e colocação sem sobreposição. Após conclusão, retorno é R0. São transições explícitas de tentativa, com aviso de recuperação/retorno e apresentação de saída/entrada; não teleporte para resolver bloqueio durante marcha/combate. A marcha ativa sempre usa caminhos físicos. Retirada termina sem retorno automático.

Enquadrar grupo em XZ com margem de 1.5 m e ameaça ativa; teto de separação dos vivos 8 m navegáveis. Limite inicial de extensão visível no chão: 14 m no maior eixo ao redor do combate, a calibrar por UI/UX em compacto. Se a composição não couber, indicar ameaça fora do quadro e reunir por navegação; nunca reduzir infinitamente a escala nem mover atores pelo enquadramento. C2 contrata projeção/zoom e C3 mede: 14 m é hipótese de desenho, não capacidade medida do viewport curto.

Aceite para C2/C3: reproduzir a união exata; preservar portal A/B de 8 m, B/N e N/C de 1.6 m, B/S e S/C de 4 m, C/D e D/F de 12 m; encontrar caminho pelos dois contornos com agente de raio 0.35; impedir reta J0→R3 pela máquina; atravessar N sem sobreposição; reunir antes de E3; completar E1–E5 tanto por N quanto por S com exatamente cinco oportunidades de recompensa. Restaurar checkpoint dentro de N conserva caminho/índice/reservas e direção. Um objetivo desconectado deve entrar na recuperação de bloqueio e depois parar com motivo, sem atravessar sólido.

Verificação estática desta entrega: ver `combat-rules.md`, seção de evidências. C3 ainda precisa validar colisão/avoidance e giro em runtime; C5 precisa observar ritmo, leitura em profundidade e enquadramento. Mudar raio, folga ou largura exige repetir o cálculo do gargalo e revisar MapRevision.
