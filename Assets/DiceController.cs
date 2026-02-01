using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public SpriteRenderer[] diceFaces; // ˳��Up(0), Down(1), Left(2), Right(3), Front(4), Back(5)
    public float flipDuration = 0.3f;
    private int[] diceState = { 0, 1, 2, 3, 4, 5 };
    private bool isFlipping = false;

    //新增UI相关变量
    [Header("UI显示")]
    public Image[] diceUI;//注意顺序 up,left,right,back,front
    public Sprite[] diceSprites;

    
    

    // �ؼ��޸ģ���С��ʼλ��ƫ�ƣ����¾����޷�����
    private Vector2[][] dirVisualConfig = new Vector2[][]
    {
        new Vector2[] { Vector2.up, Vector2.up, new Vector2(0, -0.5f) },     // ���� �� �����ʼY=-0.5�����Ͼ���ײ���
        new Vector2[] { Vector2.up, Vector2.down, new Vector2(0, 0.5f) },    // ���� �� �����ʼY=0.5�����Ͼ��涥����
        new Vector2[] { Vector2.right, Vector2.right, new Vector2(-0.5f, 0) },// ���� �� �����ʼX=-0.5�����Ͼ�����ࣩ
        new Vector2[] { Vector2.right, Vector2.left, new Vector2(0.5f, 0) }  // ���� �� �����ʼX=0.5�����Ͼ����Ҳࣩ
    };

    private void Start()
    {
        UpdateFaceDisplay();
        UpdateDiceUI();
    }

    //新增骰子UI显示函数
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

        Vector2 targetPos = (Vector2)transform.position + pushDir;
        targetPos = new Vector2(Mathf.Round(targetPos.x), Mathf.Round(targetPos.y));

        int dirType = GetPushDirectionType(pushDir);
        int oldTopFace = diceState[0];
        int newTopFace = RollDiceState(dirType);

        //立即更新UI
        UpdateDiceUI();


        StartCoroutine(FlipAndMove(targetPos, oldTopFace, newTopFace, dirType));
    }

   

    private int RollDiceState(int dirType)
    {
        int[] newState = (int[])diceState.Clone();
        switch (dirType)
        {
            case 0: // ���� �� �Ϸ�
                newState[0] = diceState[4];
                newState[1] = diceState[5];
                newState[4] = diceState[1];
                newState[5] = diceState[0];
                break;
            case 1: // ���� �� �·�
                newState[0] = diceState[5];
                newState[1] = diceState[4];
                newState[4] = diceState[0];
                newState[5] = diceState[1];
                break;
            case 2: // ���� �� �ҷ�
                newState[0] = diceState[2];
                newState[1] = diceState[3];
                newState[2] = diceState[1];
                newState[3] = diceState[0];
                break;
            case 3: // ���� �� ��
                newState[0] = diceState[3];
                newState[1] = diceState[2];
                newState[2] = diceState[0];
                newState[3] = diceState[1];
                break;
        }
        diceState = newState;
        return diceState[0];
    }

    // �����޸ģ�λ��������������ʵ���޷�����
    IEnumerator FlipAndMove(Vector2 targetPos, int oldTopFace, int newTopFace, int dirType)
    {
        isFlipping = true;
        SpriteRenderer oldFace = diceFaces[oldTopFace];
        SpriteRenderer newFace = diceFaces[newTopFace];
        Vector2[] visualConfig = dirVisualConfig[dirType];
        Vector2 compressAxis = visualConfig[0];
        Vector2 oldCompressDir = visualConfig[1];
        Vector2 newStartPos = visualConfig[2];

        // ��ʼ�����棺���������Ե
        newFace.enabled = true;
        newFace.transform.localPosition = newStartPos;
        newFace.transform.localScale = Vector3.zero;
        newFace.color = new Color(1, 1, 1, 0);

        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            // �ؼ�ϵ������λ���ƶ����Ⱥ����ŷ�����ȫƥ��
            float posT = Mathf.Lerp(0, 0.5f, t);
            float scaleT = Mathf.Lerp(1, 0, t);
            float newScaleT = Mathf.Lerp(0, 1, t);

            // ���棺ѹ����ͬʱ���ƶ����ȸպ��ñ�Ե����������
            Vector3 oldScale = Vector3.one;
            if (compressAxis == Vector2.up)
                oldScale.y = scaleT;
            else
                oldScale.x = scaleT;
            oldFace.transform.localScale = oldScale;
            oldFace.transform.localPosition = oldCompressDir * posT;
            oldFace.color = new Color(1, 1, 1, scaleT);

            // ���棺չ����ͬʱ��ͬ���ƶ������ģ�ʼ�ս�������
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
}