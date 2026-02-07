using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Collections : MonoBehaviour
{
    //牌面
    public Sprite sprite;

    //卡牌名字以及卡牌描述
    public string name;
    public string issue;

    private Transform CardName;
    private TMP_Text cardNameText;
    private Transform CardIssue;
    private TMP_Text cardIssueText;
    private Transform Got;
    private Image myImage;
    public GameObject NextButton;


    private void Awake()
    {
        myImage = GetComponent<Image>();
        CardName = transform.Find("cardName");
        CardIssue = transform.Find("cardIssue");
        Got = transform.Find("Got");
        cardNameText = CardName.GetComponent<TMP_Text>();
        cardIssueText = CardIssue.GetComponent<TMP_Text>();
    }



    public void GetCard()
    {
        myImage.sprite = sprite;
        cardNameText.text = name;
        cardIssueText.text = issue;
       
}
   
    public void ShowGot()
    {
        Got.transform.DOScale(Vector3.one, .2f);
        Got.transform.DOShakePosition(2.5f, 7);
    }

    public void ShowName()
    {

     /*
            加上<color=red>就可以让这个标识符之后的所有文本变红，相应的，在句段末尾加上</color>就可以把颜色变回去     
     */
        CardName.transform.DOScale(Vector3.one, .2f);
        CardName.transform.DOShakePosition(1, 7);
    }

    public void ShowNextButton()
    {
        NextButton.SetActive(true);
    }
}
