using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class EndAnimations : MonoBehaviour
{
    public GameObject origin;
    public float radius = 5f;
    public float spawnPosZ = -200f;
    private int count = 0;

    private void Awake()
    {
        CardCreate();
        StartCoroutine(MoveAnim());
    }

    private void CardCreate()
    {
        for (int i = 0;i < 100;i++)
        {
            GameObject clone = Instantiate(origin);
            clone.transform.parent = transform;
            float angle = Random.Range(0, 360);
            clone.transform.position = new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad), spawnPosZ - i * 5);
            clone.transform.eulerAngles = new Vector3(-angle, clone.transform.eulerAngles.y, clone.transform.eulerAngles.z);
        }
    }

    public IEnumerator MoveAnim()
    {
        transform.DOMoveZ(522, 6f).SetEase(Ease.InOutQuad);
        transform.DORotate(new Vector3(0, 0, 1440), 6f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
        yield return null;
    }

}
        

