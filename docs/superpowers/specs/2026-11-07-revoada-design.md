# Revoada — design

Tower defense 2D com visão de cima, feito em Unity 6 (6000.6.4f1) no lugar do
Fox Fling. Bandos de pássaros voam por uma trilha até a **Torre dos Porcos**; o
jogador compra **torretas de porcos verdes** com **penas**, que caem dos pássaros
estourados, e decide se gasta essas penas em mais defesa agora ou em ganhar mais
penas depois.

Origem: conversa com o João em 07/11/2026. Marcação usada neste documento:

- **(João)**: decidido por ele.
- **(proposta aprovada)**: proposta minha que ele aprovou no chat.
- **(minha escolha)**: decisão minha que ele ainda não viu; ele pode mudar.

O gênero é o do Bloons TD 6, que o João citou como referência **(João)**. Nome,
personagens, mapa, nomes de upgrades e visual são todos próprios: nada do BTD6 e
nada dos personagens de Angry Birds é copiado.

## Por quê

**(João)** O Fox Fling ficou pouco dinâmico: o jogador mira, solta e espera a
física. O jogo novo precisa ter **fluxo** e **sensação de progressão** entre as
ondas, com a dificuldade subindo **na medida certa**. O jogador aprende a:

1. **ponderar o gasto**: mais torretas deixam a onda atual mais fácil, enquanto
   aumentar o ganho de penas permite melhorar as torretas no futuro;
2. **posicionar bem**: os tiros perfurantes de algumas torretas atravessam
   vários pássaros quando eles vêm em linha reta;
3. **agrupar torretas** para montar uma estratégia.

**Sucesso:** o jogador sente que foi melhor de uma partida para a outra porque
decidiu melhor, e não porque teve sorte.

**Fracasso mesmo funcionando (anti-metas):**

- existir uma estratégia que sempre ganha ("só compro income", "só espalho
  estilingueiros");
- uma onda que de repente fica impossível (pico de dificuldade);
- esconder a informação de que o jogador precisa para decidir (penas, custo,
  próxima onda, retorno do investimento);
- copiar nomes, mapas ou visual do BTD6, ou os personagens de Angry Birds.

## Como joga

### Regras centrais (João)

- Visão de cima. Os pássaros vêm em **ondas**, cada uma um pouco mais difícil.
  ("Fases" na conversa = ondas dentro de um mapa: palpite meu que ele confirmou.)
- Pássaros estourados deixam **penas**, a moeda do jogo.
- As penas compram **torretas de porcos**, **upgrades dos porcos** ou um aumento
  no **ganho de penas por pássaro estourado**.
- A **Torre dos Porcos** começa com **100 de vida**. Cada pássaro que passa por
  todas as torretas e chega nela tira vida. Com a vida em 0, o jogador perde.
- Algumas torretas têm **tiros perfurantes**, e porcos agrupados criam
  estratégia (ver o Tamboreiro).

### Uma partida (minha escolha)

1. **Preparação:** o mapa aparece com 300 penas e 100 de vida. O jogador clica num
   porco da loja, vê o fantasma com o círculo de alcance (verde = pode, vermelho =
   não pode) e clica no mapa para posicionar.
2. **▶ Soltar a revoada** começa a onda. Os pássaros saem pela borda do mapa e
   voam pela trilha até a Torre dos Porcos.
3. O jogador pode comprar, melhorar e vender a qualquer momento, inclusive com o
   jogo pausado.
4. A onda acaba quando todos os pássaros dela já saíram e nenhum continua vivo.
   O jogador ganha o **bônus de fim de onda**. Com **Auto** ligado, a próxima onda
   começa sozinha depois de 2 s; senão, espera o ▶. Não dá para soltar a próxima
   onda antes de a atual acabar, então duas ondas nunca se sobrepõem.
5. São **30 ondas**, com chefes nas ondas **10, 20 e 30**. Quem sobrevive à
   onda 30 vence.
6. **Velocidade:** 1×, 2× e 3×. **Pausa** a qualquer momento.

**Vitória:** tela "A Torre resistiu!", com a vida que sobrou e uma medalha (bronze
abaixo de 40 de vida, prata de 40 a 79, ouro de 80 para cima).
**Derrota:** tela "A Torre caiu na onda X", com o recorde. Os dois finais oferecem
*Jogar de novo* e *Menu*.

**Salvo entre partidas (PlayerPrefs):** a melhor onda alcançada e quantas vezes
o jogador venceu. Não existe save no meio da partida.

### Atalhos (minha escolha)

| Tecla | Ação |
|---|---|
| 1–4 | Escolhe o porco da loja para posicionar |
| Clique direito / Esc | Cancela a colocação ou a seleção (Esc sem nada selecionado abre o menu de pausa) |
| Espaço | Solta a próxima onda |
| P | Pausa |
| F | Troca a velocidade (1× → 2× → 3×) |
| U | Compra o próximo upgrade do porco selecionado |
| Delete / Backspace | Vende o porco selecionado |

## Tela (João: 600×480, regra da disciplina)

Eu tinha proposto 960×600 e o João aprovou, mas depois ele avisou que a
disciplina exige **600×480**. Fica 600×480 **(João)**. A divisão da tela é
escolha minha:

- **Mapa** nos 448×480 da esquerda, sem nenhuma interface por cima.
- **Barra lateral** de 152×480 à direita, de cima para baixo: ❤ vida e 🪶 penas
  em destaque · Onda 7/30 · ⏸, velocidade e ☰ · os 4 porcos numa grade 2×2
  (ícone e preço, apagados quando faltam penas; o nome e o papel aparecem ao
  passar o mouse) · o cartão do **Ninho de Penas** · a **prévia da próxima onda**
  (ícones dos pássaros com quantidades e aviso de chefe) · o botão
  **▶ Soltar a revoada** com o interruptor **Auto**.
- Nenhum texto menor que 12 px, para continuar legível em 600×480.
- **Porco selecionado:** a parte da loja na barra lateral vira o painel do porco
  (nome, nível em bolinhas ●●○, números atuais, cartão do próximo upgrade com
  efeito e custo, as duas especializações lado a lado quando for a hora, botão de
  modo de alvo e **Vender (+N 🪶)**). No mapa aparecem o alcance e, no caso do
  Tamboreiro, a aura e um ícone ♪ sobre cada porco fortalecido.

Unidade do mundo **(minha escolha)**: 1 unidade = 32 px. Câmera ortográfica com
tamanho 7,5, ou seja, 15 unidades de altura e 18,75 de largura. O mapa ocupa
x ∈ [0, 14] e y ∈ [0, 15], e a barra lateral cobre o resto, à direita.

## Mapa (minha escolha)

Um mapa, a **Fazenda**, visto de cima: grama, uma trilha de terra, árvores,
pedras, um laguinho, cerca e flores. A trilha é uma **serpentina** que entra pela
esquerda, lá no alto, e termina na Torre dos Porcos, embaixo:

1. uma **reta longa** no alto, da esquerda para a direita, quase da largura do
   mapa (cerca de 12 unidades), o lugar ideal para o Arpoador;
2. desce e faz mais duas ou três passagens horizontais, alternando o sentido;
3. desce até a Torre dos Porcos, embaixo.

Os cantos são arredondados (raio de cerca de 1 unidade), para os pássaros não
virarem em ângulo reto. O comprimento total fica em torno de 55 unidades, ou
cerca de 22 s para um pardal atravessar em 1×. A serpentina deixa pontos no meio
que alcançam **duas passagens** da trilha (valor para alcance e para agrupar) e
curvas onde os bandos se espremem (valor para bombas).

**Onde dá para colocar porco:** em qualquer lugar do mapa que não seja a trilha
(meia-largura de 0,6), a água, as árvores, as pedras, a Torre, outro porco ou a
área da UI. Cada porco ocupa um círculo de raio 0,45. A colocação é **livre**,
sem grade, para que a mira em relação às retas da trilha faça diferença.

O mapa é dado (pontos da trilha mais uma lista de obstáculos), não código
espalhado, para dar para criar outros mapas depois.

## Pássaros (minha escolha; os números são o ponto de partida do balanceamento)

O mapa é visto de cima, mas os bichos são desenhados **de frente**, no estilo
dos sprites redondinhos e emburrados do Iran, com uma sombra no chão e uma
animação leve de voo (bater de asas ou balanço). Cada pássaro tem vida, ou seja,
quantos golpes aguenta. Ao ser atingido ele pisca branco e perde umas peninhas;
ao estourar, solta uma nuvem de penas, toca o som de estouro e mostra
"+N 🪶" flutuando. Os pássaros de vida 8 ou mais mostram uma barra de vida.

| Pássaro | Vida | Velocidade (u/s) | Dano na Torre | Penas | Especial | Estreia |
|---|---|---|---|---|---|---|
| Pardal | 1 | 2,5 | 1 | 1 | o básico; vem em bando | onda 1 |
| Sabiá | 3 | 2,8 | 2 | 2 | — | onda 3 |
| Beija-flor | 2 | 5,0 | 2 | 3 | muito rápido | onda 6 |
| Arara-azul (sprite do Iran) | 10 | 2,0 | 4 | 6 | — | onda 8 |
| Pata | 8 | 1,8 | 3 | 4 | ao estourar, solta 4 **patinhos** | onda 12 |
| Patinho | 1 | 3,0 | 1 | 1 | só nasce da pata | — |
| Urubu (sprite do Iran) | 28 | 1,4 | 6 | 12 | **couraça**: pedrinha e arpão causam metade do dano | onda 15 |
| Gavião-real (chefe) | 250 / 700 / 1.500 | 0,9 | 30 | 100 | chefe: barra de vida grande no topo, lentidão máxima de 30%, não é empurrado | ondas 10, 20, 30 |

Os patinhos nascem onde a pata estourou, um pouco espalhados para trás na
trilha e com vida cheia. O dano que sobra de um golpe não passa para eles.

Não vou usar camadas como os balões ("estoura e vira outro"): cada pássaro tem
vida, e a pata é o tipo que se divide. **(minha escolha)**

## Porcos (minha escolha; números de partida)

Todos são porcos verdes, redondos e emburrados, cada tipo com um equipamento
diferente. O porco fica de frente e vira para a esquerda ou para a direita
conforme o alvo; a arma gira e mira. Ao atirar ele dá um tranco (squash), e ao
ser posicionado cai com um quiquinho e levanta poeira.

| Porco | Custo | Ataque | Dano | Alcance | Recarga | Perfura | Papel |
|---|---|---|---|---|---|---|---|
| **Estilingueiro** | 120 | pedrinha | 1 | 3,0 | 0,8 s | 1 | barato, segura o começo |
| **Arpoador** | 220 | arpão em linha reta | 2 | 5,0 | 1,6 s | 4 | **perfurante**: rende nas retas longas |
| **Bombardeiro** | 350 | bomba de lama, área de raio 1,0 | 2 | 3,5 | 1,5 s | área | curvas e bandos; dano cheio na couraça |
| **Tamboreiro** | 300 | não ataca; **aura** de raio 2,5 | — | — | — | — | porcos na aura atiram 20% mais rápido (**agrupar**) |

**Tiros:** todos os projéteis andam em **linha reta**, mirando onde o alvo vai
estar (mira com antecipação de primeira ordem). Por isso um tiro ainda pode errar
um beija-flor. O **arpão continua voando depois do primeiro pássaro**, até 1,6×
o alcance, e acerta cada pássaro uma vez só, até acabar a perfuração. Um
Arpoador colocado no fim de uma reta, apontado ao longo dela, atravessa a fila
inteira; colocado ao lado da reta, cruza a trilha e pega um ou dois. Essa é a
habilidade de posicionamento que o João pediu.

A **bomba de lama** voa até o ponto onde o alvo vai estar e explode ali,
acertando todos os pássaros no raio. Ela explode mesmo que o alvo já tenha
estourado no caminho, e não bate em nada durante o voo.

A **vida dos pássaros aceita frações**: na couraça, uma pedrinha de dano 1
causa 0,5, então um urubu (28) precisa de 56 pedrinhas ou de 14 bombas.

**Modo de alvo** (por porco): Primeiro (padrão, o mais adiantado na trilha) ·
Último · Mais forte · Mais perto.

**Vender** devolve 70% de tudo o que foi gasto naquele porco.

### Upgrades

Cada porco tem **dois upgrades em sequência** (níveis 2 e 3) e, no nível 4, o
jogador **escolhe uma de duas especializações**. É a escolha que encaixa o porco
no lugar onde ele está.

| Porco | Nível 2 | Nível 3 | Especialização A | Especialização B |
|---|---|---|---|---|
| Estilingueiro | **Elástico Reforçado** (100): recarga 0,55 s | **Pedra Afiada** (180): dano 2, alcance 3,5 | **Rajada** (500): 3 pedrinhas em leque | **Mira Certeira** (550): alcance 5,5, dano 4, perfura 2 |
| Arpoador | **Arpão Comprido** (160): perfura 7, voa até 2× o alcance | **Ponta de Ferro** (260): dano 3 e **ignora a couraça** | **Arpão Gigante** (750): perfura 20, dano 6, arpão mais largo | **Arpão Ligeiro** (650): recarga 0,6 s |
| Bombardeiro | **Barril Maior** (220): raio 1,4 | **Lama Grudenta** (300): atingidos ficam 40% mais lentos por 1,5 s | **Chuva de Lama** (850): recarga 0,7 s | **Barril de Estrume** (800): dano 7, raio 1,8 e empurra os pássaros 1 unidade para trás na trilha |
| Tamboreiro | **Batuque** (220): aura +35%, raio 3,0 | **Grito de Guerra** (320): vizinhos com +20% de alcance | **Fanfarra** (650): aura +60%, raio 3,5 | **Tambor das Penas** (550): pássaros estourados dentro da aura rendem +50% de penas |

As auras de Tamboreiros diferentes **não se somam**: para cada bônus vale o
maior. Assim, empilhar tambores não vira uma estratégia que sempre ganha. O
**Tambor das Penas** dá ao jogador uma terceira forma de investir: em vez do
income global, transformar um bom ponto de abate numa "fazenda de penas".

O bônus de lentidão (Lama Grudenta) não se acumula: vale a lentidão mais forte.
Chefes ficam no máximo 30% mais lentos e não são empurrados.

## Economia (minha escolha; números de partida)

- **Penas iniciais:** 300.
- **Penas por pássaro:** o valor do tipo × o multiplicador do Ninho × o bônus do
  Tambor das Penas, se ele morreu dentro dessa aura. A parte fracionária se
  acumula num "resto", de modo que um pardal de 1 pena com ×1,25 rende 1 pena
  quatro vezes e na quinta rende 2. As penas são creditadas na hora do estouro.
  A animação das penas voando até o contador é só visual.
- **Bônus de fim de onda:** 25 + 5 × número da onda. Não é multiplicado pelo
  Ninho, o que impede o income de ficar forte demais.
- **Ninho de Penas** (a decisão que o João descreveu, comprado na barra lateral):

| Nível | Penas por pássaro | Custo |
|---|---|---|
| 0 | ×1,0 | — |
| 1 | ×1,25 | 250 |
| 2 | ×1,5 | 500 |
| 3 | ×1,75 | 900 |
| 4 | ×2,0 | 1.500 |
| 5 | ×2,5 | 2.500 |

O cartão do Ninho mostra o multiplicador atual, o próximo, o custo e o
**retorno estimado**: "se paga em ~N ondas". A conta soma as penas-base das
próximas ondas (que são conhecidas) × o ganho de multiplicador até cobrir o
custo. Quando não dá mais tempo, o cartão mostra "não se paga mais até a
onda 30". É isso que deixa a escolha **visível** e ensina o jogador a pensar no
tempo: income comprado cedo rende muito, comprado tarde não rende nada.

## Ondas (minha escolha)

As 30 ondas ficam em dados: cada onda é uma lista de grupos com tipo de pássaro,
quantidade, intervalo entre um e outro e atraso para o grupo começar. A curva:

| Ondas | Conteúdo | O que ensina |
|---|---|---|
| 1–5 | pardais e sabiás | comprar, posicionar, primeiros upgrades |
| 6–9 | beija-flores e as primeiras araras | cobertura e dano |
| 10 | **chefe 1**: gavião-real (250) com escolta | concentrar dano |
| 11–14 | patas (se dividem) e bandos maiores | área (Bombardeiro) |
| 15–19 | urubus (couraça); na 18, uma revoada densa de pardais | bombas ou Ponta de Ferro; o Arpoador nas retas |
| 20 | **chefe 2**: gavião-real (700) com urubus | — |
| 21–29 | misturas densas (beija-flores com urubus, patas com araras) | especializações e agrupamento |
| 30 | **chefe final**: gavião-real (1.500) com escolta pesada | tudo |

A prévia da próxima onda avisa quando um tipo novo estreia (ex.: "Novo: Urubu,
couraça") e quando vem chefe.

### Como a dificuldade fica "na medida certa"

Os números acima são só o ponto de partida. O balanceamento é feito com
**partidas simuladas por robôs**, nos testes automáticos (a simulação roda sem
Unity; ver Arquitetura). Os robôs posicionam porcos numa lista de bons lugares
do mapa e seguem regras de compra simples. As metas:

| Robô | Comportamento | Meta |
|---|---|---|
| Parado | não compra nada | perde até a onda 3 |
| Só estilingue | só compra estilingueiros e os upgrades deles | perde entre as ondas 12 e 20 |
| Defesa pura | compra e melhora porcos, **nunca** o Ninho | perde entre as ondas 22 e 29 |
| Ganancioso | compra o Ninho sempre que pode, e porco só com o que sobra | perde antes da onda 12 |
| Equilibrado | mistura porcos, upgrades e Ninho no ritmo certo | **vence a onda 30** com pelo menos 40 de vida |

Mais uma meta: na partida do robô Equilibrado, nenhuma onda sozinha tira mais
de 25 de vida (sem picos). Se mudar um número quebra uma meta, o teste acusa.

## Ensinar sem tutorial chato (minha escolha)

São **dicas curtas** que não travam o jogo e aparecem uma vez cada:

- antes da onda 1: "Clique num porco na loja e depois no mapa";
- ao posicionar o primeiro Arpoador: "O arpão atravessa os pássaros em fila: no
  fim de uma reta ele rende mais";
- na onda 4: "O Ninho de Penas faz cada pássaro render mais. Vale a pena agora?";
- quando o urubu aparece na prévia: "Couraça: só a metade do dano da pedrinha e
  do arpão entra";
- ao selecionar um Tamboreiro: "Porcos dentro do círculo atiram mais rápido".

## Sensação (minha escolha)

- **Pássaros:** sombra, animação de voo, flash ao serem atingidos, peninhas a
  cada golpe, nuvem de penas e "+N" ao estourar.
- **Porcos:** tranco ao atirar, arma girando, quique ao serem posicionados, brilho
  e bolinha de nível ao receberem upgrade, um acessório diferente por
  especialização.
- **Torre dos Porcos:** treme e pisca vermelho quando apanha, tremida curta na
  câmera, o número da vida pulsa; abaixo de 25 de vida, um alerta.
- **Ondas:** faixa "Onda 7" deslizando e corneta; chefe com faixa dramática e
  barra de vida no topo.
- **Sons gerados por código:** estouro, três tiros diferentes, explosão, compra,
  upgrade, venda, erro (sem penas ou lugar inválido), início de onda, chefe,
  Torre apanhando, vitória, derrota e clique. Uma música curta em loop, também
  gerada, entra se der tempo.

## Menu (minha escolha)

Logo "Revoada", com porco e pássaros · **Jogar** · **Como jogar** (um quadro com
5 dicas) · o recorde (melhor onda e vitórias).

## Como vamos fazer (proposta aprovada, salvo onde marcado)

- **Quem:** só o João e eu. O Iran não participa mais **(João)**.
- **Onde:** repositório `fox-fling` (github.com/joaorcmr/fox-fling), na branch
  `feat/revoada`, criada a partir da `main` já com os 2 commits de sprites do
  Iran. A branch só entra na `main` quando o João aprovar o jogo. O Fox Fling
  sai da pasta de trabalho (`Assets/FoxFling` é apagada) e fica no histórico do
  git. Antes de apagar, os sprites do urubu e da arara-azul vão para a pasta do
  jogo novo.
- **Motor:** o mesmo projeto Unity 6000.6.4f1: Built-in Render Pipeline, Input
  Manager antigo (`activeInputHandler: 0`) e UGUI (pacote 2.6.0, que já está no
  projeto). Sem pacotes novos, a não ser que algum seja indispensável.
- **Arte e sons:** gerados por código (Python 3 + Pillow, em `Tools/`), imitando
  o estilo dos sprites do Iran. O urubu e a arara-azul usam os PNGs dele.
- **Plataforma:** WebGL para o itch.io e standalone, os dois em **600×480**
  (regra da disciplina, **João**). `productName` passa a ser "Revoada".
- **Prazo:** terça-feira, 10/11/2026 **(João)**. Pela ordem de construção, o
  jogo fica jogável cedo e o resto é polimento.
- **Números editáveis no Inspector:** tipos de pássaro, tipos de porco (com
  upgrades), ondas, Ninho e economia ficam em ScriptableObjects (`.asset`).
- **Construtor de Editor:** menu **Revoada > Reconstruir tudo** gera os assets de
  dados (só se ainda não existirem, para não apagar ajustes feitos no Inspector),
  os prefabs, as cenas (Menu e Jogo) e as Build Settings. **Revoada > Build WebGL**
  gera o jogo para navegador.
- **Comentários do código em português**, explicando o porquê (é projeto de estudo).
- **Progresso:** prints da tela do Unity mandados ao João conforme o jogo avança
  **(João)**.

## Feito em Unity (João)

O João precisa poder dizer ao professor que o jogo foi feito em Unity **(João)**.
A simulação em C# puro (ver Arquitetura) não muda isso: C# é a linguagem de
scripts do Unity, e as regras são classes chamadas pelos componentes do Unity a
cada passo. Para que isso fique evidente para quem abrir o projeto:

- O jogo é um projeto Unity 6, montado e testado no Editor, com cenas, prefabs e
  ScriptableObjects. O build WebGL sai do próprio Unity.
- A simulação avança dentro do `FixedUpdate` de um componente (`Jogo`), no passo
  fixo do Unity (`Time.fixedDeltaTime` = 1/60 s). A pausa e as velocidades 2× e
  3× usam `Time.timeScale`.
- Todo pássaro, porco, projétil e efeito é um GameObject criado a partir de um
  prefab, com SpriteRenderer e um componente que mostra o estado ao vivo no
  Inspector (vida, nível, alvo).
- A trilha, os obstáculos e os alcances aparecem como Gizmos na Scene view, e os
  números ficam em ScriptableObjects editáveis no Inspector.
- Penas e estouros usam ParticleSystem, os sons usam AudioSource, a interface usa
  uGUI (Canvas) e os testes usam o Unity Test Framework.
- O README ganha uma seção **"Onde está o Unity"** que lista cada recurso do Unity
  usado e o arquivo onde ele está, para o João usar na apresentação.

## Arquitetura (minha escolha)

- **Simulação em C# puro, sem física e sem MonoBehaviour** (roda dentro do
  Unity; ver "Feito em Unity"): trilha, pássaros,
  porcos, projéteis, colisão por círculos, ondas, economia e colocação. Ela avança
  em passos fixos de 1/60 s e é determinística: mesmos comandos, mesmo resultado.
  Por isso o jogo inteiro roda dentro dos testes EditMode, sem dar Play. Isso
  permite os robôs de balanceamento e permite testar mesmo com a tela do Mac
  bloqueada (o Play Mode para de rodar quando a tela trava).
- **A Unity desenha e ouve o jogador:** o componente `Jogo` avança a simulação no
  `FixedUpdate` (em 2× e 3× o `Time.timeScale` faz o Unity rodar mais passos
  fixos por quadro), traduz cliques em comandos ("comprar porco
  X em P", "upgrade", "vender", "soltar onda") e recebe eventos (pássaro nasceu,
  levou dano, estourou, tiro disparado, Torre atingida, onda começou/acabou) para
  mover sprites, tocar sons e atualizar a UI. Usa pool de objetos para pássaros,
  projéteis e efeitos.
- **Dados:** os ScriptableObjects são convertidos em estruturas simples que a
  simulação lê, de modo que os testes conseguem montar cenários pequenos sem
  assets. Os testes de balanceamento carregam os `.asset` de verdade.
- **Testes EditMode:** trilha (posição por distância, cantos), movimento, alvo
  (os 4 modos), mira com antecipação, perfuração, área, couraça, lentidão,
  empurrão, divisão da pata, auras sem acúmulo, economia (resto fracionário,
  Ninho, venda 70%), colocação válida e inválida, fim de onda e bônus, vitória e
  derrota, validação dos dados (toda onda usa tipos que existem, custos
  positivos, a trilha dentro do mapa) e os robôs de balanceamento.
- **Pastas:** `Assets/Revoada/{Arte, Audio, Dados, Editor, Prefabs, Cenas, Scripts, Testes}`.

## Ordem de construção (minha escolha)

O João quer ver prints do Unity enquanto o jogo é feito, então a construção vai
em **fatias que aparecem na tela**, cada uma terminando num print. Não começa
pela simulação inteira, que deixaria a tela vazia por muito tempo:

1. Mapa, trilha e pássaros voando (simulação mínima mais o desenho na tela).
2. Porcos atirando, estouro com penas, compra e colocação, vida da Torre.
3. Upgrades, especializações, Ninho, HUD e loja completos.
4. Todos os pássaros e as 30 ondas, mais os robôs de balanceamento ajustando os números.
5. Sensação, sons, dicas, menu e telas de fim.
6. Build WebGL.

Se o tempo apertar, sai primeiro a música, depois as dicas, depois o 3× de
velocidade. O núcleo (porcos, upgrades, Ninho, ondas balanceadas) não sai.

## Fora do escopo desta versão

Mais de um mapa, níveis de dificuldade, modo infinito, pássaros camuflados,
heróis ou habilidades ativas, progressão entre partidas (desbloqueios), save no
meio da partida, controles de toque e ranking online. O mapa em dados e as ondas
em dados deixam a porta aberta para mapas novos e modo infinito.

## O que fica com o construtor

- Os números finais (ajustados pelos robôs, dentro das metas acima).
- As coordenadas exatas da trilha e dos obstáculos, desde que respeitem a
  estrutura da serpentina.
- Os desenhos exatos dos sprites e o número de quadros de animação.
- A composição exata de cada onda, desde que siga a curva da tabela.
- A fonte do texto (sem baixar nada de fora).
