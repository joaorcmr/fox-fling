using UnityEngine;
using UnityEngine.UI;

// Tela inicial: escolher a fase (só as liberadas) ou zerar o progresso.
public class MenuPrincipal : MonoBehaviour
{
    public CartaoFase[] cartoes;
    public Button botaoZerar;

    void Start()
    {
        Time.timeScale = 1f;
        Atualizar();
        botaoZerar.onClick.AddListener(() =>
        {
            Sons.Tocar(Sons.I != null ? Sons.I.clique : null);
            Progresso.Zerar();
            Atualizar();
        });
    }

    void Atualizar()
    {
        foreach (var c in cartoes) c.Atualizar();
    }
}
