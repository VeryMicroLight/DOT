using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    public bool isFlipping = false;


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

    //获取图片
    private Sprite GetSpriteForFace(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < diceSprites.Length)
        {
            return diceSprites[faceIndex];
        }
        return null;
    }

    public void PushDice(Vector2 pushDir)
    {
        if (isFlipping) return;

        // 确保一次只走1格
        Vector2 targetPos = (Vector2)transform.position + pushDir;

        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        //立即更新UI
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
            //oldFace.color = new Color(1, 1, 1, scaleT);

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

        // 先赋值目标位置，再强制修正到n.5格式，消除浮点数插值误差
        //transform.position = targetPos;
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
        // 更新UI
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

    // 通用工具方法：强制将物体坐标修正为n.5格式（瓦片中心），后续所有物体都可复用
    public static void CorrectToGridCenter(Transform targetTrans)
    {
        // 提取当前坐标的x、y值
        float x = targetTrans.position.x;
        float y = targetTrans.position.y;
        // 先取整得到整数n（如1.498→1，2.501→3？不，用Mathf.RoundToInt更准确，或直接取整后调整）
        // 核心逻辑：n.5 = 整数部分 + 0.5，消除所有浮点数误差
        int gridX = Mathf.RoundToInt(x - 0.5f); 
        int gridY = Mathf.RoundToInt(y - 0.5f); 
        // 重构严格的n.5格式坐标
        float correctX = gridX + 0.5f;
        float correctY = gridY + 0.5f;
        // 赋值回物体，z轴保持不变
        targetTrans.position = new Vector3(correctX, correctY, targetTrans.position.z);
    }
}