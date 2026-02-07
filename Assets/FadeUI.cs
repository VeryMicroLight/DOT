using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    public static FadeUI Instance;

    [Header("渐变")]
    public Image fadeImage; // 全屏黑色遮罩Image
    public float defaultFadeDuration = 0.5f; // 默认渐变时长

    private void Awake()
    {
        // 单例初始化，确保全局唯一且常驻内存
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景不销毁
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化遮罩状态：透明、全屏
        if (fadeImage != null)
        {
            Color initColor = fadeImage.color;
            initColor.a = 0; // 初始完全透明
            fadeImage.color = initColor;
            fadeImage.rectTransform.anchorMin = Vector2.zero;
            fadeImage.rectTransform.anchorMax = Vector2.one;
            fadeImage.rectTransform.sizeDelta = Vector2.zero;
        }
    }

    /// <summary>
    /// 渐黑（淡出）：从透明→全黑
    /// </summary>
    //渐变时长，不传则用默认值
    public Coroutine FadeOut(float duration = -1)
    {
        if (fadeImage == null) return null;
        float useDuration = duration > 0 ? duration : defaultFadeDuration;
        return StartCoroutine(FadeOutCoroutine(useDuration));
    }

    /// <summary>
    /// 渐显（淡入）：从全黑→透明
    /// </summary>
    //渐变时长
    public Coroutine FadeIn(float duration = -1)
    {
        if (fadeImage == null) return null;
        float useDuration = duration > 0 ? duration : defaultFadeDuration;
        return StartCoroutine(FadeInCoroutine(useDuration));
    }

    // 渐黑协程
    private IEnumerator FadeOutCoroutine(float duration)
    {
        Color targetColor = fadeImage.color;
        targetColor.a = 1; // 目标：全黑
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0, 1, t); // 缓入缓出，动画更自然
            fadeImage.color = Color.Lerp(fadeImage.color, targetColor, t);
            yield return null;
        }
        fadeImage.color = targetColor; // 强制到位，避免偏差
    }

    // 渐显协程
    private IEnumerator FadeInCoroutine(float duration)
    {
        Color targetColor = fadeImage.color;
        targetColor.a = 0; // 透明
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0, 1, t);
            fadeImage.color = Color.Lerp(fadeImage.color, targetColor, t);
            yield return null;
        }
        fadeImage.color = targetColor;
    }
}