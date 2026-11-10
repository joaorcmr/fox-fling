// Onde o jogo guarda o que lembra entre partidas: PlayerPrefs (no WebGL, o
// PlayerPrefs fica no armazenamento do navegador). O prefixo existe para os
// testes usarem chaves próprias e não apagarem o que fica salvo para quem joga.
public static class Salvamento
{
    public static string Prefixo = "revoada_";

    public static string Chave(string nome) => Prefixo + nome;
}
