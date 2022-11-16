using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using DG.Tweening;

public class ShooterPlayer : MonoBehaviourPun
{
     [SerializeField]float maxHealth = 100;
    [SerializeField]float health = 100;
    [SerializeField]float restoreValue = 5;
    [SerializeField]float damageValue = 10;
    [SerializeField]float speed = 5;
    [SerializeField]TMP_Text playerName;
    [SerializeField]Rigidbody2D rb;
    [SerializeField]SpriteRenderer rend;
    [SerializeField]Sprite[] avatarSprites;
    // [SerializeField]Renderer rend;
    // [SerializeField]Texture[] avatarTexture;

    [SerializeField]ShooterBullet bulletPrefab;
    Vector2 moveDir;
    [SerializeField]Animator animator;

    void Start()
    {
        Debug.Log(health);
        playerName.text = photonView.Owner.NickName + $"({health})";
        if(photonView.Owner.CustomProperties.TryGetValue(PlayerPropertyNames.Player.AvatarIndex, out var avatarIndex))
        {
            rend.sprite = avatarSprites[(int)avatarIndex];
            // rend.material.mainTexture = avatarTexture[(int)AvatarIndex];
        }
        //set local property
        if(photonView.Owner.CustomProperties.TryGetValue(PlayerPropertyNames.Room.MaxHealth, out var roomMaxHealth))
        {
            this.maxHealth = (float)roomMaxHealth;
            this.health = this.maxHealth;
            playerName.text = photonView.Owner.NickName + $"({health})";
        }

        if(photonView.Owner.CustomProperties.TryGetValue(PlayerPropertyNames.Room.RestoreValue, out var roomRestoreValue))
        {
            this.restoreValue = (float)roomRestoreValue;
        }

        if(photonView.Owner.CustomProperties.TryGetValue(PlayerPropertyNames.Room.DamageValue, out var roomDamageValue))
        {
            this.damageValue = (float)roomDamageValue;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine == false)
            return;

        moveDir = new Vector2 (
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if(moveDir == Vector2.zero)
        {
            animator.SetBool("IsMove",false);
        }
        else{
            animator.SetBool("IsMove",true);
        }
        // transform.Translate(moveDir*Time.deltaTime*speed);

        if(Input.GetMouseButtonDown(0))
        {
             var mouseScreenPos = Input.mousePosition;
             var mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
             mouseScreenPos = new Vector3(mouseWorldPos.x,mouseWorldPos.y,0);
             var directionVector = mouseScreenPos - this.transform.position;
            
            Fire(this.transform.position,directionVector.normalized,new PhotonMessageInfo());
            
            photonView.RPC("Fire",RpcTarget.Others,
                            this.transform.position,
                            directionVector.normalized
                );
        }

        // if(Input.GetKeyDown(KeyCode.Space))
        //     photonView.RPC("TakeDamage",RpcTarget.All,1);
    }

    void FixedUpdate()
    {
        if(photonView.IsMine == false)
            return;
        rb.velocity = moveDir * speed;
    }

    [PunRPC]
    public void Fire(Vector3 position, Vector3 direction, PhotonMessageInfo info)
    {
        var lag = (float) (PhotonNetwork.Time - info.SentServerTime);
        if(photonView.IsMine)
            lag = 0;
        var bullet = Instantiate(bulletPrefab);
        bullet.Set(this,position,direction,lag);
    }

    [PunRPC]
    public void TakeDamage()
    {
        health -= damageValue;
        health = Mathf.Clamp(health,0,maxHealth);
        playerName.text = photonView.Owner.NickName +  $"({health})";
        // GetComponent<SpriteRenderer>().DOColor(Color.red,0.2f).SetLoops(1,LoopType.Yoyo).From();
        var sequence = DOTween.Sequence();
        sequence.Append(
            // rend.material.DOColor(Color.red,0.2f).SetLoops(1,LoopType.Yoyo)
            GetComponent<SpriteRenderer>().DOColor(Color.red,0.2f).SetLoops(1,LoopType.Yoyo)
        );
        sequence.Append(
            // rend.material.DOColor(Color.white,0.2f).SetLoops(1,LoopType.Yoyo)
            GetComponent<SpriteRenderer>().DOColor(Color.white,0.2f).SetLoops(1,LoopType.Yoyo)
        );
    }

    public void RestoreHealth()
    {
        if(photonView.IsMine)
            photonView.RPC("RestoreHealthRPC",RpcTarget.AllViaServer);
    }
    [PunRPC]
    public void RestoreHealthRPC()
    {
        health += restoreValue;
        health = Mathf.Clamp(health,0,maxHealth);

        playerName.text = photonView.Owner.NickName +  $"({health})";

        var sequence = DOTween.Sequence();
        sequence.Append(
            // rend.material.DOColor(Color.red,0.2f).SetLoops(1,LoopType.Yoyo)
            GetComponent<SpriteRenderer>().DOColor(Color.green,0.2f).SetLoops(1,LoopType.Yoyo)
        );
        sequence.Append(
            // rend.material.DOColor(Color.white,0.2f).SetLoops(1,LoopType.Yoyo)
            GetComponent<SpriteRenderer>().DOColor(Color.white,0.2f).SetLoops(1,LoopType.Yoyo)
        );
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Bullet")
        {
            if(photonView.IsMine)
            {
                photonView.RPC("TakeDamage",RpcTarget.AllViaServer);
            }
        }
    }
}
