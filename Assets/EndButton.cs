using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EndButton : MonoBehaviour
{
    [SerializeField] private Button returnToMenuBtn;
    [SerializeField] private Button quitGameBtn;

    private void Awake()
    {
        returnToMenuBtn.onClick.AddListener(OnReturnToMenuClick);
        quitGameBtn.onClick.AddListener(OnQuitGameClick);
    }
    private void OnReturnToMenuClick()
    {
        if (PersistentSceneManager.Instance != null)
        {
            Time.timeScale = 1;
            // 调用全局管理器的返回主菜单方法
            PersistentSceneManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogError("实例不存在，无法返回主菜单");
        }
    }
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
