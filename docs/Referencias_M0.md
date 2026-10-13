# Referências do primeiro protótipo

Pesquisa em 12/10/2026. Implementação original de Aetherforge.

## Task Bar Hero

A [página oficial na Steam](https://store.steampowered.com/app/3678970/TBH_Task_Bar_Hero/) descreve um RPG idle compacto junto à barra de tarefas, coleta e progressão automáticas, classes, habilidades e equipamento. Esses pontos orientam a leitura compacta da nossa janela e a separação entre observar a expedição e abrir seus controles.

Aplicação no M0: três personagens visíveis em uma faixa, inimigos à frente, estado resumido da expedição e um painel que pode ser expandido. A referência adicionada pelo usuário exige combate animado: avanço e golpe do tank, disparos do DPS, cura do healer, reação a impactos e caminhada entre encontros. As funções e as regras econômicas seguem o GDD de Aetherforge. A descrição pública não prova detalhes internos de rede ou progressão offline do jogo de referência; não inferimos sua arquitetura.

## Assets usados

| Pacote | Autor | Origem | Licença | Uso |
|---|---|---|---|---|
| KayKit Character Pack Adventures 1.0 | Kay Lousberg | [Repositório oficial](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0), commit `672074b73ba276876a19e8816ecdc5241817ab47` | CC0 1.0, licença preservada junto aos arquivos | Knight = tank, Rogue = DPS provisório, Mage = healer provisório; renders em oito direções |

Os personagens são placeholders de fantasia, adaptados aos papéis da equipe. O cenário industrial inicial é construído com geometria simples no protótipo. Sprites derivados ficam em `Assets/Generated`, separados dos modelos originais. O README e o arquivo de licença do pacote definem a origem; não extraímos assets de Task Bar Hero.

## Ferramentas

- [Godot 4.7.2 .NET, release oficial](https://github.com/godotengine/godot/releases/tag/4.7.2-stable).
- [.NET 10, metadados oficiais de releases](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json), SDK 10.0.401.
- [Godot: C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html), [servidores dedicados](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_dedicated_servers.html), [janelas desktop](https://docs.godotengine.org/en/stable/tutorials/ui/creating_applications.html).
