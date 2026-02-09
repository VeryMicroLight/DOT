using UnityEngine;
using System.Collections;

public class StartDice : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    public bool isFlipping = false;

    // 新增：自动动画相关配置
    [Header("自动动画配置")]
    public float autoFlipInterval = 1f; // 每隔1秒触发一次动画
    private float autoFlipTimer = 0f;   // 定时器

    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }
    };

    private void Start()
    {
        UpdateFaceDisplay();
    }

    // 新增：Update中检测定时器，触发自动动画
    private void Update()
    {
        if (isFlipping) return; // 动画播放中不触发新的动画

        autoFlipTimer += Time.deltaTime;
        if (autoFlipTimer >= autoFlipInterval)
        {
            autoFlipTimer = 0f;
            PlayLeftPushAnim(); // 播放向左推的原地动画
        }
    }

    // 骰子朝上的点数（保留，不影响核心功能）
    public int TopSideNumber()
    {
        if (diceState[0] == 0) return 1;
        else if (diceState[0] == 1) return 6;
        else if (diceState[0] == 2) return 3;
        else if (diceState[0] == 3) return 4;
        else if (diceState[0] == 4) return 5;
        else if (diceState[0] == 5) return 2;
        else return 0;
    }

    // 新增：原地播放向左推的动画（不改变骰子位置）
    private void PlayLeftPushAnim()
    {
        int dirType = 3; // 向左推对应的方向类型（对应GetPushDirectionType里x<0的情况）
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        // 因为是原地动画，targetPos直接用当前位置
        Vector2 targetPos = (Vector2)transform.position;
        StartCoroutine(FlipAnim(targetPos, oldTopFace, newTopFace, dirType));
    }

    // 原PushDice方法：保留核心逻辑，移除UI更新（如果不需要外部调用可改为private）
    public void PushDice(Vector2 pushDir)
    {
        if (isFlipping) return;

        Vector2 targetPos = (Vector2)transform.position + pushDir;
        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        StartCoroutine(FlipAnim(targetPos, oldTopFace, newTopFace, dirType));
    }

    private int RollDiceState(int dirType)
    {
        int[] newState = (int[])diceState.Clone();
        switch (dirType)
        {
            case 0: // 上
                newState[0] = diceState[4];
                newState[1] = diceState[5];
                newState[4] = diceState[1];
                newState[5] = diceState[0];
                break;
            case 1: // 下
                newState[0] = diceState[5];
                newState[1] = diceState[4];
                newState[4] = diceState[0];
                newState[5] = diceState[1];
                break;
            case 2: // 右
                newState[0] = diceState[2];
                newState[1] = diceState[3];
                newState[2] = diceState[1];
                newState[3] = diceState[0];
                break;
            case 3: // 左
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

        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;

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
            newFace.color = new Color(1, 1, 1, 1); // 修正：原代码255是错误的，Color的alpha范围是0-1

            yield return null;
        }

        // 原地动画不需要修正位置，注释掉坐标修正逻辑
        // CorrectToGridCenter(transform);

        UpdateFaceDisplay();
        isFlipping = false;
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
    }

    private int GetPushDirectionType(Vector2 dir)
    {
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            return dir.y > 0 ? 0 : 1;
        }
        else
        {
            return dir.x > 0 ? 2 : 3;
        }
    }

}