using UnityEngine;
using UnityEngine.UI;

// A prévia da próxima onda: quem vem e quantos, e um aviso quando estreia um
// tipo novo ("Novo: Urubu (couraça)"), vem chefe ou a próxima onda já é num
// mapa novo ("Mapa novo: Pomar", durante a última onda de uma fase). Ver o que
// vem é o que permite decidir se dá para gastar em income agora.
public class PreviaOnda : MonoBehaviour
{
    public Jogo jogo;
    public Text titulo;
    public Image[] icones;
    public Text[] quantidades;
    public Text aviso;

    int mostrando = -1, faseMostrada = -1;

    void Update()
    {
        int proxima = jogo.Sim.Onda + 1;
        // A fase também conta: o aviso de "mapa novo" sai assim que o mapa troca.
        if (proxima != mostrando || jogo.Sim.Fase != faseMostrada) Mostrar(proxima);
    }

    public void Mostrar(int numero)
    {
        mostrando = numero;
        var s = jogo.Sim;
        faseMostrada = s.Fase;
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
        int faseDaOnda = s.FaseDaOnda(numero);
        if (chefe) aviso.text = "CHEFE!";
        else if (faseDaOnda != s.Fase) aviso.text = "Mapa novo: " + s.Dados.fases[faseDaOnda - 1].nome;
        else if (novos.Count > 0 && numero > 1) // na onda 1 tudo é novo
        {
            var p = s.Dados.passaros[novos[0]];
            // "Novo: <nome> (<detalhe>)" quebra em 2 linhas pros detalhes mais
            // compridos ("solta filhotes"), e aí o cartão só tem espaço certo
            // pra 1 linha (o aviso fica colado no botão de soltar a onda
            // embaixo). Sem o detalhe sempre cabe: sobra ele, não a tela.
            aviso.text = "Novo: " + Descrever(p);
            if (aviso.preferredWidth > aviso.rectTransform.rect.width) aviso.text = "Novo: " + p.nome;
        }
        else aviso.text = "";
        aviso.fontStyle = chefe ? FontStyle.Bold : FontStyle.Normal; // em negrito, "Novo: Urubu (couraça)" não cabia na linha
    }

    static string Descrever(DadosPassaro p)
    {
        if (p.couraca) return p.nome + " (couraça)";
        if (p.quantidadeFilhos > 0) return p.nome + " (solta filhotes)";
        if (p.velocidade >= 4f) return p.nome + " (rápido)";
        return p.nome;
    }
}
