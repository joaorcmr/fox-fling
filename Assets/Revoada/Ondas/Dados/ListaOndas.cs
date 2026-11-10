using System.Collections.Generic;
using UnityEngine;

// As 30 ondas: cada uma é uma lista de grupos (tipo de pássaro, quantidade,
// intervalo, atraso). Mexer aqui muda a dificuldade sem tocar no código.
[CreateAssetMenu(menuName = "Revoada/Lista de ondas")]
public class ListaOndas : ScriptableObject
{
    public List<DadosOnda> ondas = new List<DadosOnda>();
}
