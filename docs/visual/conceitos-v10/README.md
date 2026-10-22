# V10 · correção de cor e pesquisa BioShock 2

**Pedido do usuário:** retirar o azul/petróleo dominante, usar GPT Images para a arte conceitual de heróis e bestiário, e ampliar as referências com BioShock 2.

## Entrega parcial, com bloqueio explícito

- [Prévia de cor dos menus](../../../assets/concepts/brass-v10/01-menus-cor.png): fundo papel/pedra quente, instrumentos em fuligem/tabaco, acentos vinho/latão. Pinturas e arena identificadas como pendentes. A prévia não usa maquetes substitutas.
- [Painel BioShock 2](../../../assets/concepts/brass-v10/02-pesquisa-bioshock2.png): quatro referências inspecionadas, créditos e critérios de adaptação.
- [Direção, fontes e diagnóstico](../../../canvas/documents/revisao-latao-v10.md).
- [Origem e SHA256 das referências](../../../assets/concepts/brass-v10/references/sources.json).
- [Paleta](../../../assets/concepts/brass-v10/palette.json).

**Nenhuma nova pintura GPT Images foi produzida.** Não confundir estes HTMLs/SVGs com saída de modelo de imagem. As tentativas pelo canvas para equipe e bestiário I falharam no sandbox Codex; os modelos diretos `openai/gpt-image-1` e `openai/gpt-image-1.5` foram informados como indisponíveis pela ferramenta. O segundo prompt de bestiário está preparado, não foi individualmente executado depois da falha da conexão ser reproduzida.

Os prompts completos ficam nos nós `v10-equipe-gpt`, `v10-bestiario-1-gpt` e `v10-bestiario-2-gpt`. Quando houver uma conexão GPT Images funcional em Providers, selecionar o modelo disponível e executar os nós pela geração compartilhada do canvas. Não se alteraram permissões globais ou sandbox para contornar o bloqueio.

## Fontes e reprodução da prévia

`compose.mjs` lê a composição estática v9 e escreve **novos arquivos v10**, sem sobrescrever aquela versão. Revisa as cores, retira as maquetes e identifica os espaços de pintura pendentes. Exporta também um painel novo com quatro referências BioShock 2. A prova de cor usa massas foscas. A inspeção visual detectou um artefato intermitente de repetição na captura do viewport, apesar de o DOM passar na checagem. O exportador final rasteriza o SVG completo com fontes e imagens incorporadas, em vez de depender da captura do viewport; imagens de pesquisa são decodificadas antes da composição. Gradientes/filtros também foram achatados na prova de cor. Esse processo é exportação gráfica local, não geração de arte por modelo de imagem.

```powershell
node docs/visual/conceitos-v10/compose.mjs
# Opcional: $env:CHROME_PATH = 'C:/caminho/para/chrome.exe'
node docs/visual/conceitos-v10/render.mjs
```

Requer Node com WebSocket global e Chromium local, como os geradores v8/v9. Não requer Three.js, npm ou dependência do runtime do jogo. O caminho padrão do navegador é o executável disponível nesta estação. `render.mjs` usa HTTP/CDP locais e efêmeros somente para exportar páginas estáticas.

PNGs de 2880 × 1800, exportados de 1920 × 1200 lógicos. Fontes reais Limelight/Jost e suas licenças já estão preservadas em [fonts/v5/sources.json](../assets/fonts/v5/sources.json).

## Verificação

- Quatro imagens de referência e duas pranchas abertas e inspecionadas.
- Fontes carregadas, imagens locais carregadas, rótulos dentro do viewport, sem erros JavaScript/carregamento registrados.
- PNGs com dimensões corretas, links locais conferidos e `canvas_check` sem problemas.
- Contexto de pesquisa identificado: James Ku mostra trabalho de cinemática, não orçamento de gameplay.
- Sem alteração de cliente, domínio, API, multiplayer, scripts do jogo ou saves. Não há build/teste de gameplay a executar para esta revisão de conceito.

Arte conceitual GPT, rig, animação, DPI, janelas nativas e performance de jogo continuam não entregues/não verificados nesta rodada.
