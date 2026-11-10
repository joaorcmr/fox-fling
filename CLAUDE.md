# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## O projeto

**Revoada** é um tower defense 2D com visão de cima, feito em Unity **6000.6.4f1**. A raiz do repositório é o próprio projeto Unity. Pássaros voam por uma trilha até a Torre dos Porcos. O jogador compra porcos com penas e decide entre defesa e o Ninho de Penas (income). É um trabalho de Jogos Digitais do Insper: a resolução é fixa em **600×480** (regra da disciplina) e o build é WebGL para o itch.io. As regras, os números, os porcos e os pássaros estão descritos no `README.md`.

Código, comentários, interface e commits são em **português**, e os identificadores também (sem acento: `Passaro`, `Simulacao`). Não há namespaces: todas as classes ficam no namespace global. Os arquivos são UTF-8 sem BOM, com quebra de linha CRLF. Os commits seguem o padrão `feat: ...`, `fix: ...`, `docs: ...`, `chore: ...`.

O repositório se chama `fox-fling` porque abrigava um jogo anterior, o Fox Fling, que hoje só existe no histórico do git.

## Comandos

Não há linter nem build fora do Unity: tudo passa pelo Editor.

**Menu `Revoada` do Editor** (`Editor/ConstrutorRevoada.cs`):
- *Reconstruir tudo*: gera de novo os prefabs e as duas cenas a partir do código e reimporta a arte. Mantém os números dos assets de dados que já existem.
- *Restaurar números padrão*: sobrescreve os assets de dados com os valores de `Editor/DefinicoesPadrao.cs`.
- *Balanceamento (robôs)*: escreve no Console a tabela dos robôs com os números atuais.
- *Pular para a onda 18 (robô)*: com o jogo em Play, o robô Equilibrado joga sozinho até a onda 18.
- *Build WebGL (itch.io)*: gera `Builds/WebGL`. Pelo terminal: `unity build --target WebGL --execute-method ConstrutorRevoada.BuildWebGL`.

**Testes** (Unity Test Framework, em `Window > General > Test Runner`):
- EditMode, em `Assets/Revoada/Testes`: regras, dados reais e robôs de balanceamento.
- PlayMode, em `Assets/Revoada/TestesPlay`: carregam a cena Jogo e rodam quadros.

Com o Editor aberto, a CLI `unity` (pacote `com.unity.pipeline`) controla o Editor:

```sh
unity --no-banner command --result-only test run --mode EditMode                         # todos os EditMode
unity --no-banner command --result-only test run --mode EditMode --filter SelecaoTests   # um teste ou classe (nome parcial)
unity --no-banner command --result-only test run --mode PlayMode --async_tests true      # PlayMode só roda assíncrono...
unity --no-banner command --result-only test status                                      # ...e o resultado sai aqui
unity --no-banner command --result-only console status                                   # compilou? quantos erros?
unity --no-banner command editor invoke_menu --path "Revoada/Reconstruir tudo"
```

Com o Editor fechado (o Unity não abre o mesmo projeto duas vezes), use o modo batch padrão do Unity:
`Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Temp/resultados.xml [-testFilter SelecaoTests]`.

**Arte e sons** são gerados por código:
- `python3 Tools/gerar_arte.py` precisa do Pillow e escreve em `Assets/Revoada/Compartilhado/Arte`. O urubu e a arara saem dos originais em `Compartilhado/Arte/Fontes`, que o importador do Unity ignora. O script usa fontes do macOS (`/System/Library/Fonts/...`); em outro sistema, o texto do logo sai com a fonte padrão.
- `python3 Tools/gerar_sons.py` usa só a biblioteca padrão e escreve em `Assets/Revoada/Sons/Clipes`.

## Arquitetura

### Simulação em C# puro; o Unity só escuta

O jogo inteiro roda na classe `Simulacao`, que não é `MonoBehaviour`:
- Avança em passos fixos de 1/60 s (`Passo()`), sempre na mesma ordem: nascimentos → pássaros andam → porcos atiram → tiros voam → limpeza → fim de onda → Auto.
- Avisa por eventos o que aconteceu (`PassaroEstourou`, `TorreAtingida`, `FimDeJogo`...).
- Não depende de quadros, de física nem de sorte. Por isso os testes e os robôs jogam partidas inteiras sem dar Play.
- Os comandos do jogador (`Comprar`, `Melhorar`, `Vender`, `SoltarOnda`) são imediatos e devolvem um `Resultado`, por isso funcionam com o jogo pausado.

A camada Unity só lê o estado e reage aos eventos. `Jogo` (um MonoBehaviour com `[DefaultExecutionOrder(-100)]`) é quem monta a partida. No `Awake`, ele cria:
- a `Simulacao`, a partir dos ScriptableObjects (via `ConversorDados`);
- o `RelogioPartida`: pausa e 1×/2×/3×, que são só o `Time.timeScale`;
- o `CatalogoTipos`: sprite e cor de cada espécie;
- a `Selecao`: o que o jogador tem na mão;
- o `FimDePartida`: grava o resultado no Progresso.

No `FixedUpdate`, o `Jogo` chama `Sim.Passo()`. Os outros componentes chegam a tudo por `jogo.Sim`, `jogo.Relogio`, `jogo.Selecao` e `jogo.Tipos`. `Jogo.Atual` é o acesso estático, usado pelos testes PlayMode e pelo menu do robô. Os visuais interpolam entre os passos fixos (`GerenteVisuais`, com o alfa entre `Time.fixedTime` e `Time.time`). A troca de cena passa sempre por `Navegacao`, que chama a `Transicao` (um escurecer de 0,3 s). É a `Transicao` que volta o `timeScale` para 1.

### Pastas por assunto (Screaming Architecture) e uma responsabilidade por pasta (SoC)

`Assets/Revoada/` tem uma pasta por assunto do jogo: `Passaros`, `Porcos`, `Combate`, `Ondas`, `Penas`, `Mapa`, `Torre`, `Partida`, `Menu`, `Dicas`, `Sons`, `Progresso` e `Compartilhado`. Dentro de cada uma, as subpastas são sempre as mesmas:

- `Regras/`: C# puro, com o estado e as regras. Compila no assembly **`Revoada.Nucleo`**, definido em `Partida/Regras/Revoada.Nucleo.asmdef`. As outras pastas `Regras/` entram nele por um `Revoada.Nucleo.asmref`, então uma pasta `Regras/` nova também precisa de um. Esse assembly não referencia o resto do jogo, e assim uma regra não consegue depender de visual, som ou interface.
- `Dados/`: os ScriptableObjects, com a classe e os `.asset`. Os números em si são classes `[Serializable]` (`DadosPassaro`, `DadosPorco`...) que ficam em `Regras/`. O asset só os embrulha junto com o que é de desenho (sprites, cor).
- `Visual/`, `Interface/` (uGUI) e `Prefabs/`, além de `Partida/Entrada/` para teclado e mouse.
- Todo o resto compila no assembly `Revoada` (`Assets/Revoada/Revoada.asmdef`). `Editor/`, `Testes/` e `TestesPlay/` têm assemblies próprios. `Revoada.Testes` referencia `Revoada.Editor`, então os testes podem usar `ConstrutorDados`, `DefinicoesPadrao` e `Robos`.

`Simulacao` é `partial`: cada assunto guarda a sua parte em `<Assunto>/Regras/Simulacao.<Assunto>.cs` (ondas, pássaros, Torre, penas, porcos, combate). `Partida/Regras/Simulacao.cs` tem o estado geral e a ordem do `Passo()`. Uma regra nova vai no arquivo parcial do assunto dela.

As responsabilidades estão separadas de propósito; não junte de volta:
- `Controles` só lê a entrada.
- `Selecao` guarda o que está na mão e recebe os comandos.
- `FantasmaColocacao` desenha o porco que está sendo colocado.
- `GerenteVisuais` só desenha, com um pool de visuais por entidade.
- `SonsDaPartida` toca os sons dos eventos do mapa. Os sons de interface ficam nos próprios botões e telas.
- `VisualTorre` anima a Torre apanhando.
- `TelaFim` só mostra o resultado; quem grava é o `FimDePartida`.
- `Progresso` (recorde, vitórias e dicas vistas) e `Preferencias` (volumes) gravam em PlayerPrefs através do `Salvamento`. O prefixo é `revoada_`; os testes trocam `Salvamento.Prefixo` por `teste_revoada_`.

### Cenas e prefabs são gerados por código

Os `Editor/Construtor*.cs` montam tudo:
- `ConstrutorAssets` importa a arte e cria os materiais.
- `ConstrutorDados` cria e lê os assets de dados no caminho de cada assunto.
- `ConstrutorPrefabs` monta os prefabs.
- `ConstrutorUI` monta toda a uGUI.
- `ConstrutorCenas` monta as cenas `Menu/Menu.unity` e `Partida/Jogo.unity`, liga os componentes entre si e configura as Build Settings.

Para mudar uma cena, um prefab ou uma ligação entre componentes, altere o construtor e rode *Reconstruir tudo*. Edição feita à mão na cena se perde na próxima reconstrução. Os caminhos de pasta ficam em constantes nesses construtores e nos `Tools/*.py`; se mover assets, atualize os dois.

### Dados e balanceamento

Os números padrão ficam em `Editor/DefinicoesPadrao.cs`. Os que valem no jogo ficam nos `.asset`, editáveis no Inspector: `Passaros/Dados`, `Porcos/Dados`, `Ondas/Dados/Ondas.asset`, `Penas/Dados/Economia.asset` e `Mapa/Dados/Fazenda.asset`.

`BalanceamentoTests` roda os robôs de `Editor/Robos.cs` com os números reais e falha se alguma meta de design quebrar (por exemplo, o Equilibrado tem que vencer e o Defesa pura tem que perder por volta da onda 22). As metas têm pouca folga, então qualquer mudança de número precisa passar pelos robôs ou pelos testes. Os testes de regra usam `Testes/CenarioTeste.cs`, com cenários pequenos que não dependem dos números de balanceamento.

## Armadilhas conhecidas

- **Mova ou renomeie um asset sempre junto com o seu `.meta`**: é o GUID dele que liga cenas, prefabs e assets. O mais seguro é mover pelo próprio Unity (Project window ou `unity command asset move`). A classe de um `MonoBehaviour` ou `ScriptableObject` precisa ter o mesmo nome do arquivo.
- `ConstrutorDados.GarantirDados(false)` cria assets com os números padrão quando não os encontra no caminho esperado, e `DadosTests` e `BalanceamentoTests` chamam esse método. Se o caminho no construtor não bater com a pasta real, rodar os testes cria assets duplicados.
- **Não recompile durante o Play.** O Unity recarrega o código no meio da partida, `jogo.Sim` (criado no `Awake`) fica null e as telas enchem o Console de `NullReferenceException`. Pare o Play antes; o `editor status` mostra o `playMode`.
- **Janelas modais travam o Editor**, e aí os comandos da CLI estouram o tempo. As duas mais comuns:
  - "Salvar cena modificada?", que aparece ao rodar testes ou *Reconstruir tudo* com a cena suja. Confira com `scene list_open` e salve antes com `scene save`.
  - O pedido de permissão do primeiro `script eval`.
- O caminho do projeto tem "º" (`5º Semestre`), e por isso `unity command script eval` e `eval_file` costumam falhar com `Illegal byte sequence`. Prefira os comandos prontos (`asset`, `scene`, `editor`, `test`, `console`, `job batch`).
- `capture game_view --save_path` grava dentro de `Assets/` (`Temp/x.png` vira `Assets/Temp/x.png`). Apague o arquivo depois.
- O projeto usa de propósito o **Input Manager** antigo, e não o Input System; o aviso de depreciação no Editor é esperado. Como o Input antigo não pode ser simulado, os comandos ficam em classes testáveis (`Selecao`) e os testes PlayMode chamam esses métodos direto.
- Se aparecer `Assets/_Recovery/`, é a cópia de cena que o Unity guarda depois de um crash. Não faz parte do jogo.
