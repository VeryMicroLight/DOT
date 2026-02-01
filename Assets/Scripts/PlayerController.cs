using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveDuration = 0.2f;
    private PlayerInputControl inputControl;
    private bool isMoving = false;

    private void Awake()
    {
        inputControl = new PlayerInputControl();
    }

    private void OnEnable() => inputControl.Enable();
    private void OnDisable() => inputControl.Disable();

    private void Update()
    {
        if (!isMoving)
        {
            Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
            if (inputDir.magnitude > 0.1f)
            {
                inputDir = inputDir.normalized;
                StartCoroutine(MovePlayer(inputDir));
            }
        }
    }

    IEnumerator MovePlayer(Vector2 inputDir)
    {
        isMoving = true;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + inputDir;
        targetPos = new Vector2(Mathf.Round(targetPos.x), Mathf.Round(targetPos.y));

        // 检测目标位置是否有骰子
        Collider2D[] hits = Physics2D.OverlapBoxAll(targetPos, Vector2.one * 0.5f, 0);
        foreach (var hit in hits)
        {
            DiceController dice = hit.GetComponent<DiceController>();
            if (dice != null)
            {
                dice.PushDice(inputDir);
            }
        }

        // 移动玩家
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
}