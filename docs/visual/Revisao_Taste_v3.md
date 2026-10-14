# Revisão com Taste Skill

Skill utilizada: [Taste Skill](https://www.tasteskill.dev/), [fonte oficial](https://github.com/Leonxlnx/taste-skill/tree/main/skills/taste-skill). Snapshot do SKILL.md e licença MIT em [referencias/taste-skill](referencias/taste-skill/SKILL.md). Scripts completos foram baixados para ferramentas locais em `artifacts/`, fora do runtime e do versionamento.

## Direção escolhida

`page-is-the-object`, seed `1337574584`, paleta preservada a partir do Color Hunt. O objeto organizador é o registro de expedição das Galerias da Caldeira, com o escudo e sua faixa desgastada como identidade dos modelos. O único recurso interativo principal é a comparação da imagem estática na escala da faixa. Isso não é um jogo 3D em execução.

Dois redraws: hero de convite incompatível com conteúdo e fotografia disponíveis; proof de convidados substituído por uma tabela de créditos/fontes. A ausência de vídeo real do projeto é justificável nesta entrega de conceitos estáticos; não criar filmagem fictícia do jogo.

## ROUND 0 — painel v2

Seen in one second: duas pranchas de peso semelhante sob um cabeçalho de documentação.

Idea (one sentence): as Galerias da Caldeira aparecem como assunto de uma página editorial, mas ainda não organizam sua composição.

Live thing: ampliação das pranchas; a escala é mostrada numa redução estática.

Layout words by section: hero=split / pranchas=grid / escala=split / referencias=grid / handoff=grid.

Scores: design 5.8 · usability 6.8 · creativity 5.2 · content 7.5 → overall 6.0.

Caps applied: teto 6.0 por falhas do inspector. Notas são avaliação interna subjetiva, não aprovação do usuário ou de um júri externo.

Top three problems (most visible first), each with the screenshot it shows in and the fix:

1. Em desktop-fold.jpg, as duas pranchas pequenas competem com texto de documentação. Tornar a cena panorâmica o assunto principal e a comparação de escala uma ação evidente.
2. Em tablet-fold.jpg e mobile-fold.jpg, rótulos e legendas pequenos, números decorativos e linhas se acumulam. Usar tamanhos de leitura, poucos níveis e superfícies preenchidas para separar conteúdo.
3. Nas pranchas v2, pintura e detalhe fino têm valor limitado para modelos 3D pequenos. A v3 reduz ruído e mostra volumes facetados, kit modular e silhuetas com função.

Checks actually run: lint da v2, com 42 ocorrências FAIL e seis WARN; inspector a 1440/768/390, com nove categorias FAIL e seis WARN. Capturas e relatório estão no diretório local de QA `artifacts/taste-v2-audit/`. Não houve alteração de runtime nesta auditoria.

## Referências e origem das imagens

Os novos conceitos são gerados com GPT Images. As imagens Dribbble de Roman Klčo e Reijo são referências de pesquisa, com autores e links no [estudo v3](Direcao_Artistica_v3.md). Os direitos pertencem aos respectivos titulares; não foram incorporadas como assets de runtime. As paletas escolhidas têm links diretos para Color Hunt. A fonte Archivo e sua licença devem acompanhar o painel caso sejam baixadas localmente.

## Crítica final

A [crítica completa do painel](critica-taste-v3.md) registra três rodadas de revisão. Resultado: lint sem findings, inspector com zero FAIL e dois WARN justificados; 79 verificações funcionais registradas por Local Codex. As larguras inspecionadas foram 1440, 768 e 390 pixels, com verificação adicional de arquivo local, ausência de JavaScript e movimento reduzido. A nota final 7.90 é autoavaliação interna de frontend, não validação externa ou aprovação do usuário.

O [estudo v3](Direcao_Artistica_v3.md) registra o que falta para produção: modelos e rigs efetivos, consistência entre vistas, composição na escala real e medição no Godot. As artes novas foram geradas com GPT Images antes do refinamento do painel. A cópia local inclui a fonte Archivo e a licença OFL.
