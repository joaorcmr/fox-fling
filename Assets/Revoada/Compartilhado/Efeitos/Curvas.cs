// Curvas de animação usadas pelas telas (t vai de 0 a 1).
public static class Curvas
{
    // "OutBack": passa um pouco de 1 e volta, como algo que cai e quica.
    public static float VoltaComQuique(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    // "OutQuad": rápida no começo e desacelerando.
    public static float Desacelera(float t) => 1f - (1f - t) * (1f - t);
}
