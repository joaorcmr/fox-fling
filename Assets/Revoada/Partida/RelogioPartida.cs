using UnityEngine;

// Pausa e velocidade da partida: são só o Time.timeScale do Unity. Com 2× ou 3×
// o Unity chama o FixedUpdate 2 ou 3 vezes por quadro; com 0 não chama nenhuma.
// Os comandos (comprar, melhorar) continuam funcionando pausado, porque não
// dependem do passo.
//
// Reiniciar e ir para o menu não mexem aqui: durante o escurecer o jogo fica
// como estava (pausado, se veio do menu de pausa), e a Transicao volta o tempo
// para 1 na hora de trocar de cena.
public class RelogioPartida
{
    public int Velocidade { get; private set; } = 1;
    public bool Pausado { get; private set; }

    // A partida sempre começa andando em 1×.
    public RelogioPartida() => Aplicar();

    public void AlternarPausa()
    {
        Pausado = !Pausado;
        Aplicar();
    }

    public void Pausar(bool pausar)
    {
        Pausado = pausar;
        Aplicar();
    }

    public void ProximaVelocidade()
    {
        Velocidade = Velocidade >= 3 ? 1 : Velocidade + 1;
        Aplicar();
    }

    void Aplicar() => Time.timeScale = Pausado ? 0f : Velocidade;
}
