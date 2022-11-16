using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mixer;
    
    public void Volume(float value)
    {
        value = value*80 - 80;

        mixer.SetFloat("SFX_VOL",value);
    }
}
