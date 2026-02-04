using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectUI : MonoBehaviour
{
    public static LevelSelectUI Instance;

    [Header("关卡按钮与对应数据")]
    // 手动绑定按钮和关卡
    public List<LevelButtonBinding> levelButtonBindings = new List<LevelButtonBinding>();

    // 记录已加载过的关卡索引
    private HashSet<int> loadedLevelIndexes = new HashSet<int>();

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
        PlayerPrefs.DeleteKey("LoadedLevelIndexes"); // 临时清空关卡记录，运行一次后删掉
        PlayerPrefs.Save();
        // 从持久化数据加载已解锁关卡
        LoadLoadedLevelRecords();
        // 绑定按钮点击事件 + 刷新按钮状态
        InitLevelButtons();
        // 默认隐藏关卡选择UI
        HideLevelSelect();
    }

    private void InitLevelButtons()
    {
        foreach (var binding in levelButtonBindings)
        {
            // 跳过无效绑定（按钮或关卡数据为空）
            if (binding.levelButton == null || binding.levelData == null)
            {
                Debug.LogWarning("存在无效的关卡按钮绑定");
                continue;
            }

            // 绑定按钮点击事件
            LevelData targetData = binding.levelData;
            binding.levelButton.onClick.AddListener(() => OnLevelButtonClicked(targetData));

            // 初始化按钮状态
            RefreshSingleButtonStatus(binding);
        }
    }

    /// <summary>
    /// 刷新单个按钮的解锁/锁定状态
    /// </summary>
    private void RefreshSingleButtonStatus(LevelButtonBinding binding)
    {
        if (binding.levelButton == null || binding.levelData == null) return;

        // 核心逻辑：已加载过在loadedLevelIndexes中则解锁，否则锁定
        bool isUnlocked = loadedLevelIndexes.Contains(binding.levelData.levelIndex);
        binding.levelButton.interactable = isUnlocked;

    }


    //刷新所有按钮的状态（关卡加载成功后调用）
    public void RefreshAllLevelButtonsStatus()
    {
        foreach (var binding in levelButtonBindings)
        {
            RefreshSingleButtonStatus(binding);
        }
    }


    // 按钮点击回调：加载对应关卡
    private void OnLevelButtonClicked(LevelData levelData)
    {
        if (levelData == null) return;
        // 调用全局管理器加载关卡
        PersistentSceneManager.Instance.LoadLevel(levelData);
    }

    //加载持久化的已加载关卡记录（PlayerPrefs，重启游戏不丢失）
    private void LoadLoadedLevelRecords()
    {
        loadedLevelIndexes.Clear();
        // 读取保存的已加载关卡字符串
        string loadedLevelsStr = PlayerPrefs.GetString("LoadedLevelIndexes", "");
        if (!string.IsNullOrEmpty(loadedLevelsStr))
        {
            string[] indexArr = loadedLevelsStr.Split(',');
            foreach (string indexStr in indexArr)
            {
                if (int.TryParse(indexStr, out int levelIndex))
                {
                    loadedLevelIndexes.Add(levelIndex);
                }
            }
        }

        //// 默认解锁第1关（避免玩家无关卡可玩）
        //if (loadedLevelIndexes.Count == 0 && levelButtonBindings.Count > 0)
        //{
        //    int firstLevelIndex = levelButtonBindings[0].levelData.levelIndex;
        //    AddLoadedLevelRecord(firstLevelIndex);
        //}
    }

    /// <summary>
    /// 添加已加载关卡记录并持久化
    /// </summary>
    public void AddLoadedLevelRecord(int levelIndex)
    {
        if (loadedLevelIndexes.Contains(levelIndex)) return;

        // 添加到内存集合
        loadedLevelIndexes.Add(levelIndex);

        // 持久化到PlayerPrefs
        string loadedLevelsStr = string.Join(",", loadedLevelIndexes);
        PlayerPrefs.SetString("LoadedLevelIndexes", loadedLevelsStr);
        PlayerPrefs.Save();

        // 刷新按钮状态
        RefreshAllLevelButtonsStatus();
    }

    public void ShowLevelSelect()
    {
        gameObject.SetActive(true);
        // 显示时刷新一次按钮状态，确保数据同步
        RefreshAllLevelButtonsStatus();
    }

    public void HideLevelSelect()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 按钮-关卡数据绑定类
    /// </summary>
    [System.Serializable]
    public class LevelButtonBinding
    {
        public Button levelButton; // 手动创建的关卡按钮
        public LevelData levelData; // 按钮对应的关卡数据
    }
}