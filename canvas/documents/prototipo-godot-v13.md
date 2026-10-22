# Godot — primeira fatia jogável de A Promessa do Latão

**Status:** protótipo local executável e testado. Brass v12 GPT e a paleta quente v10 são a base de trabalho aceita; não representam aprovação final de todos os detalhes. As três pinturas originais e todos os estudos anteriores foram preservados.

## O que já funciona

- Equipe realmente percorre o corredor; a câmera acompanha. Alquimista atrás, Pistoleiro no meio e Guarda na frente.
- Cinco trechos (Entrada, Trilhos, Pressão, Ninho, Fornalha), seis tipos de inimigo e chefe. Marcha para ao encontrar inimigos e retoma após a vitória/descanso.
- Ataques, cura, escudo com mitigação real, impacto, números, vida e mortes; pausa, recuo e reinício. Rota automática completa em aproximadamente 137,45 segundos simulados e 108 unidades de deslocamento.
- Status, Herói e Expedição funcionais; três integrantes selecionáveis, retrato derivado da própria pintura v12, sete colunas de slots **indisponíveis**, cinco etapas reais. Não foram inventados itens/níveis/XP.
- Kit de cenário 3D modular com trilhos, tubulações e caldeiras; recursos reutilizados e áudio original sintetizado de protótipo.

## Imagens deste documento

São **capturas reais do viewport Godot**, 1000 × 320 no palco compacto e 1160 × 880 no Overview, copiadas sem corte/upscale das evidências de QA. Não são pinturas GPT novas, mockups estáticos propostos, meshes finais ou prova de performance. O modo Overview embute os três instrumentos para revisão; o modo normal usa palco compacto e três janelas independentes.

- Palco compacto: apresentação normal junto à barra, instrumentos inicialmente fechados.
- Marcha: equipe avançando pela Entrada.
- Capataz: combate da Fornalha, após quatro encontros vencidos.
- Conclusão: cinco encontros vencidos e controles de reinício/recuo.

Proveniência: `assets/concepts/brass-v13-godot/provenance.json` registra origem e SHA256. As pinturas v12 são referência e fornecem os retratos; **os modelos em cena são construções procedurais técnicas articuladas**, não conversões automáticas da pintura nem arte final. Nenhuma imagem de pesquisa de terceiros entrou no runtime.

## Abrir e continuar

Na raiz, **Jogar Expedicao.cmd** abre a fatia; **Revisar Expedicao.cmd** abre a apresentação desta captura; **Abrir Godot.cmd** abre o editor Godot .NET portátil.

O cadastro OhMyGame desta sessão ainda é Asset Canvas. Foi preparado **Migrar para Studio Godot.cmd** para executar com o OhMyGame fechado, com backup e possibilidade de voltar ao Canvas. A alteração não foi aplicada nesta sessão, e a conexão Studio/MCP ainda não foi verificada. Não há promessa de preview web de Godot nativo.

Instruções completas, autoria dos workers, testes e limites: [docs/studio-godot/README.md](../../docs/studio-godot/README.md).

## Verificação e pendências

Build do cliente sem erros/avisos; verificações de domínio, rota/câmera/poses em Godot, estrutura/callbacks da UI, três janelas nativas com IDs distintos e regressões API/multiplayer/transferência/persistência passaram. Capturas abertas e inspecionadas. Artefatos locais em `artifacts/studio-godot/`.

Ainda não é M1 completo: faltam modelos/texturas/rigs/clips de produção, maior riqueza/diferenciação ambiental, avaliação perceptiva do áudio e animação, cliques físicos/foco/DPI/múltiplos monitores, benchmark e integração da nova rota com economia/salvamento/API. A fatia usa animação procedural por pivôs, não Skeleton3D/AnimationTree final. A limitação a 30 FPS não é uma medição de performance.

Os três subagentes (regras, cena e interface) foram executados de verdade via Codex CLI em `workspace-write`; o orquestrador integrou, corrigiu e testou. Foram utilizadas as skills Godot C#, 3D, animação, UI, game-feel e áudio do conjunto instalado. Não houve commit, alteração de saves manuais ou redução de segurança/sandbox.
