## Colocação de números de combate — GAMELAB-30

Correção empilhada sobre `1fae8498af3ac7518fca4d11839366652e93250e` / PR #5.
Implementa a proposta e as precisões do Director em GAMELAB-30. O domínio,
Stage, câmera, fontes, paleta, equipamento e saves não recebem alterações.

### Contrato

`ExpeditionVitals.Refresh` resolve HP/roles/boss, seleção/hover e todos os nomes
antes dos números. Um número nunca desloca essas reservas. Continuam o pool
de dez entradas, limite de duas por alvo e substituição do mais antigo do mesmo
tipo antes do outro tipo. Dano e cura não são agregados.

O posicionamento oferece um lugar a cada alvo ativo antes dos segundos números,
incluindo alvos novos e pendentes. Canais primários ainda válidos têm prioridade;
um segundo previamente visível cede espaço para o único número de outro alvo.
Dentro dessas prioridades a ordem é: canal anterior válido, selecionado, hovered,
índice do ator, horário do evento, EventId, posição do pool.

A busca tenta oito faixas laterais, três superiores e três inferiores
(deslocamentos 0, −U(12), +U(12)), depois seis alternativas estacionárias.
Os locais superiores/inferiores ficam fora da pilha da cabeça/HP/nome do próprio
alvo. Esse detalhe evita que o feedback de um alvo vivo atravesse seu próprio HP;
não muda a posição dos textos. Para alvos sem placa, o limite é a cabeça.
As seis alternativas estacionárias limitam sua posição vertical à área segura,
incluindo a margem U(3). Podem reduzir a folga preferida em relação à pilha
de identificação, mas continuam rejeitando toda colisão com cabeças, HP, nomes,
outras reservas e associação horizontal ambígua. Não movem a identificação nem
limitam artificialmente uma trajetória animada que não cabe.

Todos os candidatos usam o mesmo canal com margem de U(3) para validar e reservar.
O canal cobre a caixa e toda a subida de U(14), ou somente a caixa no fallback
estacionário. A área segura e todas as cabeças projetadas são impedimentos
absolutos. Superiores/inferiores também exigem ao menos metade da largura sobre
a faixa horizontal da cabeça própria e centro estritamente mais próximo dela
que das demais. Empates, inclusive cabeças na mesma abscissa, são rejeitados
(tolerância U(0,01)), sem desempate por índice.

O candidato é mantido enquanto seguro. Mudanças de geometria revalidam também o
envelope entre as reservas anterior e atual. Se bloqueado, a busca reposiciona
diretamente para um canal atualmente seguro, sem interpolação por textos/rostos.
Não retorna ao primeiro candidato só porque ele ficou livre.

Identidade, cursor/filtro de eventos e `At` continuam os originais. A vida é
`Elapsed − At < 0.65`, inclusive em Loot; o fade original é preservado. Esperar por
espaço ou trocar de candidato não reinicia a vida. Pausa congela simulação;
inspeção/resize podem realocar sem mudar idade. Fases terminais e trocas reais
de identidade continuam retirando as entradas conforme o contrato anterior.

`Placement` separa `placed`, `no-safe-space`, `ambiguous-target` e `offscreen`;
`CoveragePass`, `Candidate`, `Travel` e `Reservation` permitem auditar a decisão.
Atividade ou colocação geométrica não comprovam compreensão humana. Não há
promessa de todos os números em qualquer tamanho arbitrário. A falta do único
feedback de um alvo nos casos suportados é falha, mesmo que a entrada esteja
ativa/pendente.

### Verificação reproduzível do autor

Após `scripts/Setup.ps1 -SkipAssets` e `scripts/Build.ps1`:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NumberLayout.ps1 -Screenshots -OutputDirectory artifacts/number-layout/legacy
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-NumberLayout.ps1 -Screenshots -Native -Size 1000x320 -OutputDirectory artifacts/number-layout/compact
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Presentation.ps1 -Fast -OutputDirectory artifacts/number-layout/presentation
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-EquipmentUi.ps1 -Size 1160x880 -OutputDirectory artifacts/number-layout/equipment
```

O teste novo usa simulação isolada, passos fixos 1/30 s e frames reais do engine.
Registra o primeiro kill natural do rato/actor 3/−10, todos os alvos ativos durante
o primeiro encontro, idade/retângulos/reservas, pausa/retomada, queda e encontro
seguinte. Inspeção automática muda seleção/hover/Detalhes e tamanho durante a
pausa; compara HP/nomes com e sem números. Capturas do root e recortes do HUD
incluem o Control; a textura isolada do SubViewport 3D não inclui os números.
O assert de cobertura executa também antes de encerrar o primeiro encontro.
A cura natural +36 do Guarda na transição é acompanhada até a expiração original
de 0,65 s, exigindo visibilidade, identidade e At em cada amostra. O JSON
`transitionHealing` registra também o frame expirado e as dimensões efetivas.

Fixtures sintéticas de apresentação, identificadas separadamente no JSON,
preparam dois canais livres: um segundo número existente ocupa o único canal de
um alvo novo. O teste exige cessão desse canal antes da segunda passagem e
retorno do pendente com a idade original. Também exige rejeição de empate e
abscissa igual, fallback estacionário num espaço baixo, pendência de valor largo,
pool de dez e terceiro evento substituindo o mesmo tipo sem retirar a cura.
Essas fixtures não entram na simulação de produção e não são capturas do jogo
natural nem testes de cálculo de dano.

Casos de aceite: overview root1160×880/palco1114×180 e modo de barra nativo
root1000×320/palco974×184, escala 1. O JSON sempre registra as dimensões efetivas.
A notificação de recompensa reduz o palco da barra para 974×162: esse contorno
também faz parte do aceite e é verificado pela cura na transição.
`--overview` em 1000×320 é outra composição: produz palco954×100 e menus cobrem
parte dele. Essa variante muito curta não passou no ensaio e não é apresentada
como equivalente ao modo de barra; não se reformulou a UI para suportá-la.

Os ensaios do autor não substituem GAMELAB-67 (revisão técnica), GAMELAB-68
(QA independente) ou leitura humana/DPI físico. A sequência continua sob o
Director; GAMELAB-30 permanece in_progress após a entrega parcial do código.
Preservação seletiva de VFX aliados e eventos da geração nova antes da primeira
amostragem pertencem a C3–C5 e não foram alterados incidentalmente aqui.
