using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectUI : MonoBehaviour
{
    public static LevelSelectUI Instance;

    [Header("UI 配置")]
    public Transform levelButtonParent;   // 关卡按钮父物体
    public Button levelButtonPrefab;      // 关卡按钮预制体
    public List<LevelData> allLevelDatas; // 所有关卡的 LevelData 列表

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 生成所有关卡按钮
        GenerateLevelButtons();
        // 默认隐藏关卡选择UI（可选）
        HideLevelSelect();
    }

    private void GenerateLevelButtons()
    {
        foreach (var levelData in allLevelDatas)
        {
            Button btn = Instantiate(levelButtonPrefab, levelButtonParent);
            btn.name = $"Btn_Level_{levelData.levelName}";

            // 设置按钮文字和图标
            Text btnText = btn.GetComponentInChildren<Text>();
            Image btnIcon = btn.GetComponent<Image>();
            if (btnText != null) btnText.text = levelData.levelName;
          //  if (btnIcon != null && levelData.levelIcon != null) btnIcon.sprite = levelData.levelIcon;

            // 绑定按钮点击事件
            LevelData data = levelData;
            btn.onClick.AddListener(() => OnLevelButtonClicked(data));

            // 锁定状态处理（可选）
            btn.interactable = levelData.isUnlocked;
        }
    }

    private void OnLevelButtonClicked(LevelData levelData)
    {
        // 调用全局管理器加载关卡
        PersistentSceneManager.Instance.LoadLevel(levelData);
    }

    public void ShowLevelSelect()
    {
        gameObject.SetActive(true);
    }

    public void HideLevelSelect()
    {
        gameObject.SetActive(false);
    }
}