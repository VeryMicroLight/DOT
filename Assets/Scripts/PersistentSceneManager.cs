using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PersistentSceneManager : MonoBehaviour
{
    public static PersistentSceneManager Instance; // 单例，全局调用

    [Header("开始场景配置")]
    public AssetReference startSceneRef; 

    [Header("所有关卡数据")]
    public List<LevelData> allLevelDatas; 
    private AsyncOperationHandle<SceneInstance> currentLoadedScene; // 记录当前加载的场景
    private LevelData currentLevelData; // 记录当前关卡数据
    public LevelData CurrentLevelData => currentLevelData;

    private void Awake()
    {
        // 单例初始化，确保常驻场景唯一
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 游戏启动时自动加载开始场景
        LoadStartScene();
    }

    #region 加载开始场景
    public void LoadStartScene()
    {
        // 先卸载当前可能存在的场景，再加载开始场景
        if (currentLoadedScene.IsValid())
        {
            UnloadCurrentScene(() => LoadStartSceneInternal());
        }
        else
        {
            LoadStartSceneInternal();
        }
    }

    private void LoadStartSceneInternal()
    {
        startSceneRef.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentLoadedScene = handle;
                Debug.Log("开始场景加载成功");
            }
            else
            {
                Debug.LogError("开始场景加载失败：" + handle.OperationException);
            }
        };
    }
    #endregion

    #region 加载指定关卡（从开始界面点击调用）
    public void LoadLevelByIndex(int targetIndex)
    {
        // 根据序号匹配关卡数据
        LevelData targetLevel = allLevelDatas.Find(data => data.levelIndex == targetIndex);
        if (targetLevel == null)
        {
            Debug.LogError("未找到序号为" + targetIndex + "的关卡");
            return;
        }

        // 卸载当前场景，再加载目标关卡
        if (currentLoadedScene.IsValid())
        {
            UnloadCurrentScene(() => LoadLevelInternal(targetLevel));
        }
        else
        {
            LoadLevelInternal(targetLevel);
        }
    }
    //加载下一关（关卡胜利后调用）
    public void LoadNextLevel()
    {
        if (currentLevelData == null) return;
        // 下一关序号=当前序号+1
        LoadLevelByIndex(currentLevelData.levelIndex + 1);
    }
    #endregion

    #region 内部方法：加载关卡、卸载当前场景
    private void LoadLevelInternal(LevelData levelData)
    {
        currentLevelData = levelData;
        // 加载Addressable关卡（叠加模式，保留常驻场景）
        levelData.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentLoadedScene = handle;
                SceneManager.SetActiveScene(handle.Result.Scene); // 设置关卡为活动场景
                Debug.Log("关卡" + levelData.levelIndex + "加载成功：" + levelData.levelName);

                // 关键修改：关卡加载成功后，通知LevelSelectUI添加已加载记录（解锁该关卡）
                if (LevelSelectUI.Instance != null)
                {
                    LevelSelectUI.Instance.AddLoadedLevelRecord(levelData.levelIndex);
                }
            }
            else
            {
                Debug.LogError("关卡加载失败：" + handle.OperationException);
                currentLevelData = null;
            }
        };
    }

    private void UnloadCurrentScene(System.Action onUnloaded = null)
    {
        // 用Addressable卸载，保证资源释放
        Addressables.UnloadSceneAsync(currentLoadedScene).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("当前场景卸载成功");
                currentLoadedScene = default;
                currentLevelData = null;
                onUnloaded?.Invoke(); // 卸载完成后执行回调
            }
            else
            {
                Debug.LogError("场景卸载失败：" + handle.OperationException);
            }
        };
    }
    #endregion

    // 防止内存泄漏，退出时释放
    private void OnDestroy()
    {
        if (currentLoadedScene.IsValid())
        {
            Addressables.Release(currentLoadedScene);
        }
    }

    // 新增：直接通过LevelData加载关卡（适配LevelSelectUI的调用）
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null) return;

        // 卸载当前场景，再加载目标关卡
        if (currentLoadedScene.IsValid())
        {
            UnloadCurrentScene(() => LoadLevelInternal(levelData));
        }
        else
        {
            LoadLevelInternal(levelData);
        }
    }
}