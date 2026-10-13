import fs from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';
import { Workbook, SpreadsheetFile } from '@oai/artifact-tool';

const dir = fileURLToPath(new URL('../../outputs/aetherforge-v03/', import.meta.url));
await fs.mkdir(dir, { recursive: true });
const wb = Workbook.create();
const economy = wb.worksheets.add('Economia');
const inputs = wb.worksheets.add('Parametros');
const mining = wb.worksheets.add('Mineracao');
const sheets = [economy, inputs, mining];
const params = [
  ['Contas totais', 1000, 'contas', 'Hipótese de população para dimensionar fluxos.'],
  ['Participação diária', 0.6, '% das contas', 'Contas que geram ou consomem no cenário diário.'],
  ['Horas de idle por conta/dia', 8, 'h/dia', 'Tempo total das duas atividades, sem simultaneidade.'],
  ['Fração do tempo em caça', 0.5, '% do tempo', 'O tempo restante é mineração.'],
  ['Vitórias comuns na caça/h', 24, 'vitórias/h', 'Hipótese de permanência; sem chefes no cenário.'],
  ['Moedas por vitória comum', 10, 'moedas', 'Etapa representativa, ajustar com telemetria.'],
  ['Sucata por vitória comum', 1.5, 'unidades', 'Média inicial das etapas da caça.'],
  ['Teto de moedas idle/dia', 500, 'moedas/dia', 'Cap de emissão, não cap de transferência.'],
  ['Moedas MMORPG por conta/dia', 120, 'moedas/dia', 'Hipótese de recompensas recorrentes.'],
  ['Etapa de mineração', 3, 'etapa 1 a 5', 'Cenário estacionário na etapa escolhida.'],
  ['Extração por ciclo', 60, 'segundos', 'Tempo produtivo, antes da emboscada.'],
  ['Chance de emboscada', 0.2, '% dos ciclos', 'Acrescenta tempo; sem recursos extra.'],
  ['Combate médio da emboscada', 30, 'segundos', 'Hipótese de duração; não simula derrotas.'],
  ['Compartilhar minério + sucata', 200, 'unidades/dia', 'Cap combinado por conta.'],
  ['Compartilhar cristais', 12, 'unidades/dia', 'Cap independente por conta.'],
  ['Pontos reparados por conta/dia', 120, 'pontos/dia', 'Somatório de pontos de todas as peças.'],
  ['Custo de reparo por ponto', 2, 'moedas/ponto', 'Sem consumo de material na POC.'],
  ['Melhorias simples por conta/dia', 2, 'crafts/dia', 'Hipótese de demanda recorrente.'],
  ['Moedas por melhoria', 60, 'moedas/craft', 'Receita simples, separada do Regulador.'],
  ['Minério por melhoria', 10, 'unidades/craft', 'Receita simples.'],
  ['Sucata por melhoria', 2, 'unidades/craft', 'Receita simples.'],
  ['Cristais por melhoria', 1, 'unidades/craft', 'Receita simples.'],
  ['Consumíveis usados por conta/dia', 3, 'unidades/dia', 'Hipótese de gasto em moedas comuns.'],
  ['Moedas por consumível', 20, 'moedas/unidade', 'Preço de design, não observado.'],
  ['Vendas por conta/dia', 1, 'vendas/dia', 'Fluxo hipotético; exige compradores com saldo.'],
  ['Preço médio de venda', 10, 'premium/venda', 'Preço hipotético, sem câmbio oficial.'],
  ['Coeficiente da taxa comum', 0.5, 'moedas/premium', 'Aplicado ao preço, com arredondamento para cima.'],
  ['Taxa mínima de venda', 5, 'moedas/venda', 'Pago pelo vendedor na conclusão.'],
  ['Premium de teste injetado', 0, 'premium/conta/dia', 'Entrada explícita; não vem do gameplay.'],
  ['Reguladores por conta/dia', 0, 'crafts/dia', 'Quest única; zero no cenário recorrente.'],
  ['Moedas por Regulador', 120, 'moedas/craft', 'Mais projeto e núcleo narrativo, não modelados em volume.'],
  ['Minério por Regulador', 20, 'unidades/craft', 'Receita da primeira quest.'],
  ['Sucata por Regulador', 5, 'unidades/craft', 'Receita da primeira quest.'],
  ['Cristais por Regulador', 2, 'unidades/craft', 'Receita da primeira quest.'],
  ['Sucata de desmontagem/dia', 0, 'unidades/dia', 'Fonte separada, preencher após medir drops/desmontagem.'],
];
const row = Object.fromEntries(params.map((p,i)=>[p[0],i+7]));
const ref = key => `'Parametros'!$B$${row[key]}`;
inputs.getRange('A2').values = [['Aetherforge: parâmetros econômicos']];
inputs.getRange('A4').values = [['Hipóteses de design do GDD v0.3, seção 15.6. Não são dados observados.']];
inputs.getRange('A6:D6').values = [['Parâmetro', 'Valor editável', 'Unidade', 'Uso no modelo']];
inputs.getRange(`A7:D${params.length+6}`).values = params;
inputs.getRange(`B7:B${params.length+6}`).format.fill = '#FFF2CC';
inputs.getRange('A44').values = [['Células amarelas são editáveis. Caça e mineração dividem uma única equipe.']];
inputs.getRange('A45').values = [['Produção contínua estimada; derrotas, chefes e concessões únicas da quest não entram nas taxas.']];

mining.getRange('A2').values = [['Mineração: taxas por hora de extração']];
mining.getRange('A4').values = [['Fonte: GDD v0.3, seção 15.3. Valores ajustáveis por playtest.']];
mining.getRange('A6:E11').values = [
  ['Etapa', 'Local', 'Minério/h', 'Sucata/h', 'Cristal/h'],
  [1,'Escavação rasa',30,8,0], [2,'Filão de cobre',42,10,0],
  [3,'Veio etéreo',54,12,2], [4,'Bolsão instável',66,14,4],
  [5,'Câmara cristalina',78,16,6],
];
mining.getRange('C7:E11').format.fill = '#FFF2CC';
mining.getRange('A14').values = [['Emboscadas ampliam o tempo de ciclo. Chefes e derrotas reduzem ainda mais o rendimento real.']];
mining.getRange('A15').values = [['O modelo usa médias fracionárias; o servidor credita inteiros e conserva os restos de extração.']];

economy.getRange('A2').values = [['Aetherforge: economia global diária']];
economy.getRange('A4').values = [['Cenário de design. Moedas comuns e premium são contabilizadas separadamente.']];
economy.getRange('A6:C6').values = [['Indicador', 'Por conta ativa/dia', 'Global/dia']];
const outputRows = [
  ['Contas ativas','="n.a."','=B25'],
  ['Moedas comuns geradas','=SUM(B33:B34)','=B8*$B$25'],
  ['Moedas comuns consumidas','=SUM(B35:B39)','=B9*$B$25'],
  ['Variação líquida de moedas','=B8-B9','=B10*$B$25'],
  ['Consumo / geração','=IF(B8=0,"n.a.",B9/B8)','=B11'],
  ['Premium novo de teste',`=${ref('Premium de teste injetado')}`,'=B12*$B$25'],
  ['Premium negociado (redistribuição)',`=${ref('Vendas por conta/dia')}*${ref('Preço médio de venda')}`,'=B13*$B$25'],
];
for (let i=0;i<outputRows.length;i++) {
  economy.getRange(`A${i+7}`).values = [[outputRows[i][0]]];
  economy.getRange(`B${i+7}:C${i+7}`).formulas = [[outputRows[i][1],outputRows[i][2]]];
}
economy.getRange('A16:F16').values = [['Material', 'Produzido/conta', 'Compartilhado/conta', 'Consumido/conta', 'Saldo/conta', 'Saldo global']];
const materialFormula = [
  ['Minério','=B42','=B46','=B49','=B17-D17','=E17*$B$25'],
  ['Sucata','=B43','=B47','=B50','=B18-D18','=E18*$B$25'],
  ['Cristal','=B44','=B48','=B51','=B19-D19','=E19*$B$25'],
];
for(let i=0;i<3;i++) {
  economy.getRange(`A${17+i}`).values = [[materialFormula[i][0]]];
  economy.getRange(`B${17+i}:F${17+i}`).formulas = [materialFormula[i].slice(1)];
}
economy.getRange('A21').values = [['Compartilhar muda disponibilidade; não aumenta o estoque global. Saldos negativos indicam demanda maior que oferta.']];
economy.getRange('A22').values = [['Premium negociado exige saldo anterior ou injeção no teste. Seu efeito líquido na oferta global é zero.']];
economy.getRange('A24:C24').values = [['Cálculo diário por conta', 'Resultado', 'Unidade']];
const buildRows = [
  ['Contas ativas',`=${ref('Contas totais')}*${ref('Participação diária')}`,'contas'],
  ['Horas de caça',`=${ref('Horas de idle por conta/dia')}*${ref('Fração do tempo em caça')}`,'h/dia'],
  ['Horas de mineração',`=${ref('Horas de idle por conta/dia')}-B26`,'h/dia'],
  ['Vitórias de caça',`=B26*${ref('Vitórias comuns na caça/h')}`,'vitórias/dia'],
  ['Ciclo médio de mineração',`=${ref('Extração por ciclo')}+${ref('Chance de emboscada')}*${ref('Combate médio da emboscada')}`,'segundos'],
  ['Minério por ciclo',`=VLOOKUP(${ref('Etapa de mineração')},'Mineracao'!$A$7:$E$11,3,FALSE)*${ref('Extração por ciclo')}/3600`,'unidades/ciclo'],
  ['Sucata por ciclo',`=VLOOKUP(${ref('Etapa de mineração')},'Mineracao'!$A$7:$E$11,4,FALSE)*${ref('Extração por ciclo')}/3600`,'unidades/ciclo'],
  ['Cristais por ciclo',`=VLOOKUP(${ref('Etapa de mineração')},'Mineracao'!$A$7:$E$11,5,FALSE)*${ref('Extração por ciclo')}/3600`,'unidades/ciclo'],
  ['Emissão de moedas idle',`=MIN(B28*${ref('Moedas por vitória comum')},${ref('Teto de moedas idle/dia')})`,'moedas/dia'],
  ['Emissão de moedas MMORPG',`=${ref('Moedas MMORPG por conta/dia')}`,'moedas/dia'],
  ['Reparos',`=${ref('Pontos reparados por conta/dia')}*${ref('Custo de reparo por ponto')}`,'moedas/dia'],
  ['Craft de melhorias',`=${ref('Melhorias simples por conta/dia')}*${ref('Moedas por melhoria')}`,'moedas/dia'],
  ['Craft do Regulador',`=${ref('Reguladores por conta/dia')}*${ref('Moedas por Regulador')}`,'moedas/dia'],
  ['Consumíveis',`=${ref('Consumíveis usados por conta/dia')}*${ref('Moedas por consumível')}`,'moedas/dia'],
  ['Taxas do mercado',`=${ref('Vendas por conta/dia')}*MAX(${ref('Taxa mínima de venda')},ROUNDUP(${ref('Preço médio de venda')}*${ref('Coeficiente da taxa comum')},0))`,'moedas/dia'],
  ['Ciclos de mineração','=B27*3600/B29','ciclos/dia'],
  ['Sucata por caça',`=B28*${ref('Sucata por vitória comum')}`,'unidades/dia'],
  ['Minério produzido','=B40*B30','unidades/dia'],
  ['Sucata produzida',`=B40*B31+B41+${ref('Sucata de desmontagem/dia')}`,'unidades/dia'],
  ['Cristais produzidos','=B40*B32','unidades/dia'],
  ['Compartilhamento combinado',`=MIN(ROUNDDOWN(SUM(B42:B43),0),${ref('Compartilhar minério + sucata')})`,'unidades/dia'],
  ['Minério compartilhado','=IF(SUM(B42:B43)=0,0,ROUNDDOWN(B45*B42/SUM(B42:B43),0))','unidades/dia'],
  ['Sucata compartilhada','=MIN(ROUNDDOWN(B43,0),B45-B46)','unidades/dia'],
  ['Cristais compartilhados',`=MIN(ROUNDDOWN(B44,0),${ref('Compartilhar cristais')})`,'unidades/dia'],
  ['Consumo de minério',`=${ref('Melhorias simples por conta/dia')}*${ref('Minério por melhoria')}+${ref('Reguladores por conta/dia')}*${ref('Minério por Regulador')}`,'unidades/dia'],
  ['Consumo de sucata',`=${ref('Melhorias simples por conta/dia')}*${ref('Sucata por melhoria')}+${ref('Reguladores por conta/dia')}*${ref('Sucata por Regulador')}`,'unidades/dia'],
  ['Consumo de cristais',`=${ref('Melhorias simples por conta/dia')}*${ref('Cristais por melhoria')}+${ref('Reguladores por conta/dia')}*${ref('Cristais por Regulador')}`,'unidades/dia'],
];
for(let i=0;i<buildRows.length;i++) {
  economy.getRange(`A${i+25}`).values=[[buildRows[i][0]]];
  economy.getRange(`B${i+25}`).formulas=[[buildRows[i][1]]];
  economy.getRange(`C${i+25}`).values=[[buildRows[i][2]]];
}
economy.getRange('A53').values=[['Compartilhamento proporcional é um cenário de demanda; o jogador escolhe quais lotes mover.']];
economy.getRange('A54').values=[['Consumo de materiais considera o estoque total da conta, incluindo o uso no próprio idle.']];
economy.getRange('A55').values=[['A primeira quest fornece faltantes uma vez; essas concessões não representam produção diária recorrente.']];

for(const [sheet,bound] of [[economy,'A1:F55'],[inputs,'A1:D45'],[mining,'A1:E15']]) {
  sheet.showGridLines=false;
  sheet.getRange(bound).format.font={name:'Arial',size:10,color:'#34312E'};
  sheet.getRange(bound).format.verticalAlignment='center';
  sheet.getRange(bound).format.rowHeight=24;
  sheet.getRange('A2').format.font={name:'Arial',size:15,bold:true,color:'#34312E'};
  sheet.getRange('A2:F2').format.rowHeight=30;
}
economy.tabColor='#6B4226'; inputs.tabColor='#B98755';
for(const [sheet,ranges] of [[economy,['A6:C6','A16:F16','A24:C24']],[inputs,['A6:D6']],[mining,['A6:E6']]])
  for(const range of ranges) sheet.getRange(range).format={fill:'#6B4226',font:{name:'Arial',size:10,bold:true,color:'#FFFFFF'},horizontalAlignment:'center',wrapText:true,rowHeight:34};
economy.getRange('A1:A55').format.columnWidth=43;
economy.getRange('B1:F55').format.columnWidth=21;
economy.getRange('B7:F51').setNumberFormat('#,##0.00');
economy.getRange('B7:C10').setNumberFormat('#,##0');
economy.getRange('B11:C11').setNumberFormat('0.0%');
economy.getRange('B12:C13').setNumberFormat('#,##0');
economy.getRange('B7:F51').format.horizontalAlignment='right';
economy.getRange('A16:F16').format.horizontalAlignment='center';
economy.getRange('C25:C51').setNumberFormat('@');
economy.getRange('C25:C51').format.horizontalAlignment='left';
economy.getRange('B10:C10').format.font.bold=true;
economy.getRange('E17:F19').conditionalFormats.add('cellIs',{operator:'lessThan',formula:0,format:{fill:'#FCE4D6',font:{color:'#9C0006'}}});
economy.getRange('B10:C10').conditionalFormats.add('cellIs',{operator:'lessThan',formula:0,format:{fill:'#FCE4D6',font:{color:'#9C0006'}}});
inputs.getRange('A1:A45').format.columnWidth=40;
inputs.getRange('B1:B45').format.columnWidth=18;
inputs.getRange('C1:C45').format.columnWidth=23;
inputs.getRange('D1:D45').format.columnWidth=62;
inputs.getRange('D7:D41').format.wrapText=true;
inputs.getRange('A7:D41').format.rowHeight=32;
inputs.getRange('B7:B41').setNumberFormat('#,##0.00');
for(const key of ['Participação diária','Fração do tempo em caça','Chance de emboscada']) {
  const rr=`B${row[key]}`;
  inputs.getRange(rr).setNumberFormat('0.0%');
  inputs.getRange(rr).dataValidation={rule:{type:'decimal',operator:'between',formula1:0,formula2:1}};
}
inputs.getRange(`B${row['Etapa de mineração']}`).dataValidation={rule:{type:'whole',operator:'between',formula1:1,formula2:5}};
inputs.getRange(`B${row['Horas de idle por conta/dia']}`).dataValidation={rule:{type:'decimal',operator:'between',formula1:0,formula2:24}};
inputs.freezePanes.freezeRows(6);
mining.getRange('A1:A15').format.columnWidth=10;
mining.getRange('B1:B15').format.columnWidth=29;
mining.getRange('C1:E15').format.columnWidth=19;
mining.getRange('C7:E11').setNumberFormat('#,##0');
for(const [sheet,rows,end] of [[economy,[4,21,22,53,54,55],'F'],[inputs,[4,44,45],'D'],[mining,[4,14,15],'E']]) {
  for(const r of rows) {
    // Notes flow through the empty cells to the right of the working tables.
    const text=sheet.getRange(`A${r}`).values[0][0];
    sheet.getRange(`A${r}`).values=[[text]];
    sheet.getRange(`A${r}:${end}${r}`).format.rowHeight=28;
    sheet.getRange(`A${r}`).format.font={name:'Arial',size:10,italic:true,color:'#666666'};
  }
}

wb.recalculate();
const value=(sheet,cell)=>sheet.getRange(cell).values[0][0];
assert.equal(value(economy,'B10'),195);
assert.equal(value(economy,'C10'),117000);
assert.ok(Math.abs(value(economy,'B42')-196.36363636363637)<0.00001);
assert.equal(value(economy,'B46')+value(economy,'B47'),200);
inputs.getRange('B7').values=[[2000]];
assert.equal(value(economy,'C10'),234000);
inputs.getRange('B7').values=[[1000]];
inputs.getRange(`B${row['Horas de idle por conta/dia']}`).values=[[0]];
assert.equal(value(economy,'B42'),0);
assert.equal(value(economy,'B46'),0);
inputs.getRange(`B${row['Horas de idle por conta/dia']}`).values=[[8]];
inputs.getRange(`B${row['Etapa de mineração']}`).values=[[1]];
assert.equal(value(economy,'B44'),0);
inputs.getRange(`B${row['Etapa de mineração']}`).values=[[3]];
wb.recalculate();
console.log((await wb.inspect({kind:'table',range:'Economia!A6:F19',include:'values,formulas',tableMaxRows:14,tableMaxCols:6,maxChars:4500})).ndjson);
const errors=await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:30},maxChars:2000});
console.log(errors.ndjson);
for(const [sheetName,range,file] of [['Economia','A1:F55','economia.png'],['Parametros','A1:D45','parametros.png'],['Mineracao','A1:E15','mineracao.png']]) {
  const preview=await wb.render({sheetName,range,scale:1,format:'png'});
  await fs.writeFile(dir+file,new Uint8Array(await preview.arrayBuffer()));
}
await (await SpreadsheetFile.exportXlsx(wb)).save(dir+'Economia_Aetherforge_POC.xlsx');
console.log('Planilha exportada; testes de população, horas zero, etapa sem cristais e cap combinado passaram.');
