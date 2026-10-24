# Cobertura e questões abertas

[Índice](README.md) · [Fontes](Evidencias.md). Estado ao concluir a investigação de 23/10/2026. “Respondida” se refere à pergunta delimitada e à versão identificada; não significa implementação integral do sistema.

| Pergunta | Situação | Evidência / limite |
|---|---|---|
| Qual instalação foi examinada? | Respondida | Versão 1.3.0, caminhos, tamanhos e hashes no manifesto |
| Qual a tecnologia distribuída? | Respondida | Unity/IL2CPP, PE x64 sem CIL na DLL, metadata v31, manifestos |
| Como se organiza a cena de gerenciamento? | Respondida estruturalmente | Três áreas, objetos e transforms; apresentação em execução não validada |
| Quais blocos e estados o Status declara? | Respondida estruturalmente | UI_Status, EStatusUIState e cena |
| Quais atributos e regras de apresentação existem? | Parcial | Enum/campos e overrides; lista final e binding em execução desconhecidos |
| Como se relacionam heróis, pontos e grupos de habilidades? | Respondida nos dados | Chaves, grupos, custos e máximos; execução e efeito final não testados |
| Qual clique abre cada estado, e como voltar/cancelar? | Não resolvida | Bindings customizados/corpos ofuscados não recuperados; UI bloqueada |
| Que conteúdo está configurado? | Respondida no catálogo distribuído | 59 tabelas; disponível online/desbloqueado é outra questão |
| Como equipamentos, raridades, loot e receitas se relacionam? | Respondida estruturalmente | Tabelas tipadas e relações; sorteio final e carregador não demonstrados |
| Quais fórmulas exatas calculam dano, DPS, armadura e bônus? | Não resolvida | Sem corpos da DLL; não assumir unidades ou divisores |
| Qual a obtenção efetiva de pontos e regra de reembolso? | Parcial | Textos descrevem ponto por nível; tabela tem custos; execução e restrições abertas |
| Qual a frequência real de loot e tempo de cada combate? | Não resolvida | Não houve medição de sessão |
| Existe progresso offline e como é apresentado? | Respondida no contrato | Tabela e resultados por herói, tela e textos presentes |
| Qual o teto, período-base e fórmula offline? | Não resolvida | Valores de tabela sem prova de unidade/processamento |
| Como são estruturados os saves? | Respondida no contrato declarado | Classes/modelos e etapas nominais; arquivos reais não acessados |
| Save é atômico, seguro e resistente a relógio alterado? | Não resolvida | Nomes de campos não provam garantias; sem teste de falha |
| Quais opções de janela/zoom existem? | Respondida nominalmente | Enums, configuração, UI e eventos |
| Há click-through, DPI/multimonitor corretos e baixo consumo? | Não resolvida | APIs e runtime não inspecionados/medidos |
| Como funciona servidor, loja/mercado e DLC em produção? | Parcial | Flags/contratos/textos presentes; nenhuma chamada ou compra feita |
| Qual o som, fonte exata, timing e animação de cada estado? | Parcial | Recursos/nomes existem; sem reprodução nem análise audiovisual completa |
| Podemos orientar nosso novo Status com os achados? | Respondida como proposta | Hierarquia e critérios em Adaptacao_Aetherforge.md; ainda não implementada |

## Limites que afetam a interpretação

**Nativo:** REA confirmou a natureza da DLL, mas Ghidra Windows P0 não aceita DLLs. Somente o launcher teve um corpo de função decompilado. Metadata preserva declarações, não implementações.

**Interface:** a revisão automática recusou o lançamento pelo Computer Use. A captura histórica existente é útil para linguagem visual e composição, mas não substitui uma sessão atual da versão 1.3.0. Coordenadas são locais à cena e componentes customizados foram majoritariamente lidos apenas pelo cabeçalho.

**Conteúdo:** registros podem estar inativos, legados, condicionados a evento/DLC ou marcados como removidos no servidor. Um valor de dado não comprova frequência, acesso ou efeito em combate. Nomes históricos também podem divergir do significado atual: Poison em campos de UI e Chaos em enum/textos são um exemplo.

**Sessão REA:** export e fechamento foram recusados pelo ambiente, com aprovação indisponível. O relatório guarda registros inline e resumos portáteis; não existe bundle autenticado novo nem confirmação de encerramento. Ver os retornos em [Evidências](Evidencias.md).

## Próxima investigação que mais reduziria incerteza

1. Inspeção interativa autorizada do Status atual, cobrindo seus cinco estados nominais, seleção de personagem, detalhe e cancelamento.
2. Verificação de aplicação de `StatViewInfoData`: quais linhas entram no resumo/detalhe e quando zeros são ocultados.
3. Se ainda necessário, análise da DLL com um provedor que admita esse alvo, conservando hashes e relacionando metadata a endereços nativos.
4. Experimentos de combate e offline em estado de teste controlado, para medir fórmulas/unidades e arredondamento.

Nenhum desses itens é requisito para aproveitar agora a estrutura visual documentada; todos são necessários antes de afirmar equivalência funcional completa com o jogo de referência.
