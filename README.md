# Fox Fling

Jogo 2D de estilingue com física, feito em Unity 6 para explorar **física 2D e objetos**.
Lance raposas contra torres de madeira, vidro e rocha e derrube todos os tubarões.

- **Autor:** Joao Ricardo Ribeiro ([@joaorcmr](https://github.com/joaorcmr))
- **Resolução:** 600×480
- Desenhado a partir dos quadros "Estágio 1" e "Estágio 2" (design em
  `docs/superpowers/specs/2026-10-27-fox-fling-design.md`)

## Como jogar

| Ação | Como |
|---|---|
| Mirar e lançar | Clique na raposa, arraste para trás (a linha pontilhada mostra o caminho) e solte |
| Habilidade | Clique com a raposa no ar (ninja: arrancada · bomba: explode) |
| Reiniciar a fase | Botão ↻ ou tecla **R** |
| Voltar ao menu | Botão ☰ ou tecla **Esc** |

**Objetivo:** eliminar todos os tubarões antes de acabarem as raposas. Cada raposa que
sobra vale 10.000 pontos de bônus. As estrelas dependem da pontuação: 1 por vencer, 2 a
partir de 45% e 3 a partir de 75% da pontuação máxima da fase. Vencer uma fase libera a próxima.

## Fases

| Fase | Dificuldade | Raposas | Estrutura |
|---|---|---|---|
| 1 | Fácil | 2 normais | Torre de madeira de dois andares (o desenho do quadro) |
| 2 | Médio | 2 normais + ninja | Casinha de vidro e torre com andar de vidro |
| 3 | Médio | 2 normais + ninja | Mesa de rocha com torre de madeira em cima |
| 4 | Difícil | 2 normais + ninja + bomba | Fortaleza de rocha com tubarão grande |
| 5 | Desafio | normal + ninja + bomba | Duas fortalezas mistas, 5 tubarões |

## A física, script por script

Tudo em `Assets/FoxFling/Scripts`:

| Script | O que mostra |
|---|---|
| `Estilingue.cs` | Lê o mouse e converte a puxada em velocidade (como uma mola: mais esticado = mais rápido) |
| `Trajetoria.cs` | Linha pontilhada calculada com `p = p0 + v·t + ½·g·t²`, sem simular a física |
| `Raposa.cs` | `Kinematic` enquanto espera (o código manda), `Dynamic` ao ser lançada (a física manda); detecção de colisão `Continuous` para não atravessar blocos finos |
| `Destrutivel.cs` | Dano pelo **impulso** da colisão (`ContactPoint2D.normalImpulse`): batida rápida ou de coisa pesada causa mais dano |
| `Bloco.cs` + `MaterialBloco.cs` | Massa = densidade × área (`useAutoMass`), atrito e quique por `PhysicsMaterial2D`; madeira, vidro e rocha são assets que você ajusta no Inspector |
| `Inimigo.cs` | Mesmo dano dos blocos; collider com o contorno do tubarão (`PolygonCollider2D`) |
| `RaposaNinja.cs` | Impulso instantâneo: troca a velocidade direto, na mesma direção |
| `RaposaBomba.cs` + `Explosao.cs` | Explosão feita na mão: `OverlapCircleAll` + `AddForce(..., Impulse)`, mais forte quanto mais perto |
| `GerenciadorFase.cs` | Regras da fase: fila de raposas, pontos, vitória, e derrota só depois que o mundo para |
| `Regras.cs` | As contas puras (dano, estrelas, trajetória), com testes |

Detalhes que deixam a física estável:

- Blocos e inimigos começam **dormindo** (`RigidbodySleepMode2D.StartAsleep`), então a torre não se mexe até alguém encostar nela.
- No primeiro 1 s da fase, impactos não causam dano, enquanto as peças se acomodam.
- O solver 2D usa 10 iterações de velocidade e 6 de posição (Project Settings > Physics 2D), o que deixa as pilhas mais firmes.

### Números para brincar

| Material | Densidade | Vida | Pontos |
|---|---|---|---|
| Madeira | 1 | 50 | 500 |
| Vidro | 0,6 | 15 | 300 |
| Rocha | 3 | 160 | 1.000 |

Ficam em `Assets/FoxFling/Materiais` (Madeira/Vidro/Rocha.asset). Tubarão: vida 20 (grande: 45),
5.000 pontos. Raposas: massa 1 (ninja 0,9, bomba 1,4). Estilingue: puxada máxima 1,5 e força 8,5.

## Abrindo o projeto

1. Unity **6000.6.4f1**
2. Abra a pasta `New Unity Project` pelo Unity Hub
3. Cena inicial: `Assets/FoxFling/Scenes/Menu` (as fases são `Fase1` a `Fase5`)

### Menu "Fox Fling" no Editor

- **Reconstruir tudo**: gera de novo materiais, prefabs e as cenas a partir do código
  (`Assets/FoxFling/Editor`). As cenas são sobrescritas; os números dos materiais que você
  ajustou no Inspector são mantidos.
- **Build WebGL (itch.io)**: gera o jogo para navegador em `Builds/WebGL` (precisa do módulo *Web Build Support*).

### Editando as fases

Dois jeitos:

- **No código:** `Assets/FoxFling/Editor/DefinicoesFases.cs` descreve cada fase como uma
  "planta" (`p.Andar(...)`, `p.Bloco(...)`, `p.Inimigo(...)`). Depois use *Reconstruir tudo*.
  Os testes conferem se nada ficou flutuando, sobreposto ou fora da área.
- **Na cena:** abra `Fase1`...`Fase5` e mova os prefabs à mão. Nesse caso, não use
  *Reconstruir tudo* depois, porque ele apagaria suas mudanças.

## Estrutura

```
Assets/FoxFling/
├── Art/        sprites PNG (gerados por Tools/gerar_arte.py)
├── Audio/      efeitos WAV (gerados por Tools/gerar_sons.py)
├── Editor/     construtor do jogo (assets, prefabs, UI, cenas) e definições das fases
├── Materiais/  materiais de bloco, materiais físicos e de render
├── Prefabs/    raposas, bloco, inimigos, Sistemas (câmera, estilingue, HUD...)
├── Scenes/     Menu, Fase1..Fase5
├── Scripts/    código do jogo
└── Tests/      testes EditMode (Window > General > Test Runner)
Tools/          geradores de arte e som (Python 3 + Pillow)
```
