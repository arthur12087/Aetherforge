// Reference-only boards. Third-party screenshots stay outside Godot resources.
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(dir,'../../..');
const base='assets/concepts/brass-v16-ficha/references/';
const entries=[
 ['nier-items','NieR:Automata / Items','nier-items.jpg','https://interfaceingame.com/screenshots/nierautomata-items/','https://interfaceingame.com/wp-content/uploads/nierautomata/nierautomata-items.jpg','PlatinumGames / Square Enix'],
 ['nier-hud','NieR:Automata / HUD','nier-hud.png','https://interfaceingame.com/screenshots/nierautomata-dialoge-options-2/','https://interfaceingame.com/wp-content/uploads/nierautomata/nierautomata-dialoge-options.png','PlatinumGames / Square Enix'],
 ['rdr-stable','Red Dead Redemption 2 / Stable','rdr-stable.png','https://interfaceingame.com/screenshots/red-dead-redemption-2-stable/','https://interfaceingame.com/wp-content/uploads/red-dead-redemption-2/red-dead-redemption-2-stable.png','Rockstar Games'],
 ['rdr-hud','Red Dead Redemption 2 / HUD','rdr-hud.png','https://interfaceingame.com/screenshots/red-dead-redemption-2-hud/','https://interfaceingame.com/wp-content/uploads/red-dead-redemption-2/red-dead-redemption-2-hud.png','Rockstar Games'],
 ['rdr-progress','Red Dead Redemption 2 / Progress','rdr-progress.jpg','https://interfaceingame.com/screenshots/red-dead-redemption-2-progress/','https://interfaceingame.com/wp-content/uploads/red-dead-redemption-2/red-dead-redemption-2-progress.jpg','Rockstar Games'],
 ['outer-inventory','The Outer Worlds / Inventory','outer-inventory.png','https://interfaceingame.com/screenshots/the-outer-worlds-inventory/','https://interfaceingame.com/wp-content/uploads/the-outer-worlds/the-outer-worlds-inventory.png','Obsidian Entertainment / Private Division'],
 ['outer-hud','The Outer Worlds / HUD','outer-hud.png','https://interfaceingame.com/screenshots/the-outer-worlds-hud-2/','https://interfaceingame.com/wp-content/uploads/the-outer-worlds/the-outer-worlds-hud-2.png','Obsidian Entertainment / Private Division'],
 ['dishonored-powers','Dishonored 2 / Powers','dishonored-powers.jpg','https://interfaceingame.com/screenshots/dishonored-2-powers/','https://interfaceingame.com/wp-content/uploads/dishonored-2/dishonored-2-powers.jpg','Arkane Studios / Bethesda Softworks']
];
const bytes={};for(const [id,,file]of entries)bytes[id]=await fs.readFile(path.join(root,base+file));
const data=b=>`data:image/${b[0]===137?'png':'jpeg'};base64,${b.toString('base64')}`;
const source=await fs.readFile(path.join(root,'docs/visual/assets/fonts/v5/SourceSans3-Variable.ttf'));
const display=await fs.readFile(path.join(root,'docs/visual/assets/fonts/v5/Limelight-Regular.ttf'));
const C={paper:'#BFB49E',soot:'#241F1A',tobacco:'#40332B',wine:'#6C3A32',brass:'#B99B63',linen:'#E4D8B7'};
let s='';const esc=t=>String(t).replaceAll('&','&amp;').replaceAll('<','&lt;');
const text=(x,y,t,size=16,color=C.soot,font='Source',weight=400)=>s+=`<text x="${x}" y="${y}" font-family="${font}" font-size="${size}" fill="${color}" font-weight="${weight}">${esc(t)}</text>`;
const rect=(x,y,w,h,c)=>s+=`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${c}"/>`;
function begin(title,sub){s=`<svg xmlns="http://www.w3.org/2000/svg" width="1500" height="1250" viewBox="0 0 1500 1250"><defs><style>@font-face{font-family:Source;src:url('data:font/ttf;base64,${source.toString('base64')}');font-weight:200 900}@font-face{font-family:Limelight;src:url('data:font/ttf;base64,${display.toString('base64')}')}</style></defs>`;rect(0,0,1500,1250,C.paper);text(48,36,'AETHERFORGE / PESQUISA V16 / REFERÊNCIAS DE TERCEIROS',13,C.soot,'Source',600);text(48,85,title,34,C.soot,'Limelight');text(48,117,sub,18);}
function img(id,x,y,w,h,roi=null){rect(x,y,w,h,C.soot);if(roi)s+=`<svg x="${x}" y="${y}" width="${w}" height="${h}" viewBox="${roi.join(' ')}" overflow="hidden"><image width="1920" height="1080" href="${data(bytes[id])}"/></svg>`;else s+=`<image x="${x}" y="${y}" width="${w}" height="${h}" preserveAspectRatio="xMidYMid meet" href="${data(bytes[id])}"/>`;}
begin('Referências para compor, não para copiar.','Quatro menus inspecionados: separação de funções, foco visual e diferentes relações entre retrato e informação.');
const menu=[
 ['nier-items','01 / NieR:Automata — dados sem caixas pesadas',['OBSERVAR  Lista, detalhe e status têm papéis distintos.','ADAPTAR  Régua comum, divisórias finas e seleção preenchida.','EVITAR  Copiar padrões, tipografia ou esconder o HP.']],
 ['rdr-stable','02 / Red Dead Redemption 2 — foco e contraste',['OBSERVAR  Uma lista lateral; a imagem domina o restante.','ADAPTAR  Hierarquia forte e seleção por forma + tinta.','EVITAR  Fonte western e contorno vermelho copiados.']],
 ['dishonored-powers','03 / Dishonored 2 — composição assimétrica',['OBSERVAR  Retrato, seleção e explicação não pesam igual.','ADAPTAR  Um protagonista e ornamento concentrado.','EVITAR  Azul/ciano, símbolos e árvores de poderes.']],
 ['outer-inventory','04 / The Outer Worlds — inventário e identidade',['OBSERVAR  Grade de um lado, personagem do outro.','ADAPTAR  Relação entre retrato, bolsa e área de leitura.','EVITAR  Néon, texturas e bordas douradas em tudo.']]
];
menu.forEach(([id,head,notes],i)=>{const x=48+(i%2)*714,y=158+Math.floor(i/2)*505;rect(x,y,690,31,C.wine);text(x+12,y+22,head,17,C.linen,'Source',600);img(id,x,y+31,690,388);notes.forEach((n,j)=>text(x,y+445+j*21,n,15));});
text(48,1206,'Fonte: Interface In Game. Direitos: PlatinumGames/Square Enix; Rockstar; Arkane/Bethesda; Obsidian/Private Division.',14);text(48,1228,'Capturas originais preservadas e links completos no documento. Não são recursos licenciados para o jogo.',14);
s+='</svg>';await fs.writeFile(path.join(dir,'pesquisa-menus.svg'),s);
begin('HUD: menos ruído, informação no lugar certo.','A mesma lógica de hierarquia em três jogos. Imagens estáticas não comprovam timings, input ou acessibilidade.');
const hud=[
 ['nier-hud','NieR:Automata',[45,25,525,280],['Barras finas e agrupamento discreto.','A cena continua sendo a maior massa.','Levar: rail compacto, não texto minúsculo.','Manter HP escrito no nosso projeto.']],
 ['rdr-hud','Red Dead Redemption 2',[5,705,510,350],['Estados concentrados na periferia.','O centro da cena permanece livre.','Levar: separar persistente de situacional.','Não importar radar ou recursos novos.']],
 ['outer-hud','The Outer Worlds',[15,0,570,285],['Barras encaixadas e metal localizado.','A textura não substitui o agrupamento.','Levar: âncora e material contido.','Não copiar compassos/energia/sistemas.']]
];
hud.forEach(([id,head,roi,notes],i)=>{const x=48+i*476;rect(x,160,452,33,C.wine);text(x+12,184,head,20,C.linen,'Source',600);img(id,x,193,452,254);text(x,476,'VISTA GERAL / REDUZIDA SEM DISTORÇÃO',13,C.tobacco,'Source',600);img(id,x,502,452,302,roi);text(x,832,'RECORTE DE HUD / REFERÊNCIA',13,C.tobacco,'Source',600);notes.forEach((n,j)=>text(x,866+j*25,n,17));});
rect(48,1014,1404,129,C.linen);text(68,1046,'DRIBBBLE / PISTAS ENCONTRADAS, NÃO IMAGENS INSPECIONADAS',15,C.wine,'Source',600);text(68,1075,'Jeffrey Karl: Steampunk UI elemtents (Perchta) · BimGraph/Kretya: MMORPG Mobile · Meng To: Steampunk UI',17);text(68,1101,'Acesso direto/embutido retornou HTTP 202 vazio. Não alegamos ter visto os shots nem derivamos formas deles.',16);text(68,1127,'Links para revisão manual no documento. Nenhum login, credencial ou contorno de proteção foi usado.',15);
text(48,1198,'Interface In Game: capturas reais baixadas e inspecionadas. Recortes apenas nesta prancha de análise; arquivos fonte preservados.',15);text(48,1224,'Direitos dos jogos e demais titulares. Sem importação em Godot e sem alegação de autorização de redistribuição comercial.',14);
s+='</svg>';await fs.writeFile(path.join(dir,'pesquisa-hud.svg'),s);
const dimensions=b=>{if(b[0]===137)return[b.readUInt32BE(16),b.readUInt32BE(20)];let p=2;while(p<b.length){if(b[p]!==255){p++;continue;}let m=b[p+1];p+=2;if([0xd8,0xd9].includes(m))continue;let len=b.readUInt16BE(p);if([0xc0,0xc1,0xc2].includes(m))return[b.readUInt16BE(p+5),b.readUInt16BE(p+3)];p+=len;}throw Error('Unknown dimensions');};
const references=entries.map(([id,title,file,pageUrl,imageUrl,credit])=>({id,title,path:base+file,pageUrl,imageUrl,credit,sha256:crypto.createHash('sha256').update(bytes[id]).digest('hex'),dimensions:dimensions(bytes[id]),visuallyInspected:true,originalUnedited:true,rights:'Third-party research only; no runtime license granted.'}));
await fs.writeFile(path.join(dir,'sources.json'),JSON.stringify({provider:'Interface In Game',references,dribbble:{search:'https://dribbble.com/search/game-ui',status:'Indexed title/author/description only; direct and embed HTTP 202 empty. Search provider later rate-limited. No visual verification.',shots:[{title:'Steampunk UI elemtents',author:'Jeffrey Karl',url:'https://dribbble.com/shots/4566535-Steampunk-UI-elemtents'},{title:'MMORPG Mobile in Game Concept (Mini Study Case)',author:'BimGraph for Kretya Studio',url:'https://dribbble.com/shots/24766433-MMORPG-Mobile-in-Game-Concept-Mini-Study-Case'},{title:'Steampunk UI',author:'Meng To',url:'https://dribbble.com/shots/647020-Steampunk-UI'}]}},null,2));
console.log('Reference boards composed / 8 inspected sources / Dribbble clearly separated.');
