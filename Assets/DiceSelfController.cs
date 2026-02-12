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
    public Image[] diceUI;//顺序 :up,left,right,back,front
    public Sprite[] diceSprites;

    [Header("冰面滑动配置")]
    public LayerMask iceLayer; // 冰面所在图层
    public float slideMoveTime = 0.15f; // 冰面滑行一格耗时

    // 调试开关
    public bool isDebugMode = true;

    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }
    };

    private PlayerInputControl inputControl;
    private bool isMoving = false;
    private bool isSliding = false; // 冰面滑动锁
    private Vector2 lastInputDir; // 记录最后一次输入方向

    private void Awake()
    {

        runLevel = LevelManager.GetComponent<RunLevel>();

        inputControl = new PlayerInputControl();
        inputControl.Player.Move.performed += OnMovePerformed;
        inputControl.Player.Move.canceled += ctx => lastInputDir = Vector2.zero;
    }

    private void OnEnable()
    {
        inputControl.Enable();
    }

    private void OnDisable()
    {
        inputControl.Disable();
    }

    private void Start()
    {
        // 强制校准初始位置
        CorrectToGridCenter(transform);
        UpdateFaceDisplay();
        UpdateDiceUI();

        //// 调试：输出初始位置
        //if (isDebugMode)
        //    Debug.Log($"[骰子调试] 初始位置：{transform.position}，校准后：{transform.position}", this);

        //// 调试：检查骰子Collider
        //if (GetComponent<Collider2D>() == null)
        //    Debug.LogError("[骰子调试] 骰子无Collider2D组件！", this);
        //else if (GetComponent<Collider2D>().isTrigger)
        //    Debug.LogWarning("[骰子调试] 骰子Collider是Trigger，可能导致检测异常！", this);
    }

    private void Update()
    {
        // 仅在无操作时，检测持续输入
        if (!isMoving && !isFlipping && !isSliding)
        {
            Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
            if (inputDir.magnitude > 0.1f && lastInputDir != inputDir)
            {
                lastInputDir = inputDir;
                inputDir = GetSingleGridDirection(inputDir);
                if (isDebugMode)
                    Debug.Log($"[骰子调试] Update检测到输入方向：{inputDir}", this);
                TryMoveDice(inputDir);
            }
        }
    }

    // ====================== 输入处理（保留+加调试） ======================
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // 移动、翻转、滑动中都禁止操作
        if (isMoving || isFlipping || isSliding)
        {
             return;
        }

        Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
      
        if (inputDir.magnitude > 0.1f)
        {
            inputDir = GetSingleGridDirection(inputDir);
            lastInputDir = inputDir;
         
            TryMoveDice(inputDir);
        }
    }

    // ====================== 核心：尝试移动（加全流程调试） ======================
    private void TryMoveDice(Vector2 dir)
    {
        Vector2 checkPos = (Vector2)transform.position + dir;
        CorrectToGridCenter(ref checkPos);

        // 调试：输出检测位置
        
        // 检测障碍物
        bool hasObstacle = IsPositionHasObstacle(checkPos);
      
        if (hasObstacle)
            return;

        // 检测冰面
        bool isIce = IsPositionHasIce(checkPos);
        if (isDebugMode)
        {
          
            Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0, iceLayer);
          
        }

        isMoving = true;
        if (runLevel != null)
            runLevel.isMoving = true;

        if (isIce)
        {
            
            StartCoroutine(SlideOnIce(dir));
        }
        else
        {
          
            PushDice(dir);
        }
    }

    //冰面滑行
    private IEnumerator SlideOnIce(Vector2 slideDir)
    {
        isSliding = true;
        int slideCount = 0;
      
        while (true)
        {
            slideCount++;
            //  计算下一格位置
            Vector2 nextPos = (Vector2)transform.position + slideDir;
            CorrectToGridCenter(ref nextPos);

            // 检测障碍物
            if (IsPositionHasObstacle(nextPos))
            {
                break;
            }

            // 检测冰面
            if (!IsPositionHasIce(nextPos))
            {
                break;
            }

            // 平滑移动
            if (isDebugMode)
                Debug.Log($"[骰子调试] 滑行到{nextPos}", this);
            yield return MoveSmooth(transform.position, nextPos, slideMoveTime);
            CorrectToGridCenter(transform);

            //安全兜底
            if (slideCount >= 20)
            {
                break;
            }
        }
        isSliding = false;
        isMoving = false;
        if (runLevel != null)
            runLevel.isMoving = false;
    }

    // 平滑移动一（保留）
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

    // ====================== 障碍检测（加调试） ======================
    private bool IsPositionHasObstacle(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0);
        if (hit == null) return false;
        if (hit.CompareTag("Obstacle"))
            return true;

        if (hit.CompareTag("Door"))
        {
            var door = hit.GetComponent<DoorBehaviour>();
            bool doorClosed = door == null || !door.isOpen;
           
            return doorClosed;
        }

        if (hit.CompareTag("Dice"))
            return true;

        return false;
    }

    // 冰面检测
    private bool IsPositionHasIce(Vector2 checkPos)
    {
        // 检测图层+区域
        Collider2D hit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0, iceLayer);
        if (hit == null) return false;

        // 检测Tag
        bool isIceTag = hit.CompareTag("Ice");
       
        return isIceTag;
    }


    private Vector2 GetSingleGridDirection(Vector2 rawInput)
    {
        float absX = Mathf.Abs(rawInput.x);
        float absY = Mathf.Abs(rawInput.y);
        if (absX > absY)
            return new Vector2(Mathf.Sign(rawInput.x), 0);
        else
            return new Vector2(0, Mathf.Sign(rawInput.y));
    }

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
        CorrectToGridCenter(ref targetPos);
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

        Vector2 startPos = transform.position;
        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;

            transform.position = Vector2.Lerp(startPos, targetPos, t);

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

        transform.position = targetPos;
        CorrectToGridCenter(transform);

        UpdateFaceDisplay();
        isFlipping = false;
        isMoving = false;
        if (runLevel != null)
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

    private void CorrectToGridCenter(ref Vector2 targetPos)
    {
        int gridX = Mathf.RoundToInt(targetPos.x - 0.5f);
        int gridY = Mathf.RoundToInt(targetPos.y - 0.5f);
        targetPos = new Vector2(gridX + 0.5f, gridY + 0.5f);
    }
}