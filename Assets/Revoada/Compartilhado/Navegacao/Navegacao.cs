using UnityEngine;
using UnityEngine.SceneManagement;

// Para onde o jogador pode ir: os nomes das cenas ficam só aqui, e toda troca
// passa pelo escurecer da Transicao.
public static class Navegacao
{
    public const string CenaMenu = "Menu";
    public const string CenaJogo = "Jogo";

    public static void Jogar() => Transicao.Carregar(CenaJogo);

    // Recarrega a cena em que a partida está (é a cena Jogo; num teste, a cena do teste).
    public static void ReiniciarPartida() => Transicao.Carregar(SceneManager.GetActiveScene().name);

    public static void IrParaMenu()
    {
        string cena = Application.CanStreamedLevelBeLoaded(CenaMenu) ? CenaMenu : SceneManager.GetActiveScene().name;
        Transicao.Carregar(cena);
    }
}
