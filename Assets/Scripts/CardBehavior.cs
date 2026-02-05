using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class CardBehavior : MonoBehaviour
{
    public float singleSideDuration = .3f;
    public float A ;
    private Tweener tweener;
    private float originY;

    public GameObject LevelManager;
    private RunLevel runLevel;

    public Animator ShowCardAnimator;
    
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
        yield return new WaitForSeconds(.6f);
        yield return null;
    }

    private IEnumerator ShowCardAnim()
    {
        ShowCardAnimator.SetTrigger("Show");
        yield return null;
    }
}
