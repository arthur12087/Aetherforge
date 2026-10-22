# Handoff de execução CLI · GPT Images · Aetherforge v11

> **Tarefa de execução, não de planejamento:** gerar arte conceitual real com GPT Images para a equipe e o bestiário. Este handoff foi preparado na sessão do editor; **nenhuma pintura v11 foi gerada nesta preparação**. A disponibilidade de GPT Images na nova sessão CLI precisa ser confirmada lá.

## 1. Pedido e decisões do usuário

O usuário escolheu **B — A Promessa do Latão**, inspirada em BioShock, e autorizou incluir **BioShock 2** como referência. Pediu novos conceitos de personagens e monstros **com GPT Images**, não maquetes ou modelos procedurais. Depois rejeitou o fundo azul/petróleo dominante e **gostou da paleta quente v10**, que deve ser mantida.

**Aprovado como direção:** B e a nova família de cor. **Ainda não aprovado:** nenhum desenho novo de personagem ou inimigo. Gerar e revisar não equivale a obter aprovação do usuário.

### Escopo obrigatório desta execução

Entregar **três pinturas conceituais novas**:

1. Equipe: Pistoleiro, Guarda da Caldeira e Alquimista do Éter.
2. Bestiário I: Rato de Fuligem, Saqueador Mecânico e Drone.
3. Bestiário II: Sentinela a Vapor, Vigia e Capataz de Ferro.

São nove sujeitos em três imagens. Não abrir um lote de nove gerações individuais nem produzir variantes decorativas sem necessidade. Não criar uma arena nova, novos equipamentos avulsos ou outros jobs pagos nesta execução sem confirmação adicional.

**Não fazer:** implementar jogo, alterar Godot/domínio/API/persistência/multiplayer/saves, criar runtime, reconstruir BioShock, diagnosticar indefinidamente o editor ou trocar GPT Images por outro gerador silenciosamente.

## 2. Onde trabalhar

Raiz do projeto Windows:

```text
C:\Users\Arthur\Documents\jogo poc mmorpg
```

Os caminhos abaixo são relativos a essa raiz, salvo os links Markdown, que são relativos a este documento.

Leitura obrigatória antes de executar:

- `AGENTS.md` na raiz.
- [Contrato do canvas](../AGENTS.md), [índice](../index.json), [manifesto de assets](../assets.json) e [board atual](../boards/ab163f7a-13a0-4f45-b5c5-ba7638f4083d.json).
- [Direção v10, cor e pesquisa](revisao-latao-v10.md).
- [Plano legível por máquina](../../docs/visual/gpt-images-v11/execution-plan.json).
- Os três arquivos de prompt completos da seção 5.

O repositório tem arquivos de jogo, mas **este pedido é apenas produção de referência visual**. Não é necessário compilar ou executar os testes de gameplay para gerar imagens/documentação.

## 3. Referências visuais e hierarquia

Abrir os arquivos de imagem, não inferir sua aparência pelo nome:

| Referência | Uso |
|---|---|
| [Menus v10](../../assets/concepts/brass-v10/01-menus-cor.png) | **Cor aceita pelo usuário**, contraste e identidade do mundo. Não reproduzir as janelas dentro das pinturas de personagem. |
| [Pesquisa BioShock 2](../../assets/concepts/brass-v10/02-pesquisa-bioshock2.png) | Função, postura, silhueta e construção do equipamento. |
| [Big Sister / Colin Fix](../../assets/concepts/brass-v10/references/colin-fix-big-sister.jpg) | Relações de proporção, organização de equipamento e pintura de conceito. Não copiar personagem, capacete ou gaiola. |
| [Ceiling Crawler / Colin Fix](../../assets/concepts/brass-v10/references/colin-fix-crawler.jpg) | Gesto e assimetria. Não copiar máscara, corpo ou mutação. |
| [Mão / Colin Fix](../../assets/concepts/brass-v10/references/colin-fix-mao.jpg) | Tecido, couro e metal com função; desgaste de contato. Não copiar a luva. |
| [Render / James Ku](../../assets/concepts/brass-v10/references/james-ku-big-sister.jpg) | Comparação de materiais. É **cinemática**, não promessa de asset de gameplay ou orçamento técnico. |

[URLs e hashes das referências](../../assets/concepts/brass-v10/references/sources.json). Fontes primárias: [Colin Fix](https://colinfix.blogspot.com/2009/04/more-bioshock-2-concept-art.html) e [James Ku](https://www.jameskuart.com/bio2).

**As imagens de terceiros são pesquisa, não material licenciado para recortar ou aplicar como textura.** Não apagar suas assinaturas/créditos, não apresentá-las como Aetherforge e não extrair peças para a entrega.

As maquetes v9 e as artes v6 são **histórico**, não base raster a reutilizar ou alvo visual a reproduzir. Não anexar as maquetes como referência principal: o usuário pediu justamente uma pintura melhor, não sua reconstrução por outro meio.

Se a ferramenta aceitar referências, usar seletivamente e explicar sua função. Não é necessário carregar todos os exemplos de franquia na chamada. Priorizar o brief escrito, os princípios de desenho e a paleta; não pedir cópia de personagem. Registrar quais imagens foram realmente anexadas, distinguindo-as das apenas consultadas.

## 4. Direção de arte comum

**Pintura conceitual de produção**, com volume convincente para orientar futura modelagem 3D, materiais legíveis e desenho intencional. Não confundir pintura volumétrica com gerar GLB, rig ou render por primitivas.

- Aetherforge é uma fantasia industrial terrestre, em Brumaférrea, não Rapture submersa.
- Adulto, pesado, melancólico, um pouco sinistro. Profissão, cansaço e sobrevivência antes de pose heroica.
- Silhuetas grandes e diferentes; equipamento funcional e desgaste localizado. Superfícies calmas entre grupos de detalhes.
- Art Déco em costuras escalonadas, placas de esmalte, nervuras e desenho institucional; não engrenagens decorativas em tudo.
- **Arco Partido:** três nervuras de arco interrompidas por uma fenda vertical. Motivo autoral discreto, não logotipo BioShock.
- Sem letras, números, marcas, legendas ou janelas nas imagens geradas. Tipografia deve ser uma camada posterior, com fontes reais.

### Paleta aceita

| Material/função | Referência |
|---|---|
| Papel de arquivo / pedra quente | `#BFB49E` |
| Fuligem quente / cavidades | `#241F1A` |
| Tabaco / carcaça | `#40332B` |
| Vinho oxidado / esmalte e seleção | `#6C3A32` |
| Latão envelhecido | `#B99B63` |
| Porcelana e linho | `#E4D8B7` |
| Éter — apenas acento pequeno | `#97A581` |

São guias de relação e material, não preenchimentos chapados obrigatórios. **Não voltar ao fundo azul/petróleo**, ao gradiente ciano/dourado, à névoa azul ou a halos luminosos. Não dourar a armadura inteira. Não saturar todos os acessórios de verde neon.

O [brief comum em inglês](../../docs/visual/gpt-images-v11/00-direcao-comum.txt) já está incorporado em cada um dos três prompts completos. **Não concatená-lo outra vez.**

## 5. Jobs prontos para enviar ao GPT Images

| Ordem | Prompt completo | Destino planejado, relativo à raiz |
|---|---|---|
| G01 | [01-equipe.txt](../../docs/visual/gpt-images-v11/01-equipe.txt) | `assets/concepts/brass-v11-gpt/generated/01-equipe.png` |
| G02 | [02-bestiario-1.txt](../../docs/visual/gpt-images-v11/02-bestiario-1.txt) | `assets/concepts/brass-v11-gpt/generated/02-bestiario-1.png` |
| G03 | [03-bestiario-2.txt](../../docs/visual/gpt-images-v11/03-bestiario-2.txt) | `assets/concepts/brass-v11-gpt/generated/03-bestiario-2.png` |

Os prompts específicos foram copiados dos nós v10 atuais, acrescidos do brief comum. O [execution-plan.json](../../docs/visual/gpt-images-v11/execution-plan.json) registra os IDs de origem e SHA256 dos textos. Ele é **um plano, não prova de geração**. Os caminhos de destino acima são esperados, não arquivos que já existam.

### G01 · equipe

Ordem **editorial da prancha**, da esquerda para a direita: Pistoleiro, Guarda e Alquimista.

- **Pistoleiro / A Inspetora dos Trilhos:** mulher adulta como proposta visual, cabelo curto, face exposta/cansada, casaco tabaco em painéis e faixa diagonal clara; rifle de pressão com coronha de madeira e uma câmara de latão escalonada. Sem cartola, pose de pin-up ou engrenagens sem função. Sexo/biografia são proposta, não mudança aprovada de cânone.
- **Guarda da Caldeira / O Porteiro:** trabalhador mais velho, trapézio largo, face humana visível sob capacete compacto angular, armadura de trabalho em fuligem/vinho, reservatório plano e martelo curto. **Escudo retangular com uma única faixa ocre gasta**. Não capacete de mergulho, vigias, broca, caixão ou escudo medieval de pipa.
- **Alquimista do Éter / O Socorrista:** adulto, capuz e capa triangular de lona/linho claro, satchel de socorro e **uma cápsula jade pequena** conectada a aplicador compacto. Não criança, Little Sister, seringa gigante ou vários frascos neon. Continua healer NPC, não classe jogável nova no MMO.

A ordem da prancha **não muda a formação de combate**: em eventual cena futura, Alquimista atrás, Pistoleiro no meio e Guarda na frente; inimigos à direita.

### G02 · bestiário I

- **Rato de Fuligem:** roedor baixo/horizontal, focinho em cunha, cauda fina longa e curva, patas traseiras tensas; poucas alterações etéreas. Não javali, ouriço, mascote ou rato de armadura.
- **Saqueador Mecânico:** autômato de recolha abandonado, tórax estreito/inclinado, avental de placas e um gancho curvo assimétrico. Não humano com máscara de Splicer.
- **Drone / Sino de Ronda:** sino invertido suspenso, dois rotores laterais protegidos e pequena lente inferior. Horizontal com centro pendente; não olho flutuante, quadricóptero ou brinquedo fofo.

### G03 · bestiário II

- **Sentinela a Vapor:** caldeira larga/baixa, venezianas horizontais, pistões curtos e punhos pesados. Não repetir Guarda, escudo ou chefe.
- **Vigia / Fiscal Mudo:** cabeça vertical de porcelana, coluna estreita e **apoio tripodal** claramente legível. Não apenas outro humanoide largo.
- **Capataz de Ferro:** prensa ambulante com torso largo, ombros de trilho, pequena cabeça, braço-martelo assimétrico e uma pequena fornalha. A ameaça cresce principalmente para os lados, não como torre que não caberia no palco.

As figuras do bestiário precisam de proeminência editorial semelhante, **não da mesma escala física**. Os nomes são os inimigos existentes; não inventar regras ou espécies extras.

## 6. Preflight da sessão CLI

1. Confirmar o diretório e ler as instruções anteriores.
2. Identificar a **ferramenta real de geração GPT Images disponível nesta sessão** e a autenticação já autorizada. Um modelo de texto responder não prova acesso à geração de imagens.
3. Os nomes `codex-cli / codex-image` são identificadores da integração do editor: **não assumir que sejam valores válidos para `codex --model`**. Não inventar comandos como `codex --image` ou flags de resolução sem conferir suporte real.
4. Usar o GPT Images disponível de fato. Registrar versão/identificador quando expostos; quando não forem expostos, escrever isso. Não adivinhar que foi `gpt-image-1`, `1.5` ou outra versão.
5. Se precisar de nova conexão paga, contratação, credencial ou alteração global para prosseguir, pedir confirmação. Nunca imprimir tokens, ler/copiar `auth.json` para a entrega ou guardar chaves no projeto.
6. Se a sessão não tiver ferramenta de imagem, **parar com o impedimento exato**. Não substituir por SVG, Three.js, Blender, Pillow desenhando personagens ou uma foto antiga. Processamento/composição desses arquivos só é aceitável **depois** de existir uma pintura real gerada.

O editor falhou anteriormente ao iniciar o sandbox local com `elevated Windows sandbox requires effective :root read access`. A CLI externa pode ou não compartilhar essa condição; isso ainda não foi comprovado. Se reproduzir o erro, relatar e parar: este handoff **não autoriza** liberar leitura de todo o computador, desativar sandbox, alterar ACLs/antivírus ou executar elevado como atalho.

Não é preciso restaurar a conexão do editor para usar uma ferramenta de imagem funcional da CLI. Se `generate_canvas_media` não existir na CLI, não tentar chamar uma ferramenta inexistente. Usar os prompts e salvar os resultados como geração externa; ver seção 9.

## 7. Execução, formato e limite de iteração

1. Executar **G01 primeiro**, usando o conteúdo integral de `01-equipe.txt`. Esta própria pintura é o teste útil da integração; não gastar uma chamada adicional com imagem de teste aleatória.
2. Salvar o arquivo real, abrir e revisar. Só continuar depois de confirmar que foi produzido um resultado utilizável.
3. Executar G02 e G03 sequencialmente, mantendo a mesma família de pintura, cor, materiais e contraste. Se o sistema aceitar referência, a imagem própria G01 pode orientar acabamento/material, **não repetir pessoas no bestiário**.
4. Pedir landscape 16:9 e resolução nativa até 2K **se suportadas**. Se o GPT Images disponível só oferecer, por exemplo, `1536 × 1024`, aceitar essa paisagem nativa, manter os corpos inteiros e registrar a dimensão real. Não esticar a imagem ou inflar resolução para alegar 2K.
5. Preferir PNG. Se a saída nativa for JPEG/WebP, preservar o original e, se necessário, converter para PNG sem deformar ou fingir transparência. Mudar a extensão não é conversão.
6. Não sobrescrever resultados prévios ou arquivos concorrentes. Guardar a versão original; uma correção deve ter sufixo como `-r02`. Selecionar a melhor versão no registro.
7. Uma correção dirigida é aceitável quando houver falha material — sujeito ausente, membro fundido, escudo incorreto, fundo azul etc. **Máximo de seis chamadas no total**, incluindo as três iniciais e reparos, antes de pedir orientação. Não gerar infinitas variantes cosméticas.
8. Registrar o prompt realmente usado, inclusive instrução de correção e referências anexadas. Não atribuir à ferramenta um resultado de montagem local.

## 8. Critérios de aceitação e revisão visual

Abrir cada arquivo produzido na ferramenta de visualização disponível. Não declarar sucesso só porque a chamada retornou um caminho ou porque o arquivo existe.

### Para as três pinturas

- [ ] Saída efetiva de GPT Images, com arquivo de imagem válido e dimensões reais registradas.
- [ ] Exatamente três sujeitos distintos por prancha, sem duplicações ou personagens extras.
- [ ] Sujeitos inteiros, margens suficientes; sem corte de cabeça, pés, arma, cauda, rotor ou martelo.
- [ ] Paleta quente aceita; sem azul/petróleo como campo dominante, halo ciano ou neon generalizado.
- [ ] Anatomia/apoio/articulação convincentes; mãos em contato funcional com ferramentas; sem membros fundidos ou acessórios atravessando o corpo de modo óbvio.
- [ ] Silhuetas claras e materiais diferentes; não aparência de primitivas rígidas, brinquedo plástico ou maquete anterior.
- [ ] Tom adulto/melancólico; nenhum herói infantilizado ou cópia de personagem da franquia.
- [ ] Sem texto, logos, números e janelas gerados na pintura.
- [ ] Não há pixels recortados de obras de terceiros ou da arte v6/v9 disfarçados como geração nova.

### Identificadores que não podem se perder

- [ ] Guarda humano + escudo retangular + faixa ocre única + reservatório plano.
- [ ] Pistoleiro estreito + faixa diagonal + casaco em painéis + rifle funcional de madeira/pressão.
- [ ] Alquimista adulto + lona clara triangular + satchel + cápsula única discreta.
- [ ] Rato é roedor horizontal; Saqueador assimétrico com gancho; Drone suspenso com dois rotores.
- [ ] Sentinela larga/baixa; Vigia vertical/tripodal; Capataz largo com braço-martelo.

Produzir, se útil, uma **prova derivada de redução** dos próprios resultados a aproximadamente 64–96 px de altura por sujeito. Isso avalia intenção de leitura, não DPI, acessibilidade, performance ou implementação no Godot. Não reduzir/sobrescrever os originais para fazer a prova.

Anotar limitações remanescentes. Um resultado revisado é `gerado e inspecionado`, não `aprovado pelo usuário`, `modelo pronto`, `rig validado` ou `gameplay implementado`.

## 9. Entrega e proveniência

### Obrigatório se as três gerações funcionarem

- Os três arquivos escolhidos em `assets/concepts/brass-v11-gpt/generated/`.
- Um registro em `assets/concepts/brass-v11-gpt/provenance.json` com, por resultado:
  - job, ferramenta/provedor/modelo reais — ou a informação de que a versão não foi exposta;
  - prompt efetivo ou arquivo que o contém; referências realmente enviadas;
  - data, caminho, formato e dimensões; identificador retornado pela ferramenta, se disponível;
  - correções/variantes e arquivo escolhido; inspeção realizada e defeitos remanescentes;
  - origem **geração externa na CLI**, quando não tiver usado a ferramenta do editor.
- Relatório em `docs/visual/gpt-images-v11/EXECUCAO.md`, distinguindo `gerado`, `corrigido`, `inspecionado`, `pendente` e `bloqueado`. Se houver falha parcial, registrar só o que de fato ocorreu.

Não criar esses registros com metadados inventados. Um identificador não exposto deve ficar explicitamente não informado. Não guardar credenciais, URLs assinadas sensíveis ou logs com tokens.

### Importação no canvas após existir imagem real

1. Reler `canvas/index.json`, `canvas/assets.json`, o board e seus schemas **imediatamente antes de editar**. Não usar cópia antiga do arquivo inteiro.
2. Acrescentar assets com IDs novos: `v11-gpt-equipe`, `v11-gpt-bestiario-1`, `v11-gpt-bestiario-2`, apontando para os PNGs reais escolhidos. Ajustar caminho se a seleção for uma revisão.
3. Acrescentar nós `type: "asset"`, `data: { "assetId": "...", "mediaType": "image" }`, com títulos de **conceito GPT gerado na CLI**, não asset pronto de runtime.
4. Preservar IDs e trabalho anterior. Os nós `v10-equipe-gpt`, `v10-bestiario-1-gpt`, `v10-bestiario-2-gpt` são fontes de prompt. Se a geração foi externa, não falsificar seu histórico ou preencher campos de resultado como se o editor tivesse executado a chamada. Pode-se acrescentar uma descrição apontando para o resultado externo real, mantendo o prompt.
5. Não editar `canvas/jobs.json` manualmente para fabricar histórico. Evitar edição simultânea do mesmo board no editor e na CLI; se houver conflito, preservar as mudanças novas e reler/mesclar em vez de reverter.
6. Se houver `canvas_check`, executar e corrigir os problemas. Na CLI sem essa ferramenta, validar JSON, unicidade de IDs, referências de nós/assets e existência dos caminhos; registrar que o validador específico do editor não estava disponível. Não inventar um comando de terminal `canvas_check`.
7. Se a importação segura não for possível, entregar os arquivos e um fragmento de registro separado, indicando importação pendente. Não arriscar sobrescrever o board para simular conclusão.

O board alvo é `ab163f7a-13a0-4f45-b5c5-ba7638f4083d`. O nó de documento deste handoff é `v11-handoff-gpt`.

## 10. Composição posterior — opcional e sem novas chamadas de geração

Depois de completar as três pinturas, é aceitável montar uma prancha de apresentação com os resultados próprios e fontes reais, preservando os PNGs originais. [Limelight e Jost / licenças](../../docs/visual/assets/fonts/v5/sources.json) já estão no projeto.

Se fizer recortes individuais da equipe ou atualizar o retrato do Herói, medir as posições **da imagem real**: não presumir terços exatos ou prometer fundo transparente. Identificar arquivos como derivados de G01. Não deformar o Guarda para caber na moldura e não redesenhar a paleta aceita.

**Não é obrigatório preencher a arena vazia**: um lineup não é uma cena lateral de combate. Não colar três retratos e fingir que se gerou a cena. Deixar essa parte pendente se não houver um pedido posterior de pintura de arena.

Em qualquer estudo de UI posterior, preservar a essência Taskbar Hero: janelas independentes Status/Herói/Expedição, inventário de sete colunas, três integrantes, cinco etapas e palco pequeno junto à barra. Manter as regras/valores já documentados, sem nível 30 numa POC limitada a 20, sem mecânica de calor ou progressão inventada.

## 11. Quando parar e como responder

**Sucesso:** três pinturas reais GPT Images, revisadas, arquivos/proveniência entregues e canvas importado com segurança ou importação claramente pendente. Apresentar links/previews e o que ainda precisa da escolha do usuário.

**Bloqueio:** falta de ferramenta GPT Images, autenticação/cobrança não autorizada ou sandbox que não inicia. Informar a chamada/tipo de erro sem segredos, o que chegou a ser produzido e a ação mínima necessária. Não declarar que a CLI resolveu a integração do editor só porque uma geração externa funcionou.

Nunca substituir o objetivo por outro pacote de maquetes, mais pesquisa sem imagens ou apenas novos prompts: os prompts já estão prontos. Se não puder gerar, dizer isso claramente.

## 12. Texto curto para iniciar a sessão

Abrir o Codex CLI na raiz do projeto e enviar:

```text
Leia integralmente canvas/documents/handoff-gpt-images-v11.md e o execution-plan.json que ele referencia. Execute os três jobs obrigatórios com a ferramenta real de GPT Images disponível nesta sessão, começando por G01. Os prompts completos já estão nos arquivos .txt indicados. Preserve a paleta quente v10 que o usuário gostou. Gere os arquivos, abra e revise as imagens, registre a proveniência real e importe no canvas sem sobrescrever trabalho concorrente. Não faça maquetes/procedurais, não altere o runtime e não finja geração se a ferramenta estiver indisponível. Se houver impedimento real de ferramenta, autenticação ou sandbox, pare e informe o erro exato sem modificar permissões globais.
```
