using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Cria e lê os assets de dados, cada um na pasta Dados do seu assunto
// (Passaros, Porcos, Ondas, Moedas, Mapa). Com sobrescrever =
// false, um asset que já existe não é tocado: os números que o João ajustou no
// Inspector ficam. Os sprites são sempre religados (vêm da pasta de arte).
public static class ConstrutorDados
{
    const string Raiz = ConstrutorAssets.Raiz;
    const string PastaPassaros = Raiz + "/Passaros/Dados";
    const string PastaPorcos = Raiz + "/Porcos/Dados";
    const string CaminhoOndas = Raiz + "/Ondas/Dados/Ondas.asset";
    const string CaminhoEconomia = Raiz + "/Moedas/Dados/Economia.asset";
    const string CaminhoFazenda = Raiz + "/Mapa/Dados/Fazenda.asset";
    const string CaminhoPomar = Raiz + "/Mapa/Dados/Pomar.asset";
    const string CaminhoFloresta = Raiz + "/Mapa/Dados/Floresta.asset";
    const string CaminhoFases = Raiz + "/Mapa/Dados/Fases.asset";

    // Sprite e cor das penas de cada pássaro.
    static readonly Dictionary<EspeciePassaro, (string sprite, string cor)> VisualPassaro = new Dictionary<EspeciePassaro, (string, string)>
    {
        { EspeciePassaro.Pardal, ("pardal", "#B98352") },
        { EspeciePassaro.Sabia, ("sabia", "#E07B37") },
        { EspeciePassaro.BeijaFlor, ("beija_flor", "#3FBF7A") },
        { EspeciePassaro.Arara, ("arara", "#3E8FE0") },
        { EspeciePassaro.Pata, ("pata", "#F4F1EA") },
        { EspeciePassaro.Patinho, ("patinho", "#F7D84A") },
        { EspeciePassaro.Urubu, ("urubu", "#3A3A40") },
        { EspeciePassaro.Gaviao, ("gaviao", "#9AA1A8") },
    };

    // Sprites de cada porco: corpo, arma, tiro e os acessórios das especializações.
    static readonly Dictionary<EspeciePorco, string[]> VisualPorco = new Dictionary<EspeciePorco, string[]>
    {
        { EspeciePorco.Estilingueiro, new[] { "raposa_estilingueiro", "arma_estilingue", "pedrinha", "acc_rajada", "acc_mira" } },
        { EspeciePorco.Arpoador, new[] { "raposa_arpoador", "arma_arpao", "arpao", "acc_gigante", "acc_ligeiro" } },
        { EspeciePorco.Bombardeiro, new[] { "raposa_bombardeiro", "arma_canhao", "bomba", "acc_chuva", "acc_estrume" } },
        { EspeciePorco.Tamboreiro, new[] { "raposa_tamboreiro", "arma_tambor", null, "acc_fanfarra", "acc_moedas" } },
    };

    public static void GarantirDados(bool sobrescrever)
    {
        foreach (var pasta in new[] { PastaPassaros, PastaPorcos, Pasta(CaminhoOndas), Pasta(CaminhoEconomia), Pasta(CaminhoFazenda) })
            ConstrutorAssets.GarantirPasta(pasta);

        foreach (var padrao in DefinicoesPadrao.Passaros())
        {
            var tipo = Unico<TipoPassaro>($"{PastaPassaros}/{padrao.especie}.asset", t => t.dados = padrao, sobrescrever);
            var (sprite, cor) = VisualPassaro[padrao.especie];
            tipo.sprite = ConstrutorAssets.Sprite(sprite);
            ColorUtility.TryParseHtmlString(cor, out tipo.corPenas);
            EditorUtility.SetDirty(tipo);
        }

        foreach (var padrao in DefinicoesPadrao.Porcos())
        {
            var tipo = Unico<TipoPorco>($"{PastaPorcos}/{padrao.especie}.asset", t => t.dados = padrao, sobrescrever);
            var s = VisualPorco[padrao.especie];
            tipo.corpo = ConstrutorAssets.Sprite(s[0]);
            // A arte do corpo (raposa) já vem com a arma desenhada nela: sem
            // sprite de arma avulso, senão apareceria duas vezes.
            tipo.arma = null;
            tipo.projetil = s[2] != null ? ConstrutorAssets.Sprite(s[2]) : null;
            tipo.acessorioA = ConstrutorAssets.Sprite(s[3]);
            tipo.acessorioB = ConstrutorAssets.Sprite(s[4]);
            EditorUtility.SetDirty(tipo);
        }

        Unico<ListaOndas>(CaminhoOndas, a => a.ondas = DefinicoesPadrao.Ondas(), sobrescrever);
        Unico<ConfigEconomia>(CaminhoEconomia, a => a.dados = DefinicoesPadrao.Economia(), sobrescrever);
        var fazenda = Unico<Mapa>(CaminhoFazenda, a => a.dados = DefinicoesPadrao.Fazenda(), sobrescrever);
        var pomar = Unico<Mapa>(CaminhoPomar, a => a.dados = DefinicoesPadrao.Pomar(), sobrescrever);
        var floresta = Unico<Mapa>(CaminhoFloresta, a => a.dados = DefinicoesPadrao.Floresta(), sobrescrever);
        Unico<ListaFases>(CaminhoFases, a => a.mapas = new List<Mapa> { fazenda, pomar, floresta }, sobrescrever);
        AssetDatabase.SaveAssets();
    }

    // Carrega o asset; se não existe, cria com os números padrão. Com
    // "sobrescrever", troca os números de um asset existente pelos padrão.
    static T Unico<T>(string caminho, Action<T> preencher, bool sobrescrever) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(caminho);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            preencher(asset);
            AssetDatabase.CreateAsset(asset, caminho);
        }
        else if (sobrescrever)
        {
            preencher(asset);
            EditorUtility.SetDirty(asset);
        }
        return asset;
    }

    public static TipoPassaro[] Passaros() => Todos<TipoPassaro>(PastaPassaros).OrderBy(t => t.dados.especie).ToArray();
    public static TipoPorco[] Porcos() => Todos<TipoPorco>(PastaPorcos).OrderBy(t => t.dados.especie).ToArray();
    public static ListaOndas Ondas() => AssetDatabase.LoadAssetAtPath<ListaOndas>(CaminhoOndas);
    public static ConfigEconomia Economia() => AssetDatabase.LoadAssetAtPath<ConfigEconomia>(CaminhoEconomia);
    public static Mapa Fazenda() => AssetDatabase.LoadAssetAtPath<Mapa>(CaminhoFazenda);
    public static Mapa Pomar() => AssetDatabase.LoadAssetAtPath<Mapa>(CaminhoPomar);
    public static Mapa Floresta() => AssetDatabase.LoadAssetAtPath<Mapa>(CaminhoFloresta);
    public static ListaFases Fases() => AssetDatabase.LoadAssetAtPath<ListaFases>(CaminhoFases);

    static string Pasta(string caminhoAsset) => caminhoAsset.Substring(0, caminhoAsset.LastIndexOf('/'));

    public static DadosJogo Carregar() => ConversorDados.Montar(Passaros(), Porcos(), Ondas(), Economia(), Fazenda(), Fases());

    static IEnumerable<T> Todos<T>(string pasta) where T : UnityEngine.Object
    {
        if (!AssetDatabase.IsValidFolder(pasta)) yield break;
        foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { pasta }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) yield return asset;
        }
    }
}
