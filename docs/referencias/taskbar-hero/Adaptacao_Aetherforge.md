# Proposta de Status para Aetherforge

[Índice](README.md) · [Status da referência](Status.md). Este documento descreve o **design aprovado pelo usuário em 23/10/2026**, sem atribuir suas decisões ao Taskbar Hero. A primeira fatia está [implementada como Status V18](../../visual/UI_Status_v18.md); os elementos futuros continuam identificados abaixo.

## Direção

Transformar o Status em uma ficha compacta do personagem selecionado: identificação, poucos atributos úteis e acesso rápido ao detalhe. Quando as regras de progressão estiverem implementadas, acrescentar habilidades e pontos na mesma hierarquia. A inspiração principal é a relação entre resumo, detalhe e escolha de build; a aparência deve continuar original, com o tema industrial e as fontes já adotadas pelo projeto.

A V17 usava um dossiê com régua e fichas de equipe. Ela foi o ponto de partida da proposta, posteriormente aprovada pelo usuário. A V18 substitui a régua do Status por resumo/detalhe, mantém os outros instrumentos e a linguagem original do projeto.

## Composição proposta

```text
┌ STATUS ────────────────────────────────── fechar ┐
│ [retrato] Nome / função          [anterior][próx] │
│           Nível / EXP, quando reais              │
├─────────────────────────────────────────────────┤
│ Vida atual / máxima                barra curta  │
│ Dano                 valor   Armadura     valor  │
│ Intervalo de ataque  valor   Cura         valor  │
│ [Estatísticas detalhadas]                       │
├─────────────────────────────────────────────────┤
│ [Retrato tank] [Retrato DPS] [Retrato healer]     │
├─────────────────────────────────────────────────┤
│ Futuro: pontos disponíveis e habilidades        │
│ grupos por investimento → detalhe da escolhida  │
└─────────────────────────────────────────────────┘
```

Os campos ilustram a organização, não uma lista já aprovada de regras. Mostrar somente métricas com significado e origem confirmados no domínio. EXP, pontos e árvore de habilidades não devem aparecer como funcionalidades disponíveis enquanto não existirem. Dados fictícios só cabem em uma prancha de estudo identificada como tal.

## Responsabilidade de cada tela

| Tela | Pergunta principal | Conteúdo proposto |
|---|---|---|
| Status | Como este integrante está e por quê? | Vida, atributos, modificadores; progressão quando implementada |
| Herói | O que está equipado e guardado? | Modelo/retrato, slots e bolsa; alterações reais de equipamento |
| Expedição | Onde a equipe está e para onde vai? | Etapa, inimigos, chefe, derrota/recuo e ações da rota |

Selecionar um integrante deve atualizar Status e Herói de forma consistente. O alvo selecionado deve permanecer identificável ao abrir o detalhe. A equipe não deve ser confundida com uma estatística agregada: cura de um integrante, vida de outro e bônus coletivos exigem rótulos próprios.

## Camadas de informação

| Camada | Conteúdo | Regra de leitura |
|---|---|---|
| Resumo | Até seis métricas úteis para o papel selecionado | Nomes à esquerda, valores alinhados à direita; unidade explícita |
| Detalhe | Ataque, defesa, suporte/utilidade | Lista rolável, ordem estável e explicação acessível |
| Composição | Valor base, equipamento, bônus/penalidade e resultado | Abrir ao selecionar atributo; não depender só de hover |
| Progressão futura | Pontos, opções e requisitos | Disponibilidade real, custo e resultado antes da ação |

O DPS precisa de definição própria. Se exibirmos dano teórico do ataque básico, usar esse rótulo. Se houver uma medição de combate, informar janela de tempo e o que entra no cálculo. Uma única sigla não deve misturar estimativa, cura e dano efetivamente causado.

## O que trazer e o que adiar

| Ideia da referência | Aplicação em Aetherforge | Etapa |
|---|---|---|
| Resumo compacto + detalhe | Novo corpo do Status com um comando de aprofundamento | Próximo incremento visual autorizado |
| Personagem selecionado como contexto | Retrato/nome estáveis, seleção compartilhada entre telas | Mesmo incremento |
| Seções de atributos | Ataque, defesa e suporte com nossos atributos reais | Mesmo incremento, se houver dados suficientes |
| Slots e tooltips de habilidades | Ícones próprios e descrição acionável | Após regras de habilidades/progressão |
| Pontos e grupos por investimento | Requisitos e custos próprios testados no domínio | Extensão de IDL-02, quando autorizada |
| Resumo de ganhos offline | Período, ganhos e deltas por integrante | UI-02 após consolidação do ciclo e salvamento |
| Runa, Cubo, relíquia e mercado | Avaliar como referências de sistemas futuros | Não entram no próximo menu/M1 por esta pesquisa |

Os requisitos 0/10/20…70 do Taskbar Hero explicam sua árvore distribuída; não são uma sugestão de balanceamento para nosso jogo. Da mesma forma, suas raridades, quantidades de fases e milhares de itens não definem escopo de Aetherforge.

## Estados de interação propostos

| Estado | Ação | Resultado esperado |
|---|---|---|
| Resumo | Selecionar integrante | Atualiza identidade, valores e seleção visível |
| Resumo | Abrir detalhe | Exibe seções; preserva contexto do integrante |
| Detalhe | Selecionar atributo | Explica unidade, composição e limites conhecidos |
| Detalhe | Voltar | Retorna ao resumo; mantém seleção |
| Qualquer subestado | Fechar Status | Fecha apenas o painel; expedição continua |
| Progressão futura | Selecionar habilidade | Mostra nível atual/próximo, custo e requisito |
| Progressão futura | Confirmar melhoria | Debita pontos uma vez e atualiza efeitos reais |

Teclado: foco inicial previsível, navegação por Tab, ativação por Enter/Espaço e Escape voltando uma camada antes de fechar. Esses são requisitos nossos; não foram verificados no Taskbar Hero. Tooltips devem ter uma alternativa por foco/seleção para quem não usa mouse.

## Organização técnica proposta

- **Domain:** valores e regras reais, composição de bônus, requisitos/custos e consequências de progressão. Continuar compatível com `netstandard2.1`, sem tipos Godot.
- **Client:** catálogo de apresentação com rótulo localizado, unidade, ordem, grupo, precisão e regra de ocultação; Control/Container e componentes de interação.
- **Apresentação do personagem:** objeto de leitura com o integrante selecionado, seus valores e origem; não fazer cada linha consultar a cena ou recalcular combate.
- **Atualização:** mudança de seleção, equipamento, progressão ou vida atualiza as partes afetadas; animação do feedback visual não altera a regra.
- **Persistência:** só mudar formato de save quando existir uma regra nova que exija isso; não acoplar a reorganização visual a uma migração desnecessária.

Uma definição de apresentação pode usar `id`, `grupo`, `ordem`, `rótulo`, `unidade`, `precisão`, `mostrarZero` e `ajuda`. A identidade e semântica dos atributos vêm do domínio; localização, layout e tooltip pertencem ao cliente. Essa é uma proposta original inspirada nos campos de `StatViewInfoData`, não uma transcrição de sua implementação.

## Critérios para implementar

1. Os três integrantes são selecionáveis; nome, retrato, valores e detalhe sempre correspondem ao mesmo alvo.
2. As métricas visíveis têm origem verificável nas regras atuais e unidades compreensíveis.
3. Resumo, detalhe e retorno funcionam por mouse e teclado; foco permanece visível.
4. O painel passa nos tamanhos e escalas 100/125/150% já usados pelo projeto; áreas curtas rolam sem esconder o fechar ou capturar o input da expedição indevidamente.
5. Cor complementa texto/ícone; não é o único sinal de bônus, perda, bloqueio ou seleção.
6. A opção de reduzir movimento existente continua respeitada.
7. A expedição continua enquanto o painel abre/fecha; a navegação não modifica equipe ou equipamento sem uma ação explícita.
8. Build, verificações de comportamento pertinentes e integração existente passam quando houver alteração de runtime. Usar saves isolados em `artifacts/`.
9. Nenhuma interface de habilidade/ponto/EXP sugere uma funcionalidade inexistente.

## Próximo pacote concreto

A primeira entrega **Status resumido + detalhe de atributos + seleção de integrante** foi implementada com dados atuais como [V18](../../visual/UI_Status_v18.md). A árvore de habilidades continua sendo uma segunda entrega que depende de regras próprias. A aprovação do desenho não transforma os sistemas futuros desta pesquisa em funcionalidades já disponíveis nem amplia automaticamente o M1.

Na continuação autorizada, a [V19](../../visual/UI_Progressao_v19.md) entrega XP/nível reais, crescimento e loot básico da sessão. A barra de XP deixa de ser uma proposta; os pontos e a árvore de habilidades continuam futuros. O Herói permite inspecionar itens recebidos, ainda sem equipá-los ou aplicar bônus. Reiniciar a rota conserva progresso local; fechar a execução o perde.
