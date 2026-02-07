using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    public RawImage img;
    public float dx, dy;


    public void Update()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(dx, dy) * Time.deltaTime, img.uvRect.size);
    }
}
