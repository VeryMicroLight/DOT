using UnityEngine;
using UnityEngine.UI;

public class RestartLevelButton : MonoBehaviour
{
    [SerializeField] private Button restartBtn; // 拖拽你的重新开始按钮

    private void Awake()
    {
        // 绑定按钮点击事件
        restartBtn.onClick.AddListener(OnRestartButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        // 获取全局管理器
        PersistentSceneManager manager = PersistentSceneManager.Instance;
        if (manager == null || manager.CurrentLevelData == null)
        {
            Debug.LogError("无法获取当前关卡数据，无法重新开始");
            return;
        }

        // 重新加载当前关卡
        manager.LoadLevel(manager.CurrentLevelData);
    }
}