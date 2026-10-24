"""Read-only, bounded IL2CPP v31 declaration inventory. No native execution.

This parser recovers declarations, not bodies, calls, field runtime offsets,
native type registrations, or formulas. It excludes literal string data and
does not access player saves, Backend resources, or remote services.
"""
import argparse
import collections
import hashlib
import json
import pathlib
import re
import struct

cli = argparse.ArgumentParser(description=__doc__)
cli.add_argument('--data-dir',type=pathlib.Path,default=pathlib.Path(r'C:\Program Files (x86)\Steam\steamapps\common\TaskbarHero\TaskBarHero_Data'))
cli.add_argument('--output',type=pathlib.Path,default=pathlib.Path(__file__).resolve().parents[2]/'artifacts/taskbar-hero-rea/current/metadata')
cli.add_argument('--summary',type=pathlib.Path,help='Optional portable, focused declaration evidence JSON')
args=cli.parse_args()
BASE=args.data_dir.resolve()
OUT=args.output.resolve()
installation = BASE.parent
assert OUT != installation and installation not in OUT.parents, 'Output must be outside installed game'
assert not args.summary or (args.summary.resolve() != installation and installation not in args.summary.resolve().parents), 'Summary must be outside installed game'
OUT.mkdir(parents=True,exist_ok=True)
PATH = BASE / 'il2cpp_data/Metadata/global-metadata.dat'
b = PATH.read_bytes()
assert struct.unpack_from('<II', b) == (0xFAB11BAF, 31)
names = ['stringLiteral','stringLiteralData','string','events','properties','methods',
         'parameterDefaultValues','fieldDefaultValues','fieldAndParameterDefaultValueData',
         'fieldMarshaledSizes','parameters','fields','genericParameters',
         'genericParameterConstraints','genericContainers','nestedTypes','interfaces',
         'vtableMethods','interfaceOffsets','typeDefinitions','images','assemblies',
         'fieldRefs','referencedAssemblies','attributeData','attributeDataRange',
         'unresolvedIndirectCallParameterTypes','unresolvedIndirectCallParameterRanges',
         'windowsRuntimeTypeNames','windowsRuntimeStrings','exportedTypeDefinitions']
tables = {n: dict(zip(('offset','bytes'), struct.unpack_from('<II',b,8+8*i))) for i,n in enumerate(names)}
for t in tables.values():
    assert 0 <= t['offset'] <= len(b) and t['offset'] + t['bytes'] <= len(b)
def rows(table, fmt):
    t=tables[table]; size=struct.calcsize(fmt)
    assert t['bytes'] % size == 0
    return [struct.unpack_from(fmt,b,p) for p in range(t['offset'],t['offset']+t['bytes'],size)]
def string(i):
    t=tables['string']; assert 0 <= i < t['bytes']
    start=t['offset']+i; end=b.index(0,start,t['offset']+t['bytes'])
    return b[start:end].decode('utf-8',errors='strict')
types=rows('typeDefinitions','<16i8HII')
methods=rows('methods','<6iI4H')
fields=rows('fields','<3i')
parameters=rows('parameters','<3i')
images=rows('images','<10i')
validations=collections.Counter()
for i,t in enumerate(types):
    string(t[0]); string(t[1]); assert (t[25] >> 24) == 2
    validations['type_names_and_tokens']+=1
    for j in range(t[9],t[9]+t[16]) if t[16] else []:
        assert methods[j][1] == i and (methods[j][6] >> 24) == 6
        validations['method_owner_and_tokens']+=1
    for j in range(t[8],t[8]+t[18]) if t[18] else []:
        string(fields[j][0]); assert (fields[j][2] >> 24) == 4
        validations['field_names_and_tokens']+=1
for m in methods:
    string(m[0]); assert m[10] == 0 or 0 <= m[4] <= len(parameters)-m[10]
    validations['method_name_and_parameter_ranges']+=1
for p in parameters:
    string(p[0]); assert (p[1] >> 24) == 8
    validations['parameter_names_and_tokens']+=1
def method(j):
    m=methods[j]
    return {'index':j,'offset':tables['methods']['offset']+36*j,'name':string(m[0]),
            'token':f'0x{m[6]:08x}','parameter_names':[string(parameters[p][0]) for p in range(m[4],m[4]+m[10])] if m[10] else [],
            'return_type_registration_index':m[2]}
def type_record(i):
    t=types[i]
    return {'index':i,'offset':tables['typeDefinitions']['offset']+88*i,
            'namespace':string(t[1]),'name':string(t[0]),'token':f'0x{t[25]:08x}',
            'methods':[method(j) for j in range(t[9],t[9]+t[16])] if t[16] else [],
            'fields':[{'index':j,'offset':tables['fields']['offset']+12*j,'name':string(fields[j][0]),
                       'type_registration_index':fields[j][1],'token':f'0x{fields[j][2]:08x}'} for j in range(t[8],t[8]+t[18])] if t[18] else []}
img=[]
for v in images:
    img.append({'name':string(v[0]),'type_start':v[2],'type_count':v[3]})
game=next(v for v in img if v['name']=='Assembly-CSharp.dll')
records=[type_record(i) for i in range(game['type_start'],game['type_start']+game['type_count'])]
def write(name,value):
    (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
write('declarations-game.json',records)
write('inventory.json',{'source':str(PATH),'sha256':hashlib.sha256(b).hexdigest(),'size':len(b),'magic':'0xFAB11BAF','version':31,
                       'tables':tables,'images':img,'counts':{'types':len(types),'methods':len(methods),'fields':len(fields),'parameters':len(parameters)},
                       'validations':dict(validations),'limitations':['Declarations only; no code bodies or native call graph.',
                       'Native type registration indices are not resolved to signatures.',
                       'Some game symbols are obfuscated; no deobfuscation attempted.',
                       'No literal string table exported; no player saves or Backend credential resources read.']})
runtime=json.loads((BASE/'RuntimeInitializeOnLoads.json').read_text(encoding='utf-8'))
write('runtime-initializers-game.json',[v for v in runtime['root'] if v['assemblyName']=='Assembly-CSharp'])
assembly=json.loads((BASE/'ScriptingAssemblies.json').read_text(encoding='utf-8'))
write('scripting-assemblies.json',assembly)
plugins=[{'file':str(p.relative_to(BASE)),'size':p.stat().st_size} for p in (BASE/'Plugins').rglob('*') if p.is_file()]
write('plugins.json',plugins)
write('boot-config.json',{'app_info':(BASE/'app.info').read_text(encoding='utf-8'),'boot_config':(BASE/'boot.config').read_text(encoding='utf-8')})
focus=re.compile(r'stat|hero|combat|offline|save|window|desktop|^Damage|Health|^EUNIT|^EATTACK',re.I)
selected=[r for r in records if focus.search(r['name']) and not r['name'].startswith('<')]
write('declarations-focus.json',selected)
lines=[]
for r in selected:
    lines += [f"[{r['index']}] {r['namespace']}.{r['name']} @0x{r['offset']:x}",
              'FIELDS: '+', '.join(f['name'] for f in r['fields']),
              'METHODS: '+', '.join(m['name']+'('+', '.join(m['parameter_names'])+')' for m in r['methods']), '']
(OUT/'declarations-focus.txt').write_text('\n'.join(lines),encoding='utf-8')
if args.summary:
    selected_indices={11,12,48,54,55,111,557,635,642,643,645,723,737,744,771,773,831,832,865,889,892,899,900,
                      1074,1075,1085,1373,1382,1383,1384,1404,1405,1406,1442,1553,
                      1689,1723,1764,1765,1766,1767,1841,1882,1919,1920,1921,1957,1958,2125,2132}
    compact=[]
    for r in records:
        if r['index'] in selected_indices:
            compact.append({'id':f"M01:T{r['index']}",'index':r['index'],'namespace':r['namespace'],'name':r['name'],
                            'metadata_offset':r['offset'],'token':r['token'],'field_count':len(r['fields']),
                            'method_count':len(r['methods']),'fields':[f['name'] for f in r['fields']],
                            'methods':[m['name']+'('+', '.join(m['parameter_names'])+')' for m in r['methods']]})
    summary={'evidence_id':'M01','source_relative_to_installation':'TaskBarHero_Data/il2cpp_data/Metadata/global-metadata.dat',
             'sha256':hashlib.sha256(b).hexdigest(),'size':len(b),'metadata_version':31,'validation_counts':dict(validations),
             'evidence_type':'Static declarations, not executed behavior or decompiled method bodies',
             'limitations':['Native type registrations unresolved; method signatures include parameter names only.',
                            'Some symbols are obfuscated. No formulas or call graph recovered.',
                            'No player saves, literal string table, Backend settings, credentials or remote data read.'],
             'counts':{'metadata_images':len(images),'game_types':len(records),'all_types':len(types),'methods':len(methods),'fields':len(fields),'parameters':len(parameters)},
             'declarations':compact,'game_runtime_initializers':json.loads((OUT/'runtime-initializers-game.json').read_text(encoding='utf-8')),
             'M02_scripting_assemblies':assembly,'M04_plugins':plugins,
             'M04_boot_config':json.loads((OUT/'boot-config.json').read_text(encoding='utf-8')),
             'manifest_sha256':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in [BASE/'ScriptingAssemblies.json',BASE/'RuntimeInitializeOnLoads.json',BASE/'app.info',BASE/'boot.config']}}
    args.summary.parent.mkdir(parents=True,exist_ok=True)
    args.summary.write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'size':len(b),'sha256':hashlib.sha256(b).hexdigest(),'counts':{'game_types':len(records),'all_types':len(types),'methods':len(methods),'fields':len(fields)},'validated':dict(validations),'output':str(OUT)},ensure_ascii=False))
