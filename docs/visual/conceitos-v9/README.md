# A Promessa do Latão · produção conceitual v9

**Base escolhida pelo usuário:** B, inspirada em BioShock. O conteúdo novo desta rodada continua sujeito à avaliação do usuário.

## Pranchas

1. [Nova equipe](../../../assets/concepts/brass-v9/01-equipe.png) — Guarda, Pistoleiro e Alquimista.
2. [Novo bestiário](../../../assets/concepts/brass-v9/02-bestiario.png) — seis propostas de aparência para os inimigos da rota.
3. [Novos menus](../../../assets/concepts/brass-v9/03-menus.png) — Mesa de Regulagem, Estojo de Expedição e Telégrafo de Rotas.
4. [Identidade e estilização](../../../assets/concepts/brass-v9/04-assinatura.png) — Arco Partido, medalhões, gavetas, paleta e leitura reduzida.
5. [Pesquisa de BioShock](../../../assets/concepts/brass-v9/05-referencias.png) — fontes dos portfólios de Shelby Tang e Robb Waters.

PNGs de **2880 × 1800**, compostos em **1920 × 1200 pixels lógicos**. [Direção](../../../canvas/documents/promessa-latao-v9.md) · [pesquisa e fontes](../../../canvas/documents/pesquisa-bioshock-v9.md).

## O que é novo e o que não é

A nova chamada de geração de pintura pelo canvas falhou antes de produzir uma imagem: `elevated Windows sandbox requires effective :root read access`. O nó `v9-herois-geracao` fica identificado como pendente, com seu prompt preservado. Não houve geração de pintura por IA bem-sucedida nesta rodada.

As pranchas entregues usam **novas maquetes volumétricas originais**, construídas localmente em código e renderizadas com Three.js, além de composição vetorial e fontes reais. **Não reutilizam os personagens ou os equipamentos v6.** Nenhuma geometria, textura ou pixel dos personagens de BioShock foi usado nos modelos ou nos menus Aetherforge. As cinco imagens de terceiros aparecem somente na pesquisa.

As maquetes representam forma, silhueta, equipamento e paleta. Anatomia, roupa e expressão facial são deliberadamente simplificadas. Não são escultura AAA, pintura final, modelos de produção, GLBs entregues, rigs, malhas skinned ou personagens prontos para animação. Em particular, as roupas ainda são volumes rígidos e as mãos não foram solucionadas como anatomia final.

O símbolo Arco Partido, os ícones das três habilidades, as gavetas e os diagramas de rota são novos. As fontes são Limelight e Jost, arquivos OFL já preservados em [fonts/v5](../assets/fonts/v5/sources.json), não fontes comerciais extraídas de BioShock.

## Conteúdo das fontes editáveis

- `models.js`: nove fábricas de volumes originais e um palco modular estático, eixos +Y para cima / +Z para frente.
- `volume.html`: cena e câmera estáticas; sem loop de jogo, input de gameplay ou animações.
- `render.mjs`: servidor HTTP efêmero e Chromium/CDP apenas para exportação de imagens e evidências.
- `crops.mjs`: mede limites de alpha dos nove PNGs com módulos nativos Node; não estima qualidade artística.
- `compose.mjs`: escreve as cinco composições HTML/SVG e o CSS.
- `design-spec.json`: objetivos, contrato de aproximação e critérios desta rodada.

Todos são geradores/documentos de conceito, separados de `src/`. Não há dependência nova no cliente Godot ou no domínio.

## Reproduzir

Node 22+ e Chromium local são necessários. A dependência Three.js **0.180.0**, licença MIT, fica em `.tools/concept-v9/three/`, ignorada pelo Git. Ela não é incorporada ao runtime do jogo.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File docs/visual/conceitos-v9/Setup-Concept.ps1
# Opcional: $env:CHROME_PATH = 'C:/caminho/para/chrome.exe'
node docs/visual/conceitos-v9/render.mjs volumes
node docs/visual/conceitos-v9/crops.mjs
node docs/visual/conceitos-v9/compose.mjs
node docs/visual/conceitos-v9/render.mjs pages
```

O caminho padrão de Chromium corresponde ao executável já disponível no ambiente desta entrega; em outro PC, fornecer `CHROME_PATH`. Perfil e relatórios ficam em `artifacts/concepts-v9/`, sem tocar no save manual. Os servidores são locais e temporários, encerrados ao terminar a exportação.

## Verificação e revisão

- Referências, renders e cinco pranchas inspecionados visualmente.
- Nove modelos capturados em três quartos; Guarda, Pistoleiro, Alquimista e Capataz também capturados em 110°, 200° e 290° para evidência de lados e costas.
- Revisão levou à correção de encaixe do pescoço, tamanho do rifle, orientação da arma no palco, posição das nervuras do peitoral e contraste do ensaio de silhuetas.
- Limites de alpha medidos: nenhum dos nove modelos chega à borda da imagem isolada. Isso não comprova ausência de interseções internas.
- Fontes locais Limelight/Jost carregadas, imagens sem falha, contexto WebGL preservado, sem exceções JavaScript ou erros de carregamento registrados na exportação final.
- Rótulos das composições conferidos quanto ao limite do viewport. A interface compacta foi inspecionada como mockup, não executada como janela nativa.
- JSONs, IDs, caminhos de mídia e referências do canvas validados com `canvas_check`.

Contagens geométricas são características destas **maquetes**, não um orçamento aprovado de produção. O palco de estudo tem cerca de 32 mil triângulos antes de qualquer migração ou rig; isso não mede draw calls, CPU, GPU, transparência ou consumo no Godot.

### Ainda não verificado

Rig, deformação, animações, colisões, topologia final, UVs, texturas autorais definitivas, custo no Godot, DPI, janela transparente, áudio, estados interativos completos e comportamento de menus. A prova reduzida indica intenção de contraste, não aprovação de acessibilidade em uso real.

Não foram executados build/testes de gameplay porque nenhum arquivo do cliente, domínio, API, persistência ou save foi alterado. A integração multiplayer existente está preservada.
