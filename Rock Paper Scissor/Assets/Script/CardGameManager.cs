using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CardGameManager : MonoBehaviour, IOnEventCallback
{
    public GameObject netPlayerPrefab;
    public CardPlayer P1;
    public CardPlayer P2;

    public PlayerStats defaultPlayerStats = new PlayerStats
    {
        MaxHealth = 100,
        RestoreValue = 5,
        DamageValue = 10
    };
    // public float restoreValue = 5;
    // public float damageValue = 30;
    private CardPlayer damagedPlayer;
    
    private CardPlayer winner;
    public GameState State,NexState = GameState.NetPlayersInitialization ;//= GameState.ChooseAttack;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text pingText;

    public bool Online=true;
    //untuk offline di gamemanager -> state & next state diubah ke choose attack langsung
    //ide : hidupkan script bot?


    // public List<int> syncReadyPlayers = new List<int>(2);
    HashSet<int> syncReadyPlayers = new HashSet<int>();
    public enum GameState
    {
        SyncState,
        NetPlayersInitialization,
        ChooseAttack,
        Attacks,
        Damages,
        Draw,
        GameOver,
    }

    
    private void Start()
    {
        gameOverPanel.SetActive(false);

         if(Online)
         {
            PhotonNetwork.Instantiate(netPlayerPrefab.name,Vector3.zero,Quaternion.identity);
            StartCoroutine(PingCoroutine());
            State = GameState.NetPlayersInitialization;
            NexState = GameState.NetPlayersInitialization;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PlayerPropertyNames.Room.RestoreValue,out var restoreValue))
            {
                defaultPlayerStats.RestoreValue = (float) restoreValue;
            }
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PlayerPropertyNames.Room.DamageValue,out var damageValue))
            {
                defaultPlayerStats.DamageValue = (float) damageValue;
            }
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PlayerPropertyNames.Room.MaxHealth,out var maxHealth))
            {
                defaultPlayerStats.MaxHealth = (float) maxHealth;
            }
        }
         else
         {
                State = GameState.ChooseAttack;
         }

         P1.SetStats(defaultPlayerStats,true);
         P2.SetStats(defaultPlayerStats,true);
         P1.IsReady = true;
         P2.IsReady = true;
    }

    private void Update()
    {
        switch(State)
        {
            case GameState.SyncState:
                if(syncReadyPlayers.Count ==2)
                {
                    syncReadyPlayers.Clear();
                    State = NexState;
                }
                break;
            case GameState.NetPlayersInitialization:
                if(CardNetPlayer.netPlayers.Count == 2)
                {
                    foreach(var netplayer in CardNetPlayer.netPlayers)
                    {
                        if(netplayer.photonView.IsMine)
                        {
                            netplayer.Set(P1);
                        }
                        else{
                            netplayer.Set(P2);
                        }
                    }
                    ChangeState(GameState.ChooseAttack);
                }
                break;
            case GameState.ChooseAttack:
                if(P1.AttackValue!=null && P2.AttackValue!=null)
                {
                    P1.AnimateAttacks();
                    P2.AnimateAttacks();
                    P1.isClickable(false);
                    P2.isClickable(false);
                    ChangeState(GameState.Attacks);
                }
                break;
            case GameState.Attacks:
                if(P1.IsAnimating()==false && P2.IsAnimating()==false)
                {
                    damagedPlayer = GetDamagedPlayer();
                    if(damagedPlayer!=null)
                    {
                        damagedPlayer.AnimateDamage();
                        ChangeState(GameState.Damages);
                    }
                    else 
                    {
                        P1.AnimateDraw();
                        P2.AnimateDraw();
                        ChangeState(GameState.Damages);
                    }
                }
                break;
            case GameState.Damages:
                if(P1.IsAnimating()==false && P2.IsAnimating()==false)
                {
                    //Calculate Health
                    if(damagedPlayer==P1)
                    {
                        //P2 Nyerang
                        P1.ChangeHealth(-P2.stats.DamageValue);
                        P2.ChangeHealth(P2.stats.RestoreValue);
                    }
                    else
                    {
                        //P1 nyerang
                        P1.ChangeHealth(P1.stats.RestoreValue);
                        P2.ChangeHealth(-P1.stats.DamageValue);
                    }
                    
                    var winner = GetWinner();

                    if(winner==null)
                    {
                        ResetPlayer();
                        P1.isClickable(true);
                        P2.isClickable(true);
                        ChangeState(GameState.ChooseAttack);
                    }
                    else
                    {
                        //Debug.Log(winner +" is win");
                        gameOverPanel.SetActive(true);
                        winnerText.text = winner == P1 ? $"{P1.NickName.text} wins": $"{P2.NickName.text} wins";
                        ResetPlayer();
                       ChangeState(GameState.GameOver);
                    }
                }
                break;
            case GameState.Draw:
                if(P1.IsAnimating()==false && P2.IsAnimating()==false)
                {
                    ResetPlayer();
                    P1.isClickable(true);
                    P2.isClickable(true);
                    ChangeState(GameState.ChooseAttack);
                }
                break;
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private const byte playerChangeState=1;

    private void ChangeState(GameState nextState)
    {
        if(Online==false)
         {
             State = nextState;
             return;
         }
        if(this.NexState == nextState)
            return;
        //kirim msg ready
        var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        var raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(playerChangeState,actorNum,
                                raiseEventOptions,SendOptions.SendReliable);
        
        //msk ke state sync sbg transisi sblm state baru
        this.State = GameState.SyncState;
        this.NexState = nextState;

        // if(syncReadyPlayers.Contains(actorNum)==false)
        // {
        //     syncReadyPlayers.Add(actorNum);
        // }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch(photonEvent.Code)
        {
            case playerChangeState:
                var actorNum = (int) photonEvent.CustomData;
        
                // if(syncReadyPlayers.Contains(actorNum)==false)

                //kalau pakai HashSet tidak perlu pengecekan dgn If
                syncReadyPlayers.Add(actorNum);
                 
                break;
            default:
                break;

        }
       
    }

    IEnumerator PingCoroutine()
    {
        var wait = new WaitForSeconds(1);
        while(true)
        {
            pingText.text = "ping: " + PhotonNetwork.GetPing()+" ms";
            yield return wait;
        }
    }

    private void ResetPlayer()
    {
        damagedPlayer = null;
        P1.Reset();
        P2.Reset();
    }

    private CardPlayer GetDamagedPlayer()
    {
        Attack? PlayerAtk1 = P1.AttackValue;
        Attack? PlayerAtk2 = P2.AttackValue;

        if(PlayerAtk1==Attack.Rock&&PlayerAtk2==Attack.Paper)
        {
            return P1;
        }
        else if(PlayerAtk1==Attack.Rock&&PlayerAtk2==Attack.Scissor)
        {
            return P2;
        }
        else if(PlayerAtk1==Attack.Paper&&PlayerAtk2==Attack.Rock)
        {
            return P2;
        }
        else if(PlayerAtk1==Attack.Paper&&PlayerAtk2==Attack.Scissor)
        {
            return P1;
        }
        else if(PlayerAtk1==Attack.Scissor&&PlayerAtk2==Attack.Rock)
        {
            return P1;
        }
        else if(PlayerAtk1==Attack.Scissor&&PlayerAtk2==Attack.Paper)
        {
            return P2;
        }
        return null;
    }

    private CardPlayer GetWinner()
    {
        if(P1.Health==0)
        {
            return P2;
        }
        else if(P2.Health==0)
        {
            return P1;
        }
        else
        {
            return null;
        }
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void QuitGame(){
        Application.Quit();
    }

}
