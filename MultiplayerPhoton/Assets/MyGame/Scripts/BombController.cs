using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombController : MonoBehaviour
{
    public float bombSpeed = 100f;
    Rigidbody2D rigidB2D;

    public float bombDamage = 100;
    public GameObject explosionPrefab;

    [Header("Bomb")]
    public GameObject explosionGoPhotonView;

    void Start()
    {
        rigidB2D = GetComponent<Rigidbody2D>();
        rigidB2D.AddForce(transform.up * bombSpeed, ForceMode2D.Force);
    }

    [PunRPC]
    void BombExplode()
    {
        PhotonNetwork.Instantiate(explosionGoPhotonView.name, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player_ID " + collision.GetComponent<PhotonView>().Owner.ActorNumber + " Player_Name " + collision.GetComponent<PhotonView>().Owner.NickName);
            this.GetComponent<PhotonView>().RPC("BombExplode", RpcTarget.AllViaServer);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Barriers"))
        {
            this.GetComponent<PhotonView>().RPC("BombExplode", RpcTarget.AllViaServer);
        }
    }
}
