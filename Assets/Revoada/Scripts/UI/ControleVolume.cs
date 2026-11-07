using UnityEngine;
using UnityEngine.UI;

// Um controle de volume (Música ou Efeitos) ligado à camada de ganho do Sons.
public class ControleVolume : MonoBehaviour
{
    public Slider slider;
    [Tooltip("\"efeitos\" ou \"musica\"")]
    public string canal = "efeitos";

    void Start()
    {
        slider.SetValueWithoutNotify(Progresso.Volume(canal, canal == "efeitos" ? 0.8f : 0.5f));
        slider.onValueChanged.AddListener(v => Sons.DefinirVolume(canal, v));
    }
}
