using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Attack AttackValue;
    public CardPlayer player;
    public Transform atkPosRef;
    
    public Vector2 OriginalPosition;
    Vector2 originalScale;

    Color originalColor;

    bool isClickable=true;

    private void Start()
    {
        OriginalPosition = this.transform.position;
        originalScale = this.transform.localScale;
        originalColor = GetComponent<Image>().color;
    }
    public void OnClick()
    {
        if(isClickable)
        {
            OriginalPosition = this.transform.position;
            //scale kartu tidak keliatan wkt multiplayer?
            //originalScale = this.transform.localScale;
            player.SetChosenCard(this);
        }
    }


    internal void Reset()
    {
        transform.position=OriginalPosition;
        transform.localScale=originalScale;
        GetComponent<Image>().color=originalColor;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }
    
    private void Update()
    {
        
        
    }

    
}
