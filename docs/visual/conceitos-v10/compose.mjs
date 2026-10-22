// V10: palette correction and cited research only. Does not generate character art.
// The previous v9 maquettes are deliberately NOT presented as replacement GPT paintings.
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../../..');
const previous=path.join(dir,'../conceitos-v9');
const palette=new Map([
 ['#10272b','#bfb49e'],['#0c2025','#241f1a'],['#205055','#6c3a32'],['#526c70','#8b7b65'],
 ['#233337','#322a24'],['#b7c4bc','#ded3bf'],['#89b5a2','#97a581'],
 ['#23464b','#bfb49e'],['#0d2026','#bfb49e'],['#39666a','#57483c'],['#214b50','#40332b'],['#112a30','#2c2420'],
 ['#0a1b20','#1e1a17'],['#24464a','#3b3028'],['#000e13','#100d0b'],['#06161a','#17120f'],
 ['#3d5856','#695b48'],['#081b20','#1a1512'],['#315e61','#754c3b'],['#153339','#342923'],
 ['#4b7774','#80503d'],['#25484b','#49392d'],['#557271','#958168'],['#9ca69a','#b8aa8f']
]);
const warm=s=>s.replace(/#[a-fA-F0-9]{6}/g,c=>palette.get(c.toLowerCase())||c);
let menu=warm(await fs.readFile(path.join(previous,'03-menus.html'),'utf8'));
menu=menu.replaceAll('Aetherforge v9','Aetherforge v10')
 .replaceAll('A PROMESSA DO LATÃO · V9','A PROMESSA DO LATÃO · V10 / COR')
 .replace('Instrumentos de uma cidade que ainda funciona','Fuligem, vinho e papel')
 .replace('Porcelana, esmalte e latão. Não apenas outra borda dourada.','Nova paleta · sem azul/petróleo dominante · pinturas GPT pendentes')
 .replace('MOCKUP · SEM INTERAÇÃO IMPLEMENTADA','ESTUDO DE COR · PINTURAS GPT PENDENTES')
 .replace('<title id="title">Novos menus · A Promessa do Latão</title>','<title id="title">Revisão de cor v10 · A Promessa do Latão</title>')
 .replace('<desc id="desc">Nova composição estática autoral. Volumes locais simplificados, sem rig ou arte final aprovada. Direção B escolhida pelo usuário; formato compacto da v7 preservado.</desc>','<desc id="desc">Prévia estática da nova paleta. Fundo de papel e armações de fuligem/tabaco/vinho. Pinturas GPT Images ainda não produzidas; áreas de personagem e arena identificadas como pendentes, sem novas maquetes substitutas.</desc>');
// Empty the illustration slots. We do not claim the v9 procedural figures are GPT concepts.
menu=menu.replace(/<image\b[^>]*\bhref="[^"\n]*brass-v9\/renders\/[^"\n]+"[^>]*\/>/g,'');
menu=menu.replace(/<svg x="150" y="139"[^>]*><\/svg>/,'<text x="250" y="232" class="section center">PINTURA GPT</text><text x="250" y="261" class="small center">pendente</text>');
menu=menu.replace('<text x="1348" y="1146"','<text x="960" y="1035" class="section center arena-label">ARENA COMPACTA</text><text x="960" y="1065" class="small center arena-label">arte conceitual pendente</text><text x="1348" y="1146"');
for(const [x,y] of [[572,940],[1348,940],[1348,1146]])menu=menu.replace(new RegExp(`(<text x="${x}" y="${y}" class=")([^"]+)(")`),'$1$2 arena-label$3');
// Keep the color proof matte and avoid offscreen SVG resource artifacts in this Chromium build.
menu=menu.replace(/\sfilter="url\(#(?:shadow|relief)\)"/g,'').replace(/<svg\b[^>]*><\/svg>/g,'');
for(const [id,color] of Object.entries({back:'#bfb49e',enamel:'#40332b',brass:'#b99b63',porcelain:'#e4d8b7',slot:'#241f1a','paper-rules':'none'}))menu=menu.replaceAll(`url(#${id})`,color);
menu=menu.replace(/<defs>[\s\S]*?<\/defs>/,'');
menu=menu.replaceAll('fill="#b8aa8f"','fill="#b8aa8f" stroke="#8b7b65"');
for(const [x,y] of [[272,316],[329,262]])menu=menu.replace(`<text x="${x}" y="${y}" class="map-node center"`,`<text x="${x}" y="${y}" class="map-node map-future center"`);
await fs.writeFile(path.join(dir,'01-menus-cor.html'),menu);
let css=warm(await fs.readFile(path.join(previous,'style.css'),'utf8'));
css+='\n/* Paper outside; cream lettering stays inside each dark floating instrument. */\n.map-node.map-future{fill:#322a24}svg.root > text{fill:#322a24}svg.root > text.taskbar,svg.root > text.arena-label{fill:#e4d8b7}svg.root > g[transform^="translate(74 29)"] path{stroke:#574637}svg.root > g[transform^="translate(74 29)"] rect{fill:#bfb49e}\n';
await fs.writeFile(path.join(dir,'style.css'),css);
const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
const rect=(x,y,w,h,c,extra='')=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${c}" ${extra}/>`;
const text=(x,y,s,cls='',extra='')=>`<text x="${x}" y="${y}" class="${cls}" ${extra}>${esc(s)}</text>`;
let s=rect(0,0,1920,1200,'#bfb49e')+text(80,65,'AETHERFORGE','brand')+text(80,94,'V10 · PESQUISA DE BIOSHOCK 2','eyebrow')+text(1840,60,'Desenho antes de acabamento','page-title end')+text(1840,91,'Conceito pintado, função e contraste — não mais maquetes substitutas.','small end');
const refs=[
 ['colin-fix-big-sister.jpg','SILHUETA / FRENTE E COSTAS','Colin Fix · conceito de Big Sister',['Proporções e equipamento contam uma função.','Estudar hierarquia, não copiar capacete ou gaiola.']],
 ['colin-fix-crawler.jpg','GESTO / ASSIMETRIA','Colin Fix · Ceiling Crawler',['Pose e distribuição de massas antes da textura.','Não transplantar máscara, mutação ou personagem.']],
 ['colin-fix-mao.jpg','EQUIPAMENTO / CONSTRUÇÃO','Colin Fix · mão do jogador',['Couro, metal e tecido têm funções diferentes.','Latão pontual, articulação e desgaste localizado.']],
 ['james-ku-big-sister.jpg','MATERIAL / CINEMÁTICA','James Ku · personagem da cinemática',['Comparar pintura e tradução volumétrica.','Não é orçamento nem asset de gameplay do idle.']]
];
for(let i=0;i<refs.length;i++){const[f,title,author,notes]=refs[i],x=80+i*450;s+=rect(x,147,420,829,'#e2d8c3','rx="3"')+text(x+17,180,title,'research-label')+`<image href="../../../assets/concepts/brass-v10/references/${f}" x="${x+14}" y="201" width="392" height="613" preserveAspectRatio="xMidYMid meet"/>`+text(x+17,853,author,'research-author');notes.forEach((l,j)=>s+=text(x+17,890+j*26,l,'research-note'))}
s+=text(80,1020,'ADAPTAR: história de trabalho, assimetria, materiais sóbrios e desenho funcional.','research-summary')+text(80,1052,'PRESERVAR: Guarda com escudo retangular, Pistoleiro, Alquimista e bestiário do GDD.','research-summary')+text(80,1100,'Fontes: colinfix.blogspot.com/2009/04/more-bioshock-2-concept-art.html · jameskuart.com/bio2','research-note')+text(80,1167,'Pesquisa de terceiros · direitos dos artistas e titulares. Sem recortes para personagens, texturas ou UI Aetherforge.','research-note');
css+='\n.research-label{font-size:14px;fill:#322a24;font-weight:600;letter-spacing:.7px}.research-author{font-size:17px;fill:#322a24;font-weight:500}.research-note{font-size:16px;fill:#4c4035}.research-summary{font-size:20px;fill:#322a24}\n';
await fs.writeFile(path.join(dir,'style.css'),css);
await fs.writeFile(path.join(dir,'02-pesquisa-bioshock2.html'),`<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Aetherforge v10 · pesquisa BioShock 2</title><link rel="stylesheet" href="style.css"><body><svg xmlns="http://www.w3.org/2000/svg" class="root" viewBox="0 0 1920 1200" role="img" aria-labelledby="research-title"><title id="research-title">BioShock 2 · quatro referências inspecionadas, não assets do Aetherforge</title>${s}</svg></body></html>`);
await fs.writeFile(path.join(root,'assets/concepts/brass-v10/palette.json'),JSON.stringify({version:1,scope:'v10 proposed color correction; no generated character art',background:{name:'papel de arquivo / pedra quente',hex:'#BFB49E'},colors:{soot:'#241F1A',tobacco:'#40332B',oxblood:'#6C3A32',brass:'#B99B63',porcelain:'#E4D8B7',etherAccent:'#97A581'},dominantBlueRemoved:true,generatedCharactersProduced:false},null,2)+'\n');
console.log('V10: color preview and BioShock 2 research composed. No new character/bestiary paintings produced.');
