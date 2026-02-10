using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class DoorBehaviour : MonoBehaviour
{
    public bool isOpen = false;
    public Sprite Unlock;
    private Tweener tweener;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D collider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
    }



    public IEnumerator Open()
    {
        spriteRenderer.sprite = Unlock;
        collider.enabled = false;
        isOpen = true;
        spriteRenderer.sortingOrder = 0;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(1.2f, .05f));
        sequence.Append(transform.DOScale(1f, .2f));
        yield return null;
    }
}
