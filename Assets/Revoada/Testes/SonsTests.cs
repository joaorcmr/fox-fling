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
}
