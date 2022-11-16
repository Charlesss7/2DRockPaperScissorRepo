using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHastable = ExitGames.Client.Photon.Hashtable;

public class AvatarSelector : MonoBehaviour
{
    [SerializeField]Image avatarImage;
    [SerializeField]Sprite[] avatarSprites;
    
    private int selectedIndex;
    void Start()
    {
        selectedIndex = PlayerPrefs.GetInt(PlayerPropertyNames.Player.AvatarIndex,0);
        avatarImage.sprite = avatarSprites[selectedIndex];
        SaveSelectedIndex();
    }
    public void ShiftSelectedIndex(int shift)
    {
        selectedIndex += shift;
        // Debug.Log(selectedIndex);
        while(selectedIndex >= avatarSprites.Length)
        {
            selectedIndex -= avatarSprites.Length;
        }

        while(selectedIndex < 0)
        {
            selectedIndex += avatarSprites.Length;
        }
        avatarImage.sprite = avatarSprites[selectedIndex];

        SaveSelectedIndex();

    }

    public void SaveSelectedIndex()
    {
        //simpan local
        PlayerPrefs.SetInt(PlayerPropertyNames.Player.AvatarIndex,selectedIndex);

        //simpan di network
        var property = new PhotonHastable();
        property.Add(PlayerPropertyNames.Player.AvatarIndex,selectedIndex);
        PhotonNetwork.LocalPlayer.SetCustomProperties(property);

    }
}
