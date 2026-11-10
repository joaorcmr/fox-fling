using NUnit.Framework;
using UnityEngine;

// No navegador (WebGL) o próprio navegador decodifica o áudio, sem esperar: no
// Awake os clipes ainda não estão prontos, e um Play() ali não toca nada. A
// música e o ambiente só começam quando o clipe terminou de carregar, uma vez.
public class SonsTests
{
    [Test]
    public void Loop_EsperaOClipeCarregar()
    {
        Assert.IsFalse(Sons.PodeComecarLoop(false, true, AudioDataLoadState.Unloaded));
        Assert.IsFalse(Sons.PodeComecarLoop(false, true, AudioDataLoadState.Loading));
        Assert.IsTrue(Sons.PodeComecarLoop(false, true, AudioDataLoadState.Loaded));
    }

    [Test]
    public void Loop_ComecaUmaVezSo_ESoComClipe()
    {
        Assert.IsFalse(Sons.PodeComecarLoop(true, true, AudioDataLoadState.Loaded)); // já tocando: não recomeça
        Assert.IsFalse(Sons.PodeComecarLoop(false, false, AudioDataLoadState.Loaded)); // sem clipe no campo
        Assert.IsFalse(Sons.PodeComecarLoop(false, true, AudioDataLoadState.Failed));
    }

    // Uma voz sozinha toca no volume cheio (uma revoada pequena não muda nada);
    // muitas vozes ao mesmo tempo vão cedendo espaço, pra soma não virar chiado.
    [Test]
    public void FatorPolifonia_UmaVozTocaCheio_MuitasVozesCedemEspaco()
    {
        Assert.AreEqual(1f, Sons.FatorPolifonia(0));
        Assert.AreEqual(1f, Sons.FatorPolifonia(1));

        float com8 = Sons.FatorPolifonia(8);
        float com16 = Sons.FatorPolifonia(16);
        Assert.Less(com8, 1f, "muitas vozes juntas precisam ceder espaço");
        Assert.Less(com16, com8, "quanto mais vozes, mais cada uma cede");
        Assert.Greater(com16, 0f, "nunca pode silenciar de vez");
    }
}
