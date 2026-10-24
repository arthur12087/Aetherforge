# Estudos V16 — quatro estruturas de interface

Exploração de layout após a preferência do usuário pela **B — Ficha de campo** e a autorização para mudanças mais criativas. A direção geral continua **A Promessa do Latão**. Nada foi aplicado ao cliente; o runtime permanece V15 A.

- [Documento canônico e pesquisa](../../../canvas/documents/ui-v16-layouts-exploratorios.md).
- [Galeria local](index.html).
- [Layouts, dimensões e valores compartilhados](layouts.json).
- [Fontes de pesquisa, direitos e SHA-256](sources.json).
- [Proveniência dos arquivos finais](../../../assets/concepts/brass-v16-ficha/provenance.json).

## Saídas

| Arquivo em `assets/concepts/brass-v16-ficha/` | Conteúdo | Dimensões |
|---|---|---|
| `01-dossie.png` | Continuidade editorial da ficha B | 1500 × 1250 |
| `02-caderno.png` | Herói em duas páginas; Status à margem | 1500 × 1250 |
| `03-trilho.png` | Equipe e rota em faixas horizontais | 1500 × 1250 |
| `04-arquivo.png` | Retrato dominante e assimetria | 1500 × 1250 |
| `comparacao.png` | Montagem reduzida das quatro propostas | 2000 × 1900 |
| `pesquisa-menus.png` | Quatro menus inspecionados | 1500 × 1250 |
| `pesquisa-hud.png` | Três HUDs e recortes de análise | 1500 × 1250 |

`references/` preserva os oito downloads originais do Interface In Game; não são assets de runtime ou recursos liberados para distribuição comercial. O registro de fontes mantém URLs, titulares, dimensões e hashes. Dribbble tem somente links/descrições indexadas, sem imagem visualmente verificada (HTTP 202 vazio no acesso direto).

## Reprodução local

Node.js e Chromium já disponíveis no ambiente. Não é necessária instalação global de ferramentas ou fontes.

```powershell
node docs/visual/conceitos-v16/compose.mjs
node docs/visual/conceitos-v16/research.mjs
node docs/visual/conceitos-v16/render.mjs
node docs/visual/conceitos-v16/audit.mjs
```

`compose.mjs` chama `crops.ps1` (System.Drawing no Windows), produz seis recortes de referência em pixels nativos, quatro SVGs e `layouts.json`. `research.mjs` produz duas pranchas e `sources.json`, usando os originais locais (não baixa novamente a internet). `render.mjs` usa `canvas-raster.js` para reproduzir as primitivas vetoriais em Canvas2D, monta a comparação e verifica a galeria. `audit.mjs` registra hashes/dimensões/proveniência e verifica vínculos/invariantes sem executar o jogo.

Chromium padrão:

```text
C:/Users/Arthur/AppData/Local/ms-playwright/chromium-1223/chrome-win64/chrome.exe
```

Pode ser substituído pela variável `CHROME_PATH`. O exportador abre apenas servidor **loopback**, escolhe portas livres e cria seu próprio perfil em `artifacts/ui-v16/design/`. Só encerra o processo que iniciou. Não se conecta a sessões do navegador do usuário. Não usa screenshot do compositor nem SVG inteiro como imagem aninhada: `canvas-raster.js` reproduz apenas nossas primitivas suportadas (texto, retângulos, paths e imagens decodificadas) em Canvas2D e rejeita outras. Uma tentativa inicial de rasterização direta do SVG produziu placeholders intermitentes; esperar após `decode()` não bastou. O replay explícito eliminou essa dependência na entrega final.

QA local: `artifacts/ui-v16/design/{render-qa.json,gallery-qa.json,final-audit.json}`. A verificação de limites de texto é da prancha, **não** prova de hitboxes/foco/ergonomia no Godot. Os estudos são composições absolutas para revisão; uma implementação precisará de Containers/anchors, medidas reais, foco, escala e testes próprios.

## Materiais reutilizados e limites

- Fontes locais: [Source Sans 3 / OFL](../assets/fonts/v5/sourcesans3-OFL.txt) e [Limelight / OFL](../assets/fonts/v5/limelight-OFL.txt), sem instalação global. Source Sans 400/600 nos dados; Limelight na identidade.
- Símbolos: os sete SVGs originais do projeto em `src/Aetherforge.Client/Assets/Brass/Icons/`, sem importar símbolos de terceiros.
- Personagens: folha v12, gerada historicamente fora desta rodada e aceita como base de trabalho, não arte final. Os estudos usam somente recortes proporcionais. Seis recortes locais em `derived/` evitam dependência de clipping aninhado; originais não são modificados. `crops.ps1` registra os retângulos e não redimensiona os pixels. Não houve repintura ou novo modelo.
- Cena: recorte `0,62,1000,210` da captura V13 em 1000 × 320, em escala igual ou menor que a original. O rail em volta é um mockup, não o HUD V16 implementado.
- Valores demonstrativos iguais; bolsa 7 × 3 indisponível, três integrantes e cinco etapas. Sem progressão/itens/dinheiro/energia inventados.
- Sem geração de imagem por IA nesta rodada. As formas, textos, hierarquia e montagens novas são código vetorial autoral. Referências de jogos têm seus próprios direitos; pesquisa não concede licença.
- Não foram executados build, testes do jogo ou novos agentes/Studio. Sem alteração do domínio, cliente, saves, multiplayer ou integração. Sem novo commit/push.
