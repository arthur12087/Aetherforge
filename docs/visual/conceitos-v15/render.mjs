// Local reference board export. No gameplay, external art generation or runtime textures.
import fs from 'node:fs/promises';
import path from 'node:path';
import http from 'node:http';
import {spawn} from 'node:child_process';
import {fileURLToPath} from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(dir,'../../..');
const audit=path.join(root,'artifacts/ui-v15/design'),output=path.join(root,'assets/concepts/brass-v15-ui');
await fs.mkdir(audit,{recursive:true}); await fs.mkdir(output,{recursive:true});
const server=http.createServer(async(req,res)=>{try{const name=decodeURIComponent(new URL(req.url,'http://localhost').pathname);if(name==='/favicon.ico'){res.writeHead(204).end();return;}const file=path.resolve(root,'.'+name);if(!file.startsWith(root+path.sep))throw Error('outside root');const data=await fs.readFile(file);res.writeHead(200,{'Content-Type':({'.html':'text/html','.svg':'image/svg+xml','.ttf':'font/ttf','.png':'image/png','.jpg':'image/jpeg'})[path.extname(file)]||'application/octet-stream'}).end(data);}catch{res.writeHead(404).end();}});
await new Promise(r=>server.listen(19458,'127.0.0.1',r));
const chrome=spawn(process.env.CHROME_PATH||'C:/Users/Arthur/AppData/Local/ms-playwright/chromium-1223/chrome-win64/chrome.exe',['--headless=new','--no-first-run','--no-default-browser-check','--hide-scrollbars','--enable-unsafe-swiftshader','--use-angle=swiftshader','--remote-debugging-port=19459',`--user-data-dir=${path.join(audit,'chrome-profile')}`],{stdio:'ignore'});
let ws;const pending=new Map();let serial=0;
try{
 let ready=false;for(let i=0;i<100;i++){try{await fetch('http://127.0.0.1:19459/json/version');ready=true;break;}catch{await new Promise(r=>setTimeout(r,100));}}if(!ready)throw Error('Chrome timeout');
 const target=await(await fetch('http://127.0.0.1:19459/json/new?about%3Ablank',{method:'PUT'})).json();
 ws=new WebSocket(target.webSocketDebuggerUrl); await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j;});
 ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result);}};
 const call=(method,params={})=>new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
 await call('Page.enable');await call('Runtime.enable');await call('Emulation.setDeviceMetricsOverride',{width:1440,height:1200,deviceScaleFactor:1,mobile:false});
 await call('Page.navigate',{url:'http://127.0.0.1:19458/docs/visual/conceitos-v15/propostas.html'});
 for(let i=0;i<100;i++){let a=await call('Runtime.evaluate',{expression:'document.readyState',returnByValue:true});if(a.result.value==='complete')break;await new Promise(r=>setTimeout(r,100));}
 const r=await call('Runtime.evaluate',{expression:`(async()=>{await document.fonts.ready;await Promise.all([...document.images].map(i=>i.decode()));return{fonts:[...document.fonts].map(f=>({family:f.family,status:f.status})),images:[...document.images].map(i=>({src:i.src,ok:i.naturalWidth>0})),overflow:[...document.body.querySelectorAll('h1,h2,p,.disclaimer,.motion')].filter(e=>e.getBoundingClientRect().bottom>1200).map(e=>e.textContent),scrollWidth:document.documentElement.scrollWidth,scrollHeight:document.documentElement.scrollHeight}})()`,awaitPromise:true,returnByValue:true});
 if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));
 const report=r.result.value;if(report.overflow.length||report.images.some(i=>!i.ok))throw Error(JSON.stringify(report));
 // Direct SVG raster avoids the observed repeated/black compositor tiles in headless Chrome.
 const renders=[];
 for(const name of ['propostas','pesquisa']){
  const svg=await fs.readFile(path.join(dir,name+'.svg'),'utf8');
  const raster=await call('Runtime.evaluate',{expression:`(async()=>{const blob=new Blob([${JSON.stringify(svg)}],{type:'image/svg+xml;charset=utf-8'});const url=URL.createObjectURL(blob);const im=new Image();im.src=url;await im.decode();await new Promise(r=>setTimeout(r,300));const canvas=document.createElement('canvas');canvas.width=im.naturalWidth;canvas.height=im.naturalHeight;canvas.getContext('2d').drawImage(im,0,0);URL.revokeObjectURL(url);return{data:canvas.toDataURL('image/png').split(',')[1],width:canvas.width,height:canvas.height}})()`,awaitPromise:true,returnByValue:true});
  if(raster.exceptionDetails||!raster.result.value)throw Error(JSON.stringify(raster.exceptionDetails));
  const {data,width,height}=raster.result.value;
  await fs.writeFile(path.join(output,name+'.png'),Buffer.from(data,'base64'));
  renders.push({name,width,height,method:'self-contained SVG with embedded fonts and local images; no crop/upscale of runtime'});
  console.log(name+' PASS / '+width+'x'+height+' / embedded SVG fonts');
 }
 await fs.writeFile(path.join(audit,'qa.json'),JSON.stringify({...report,renders},null,2));
}finally{ws?.close();chrome.kill();server.close();}
