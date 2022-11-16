using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CardNetPlayer : MonoBehaviourPun
{
    public static List<CardNetPlayer> netPlayers= new List<CardNetPlayer>(2);
    // private CardPlayer cardPlayer;

    private Card[] cards;
    public void Set(CardPlayer player)
    {
        //ambil image
        //PhotonView ..............
        player.NickName.text = photonView.Owner.NickName;
        cards = player.GetComponentsInChildren<Card>();
        foreach(var card in cards)
        {
            var button = card.GetComponent<Button>();
            button.onClick.AddListener(()=>RemoteClickButton(card.AttackValue));

            if(photonView.IsMine == false)
            {
                button.interactable = false;
            }
        }
    }  

    void OnDestroy()
    {
        foreach(var card in cards)
        {
            var button = card.GetComponent<Button>();
            button.onClick.RemoveListener(()=>RemoteClickButton(card.AttackValue));
        }
    }

    private void RemoteClickButton(Attack value)
    {
        if(photonView.IsMine)
        {
            photonView.RPC("RemoteClickButtonRPC",RpcTarget.Others,(int)value);
        }
    }

    [PunRPC]
    private void RemoteClickButtonRPC(int value)
    {
        foreach(var card in cards)
        {
            if(card.AttackValue==(Attack)value)
            {
                var button = card.GetComponent<Button>();
                button.onClick.Invoke();
                break;
            }
        }
    }
    private void OnEnable()
    {
        netPlayers.Add(this);
    }

    private void OnDisable()
    {
        foreach (var card in cards)
        {
            if(card==null)
                continue;
            var button = card.GetComponent<Button>();
            button.onClick.RemoveListener(()=>RemoteClickButton(card.AttackValue));
        }
        netPlayers.Remove(this);
    }
}
