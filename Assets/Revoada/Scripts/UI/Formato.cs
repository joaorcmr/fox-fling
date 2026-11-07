using System.Globalization;

// Números do jogo em português: vírgula decimal sempre, não importa a cultura
// do computador (no WebGL a cultura é a invariante, que usa ponto).
public static class Formato
{
    public static string Num(float valor, string formato = "0.##") =>
        valor.ToString(formato, CultureInfo.InvariantCulture).Replace('.', ',');

    public static string Porcento(float fracao) => (fracao * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";
}
