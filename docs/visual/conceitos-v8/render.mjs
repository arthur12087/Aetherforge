// Headless static-art export and QA through Chrome DevTools; no npm dependencies.
import fs from 'node:fs/promises';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath,pathToFileURL } from 'node:url';
const dir=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(dir,'../../..');
const out=path.join(root,'assets/concepts/idle-v8');
const artifacts=path.join(root,'artifacts/concepts-v8');
const chrome=process.env.CHROME_PATH||'C:/Users/Arthur/AppData/Local/ms-playwright/chromium-1223/chrome-win64/chrome.exe';
await fs.mkdir(artifacts,{recursive:true});
const port=19437;
const proc=spawn(chrome,['--headless=new','--disable-gpu','--no-first-run','--no-default-browser-check','--allow-file-access-from-files',`--remote-debugging-port=${port}`,`--user-data-dir=${path.join(artifacts,'chrome-profile')}`],{stdio:'ignore'});
let serial=0;
async function connect(url){const ws=new WebSocket(url);await new Promise((ok,bad)=>{ws.onopen=ok;ws.onerror=bad});const pending=new Map();const events=[];ws.onmessage=({data})=>{const m=JSON.parse(data);if(m.id){const p=pending.get(m.id);if(p){pending.delete(m.id);m.error?p.reject(new Error(JSON.stringify(m.error))):p.resolve(m.result)}}else events.push(m)};return{events,close:()=>ws.close(),send:(method,params={})=>new Promise((resolve,reject)=>{const id=++serial;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}))})}}
const files=['a-fuligem-decreto','b-promessa-latao','c-ultimo-turno','menus-referencias'];
const reports=[];
try{
 let ready=false;for(let i=0;i<80;i++){try{await fetch(`http://127.0.0.1:${port}/json/version`);ready=true;break}catch{await new Promise(r=>setTimeout(r,150))}}if(!ready)throw new Error('Chromium did not start.');
 for(const name of files){
  const url=pathToFileURL(path.join(dir,name+'.html')).href;
  const target=await(await fetch(`http://127.0.0.1:${port}/json/new?${encodeURIComponent('about:blank')}`,{method:'PUT'})).json();
  const c=await connect(target.webSocketDebuggerUrl);
  await c.send('Page.enable');await c.send('Runtime.enable');await c.send('Log.enable');
  await c.send('Emulation.setDeviceMetricsOverride',{width:1920,height:1080,deviceScaleFactor:1.5,mobile:false});
  await c.send('Page.navigate',{url});
  const result=await c.send('Runtime.evaluate',{expression:`(async()=>{await document.fonts.ready;await Promise.all([...document.images].map(i=>i.complete?Promise.resolve():new Promise(r=>{i.onload=r;i.onerror=r})));await new Promise(r=>setTimeout(r,650));const svgImages=[...document.querySelectorAll('svg image')];const imageUrls=[...new Set(svgImages.map(i=>i.getAttribute('href')))];const imageChecks=await Promise.all(imageUrls.map(src=>new Promise(r=>{const i=new Image();i.onload=()=>r({src,ok:true,width:i.naturalWidth,height:i.naturalHeight});i.onerror=()=>r({src,ok:false});i.src=src})));const fonts=[...document.fonts].filter(f=>f.status==='loaded').map(f=>({family:f.family,status:f.status}));const textBounds=[...document.querySelectorAll('svg text')].map(t=>{const box=t.getBBox();const m=t.getCTM();const x=box.x*m.a+m.e,y=box.y*m.d+m.f;return{text:t.textContent,x,y,width:box.width*m.a,height:box.height*m.d}});const overflow=textBounds.filter(b=>b.x<0||b.y<0||b.x+b.width>1920.5||b.y+b.height>1080.5);const badHtmlImages=[...document.images].filter(i=>!i.naturalWidth).map(i=>i.src);return{title:document.title,fonts,imageChecks,badHtmlImages,overflow,textBounds,bodyOverflow:document.documentElement.scrollWidth>1920||document.documentElement.scrollHeight>1080,svgWindowTitles:[...document.querySelectorAll('.panel-title')].map(t=>t.textContent)}})()`,awaitPromise:true,returnByValue:true});
  if(result.exceptionDetails)throw new Error(JSON.stringify(result.exceptionDetails));
  const report=result.result.value;report.name=name;report.errors=c.events.filter(m=>m.method==='Runtime.exceptionThrown'||m.method==='Log.entryAdded'&&m.params.entry.level==='error');reports.push(report);
  if(report.overflow.length||report.badHtmlImages.length||report.imageChecks.some(i=>!i.ok)||report.bodyOverflow||report.errors.length)throw new Error('QA failed: '+JSON.stringify(report));
  const screenshot=await c.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false,fromSurface:true});
  await fs.writeFile(path.join(out,name+'.png'),Buffer.from(screenshot.data,'base64'));
  console.log(`${name}: PNG exported, ${report.fonts.map(f=>f.family).join(' / ')}, ${report.textBounds.length} SVG text labels, zero loading/viewport errors.`);
  c.close();await fetch(`http://127.0.0.1:${port}/json/close/${target.id}`);
 }
 await fs.writeFile(path.join(artifacts,'qa.json'),JSON.stringify(reports,null,2)+'\n');
}catch(e){await fs.writeFile(path.join(artifacts,'qa-failure.json'),JSON.stringify({error:String(e),reports},null,2));throw e}
finally{proc.kill()}
