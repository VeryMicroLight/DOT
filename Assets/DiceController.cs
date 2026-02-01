using UnityEngine;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // 顺序：Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    private bool isFlipping = false;

    // 关键修改：缩小初始位置偏移，让新旧面无缝贴合
    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },     // 下推 → 新面初始Y=-0.5（贴合旧面底部）
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },    // 上推 → 新面初始Y=0.5（贴合旧面顶部）
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },// 左推 → 新面初始X=-0.5（贴合旧面左侧）
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }  // 右推 → 新面初始X=0.5（贴合旧面右侧）
    };

    private void Start()
    {
        UpdateFaceDisplay();
    }

    public void PushDice(Vector2 pushDir)
    {
        if (isFlipping) return;

        Vector2 targetPos = (Vector2)transform.position + pushDir;
        targetPos = new Vector2(Mathf.Round(targetPos.x), Mathf.Round(targetPos.y));

        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        StartCoroutine(FlipAndMove(targetPos, oldTopFace, newTopFace, dirType));
    }

    private int RollDiceState(int dirType)
    {
        int[] newState = (int[])diceState.Clone();
        switch (dirType)
        {
            case 0: // 下推 → 上翻
                newState[0] = diceState[4];
                newState[1] = diceState[5];
                newState[4] = diceState[1];
                newState[5] = diceState[0];
                break;
            case 1: // 上推 → 下翻
                newState[0] = diceState[5];
                newState[1] = diceState[4];
                newState[4] = diceState[0];
                newState[5] = diceState[1];
                break;
            case 2: // 左推 → 右翻
                newState[0] = diceState[2];
                newState[1] = diceState[3];
                newState[2] = diceState[1];
                newState[3] = diceState[0];
                break;
            case 3: // 右推 → 左翻
                newState[0] = diceState[3];
                newState[1] = diceState[2];
                newState[2] = diceState[0];
                newState[3] = diceState[1];
                break;
        }
        diceState = newState;
        return diceState[0];
    }

    // 核心修改：位置与缩放联动，实现无缝贴合
    IEnumerator FlipAndMove(Vector2 targetPos, int oldTopFace, int newTopFace, int dirType)
    {
        isFlipping = true;
        SpriteRenderer oldFace = diceFaces[oldTopFace];
        SpriteRenderer newFace = diceFaces[newTopFace];
        Vector2[] visualConfig = dirVisualConfig[dirType];
        Vector2 compressAxis = visualConfig[0];
        Vector2 oldCompressDir = visualConfig[1];
        Vector2 newStartPos = visualConfig[2];

        // 初始化新面：紧贴旧面边缘
        newFace.enabled = true;
        newFace.transform.localPosition = newStartPos;
        newFace.transform.localScale = Vector3.zero;
        newFace.color = new Color(1, 1, 1, 0);

        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            // 关键系数：让位置移动幅度和缩放幅度完全匹配
            float posT = Mathf.Lerp(0, 0.5f, t);
            float scaleT = Mathf.Lerp(1, 0, t);
            float newScaleT = Mathf.Lerp(0, 1, t);

            // 旧面：压缩的同时，移动幅度刚好让边缘和新面贴合
            Vector3 oldScale = Vector3.one;
            if (compressAxis == Vector2.up)
                oldScale.y = scaleT;
            else
                oldScale.x = scaleT;
            oldFace.transform.localScale = oldScale;
            oldFace.transform.localPosition = oldCompressDir * posT;
            oldFace.color = new Color(1, 1, 1, scaleT);

            // 新面：展开的同时，同步移动到中心，始终紧贴旧面
            newFace.transform.localPosition = Vector2.Lerp(newStartPos, Vector2.zero, t);
            Vector3 newScale = Vector3.one;
            if (compressAxis == Vector2.up)
                newScale.y = newScaleT;
            else
                newScale.x = newScaleT;
            newFace.transform.localScale = newScale;
            newFace.color = new Color(1, 1, 1, newScaleT);

            yield return null;
        }

        transform.position = targetPos;
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