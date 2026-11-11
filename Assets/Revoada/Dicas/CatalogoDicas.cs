// As dicas do jogo: o id (que o Progresso usa para lembrar quais já
// apareceram) e o texto. Quando cada uma aparece é com o componente Dicas.
public static class CatalogoDicas
{
    public const string Inicio = "inicio";
    public const string Arpao = "arpao";
    public const string Cofrinho = "cofrinho";
    public const string Couraca = "couraca";
    public const string Tambor = "tambor";

    public static readonly string[] Ids = { Inicio, Arpao, Cofrinho, Couraca, Tambor };

    public static string Texto(string id)
    {
        switch (id)
        {
            case Inicio: return "Clique numa raposa da loja e depois no mapa para colocá-la. Depois, solte a onda.";
            case Arpao: return "O arpão atravessa os pássaros em fila: no fim de uma reta ele rende mais.";
            case Cofrinho: return "O Cofrinho faz cada inimigo render mais. Vale a pena agora?";
            case Couraca: return "Couraça: pedrinha e arpão só tiram metade. Bomba e Ponta de Ferro tiram tudo.";
            case Tambor: return "Raposas dentro do círculo do Tamboreiro atiram mais rápido. Agrupe!";
            default: return "";
        }
    }
}
