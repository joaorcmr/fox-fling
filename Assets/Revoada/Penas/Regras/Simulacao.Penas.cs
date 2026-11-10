// As penas do jogador e o Ninho de Penas, vistos pela simulação. A conta em si
// (resto fracionário, custo de cada nível, retorno) fica na Economia.
public partial class Simulacao
{
    public readonly Economia Economia;

    public int Penas => Economia.Penas;

    public Resultado ComprarNinho()
    {
        if (Acabou || Economia.ProximoNinho == null) return Resultado.Indisponivel;
        return Economia.ComprarNinho() ? Resultado.Ok : Resultado.SemPenas;
    }

    // Conta a partir da próxima onda que ainda vai começar.
    public int? OndasParaPagarNinho() => Economia.OndasParaPagar(penasBasePorOnda, Onda + 1);
}
