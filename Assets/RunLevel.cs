using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunLevel : MonoBehaviour
{
    private List<Transform> Dice = new List<Transform>(); 

    public bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        Listing("Dice", Dice);
    }

    // Update is called once per frame
    private void Update()
    {
        if (isMoving)
        {

        }
        else
        {
            ReleaseAllDice();
        }
    }


    private void Listing(string tag, List<Transform> lists)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject gameObject in gameObjects)
        {
            lists.Add(gameObject.transform);
        }
    }
    private void ReleaseAllDice()
    {
        foreach (Transform dice in Dice)
        {
            dice.SetParent(null, true);
        }
    }
}
