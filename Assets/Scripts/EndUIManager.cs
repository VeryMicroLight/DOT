using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EndUIManager : MonoBehaviour
{
    [Header("卡片螺旋效果设置")]
    public GameObject origin;           // 卡片预制体
    public float radius = 5f;            // 圆形半径
    public float spawnPosZ = -200f;      // 起始Z轴位置
    private GameObject cardContainer;    // 卡片的容器（独立物体）
    private List<GameObject> cards = new List<GameObject>(); // 存储生成的卡片

    [Header("按钮设置")]
    [SerializeField] private UnityEngine.UI.Button nextLevelBtn;
    [SerializeField] private UnityEngine.UI.Button backToStartBtn;
    [SerializeField] private GameObject winnerPanel;

    [Header("UI元素")]
    public GameObject panel;
    public GameObject wlight;
    public GameObject man;
    public GameObject thank;

    [Header("图片数组")]
    public Sprite[] vortexSprites;

    private void Awake()
    {
        panel.SetActive(false);
        wlight.SetActive(false);
        thank.SetActive(false);
        man.SetActive(false);
        backToStartBtn.interactable = false;
        
        // 创建卡片容器（不会影响UI）
        CreateCardContainer();
    }

    private void Start()
    {
        nextLevelBtn.onClick.AddListener(OnNextLevelClick);
        backToStartBtn.onClick.AddListener(OnBackToStartClick);

        panel.SetActive(false);
        wlight.SetActive(false);
        thank.SetActive(false);
        man.SetActive(false);
        backToStartBtn.interactable = false;
    }

    // 创建独立的卡片容器
    private void CreateCardContainer()
    {
        if (cardContainer == null)
        {
            cardContainer = new GameObject("CardContainer");
            // 让容器在3D空间中独立，不影响UI
            cardContainer.transform.position = Vector3.zero;
            cardContainer.transform.rotation = Quaternion.identity;
        }
    }

    // 清理之前生成的卡片
    private void ClearCards()
    {
        foreach (GameObject card in cards)
        {
            if (card != null)
                Destroy(card);
        }
        cards.Clear();
    }

    // 外部调用：关卡胜利时显示面板
    public void ShowWinnerPanel()
    {
        winnerPanel.SetActive(true);
    }

    // 点击下一关
    private void OnNextLevelClick()
    {
        // 先清理之前的卡片
        ClearCards();

        // 显示UI元素（这些不会旋转）
        panel.SetActive(true);
        wlight.SetActive(true);
        thank.SetActive(true);
        man.SetActive(true);

        // 确保卡片容器存在且位置重置
        CreateCardContainer();
        cardContainer.transform.position = Vector3.zero;
        cardContainer.transform.rotation = Quaternion.identity;

        // 创建卡片螺旋（卡片会放在cardContainer下）
        CardCreate();

        // 只移动和旋转卡片容器（不影响UI）
        MoveAnim();

        // 按钮状态控制
        nextLevelBtn.gameObject.SetActive(false);
        backToStartBtn.interactable = true;
        
        backToStartBtn.onClick.RemoveAllListeners();
        backToStartBtn.onClick.AddListener(OnBackToStartClick);

        Debug.Log($"Back按钮状态: interactable={backToStartBtn.interactable}, active={backToStartBtn.gameObject.activeSelf}");
    }

    private void CardCreate()
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject clone = Instantiate(origin);
            // ★★★ 关键：把卡片放在独立的容器下，而不是panel下 ★★★
            clone.transform.parent = cardContainer.transform;
            
            float angle = Random.Range(0, 360);
            
            // 使用局部坐标，这样容器的移动旋转会影响所有卡片
            clone.transform.localPosition = new Vector3(
                radius * Mathf.Cos(angle * Mathf.Deg2Rad),
                radius * Mathf.Sin(angle * Mathf.Deg2Rad),
                spawnPosZ - i * 5
            );
            
            clone.transform.localEulerAngles = new Vector3(
                -angle, 
                clone.transform.localEulerAngles.y, 
                clone.transform.localEulerAngles.z
            );
            
            cards.Add(clone);
        }
    }

    public void MoveAnim()
    {
        // ★★★ 只移动和旋转卡片容器，不影响panel和按钮 ★★★
        cardContainer.transform.DOMoveZ(522, 6f).SetEase(Ease.InOutQuad);
        cardContainer.transform.DORotate(new Vector3(0, 0, 1440), 6f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad);
    }

    // 点击返回开始界面
    private void OnBackToStartClick()
    {
        Debug.Log("点击返回按钮");
        
        // 清理卡片
        ClearCards();

        if (PersistentSceneManager.Instance != null)
        {
            PersistentSceneManager.Instance.LoadStartScene();
        }
        else
        {
            Debug.LogError("PersistentSceneManager.Instance 为 null！");
        }
    }
}