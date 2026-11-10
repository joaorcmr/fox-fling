using System.Collections.Generic;
using UnityEngine;

// Pool de objetos: em vez de criar e destruir um GameObject para cada pássaro
// ou tiro (o que gera lixo e engasgos no WebGL), desativa e reaproveita.
public class Pool<T> where T : Component
{
    readonly T prefab;
    readonly Transform pai;
    readonly Stack<T> livres = new Stack<T>();

    public Pool(T prefab, Transform pai)
    {
        this.prefab = prefab;
        this.pai = pai;
    }

    public T Pegar()
    {
        var obj = livres.Count > 0 ? livres.Pop() : Object.Instantiate(prefab, pai);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Devolver(T obj)
    {
        obj.gameObject.SetActive(false);
        livres.Push(obj);
    }
}
