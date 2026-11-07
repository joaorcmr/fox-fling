using UnityEngine;
using UnityEngine.UI;

// Esc (sem nada selecionado) ou ☰: pausa e oferece continuar, reiniciar ou
// voltar ao menu. Reiniciar e Menu voltam o Time.timeScale para 1 (no Jogo).
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
        botaoReiniciar.onClick.AddListener(() => jogo.Reiniciar());
        botaoMenu.onClick.AddListener(() => jogo.IrParaMenu());
    }

    public void Abrir()
    {
        Ligar();
        gameObject.SetActive(true);
        jogo.Pausar(true);
    }

    public void Fechar()
    {
        gameObject.SetActive(false);
        jogo.Pausar(false);
    }
}
