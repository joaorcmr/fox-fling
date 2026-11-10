using UnityEngine;
using static Salvamento;

// Os volumes que o jogador escolheu (música e efeitos), de 0 a 1. São
// preferência, não progresso: "Zerar progresso" no menu não mexe aqui.
public static class Preferencias
{
    public const string Efeitos = "efeitos";
    public const string Musica = "musica";

    public static float VolumePadrao(string canal) => canal == Efeitos ? 0.8f : 0.5f;

    public static float Volume(string canal, float padrao) => Mathf.Clamp01(PlayerPrefs.GetFloat(Chave("volume_" + canal), padrao));

    public static float Volume(string canal) => Volume(canal, VolumePadrao(canal));

    public static void SalvarVolume(string canal, float valor)
    {
        PlayerPrefs.SetFloat(Chave("volume_" + canal), Mathf.Clamp01(valor));
        PlayerPrefs.Save();
    }
}
