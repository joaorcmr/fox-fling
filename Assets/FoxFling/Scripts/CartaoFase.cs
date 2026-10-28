using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Um cartão do menu: número da fase, dificuldade, estrelas ganhas e cadeado.
public class CartaoFase : MonoBehaviour
{
    public int numero = 1;
    public Button botao;
    public Image[] estrelas;
    public Sprite estrelaCheia, estrelaVazia;
    public GameObject cadeado;
    public CanvasGroup conteudo;

    void Start()
    {
        botao.onClick.AddListener(() =>
        {
            Sons.Tocar(Sons.I != null ? Sons.I.clique : null);
            SceneManager.LoadScene("Fase" + numero);
        });
    }

    public void Atualizar()
    {
        bool liberada = numero <= Progresso.FaseMaisAltaLiberada;
        botao.interactable = liberada;
        cadeado.SetActive(!liberada);
        conteudo.alpha = liberada ? 1f : 0.45f;

        int ganhas = Progresso.MelhoresEstrelas(numero);
        for (int i = 0; i < estrelas.Length; i++)
            estrelas[i].sprite = i < ganhas ? estrelaCheia : estrelaVazia;
    }
}
