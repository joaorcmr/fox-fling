# Revoada

Tower defense 2D com visão de cima, feito em **Unity 6**. Bandos de pássaros voam por uma trilha até a
**Torre dos Porcos**. Você compra **porcos verdes** com **penas**, que caem dos pássaros estourados, e
decide a toda hora: gastar em mais defesa agora ou no **Ninho de Penas**, que faz cada pássaro render mais
depois?

- **Disciplina:** Jogos Digitais, Insper
- **Resolução:** 600×480
- **Arte do urubu e da arara-azul:** Iran. O resto da arte e todos os sons são gerados por código (`Tools/`).
- Design e plano: `docs/superpowers/specs/2026-11-07-revoada-design.md` e `docs/superpowers/plans/2026-11-07-revoada.md`

## Como jogar

| Ação | Como |
|---|---|
| Comprar e colocar um porco | Clique no porco da loja (ou aperte **1** a **4**) e depois no mapa. Verde = pode, vermelho = não pode |
| Soltar a onda | Botão **▶ Soltar a revoada** ou **Espaço**. Com **Auto** ligado, as ondas seguem sozinhas |
| Ver e melhorar um porco | Clique nele: o painel mostra os números, o próximo upgrade (**U**), a mira e **Vender** (**Delete**) |
| Pausar / acelerar | **P** ou ⏸ · **F** ou o botão 1×/2×/3× |
| Menu | **Esc** (sem nada selecionado) ou ☰ |

**Objetivo:** sobreviver às 30 ondas. A Torre começa com 100 de vida, e cada pássaro que chega nela tira um
pouco. Há chefes (gavião-real) nas ondas 10, 20 e 30.

### Os porcos

| Porco | Custo | O que faz | Especializações (no nível 4) |
|---|---|---|---|
| **Estilingueiro** | 120 | Pedrinha rápida, barato: segura o começo | Rajada (3 pedrinhas em leque) · Mira Certeira (longe e forte) |
| **Arpoador** | 220 | O arpão **atravessa** a fila: rende no fim das retas longas | Arpão Gigante (atravessa 20) · Arpão Ligeiro (muito rápido) |
| **Bombardeiro** | 350 | Bomba de lama em **área**; dano cheio na couraça | Chuva de Lama (o dobro de bombas) · Barril de Estrume (empurra para trás) |
| **Tamboreiro** | 300 | Não ataca: porcos no círculo dele atiram mais rápido (**agrupar**) | Fanfarra (aura maior) · Tambor das Penas (+50% de penas na aura) |

Cada porco tem 2 upgrades em sequência e, no nível 3, a escolha de uma especialização. Vender devolve 70%
do que foi gasto nele. As auras de Tamboreiros diferentes **não se somam**: vale a maior.

### Os pássaros

| Pássaro | Vida | Velocidade | Dano na Torre | Penas | Especial |
|---|---|---|---|---|---|
| Pardal | 1 | 2,5 | 1 | 2 | vem em bando |
| Sabiá | 3 | 2,8 | 2 | 4 | |
| Beija-flor | 2 | 5,0 | 2 | 6 | muito rápido |
| Arara-azul | 10 | 2,0 | 4 | 12 | |
| Pata | 8 | 1,8 | 3 | 8 | ao estourar, solta 4 patinhos |
| Urubu | 28 | 1,4 | 6 | 24 | **couraça**: pedrinha e arpão tiram só metade |
| Gavião-real (chefe) | 90 · 356 · 1.404 | 0,9 | 30 | 200 | ondas 10, 20 e 30; no máximo 30% mais lento |

A partir da onda 11, os pássaros ganham **8% de vida por onda** (onda 20 = ×1,8; onda 30 = ×2,6).

### Penas e o Ninho

Você começa com 300 penas. Cada pássaro estourado rende as penas dele × o multiplicador do Ninho, e cada onda
vencida dá um bônus pequeno (25 + 5 × a onda). O **Ninho de Penas** sobe o multiplicador: ×1,25 (120) →
×1,5 (250) → ×1,75 (450) → ×2 (750) → ×2,5 (1.200). O cartão dele mostra **em quantas ondas o investimento se
paga**: income comprado cedo rende muito, e comprado tarde não rende nada.

## Onde está o Unity

O jogo inteiro é um projeto Unity 6. As regras ficam em classes C# puras (as pastas `Regras/` de cada assunto,
ver [Estrutura](#estrutura)) chamadas pelos componentes do Unity a cada passo. Recurso por recurso:

| Recurso do Unity | Onde |
|---|---|
| **Ciclo do jogo**: `FixedUpdate` avança a simulação no passo fixo (`Time.fixedDeltaTime` = 1/60 s, gravado no TimeManager); pausa e 1×/2×/3× são o `Time.timeScale` | `Partida/Jogo.cs`, `Partida/RelogioPartida.cs` |
| **Cenas** (Menu e Jogo), Build Settings, `SceneManager` e o escurecer entre telas (`DontDestroyOnLoad`) | `Menu/Menu.unity`, `Partida/Jogo.unity`, `Compartilhado/Navegacao/` |
| **Prefabs** (pássaro, porco, tiro, texto, efeitos) e **pool** de objetos | `Prefabs/` de cada assunto, `Compartilhado/Pool.cs` |
| **ScriptableObjects** com os números do jogo, editáveis no Inspector (`[CreateAssetMenu]`) | `Dados/` de cada assunto (a classe e os `.asset`) |
| **SpriteRenderer**, **Sorting Group** e o eixo de ordenação da câmera (Y) para a visão de cima | `Editor/ConstrutorPrefabs.cs`, `Editor/ConstrutorCenas.cs` |
| **LineRenderer**: a trilha de terra, com textura repetida | `Mapa/Visual/VisualMapa.cs` |
| **Gizmos** na Scene view (trilha, obstáculos, alcance do porco selecionado) | `VisualMapa.OnDrawGizmos`, `VisualPorco.OnDrawGizmosSelected` |
| **ParticleSystem** (penas na cor de cada pássaro, poeira, brilho) com `Emit` | `Compartilhado/Efeitos/Efeitos.cs` |
| **AudioSource** (vozes de efeito, música e ambiente em loop, volume) | `Sons/Sons.cs`, `Sons/SonsDaPartida.cs` |
| **uGUI**: Canvas e CanvasScaler 600×480, Button, Slider, CanvasGroup, Image Filled, eventos de ponteiro | `Interface/` de cada assunto, `Editor/ConstrutorUI.cs` |
| **Input Manager** e `EventSystem.IsPointerOverGameObject` (clique na loja não vaza para o mapa) | `Partida/Entrada/Controles.cs` |
| **PlayerPrefs** (recorde, vitórias, dicas vistas, volumes) | `Progresso/`, `Sons/Preferencias.cs` |
| **Editor scripting**: menu Revoada (`MenuItem`), `AssetDatabase`, `PrefabUtility`, `EditorSceneManager`, `TextureImporter`, `SerializedObject` | `Editor/` |
| **Unity Test Framework**: 101 testes EditMode (incluindo os robôs de balanceamento) e 3 testes PlayMode, que carregam a cena Jogo de verdade (pausa, escurecer entre cenas, fim de jogo) | `Testes/`, `TestesPlay/` |
| **Build WebGL** (`BuildPipeline`) | `ConstrutorRevoada.BuildWebGL` |

Durante o Play, selecione um pássaro ou um porco na Hierarchy: o Inspector mostra o estado dele ao vivo
(vida, distância na trilha, nível, alcance).

## Balanceamento: robôs que jogam o jogo

"Difícil na medida certa" é medido. Robôs jogam partidas inteiras com os números de verdade, sem desenhar
nada (menu **Revoada > Balanceamento (robôs)**, ou os testes de `BalanceamentoTests`). Três deles seguem a
mesma ordem de construção e só mudam o que fazem com o Ninho:

| Robô | O que faz | Resultado |
|---|---|---|
| Parado | não compra nada | perde na onda 5 |
| Só Estilingue | só Estilingueiros e os upgrades deles | perde na onda 19 |
| Defesa pura | porcos e upgrades, **nunca** o Ninho | perde na onda 22 |
| Ganancioso | guarda tudo para o Ninho | perde na onda 8 |
| **Equilibrado** | mistura defesa e Ninho (compra quando ele se paga em até 6 ondas) | **vence** a onda 30 com 46 de vida |

A virada do jogo são as ondas 16 e 19, com urubus de couraça: quem só tem pedrinha, ou não investiu em penas,
cai ali ou logo depois. Se alguém mexer num número e quebrar esse equilíbrio, os testes avisam. As metas estão
com pouca folga, então depois de mexer em qualquer número rode os robôs (ou os testes) antes de jogar.

## Mexendo nos números

- **No Inspector:** a pasta `Dados/` de cada assunto tem os assets: um para cada pássaro (`Passaros/Dados`) e
  cada porco, com os upgrades (`Porcos/Dados`), as ondas (`Ondas/Dados/Ondas.asset`), a economia
  (`Penas/Dados/Economia.asset`) e o mapa (`Mapa/Dados/Fazenda.asset`). Mude e dê Play.
- **Menu Revoada:**
  - *Reconstruir tudo*: gera de novo prefabs e cenas a partir do código. Os números dos assets são mantidos.
  - *Restaurar números padrão*: volta os assets para os números de `Editor/DefinicoesPadrao.cs`.
  - *Balanceamento (robôs)*: escreve no Console a tabela acima com os números atuais.
  - *Pular para a onda 18 (robô)*: no Play, o robô Equilibrado joga sozinho até a revoada densa.
  - *Build WebGL (itch.io)*: gera o jogo para navegador em `Builds/WebGL`.
- **O mapa:** os pontos da trilha e os obstáculos estão no `Fazenda.asset`. Os Gizmos mostram a trilha nova na
  Scene view na hora; clique com o botão direito no componente `VisualMapa` → *Redesenhar* para atualizar os
  sprites.

## Guia de estilo

Bichos redondos e emburrados, de frente, com contorno marrom-escuro (`#2B1D14`) e sombreado em dois tons.
Paleta: grama `#79BC57`, terra `#CDA46E`, creme `#FFF6E0`, marrom `#6B4A2E`, verde dos porcos `#8CCB5E`,
ouro `#F2C14E` e vermelho `#C8443A`. A interface usa painéis creme com borda marrom, botões verdes e texto
creme contornado.

## Estrutura

As pastas têm o nome do que o jogo **é** (pássaros, porcos, ondas, penas, Torre), e não do tipo de arquivo
(*Screaming Architecture*): quem abre `Assets/Revoada` vê de cara que é um tower defense de pássaros contra
porcos, e tudo sobre um assunto (regra, números, desenho, tela) fica junto.

```
Assets/Revoada/
├── Passaros/      o pássaro na simulação, os tipos de pássaro, o desenho e a barra do chefe
├── Porcos/        o porco, onde pode ficar, upgrades e auras; a loja, o painel e o porco fantasma
├── Combate/       tiros, mira e golpes (lentidão, empurrão, explosão)
├── Ondas/         o gerador de ondas, as 30 ondas e a prévia da próxima
├── Penas/         a economia e o Ninho de Penas
├── Mapa/          a trilha, os obstáculos e o desenho do mapa
├── Torre/         a vida da Torre e a Torre apanhando
├── Partida/       a simulação (estado e ordem do passo), o Jogo que monta a partida, relógio, seleção do
│                  jogador, teclado e mouse, HUD, pausa e tela de fim; cena Jogo
├── Menu/          a tela de título e os pássaros de enfeite; cena Menu
├── Dicas/         os textos das dicas e o balão que as mostra
├── Sons/          o Sons (vozes e loops), os sons da partida, os volumes e os clipes WAV
├── Progresso/     recorde, vitórias e dicas vistas (PlayerPrefs)
├── Compartilhado/ efeitos, pool, troca de cena, formatação de números, arte e materiais
├── Editor/        o construtor (menu Revoada), os números padrão e os robôs
├── Testes/        testes EditMode (Window > General > Test Runner)
└── TestesPlay/    testes PlayMode (carregam a cena Jogo e rodam quadros)
Tools/             geradores de arte (→ Compartilhado/Arte) e de som (→ Sons/Clipes), em Python 3 + Pillow
```

Dentro de cada assunto, cada pasta cuida de uma coisa só (*separação de responsabilidades*):

| Pasta | O que tem |
|---|---|
| `Regras/` | C# puro: o estado e as regras. A `Simulacao` é `partial` e cada assunto guarda a sua parte (`Simulacao.Ondas.cs`, `Simulacao.Torre.cs`...) |
| `Dados/` | os ScriptableObjects: a classe e os `.asset` com os números |
| `Visual/` | o que desenha no mapa |
| `Interface/` | a barra lateral, as telas e os botões (uGUI) |
| `Prefabs/` | os prefabs que o construtor gera |

Todas as pastas `Regras/` compilam num assembly só, o `Revoada.Nucleo` (definido em `Partida/Regras`; as
outras entram nele por um `.asmref`). Esse assembly não enxerga o resto do jogo, então o compilador não deixa
uma regra depender de desenho, som ou tela: o resto (assembly `Revoada`) é que escuta os eventos da simulação.

Na partida, cada responsabilidade é uma classe, e o `Jogo` só as junta:

| Classe | Faz |
|---|---|
| `Simulacao` | as regras do jogo, sem Unity |
| `Jogo` | monta a partida e chama o `Passo()` no `FixedUpdate` |
| `RelogioPartida` | pausa e velocidade (`Time.timeScale`) |
| `Selecao` | o que o jogador tem na mão (um porco da loja ou um porco selecionado) |
| `Controles` | lê teclado e mouse e transforma em comandos |
| `FantasmaColocacao` | desenha o porco que está sendo colocado |
| `GerenteVisuais` | mantém um GameObject para cada pássaro, porco e tiro |
| `VisualTorre` | a Torre tremendo quando apanha |
| `SonsDaPartida` | os sons do que acontece no mapa |
| `FimDePartida` | guarda o resultado no Progresso (a tela de fim só mostra) |
| `Navegacao` | os nomes das cenas e as trocas de cena |

## Abrindo o projeto

1. Unity **6000.6.4f1**
2. No Unity Hub, *Add project from disk* e escolha a pasta do repositório (a que tem `Assets/`)
3. Cena inicial: `Assets/Revoada/Menu/Menu` (o jogo em si é `Assets/Revoada/Partida/Jogo`)
4. Na Game view, escolha o tamanho **Revoada 600x480**

O Fox Fling, o jogo anterior deste repositório, continua no histórico do git.
