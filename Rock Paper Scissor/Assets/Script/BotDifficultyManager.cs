using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;


public class BotDifficultyManager : MonoBehaviour
{
    [SerializeField]Bot bot;
    [SerializeField]int selectedDifficulty;
    [SerializeField]BotStats[] botsDifficulties;

    [Header("Remote Config Parameters: ")]
    [SerializeField]bool enableRemoteConfig=false;
    [SerializeField]string difficultyKey ="Difficulty";

    struct userAttribute{};
    struct appAtrribute{};
    IEnumerator Start()
    {
        // tunggu bot selesai set up
        yield return new WaitUntil(()=> bot.IsReady);

        //set stat default dari difficulty manager
        //sesuai selected difficulty dr inspector
        var newStats = botsDifficulties[selectedDifficulty];
        bot.SetStats(newStats,true);

        //ambil difficulty dari remote config kl enabled
        if(enableRemoteConfig == false)
            yield break;
        
        // tapi tunggu s.d Unity Services siap
        yield return new WaitUntil(
            ()=> 
            UnityServices.State == 
            ServicesInitializationState.Initialized
            &&
            AuthenticationService.Instance.IsSignedIn
            );
        
        //daftar dulu untuk event fetch completed
        RemoteConfigService.Instance.FetchCompleted += OnRemoteConfigFetched;

        //lalu fetch disini, cukup 1x di awal permainan
        RemoteConfigService.Instance.FetchConfigsAsync(
            new userAttribute(),new appAtrribute());
    }

    void OnDestroy()
    {
        //unregister event untuk menghindari memory leak
        RemoteConfigService.Instance.FetchCompleted -= OnRemoteConfigFetched;
    }

    //setiap data baru didapatkan (via fetch) fungsi ini akan dipanggil
    void OnRemoteConfigFetched(ConfigResponse response)
    {
        if(RemoteConfigService.Instance.appConfig.HasKey(difficultyKey)==false)
        {
            Debug.LogWarning($"Difficulty Key: {difficultyKey} not found on remote config server");
            return;
        }
        
        switch(response.requestOrigin)
        {
            case ConfigOrigin.Default:
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
                selectedDifficulty = RemoteConfigService.Instance.appConfig.GetInt(difficultyKey);
                selectedDifficulty = Mathf.Clamp(selectedDifficulty,0,botsDifficulties.Length-1);
                var newStats = botsDifficulties[selectedDifficulty];
                bot.SetStats(newStats,true);
                break;
        }
    }
    
}
