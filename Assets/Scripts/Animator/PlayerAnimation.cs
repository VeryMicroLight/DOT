using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public RunLevel runLevel;
    public Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        SetAnimation();
    }

    private void SetAnimation()
    {
        anim.SetBool("isMoving",runLevel.isMoving);
    }
}
