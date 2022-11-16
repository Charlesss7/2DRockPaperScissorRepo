using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CardPlayer : MonoBehaviour
{
    public Transform atkPosRef;
    public Card chosenCard;
    [SerializeField]private TMP_Text nameText;
    public TMP_Text healthText;

    public HealthBar healthBar;

    public float Health;
    public float MaxHealth;
    public PlayerStats stats = new PlayerStats
    {
        MaxHealth = 100,
        RestoreValue = 5,
        DamageValue = 10
    };

    public AudioSource audioSource;
    public AudioClip damageClip;
    public AudioClip drawClip;

    private Tweener animationTweener;

    public TMP_Text NickName {get=> nameText;}

    public bool IsReady=false;
    private void Start(){
        Health=stats.MaxHealth;
    }

    public void SetStats(PlayerStats newStats, bool restoreFullHealth = false)
    {
        this.stats = newStats;
        if(restoreFullHealth)
            Health = stats.MaxHealth;

        UpdateHealthBar();
    }
    public Attack? AttackValue
    {
        get=> chosenCard==null?null:chosenCard.AttackValue;
        // get
        // {
        //     if(chosenCard==null)
        //         return null;
        //     else
        //         return chosenCard.AttackValue;
        // }
    }

    public void Reset()
    {
        if(chosenCard != null)
        {
            chosenCard.Reset();
        }
        chosenCard = null;
    }

    public void SetChosenCard(Card newCard){
        if(chosenCard!=null)
        {
            chosenCard.transform.DOKill();
            chosenCard.Reset();
        }
        chosenCard=newCard;

        //problem?
        chosenCard.transform.DOScale(chosenCard.transform.localScale*1.2f,0.2f);
    }

    public void ChangeHealth(float amount)
    {
        Health += amount;
        Health = Mathf.Clamp(Health,0, stats.MaxHealth);
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        //healthbar
        healthBar.UpdateBar(Health/stats.MaxHealth);

        //text
        healthText.text = Health + "/" + stats.MaxHealth;

    }

    public void AnimateAttacks()
    {
        animationTweener = chosenCard.transform
            .DOMove(atkPosRef.position,1);
    }

    public void AnimateDamage()
    {
        audioSource.PlayOneShot(damageClip);
        var image =chosenCard.transform.GetComponent<Image>();
        animationTweener=image
            .DOColor(Color.black,0.1f)
            .SetLoops(3,LoopType.Yoyo)
            .SetDelay(0.2f);
    }
    public void AnimateDraw()
    {

            audioSource.PlayOneShot(drawClip);
            animationTweener=chosenCard.transform
                .DOMove(chosenCard.OriginalPosition,1)
                .SetEase(Ease.InBack)
                .SetDelay(0.2f);
    
    }

    public bool IsAnimating()
    {
        return animationTweener.IsActive();
    }

    public void isClickable(bool value)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        foreach(var card in cards)
        {
            card.SetClickable(value);
        }
    }
}
