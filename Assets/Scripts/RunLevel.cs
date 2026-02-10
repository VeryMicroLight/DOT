using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunLevel : MonoBehaviour
{
    private List<Transform> Dice = new List<Transform>(); 
    private List<Transform> Buttons = new List<Transform>();

    public PlayerInputControl inputControl;
    public GameObject Player;
    //private PlayerController playerController;
    public Vector2 inputDir;
    public bool isMoving = false;
    public bool isWin = false;

    //public GameObject Button;
    public GameObject Card;
    private ButtonBehaviour buttonBehaviour;
    private CardBehavior cardBehaviour;
    // Start is called before the first frame update
    private void Awake()
    {
        //buttonBehaviour = Button.GetComponent<ButtonBehaviour>();
        cardBehaviour = Card.GetComponent<CardBehavior>();
        //playerController = Player.GetComponent<PlayerController>();
        inputControl = new PlayerInputControl();
        Listing("Dice", Dice);
        Listing("Button", Buttons);
        
    }

    
    // Update is called once per frame
    private void Update()
    {
        inputDir = inputControl.Player.Move.ReadValue<Vector2>();
        cardBehaviour.CardFloating();
        //buttonBehaviour.CheckIfAnyDiceOnButton();
        if (isMoving)
        {
            cardBehaviour.CheckIfIsWin();
            //buttonBehaviour.CheckIfAnythingOnButton(inputDir, "enter");
            CheckIfAnythingOnButton(inputDir, "enter");
        }
        else
        {
            //buttonBehaviour.CheckIfAnythingOnButton(inputDir, "leave");
            CheckIfAnythingOnButton(inputDir, "leave");
            ReleaseAllDice();
        }
    }


    private void Listing(string tag, List<Transform> lists)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
        if (gameObjects == null)
        {
            return;
        }
        foreach (GameObject gameObject in gameObjects)
        {
            lists.Add(gameObject.transform);
        }
    }

    private void CheckIfAnythingOnButton(Vector2 inputDir, string type)
    {
        foreach (Transform button in Buttons)
        {
            button.GetComponent<ButtonBehaviour>().CheckIfAnythingOnButton(inputDir, type);
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
