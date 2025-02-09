using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


interface IButton
{
    HoldArea Player { get; set; }
    Vector3 Position { get; set; }
    GameObject TheButton { get; set; }
    void OnClick()
    {
        Player.Issue();
    }
}

public class MyButton : IButton
{
    public HoldArea Player { get; set; }
    public Vector3 Position { get; set; }
    public GameObject TheButton { get; set; }

    public MyButton(HoldArea player, Vector3 position)
    {
        Player = player;
        Position = position;

        TheButton = Object.Instantiate(Resources.Load<GameObject>("Prefab/IssueButton"));
        TheButton.transform.position = Position;
        TheButton.SetActive(false);
    }
}