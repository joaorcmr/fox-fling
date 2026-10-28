using UnityEngine;

// Interface da fase (pontos, dicas, tela de resultado). Implementada na Task 5.
public class Hud : MonoBehaviour
{
    public void Configurar(string nomeFase, string dificuldade) { }

    public void AtualizarPontos(int pontos) { }

    public void MostrarDica(string texto) { }

    public void MostrarResultado(bool venceu, int pontos, int estrelas, bool temProxima) { }
}
