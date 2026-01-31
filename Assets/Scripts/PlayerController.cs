using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    [Header("攻击设置")]
    GameObject daoEffect;
    public bool canAttack = true;
    public Vector2 direction;
    private Vector2 mousePosition;
    public Transform attackPoint;
    [Header("移动设置")]
    public Vector2 inputDirection;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    [Header("转向")]
    public bool canRotate = true; //是否启用转向
    public bool smoothRotate = true;   //是否启用平滑转向
    public float rotateSpeed = 10f;   //平滑转向速度
    public float angleOffset = -90f;  //角度偏移
    private void Awake()
    {
        inputControl = new PlayerInputControl();
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        inputControl.Enable();
        inputControl.Player.MouseAttack.started += OnAttack;
    }
    private void OnDisable()
    {
        inputControl.Disable();
    }
    private void Update()
    {
        inputDirection = inputControl.Player.Move.ReadValue<Vector2>();

        // 归一化输入方向,斜向移动时速度和上下左右一致
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }
        if (inputDirection.magnitude > 0.01f&&canRotate ) // 避免微小输入导致的抖动
        {
            RotatePlayer(inputDirection);
        }

        // 使用新的Input System获取鼠标位置
        mousePosition = Mouse.current.position.ReadValue();

        // 将屏幕坐标转换为世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane)
        );
        worldPosition.z = 0;
        // 计算方向
        direction = (worldPosition - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        // 计算目标速度：输入方向 * 移动速度
        Vector2 targetVelocity = new Vector2(inputDirection.x * moveSpeed, inputDirection.y * moveSpeed);

        rb.velocity = targetVelocity;
    }
    private void RotatePlayer(Vector2 direction)
    {
        // 计算目标角度
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;

        if (smoothRotate)
        {
            // 平滑转向
            float currentAngle = transform.rotation.eulerAngles.z;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
        else
        {
            // 立即转向
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (canAttack)
        {
          
            if (transform.localScale.x < 0 && direction.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            if (transform.localScale.x > 0 && direction.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            Vector3 spawnPosition = attackPoint.position;
            // 实例化刀光特效
            GameObject effect = Instantiate(daoEffect, spawnPosition, Quaternion.identity);
            // 计算刀光朝向鼠标的角度[1,2]
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle -= 90f;

            effect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 特效播放完成后销毁
            Destroy(effect, 0.5f); // 根据特效实际长度调整时间
            canAttack = false;

        }
    }
}

