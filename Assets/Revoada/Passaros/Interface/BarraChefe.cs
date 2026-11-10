using UnityEngine;
using UnityEngine.UI;

// A barra de vida do chefe no alto do mapa, enquanto ele estiver voando.
public class BarraChefe : MonoBehaviour
{
    public Image preenchimento; // Image do tipo Filled
    public Text nome;
    public CanvasGroup grupo;

    Passaro chefe;

    void Awake() => grupo.alpha = 0f;

    public void Seguir(Passaro p)
    {
        chefe = p;
        nome.text = p.Dados.nome;
    }

    void Update()
    {
        if (chefe == null || !chefe.Vivo)
        {
            chefe = null;
            grupo.alpha = 0f;
            return;
        }
        grupo.alpha = 1f;
        preenchimento.fillAmount = Mathf.Clamp01(chefe.Vida / chefe.VidaMaxima);
    }
}
