using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public int face = 1;
    public bool rightPressed = false;
    
    private Animator anim;
    public GameObject LevelManager;
    public GameObject[] TargetDoors;
    public Sprite RightFaceImage;
    private Transform RightFace;
    private RunLevel runLevel;

    private void Awake()
    {
        runLevel = LevelManager.GetComponent<RunLevel>();
        anim = GetComponent<Animator>();
        transform.position = new Vector3(Mathf.Round(transform.position.x) - .5f, Mathf.Round(transform.position.y) + .5f, Mathf.Round(transform.position.z));
        RightFace = transform.Find("rightImage");
        RightFace.GetComponent<SpriteRenderer>().sprite = RightFaceImage;
    }

    // 检查按钮该不该被按下
    public void CheckIfAnythingOnButton(Vector3 inputDir, string type) // 两种情况："enter":检查是否有东西要走上按钮，在isMoving为true时检测
                                                                       //           "leave":检查是否有东西要离开按钮，在isMoving为false时检测
    {
        if (type == "enter" && !rightPressed)
        {
            Collider2D hit = Physics2D.OverlapBox((Vector2)(transform.position - inputDir), Vector2.one * .5f, 0);
            if (hit != null)
            {
                if (hit.CompareTag("Dice") || hit.CompareTag("Player"))
                {
                    BePressed();
                    if (hit.CompareTag("Dice"))
                    {
                        Debug.Log(hit.GetComponent<DiceController>().TopSideNumber());
                        if (hit.GetComponent<DiceController>().TopSideNumber() == face)
                        {
                            rightPressed = true;
                            OpenTheDoors();
                        }
                    }
                    
                }
            }
        }
       else if (type == "leave" && !rightPressed)
        {
            rightPressed = false;
            Collider2D hit = Physics2D.OverlapBox((Vector2)(transform.position ), Vector2.one * .5f, 0);
            if (hit == null)
            {
                
                BeNotPressed();
                
            }
        }


    }


    //开门
    private void OpenTheDoors()
    {
        foreach (GameObject door in TargetDoors)
        {
            StartCoroutine(door.GetComponent<DoorBehaviour>().Open());
            StopCoroutine(door.GetComponent<DoorBehaviour>().Open());
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
