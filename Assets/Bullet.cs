using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAutoDestroy : MonoBehaviour
{

    void Start()
    {
        Destroy(gameObject, 2.5f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // 碰撞到任意物体后立即销毁
        Destroy(gameObject);
    }
}
