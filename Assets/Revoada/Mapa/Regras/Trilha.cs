using System.Collections.Generic;
using UnityEngine;

// O caminho dos pássaros. Os pontos do mapa formam uma linha com quinas; cada
// quina vira um arco de círculo, para os pássaros não virarem em ângulo reto.
// O resultado é uma polilinha densa (Amostras), e tudo no jogo mede a posição
// de um pássaro pela DISTÂNCIA percorrida nela: "quem está mais adiantado" é
// só comparar distâncias.
public class Trilha
{
    const float PassoArco = 0.1f; // tamanho máximo de cada pedacinho do arco

    readonly List<Vector2> amostras = new List<Vector2>();
    readonly List<float> acumulado = new List<float>(); // distância desde o início até cada amostra

    public float Comprimento { get; }
    public IReadOnlyList<Vector2> Amostras => amostras;

    public Trilha(IList<Vector2> pontos, float raioCanto)
    {
        if (pontos == null || pontos.Count == 0)
        {
            amostras.Add(Vector2.zero);
            acumulado.Add(0f);
            return;
        }

        Adicionar(pontos[0]);
        for (int i = 1; i < pontos.Count - 1; i++)
            AdicionarCanto(pontos[i - 1], pontos[i], pontos[i + 1], raioCanto);
        if (pontos.Count > 1) Adicionar(pontos[pontos.Count - 1]);

        Comprimento = acumulado[acumulado.Count - 1];
    }

    // Troca a quina "b" por um arco tangente aos dois trechos.
    void AdicionarCanto(Vector2 a, Vector2 b, Vector2 c, float raio)
    {
        Vector2 entrada = b - a, saida = c - b;
        float lenEntrada = entrada.magnitude, lenSaida = saida.magnitude;
        if (lenEntrada < 1e-5f || lenSaida < 1e-5f || raio <= 0f)
        {
            Adicionar(b);
            return;
        }
        entrada /= lenEntrada;
        saida /= lenSaida;

        float angulo = Vector2.Angle(entrada, saida) * Mathf.Deg2Rad; // quanto a trilha vira
        if (angulo < 1e-3f || angulo > Mathf.PI - 1e-3f)
        {
            Adicionar(b); // reta (ou volta completa): não há arco que faça sentido
            return;
        }

        // Distância da quina até onde o arco encosta em cada trecho. Ela não pode
        // passar da metade de nenhum dos dois trechos, senão o arco "come" a reta.
        float tangente = Mathf.Min(raio * Mathf.Tan(angulo / 2f), lenEntrada / 2f, lenSaida / 2f);
        float raioReal = tangente / Mathf.Tan(angulo / 2f);

        Vector2 inicio = b - entrada * tangente;
        Vector2 fim = b + saida * tangente;
        float lado = Mathf.Sign(entrada.x * saida.y - entrada.y * saida.x); // +1 vira à esquerda
        Vector2 normal = new Vector2(-entrada.y, entrada.x) * lado;
        Vector2 centro = inicio + normal * raioReal;

        float anguloInicio = Mathf.Atan2(inicio.y - centro.y, inicio.x - centro.x);
        int pedacos = Mathf.Max(2, Mathf.CeilToInt(raioReal * angulo / PassoArco));
        Adicionar(inicio);
        for (int k = 1; k < pedacos; k++)
        {
            float a2 = anguloInicio + lado * angulo * k / pedacos;
            Adicionar(centro + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * raioReal);
        }
        Adicionar(fim);
    }

    void Adicionar(Vector2 p)
    {
        if (amostras.Count == 0)
        {
            amostras.Add(p);
            acumulado.Add(0f);
            return;
        }
        float passo = Vector2.Distance(amostras[amostras.Count - 1], p);
        if (passo < 1e-5f) return; // ponto repetido: não cria trecho de tamanho zero
        amostras.Add(p);
        acumulado.Add(acumulado[acumulado.Count - 1] + passo);
    }

    // Índice do trecho [i, i+1] que contém a distância d (busca binária).
    int Trecho(float d)
    {
        int lo = 0, hi = acumulado.Count - 2;
        if (hi < 0) return 0;
        while (lo < hi)
        {
            int meio = (lo + hi + 1) / 2;
            if (acumulado[meio] <= d) lo = meio;
            else hi = meio - 1;
        }
        return lo;
    }

    public Vector2 Posicao(float distancia)
    {
        if (amostras.Count == 1) return amostras[0];
        float d = Mathf.Clamp(distancia, 0f, Comprimento);
        int i = Trecho(d);
        float tamanho = acumulado[i + 1] - acumulado[i];
        float t = tamanho > 0f ? (d - acumulado[i]) / tamanho : 0f;
        return Vector2.Lerp(amostras[i], amostras[i + 1], t);
    }

    public Vector2 Direcao(float distancia)
    {
        if (amostras.Count == 1) return Vector2.right;
        int i = Trecho(Mathf.Clamp(distancia, 0f, Comprimento));
        return (amostras[i + 1] - amostras[i]).normalized;
    }

    // Menor distância de um ponto até a trilha (usada para não deixar
    // colocar porco em cima dela).
    public float DistanciaAte(Vector2 ponto)
    {
        if (amostras.Count == 1) return Vector2.Distance(ponto, amostras[0]);
        float melhor = float.MaxValue;
        for (int i = 0; i < amostras.Count - 1; i++)
        {
            Vector2 a = amostras[i], ab = amostras[i + 1] - a;
            float t = Mathf.Clamp01(Vector2.Dot(ponto - a, ab) / ab.sqrMagnitude);
            float dist = Vector2.Distance(ponto, a + ab * t);
            if (dist < melhor) melhor = dist;
        }
        return melhor;
    }
}
