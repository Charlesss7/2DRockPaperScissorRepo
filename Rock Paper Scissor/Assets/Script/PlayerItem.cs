using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField]Image avatarImage;
    [SerializeField]TMP_Text playerName;
    [SerializeField] Sprite[] avatarSprite;

    public void Set(Photon.Realtime.Player player)
    {
        if(player.CustomProperties.TryGetValue(PlayerPropertyNames.Player.AvatarIndex,out var value))
        {
            avatarImage.sprite = avatarSprite[(int)value];
        }

        playerName.text = player.NickName;
        if(player == PhotonNetwork.MasterClient)
            playerName.text += " (Master)";
    }
}
