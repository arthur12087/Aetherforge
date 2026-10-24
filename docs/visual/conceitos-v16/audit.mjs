// Static design audit; deliberately does not run or mutate the game.
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import assert from 'node:assert/strict';
import {execFileSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import zlib from 'node:zlib';
const dir=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(dir,'../../..');
const base='assets/concepts/brass-v16-ficha/',audit=path.join(root,'artifacts/ui-v16/design');
const load=async p=>JSON.parse(await fs.readFile(path.join(root,p),'utf8'));
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const pngSize=b=>{assert.equal(b.subarray(0,8).toString('hex'),'89504e470d0a1a0a');return[b.readUInt32BE(16),b.readUInt32BE(20)];};
const render=await load('artifacts/ui-v16/design/render-qa.json'),gallery=await load('artifacts/ui-v16/design/gallery-qa.json'),layouts=await load('docs/visual/conceitos-v16/layouts.json'),sources=await load('docs/visual/conceitos-v16/sources.json');
assert.equal(render.passed,true);assert.equal(render.renders.length,7);assert.equal(gallery.images.length,7);
for(const r of render.renders){assert.equal(r.outside.length,0);assert.equal(r.externalResources,0);assert.ok(r.fonts.every(f=>f.status==='loaded'));assert.ok(r.decodedImages.length>0);}
const filenames=['01-dossie','02-caderno','03-trilho','04-arquivo','comparacao','pesquisa-menus','pesquisa-hud'];
const images=[];
for(const name of filenames){const p=base+name+'.png',b=await fs.readFile(path.join(root,p)),dimensions=pngSize(b);assert.deepEqual(dimensions,name==='comparacao'?[2000,1900]:[1500,1250]);images.push({path:p,dimensions,sha256:hash(b),kind:name.startsWith('pesquisa')?'third-party-reference-composition':'original-static-layout-study',runtimeCapture:false});}
for(const r of sources.references){assert.equal(hash(await fs.readFile(path.join(root,r.path))),r.sha256);assert.equal(r.visuallyInspected,true);}
const reused=['assets/concepts/brass-v12-gpt/generated/01-equipe-r02.png','assets/concepts/brass-v13-godot/compact.png'];
const originals=[];for(const p of reused){const b=await fs.readFile(path.join(root,p)),old=execFileSync('git',['show','HEAD:'+p],{cwd:root,maxBuffer:12*1024*1024});assert.equal(hash(b),hash(old));originals.push({path:p,sha256:hash(b),originalUnchangedFromHead:true});}
const crops=[['guard',reused[0],[555,0,565,941]],['guard-upper',reused[0],[555,0,565,750]],['guard-face',reused[0],[650,0,410,455]],['gunner-face',reused[0],[130,0,350,455]],['healer-face',reused[0],[1195,0,350,455]],['stage',reused[1],[0,62,1000,210]]];
const derived=[];for(const [name,source,crop]of crops){const p=base+'derived/'+name+'.png',b=await fs.readFile(path.join(root,p));assert.deepEqual(pngSize(b),crop.slice(2));derived.push({path:p,source,crop,sha256:hash(b),method:'System.Drawing native-pixel rectangular crop; no resizing, no source edit.'});}
for(const layout of layouts.images){assert.equal(layout.regions.length,4);assert.equal(new Set(layout.regions.map(r=>r.role)).size,4);for(const r of layout.regions){assert.ok(r.x>=0&&r.y>=0&&r.x+r.width<=1500&&r.y+r.height<=1110);}for(let i=0;i<4;i++)for(let j=i+1;j<4;j++){const a=layout.regions[i],b=layout.regions[j];assert.ok(!(a.x<b.x+b.width&&a.x+a.width>b.x&&a.y<b.y+b.height&&a.y+a.height>b.y),layout.name+' instruments overlap');}}
execFileSync('git',['diff','--quiet','HEAD','--','src','tests','scripts'],{cwd:root});
const boardPath='canvas/boards/ab163f7a-13a0-4f45-b5c5-ba7638f4083d.json',board=await load(boardPath),assets=await load('canvas/assets.json'),index=await load('canvas/index.json');
const old=p=>JSON.parse(execFileSync('git',['show','HEAD:'+p],{cwd:root,encoding:'utf8',maxBuffer:12*1024*1024}));
const oldBoard=old(boardPath),oldAssets=old('canvas/assets.json'),oldIndex=old('canvas/index.json');
assert.deepEqual(board.nodes.slice(0,oldBoard.nodes.length),oldBoard.nodes);assert.deepEqual(board.edges,oldBoard.edges);assert.deepEqual(board.viewport,oldBoard.viewport);for(const [k,v]of Object.entries(oldAssets.assets))assert.deepEqual(assets.assets[k],v);assert.deepEqual(index.documents.slice(0,oldIndex.documents.length),oldIndex.documents);assert.equal(index.mainDocumentId,oldIndex.mainDocumentId);
assert.equal(board.nodes.length-oldBoard.nodes.length,8);assert.equal(Object.keys(assets.assets).length-Object.keys(oldAssets.assets).length,7);
const links=[];for(const p of ['canvas/documents/ui-v16-layouts-exploratorios.md','docs/visual/conceitos-v16/README.md']){const body=await fs.readFile(path.join(root,p),'utf8');for(const m of body.matchAll(/!?\[[^\]]*\]\(([^)]+)\)/g)){if(/^(https?:|#)/.test(m[1]))continue;const target=path.resolve(root,path.dirname(p),decodeURIComponent(m[1].split('#')[0]));links.push({from:p,to:m[1]});}}
// Verify actual PNG pixels, not just XML bounds: catches missing thumbnails/placeholders.
function pixels(bytes){const [w,h]=pngSize(bytes);assert.equal(bytes[24],8);const channels=bytes[25]===6?4:bytes[25]===2?3:0;assert.ok(channels);const parts=[];for(let p=8;p<bytes.length;){const n=bytes.readUInt32BE(p),type=bytes.toString('ascii',p+4,p+8);if(type==='IDAT')parts.push(bytes.subarray(p+8,p+8+n));p+=n+12;}const packed=zlib.inflateSync(Buffer.concat(parts)),stride=w*channels,out=Buffer.alloc(stride*h);let q=0;const paeth=(a,b,c)=>{const p=a+b-c,pa=Math.abs(p-a),pb=Math.abs(p-b),pc=Math.abs(p-c);return pa<=pb&&pa<=pc?a:pb<=pc?b:c;};for(let y=0;y<h;y++){const f=packed[q++];for(let x=0;x<stride;x++){const raw=packed[q++],a=x>=channels?out[y*stride+x-channels]:0,b=y?out[(y-1)*stride+x]:0,c=y&&x>=channels?out[(y-1)*stride+x-channels]:0;out[y*stride+x]=(raw+(f===0?0:f===1?a:f===2?b:f===3?Math.floor((a+b)/2):paeth(a,b,c)))&255;}}return(x,y)=>[...out.subarray(y*stride+x*channels,y*stride+x*channels+3)];}
const paper=[191,180,158];for(const name of filenames.filter(n=>/^0/.test(n))){const px=pixels(await fs.readFile(path.join(root,base+name+'.png')));assert.deepEqual(px(8,8),paper);}
const px=pixels(await fs.readFile(path.join(root,base+'comparacao.png')));for(const [x,y]of [[64,198],[1034,198],[64,998],[1034,998]])assert.deepEqual(px(x,y),paper);
const provenance={version:1,kind:'Reference-only visual exploration; not Godot implementation',noAiImageGenerationThisRound:true,runtimeChanged:false,method:'Original SVG composition, native-pixel crops, restricted primitive replay into Canvas2D with embedded local fonts and decoded images. No compositor screenshots or nested whole-SVG image raster.',approval:'Awaiting user layout choice; 03 is assistant recommendation, not approval.',images,derived,originals,references:sources.references,dribbble:sources.dribbble,rights:'Third-party screenshots for research only. No license for runtime or commercial redistribution is asserted. Historical v12 art is reused reference, not newly generated in this round.'};
await fs.writeFile(path.join(root,base+'provenance.json'),JSON.stringify(provenance,null,2));
for(const link of links)await fs.access(path.resolve(root,path.dirname(link.from),decodeURIComponent(link.to.split('#')[0])));
const report={passed:true,images:images.length,references:sources.references.length,derived:derived.length,textsChecked:render.renders.reduce((n,r)=>n+r.textCount,0),links:links.length,newCanvasNodes:8,oldCanvasNodesPreserved:true,oldAssetsPreserved:true,mainDocumentPreserved:true,runtimeUnchanged:true,referenceOriginalsUnchanged:true,comparisonThumbnailPixelSamples:4,linksChecked:links};
await fs.writeFile(path.join(audit,'final-audit.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({...report,linksChecked:undefined}));
