using UnityEngine;
using UnityEngine.UI;

public class LevelWinnerUIManager : MonoBehaviour
{
    [SerializeField] private Button nextLevelBtn; // 下一关按钮
    [SerializeField] private Button backToStartBtn; // 返回开始界面按钮
    [SerializeField] private GameObject winnerPanel; // 胜利面板（默认隐藏）

    private void Awake()
    {
        nextLevelBtn.onClick.AddListener(OnNextLevelClick);
        backToStartBtn.onClick.AddListener(OnBackToStartClick);
       // winnerPanel.SetActive(false); // 初始隐藏胜利面板
    }

    // 外部调用：关卡胜利时显示面板（如玩家触达终点、消灭所有敌人时调用）
    public void ShowWinnerPanel()
    {
        winnerPanel.SetActive(true);
    }

    // 点击下一关
    private void OnNextLevelClick()
    {
        PersistentSceneManager.Instance.LoadNextLevel();
    }

    // 点击返回开始界面
    private void OnBackToStartClick()
    {
        PersistentSceneManager.Instance.LoadStartScene();
    }
}
