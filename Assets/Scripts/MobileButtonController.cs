using UnityEngine;
using UnityEngine.UI;

// 手机虚拟方向按钮控制
// 完全不影响原有键盘/手柄输入，自动兼容
public class MobileButtonController : MonoBehaviour
{
    [Header("绑定四个方向按钮")]
    public Button upBtn;
    public Button downBtn;
    public Button leftBtn;
    public Button rightBtn;

    [Header("绑定你的Player控制器")]
    public PlayerController playerController;

    private void Awake()
    {
        // 自动寻找PlayerController（你也可以手动拖）
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        // 给按钮注册点击事件
        upBtn.onClick.AddListener(() => OnClickDirection(Vector2.up));
        downBtn.onClick.AddListener(() => OnClickDirection(Vector2.down));
        leftBtn.onClick.AddListener(() => OnClickDirection(Vector2.left));
        rightBtn.onClick.AddListener(() => OnClickDirection(Vector2.right));
    }

    // 按钮点击 = 触发一次移动
    private void OnClickDirection(Vector2 dir)
    {
        if (playerController == null) return;

        // 直接调用你原有移动逻辑！完全复用
        playerController.OnMovePerformedByDirection(dir);
    }
}