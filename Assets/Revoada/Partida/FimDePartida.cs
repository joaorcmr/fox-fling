// Quando a partida acaba, guarda o resultado no Progresso (recorde e vitórias)
// e lembra qual era o recorde antes, para a tela de fim poder dizer "Novo
// recorde!". A tela só mostra; quem registra é a partida.
public class FimDePartida
{
    public int RecordeAnterior { get; private set; }

    // Recorde de onda (só conta na derrota: quem vence chegou à última onda).
    public bool NovoRecorde { get; private set; }

    public FimDePartida(Simulacao sim)
    {
        sim.FimDeJogo += venceu => Registrar(sim.Onda, venceu);
    }

    void Registrar(int onda, bool venceu)
    {
        RecordeAnterior = Progresso.MelhorOnda;
        Progresso.RegistrarFim(onda, venceu);
        NovoRecorde = onda > RecordeAnterior && !venceu;
    }
}
