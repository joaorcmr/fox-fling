using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dicas curtas que ensinam sem travar o jogo: um balão no canto do mapa que
// some sozinho, e cada dica aparece uma vez só (fica marcada no Progresso).
public class Dicas : MonoBehaviour
{
    public static readonly string[] Ids = { "inicio", "arpao", "ninho", "couraca", "tambor" };
    const float Duracao = 6f;

    public Jogo jogo;
    public Controles controles;
    public CanvasGroup grupo;
    public Text texto;

    readonly Queue<string> fila = new Queue<string>();
    float fim = -1f;

    void Start()
    {
        grupo.alpha = 0f;
        var s = jogo.Sim;
        Mostrar("inicio", "Clique num porco da loja e depois no mapa para colocá-lo. Depois, solte a revoada.");
        s.PorcoCriado += p =>
        {
            if (p.Especie == EspeciePorco.Arpoador)
                Mostrar("arpao", "O arpão atravessa os pássaros em fila: no fim de uma reta ele rende mais.");
        };
        s.OndaTerminou += (onda, bonus) =>
        {
            if (onda == 3) Mostrar("ninho", "O Ninho de Penas faz cada pássaro render mais. Vale a pena agora?");
            foreach (var e in s.Estreias(onda + 1))
                if (s.Dados.passaros[e].couraca)
                    Mostrar("couraca", "Couraça: pedrinha e arpão só tiram metade. Bomba e Ponta de Ferro tiram tudo.");
        };
        controles.SelecaoMudou += p =>
        {
            if (p != null && p.Especie == EspeciePorco.Tamboreiro)
                Mostrar("tambor", "Porcos dentro do círculo do Tamboreiro atiram mais rápido. Agrupe!");
        };
        s.FimDeJogo += venceu =>
        {
            fila.Clear(); // a tela de fim não divide espaço com dica
            fim = -1f;
        };
    }

    public void Mostrar(string id, string conteudo)
    {
        if (Progresso.DicaVista(id)) return;
        Progresso.MarcarDica(id);
        fila.Enqueue(conteudo);
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
