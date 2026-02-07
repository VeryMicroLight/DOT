using UnityEngine;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    // 单例实例
    public static FadeUI Instance;
    // 动画控制器
    private Animator anim;
    // 标记是否正在执行淡入/淡出动画
    private bool isFading = false;
    // 动画完成回调
    private System.Action onFadeComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        anim = GetComponent<Animator>();
    }


    // 淡出动画

    public IEnumerator FadeIn()
    {
        // 防止重复触发
        if (isFading || anim == null) yield break;

        isFading = true;
        onFadeComplete = null;

        anim.SetBool("FadeOut", true);
        anim.SetBool("FadeIn", false);
        // 等待动画事件回调
        yield return new WaitUntil(() => !isFading);
    }


    // 淡入动画
    public IEnumerator FadeOut()
    {
        if (isFading || anim == null) yield break;

        isFading = true;
        onFadeComplete = null;

        anim.SetBool("FadeIn", true);
        anim.SetBool("FadeOut", false);
        yield return new WaitUntil(() => !isFading);
    }


    // 动画事件回调
    public void OnFadeAnimationEnd()
    {
        isFading = false;
        onFadeComplete?.Invoke();
    }

    // 外部判断是否正在淡入淡出
    public bool IsFading => isFading;
}