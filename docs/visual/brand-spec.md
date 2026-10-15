# Aetherforge - contrato visual v3

Revisão proposta em 14/10/2026: o [estudo v5](Tipografia_e_Direcao_v5.md) compara novas referências e recomenda testar Libre Caslon Text nos títulos e Jost no texto e nos números. Essa proposta já aparece nas artes de inventário e habilidades v5; o contrato abaixo descreve o painel v3 e não identifica a tipografia gerada nas imagens v4.

O dossiê de expedição das Galerias da Caldeira organiza o estudo, com a faixa do escudo como acento contido. A v3 é full 3D planar fosco; imagens fornecidas são conceitos raster.

Fonte principal: [Color Hunt](https://colorhunt.co/palette/222831393e46948979dfd0b8), mantida por --keep palette. Origem: #222831, #393E46, #948979, #DFD0B8.

```css
:root {
 --bg: oklch(0.2749 0.0188 258.37);
 --surface: oklch(0.3623 0.0154 259.81);
 --fg: oklch(0.8635 0.0363 79.65);
 --muted: var(--fg);
 --border: var(--surface);
 --accent: oklch(0.6352 0.0268 77.34);
 --accent-ink: oklch(0.19 0.0188 258.37);
 --font-display: Archivo, 'Franklin Gothic Medium', 'Helvetica Neue', sans-serif;
 --font-body: Archivo, 'Segoe UI', Tahoma, sans-serif;
}
```

Archivo variable está local, com OFL em assets/fonts/OFL-Archivo.txt. Display: peso 800, eixo wdth 62; texto: peso 400/500, largura normal. Uma família, cinco tamanhos: 16, 18, 24, 56, 88 px no desktop, adaptados no mobile. Sem uso de monospace.

Regras:
- Corpos adultos e cansados; silhuetas por função, não por rostos e rebites.
- Planos angulares, aço fosco, lona clara no socorrista e uma faixa gasta no escudo.
- Objeto editorial preenchido e fichas com abas; sem divisores finos, labels ornamentais ou cards de marketing.
- Imagens intactas e ampliáveis; obras de referência não são recoloridas.
- Custos 3D são propostas não medidas; migração, GLBs/rigs e perfil no motor continuam pendentes.

Alternativas pesquisadas e não aplicadas:
- https://colorhunt.co/palette/181c143c3d37697565ecdfcc
- https://colorhunt.co/palette/2c36393f4e4fa27b5cdcd7c9

Contrato anterior preservado em brand-spec-v2.md.
