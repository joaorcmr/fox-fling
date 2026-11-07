// Os "nomes" do jogo. Usar enums (e não textos) deixa o compilador avisar se
// alguém escrever um pássaro ou um porco que não existe.

public enum EspeciePassaro { Pardal, Sabia, BeijaFlor, Arara, Pata, Patinho, Urubu, Gaviao }

public enum EspeciePorco { Estilingueiro, Arpoador, Bombardeiro, Tamboreiro }

// Nenhum = porco que não atira (o Tamboreiro só fortalece os vizinhos).
public enum TipoTiro { Nenhum, Pedrinha, Arpao, Bomba }

// Em quem o porco mira quando há vários pássaros no alcance.
public enum ModoAlvo { Primeiro, Ultimo, MaisForte, MaisPerto }

public enum EstadoJogo { Preparando, EmOnda, Intervalo, Vitoria, Derrota }

// Resposta dos comandos do jogador (comprar, melhorar...), para a interface
// saber o que mostrar quando algo não deu certo.
public enum Resultado { Ok, SemPenas, LugarInvalido, Indisponivel }
