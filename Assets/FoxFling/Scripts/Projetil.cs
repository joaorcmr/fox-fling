using UnityEngine;

// O que as armas atiram. Voa em arco até a raposa visada (sem física: não esbarra na torre) e,
// ao chegar, causa o dano: em uma raposa só, ou em área (raio > 0) só nas raposas — a
// explosão do canhão não machuca os tubarões nem os blocos.
public class Projetil : MonoBehaviour
{
    const float DistanciaDeAcerto = 0.15f;
    const float AlturaDoArco = 0.6f;

    // Quanto do dano uma raposa na borda da explosão ainda leva.
    const float DanoMinimoNaBorda = 0.4f;

    RaposaAtacante alvo;
    Vector2 destino, posicaoNoChao;
    float dano, raioArea, velocidade, distanciaInicial;

    public static void Criar(Sprite sprite, Color cor, Vector2 origem, RaposaAtacante alvo,
        float dano, float raioArea, float velocidade)
    {
        var go = new GameObject("Projetil");
        go.transform.position = origem;

        var desenho = go.AddComponent<SpriteRenderer>();
        desenho.sprite = sprite;
        desenho.color = cor;
        desenho.sortingOrder = 9;
        if (sprite != null)
        {
            // Tamanho na tela: pequeno para o tiro simples, maior para a bola do canhão.
            float lado = raioArea > 0f ? 0.4f : 0.22f;
            go.transform.localScale = Vector3.one * (lado / Mathf.Max(0.01f, sprite.bounds.size.x));
        }

        var p = go.AddComponent<Projetil>();
        p.alvo = alvo;
        p.destino = alvo.transform.position;
        p.posicaoNoChao = origem;
        p.dano = dano;
        p.raioArea = raioArea;
        p.velocidade = velocidade;
        p.distanciaInicial = Mathf.Max(0.5f, Vector2.Distance(origem, p.destino));
    }

    void Update()
    {
        // Persegue a raposa; se ela morreu no caminho, o tiro simples some e o de área
        // continua até onde ela estava.
        if (alvo != null && !alvo.Morto) destino = alvo.transform.position;
        else if (raioArea <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        posicaoNoChao = Vector2.MoveTowards(posicaoNoChao, destino, velocidade * Time.deltaTime);

        // Arco: sobe e desce conforme anda do ponto de partida ao destino.
        float restante = Vector2.Distance(posicaoNoChao, destino);
        float progresso = Mathf.Clamp01(1f - restante / distanciaInicial);
        float altura = Mathf.Sin(progresso * Mathf.PI) * AlturaDoArco;
        transform.position = new Vector3(posicaoNoChao.x, posicaoNoChao.y + altura, 0f);

        if (restante <= DistanciaDeAcerto)
        {
            Acertar();
            Destroy(gameObject);
        }
    }

    void Acertar()
    {
        if (raioArea <= 0f)
        {
            if (alvo != null) alvo.ReceberDano(dano);
            Efeitos.Fumaca(destino, 0.25f);
            return;
        }

        Efeitos.Explosao(destino, raioArea);
        Sons.Tocar(Sons.I != null ? Sons.I.explosao : null, 0.5f);

        foreach (var colisor in Physics2D.OverlapCircleAll(destino, raioArea))
        {
            var raposa = colisor.GetComponent<RaposaAtacante>();
            if (raposa == null || raposa.Morto) continue;

            float fator = Regras.FatorExplosao(Vector2.Distance(colisor.ClosestPoint(destino), destino), raioArea);
            raposa.ReceberDano(dano * Mathf.Max(DanoMinimoNaBorda, fator));
        }
    }
}
