using UnityEngine;
using UnityEngine.UI;

// O balão que aparece ao passar o mouse num porco da loja: nome, papel e
// preço. Fica à esquerda da barra lateral, na altura do cartão.
public class BalaoLoja : MonoBehaviour
{
    public RectTransform caixa;
    public Text titulo;
    public Text texto;

    void Awake() => gameObject.SetActive(false);

    public void Mostrar(TipoPorco tipo, RectTransform cartao)
    {
        titulo.text = $"{tipo.dados.nome} · {tipo.dados.custo} penas";
        texto.text = tipo.dados.papel;
        gameObject.SetActive(true);
        var p = caixa.position;
        var centro = cartao.TransformPoint(cartao.rect.center);
        caixa.position = new Vector3(p.x, centro.y, p.z);
    }

    public void Esconder() => gameObject.SetActive(false);
}
