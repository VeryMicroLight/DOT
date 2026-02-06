using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveDuration = 0.2f;
    private PlayerInputControl inputControl;
    private RunLevel runLevel;
    public AudioSource MusicSource;
    public DiceController Dice;
    public Vector2 inputDir;
    public GameObject LevelManager;

    private void Awake()
    {
        runLevel = LevelManager.GetComponent<RunLevel>();
        //inputControl = LevelManager.GetComponent<PlayerInputControl>();
        inputControl = new PlayerInputControl();
        inputControl.Player.Move.performed += OnMovePerformed;
    }

    private void OnEnable() => inputControl.Enable();
    private void OnDisable() => inputControl.Disable();

    private void Start()
    {
        // 保留原有坐标修正
        DiceController.CorrectToGridCenter(transform);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!runLevel.isMoving)
        {
            Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
            if (inputDir.magnitude > 0.1f)
            {
                inputDir = GetSingleGridDirection(inputDir);
                runLevel.isMoving = true;
                StartCoroutine(MovePlayer(inputDir));
            }
        }
    }

    IEnumerator MovePlayer(Vector2 inputDir)
    {
        Vector2 startPos = transform.position;
        Vector2 playerTargetPos = startPos + inputDir; // 人物目标位置
        // 检测:人物自身目标位置是否有障碍物（Obstacle）
        if (IsPositionHasObstacle(playerTargetPos))
        {
            runLevel.isMoving = false; // 重置状态，避免人物卡住
            yield break; // 终止协程，禁止移动
        }

        // 保留原有骰子检测
        Collider2D[] hits = Physics2D.OverlapBoxAll(playerTargetPos, Vector2.one * 0.5f, 0);
        DiceController targetDice = null;
        foreach (var hit in hits)
        {
            targetDice = hit.GetComponent<DiceController>();
            if (targetDice != null)
            {
                break;
            }
        }


        // 若有骰子，检测骰子目标位置是否有障碍物

        if (targetDice != null)
        {
            Vector2 diceTargetPos = (Vector2)targetDice.transform.position + inputDir; // 骰子目标位置
            if (IsPositionHasObstacle(diceTargetPos))
            {
                runLevel.isMoving = false; // 重置状态
                yield break; // 终止协程，既不推骰子也不移动
            }
        }

        // 两层检测都通过.执行原有推骰子+人物移动逻辑
        if (targetDice != null)
        {
            MusicSource.Play();
        }

        // 原有人物移动插值动画
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            transform.position = Vector2.Lerp(startPos, playerTargetPos, t);
            yield return null;
        }

        // 原有坐标修正，消除浮点数误差
        transform.position = playerTargetPos;
        DiceController.CorrectToGridCenter(transform);

        runLevel.isMoving = false;
    }

    // 保留原有单格方向处理
    private Vector2 GetSingleGridDirection(Vector2 rawInput)
    {
        float absX = Mathf.Abs(rawInput.x);
        float absY = Mathf.Abs(rawInput.y);
        if (absX > absY)
        {
            return new Vector2(Mathf.Sign(rawInput.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(rawInput.y));
        }
    }

    // 通用障碍物检测方法（可复用给后续所有物体）
    // 检测指定位置是否有标签为Obstacle的物体
    private bool IsPositionHasObstacle(Vector2 checkPos)
    {
        // 用和骰子检测相同的尺寸，适配瓦片中心碰撞
        Vector2 inputDir = inputControl.Player.Move.ReadValue<Vector2>();
        Collider2D obstacleHit = Physics2D.OverlapBox(checkPos, Vector2.one * 0.5f, 0);
        if (obstacleHit)
        {
            if (obstacleHit.CompareTag("Obstacle") || obstacleHit.CompareTag("Door"))
            {
                return true;
            }
            else if (obstacleHit.CompareTag("Dice"))
            {
                obstacleHit.transform.SetParent(transform, true);
                if (!IsPositionHasObstacle(checkPos + inputDir))
                {
                    obstacleHit.GetComponent<DiceController>().PushDice(inputDir);
                }
                return IsPositionHasObstacle(checkPos + inputDir);

            }
            
            else
            {
                return false; // 无障碍物
            }
        }
        else
        {
            return false; // 无障碍物
        }
            
    }
}