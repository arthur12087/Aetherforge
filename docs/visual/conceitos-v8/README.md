# Aetherforge v8 · conceitos de menus

Três **composições estáticas** para comparar a adaptação da organização v7 de Taskbar Hero ao Aetherforge. Não há jogo, navegação, modelos 3D ou regras implementadas nesta pasta.

## Entrega

- [A — Fuligem & Decreto](../../../assets/concepts/idle-v8/a-fuligem-decreto.png): leitura de menus de Dishonored; placas recortadas, tinta clara e acento de lacre.
- [B — A Promessa do Latão](../../../assets/concepts/idle-v8/b-promessa-latao.png): leitura de BioShock; chassi escalonado, encaixes e hierarquia Art Déco.
- [C — O Último Turno](../../../assets/concepts/idle-v8/c-ultimo-turno.png): leitura de Frostpunk; aço frio, níveis, nós e seleção em âmbar.
- [Painel de referências dos menus](../../../assets/concepts/idle-v8/menus-referencias.png): cinco capturas de terceiros com critérios de adaptação e créditos.
- [Brief do diretor](../../../canvas/documents/direcao-idle-v8.md) e [pesquisa dos menus](../../../canvas/documents/pesquisa-menus-v8.md).

Os quatro PNGs medem **2880 × 1620**, exportados de composições em 1920 × 1080 pixels lógicos. Os dados de nível, atributos, ranks e equipamento são demonstrativos. Os ranks/marcos são continuidade do estudo visual v7, não regras novas aprovadas. A rota usa as cinco etapas das Galerias da Caldeira, conforme o GDD.

## Proveniência e limite importante

A geração de novas bases artísticas foi tentada pelo canvas e pela ferramenta alternativa, mas falhou no sandbox Windows antes de produzir imagens (`elevated Windows sandbox requires effective :root read access`). Os três prompts de nova arte permanecem no canvas com o prefixo **PENDENTE**. A e a tentativa alternativa foram executadas; B e C ficaram preparados, sem chamada individual após a confirmação do bloqueio de ambiente.

As pranchas finais **não são resultados desses nós**: usam os conceitos v6 de Guarda, equipamentos e arena, preservados em `assets/concepts/idle-v8/bases/`. A composição, os ícones vetoriais, o diagrama industrial da rota, as molduras e a camada tipográfica são novos. Os três ajustes de cor são contidos, sem alegar três trabalhos novos de modelagem ou três novas pinturas geradas.

As capturas de Dishonored, BioShock e Frostpunk aparecem somente na pesquisa. Não foram usadas como textura, recorte ou fundo dentro dos mockups do nosso jogo. Seus direitos pertencem aos titulares.

## Tipografia

- A: Libre Caslon Text + Jost.
- B: Limelight + Jost.
- C: Bodoni Moda + Source Sans 3.

São arquivos OFL reais já disponíveis em [fonts/v5](../assets/fonts/v5/sources.json). As letras não foram desenhadas pelo gerador nem extraídas dos jogos de referência. As famílias são alternativas de direção, não uma afirmação de equivalência com as fontes comerciais dos jogos.

## Fontes de composição e exportação

- `compose.mjs` escreve os HTML/SVG e o CSS. Os HTMLs são pranchas estáticas editáveis, não um protótipo de navegação.
- `render.mjs` usa Chromium via DevTools, sem dependências npm. É possível fornecer um executável local por `CHROME_PATH`; o caminho padrão aponta para o Chromium já disponível no ambiente da entrega.

```powershell
node docs/visual/conceitos-v8/compose.mjs
node docs/visual/conceitos-v8/render.mjs
```

A exportação usa perfil isolado e grava evidências em `artifacts/concepts-v8/qa.json`, ignorado pelo Git. Não lê nem altera saves.

## Verificação

- Inspeção visual das referências, das bases reaproveitadas e das pranchas exportadas.
- Fontes locais efetivamente carregadas: Caslon/Jost, Limelight/Jost, Bodoni/Source.
- Imagens locais carregadas sem falha; nenhuma exceção de JavaScript ou erro de carregamento registrado no Chromium.
- 81 rótulos SVG por proposta, sem transbordamento do viewport de 1920 × 1080.
- Após a primeira inspeção: retrato recomposto para não esconder os pés sob a formação; ornamento Art Déco afastado do fechar; linhas de níveis afastadas dos ranks; contraste do acento textual de A aumentado para aproximadamente 4,83:1 contra a superfície do painel.
- Arquivos, IDs, documentos, referências e caminhos verificados com `canvas_check`.

Ainda não verificados: interface nativa no Windows, DPI, contraste de todos os estados em uso real, animações, equivalência funcional com Taskbar Hero, modelagem 3D específica de cada direção e desempenho no Godot. Cliente, domínio, API, integração multiplayer e save manual foram preservados. Alterações exclusivamente de conceitos/documentação não exigem recompilar o jogo.
