## Evidência do autor — C3-A, 03/11/2026

Código executado: `e0e56f095281e7fc9bcb3083ab8d06fda2fc3acd`, descendente de
`e54971d616eac6900a218a4051f8dad4921daea8`. Branch própria
`agent/gameplay-engineer/08687138e308`; base do PR é a branch do PR #6 aberto.
O commit posterior deste relatório não altera o binário. Delta de produção:
somente arquivos novos; entrada/HUD/combate/equipamento/saves anteriores intactos.

Entradas autenticadas C1.1/C2.1 e hashes dos documentos exatos estão em
`input-provenance.json`. Reprodução e limites: `navigation-trial.md`.
Os JSONs completos, capturas e manifesto SHA-256 são anexos da GAMELAB-64;
não são caminhos de entrega locais para o leitor.

### Resultado lógico

**17.170 asserções aprovadas** no executável novo, runtime .NET 10.0.12.
União física, máquina/exterior, tangência/clearance, conectores, travessia entre
endpoints, swept-disc, coesão atrasando suporte, leash 6 m/fora de 6 m/retorno,
duas rotas, reservas, Pending/timeout, pausa, checkpoint destacado, restore com
outras partições de Advance, geração inválida e bloqueio/retomada passaram.

Com orçamento de referência 8.192: N chegou a R5 em 28,10 s lógicos e S em
27,30 s; separações mínimas amostradas 0,804935 m / 1,224595 m; N admitiu três
tokens simultâneos. Não são tempo natural, resultado de combate ou duração de
encontro. O padrão calibrado de 512 chegou por N em 30,80 s, incluindo Pending.

Medições em milissegundos no processo de checks:

| Operação | p50 | p95 | p99 | máximo |
|---|---:|---:|---:|---:|
| Fronteira, referência fria 8192 | 0,057 | 2,007 | 9,561 | 213,883 |
| Fronteira, grade nova com padrão 512 | 0,062 | 10,728 | 25,156 | 31,750 |
| Segmento/clearance (1000 consultas) | 0,0103 | 0,0115 | 0,0147 | 0,1183 |
| Discos varridos (1000 predicados) | 0,0008 | 0,0009 | 0,0012 | 0,0016 |
| Rodada A* 512, cache aquecido (3 amostras) | 0,730 | 0,759 | 0,759 | 0,759 |
| Clone+restore Pending (30 amostras) | 0,0045 | 0,0133 | 0,0134 | 0,0134 |

Construção da máscara/grade: 611,97 ms, fora da marcha. Clone+restore no
corredor: 0,143 ms. Memória do processo de checks: working set ~71,1 MB,
managed ~21,1 MB na leitura final. São leituras pontuais, sem prova de estabilização
ou pico de memória. Os três samples de A* não sustentam inferência estatística
de cauda; o JSON conserva a quantidade de cada conjunto.

### Resultado gráfico

Godot mono 4.7.2, Windows/OpenGL Compatibility, Ryzen 7 7700 (16 threads lógicas),
RTX 4070 SUPER. Quatro execuções separadas, avanço lógico fixo 50 ms por frame
renderizado; cada uma concluiu a jornada e as fixtures de falha, com checkpoint
verificado. Duração de parede por caso: 8,8–9,4 s. Não são execuções natural 1×,
benchmark de longa duração, playtest humano nem validação de DPI físico.

| Caso | Root / SubViewport real | Navegação p99 / máx. ms | Câmera p99 ms | Frame p50 / p95 / p99 ms | Recortes corpo / ponto da placa |
|---|---|---:|---:|---|---:|
| N normal | 1160×880 / 1136×780 | 19,893 / 25,461 | 0,0320 | 13,636 / 14,286 / 20,000 | 0 / 0 |
| N curto | 1000×320 / 640×180 | 19,991 / 25,601 | 0,0370 | 13,333 / 13,636 / 13,636 | 0 / 0 |
| S normal | 1160×880 / 1136×780 | 17,129 / 25,104 | 0,0385 | 13,636 / 13,698 / 28,215 | 0 / 0 |
| S curto | 1000×320 / 640×180 | 17,360 / 25,066 | 0,0357 | 13,333 / 14,286 / 15,000 | 0 / 0 |

Os máximos de frame de 400–446,95 ms incluem o primeiro delta de inicialização.
O runner não remove esse outlier. Capturas sincronizadas também afetam cadência;
nenhum desses percentis certifica 30/60 FPS em uso natural. Working set gráfico
ao final: ~253,7–273,5 MB, leitura pontual. RAM total do host/consumo GPU/energia
não foram medidos. A contagem de enquadramento verifica pés/topo e ponto lógico
da placa, não todos os pixels do texto nem entendimento humano.

Antes da solução, 976×180 perdeu enquadramento de corpos em 328 frames no N
(jornada com orçamento 512). A primeira tentativa com 640×180 ainda cortava
placas; a câmera passou a limitar foco usando altura de placa e margem, além de
validar a jornada inteira com erro se houver recorte de ponto/corpo. A prova final
acima usa esse código. O compacto 640×180 centralizado é escolha explícita do
ensaio; não modifica nem certifica a barra nativa já revisada de GAMELAB-30.

As capturas distinguem viagem, Pending, pausa/restore, bloqueio/replanejamento,
Hold de orçamento/bloqueio e retorno por leash. A retomada de Hold é um comando
explícito. Fixtures de falha não são inimigos/obstáculos extras no mapa C1.

### Regressão e decisão pendente

`Build.ps1`, `Test.ps1` e `Test-Integration.ps1` produziram sucesso: build/import,
domínio/equipamento/persistência, rede/parede e transferência real com defesa
13/dano17 e reinício. Os checks legados foram reexecutados no build final; 254
asserções de equipamento incluídas. Não se repetiu QA geral 29/30. O invólucro
inicial de PowerShell emitiu dois erros de guarda `-ne`; os scripts completaram
e seus resultados foram conferidos, e os checks finais foram chamados diretamente.

Ainda precisam de decisão/revisão: selecionador macro automático N/S; política
geral de alternativa até 1,5 m e retorno após bloqueio dentro de N; solver de
primeiro microssegundo/FirstContact; persistência futura e máscara do chefe.
As alternativas conservadoras deste piloto estão explicitadas no contrato do
ensaio, sem reescrever C1/C2 ou conceder passe aos 48 casos.

O restante de C3 foi reestimado em 5–9 dias de engenharia, incluindo 1–2 dias
para fechar as lacunas espaciais acima; não é autorização de C3-B. GAMELAB-64
continua in_progress para revisão do Director na coordenação GAMELAB-61.
