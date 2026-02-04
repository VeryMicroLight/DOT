using UnityEngine;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{
    [SerializeField] private Button startGameBtn; // 拖拽开始游戏按钮

    private void Awake()
    {
        startGameBtn.onClick.AddListener(OnStartGameClick);
    }

    // 点击开始游戏：加载第一关（序号1）
    private void OnStartGameClick()
    {
        if (PersistentSceneManager.Instance != null)
        {
            PersistentSceneManager.Instance.LoadLevelByIndex(1);
        }
    }
}