// Em que pé está a partida. Usar enums (e não textos) deixa o compilador avisar
// se alguém escrever um estado que não existe.
public enum EstadoJogo { Preparando, EmOnda, Intervalo, Vitoria, Derrota }

// Resposta dos comandos do jogador (comprar, melhorar...), para a interface
// saber o que mostrar quando algo não deu certo.
public enum Resultado { Ok, SemPenas, LugarInvalido, Indisponivel }
