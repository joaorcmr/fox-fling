using UnityEngine;

// Progresso salvo no computador (PlayerPrefs): até qual fase está liberada
// e o melhor número de estrelas de cada uma. Funciona também no WebGL.
public static class Progresso
{
    public const int TotalFases = 5;

    // Os testes trocam o prefixo para não mexer no progresso de verdade.
    public static string Prefixo = "foxfling_";

    static string ChaveLiberada => Prefixo + "liberada";
    static string ChaveEstrelas(int fase) => Prefixo + "estrelas_" + fase;

    public static int FaseMaisAltaLiberada =>
        Mathf.Clamp(PlayerPrefs.GetInt(ChaveLiberada, 1), 1, TotalFases);

    public static int MelhoresEstrelas(int fase) => PlayerPrefs.GetInt(ChaveEstrelas(fase), 0);

    public static void RegistrarVitoria(int fase, int estrelas)
    {
        if (estrelas > MelhoresEstrelas(fase))
            PlayerPrefs.SetInt(ChaveEstrelas(fase), estrelas);

        int proxima = Mathf.Min(fase + 1, TotalFases);
        if (proxima > FaseMaisAltaLiberada)
            PlayerPrefs.SetInt(ChaveLiberada, proxima);

        PlayerPrefs.Save();
    }

    public static void Zerar()
    {
        PlayerPrefs.DeleteKey(ChaveLiberada);
        for (int f = 1; f <= TotalFases; f++)
            PlayerPrefs.DeleteKey(ChaveEstrelas(f));
        PlayerPrefs.Save();
    }
}
