using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Toda troca de cena passa por um escurecer e clarear de 0,3 s: um corte seco
// entre telas parece defeito. O véu é um Canvas que sobrevive à troca de cena
// (DontDestroyOnLoad) e anima em tempo sem escala (funciona com o jogo pausado).
public class Transicao : MonoBehaviour
{
    const float Duracao = 0.3f;
    static Transicao instancia;

    Image veu;
    bool trocando;

    public static void Carregar(string cena)
    {
        Garantir();
        if (!instancia.trocando) instancia.StartCoroutine(instancia.Trocar(cena));
    }

    static void Garantir()
    {
        if (instancia != null) return;
        var go = new GameObject("Transicao");
        DontDestroyOnLoad(go);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var img = new GameObject("Veu", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(go.transform, false);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        img.color = new Color(0.17f, 0.11f, 0.08f, 0f); // o marrom mais escuro da paleta, não preto puro
        img.raycastTarget = false;
        instancia = go.AddComponent<Transicao>();
        instancia.veu = img;
    }

    IEnumerator Trocar(string cena)
    {
        trocando = true;
        veu.raycastTarget = true; // ninguém clica em nada durante a troca
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(cena);
        yield return null;
        yield return Fade(1f, 0f);
        veu.raycastTarget = false;
        trocando = false;
    }

    IEnumerator Fade(float de, float para)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duracao)
        {
            var c = veu.color;
            veu.color = new Color(c.r, c.g, c.b, Mathf.Lerp(de, para, t));
            yield return null;
        }
        var f = veu.color;
        veu.color = new Color(f.r, f.g, f.b, para);
    }
}
