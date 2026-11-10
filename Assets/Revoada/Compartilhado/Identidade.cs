// O nome do jogo num lugar só: o título da janela, o menu do Editor, o tamanho
// da Game view e os relatórios saem daqui. O logo é imagem (Tools/gerar_arte.py)
// e precisa ser gerado de novo se o nome mudar.
//
// Por dentro, a pasta Assets/Revoada, os assemblies Revoada.* e o
// ConstrutorRevoada guardam o nome antigo do projeto: são nomes de código, que
// o jogador nunca vê, e renomeá-los mexeria em todos os caminhos e referências.
public static class Identidade
{
    public const string NomeDoJogo = "Fox Tower";
}
