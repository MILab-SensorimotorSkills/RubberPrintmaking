using UnityEngine;
public class EdgeCompletionChecker : MonoBehaviour
{
    public Texture2D actualTexture;  // 실제 도안 이미지 (Design3.png)
    // RGB 값 상수
    private Color dogColor = new Color(108f / 255f, 108f / 255f, 108f / 255f, 1f);
    private Color carvedColor = new Color(75f / 255f, 75f / 255f, 75f / 255f, 1f);
    void Start()
    {
        Color[] actualPixels = actualTexture.GetPixels();
        int width = actualTexture.width;
        int height = actualTexture.height;
        int totalDogPixels = 0;
        int carvedPixels = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color actualPixel = actualPixels[x + y * width];
                // 강아지 도안의 색상과 일치하는 픽셀을 찾기
                if (ColorsAreSimilar(actualPixel, dogColor))
                {
                    totalDogPixels++;
                    // 해당 픽셀이 깎인 부분의 색상과 일치하는지 확인
                    if (ColorsAreSimilar(actualPixel, carvedColor))
                    {
                        carvedPixels++;
                    }
                }
            }
        }
        // 깎인 비율 계산
        float erosionPercentage = (totalDogPixels > 0) ? (float)carvedPixels / totalDogPixels * 100f : 0f;
        Debug.Log("Erosion Percentage: " + erosionPercentage + "%");
    }
    bool ColorsAreSimilar(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}