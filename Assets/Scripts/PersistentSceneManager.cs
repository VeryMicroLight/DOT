using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;


public class PersistentSceneManager : MonoBehaviour
{
    public static PersistentSceneManager Instance;

    [Header("开始场景配置")]
    public AssetReference startSceneRef;

    [Header("所有关卡数据")]
    public List<LevelData> allLevelDatas;
    private AsyncOperationHandle<SceneInstance> currentLoadedScene;
    private LevelData currentLevelData;
    public LevelData CurrentLevelData => currentLevelData;

    [Header("场景渐变配置")]
    public float sceneFadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 游戏启动时加载开始场景
        LoadStartScene();
    }

    #region 加载开始场景（适配动画事件）
    public void LoadStartScene()
    {
        if (currentLoadedScene.IsValid())
        {
            // 先渐黑→卸载场景→加载开始场景→渐显
            StartCoroutine(UnloadAndLoadSceneCoroutine(LoadStartSceneInternal));
        }
        else
        {
            // 直接渐黑→加载开始场景→渐显
            StartCoroutine(FadeAndLoadSceneCoroutine(LoadStartSceneInternal));
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
                // 场景加载完成后，启动淡入动画（等待动画完成）
                if (FadeUI.Instance != null)
                {
                    StartCoroutine(FadeUI.Instance.FadeIn());
                }
            }
            else
            {
                Debug.LogError("开始场景加载失败：" + handle.OperationException);
                // 加载失败也恢复显示
                if (FadeUI.Instance != null)
                {
                    StartCoroutine(FadeUI.Instance.FadeIn());
                }
            }
        };
    }
    #endregion

    #region 加载指定关卡（适配动画事件）
    public void LoadLevelByIndex(int targetIndex)
    {
        LevelData targetLevel = allLevelDatas.Find(data => data.levelIndex == targetIndex);
        if (targetLevel == null)
        {
            Debug.LogError("未找到序号为" + targetIndex + "的关卡");
            return;
        }

        if (currentLoadedScene.IsValid())
        {
            StartCoroutine(UnloadAndLoadSceneCoroutine(() => LoadLevelInternal(targetLevel)));
        }
        else
        {
            StartCoroutine(FadeAndLoadSceneCoroutine(() => LoadLevelInternal(targetLevel)));
        }
    }

    public void LoadNextLevel()
    {
        if (currentLevelData == null) return;
        LoadLevelByIndex(currentLevelData.levelIndex + 1);
    }
    #endregion

    #region 整合渐变+场景加载/卸载（核心修改：等待动画事件完成）
    // 先渐黑→执行场景加载逻辑（等待淡出动画完成）
    private IEnumerator FadeAndLoadSceneCoroutine(System.Action loadAction)
    {
        // 等待淡出动画完全结束
        if (FadeUI.Instance != null)
        {
            yield return StartCoroutine(FadeUI.Instance.FadeOut());
        }
        // 执行加载逻辑
        loadAction?.Invoke();
    }

    // 先渐黑→卸载当前场景→执行新场景加载逻辑（等待淡出动画完成）
    private IEnumerator UnloadAndLoadSceneCoroutine(System.Action loadAction)
    {
        // 等待淡出动画完全结束
        if (FadeUI.Instance != null)
        {
            yield return StartCoroutine(FadeUI.Instance.FadeOut());
        }
        // 卸载当前场景，等待卸载完成
        bool unloadDone = false;
        UnloadCurrentScene(() => unloadDone = true);
        while (!unloadDone)
        {
            yield return null;
        }
        // 执行新场景加载逻辑
        loadAction?.Invoke();
    }

    private void LoadLevelInternal(LevelData levelData)
    {
        currentLevelData = levelData;
        levelData.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentLoadedScene = handle;
                SceneManager.SetActiveScene(handle.Result.Scene);
                Debug.Log("关卡" + levelData.levelIndex + "加载成功：" + levelData.levelName);

                if (LevelSelectUI.Instance != null)
                {
                    LevelSelectUI.Instance.AddLoadedLevelRecord(levelData.levelIndex);
                }
                // 关卡加载完成后，启动淡入动画（等待动画完成）
                if (FadeUI.Instance != null)
                {
                    StartCoroutine(FadeUI.Instance.FadeIn());
                }
            }
            else
            {
                Debug.LogError("关卡加载失败：" + handle.OperationException);
                currentLevelData = null;
                // 加载失败也恢复显示
                if (FadeUI.Instance != null)
                {
                    StartCoroutine(FadeUI.Instance.FadeIn());
                }
            }
        };
    }

    private void UnloadCurrentScene(System.Action onUnloaded = null)
    {
        Addressables.UnloadSceneAsync(currentLoadedScene).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("当前场景卸载成功");
                currentLoadedScene = default;
                currentLevelData = null;
                onUnloaded?.Invoke();
            }
            else
            {
                Debug.LogError("场景卸载失败：" + handle.OperationException);
                onUnloaded?.Invoke(); // 即使失败也执行回调，避免卡住
            }
        };
    }
    #endregion

    public void ReturnToMainMenu()
    {
        if (currentLoadedScene.IsValid())
        {
            // 返回主菜单也加渐变（等待动画完成）
            StartCoroutine(UnloadAndLoadSceneCoroutine(() =>
            {
                LoadStartSceneInternal();
                Debug.Log("已卸载当前关卡，回到主菜单");
            }));
        }
        else
        {
            LoadStartScene();
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null) return;

        if (currentLoadedScene.IsValid())
        {
            StartCoroutine(UnloadAndLoadSceneCoroutine(() => LoadLevelInternal(levelData)));
        }
        else
        {
            StartCoroutine(FadeAndLoadSceneCoroutine(() => LoadLevelInternal(levelData)));
        }
    }

    private void OnDestroy()
    {
        if (currentLoadedScene.IsValid())
        {
            Addressables.Release(currentLoadedScene);
        }
    }
}