// Static game-art composition. No runtime, economy or gameplay implementation.
// Run: node docs/visual/prototipos-v6/compose.mjs
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url));
const C={bg:'#10191c',panel:'#172328',paper:'#d4cbb6',bone:'#e1d8c5',line:'#3e4b4b',gold:'#bfaa7b'};
const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
const t=(x,y,s,cls='',extra='')=>`<text x="${x}" y="${y}" class="${cls}" ${extra}>${esc(s)}</text>`;
const rect=(x,y,w,h,fill,extra='')=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${fill}" ${extra}/>`;
const line=(x1,y1,x2,y2,cls='line')=>`<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" class="${cls}"/>`;
const image=(file,x,y,w,h,fit='xMidYMid meet')=>`<image href="../conceitos/${file}" x="${x}" y="${y}" width="${w}" height="${h}" preserveAspectRatio="${fit}"/>`;
let clipId=0;
function item(index,x,y,w,h){const bounds=[[0,0,512,490],[550,0,400,502],[970,0,566,502],[30,502,450,512],[530,514,460,510],[1040,515,480,509]][index],id=`item-clip-${clipId++}`;return `<svg x="${x}" y="${y}" width="${w}" height="${h}" viewBox="${bounds.join(' ')}" overflow="hidden"><defs><clipPath id="${id}" clipPathUnits="userSpaceOnUse">${rect(...bounds,'white')}</clipPath></defs><g clip-path="url(#${id})">${image('23-equipamentos-arte-v6.png',0,0,1536,1024)}</g></svg>`}
function button(x,y,w,label,paper=true){return rect(x,y,w,58,paper?C.paper:C.panel,paper?'':`stroke="${C.line}"`)+t(x+w/2,y+37,label,paper?'button-text center':'body center')}
function header(active){let s=rect(0,0,1920,92,C.bg)+t(48,54,'AETHERFORGE','brand');const tabs=[['Expedição',590],['Inventário',812],['Habilidades',1034],['Rotas',1256],['Oficina',1478]];for(const [label,x]of tabs){s+=t(x,54,label,'body center '+(label===active?'':'muted'));if(label===active)s+=rect(x-70,82,140,3,C.paper)}return s+line(48,91,1872,91)+t(1870,34,'CARTEIRA','label muted end')+t(1870,64,'640 moedas','body num end')}
function progress(x,y,step=2){let s='';for(let i=1;i<=5;i++){if(i<5)s+=line(x+(i-1)*76,y,x+i*76,y);s+=`<circle cx="${x+(i-1)*76}" cy="${y}" r="${i===step?9:5}" fill="${i<step?C.paper:C.bg}" stroke="${i===step?C.paper:C.line}" stroke-width="2"/>`}return s}
function bar(x,y,w,val,max){return rect(x,y,w,5,'#2b393b')+rect(x,y,w*val/max,5,'#91a28d')}
function miniStage(){return rect(0,806,1920,274,'#0c1418')+line(48,806,1872,806)+t(48,850,'EXPEDIÇÃO EM CURSO','label accent')+t(48,894,'Galerias da Caldeira','title')+t(48,932,'Trilhos partidos · etapa 2/5','body muted')+progress(59,970)+t(48,1025,'Combate automático · encontro ativo','small muted')+image('21-combate-arte-v6.png',560,818,720,240)+line(1325,838,1325,1045)+t(1370,859,'EQUIPE','label muted')+[['Alquimista',132,142],['Pistoleiro',115,130],['Guarda',162,226]].map(([name,hp,max],i)=>t(1370,900+i*54,name,'small')+t(1872,900+i*54,`${hp}/${max}`,'small num end')+bar(1370,912+i*54,502,hp,max)).join('')}
function shield(x,y,scale=1,ink=false){return `<g transform="translate(${x} ${y}) scale(${scale})" class="icon" ${ink?'style="stroke:#394b49"':''}><path d="M-24-28H24V8C24 25 0 37 0 37S-24 25-24 8Z"/><path d="M0-22V28M-17-6H17"/></g>`}
function hammer(x,y){return `<g transform="translate(${x} ${y})" class="icon"><path d="M-18-20H22V0H-18ZM-2 0V29H7V0"/></g>`}
function signal(x,y){return `<g transform="translate(${x} ${y})" class="icon"><path d="M-19 20V-19L21-11-19 0M-28 25H-10"/></g>`}
function screen(name,body,h=1080){return `<!doctype html>\n<html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Aetherforge · ${name} · v6</title><link rel="stylesheet" href="telas.css"></head><body><figure><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1920 ${h}" role="img" aria-labelledby="title desc"><title id="title">Aetherforge — ${name}, conceito v6</title><desc id="desc">Prancha estática de arte e fluxo de jogador. Arte raster criada com GPT Images; texto nativo com Libre Caslon Text e Jost. Não é uma implementação de jogo. Valores de treinamento são uma proposta visual.</desc>${rect(0,0,1920,h,C.bg)}${body}</svg></figure></body></html>\n`}

// 01 — The game itself; large stage with a quiet HUD rather than a web dashboard.
let combat=header('Expedição')+t(48,145,'Galerias da Caldeira','display')+t(1872,136,'CAÇA · ETAPA 2/5','label end muted')+image('21-combate-arte-v6.png',0,171,1920,640)+rect(0,808,1920,272,C.bg)+line(48,817,1872,817);
const party=[['Alquimista do Éter','SUSTENTAÇÃO',132,142,'Infusão Etérea · automática'],['Pistoleiro','DANO',115,130,'Tiro Perfurante · automático'],['Guarda da Caldeira','PROTEÇÃO',162,226,'Escudo de Pressão · automático']];
party.forEach(([name,role,hp,max,skill],i)=>{const x=48+i*447;combat+=t(x,856,role,'label muted')+t(x,898,name,'title')+t(x,939,`${hp}/${max}`,'body num')+t(x+330,939,'VIDA','tiny end muted')+bar(x,954,330,hp,max)+t(x,1004,skill,'small muted');if(i<2)combat+=line(x+402,853,x+402,1020)});
combat+=line(1381,851,1381,1020)+t(1425,859,'TRILHOS PARTIDOS','label accent')+progress(1438,895)+t(1425,938,'O encontro segue automaticamente.','small muted')+button(1425,963,447,'Abrir inventário')+t(48,1060,'Uma equipe · uma atividade · avanço automático','tiny muted')+t(1872,1060,'Conceito visual v6','tiny muted end');
await fs.writeFile(path.join(dir,'expedicao.html'),screen('Expedição',combat));

// 02 — Inventory: character / six readable item cells / explicit comparison.
let inv=header('Inventário')+t(48,149,'Guarda da Caldeira','title')+t(48,182,'Nível 3 · Proteção','small muted')+image('22-guarda-arte-v6.png',40,191,452,593)+line(510,133,510,774)+t(550,143,'Equipamentos','title')+t(550,186,'Bolsa da equipe','small muted')+t(1240,186,'6 peças','small num muted end')+line(550,207,1240,207);
const tiles=[['Couraça de Latão','ARMADURA',0],['Escudo de Guarda','ARMA · GUARDA',1],['Rifle de Serviço','ARMA · PISTOLEIRO',2],['Casaco de Lona','ARMADURA',3],['Foco de Infusão','FOCO · ALQUIMISTA',4],['Regulador de Pressão','ACESSÓRIO',5]];
tiles.forEach(([name,kind,index],i)=>{const x=550+(i%3)*238,y=230+Math.floor(i/3)*271;inv+=rect(x,y,214,206,'#142024')+item(index,x+6,y+5,202,195)+rect(x,y,214,206,'none',`stroke="${i===0?C.paper:C.line}" stroke-width="${i===0?2:1}"`)+t(x,y+233,name,'small '+(i===0?'':'muted'))+t(x,y+257,kind,'tiny muted');if(i===0)inv+=rect(x+9,y+9,5,22,C.paper)});
inv+=rect(1314,122,558,652,C.panel)+t(1350,165,'ARMADURA · INCOMUM','label gain')+t(1350,211,'Couraça de Latão','title')+item(0,1461,226,255,220)+line(1350,464,1836,464)+t(1350,502,'Defesa','body')+t(1836,502,'+2 → +3','body num end')+t(1350,535,'Ganho de +1 defesa','small gain')+t(1350,583,'Durabilidade','body')+t(1836,583,'78/100','body num end')+bar(1350,599,486,78,100)+button(1350,628,486,'Equipar após encontro')+t(1593,714,'Troca aplicada entre encontros.','small center muted')+t(1593,747,'Cancelar','small center')+miniStage();
await fs.writeFile(path.join(dir,'inventario.html'),screen('Inventário',inv));

// 03 — Skill ranks are a mockup proposal, not a newly approved GDD rule.
let skills=header('Habilidades')+t(48,149,'Guarda da Caldeira','title')+t(48,182,'Nível 3 · Proteção','small muted')+image('22-guarda-arte-v6.png',40,191,452,593)+line(510,133,510,774)+t(550,144,'Treinamento','title')+t(550,186,'1 ponto disponível','body num accent')+t(1240,185,'GUARDA','label muted end')+line(550,207,1240,207);
skills+=`<path d="M895 373V386M895 460V480H662V518M895 480H1128V518M895 480V518" class="node-line"/>`;
const nodes=[{x:895,y:320,name:'Escudo de Pressão',rank:'1/3',type:'shield',selected:true},{x:662,y:570,name:'Provocação',rank:'Automática',type:'signal'},{x:895,y:570,name:'Golpe de Guarda',rank:'Ataque básico',type:'hammer'},{x:1128,y:570,name:'Resistência',rank:'Em estudo',type:'shield',disabled:true}];
for(const n of nodes){skills+=`<g ${n.disabled?'class="disabled"':''}>`+rect(n.x-52,n.y-52,104,104,'#1b282d',`class="node ${n.selected?'selected':''}"`)+(n.type==='shield'?shield(n.x,n.y,.78):n.type==='signal'?signal(n.x,n.y):hammer(n.x,n.y))+t(n.x,n.y+93,n.name,'small center')+t(n.x,n.y+125,n.rank,'small num center '+(n.selected?'accent':'muted'))+'</g>'}
skills+=t(550,749,'As habilidades são usadas automaticamente no combate.','small muted')+rect(1314,122,558,652,C.paper)+t(1350,164,'REGISTRO DE TREINAMENTO','label ink')+t(1350,211,'Escudo de Pressão','title ink')+t(1350,247,'Rank 1 → 2','body num paper-muted')+line(1350,272,1836,272,'p-line')+t(1350,312,'Redução de dano','body ink')+t(1350,375,'40% → 44%','display ink num')+t(1350,416,'Proteção durante 4 segundos.','body ink')+line(1350,445,1836,445,'p-line')+t(1350,484,'Energia','body paper-muted')+t(1836,484,'10','body num ink end')+t(1350,526,'Recarga','body paper-muted')+t(1836,526,'12 s','body num ink end')+t(1350,568,'Custo do treinamento','body paper-muted')+t(1836,568,'1 ponto','body num ink end')+rect(1350,607,486,58,'#233437')+t(1593,645,'Treinar habilidade','body center')+t(1593,700,'Uso automático · sem comando de conjurar','small center paper-muted')+t(1593,748,'Proposta de progressão para avaliação','tiny center paper-muted')+miniStage();
await fs.writeFile(path.join(dir,'habilidades.html'),screen('Habilidades',skills));

// 04 — Compact idle strip, a separate layout proof rather than a resized menu.
let compact=image('21-combate-arte-v6.png',0,0,960,320)+rect(960,0,960,320,C.bg)+rect(0,0,960,44,'#10191c')+t(24,30,'GALERIAS DA CALDEIRA · TRILHOS PARTIDOS','label')+t(1000,43,'EXPEDIÇÃO','label accent')+t(1872,43,'ETAPA 2/5','label muted end');
[['Alquimista',132,142],['Pistoleiro',115,130],['Guarda',162,226]].forEach(([n,hp,max],i)=>{const x=1000+i*294;compact+=t(x,100,n,'body')+t(x,137,`${hp}/${max}`,'body num')+bar(x,154,250,hp,max)});
compact+=line(1000,184,1872,184)+t(1000,226,'Combate automático','small muted')+t(1872,226,'640 moedas','small num end')+button(1000,249,264,'Inventário',false)+button(1294,249,264,'Habilidades',false)+button(1588,249,284,'Rotas',false);
await fs.writeFile(path.join(dir,'faixa-idle.html'),screen('Faixa compacta do idle',compact,320));
console.log('4 composições estáticas criadas com fontes locais.');
