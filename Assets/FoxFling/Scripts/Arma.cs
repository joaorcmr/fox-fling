using System.Collections.Generic;
using UnityEngine;

// Arma de defesa (issue #7): fica parada no chão e, durante a leva, atira sozinha na raposa
// mais próxima dentro do alcance (Regras.EscolherAlvo). Os números (dano, cadência, alcance)
// vêm do ItemLoja, por nível; o nível é da loja (GerenciadorFase.NivelDaArma), então melhorar
// vale para todas as armas daquele tipo. Também é Destrutivel: as raposas podem quebrá-la.
public class Arma : Destrutivel
{
    // Preenchido pelo Colocador quando a arma é colocada: de onde vêm os números.
    [HideInInspector] public ItemLoja item;

    public Sprite spriteProjetil;
    public Color corProjetil = Color.white;
    public float velocidadeProjetil = 10f;

    float recarga;
    readonly List<Vector2> posicoes = new List<Vector2>();

    // Quebrar uma arma não dá pontos.
    protected override int PontosAoMorrer => 0;

    protected override void Update()
    {
        base.Update();

        var fase = GerenciadorFase.Instance;
        if (Morto || item == null || item.NivelMaximo == 0 || fase == null || fase.Estado != EstadoFase.Jogando) return;

        recarga -= Time.deltaTime;
        if (recarga > 0f) return;

        var nivel = item.Nivel(fase.NivelDaArma(item));

        posicoes.Clear();
        foreach (var raposa in fase.Atacantes)
            posicoes.Add(raposa.transform.position);

        int i = Regras.EscolherAlvo(Boca, posicoes, nivel.alcance);
        if (i < 0) return;

        Projetil.Criar(spriteProjetil, corProjetil, Boca, fase.Atacantes[i], nivel.dano, nivel.raioArea, velocidadeProjetil);
        Sons.Tocar(Sons.I != null ? Sons.I.lancar : null, 0.5f);
        recarga = nivel.intervalo;
    }

    // De onde o tiro sai: perto do topo da arma.
    Vector2 Boca => (Vector2)transform.position + Vector2.up * (item != null ? item.tamanho.y * 0.4f : 0.3f);
}
