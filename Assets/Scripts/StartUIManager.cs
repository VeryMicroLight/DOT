using UnityEngine;
using UnityEngine.UI;
// 编辑器模式下退出需要引用这个命名空间
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartUIManager : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;   
    [SerializeField] private Button continueGameBtn;
    [SerializeField] private Button quitGameBtn;   

    private void Awake()
    {
        newGameBtn.onClick.AddListener(OnNewGameClick);
        continueGameBtn.onClick.AddListener(OnContinueGameClick);
        quitGameBtn.onClick.AddListener(OnQuitGameClick);
    }


    //新开始：重置按钮并加载第1关
    private void OnNewGameClick()
    {
        if (PersistentSceneManager.Instance != null)
        {
            //重置关卡解锁记录
            ResetLevelUnlockRecords();

            //刷新选关界面按钮状态
            if (LevelSelectUI.Instance != null)
            {
                LevelSelectUI.Instance.RefreshAllLevelButtonsStatus();
            }

            //加载第1关
            PersistentSceneManager.Instance.LoadLevelByIndex(1);
        }
    }

    // 重置关卡解锁记录
    private void ResetLevelUnlockRecords()
    {
        // 清空原有记录，只保留第一关的索引
        PlayerPrefs.SetString("LoadedLevelIndexes", "1");
        PlayerPrefs.Save();

        // 同步更新LevelSelectUI的内存中的记录（避免UI读取旧数据）
        if (LevelSelectUI.Instance != null)
        {
            LevelSelectUI.Instance.LoadLoadedLevelRecords();
        }

        Debug.Log("关卡解锁记录已重置，仅保留第一关");
    }


    // 继续游戏：加载玩家解锁的最高关卡
    private void OnContinueGameClick()
    {
        // 边界判断：全局管理器为空则直接返回
        if (PersistentSceneManager.Instance == null)
        {
            Debug.LogError("PersistentSceneManager 实例不存在");
            return;
        }

        // 从PlayerPrefs读取已解锁关卡记录（和LevelSelectUI逻辑一致）
        string loadedLevelsStr = PlayerPrefs.GetString("LoadedLevelIndexes", "");
        if (string.IsNullOrEmpty(loadedLevelsStr))
        {
            // 没有任何关卡记录，默认加载第1关
            PersistentSceneManager.Instance.LoadLevelByIndex(1);
            Debug.Log("无已保存进度，默认加载第1关");
            return;
        }

        // 解析字符串，找到最大的关卡索引
        string[] indexArr = loadedLevelsStr.Split(',');
        int maxLevelIndex = 0;
        foreach (string indexStr in indexArr)
        {
            if (int.TryParse(indexStr, out int levelIndex) && levelIndex > maxLevelIndex)
            {
                maxLevelIndex = levelIndex;
            }
        }

        // 加载最高关卡
        if (maxLevelIndex > 0)
        {
            PersistentSceneManager.Instance.LoadLevelByIndex(maxLevelIndex);
            Debug.Log("继续游戏：加载最高关卡 " + maxLevelIndex);
        }
        else
        {
            // 解析失败，默认加载第1关
            PersistentSceneManager.Instance.LoadLevelByIndex(1);
            Debug.Log("进度解析失败，默认加载第1关");
        }
    }


    // 退出游戏：区分编辑器模式和打包后模式
    private void OnQuitGameClick()
    {
        // 打包后的游戏，直接退出应用
        Application.Quit();

        // 编辑器模式下，退出播放模式（仅在编辑器中生效）
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif

        Debug.Log("退出游戏");
    }
}