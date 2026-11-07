using System;
using System.Collections.Generic;
using UnityEngine;

// Os NÚMEROS do jogo, separados da lógica. Cada classe é [Serializable] com
// campos públicos para aparecer no Inspector dentro dos ScriptableObjects
// (Assets/Revoada/Dados): balancear o jogo é editar esses valores, sem mexer
// em código. A simulação só lê; quem cria os valores padrão é o construtor
// de Editor (DefinicoesPadrao).

[Serializable]
public class DadosPassaro
{
    public EspeciePassaro especie;
    public string nome;
    [Tooltip("Quantos pontos de dano aguenta antes de estourar.")]
    public float vida = 1f;
    [Tooltip("Unidades por segundo ao longo da trilha (1 unidade = 32 px).")]
    public float velocidade = 2.5f;
    [Tooltip("Raio do corpo, usado para saber se um tiro encostou.")]
    public float raio = 0.35f;
    [Tooltip("Quanto de vida tira da Torre dos Porcos se chegar até ela.")]
    public int danoTorre = 1;
    [Tooltip("Penas que deixa ao estourar (antes do multiplicador do Ninho).")]
    public float penas = 1f;
    [Tooltip("Couraça: pedrinha e arpão causam só metade do dano.")]
    public bool couraca;
    public bool chefe;
    [Tooltip("Pássaro que nasce quando este estoura (a pata solta patinhos).")]
    public EspeciePassaro filho;
    public int quantidadeFilhos;
}

// O bloco de números de um porco num nível. Uma melhoria guarda o bloco
// COMPLETO do nível seguinte (não a diferença), então o Inspector mostra
// exatamente como o porco fica depois de comprá-la.
[Serializable]
public class Atributos
{
    public TipoTiro tiro = TipoTiro.Pedrinha;
    public float dano = 1f;
    public float alcance = 3f;
    [Tooltip("Segundos entre um disparo e outro.")]
    public float recarga = 0.8f;
    public float velocidadeProjetil = 12f;
    public float raioProjetil = 0.15f;
    [Tooltip("Quantos pássaros um mesmo tiro atravessa.")]
    public int perfura = 1;
    [Tooltip("O tiro voa até alcance × alcanceVoo antes de sumir.")]
    public float alcanceVoo = 1.3f;
    [Tooltip("Tiros por disparo (a Rajada solta 3 em leque).")]
    public int tiros = 1;
    [Tooltip("Graus entre os tiros do leque.")]
    public float abertura;
    [Tooltip("Raio da explosão (bomba).")]
    public float raioArea;
    [Tooltip("Fração da velocidade que o pássaro perde (0,4 = 40% mais lento).")]
    public float lentidao;
    public float duracaoLentidao;
    [Tooltip("Quanto o pássaro volta na trilha quando é atingido.")]
    public float empurrao;
    public bool ignoraCouraca;

    [Header("Tamboreiro (aura)")]
    public float raioAura;
    [Tooltip("0,2 = os vizinhos atiram 20% mais rápido.")]
    public float bonusCadencia;
    public float bonusAlcance;
    [Tooltip("0,5 = pássaros estourados dentro da aura rendem 50% mais penas.")]
    public float bonusPenas;

    public Atributos Copia() => (Atributos)MemberwiseClone();
}

[Serializable]
public class Melhoria
{
    public string nome;
    public string descricao;
    public int custo;
    public Atributos atributos = new Atributos();
}

[Serializable]
public class DadosPorco
{
    public EspeciePorco especie;
    public string nome;
    public string papel;
    public int custo;
    [Tooltip("Espaço que o porco ocupa no mapa.")]
    public float raio = 0.45f;
    public Atributos nivel1 = new Atributos();
    public Melhoria nivel2 = new Melhoria();
    public Melhoria nivel3 = new Melhoria();
    public Melhoria especializacaoA = new Melhoria();
    public Melhoria especializacaoB = new Melhoria();
}

[Serializable]
public class GrupoOnda
{
    public EspeciePassaro especie;
    public int quantidade = 1;
    [Tooltip("Segundos entre um pássaro e o próximo do grupo.")]
    public float intervalo = 1f;
    [Tooltip("Segundos depois do início da onda para o grupo começar.")]
    public float atraso;
    [Tooltip("Multiplica a vida (os chefes das ondas 20 e 30 são mais fortes).")]
    public float multiplicadorVida = 1f;
}

[Serializable]
public class DadosOnda
{
    public List<GrupoOnda> grupos = new List<GrupoOnda>();
}

[Serializable]
public class NivelNinho
{
    public float multiplicador;
    public int custo;
}

[Serializable]
public class DadosEconomia
{
    public int penasIniciais = 300;
    public int vidaInicial = 100;
    [Tooltip("Bônus de fim de onda = base + porOnda × número da onda.")]
    public int bonusOndaBase = 25;
    public int bonusOndaPorOnda = 5;
    [Tooltip("Fração do que foi gasto que volta ao vender um porco.")]
    public float fracaoVenda = 0.7f;
    [Tooltip("Segundos entre uma onda e a próxima com o Auto ligado.")]
    public float esperaAuto = 2f;
    public List<NivelNinho> ninho = new List<NivelNinho>();
}

[Serializable]
public class Obstaculo
{
    public string tipo;
    public Vector2 centro;
    public float raio;
}

[Serializable]
public class DadosMapa
{
    public string nome;
    public Vector2 tamanho = new Vector2(14f, 15f);
    [Tooltip("Pontos da trilha, do lugar onde os pássaros entram até a Torre.")]
    public List<Vector2> pontos = new List<Vector2>();
    public float raioCanto = 1f;
    [Tooltip("Metade da largura da trilha: porco não pode ficar em cima dela.")]
    public float meiaLargura = 0.6f;
    public Vector2 torre;
    public float raioTorre = 1.1f;
    public List<Obstaculo> obstaculos = new List<Obstaculo>();
}

// O pacote que a simulação recebe. Não é [Serializable]: quem monta é o
// ConversorDados (a partir dos assets) ou os testes (à mão).
public class DadosJogo
{
    public Dictionary<EspeciePassaro, DadosPassaro> passaros = new Dictionary<EspeciePassaro, DadosPassaro>();
    public Dictionary<EspeciePorco, DadosPorco> porcos = new Dictionary<EspeciePorco, DadosPorco>();
    public List<DadosOnda> ondas = new List<DadosOnda>();
    public DadosEconomia economia = new DadosEconomia();
    public DadosMapa mapa = new DadosMapa();
}
