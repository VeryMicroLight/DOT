using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public static LevelSelectUI Instance;

    [Header("关卡按钮与对应数据")]
    public List<LevelButtonBinding> levelButtonBindings = new List<LevelButtonBinding>();

    [Header("图片悬停配置")]
    [Tooltip("鼠标悬停图片上升高度")]
    public float hoverRiseHeight = 20f;
    [Tooltip("悬停升降动画时长")]
    public float tweenDuration = 0.2f;

    //记录已加载/解锁的关卡索引
    private HashSet<int> loadedLevelIndexes = new HashSet<int>();
    //缓存图片初始位置（用于悬停升降归位）
    private Dictionary<Button, Vector2> buttonOriginalPos = new Dictionary<Button, Vector2>();

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
        // 临时清空
        //PlayerPrefs.DeleteKey("LoadedLevelIndexes"); 
        //PlayerPrefs.Save();

        // 加载持久化的解锁记录
        LoadLoadedLevelRecords();
        // 初始化：绑定事件+刷新按钮/图片/底座状态（新增了图片和底座逻辑）
        InitLevelButtons();
        // 默认隐藏
        HideLevelSelect();
    }

    private void InitLevelButtons()
    {
        foreach (var binding in levelButtonBindings)
        {
            if (binding.levelButton == null || binding.levelData == null ||
                binding.buttonImage == null || binding.buttonRect == null ||
                binding.levelCompletedImage == null || binding.levelUncompletedImage == null ||
                binding.levelBaseTransform == null)
            {
                Debug.LogWarning("存在无效的关卡按钮绑定，请检查所有字段！");
                continue;
            }

            // 缓存每个图片按钮的初始锚点位置
            if (!buttonOriginalPos.ContainsKey(binding.levelButton))
            {
                buttonOriginalPos.Add(binding.levelButton, binding.buttonRect.anchoredPosition);
            }

            // 点击事件绑定
            LevelData targetData = binding.levelData;
            binding.levelButton.onClick.AddListener(() => OnLevelButtonClicked(targetData));

            // 绑定鼠标悬停/离开事件（实现图片升降）
            AddHoverEvents(binding);

            // 刷新单个关卡的：图片显示/隐藏、通关图/黑图、底座显隐
            RefreshSingleButtonStatus(binding);
        }
    }

    #region 鼠标悬停升降效果
    private void AddHoverEvents(LevelButtonBinding binding)
    {
        EventTrigger trigger = binding.levelButton.GetComponent<EventTrigger>();
        if (trigger == null) trigger = binding.levelButton.gameObject.AddComponent<EventTrigger>();

        // 鼠标进入
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { StartCoroutine(MoveButtonUp(binding)); });
        trigger.triggers.Add(enterEntry);

        // 鼠标离开
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { StartCoroutine(MoveButtonDown(binding)); });
        trigger.triggers.Add(exitEntry);
    }

    // 图片向上缓动升起
    private IEnumerator MoveButtonUp(LevelButtonBinding binding)
    {
        Vector2 startPos = binding.buttonRect.anchoredPosition;
        Vector2 targetPos = buttonOriginalPos[binding.levelButton] + new Vector2(0, hoverRiseHeight);
        float elapsedTime = 0f;

        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / tweenDuration);
            t = Mathf.SmoothStep(0, 1, t); // 缓入缓出，动画更自然
            binding.buttonRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        binding.buttonRect.anchoredPosition = targetPos; // 强制归位，避免偏差
    }

    // 图片向下缓动归位
    private IEnumerator MoveButtonDown(LevelButtonBinding binding)
    {
        Vector2 startPos = binding.buttonRect.anchoredPosition;
        Vector2 targetPos = buttonOriginalPos[binding.levelButton];
        float elapsedTime = 0f;

        while (elapsedTime < tweenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / tweenDuration);
            t = Mathf.SmoothStep(0, 1, t);
            binding.buttonRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        binding.buttonRect.anchoredPosition = targetPos;
    }
    #endregion

    #region 刷新状态
    /// <summary>
    /// 刷新单个按钮：已解锁显示、未解锁隐藏，底座同步显隐
    /// </summary>
    private void RefreshSingleButtonStatus(LevelButtonBinding binding)
    {
        if (binding.levelButton == null || binding.levelData == null) return;

        int levelIndex = binding.levelData.levelIndex;
        // 判断是否已解锁
        bool isUnlocked = loadedLevelIndexes.Contains(levelIndex);
        // 获取已解锁关卡的最大索引
        int maxUnlockedIndex = GetMaxUnlockedLevelIndex();

        // 状态赋值
        binding.levelButton.interactable = isUnlocked; 
        binding.levelButton.gameObject.SetActive(isUnlocked); // 已解锁显示，未解锁隐藏
        binding.levelBaseTransform.gameObject.SetActive(isUnlocked); // 底座和图片同步显隐

        // 仅已解锁时，切换通关图/黑图
        if (isUnlocked)
        {
            // 最后一个解锁关卡黑色图,,,前面的已通关专属通关图
            binding.buttonImage.sprite = (levelIndex == maxUnlockedIndex) ? binding.levelUncompletedImage : binding.levelCompletedImage;
            binding.buttonImage.SetNativeSize(); // 按图片原始尺寸适配
        }

        // 隐藏时图片归位，避免再次显示位置错误
        if (!isUnlocked && buttonOriginalPos.ContainsKey(binding.levelButton))
        {
            binding.buttonRect.anchoredPosition = buttonOriginalPos[binding.levelButton];
        }
    }

    //获取已解锁关卡的最大索引
    private int GetMaxUnlockedLevelIndex()
    {
        int maxIndex = 0;
        foreach (int index in loadedLevelIndexes)
        {
            if (index > maxIndex) maxIndex = index;
        }
        return maxIndex;
    }

    // 刷新所有按钮
    public void RefreshAllLevelButtonsStatus()
    {
        foreach (var binding in levelButtonBindings)
        {
            RefreshSingleButtonStatus(binding);
        }
    }
    #endregion

    public void ResetAllButtonsPosition()
    {
        foreach (var binding in levelButtonBindings)
        {
            // 空值保护，避免报错
            if (binding.levelButton == null || binding.buttonRect == null) continue;
            // 强制把位置设为初始缓存的位置
            if (buttonOriginalPos.ContainsKey(binding.levelButton))
            {
                binding.buttonRect.anchoredPosition = buttonOriginalPos[binding.levelButton];
            }
        }
    }
    #region 
    // 按钮点击回调：仅加载关卡，不修改任何解锁/显示状态
    private void OnLevelButtonClicked(LevelData levelData)
    {
        if (levelData == null) return;
        // 仅调用加载，无任何索引赋值，彻底还原原功能
        PersistentSceneManager.Instance.LoadLevel(levelData);
    }

    // 加载持久化的已加载关卡记录
    public void LoadLoadedLevelRecords()
    {
        loadedLevelIndexes.Clear();
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

        // 默认解锁第一关
        if (loadedLevelIndexes.Count == 0 && levelButtonBindings.Count > 0)
        {
            int firstLevelIndex = levelButtonBindings[0].levelData.levelIndex;
            AddLoadedLevelRecord(firstLevelIndex);
        }
    }

    // 添加已加载关卡记录并持久化
    public void AddLoadedLevelRecord(int levelIndex)
    {
        if (loadedLevelIndexes.Contains(levelIndex)) return;

        loadedLevelIndexes.Add(levelIndex);
        string loadedLevelsStr = string.Join(",", loadedLevelIndexes);
        PlayerPrefs.SetString("LoadedLevelIndexes", loadedLevelsStr);
        PlayerPrefs.Save();

        // 解锁新关卡后刷新状态（新关卡变成黑图，原最后关卡变成通关图）
        RefreshAllLevelButtonsStatus();
    }
    #endregion

    // 显示/隐藏
    public void ShowLevelSelect()
    {
        gameObject.SetActive(true);
        RefreshAllLevelButtonsStatus();
    }
    public void HideLevelSelect()
    {
        gameObject.SetActive(false);
    }


    [System.Serializable]
    public class LevelButtonBinding
    {
        public Button levelButton; // 关卡按钮
        public LevelData levelData; // 对应关卡数据
        [Header("图片与底座")]
        public Image buttonImage; // 按钮上的Image组件,用于替换图片
        public RectTransform buttonRect; // 按钮的RectTransform
        public Sprite levelCompletedImage; // 该关卡通关后的专属图片
        public Sprite levelUncompletedImage; // 该关卡待通关的黑色图片
        public Transform levelBaseTransform; // 关卡图片的底座物体
    }
}