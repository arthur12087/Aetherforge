# Prompts do estudo de reprodução visual Taskbar v7

Data: 14/10/2026. Ferramenta integrada GPT Images, sem CLI. Três gerações novas sem imagens de entrada. A referência de estrutura foi inspecionada separadamente na [galeria oficial](https://www.nugemstudio.com/en/games/taskbar-hero).

## Atlas transparente

```text
Use case: stylized-concept. Asset: crisp pixel-art atlas for a desktop taskbar idle RPG visual reconstruction study. Transparent background. Square 1024x1024 arranged in EXACT 4 by 4 equal cells, each cell 256x256; each independent object centered wholly within its cell with 20px margin. Authored classic 1990s dark fantasy RPG pixel art, deliberately limited palette 24 colors, hard black contours, visible 4x4 pixel clusters, nearest neighbor edges, no antialias, no painterly gradients, no 3D, no cinematic render, no text, labels, borders, cell dividers, letters or numbers. The game reference is Task Bar Hero's compact medieval fantasy UI and chibi pixel heroes. Generate original matching-language art, not screenshot pieces.
Top row left to right: full body stout medieval knight with silver closed helmet, red cape, small sword, shield; full body slender wizard with dark violet pointed hat and staff; full body blond healer with cream hood and green robe; hunched small green goblin enemy with axe.
Second row: straight silver sword with gold pommel; round wooden shield with steel rim; silver knight helmet; steel breastplate with brown leather straps.
Third row: steel gauntlets; brown leather boots; green gemstone ring; gold leather belt.
Fourth row: blue magic orb; silver crossed-swords skill medallion; golden healing cross skill medallion; bright blue diamond loot gem.
Keep consistent light from top-left, game sprite density, strong item silhouette. No shadow outside each object. True transparent background.
```

Resultado preservado: [28-atlas-estudo-taskbar-v7.png](conceitos/28-atlas-estudo-taskbar-v7.png).

## Arena lateral

```text
Use case: stylized-concept. Asset: 3:1 panoramic compact taskbar RPG battle scene, 1536x512. Crisp authored pixel art inspired by the actual compact grass-platform presentation of Task Bar Hero. 24-color palette with dark outlines, small visible pixel clusters, hard pixel edges, flat readable pixel sprites. No 3D, no painterly soft lighting, no glossy AI render, no text, letters, numerals, damage counters or UI.
Very short side-view grass-topped earthen platform, mossy stones and a little tree stump. On the left three small expressive chibi adventurers fighting to the RIGHT: cream hooded blond healer with green robe holding a short staff at back, violet pointed-hat wizard casting one tiny blue projectile in middle, silver closed-helmet knight with red cape, sword and round wooden shield at front. On right two green axe goblins and a small red-brown boar facing left. All six full bodies and weapons visible with room around both ends. Forest backdrop is simple layered flat green pine silhouettes with very dark purple ground shadows, nothing distracting. Characters occupy middle 55 percent height; feet on top of lower 22 percent platform. Fits a desktop companion width550 height180; compact and readable. Soft whimsical medieval fantasy, reference faithful composition; original sprite artwork. No inventory windows, no modern desktop icons or taskbar.
```

Resultado preservado: [29-arena-estudo-taskbar-v7.png](conceitos/29-arena-estudo-taskbar-v7.png).

## Mapa de região

```text
Use case: stylized-concept. Asset: parchment region-map background for a small classic pixel RPG portal-selection window, square 1024x1024. Task Bar Hero compact map screen visual study. Make a hand-authored overhead medieval fantasy FOREST map in a deliberately limited 8-tone palette of yellowed parchment, muted olive and warm sepia outlines. Small visible pixels, crisp clustered pixel lines, NO antialiasing, NO painterly blur, NO 3D relief, NO glossy decorative effects. Bare parchment margins 8 percent. Several little fir-tree groves in northwest and east, modest stone village ruins in lower-left, small rocky hills in upper-right, meandering stream at far left and one little bridge. A winding clear dirt route passes from bottom center toward top center, leaving middle 30 percent relatively empty for route nodes to be overlaid later. Cartographic scale, the world is drawn as a simple historical game map with pixel stipple hatching, readable at420x414px. Strictly NO text, letters, place names, numbers, compass, UI panels, frames, node circles, markers, flags or logos. Flat parchment substrate edge-to-edge, no photographed tabletop or open book.
```

Resultado preservado: [33-mapa-estudo-taskbar-v7.png](conceitos/33-mapa-estudo-taskbar-v7.png).

## Interface e exportação

A interface foi reconstruída como SVG/CSS, com enquadramentos dos sprites por viewBox e clipPath. As bordas, botões, títulos, células e indicadores são editáveis. O atlas tem 1254 × 1254 pixels e canal alfa; a arena tem 2172 × 724. As células do atlas não ficaram uniformes, então os enquadramentos separam personagens e equipamentos em faixas diferentes. São conceitos raster; ainda não são sprites de produção com pivôs, animações e consistência de resolução.

[Compositor](reproducao-taskbar-v7/compose.mjs) e [CSS](reproducao-taskbar-v7/telas.css). Regenerar as fontes HTML na raiz: `node docs/visual/reproducao-taskbar-v7/compose.mjs`.

Press Start 2P nos títulos, Pixelify Sans nos controles e Source Sans 3 nos textos e números. Arquivos e licenças OFL preservados em [assets/fonts/v7](assets/fonts/v7/sources.json) e [assets/fonts/v5](assets/fonts/v5/sources.json). São arquivos conhecidos usados no estudo, sem afirmar que sejam as fontes exatas do jogo de referência.

As três pranchas foram exportadas em Edge/Chromium headless, em 1920 × 1080. O navegador confirmou as três famílias locais, nenhuma linha de texto fora da tela e nenhum erro de console. O carregamento de todas as imagens foi aguardado antes das capturas; o portal foi inspecionado novamente após incorporar a arte do mapa.

## Limite de fidelidade

A composição reproduz a escala e a organização visíveis nas capturas oficiais: janelas verticais independentes, equipamentos ao redor do herói, bolsa 7 × 3, status com faixas de habilidades e portal com atos e nós. As ilustrações, os emblemas, os nomes, os valores de teste, os tooltips e o desktop neutro diferem da referência. Não é uma cópia pixel a pixel nem uma reconstrução funcional do conteúdo completo. As capturas não confirmam animações, timings, fórmulas ou todas as transições de interface. O estudo não altera o GDD nem o runtime de Aetherforge.
