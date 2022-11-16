using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShooterGameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    void Start()
    {
        var randomViewportPos = new Vector2(
            Random.Range(0.2f,0.8f),
            Random.Range(0.2f,0.8f)
        );
        var randomWorldPos=Camera.main.ViewportToWorldPoint(randomViewportPos);
        randomWorldPos = new Vector3(randomWorldPos.x,randomWorldPos.y,0);
        PhotonNetwork.Instantiate(
            playerPrefab.name,
            Vector2.zero,
            Quaternion.identity
            );
    }
}
   
