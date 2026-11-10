using UnityEngine;
using UnityEngine.UI;

// Esc (sem nada selecionado) ou ☰: pausa e oferece continuar, reiniciar ou
// voltar ao menu. Com Reiniciar ou Menu o jogo segue pausado durante o
// escurecer, e o Time.timeScale volta a 1 na troca de cena (Transicao).
public class MenuPausa : MonoBehaviour
{
    public Jogo jogo;
    public Button botaoContinuar;
    public Button botaoReiniciar;
    public Button botaoMenu;

    bool ligado;

    public bool Aberto => gameObject.activeSelf;

    void Ligar()
    {
        if (ligado) return;
        ligado = true;
        botaoContinuar.onClick.AddListener(Fechar);
        botaoReiniciar.onClick.AddListener(Navegacao.ReiniciarPartida);
        botaoMenu.onClick.AddListener(Navegacao.IrParaMenu);
    }

    public void Abrir()
    {
        Ligar();
        gameObject.SetActive(true);
        jogo.Relogio.Pausar(true);
    }

    public void Fechar()
    {
        gameObject.SetActive(false);
        jogo.Relogio.Pausar(false);
    }
}
