using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class DiceSelfController : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    public bool isFlipping = false;
    public int[] diceRealFace = { 1, 2, 3, 4, 5, 6 };
    private RunLevel runLevel;
    public GameObject LevelManager;

    [Header("UI显示")]
    public Image[] diceUI;//注意顺序 up,left,right,back,front
    public Sprite[] diceSprites;

    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }
    };

    // ====================== 输入系统（完全沿用你Player写法） ======================
    private PlayerInputControl inputControl;
    private bool isMoving = false;
    private bool isSliding = false; // 冰面滑动锁

    private void Awake()
    {
        runLevel = LevelManager.GetComponent<RunLevel>();
        inputControl = new PlayerInputControl();
        inputControl.Player.Move.performed += OnMovePerformed;
    }
    private void OnEnable() => inputControl.Enable();
    private void OnDisable() => inputControl.Disable();

    private void Start()
    {
        CorrectToGridCenter(transform);
        UpdateFaceDisplay();
        UpdateDiceUI();
    }

    // ====================== 输入处理 ======================
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // 移动、翻转、滑动中都禁止操作
        if (isMoving || isFlipping || isSliding)
            return;

        Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
        if (inputDir.magnitude > 0.1f)
        {
            inputDir = GetSingleGridDirection(inputDir);
            TryMoveDice(inputDir);
        }
    }

    // ====================== 核心：尝试移动（含障碍+冰面判断） ======================
    private void TryMoveDice(Vector2 dir)
    {
        Vector2 checkPos = (Vector2)transform.position + dir;

        // 1. 前方有障碍物 → 不能动
        if (IsPositionHasObstacle(checkPos))
            return;

        // 2. 判断是不是冰面
        bool isIce = IsPositionHasIce(checkPos);

        isMoving = true;
        runLevel.isMoving = true;
        if (isIce)
        {
            // 冰面：不翻面，直接滑行（一路滑到底）
            StartCoroutine(SlideOnIce(dir));
        }
        else
        {
            // 普通地面：正常移动+翻面
            PushDice(dir);
            // 移除原有 ResetMoveFlag，改在 FlipAnim 结束后解锁
            // StartCoroutine(ResetMoveFlag());
        }
    }

    // ====================== 冰面滑行（连续冰面一直滑） ======================
    private IEnumerator SlideOnIce(Vector2 slideDir)
    {
        isSliding = true;
        Vector2 currentDir = slideDir;

        while (true)
        {
            Vector2 nextPos = (Vector2)transform.position + currentDir;

            // 前面是障碍物 → 停下
            if (IsPositionHasObstacle(nextPos))
                break;

            // 移动一格（不翻面）
            yield return MoveSmooth(transform.position, nextPos, flipDuration);
            CorrectToGridCenter(transform);

            // 下一格还是冰面 → 继续滑同方向
            if (IsPositionHasIce(nextPos))
            {
                continue;
            }
            // 下一格是正常地面 → 停下
            else
            {
                break;
            }
        }

        isSliding = false;
        isMoving = false;
        runLevel.isMoving = false;
    }

    // 平滑移动一格（复用给冰面和普通移动）
    private IEnumerator MoveSmooth(Vector2 from, Vector2 to, float time)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            transform.position = Vector2.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
    }

    // ====================== 障碍检测（完全照搬你Player的逻辑） ======================
    private bool IsPositionHasObstacle(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0);
        if (hit == null) return false;

        // 障碍物
        if (hit.CompareTag("Obstacle"))
            return true;

        // 门：没开就是障碍
        if (hit.CompareTag("Door"))
        {
            var door = hit.GetComponent<DoorBehaviour>();
            return door == null || !door.isOpen;
        }

        // 骰子也算障碍（不能重叠）
        if (hit.CompareTag("Dice"))
            return true;

        return false;
    }

    // 判断是不是冰面
    private bool IsPositionHasIce(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0);
        return hit != null && hit.CompareTag("Ice");
    }

    // ====================== 原有逻辑（仅修改 FlipAnim 加入移动） ======================
    private Vector2 GetSingleGridDirection(Vector2 rawInput)
    {
        float absX = Mathf.Abs(rawInput.x);
        float absY = Mathf.Abs(rawInput.y);
        if (absX > absY)
            return new Vector2(Mathf.Sign(rawInput.x), 0);
        else
            return new Vector2(0, Mathf.Sign(rawInput.y));
    }

    // 移除原有 ResetMoveFlag，改在 FlipAnim 结束后解锁

    private void UpdateDiceUI()
    {
        if (diceUI == null || diceUI.Length < 5) return;
        if (diceSprites == null || diceSprites.Length < 6) return;

        diceUI[0].sprite = GetSpriteForFace(diceState[0]);
        diceUI[1].sprite = GetSpriteForFace(diceState[2]);
        diceUI[2].sprite = GetSpriteForFace(diceState[3]);
        diceUI[3].sprite = GetSpriteForFace(diceState[5]);
        diceUI[4].sprite = GetSpriteForFace(diceState[4]);
    }

    public int TopSideNumber()
    {
        if (diceState[0] == 0) return diceRealFace[0];
        else if (diceState[0] == 1) return diceRealFace[5];
        else if (diceState[0] == 2) return diceRealFace[2];
        else if (diceState[0] == 3) return diceRealFace[3];
        else if (diceState[0] == 4) return diceRealFace[4];
        else if (diceState[0] == 5) return diceRealFace[1];
        else return 0;
    }

    private Sprite GetSpriteForFace(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < diceSprites.Length)
            return diceSprites[faceIndex];
        return null;
    }

    public void PushDice(Vector2 pushDir)
    {
        if (isFlipping) return;

        Vector2 targetPos = (Vector2)transform.position + pushDir;
        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        UpdateDiceUI();
        StartCoroutine(FlipAnim(targetPos, oldTopFace, newTopFace, dirType));
    }

    private int RollDiceState(int dirType)
    {
        int[] newState = (int[])diceState.Clone();
        switch (dirType)
        {
            case 0:
                newState[0] = diceState[4];
                newState[1] = diceState[5];
                newState[4] = diceState[1];
                newState[5] = diceState[0];
                break;
            case 1:
                newState[0] = diceState[5];
                newState[1] = diceState[4];
                newState[4] = diceState[0];
                newState[5] = diceState[1];
                break;
            case 2:
                newState[0] = diceState[2];
                newState[1] = diceState[3];
                newState[2] = diceState[1];
                newState[3] = diceState[0];
                break;
            case 3:
                newState[0] = diceState[3];
                newState[1] = diceState[2];
                newState[2] = diceState[0];
                newState[3] = diceState[1];
                break;
        }
        diceState = newState;
        return diceState[0];
    }

    // 核心修改：在 FlipAnim 中加入和 Player 一样的平滑移动逻辑
    IEnumerator FlipAnim(Vector2 targetPos, int oldTopFace, int newTopFace, int dirType)
    {
        isFlipping = true;
        SpriteRenderer oldFace = diceFaces[oldTopFace];
        SpriteRenderer newFace = diceFaces[newTopFace];
        Vector2[] visualConfig = dirVisualConfig[dirType];
        Vector2 compressAxis = visualConfig[0];
        Vector2 oldCompressDir = visualConfig[1];
        Vector2 newStartPos = visualConfig[2];

        newFace.enabled = true;
        newFace.transform.localPosition = newStartPos;
        newFace.transform.localScale = Vector3.zero;
        newFace.color = new Color(1, 1, 1, 0);

        // ===== 新增：保存起始位置，添加平滑移动（和 Player 的 MovePlayer 逻辑一致） =====
        Vector2 startPos = transform.position;
        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;

            // 1. 位置移动插值（核心！骰子从 startPos 移动到 targetPos）
            transform.position = Vector2.Lerp(startPos, targetPos, t);

            // 2. 原有翻转动画逻辑（完全保留）
            float posT = Mathf.Lerp(0, 0.5f, t);
            float scaleT = Mathf.Lerp(1, 0, t);
            float newScaleT = Mathf.Lerp(0, 1, t);

            Vector3 oldScale = Vector3.one;
            if (compressAxis == Vector2.up)
                oldScale.y = scaleT;
            else
                oldScale.x = scaleT;
            oldFace.transform.localScale = oldScale;
            oldFace.transform.localPosition = oldCompressDir * posT;

            newFace.transform.localPosition = Vector2.Lerp(newStartPos, Vector2.zero, t);
            Vector3 newScale = Vector3.one;
            if (compressAxis == Vector2.up)
                newScale.y = newScaleT;
            else
                newScale.x = newScaleT;
            newFace.transform.localScale = newScale;
            newFace.color = new Color(1, 1, 1, 255);

            yield return null;
        }

        // 强制修正位置到网格中心（消除插值误差）
        transform.position = targetPos;
        CorrectToGridCenter(transform);

        UpdateFaceDisplay();
        isFlipping = false;
        isMoving = false; // 移动+翻面完成后解锁
        runLevel.isMoving = false;
    }

    private void UpdateFaceDisplay()
    {
        for (int i = 0; i < diceFaces.Length; i++)
        {
            if (i == diceState[0])
            {
                diceFaces[i].enabled = true;
                diceFaces[i].transform.localPosition = Vector3.zero;
                diceFaces[i].transform.localScale = Vector3.one;
                diceFaces[i].color = Color.white;
            }
            else
            {
                diceFaces[i].enabled = false;
                diceFaces[i].transform.localPosition = Vector3.zero;
                diceFaces[i].transform.localScale = Vector3.zero;
                diceFaces[i].color = new Color(1, 1, 1, 0);
            }
        }
        UpdateDiceUI();
    }

    private int GetPushDirectionType(Vector2 dir)
    {
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            return dir.y > 0 ? 0 : 1;
        else
            return dir.x > 0 ? 2 : 3;
    }

    public static void CorrectToGridCenter(Transform targetTrans)
    {
        float x = targetTrans.position.x;
        float y = targetTrans.position.y;
        int gridX = Mathf.RoundToInt(x - 0.5f);
        int gridY = Mathf.RoundToInt(y - 0.5f);
        float correctX = gridX + 0.5f;
        float correctY = gridY + 0.5f;
        targetTrans.position = new Vector3(correctX, correctY, targetTrans.position.z);
    }
}