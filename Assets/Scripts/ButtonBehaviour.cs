using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public int face;
    public bool isPressed = false;
    private Animator anim;
    public GameObject LevelManager;
    private RunLevel runLevel;

    private void Awake()
    {
        runLevel = LevelManager.GetComponent<RunLevel>();
        anim = GetComponent<Animator>();
        transform.position = new Vector3(Mathf.Round(transform.position.x) - .5f, Mathf.Round(transform.position.y) + .5f, Mathf.Round(transform.position.z));
    }

    // 检查按钮该不该被按下
    public void CheckIfAnythingOnButton(Vector3 inputDir, string type) // 两种情况："enter":检查是否有东西要走上按钮，在isMoving为true时检测
                                                                       //           "leave":检查是否有东西要离开按钮，在isMoving为false时检测
    {
        if (type == "enter")
        {
            Collider2D hit = Physics2D.OverlapBox((Vector2)(transform.position - inputDir), Vector2.one * .5f, 0);
            if (hit != null)
            {
                if (hit.CompareTag("Dice") || hit.CompareTag("Player"))
                {
                    BePressed();
                }
            }
        }
       else if (type == "leave")
        {
            Collider2D hit = Physics2D.OverlapBox((Vector2)(transform.position ), Vector2.one * .5f, 0);
            if (hit == null)
            {
                
                BeNotPressed();
                
            }
        }


    }

    // 按钮的按压动画
    public void BePressed()
    {
        anim.SetBool("isPressed", true);
    }

    public void BeNotPressed()
    {
        anim.SetBool("isPressed", false);
    }
    
}
