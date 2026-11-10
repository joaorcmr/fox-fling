using UnityEngine;
using static Salvamento;

// O progresso do jogador entre partidas: recorde de onda, vitórias e as dicas
// que já apareceram. Os volumes não ficam aqui: são preferência, não progresso
// (ver Preferencias).
public static class Progresso
{
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

    // Apaga recorde, vitórias e dicas vistas (os volumes ficam).
    public static void Zerar()
    {
        PlayerPrefs.DeleteKey(Chave("melhor_onda"));
        PlayerPrefs.DeleteKey(Chave("vitorias"));
        foreach (var id in CatalogoDicas.Ids) PlayerPrefs.DeleteKey(Chave("dica_" + id));
        PlayerPrefs.Save();
    }
}
