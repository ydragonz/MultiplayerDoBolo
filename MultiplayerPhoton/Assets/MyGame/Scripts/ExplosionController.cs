using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public float explosionRadius = 1f;
    public float explosionDamage = 50f;

    [PunRPC]
    void ExplosionDestroy()
    {
        Destroy(this.gameObject);
    }

    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D nearbyObject in colliders)
        {
            // Log para verificação
            float distanceToExplosion = Vector2.Distance(transform.position, nearbyObject.transform.position);
            Debug.Log("Detected object: " + nearbyObject.name + " at distance: " + distanceToExplosion);

            if (nearbyObject.CompareTag("Player") && nearbyObject.GetComponent<PlayerController>())
            {
                if (distanceToExplosion <= explosionRadius)
                {
                    Debug.Log("Applying damage to Player_ID " + nearbyObject.GetComponent<PhotonView>().Owner.ActorNumber + " Player_Name " + nearbyObject.GetComponent<PhotonView>().Owner.NickName);
                    nearbyObject.GetComponent<PlayerController>().TakeDamage(explosionDamage);
                }
            }
        }

        this.GetComponent<PhotonView>().RPC("ExplosionDestroy", RpcTarget.AllViaServer);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Explode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
