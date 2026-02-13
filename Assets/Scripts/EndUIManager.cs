using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EndUIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button nextLevelBtn; // 下一关按钮
    [SerializeField] private UnityEngine.UI.Button backToStartBtn; // 返回开始界面按钮
    [SerializeField] private GameObject winnerPanel; // 胜利面板（默认隐藏）

public GameObject panel;
public GameObject wlight;

public GameObject man;


public Sprite[] vortexSprites;  // 在Inspector拖拽图片到这里


  

    private void Awake()
    {
       
        nextLevelBtn.onClick.AddListener(OnNextLevelClick);
        backToStartBtn.onClick.AddListener(OnBackToStartClick);
        panel.SetActive(false); // 初始隐藏胜利面板
        wlight.SetActive(false);
        man.SetActive(false);

    }
private void Start()
    {
        // 在Start中再次确认
       
            
            panel.SetActive(false);
            wlight.SetActive(false);
            man.SetActive(false);
        
    }
    // 外部调用：关卡胜利时显示面板（如玩家触达终点、消灭所有敌人时调用）
    public void ShowWinnerPanel()
    {
        winnerPanel.SetActive(true);
      
    }

    // 点击下一关
 
private void OnNextLevelClick()
{
    StartCoroutine(VortexEffect());
}

private IEnumerator VortexEffect()
{
    panel.SetActive(true);
    wlight.SetActive(true);
    man.SetActive(true);
    // 显示EndCanvas
    Canvas canvas = GameObject.Find("EndCanvas").GetComponent<Canvas>();
    canvas.enabled = true;
    
    // 检查是否有拖拽图片
    if (vortexSprites == null || vortexSprites.Length == 0)
    {
        Debug.LogError("请先在Inspector中拖拽图片到vortexSprites数组！");
        yield break;
    }
    
    Vector2 center = Vector2.zero;
    float radius = 400f;
    
    // 生成25个粒子
    for (int i = 0; i < 25; i++)
    {
        yield return new WaitForSeconds(0.03f);
        
        GameObject particle = new GameObject($"Vortex_{i}");
        particle.transform.SetParent(canvas.transform, false);
        
        Image img = particle.AddComponent<Image>();
        
        // 从拖拽的图片数组中随机选择一张
        img.sprite = vortexSprites[Random.Range(0, vortexSprites.Length)];
        
        // 保持图片原始颜色，或者添加随机色调
        img.color = Color.white;  // 使用图片原始颜色
        // img.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f); // 如果想要随机颜色
        
        RectTransform rect = particle.GetComponent<RectTransform>();
        
        // 保持图片原始宽高比
        // if (img.sprite != null)
        // {
        //     float originalWidth = img.sprite.rect.width;
        //     float originalHeight = img.sprite.rect.height;
        //     float scaleFactor = Random.Range(0.5f, 1.5f);
        //     rect.sizeDelta = new Vector2(originalWidth * scaleFactor, originalHeight * scaleFactor);
        // }
        // else
        // {
            rect.sizeDelta = new Vector2(150, 250);  // 默认大小
        //}
        
        // 圆形边界随机位置
        float angle = Random.Range(0, 360);
        rect.anchoredPosition = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
            Mathf.Sin(angle * Mathf.Deg2Rad) * radius
        );
        
        // 随机旋转
        rect.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        
        // 螺旋动画
        rect.DOAnchorPos(center, 2f).SetEase(Ease.InSine);
        rect.DORotate(new Vector3(0, 0, Random.Range(720, 1440)), 5f, RotateMode.FastBeyond360);
        img.DOColor(Color.clear, 1.8f);
        rect.DOScale(0, 1.5f).SetEase(Ease.InBack).OnComplete(() => Destroy(particle));
    }
}
    // 点击返回开始界面
    private void OnBackToStartClick()
    {
        PersistentSceneManager.Instance.LoadStartScene();
    }

    
}
