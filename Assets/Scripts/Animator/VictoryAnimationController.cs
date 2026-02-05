using UnityEngine;
using System.Collections;

public class VictoryAnimationController : MonoBehaviour
{
    [Header("烟花预制体")]
    public GameObject fireworksPrefab;
    
    [Header("烟花参数")]
    public int fireworkCount = 4;          // 同时绽放的烟花数量
    public float spreadRadius = 1.2f;        // 烟花分散范围
    public float delayBetweenFireworks = 0.3f; // 烟花之间的延迟
    public float destroyDelay = 4f;        // 销毁延迟时间

    [Header("屏幕震动")]
    public bool enableScreenShake = true;
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 0.5f;
    
    // 存储关卡胜利时的位置（比如玩家位置）
    private Vector3 victoryPosition;
    
    // 单例模式，方便全局调用
    private static VictoryAnimationController _instance;
    public static VictoryAnimationController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VictoryAnimationController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("VictoryAnimationManager");
                    _instance = obj.AddComponent<VictoryAnimationController>();
                }
            }
            return _instance;
        }
    }
    
 
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 确保在场景切换时不销毁（如果需要）
        // DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 外部调用的胜利动画方法
    /// </summary>
    /// <param name="position">胜利发生的位置</param>
    public void PlayVictoryAnimation(Vector3 position)
    {
        victoryPosition = position;
        StartCoroutine(PlayFireworksRoutine());
        
        if (enableScreenShake)
        {
            StartCoroutine(ScreenShake());
        }
        
        // 这里可以添加其他胜利效果，如音效等
        
    }
    
    /// <summary>
    /// 播放多个烟花
    /// </summary>
    private IEnumerator PlayFireworksRoutine()
    {
        for (int i = 0; i < fireworkCount; i++)
        {
            // 在指定位置周围随机生成烟花位置
            Vector3 randomOffset = Random.insideUnitCircle * spreadRadius;
            Vector3 spawnPosition = victoryPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // 实例化烟花
            GameObject firework = Instantiate(fireworksPrefab, spawnPosition, Quaternion.identity);
            
            // 随机颜色变化（可选）
            ParticleSystem ps = firework.GetComponent<ParticleSystem>();
            var main = ps.main;
            
            // 随机主颜色
            Color randomColor = new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f)
            );
            main.startColor = randomColor;
            
            // 延迟销毁
            Destroy(firework, destroyDelay);
            
            // 等待一段时间再播放下一个
            yield return new WaitForSeconds(delayBetweenFireworks);
        }
    }
    
    /// <summary>
    /// 屏幕震动效果
    /// </summary>
    private IEnumerator ScreenShake()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 originalPosition = cameraTransform.localPosition;
        
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            
            cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cameraTransform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// 快速播放胜利动画（使用默认位置）
    /// </summary>
    public void QuickVictory()
    {
        // 默认在屏幕中央播放
        Vector3 screenCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10));
        PlayVictoryAnimation(screenCenter);
    }
}