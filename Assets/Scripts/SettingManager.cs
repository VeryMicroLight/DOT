using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Button SettingBtn;
    public GameObject SettingPanel;
    [SerializeField] private Button returnToMenuBtn;

    private void Awake()
    {
        SettingBtn.onClick.AddListener(ToggleSettingPanel);
        returnToMenuBtn.onClick.AddListener(OnReturnToMenuClick);
    }
    private void ToggleSettingPanel()
    {
        if (SettingPanel.activeInHierarchy)   //菜单如果已经激活了，那么再点一下就关闭
        {
            SettingPanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            SettingPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
    private void OnReturnToMenuClick()
    {
        if (PersistentSceneManager.Instance != null)
        {
            // 调用全局管理器的返回主菜单方法
            PersistentSceneManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogError("实例不存在，无法返回主菜单");
        }
    }
}
