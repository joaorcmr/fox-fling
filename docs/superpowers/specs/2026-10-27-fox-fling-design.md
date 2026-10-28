# Fox Fling — design

Jogo 2D de estilingue com física, feito em Unity 6 (6000.6.4f1) para explorar
**física 2D e objetos** no Unity. O jogador lança raposas com um estilingue
contra estruturas de blocos que protegem triângulos inimigos.

Origem: dois quadros do João ("Estágio 1" e "Estágio 2") + a proposta aprovada
no chat em 27/10/2026 ("Tudo certo mestre, pode seguir"). O que não veio do
João está marcado como *decisão do builder*.

## Por quê

- Projeto de estudo: o valor está em **ver a física funcionando** (corpos
  rígidos, massa, impulso, atrito, explosões) e em ter objetos/prefabs que dá
  para abrir e mexer no Inspector.
- Sucesso = o João abre o Unity, aperta Play, consegue jogar as 5 fases do
  começo ao fim e entende/ajusta os números da física pelo Inspector.

## Metas

**Estágio 1 (do quadro)**
- Bola = jogador (a raposa), lançada por um estilingue à esquerda.
- Retângulos e quadrados = obstáculos (blocos com física).
- Triângulos = inimigos, dentro/entre as estruturas.
- Mapa: chão, estilingue à esquerda, torre à direita (como o desenho).

**Estágio 2 (do quadro)**
- Tipos de raposa: **normal** (padrão), **ninja** (↑ velocidade), **bomba** (explode).
- Dosagem de raposas por fase:
  1 fácil (2 normais), 2 médias (2 normais + 1 ninja),
  1 difícil (2 normais + 1 ninja + 1 bomba), 1 desafio.
- Dificuldade do mapa = nº de raposas, nº de inimigos e tipo de estrutura:
  **madeira (padrão)**, vidro, rocha.

## Como joga (proposta aprovada)

1. A raposa espera no estilingue. Clica nela, arrasta para trás → linha
   pontilhada mostra a trajetória. Soltou → voa. Força ∝ distância puxada
   (com limite).
2. Blocos são corpos físicos de verdade: empilham, tombam, quebram. Cada bloco
   tem vida; o dano vem da **força do impacto** (impulso da colisão), então
   bloco caindo também quebra coisas.
3. Triângulos morrem com impacto forte o bastante (raposa, bloco, queda) ou ao
   cair para fora do mapa.
4. Raposa parada (ou fora da tela) → some e a próxima entra no estilingue.
5. **Vitória:** todos os triângulos eliminados. **Derrota:** acabaram as
   raposas e ainda há triângulo vivo.
6. Pontuação com 1–3 estrelas; bônus por raposa que sobrou.
7. Ninja: clicar com ela no ar → arrancada. Bomba: clicar no ar (ou alguns
   instantes depois de bater) → explode, causa dano e empurra tudo no raio.
8. Menu com seleção de fases; vencer uma libera a próxima.

## Restrições

- Resolução **600×480** (janela fixa no standalone e no WebGL).
- Projeto "New Unity Project" (Built-in Render Pipeline, vazio, Input Manager
  antigo). Fica dentro da pasta do Pixel Realm mas com **git próprio**; o
  repositório do Pixel Realm não é tocado.
- Tudo montado por um **script de Editor** (mesmo esquema do Pixel Realm):
  gera sprites, sons, materiais físicos, prefabs e cenas. Menu
  "Fox Fling > Reconstruir tudo". Rodar de novo sobrescreve as cenas geradas.
- Arte gerada por código (estilo cartoon simples com contorno); sons curtos
  gerados por código. Nada baixado da internet.

## Não-metas

- Câmera com scroll/zoom (a fase inteira cabe numa tela, como no desenho).
- Editor de fases dentro do jogo, ranking online, mobile/touch dedicado.
- Animações complexas (Animator); efeitos ficam em partículas e código.

## Anti-metas (falharia mesmo "funcionando")

- Física que parece falsa: blocos explodindo sozinhos no início da fase,
  estruturas que tremem/afundam, raposa atravessando paredes.
- Código opaco: o João precisa conseguir ler os scripts e mexer nos números
  (massa, vida, força) pelo Inspector.
- Parecer cópia de jogo existente: personagens, nomes e arte são próprios.

## Decisões do builder

- **Nome provisório:** Fox Fling.
- **Escala:** câmera ortográfica tamanho 6 → mundo de 15×12 unidades
  (40 px por unidade em 600×480). Gravidade padrão (−9,81).
- **Dano:** soma dos `normalImpulse` dos contatos no `OnCollisionEnter2D` ×
  multiplicador; impactos abaixo de um mínimo são ignorados; 1 s de
  "acomodação" sem dano no começo da fase.
- **Massa** pelo `density` do collider (`useAutoMass`), assim o material
  define o peso: madeira média, vidro leve e frágil, rocha pesada e resistente.
- **Blocos** com sprite 9-slice, então qualquer tamanho fica bonito; versão
  "rachada" quando a vida cai abaixo da metade.
- **Estrelas:** 1 por vencer; 2 e 3 por limiares de pontuação calculados a
  partir da pontuação máxima possível da fase.
- **Progresso** salvo em `PlayerPrefs` (fase liberada + melhores estrelas).
- **UI:** uGUI (`com.unity.ugui`) com `Text` + fonte embutida, textos em
  português.
- **Fase desafio:** 1 raposa de cada tipo contra fortaleza mista de rocha e
  vidro.
- **Testes:** testes de EditMode para regras puras (estrelas, dano, trajetória)
  + verificação jogando no Editor (capturas da Game view).
