using UnityEngine;
using UnityEngine.UI;

// A prévia da próxima onda: quem vem e quantos, e um aviso quando estreia um
// tipo novo ("Novo: Urubu (couraça)") ou vem chefe. Ver o que vem é o que
// permite decidir se dá para gastar em income agora.
public class PreviaOnda : MonoBehaviour
{
    public Jogo jogo;
    public Text titulo;
    public Image[] icones;
    public Text[] quantidades;
    public Text aviso;

    int mostrando = -1;

    void Update()
    {
        int proxima = jogo.Sim.Onda + 1;
        if (proxima != mostrando) Mostrar(proxima);
    }

    public void Mostrar(int numero)
    {
        mostrando = numero;
        var s = jogo.Sim;
        if (numero > s.TotalOndas)
        {
            titulo.text = "Última onda!";
            for (int i = 0; i < icones.Length; i++) icones[i].enabled = quantidades[i].enabled = false;
            aviso.text = "";
            return;
        }

        titulo.text = $"Próxima: onda {numero}";
        var resumo = s.ResumoOnda(numero);
        for (int i = 0; i < icones.Length; i++)
        {
            bool tem = i < resumo.Count;
            icones[i].enabled = quantidades[i].enabled = tem;
            if (!tem) continue;
            icones[i].sprite = jogo.Tipos.De(resumo[i].Key).sprite;
            quantidades[i].text = "×" + resumo[i].Value;
        }

        bool chefe = resumo.Exists(k => s.Dados.passaros[k.Key].chefe);
        var novos = s.Estreias(numero);
        if (chefe) aviso.text = "CHEFE!";
        else if (novos.Count > 0 && numero > 1) aviso.text = "Novo: " + Descrever(s.Dados.passaros[novos[0]]); // na onda 1 tudo é novo
        else aviso.text = "";
    }

    static string Descrever(DadosPassaro p)
    {
        if (p.couraca) return p.nome + " (couraça)";
        if (p.quantidadeFilhos > 0) return p.nome + " (solta filhotes)";
        if (p.velocidade >= 4f) return p.nome + " (rápido)";
        return p.nome;
    }
}
