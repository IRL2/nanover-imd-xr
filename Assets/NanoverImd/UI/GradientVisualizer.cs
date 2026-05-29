using UnityEngine;

public class GradientVisualizer : MonoBehaviour
{
    [SerializeField] DnemdUIDataHandler dataHandler;
    float[] gradientData;
    Texture2D gradientTex;
    [SerializeField] Renderer targetRenderer;
    private void Awake()
    {
        gradientTex = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        gradientTex.wrapMode = TextureWrapMode.Clamp;
        gradientTex.filterMode = FilterMode.Bilinear;

        targetRenderer.material.mainTexture = gradientTex;
    }
    void OnEnable()
    {
        dataHandler.OnResidueColourGradientChanged += UpdateGradient;
    }

    void OnDisable()
    {
        dataHandler.OnResidueColourGradientChanged -= UpdateGradient;
    }

    public void UpdateGradient(float[] newGradientData)
    {
        if (newGradientData == null || newGradientData.Length < 4)
            return;

        gradientData = newGradientData;
        RebuildTexture();
    }

    void RebuildTexture()
    {
        int colorCount = gradientData.Length / 4;
        int width = gradientTex.width;

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            Color c = SampleGradient(t, colorCount);
            gradientTex.SetPixel(x, 0, c);
        }

        gradientTex.Apply();
    }

    Color SampleGradient(float t, int colorCount)
    {
        float scaled = t * (colorCount - 1);
        int i = Mathf.FloorToInt(scaled);
        int j = Mathf.Min(i + 1, colorCount - 1);
        float lerp = scaled - i;

        Color a = GetColor(i);
        Color b = GetColor(j);

        return Color.Lerp(a, b, lerp);
    }

    Color GetColor(int index)
    {
        int baseIdx = index * 4;
        return new Color(
            gradientData[baseIdx],
            gradientData[baseIdx + 1],
            gradientData[baseIdx + 2],
            gradientData[baseIdx + 3]
        );
    }
}
