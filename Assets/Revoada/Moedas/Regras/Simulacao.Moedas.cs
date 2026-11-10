using System;

// As moedas do jogador e o Cofrinho, vistos pela simulação. A conta em si
// (resto fracionário, custo de cada nível, retorno) fica na Economia.
public partial class Simulacao
{
    public readonly Economia Economia;

    public int Moedas => Economia.Moedas;

    public event Action<int> CofrinhoComprado; // o nível novo (1, 2, 3...)

    public Resultado ComprarCofrinho()
    {
        if (Acabou || Economia.ProximoCofrinho == null) return Resultado.Indisponivel;
        if (!Economia.ComprarCofrinho()) return Resultado.SemMoedas;
        CofrinhoComprado?.Invoke(Economia.NivelCofrinho);
        return Resultado.Ok;
    }

    // Conta a partir da próxima onda que ainda vai começar.
    public int? OndasParaPagarCofrinho() => Economia.OndasParaPagar(moedasBasePorOnda, Onda + 1);
}
