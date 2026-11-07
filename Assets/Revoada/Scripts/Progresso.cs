using UnityEngine;

// O que o jogo lembra entre partidas, em PlayerPrefs (no WebGL, o PlayerPrefs
// fica no armazenamento do navegador). O prefixo existe para os testes usarem
// chaves próprias e não apagarem o progresso de quem joga.
public static class Progresso
{
    public static string Prefixo = "revoada_";

    static string Chave(string nome) => Prefixo + nome;

    public static int MelhorOnda => PlayerPrefs.GetInt(Chave("melhor_onda"), 0);
    public static int Vitorias => PlayerPrefs.GetInt(Chave("vitorias"), 0);

    public static void RegistrarFim(int onda, bool venceu)
    {
        if (onda > MelhorOnda) PlayerPrefs.SetInt(Chave("melhor_onda"), onda);
        if (venceu) PlayerPrefs.SetInt(Chave("vitorias"), Vitorias + 1);
        PlayerPrefs.Save();
    }

    public static bool DicaVista(string id) => PlayerPrefs.GetInt(Chave("dica_" + id), 0) == 1;

    public static void MarcarDica(string id)
    {
        PlayerPrefs.SetInt(Chave("dica_" + id), 1);
        PlayerPrefs.Save();
    }

    public static float Volume(string canal, float padrao) => Mathf.Clamp01(PlayerPrefs.GetFloat(Chave("volume_" + canal), padrao));

    public static void SalvarVolume(string canal, float valor)
    {
        PlayerPrefs.SetFloat(Chave("volume_" + canal), Mathf.Clamp01(valor));
        PlayerPrefs.Save();
    }

    // Apaga recorde, vitórias e dicas vistas (os volumes ficam: são preferência, não progresso).
    public static void Zerar()
    {
        PlayerPrefs.DeleteKey(Chave("melhor_onda"));
        PlayerPrefs.DeleteKey(Chave("vitorias"));
        foreach (var id in Dicas.Ids) PlayerPrefs.DeleteKey(Chave("dica_" + id));
        PlayerPrefs.Save();
    }
}
