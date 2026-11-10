using System.Collections.Generic;

// Os NÚMEROS do jogo ficam separados da lógica, cada bloco na pasta do assunto
// (DadosPassaro, DadosPorco, DadosOnda, DadosEconomia, DadosMapa). São classes
// [Serializable] com campos públicos para aparecer no Inspector dentro dos
// ScriptableObjects: balancear o jogo é editar esses valores, sem mexer em
// código. A simulação só lê; quem cria os valores padrão é o construtor de
// Editor (DefinicoesPadrao).

// O pacote que a simulação recebe. Não é [Serializable]: quem monta é o
// ConversorDados (a partir dos assets) ou os testes (à mão).
public class DadosJogo
{
    public Dictionary<EspeciePassaro, DadosPassaro> passaros = new Dictionary<EspeciePassaro, DadosPassaro>();
    public Dictionary<EspeciePorco, DadosPorco> porcos = new Dictionary<EspeciePorco, DadosPorco>();
    public List<DadosOnda> ondas = new List<DadosOnda>();
    public DadosEconomia economia = new DadosEconomia();
    public DadosMapa mapa = new DadosMapa();

    // As fases (ver ListaFases): cada uma troca o mapa ativo a cada
    // TotalOndas/fases.Count ondas. Vazio ou com 1 mapa só = sem fases, sempre
    // o mesmo mapa (compatível com os testes, que não preenchem isto).
    public List<DadosMapa> fases = new List<DadosMapa>();
}
