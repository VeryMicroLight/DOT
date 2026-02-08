using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RotateImageCoroutine : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20f;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // 启动旋转协程
        StartCoroutine(RotateContinuously());
    }
    
    IEnumerator RotateContinuously()
    {
        while (true)
        {
            rectTransform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null; // 等待下一帧
        }
    }
}