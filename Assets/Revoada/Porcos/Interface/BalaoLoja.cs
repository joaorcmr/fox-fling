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

    // Só o número, sem a palavra "moedas": com o nome mais comprido
    // (Estilingueiro) e "moedas", o título não cabia numa linha só e
    // invadia o texto do papel do porco embaixo.
    public void Mostrar(TipoPorco tipo, RectTransform cartao) =>
        Mostrar($"{tipo.dados.nome} · {tipo.dados.custo}", tipo.dados.papel, cartao);

    public void Mostrar(string titulo2, string texto2, RectTransform cartao)
    {
        titulo.text = titulo2;
        texto.text = texto2;
        gameObject.SetActive(true);

        // A altura acompanha o texto: as descrições têm tamanhos bem
        // diferentes (a do Bombardeiro é quase o dobro da do Cofrinho no
        // máximo), e um fundo de altura fixa deixava a descrição comprida
        // vazar pra fora do balão, por cima do que estivesse embaixo dele.
        var inicioTexto = -texto.rectTransform.anchoredPosition.y; // distância do topo até o texto começar
        var margemEmbaixo = 10f;
        var altura = Mathf.Max(62f, inicioTexto + texto.preferredHeight + margemEmbaixo);
        caixa.sizeDelta = new Vector2(caixa.sizeDelta.x, altura);

        var p = caixa.position;
        var centro = cartao.TransformPoint(cartao.rect.center);
        caixa.position = new Vector3(p.x, centro.y, p.z);
    }

    public void Esconder() => gameObject.SetActive(false);
}
