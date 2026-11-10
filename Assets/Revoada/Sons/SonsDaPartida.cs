using UnityEngine;

// Os sons do que acontece no mapa: estouros, tiros, explosões, compra, venda,
// melhoria, Cofrinho, reparo, Torre apanhando, onda começando, chefe chegando
// e os porcos voltando como moedas na troca de fase. Só escuta a simulação e
// toca. Os sons de interface (clique, erro, fim de jogo, o quadro da troca de
// fase) ficam com os próprios botões e telas, que sabem a hora certa de tocar.
public class SonsDaPartida : MonoBehaviour
{
    public Jogo jogo;

    static Sons S => Sons.Instancia;

    void Start()
    {
        var sim = jogo.Sim;
        sim.PassaroNasceu += p =>
        {
            if (p.Dados.chefe && S != null) Sons.Tocar(S.chefe, 1f, 0f);
        };
        sim.PassaroEstourou += (p, moedas) => Sons.TocarEstouro(p.Dados.chefe ? 1f : 0.55f);
        sim.TorreAtingida += dano =>
        {
            if (S != null) Sons.Tocar(S.torre, 0.9f);
        };
        sim.OndaComecou += n =>
        {
            if (S != null) Sons.Tocar(S.onda, 0.8f, 0f);
        };
        sim.PorcoCriado += p =>
        {
            if (S != null) Sons.Tocar(S.compra, 0.8f);
        };
        sim.PorcoVendido += p =>
        {
            if (S != null) Sons.Tocar(S.venda, 0.8f);
        };
        sim.PorcoMelhorado += p =>
        {
            if (S != null) Sons.Tocar(S.upgrade, 0.9f, 0f);
        };
        sim.PorcosRecolhidos += (porcos, moedas) =>
        {
            if (S != null) Sons.Tocar(S.venda, 1f, 0f); // um tilintar só, não um por porco
        };
        sim.CofrinhoComprado += nivel =>
        {
            if (S != null) Sons.Tocar(S.compra, 0.8f);
        };
        sim.TorreReparada += cura =>
        {
            if (S != null) Sons.Tocar(S.upgrade, 0.9f, 0f);
        };
        sim.TiroDisparado += (porco, tiro) =>
        {
            if (S == null) return;
            var clip = tiro.Tipo == TipoTiro.Arpao ? S.tiroArpao : tiro.Tipo == TipoTiro.Bomba ? S.tiroBomba : S.tiroEstilingue;
            Sons.Tocar(clip, 0.3f);
        };
        sim.BombaExplodiu += (centro, raio) =>
        {
            if (S != null) Sons.Tocar(S.explosao, 0.55f);
        };
    }
}
