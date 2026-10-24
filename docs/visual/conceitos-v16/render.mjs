// Self-contained SVG -> PNG. No screenshot/compositor or external network dependency.
import fs from 'node:fs/promises';
import path from 'node:path';
import http from 'node:http';
import {spawn} from 'node:child_process';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(dir,'../../..');
const output=path.join(root,'assets/concepts/brass-v16-ficha'),audit=path.join(root,'artifacts/ui-v16/design');
const rasterFunction=await fs.readFile(path.join(dir,'canvas-raster.js'),'utf8');
await fs.mkdir(audit,{recursive:true});
const server=http.createServer(async(req,res)=>{try{const name=decodeURIComponent(new URL(req.url,'http://localhost').pathname);if(name==='/favicon.ico'){res.writeHead(204).end();return;}const file=path.resolve(root,'.'+name);if(!file.startsWith(root+path.sep))throw Error('outside root');const data=await fs.readFile(file);res.writeHead(200,{'Content-Type':({'.html':'text/html','.png':'image/png','.svg':'image/svg+xml','.ttf':'font/ttf'})[path.extname(file)]||'application/octet-stream'}).end(data);}catch{res.writeHead(404).end();}});
await new Promise(r=>server.listen(0,'127.0.0.1',r));
const port=server.address().port,profile=path.join(audit,`chrome-${process.pid}-${Date.now()}`);
const chrome=spawn(process.env.CHROME_PATH||'C:/Users/Arthur/AppData/Local/ms-playwright/chromium-1223/chrome-win64/chrome.exe',['--headless=new','--no-first-run','--no-default-browser-check','--disable-gpu','--remote-debugging-port=0',`--user-data-dir=${profile}`],{stdio:'ignore'});
let ws,serial=0;const pending=new Map(),reports=[];
try{
 let debugPort;for(let i=0;i<150;i++){try{debugPort=Number((await fs.readFile(path.join(profile,'DevToolsActivePort'),'utf8')).split('\n')[0]);break;}catch{await new Promise(r=>setTimeout(r,100));}}if(!debugPort)throw Error('Chrome startup failed');
 const target=await(await fetch(`http://127.0.0.1:${debugPort}/json/new?about%3Ablank`,{method:'PUT'})).json();
 ws=new WebSocket(target.webSocketDebuggerUrl);await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j;});
 ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result);}};
 const call=(method,params={})=>new Promise((resolve,reject)=>{let id=++serial;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
 await call('Runtime.enable');
 async function render(name){const svg=await fs.readFile(path.join(dir,name+'.svg'),'utf8');
  const result=await call('Runtime.evaluate',{expression:`(async()=>{${rasterFunction};return await rasterSvg(${JSON.stringify(svg)});})()`,awaitPromise:true,returnByValue:true});
  if(result.exceptionDetails)throw Error(JSON.stringify(result.exceptionDetails));const {data,...qa}=result.result.value;
  if(qa.outside.length||qa.externalResources||qa.fonts.some(f=>f.status!=='loaded'))throw Error(name+' failed: '+JSON.stringify(qa));
  await fs.writeFile(path.join(output,name+'.png'),Buffer.from(data,'base64'));reports.push({name,...qa,method:'Restricted SVG primitive replay into Canvas2D with decoded images/local fonts; no nested SVG image raster or compositor screenshot.'});console.log(`${name} PASS ${qa.width}x${qa.height} / ${qa.textCount} texts`);
 }
 for(const name of ['01-dossie','02-caderno','03-trilho','04-arquivo','pesquisa-menus','pesquisa-hud'])await render(name);
 const font=await fs.readFile(path.join(root,'docs/visual/assets/fonts/v5/SourceSans3-Variable.ttf'));
 let comparison=`<svg xmlns="http://www.w3.org/2000/svg" width="2000" height="1900" viewBox="0 0 2000 1900"><defs><style>@font-face{font-family:Source;src:url('data:font/ttf;base64,${font.toString('base64')}');font-weight:200 900}</style></defs><rect width="2000" height="1900" fill="#241F1A"/><g font-family="Source" fill="#E4D8B7"><text x="56" y="54" font-size="17" letter-spacing="2">AETHERFORGE / EXPLORAÇÃO V16 / QUATRO ESTRUTURAS</text><text x="56" y="110" font-size="45" font-weight="600">Não é só trocar a moldura.</text><text x="56" y="151" font-size="24">Mesma direção quente. Diferentes proporções, agrupamentos e relações com o palco.</text>`;
 const names=['01-dossie','02-caderno','03-trilho','04-arquivo'];
 for(let i=0;i<4;i++){const png=await fs.readFile(path.join(output,names[i]+'.png'));comparison+=`<image x="${56+(i%2)*970}" y="${190+Math.floor(i/2)*800}" width="918" height="765" href="data:image/png;base64,${png.toString('base64')}"/>`;}
 comparison+='<text x="56" y="1822" font-size="23">01 continuidade · 02 páginas · 03 horizontal · 04 retrato protagonista</text><text x="56" y="1862" font-size="21">Estudos estáticos, não skins implementadas. Abra cada prancha separada para ler em tamanho integral.</text></g></svg>';
 await fs.writeFile(path.join(dir,'comparacao.svg'),comparison);await render('comparacao');
 await fs.writeFile(path.join(audit,'render-qa.json'),JSON.stringify({passed:true,renders:reports,runtimeChanged:false},null,2));
 await call('Page.enable');await call('Page.navigate',{url:`http://127.0.0.1:${port}/docs/visual/conceitos-v16/index.html`});
 await new Promise(r=>setTimeout(r,250));const gallery=await call('Runtime.evaluate',{expression:'(async()=>{await document.fonts.ready;await Promise.all([...document.images].map(i=>i.decode()));return{images:[...document.images].map(i=>({src:i.src,width:i.naturalWidth,height:i.naturalHeight})),title:document.title}})()',awaitPromise:true,returnByValue:true});if(gallery.exceptionDetails||gallery.result.value.images.length!==7)throw Error('Gallery failed '+JSON.stringify(gallery));await fs.writeFile(path.join(audit,'gallery-qa.json'),JSON.stringify(gallery.result.value,null,2));
}finally{ws?.close();chrome.kill();server.close();}
