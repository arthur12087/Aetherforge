# Validação do protótipo técnico M0

12/10/2026. A base do protótipo está implementada. A aprovação completa de TEC-02 ainda depende da matriz de comportamento do desktop.

Os caminhos `artifacts/` citados abaixo são saídas locais, ignoradas pelo Git. Após um clone, execute os scripts de teste e medição na raiz para gerar novas evidências; os números abaixo registram a execução original.

## Implementado e verificado

| Item | Resultado | Evidência |
|---|---|---|
| TEC-01: linguagem e runtimes | Cliente net8.0, API net10.0 e domínio netstandard2.1 compilam sem avisos ou erros | `scripts/Build.ps1`; Godot 4.7.2 .NET e SDK 10.0.401 |
| ART-00: assets | Três modelos provisórios e licença CC0 preservada | `Assets/Originals/KayKit`, catálogo em Referencias_M0.md |
| ART-01: sprites direcionais | Três atlas com oito direções e animações idle, caminhar e atacar/curar | `Assets/Generated`; baker executado no Godot |
| TEC-02: janela básica | Modo barra de 812 × 290 e janela de 812 × 382, incluindo a faixa de loot; transparência disponível; flag de não roubar foco no modo barra | `LAYOUT PASS` no teste; captura `artifacts/idle-m0.png` |
| NET-01: conexão real | Dois processos de cliente recebem snapshots e veem deslocamento do outro | `NETWORK PASS` em `artifacts/network-one.log` e `network-two.log` |
| Autoridade e colisão | Clientes enviam direção; servidor calcula posição. Um bot tenta cruzar a parede e fica bloqueado | `bounds_valid=True, wall_collision=True` nos logs |
| API de teste | `/health` responde, `/prototype/team` retorna três funções e `/integration/state` mostra o equipamento | Scripts de teste, API em loopback |
| Regras demonstrativas | Tank recebe ataques, healer cura, HP respeita limites e encontros reiniciam | Console de verificações; não representa ainda o combate completo do GDD |
| Visual de combate | Tank avança e golpeia, DPS dispara, healer lança cura, inimigo reage e contra-ataca; equipe caminha entre encontros | Captura e execução do idle; sprites de ação e efeitos desenhados em tempo real |

Os testes de rede usam ENet de verdade, com dois clientes e um servidor separados. A direção recebida é normalizada; entradas não finitas são rejeitadas. Os endpoints de teste e o servidor escutam apenas na própria máquina. O M0 não implementa tickets de acesso ou autenticação de produção.

## Consumo e orçamento visual

O relatório mais recente em `artifacts/idle-performance.json` mede o processo real do engine, excluindo o pequeno wrapper de console. A medição realizada é de cerca de um minuto em build de desenvolvimento, limitada a 30 FPS. O script suporta dez minutos, mas a amostra curta não fecha o critério de estabilidade de dez minutos.

Na versão com ataques e cura animados, a amostra de 60,88 segundos registrou pico de 235,66 MiB de memória residente e média de CPU de 0,366% da máquina. Os dois valores ficam dentro das metas iniciais nessa amostra. Ainda é necessário confirmar dez minutos e outras configurações de hardware. A correção posterior das barras de vida altera apenas seu desenho; esses números não são uma nova medição dessa correção.

PC de referência: Windows 11 Pro, build 26200, 16 processadores lógicos. O renderizador de execução identificou a NVIDIA GeForce RTX 4070 SUPER; o sistema também lista AMD integrada e adaptador virtual Parsec. Não extrapolar consumo desse computador para GPUs integradas ou outros drivers.

Cada atlas tem 1536 × 1152 pixels, 96 quadros por personagem em células de 128 × 144. RGBA8 equivale a 6,75 MiB por atlas sem compressão ou mipmaps, 20,25 MiB para os três. Esse número é um orçamento de textura, não a memória total do processo.

## Ainda pendente para fechar TEC-02

- DPI 100%, 150% e 200%, em sessões configuradas nessas escalas.
- Monitor principal e secundário, mudança de monitor durante a execução e desconexão de monitor.
- Barra com ocultação automática e mudanças de área de trabalho.
- Clique nas áreas transparentes e preservação do foco durante digitação real em outro aplicativo. O teste atual verifica flags, não toda a interação nativa.
- Convivência com outro jogo em tela cheia e retorno após suspensão do Windows.
- Medição de dez minutos e exportação release. O protótipo atual usa o executável de desenvolvimento Godot.

Nenhuma escala de tela ou preferência do Windows foi alterada automaticamente para simular essa matriz.

## Próximo incremento

O incremento de [equipamento compartilhado](Teste_Equipamento.md) já adicionou combate autoritativo demonstrativo e persistência de uma armadura, transferência, equipar e mitigação de dano. Isso ainda não implementa as rotas, XP, durabilidade, economia geral ou offline do GDD. M1 ampliará o combate por eventos, habilidade/energia, equipamento e progressão. A amostra de desempenho acima é anterior à integração HTTP e à faixa de loot; não mede este incremento.
