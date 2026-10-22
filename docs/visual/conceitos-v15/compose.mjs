// Original static alternatives, not gameplay captures. All vectors/text are editable.
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(dir,'../../..');
const esc=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;');
const colors={paper:'#BFB49E',soot:'#241F1A',tobacco:'#40332B',wine:'#6C3A32',brass:'#B99B63',linen:'#E4D8B7',ether:'#97A581'};
let svg=`<svg xmlns="http://www.w3.org/2000/svg" width="1440" height="1160" viewBox="0 0 1440 1160"><defs><style>@font-face{font-family:Source;src:url('data:font/ttf;base64,${(await fs.readFile(path.join(root,'docs/visual/assets/fonts/v5/SourceSans3-Variable.ttf'))).toString('base64')}');font-weight:200 900}@font-face{font-family:Limelight;src:url('data:font/ttf;base64,${(await fs.readFile(path.join(root,'docs/visual/assets/fonts/v5/Limelight-Regular.ttf'))).toString('base64')}')}text{font-family:Source;fill:${colors.soot}}.title{font-family:Limelight}.strong{font-weight:600}</style></defs><rect width="1440" height="1160" fill="${colors.paper}"/>`;
const text=(x,y,s,size=15,color=colors.soot,cls='')=>svg+=`<text x="${x}" y="${y}" font-size="${size}" fill="${color}" style="fill:${color}" class="${cls}">${esc(s)}</text>`;
const rect=(x,y,w,h,color,stroke='',sw=1)=>svg+=`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${color}"${stroke?` stroke="${stroke}" stroke-width="${sw}"`:''}/>`;
const line=(x,y,x2,y2,color,width=1)=>svg+=`<path d="M${x} ${y}H${x2}" transform="${y2!==y?'':''}" fill="none" stroke="${color}" stroke-width="${width}"/>`;
const iconBytes={};for(const k of ['vital','rifle','shield','heal'])iconBytes[k]=(await fs.readFile(path.join(root,`src/Aetherforge.Client/Assets/Brass/Icons/${k}.svg`))).toString('base64');
const icon=(x,y,key,dark=false,size=22)=>svg+=`<image x="${x}" y="${y}" width="${size}" height="${size}" href="data:image/svg+xml;base64,${dark?Buffer.from(Buffer.from(iconBytes[key],'base64').toString().replaceAll('#E4D8B7',colors.soot).replaceAll('#B99B63',colors.soot)).toString('base64'):iconBytes[key]}"/>`;
text(52,45,'AETHERFORGE / A PROMESSA DO LATÃO / COMPONENTES V15',13,colors.soot,'strong');text(52,91,'Menos caixas, mais instrumentos.',31,colors.soot,'title');
text(52,124,'Três tratamentos para título, atributos e equipe. Mesmos textos e valores demonstrativos; mesma direção quente.',17);
text(52,148,'Estudos estáticos originais — não são capturas do jogo nem novas pinturas.',15);
const variants=[['A · Mesa de regulagem','PILOTO RECOMENDADO',['Placa chanfrada, encaixes claros e fichas','com medalhão ocupacional.'],['Melhor equilíbrio entre leitura e material.','Latão só nos contatos; sem textura sob os números.']],['B · Ficha de campo','ALTERNATIVA EDITORIAL',['Papel dominante, cabeçalho vinho e','régua de atributos compartilhada.'],['Mais leve e editorial, com menos volume.','Pode se confundir com o bilhete da rota.']],['C · Bancada de oficina','ALTERNATIVA MECÂNICA',['Fuligem dominante, carcaça recortada','e valores em visores circulares.'],['Mais pesado, mas menos imediato na leitura.','Sem ponteiros ou escalas de atributos inventados.']]];
for(let v=0;v<3;v++){
 const x=52+v*454,y=284,w=428,h=548,p=v===1,fg=p?colors.soot:colors.linen,bg=p?colors.paper:v===2?colors.soot:colors.tobacco;
 text(x,190,variants[v][1],13,colors.wine,'strong');text(x,222,variants[v][0],23,colors.soot,'title');variants[v][2].forEach((s,i)=>text(x,248+i*19,s,16));
 rect(x,y+4,w,h,colors.soot);rect(x,y,w,h,bg,'#7d705e');
 text(x+18,y+27,'01 / STATUS',13,p?colors.tobacco:colors.paper);text(x+w-135,y+27,'ARCO PARTIDO  ×',12,p?colors.tobacco:colors.paper);
 const px=x+18,py=y+41,pw=w-36;
 if(v===0){svg+=`<path d="M${px+6} ${py}H${px+pw-6}l6 6v38l-6 6H${px+6}l-6-6V${py+6}Z" fill="${colors.paper}" stroke="${colors.brass}"/>`;rect(px+6,py+4,pw-12,41,colors.linen);text(px+14,py+34,'STATUS',26,colors.soot,'title');text(px+pw-24,py+37,'≡',20);}
 else{rect(px,py,pw,50,p?colors.wine:colors.tobacco);line(px,py,px+pw,py,colors.brass,2);text(px+14,py+34,'STATUS',26,colors.linen,'title');}
 text(px,py+70,'Mesa de regulagem',13,p?colors.tobacco:colors.paper);text(px,py+96,'Guarda da Caldeira',18,fg,'strong');
 const sy=py+111,sw=(pw-16)/3;
 if(p)rect(px,sy,pw,106,colors.linen);
 for(let i=0;i<3;i++){
  const sx=px+i*(sw+8);const [key,label,value,max]=[['vital','Vida','138','/ 180'],['rifle','Ataque','8',''],['shield','Defesa','10','']][i];
  if(v===0){rect(sx,sy,sw,106,colors.soot);rect(sx+3,sy+3,sw-6,100,colors.paper,colors.brass);rect(sx+7,sy+7,sw-14,88,colors.linen);}
  if(v===2){rect(sx,sy,sw,106,colors.tobacco,'#806944');svg+=`<circle cx="${sx+sw/2}" cy="${sy+59}" r="28" fill="none" stroke="${colors.brass}"/>`;}
  if(v!==2)icon(sx+sw/2-36,sy+10,key,true,15);
  text(sx+sw/2-(v===2?18:13),sy+23,label,13,v===2?colors.linen:colors.soot);
  text(sx+sw/2-(value.length*8),sy+71,value,30,v===2?colors.linen:colors.soot,'strong');text(sx+sw/2-17,sy+94,max,13,v===2?colors.paper:colors.soot);
 }
 const ty=sy+134;text(px,ty,'Equipe',16,fg,'strong');
 for(let i=0;i<3;i++){
  const ry=ty+14+i*56;const [key,name,val]=[['shield','Guarda da Caldeira','138/180'],['rifle','Pistoleiro','100/100'],['heal','Alquimista do Éter','110/110']][i];
  rect(px,ry,pw,49,p?(i===0?colors.linen:colors.paper):i===0?colors.tobacco:'#28221d');
  line(px+3,ry,px+pw-3,ry,'#7d705e');if(i===0)rect(px,ry+5,3,39,p?colors.wine:colors.brass);
  if(v===0)svg+=`<circle cx="${px+24}" cy="${ry+25}" r="16" fill="none" stroke="${colors.brass}"/>`;
  icon(px+13,ry+14,key,p);text(px+47,ry+20,name,14,fg,'strong');text(px+pw-58,ry+20,val,13,p?colors.tobacco:colors.paper);
  rect(px+47,ry+30,pw-56,8,colors.soot);rect(px+48,ry+31,(pw-58)*(i===0?.767:1),6,colors.ether);
 }
 text(px,y+h-16,'Selecionar integrante · Status / Herói / palco',13,p?colors.tobacco:colors.paper);
 variants[v][3].forEach((s,i)=>text(x,866+i*20,s,15));
}
line(52,918,1388,918,'#8f806b');
const notes=[['FOCO · 140 ms',['Contato curto de latão. O contorno','de teclado continua imediato.']],['CLIQUE · 80 + 140 ms',['Batente breve, sem mover texto,','janela ou área clicável.']],['VIDA · ATÉ 360 ms',['Valor real muda primeiro. Cauda','curta e localizada mostra a perda.']],['MOVIMENTO REDUZIDO',['Remove transições decorativas;','mantém seleção, números e ações.']]];
notes.forEach(([t,ls],i)=>{const x=52+i*340;text(x,950,t,14,colors.soot,'strong');ls.forEach((s,j)=>text(x,975+j*19,s,14));});
text(52,1057,'A é a recomendação e base do piloto nativo. B e C permanecem alternativas, não skins implementadas.',15,colors.soot,'strong');
text(52,1086,'Pesquisa: BioShock / Shelby Tang; Frostpunk / Interface In Game; ficha de herói de Darkest Dungeon II.',14);
text(52,1110,'Sem copiar atlas, ícones, personagens ou regras dos jogos pesquisados. SVG local com fontes e símbolos próprios.',14);
svg+='</svg>';await fs.writeFile(path.join(dir,'propostas.svg'),svg);console.log('SVG composed / 1440x1160');
