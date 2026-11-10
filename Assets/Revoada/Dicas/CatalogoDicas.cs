// As dicas do jogo: o id (que o Progresso usa para lembrar quais já
// apareceram) e o texto. Quando cada uma aparece é com o componente Dicas.
public static class CatalogoDicas
{
    public const string Inicio = "inicio";
    public const string Arpao = "arpao";
    public const string Ninho = "ninho";
    public const string Couraca = "couraca";
    public const string Tambor = "tambor";

    public static readonly string[] Ids = { Inicio, Arpao, Ninho, Couraca, Tambor };

    public static string Texto(string id)
    {
        switch (id)
        {
            case Inicio: return "Clique num porco da loja e depois no mapa para colocá-lo. Depois, solte a onda.";
            case Arpao: return "O arpão atravessa os pássaros em fila: no fim de uma reta ele rende mais.";
            case Ninho: return "O Ninho de Penas faz cada pássaro render mais. Vale a pena agora?";
            case Couraca: return "Couraça: pedrinha e arpão só tiram metade. Bomba e Ponta de Ferro tiram tudo.";
            case Tambor: return "Porcos dentro do círculo do Tamboreiro atiram mais rápido. Agrupe!";
            default: return "";
        }
    }
}
