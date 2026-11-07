using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Todo botão faz "clique" ao ser apertado (ou "erro", se estiver desligado):
// um botão mudo parece quebrado.
public class BotaoComSom : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e)
    {
        var s = Sons.Instancia;
        if (s == null) return;
        var botao = GetComponent<Selectable>();
        if (botao != null && !botao.interactable) Sons.Tocar(s.erro, 0.5f, 0f);
        else Sons.Tocar(s.clique, 0.7f, 0.03f);
    }
}
