using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Button SettingBtn;
    public GameObject SettingPanel;

    private void Awake()
    {
        SettingBtn.onClick.AddListener(ToggleSettingPanel);
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
}
