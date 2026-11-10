using UnityEngine;
using UnityEngine.UI;

// O balão que aparece ao passar o mouse num item da loja: título e um texto
// embaixo. Fica à esquerda da barra lateral, na altura do que foi passado o
// mouse. Um porco da loja usa o Mostrar(TipoPorco, ...); qualquer outro botão
// (o Reparo, por exemplo) usa o Mostrar(titulo, texto, ...) direto.
public class BalaoLoja : MonoBehaviour
{
    public RectTransform caixa;
    public Text titulo;
    public Text texto;

    void Awake() => gameObject.SetActive(false);

    public void Mostrar(TipoPorco tipo, RectTransform cartao) =>
        Mostrar($"{tipo.dados.nome} · {tipo.dados.custo} moedas", tipo.dados.papel, cartao);

    public void Mostrar(string titulo2, string texto2, RectTransform cartao)
    {
        titulo.text = titulo2;
        texto.text = texto2;
        gameObject.SetActive(true);
        var p = caixa.position;
        var centro = cartao.TransformPoint(cartao.rect.center);
        caixa.position = new Vector3(p.x, centro.y, p.z);
    }

    public void Esconder() => gameObject.SetActive(false);
}
