using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A : MonoBehaviour
{
    public GameObject EndList;


    public void ShowList()
    {
        EndList.GetComponent<Animator>().SetTrigger("isEnd");
    }
}
