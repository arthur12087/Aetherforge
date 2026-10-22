# V15 — pesquisa e três tratamentos de componentes

Escopo: título Status, atributos e fichas da equipe, preservando direção B / paleta quente e tipografia V14. Entrega canônica: [Canvas V15](../../../canvas/documents/ui-v15-componentes-microinteracoes.md).

## Arquivos

- `compose.mjs` → `propostas.svg`: três alternativas estáticas, textos/valores demonstrativos iguais; A é a base do piloto, B/C não são skins implementadas.
- `research.mjs` → `pesquisa.svg`: três imagens de terceiros com fonte e restrições; não são assets do cliente.
- `propostas.html`: esboço HTML/CSS editável com os mesmos três tratamentos. A prancha entregue é composta em SVG, não uma captura deste HTML.
- `render.mjs`: servidor só em loopback e Chromium headless via CDP; checa fontes/imagens do HTML e rasteriza os SVGs autocontidos em tamanho nativo. A rasterização direta evita os tiles repetidos/pretos observados ao capturar o compositor nesta máquina. Sem geração de imagem por IA ou instalação global.
- [Pranchas e capturas selecionadas](../../../assets/concepts/brass-v15-ui/) e [manifesto de origem/SHA-256](../../../assets/concepts/brass-v15-ui/provenance.json).

## Reproduzir

Na raiz:

```powershell
node docs/visual/conceitos-v15/compose.mjs
node docs/visual/conceitos-v15/research.mjs
node docs/visual/conceitos-v15/render.mjs
```

O renderer usa o Chromium local já disponível no projeto de estudos, em `C:/Users/Arthur/AppData/Local/ms-playwright/chromium-1223/chrome-win64/chrome.exe`; pode ser substituído por `CHROME_PATH`. Não baixa um navegador automaticamente. Fontes provêm de `docs/visual/assets/fonts/v5/`, com suas licenças OFL existentes. Símbolos são os SVGs originais do projeto.

O render produz `assets/concepts/brass-v15-ui/{propostas,pesquisa}.png` e `artifacts/ui-v15/design/qa.json`. As capturas reais vêm dos testes Godot, nunca destes scripts. Se regenerar as pranchas, atualizar o manifesto antes de publicar novas hashes.

## Referências e direitos

- BioShock The Collection: https://www.shelbypeterson.com/bioshock-the-collection — atlas histórico de UI remaster de Shelby Tang, reexaminado; não atribui toda a UI original à artista.
- Frostpunk: https://interfaceingame.com/screenshots/frostpunk-book-of-laws/ — captura local histórica, reexaminada. Página reencontrada na busca; GET direto deu timeout.
- Darkest Dungeon II: https://darkestdungeon2.wiki.fextralife.com/New_Player_Help — nova imagem de ficha 300×168 baixada e inspecionada, sem upscale. Limite de evidência: organização da ficha, não tipografia, timings ou versão atual.

Direitos dos respectivos titulares. Não extrair componentes, texturas, personagens ou tokens dessas referências para runtime. A UI implementada em `BrassStatusComponents.cs` é original e nativa; não usa os PNGs da prancha como textura. Nenhum modelo/rig/animação 3D novo foi produzido nesta rodada.
