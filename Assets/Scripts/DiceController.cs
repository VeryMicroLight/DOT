using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    public bool isFlipping = false;
    public int[] diceRealFace = { 1, 2, 3, 4, 5, 6 };

    [Header("UI显示")]
    public Image[] diceUI; // 顺序：up,left,right,back,front
    public Sprite[] diceSprites;

    [Header("冰面滑动配置")]
    public LayerMask iceLayer; // 冰面层
    public float slideMoveTime = 0.15f; // 滑动一格耗时
    private bool isSliding = false; // 滑动中标记
    private Vector2 slideDir; // 滑行方向

    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }
    };

    private void Start()
    {
        CorrectToGridCenter(transform);
        UpdateFaceDisplay();
        UpdateDiceUI();
    }

    private void UpdateDiceUI()
    {
        if (diceUI == null || diceUI.Length < 5) return;
        if (diceSprites == null || diceSprites.Length < 6) return;

        diceUI[0].sprite = GetSpriteForFace(diceState[0]);  // 中心：上面
        diceUI[1].sprite = GetSpriteForFace(diceState[2]);  // 左侧：左面
        diceUI[2].sprite = GetSpriteForFace(diceState[3]);  // 右侧：右面
        diceUI[3].sprite = GetSpriteForFace(diceState[5]);  // 上方：后面
        diceUI[4].sprite = GetSpriteForFace(diceState[4]);  // 下方：前面
    }

    // 骰子朝上的点数
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

    // 获取图片
    private Sprite GetSpriteForFace(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < diceSprites.Length)
        {
            return diceSprites[faceIndex];
        }
        return null;
    }

    // 推动骰子
    public void PushDice(Vector2 pushDir)
    {
        // 翻转/滑动中禁止操作
        if (isFlipping || isSliding) return;

        // 计算下一格位置并校准到网格中心
        Vector2 targetPos = (Vector2)transform.position + pushDir;
        CorrectToGridCenter(ref targetPos);

        // 检测下一格是否是冰面 ,, 是则滑行，否则正常翻面
        bool willEnterIce = IsPositionIce(targetPos);
        if (willEnterIce)
        {
            slideDir = pushDir;
            StartCoroutine(SlideIceCoroutine()); // 触发滑行（不翻转）
            return;
        }

        // 非冰面：执行原有翻面逻辑
        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);
        UpdateDiceUI();
        StartCoroutine(FlipAnim(targetPos, oldTopFace, newTopFace, dirType));
    }

    // 冰面滑行协程
    private IEnumerator SlideIceCoroutine()
    {
        isSliding = true;

        while (true)
        {
            //计算下一格位置并校准网格
            Vector2 nextPos = (Vector2)transform.position + slideDir;
            CorrectToGridCenter(ref nextPos);

            // 检测下一格是否有墙（Tag=Obstacle）→ 有则停止滑行
            if (IsPositionBlocked(nextPos))
            {
                break;
            }

            //平滑滑动到下一格
            Vector2 startPos = transform.position;
            float timer = 0;
            while (timer < slideMoveTime)
            {
                timer += Time.deltaTime;
                transform.position = Vector2.Lerp(startPos, nextPos, timer / slideMoveTime);
                yield return null;
            }

            //滑到下一格后，强制校准位置
            transform.position = nextPos;
            CorrectToGridCenter(transform);

            //检测当前位置是否还在冰面上 ,,不在则停止滑行（滑到普通地面了）
            if (!IsPositionIce(transform.position))
            {
                break;
            }
        }

        isSliding = false; // 重置滑动状态
    }


    // 检测指定位置是否是冰面（仅检测iceLayer + Tag=Ice）
    private bool IsPositionIce(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(checkPos, iceLayer);
        return hit != null && hit.CompareTag("Ice");
    }

    // 检测指定位置是否有墙
    private bool IsPositionBlocked(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(checkPos);
        return hit != null && hit.CompareTag("Obstacle");
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
            newFace.color = new Color(1, 1, 1, 255);

            yield return null;
        }

        CorrectToGridCenter(transform);
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
        UpdateDiceUI();
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

    // 通用工具方法：强制将物体坐标修正为n.5格式（瓦片中心）
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

    // 新增：校准Vector2位置到网格中心（用于检测）
    private void CorrectToGridCenter(ref Vector2 targetPos)
    {
        int gridX = Mathf.RoundToInt(targetPos.x - 0.5f);
        int gridY = Mathf.RoundToInt(targetPos.y - 0.5f);
        targetPos = new Vector2(gridX + 0.5f, gridY + 0.5f);
    }
}