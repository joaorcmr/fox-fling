using UnityEngine;
using UnityEngine.UI;

// Um controle de volume (Música ou Efeitos) ligado à camada de ganho do Sons.
public class ControleVolume : MonoBehaviour
{
    public Slider slider;
    [Tooltip("\"efeitos\" ou \"musica\"")]
    public string canal = Preferencias.Efeitos;

    void Start()
    {
        slider.SetValueWithoutNotify(Preferencias.Volume(canal));
        slider.onValueChanged.AddListener(v => Sons.DefinirVolume(canal, v));
    }
}
