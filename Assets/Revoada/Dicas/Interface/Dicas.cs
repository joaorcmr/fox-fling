using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dicas curtas que ensinam sem travar o jogo: um balão no canto do mapa que
// some sozinho, e cada dica aparece uma vez só (fica marcada no Progresso).
// Os textos ficam no CatalogoDicas; aqui é QUANDO cada uma aparece e o balão.
public class Dicas : MonoBehaviour
{
    const float Duracao = 6f;

    public Jogo jogo;
    public CanvasGroup grupo;
    public Text texto;

    readonly Queue<string> fila = new Queue<string>();
    float fim = -1f;

    void Start()
    {
        grupo.alpha = 0f;
        var s = jogo.Sim;
        Mostrar(CatalogoDicas.Inicio);
        s.PorcoCriado += p =>
        {
            if (p.Especie == EspeciePorco.Arpoador) Mostrar(CatalogoDicas.Arpao);
        };
        s.OndaTerminou += (onda, bonus) =>
        {
            if (onda == 3) Mostrar(CatalogoDicas.Cofrinho);
            foreach (var e in s.Estreias(onda + 1))
                if (s.Dados.passaros[e].couraca) Mostrar(CatalogoDicas.Couraca);
        };
        jogo.Selecao.SelecaoMudou += p =>
        {
            if (p != null && p.Especie == EspeciePorco.Tamboreiro) Mostrar(CatalogoDicas.Tambor);
        };
        s.FimDeJogo += venceu =>
        {
            fila.Clear(); // a tela de fim não divide espaço com dica
            fim = -1f;
        };
    }

    public void Mostrar(string id)
    {
        if (Progresso.DicaVista(id)) return;
        Progresso.MarcarDica(id);
        fila.Enqueue(CatalogoDicas.Texto(id));
    }

    void Update()
    {
        float agora = Time.unscaledTime;
        if (agora > fim && fila.Count > 0)
        {
            texto.text = fila.Dequeue();
            fim = agora + Duracao;
        }
        float resta = fim - agora;
        grupo.alpha = resta <= 0f ? 0f : Mathf.Clamp01(Mathf.Min((Duracao - resta) / 0.25f, resta / 0.5f));
    }
}
