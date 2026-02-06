using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class CardBehavior : MonoBehaviour
{
    // 卡牌浮动参数
    public float singleSideDuration = .3f;     //半周期
    public float A ;                           //振幅
    private float originY;                     //平衡点

    //DOTween
    private Tweener tweener;
    

    public GameObject LevelManager;
    private RunLevel runLevel;

    public Animator ShowCardAnimator;
    public Animator BGCanvasAnimator;
    public AudioSource CollectedSound;
    
    // Start is called before the first frame update
    void Start()
    {
        runLevel = LevelManager.GetComponent<RunLevel>();
        originY = transform.position.y;
        transform.position = new Vector3(Mathf.Round(transform.position.x) - .5f, Mathf.Round(transform.position.y) + .25f);
    }

    // Update is called once per frame
    public void CardFloating()
    {
        if (!runLevel.isWin)
        {
            //float originY = transform.position.y;
            float timer = 0;
            float w = Mathf.PI / singleSideDuration;
            timer += Time.deltaTime;
            transform.position = new Vector3(transform.position.x, A * Mathf.Sin(w * Time.fixedTime) + originY);
            if (timer >= 1)
            {
                timer = 0;
            }
        }
        else
        {
            return;
        }
    }

    public void CheckIfIsWin()
    {
        if (!runLevel.isWin)
        {
            Collider2D hit = Physics2D.OverlapBox((Vector2)transform.position, Vector2.one * .2f, 0);
            if (hit)
            {
                if (hit.CompareTag("Player"))
                {
                    runLevel.isWin = true;
                    StartCoroutine(AfterCollected());
                    StopCoroutine(AfterCollected());
                }
            }
        }
        
    }
    
    private IEnumerator AfterCollected()
    {
        StartCoroutine(WinAnim());
        yield return new WaitForSeconds(.6f);
        StartCoroutine(ShowCardAnim());
        yield return new WaitForSeconds(.3f);
        yield return null;
    }

    private IEnumerator WinAnim()
    {
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(originY - .5f, .2f));
        sequence.Append(transform.DOMoveY(originY + 10f, .4f));
        CollectedSound.Play();
        yield return new WaitForSeconds(.6f);
        yield return null;
    }


    
    private IEnumerator ShowCardAnim()
    {
        /*
        在CardCanvas的_card脚本里有关于如何给文本上色，以及编辑卡牌相关文本的方法
        */
        ShowCardAnimator.SetTrigger("Show");
        BGCanvasAnimator.SetTrigger("Do");
        yield return null;
    }
}
