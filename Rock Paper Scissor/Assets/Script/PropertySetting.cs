using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
//solusi google
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using SystemHashtable = System.Collections.Hashtable;

public class PropertySetting : MonoBehaviourPunCallbacks
{
    [SerializeField]Slider slider;
    [SerializeField]TMP_InputField inputField;
    [SerializeField]string propertyKey;
    [SerializeField]float initialValue = 50;
    [SerializeField]float minValue = 0 ;
    [SerializeField]float maxValue = 100;
    [SerializeField]bool wholeNumbers = true;

    void Start()
    {
        //setup semua ui
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        inputField.contentType = wholeNumbers ? TMP_InputField.ContentType.IntegerNumber :
                                                TMP_InputField.ContentType.DecimalNumber;
        //ambil initial value dr server kl ada
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(propertyKey,out var value))
        {
            UpdateSliderInputField((float)value);
        }
        //kl tdk ada, smbil initial value dr inspector
        else
        {
            UpdateSliderInputField(initialValue);

            //kirim ke server
            SetCustomProperty(initialValue);
        }
        //UI interactable bg master
        if(PhotonNetwork.IsMasterClient==false)
        {
            slider.interactable=false;
            inputField.interactable=false;
        }
    }
    public void InputFromSlider(float value)
    {
        if(PhotonNetwork.IsMasterClient==false)
            return;
        UpdateSliderInputField(value);
        SetCustomProperty(value);
    }

    public void InputFromFIeld(string stringValue)
    {
        if(PhotonNetwork.IsMasterClient==false)
            return;
        if(float.TryParse(stringValue,out var floatValue))
        {
            floatValue = Mathf.Clamp(floatValue,slider.minValue,slider.maxValue);
            UpdateSliderInputField(floatValue);
            SetCustomProperty(floatValue);
        }
    }
    private void SetCustomProperty(float value)
    {
        if(PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        var property = new PhotonHashtable();
        property.Add(propertyKey,value);
        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.TryGetValue(propertyKey,out var value)&&
            PhotonNetwork.IsMasterClient==false)
        {
            UpdateSliderInputField((float)value);
        }
    }

    private void UpdateSliderInputField(float value)
    {
        var floatValue = (float)value;
        slider.value = floatValue;
        if(wholeNumbers)
            inputField.text = (Mathf.RoundToInt(floatValue)).ToString("D");
        else
            inputField.text = (floatValue.ToString("F2"));
    }
}
