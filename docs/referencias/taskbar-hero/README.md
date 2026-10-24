# Taskbar Hero — dossiê de engenharia reversa

Investigação de 23/10/2026 da instalação **1.3.0**, em `C:\Program Files (x86)\Steam\steamapps\common\TaskbarHero`. Objetivo: entender o jogo e, principalmente, a organização do menu de Status para orientar uma implementação original no Aetherforge.

O resultado combina **REA/Ghidra**, leitura estruturada de recursos Unity com **UnityPy 1.24.2** e leitura de metadados **IL2CPP v31**. O executável de entrada foi analisado pelo REA; a DLL de gameplay não pôde ser decompilada pelo provedor disponível. A inspeção interativa foi bloqueada pela autorização do Computer Use. Portanto, este é um dossiê amplo da versão instalada, com lacunas explicitadas, e não uma reconstrução integral de seu código ou uma validação de todas as funções em execução.

## Por onde começar

| Documento | O que responde |
|---|---|
| [Status](Status.md) | Estrutura, estados, atributos, habilidades, pontos, detalhe, equipe e hierarquia visual |
| [Interface e fluxos](Interface.md) | Organização das janelas, Herói, inventário, Portal, HUD, tooltips e opções |
| [Sistemas](Sistemas.md) | Combate, personagens, progressão, habilidades, loot, equipamentos e sistemas adicionais |
| [Catálogo](Catalogo.md) | Inventário de tabelas, dimensões, campos e limites de interpretação |
| [Arquitetura](Arquitetura.md) | Unity/IL2CPP, classes, recursos, desktop, persistência e offline |
| [Adaptação ao Aetherforge](Adaptacao_Aetherforge.md) | Proposta concreta para o Status e etapas de implementação |
| [Evidências e reprodução](Evidencias.md) | Hashes, fontes, ferramentas, referências e roteiro de verificação |
| [Lacunas](Lacunas.md) | Cobertura por pergunta e o que falta comprovar |

## Achados que mais afetam o nosso Status

1. **Status é uma área de personagem e build.** O resumo de números fica junto de habilidades e pontos. Há estados separados para estatísticas, detalhe e equipamento de habilidades. [M01, U01]
2. **A tela abre em camadas.** Resumo compacto, lista detalhada rolável e detalhe contextual evitam colocar todos os atributos na primeira leitura. [U01, M01]
3. **A progressão tem uma estrutura visível.** Grupos de habilidades usam requisitos de pontos alocados, e cada opção tem custo e nível máximo. [D01]
4. **Herói, Status e Portal são áreas diferentes.** A cena reserva três colunas de gerenciamento acima da faixa de combate. Sua presença no mesmo Canvas não prova que sejam janelas nativas independentes do Windows. [U01]
5. **A aparência da referência pública e a cena instalada não são idênticas.** A captura histórica mostra uma lista de status larga; a versão instalada contém um topo dividido entre identificação e números. Não usar o screenshot antigo como medição da 1.3.0. [V01, U01]

## Como ler as afirmações

- **Observado no artefato:** valor, nome, estrutura ou declaração efetivamente lidos do arquivo instalado.
- **Derivado:** contagem ou relação calculada a partir desses dados; depende da fronteira documentada.
- **Inferido:** interpretação de intenção ou funcionamento, ainda sem corpo de código ou execução suficiente.
- **Proposta Aetherforge:** decisão nossa; não é comportamento atribuído ao Taskbar Hero.
- **Não resolvido:** não há evidência suficiente nesta investigação.

Os códigos `[U01]`, `[M01]`, `[D01]`, `[L01]`, `[V01]` e `[R01–R04]` são definidos no [registro de evidências](Evidencias.md). Os resumos em [evidencias/](evidencias/) acompanham a documentação. Extrações integrais e logs ficam em `artifacts/taskbar-hero-rea/current/`, ignorados pelo Git e reproduzíveis a partir da instalação local.

## Escopo no Aetherforge

A entrega inicial da pesquisa adicionou documentação e scripts opcionais de inspeção, preservando o runtime. Após a aprovação do usuário em 23/10/2026, a primeira fatia da proposta foi [implementada como Status V18](../../visual/UI_Status_v18.md): resumo, detalhe, explicações e seleção de integrante. Os sistemas futuros do catálogo não foram incorporados ao jogo. A prioridade permanece o idle/M1 descrito em [Desenvolvimento](../../Desenvolvimento.md) e [POC](../../POC_Producao.md).

As ideias de organização e interação devem ganhar arte, textos e código próprios. As tabelas e recursos extraídos são evidências da pesquisa local; não são assets adicionados ao runtime de Aetherforge.

A continuação [V19 — XP e loot da sessão](../../visual/UI_Progressao_v19.md) acrescenta nível real no Status e itens inspecionáveis no Herói, usando os valores do GDD Aetherforge. Equipamento funcional, habilidades e persistência da nova rota permanecem pendentes.
