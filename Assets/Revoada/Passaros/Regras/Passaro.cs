using UnityEngine;

// Um pássaro vivo na simulação. Não é um GameObject: é só estado. Quem desenha
// é o VisualPassaro, que lê estes campos a cada quadro.
public class Passaro
{
    public readonly int Id;
    public readonly DadosPassaro Dados;

    public float Vida;
    public float VidaMaxima;
    public float Distancia;        // quanto já andou na trilha (é a "posição" de verdade)
    public float Lentidao;         // 0,4 = 40% mais lento
    public float TempoLentidao;
    public Vector2 Posicao;
    public Vector2 PosicaoAnterior; // posição no passo anterior, para a tela interpolar
    public bool Vivo = true;

    public EspeciePassaro Especie => Dados.especie;
    public float Raio => Dados.raio;
    public float Velocidade => Dados.velocidade * (1f - Lentidao);

    public Passaro(int id, DadosPassaro dados, float multiplicadorVida, float distancia)
    {
        Id = id;
        Dados = dados;
        VidaMaxima = Vida = dados.vida * multiplicadorVida;
        Distancia = distancia;
    }
}
