using UnityEngine;

// Um porco posicionado no mapa (só estado; quem desenha é o VisualPorco).
public class Porco
{
    public readonly int Id;
    public readonly DadosPorco Dados;
    public Vector2 Posicao;

    public int Nivel = 1;            // 1..3; 4 = especializado
    public int Especializacao = -1;  // -1 nenhuma, 0 = A, 1 = B
    public ModoAlvo Modo = ModoAlvo.Primeiro;
    public float Recarga;            // segundos até poder atirar de novo
    public int Gasto;                // tudo o que já foi gasto nele (a venda devolve 70%; a troca de fase, tudo)
    public Vector2 Mira = Vector2.right; // para onde atirou por último (a arma gira para cá)
    public Atributos Atributos;      // os números do nível atual

    // Bônus das auras de Tamboreiros por perto (recalculados quando os porcos mudam).
    public float MultCadencia = 1f;
    public float MultAlcance = 1f;

    public EspeciePorco Especie => Dados.especie;
    public float Alcance => Atributos.alcance * MultAlcance;
    public bool Ataca => Atributos.tiro != TipoTiro.Nenhum;

    public Porco(int id, DadosPorco dados, Vector2 posicao)
    {
        Id = id;
        Dados = dados;
        Posicao = posicao;
        Atributos = dados.nivel1.Copia();
    }
}
