using UnityEngine;

// Moedas iniciais, vida da Torre, bônus de onda, venda e os níveis do Cofrinho.
[CreateAssetMenu(menuName = Identidade.NomeDoJogo + "/Economia")]
public class ConfigEconomia : ScriptableObject
{
    public DadosEconomia dados = new DadosEconomia();
}
