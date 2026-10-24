// Restricted SVG replay into Canvas2D. Avoids Chromium's intermittent nested-SVG image placeholders.
// Supports only the primitives emitted by compose.mjs/research.mjs; unknown shapes fail closed.
async function rasterSvg(svg) {
 document.body.innerHTML=svg;
 await document.fonts.ready;
 const root=document.querySelector('svg'),width=+root.getAttribute('width'),height=+root.getAttribute('height');
 const outside=[...root.querySelectorAll('text')].flatMap(t=>{const r=t.getBBox();return r.x<0||r.y<0||r.x+r.width>width+.5||r.y+r.height>height+.5?[{text:t.textContent,x:r.x,y:r.y,width:r.width,height:r.height}]:[]});
 const canvas=document.createElement('canvas');canvas.width=width;canvas.height=height;
 const ctx=canvas.getContext('2d'),images=new Map();
 for(const e of root.querySelectorAll('image')){const href=e.getAttribute('href');if(!href.startsWith('data:'))throw Error('External resource');if(!images.has(href)){const im=new Image();im.src=href;await im.decode();images.set(href,im);}}
 const tile=document.createElement('canvas');tile.width=79;tile.height=61;const tc=tile.getContext('2d');tc.strokeStyle='#40332B';tc.lineWidth=.6;tc.globalAlpha=.055;tc.stroke(new Path2D('M3 12h7m42 28h8m-24 15h4'));
 const paper=ctx.createPattern(tile,'repeat');
 const attr=(e,k,d=0)=>e.hasAttribute(k)?parseFloat(e.getAttribute(k)):d;
 function paint(e){const style=getComputedStyle(e);return{fill:e.getAttribute('fill')||style.fill,stroke:e.getAttribute('stroke')||style.stroke,lineWidth:parseFloat(e.getAttribute('stroke-width')||style.strokeWidth)||1};}
 function shape(e,path){const p=paint(e);if(p.fill&&p.fill!=='none'){ctx.fillStyle=p.fill.startsWith('url(')?paper:p.fill;ctx.globalAlpha=attr(e,'fill-opacity',1);ctx.fill(path);}if(p.stroke&&p.stroke!=='none'){ctx.strokeStyle=p.stroke;ctx.lineWidth=p.lineWidth;ctx.globalAlpha=attr(e,'stroke-opacity',1);ctx.stroke(path);}}
 function visit(e,isRoot=false){ctx.save();if(e.transform?.baseVal?.numberOfItems){const m=e.transform.baseVal.consolidate().matrix;ctx.transform(m.a,m.b,m.c,m.d,m.e,m.f);}const tag=e.localName;
  if(tag==='svg'||tag==='g'){
   if(tag==='svg'&&!isRoot){const x=attr(e,'x'),y=attr(e,'y'),w=attr(e,'width'),h=attr(e,'height');ctx.translate(x,y);ctx.beginPath();ctx.rect(0,0,w,h);ctx.clip();const v=e.getAttribute('viewBox')?.split(/\s+/).map(Number);if(v){const scale=Math.min(w/v[2],h/v[3]);ctx.translate((w-v[2]*scale)/2,(h-v[3]*scale)/2);ctx.scale(scale,scale);ctx.translate(-v[0],-v[1]);}}
   for(const child of e.children)visit(child);
  }else if(tag==='rect'){const p=new Path2D();p.rect(attr(e,'x'),attr(e,'y'),attr(e,'width'),attr(e,'height'));shape(e,p);
  }else if(tag==='path'){shape(e,new Path2D(e.getAttribute('d')));
  }else if(tag==='text'){const st=getComputedStyle(e);ctx.font=`${st.fontWeight} ${st.fontSize} ${st.fontFamily}`;ctx.fillStyle=st.fill;ctx.letterSpacing=st.letterSpacing==='normal'?'0px':st.letterSpacing;ctx.textAlign=st.textAnchor==='end'?'right':st.textAnchor==='middle'?'center':'left';ctx.textBaseline='alphabetic';ctx.fillText(e.textContent,attr(e,'x'),attr(e,'y'));
  }else if(tag==='image'){const im=images.get(e.getAttribute('href')),x=attr(e,'x'),y=attr(e,'y'),w=attr(e,'width'),h=attr(e,'height');const fit=e.getAttribute('preserveAspectRatio')||'xMidYMid meet';ctx.beginPath();ctx.rect(x,y,w,h);ctx.clip();const scale=(fit.includes('slice')?Math.max:Math.min)(w/im.naturalWidth,h/im.naturalHeight);ctx.drawImage(im,x+(w-im.naturalWidth*scale)/2,y+(h-im.naturalHeight*scale)/2,im.naturalWidth*scale,im.naturalHeight*scale);
  }else if(!['defs','style','title','desc'].includes(tag))throw Error('Unsupported primitive: '+tag);
  ctx.restore();
 }
 visit(root,true);
 return{width,height,textCount:root.querySelectorAll('text').length,outside,externalResources:0,decodedImages:[...images.values()].map(i=>({width:i.naturalWidth,height:i.naturalHeight})),fonts:[...document.fonts].map(f=>({family:f.family,status:f.status,weight:f.weight})),data:canvas.toDataURL('image/png').split(',')[1]};
}
