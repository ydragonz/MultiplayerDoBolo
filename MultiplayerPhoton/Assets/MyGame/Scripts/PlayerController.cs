using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun
{
    public float playerSpeed = 2f;
    Rigidbody2D rigidB2D;
    PhotonView photonVieww;

    [Header("Health")]
    public float playerHealthMax = 300f;
    float playerHealtCurrent;
    public Image playerHealthFill;

    [Header(" Bullet ")]
    public GameObject bulletGo;
    public GameObject bulletGoPhotonView;
    public GameObject spawBullet;

    [Header(" Bomb ")]
    public GameObject bombGo;
    public GameObject bombGoPhotonView;
    public float bombCooldown = 2f; // Tempo de espera entre os lan�amentos de bomba
    private float lastBombTime; // Tempo do �ltimo lan�amento de bomba

    private NetWorkController networkController; // Refer�ncia ao NetWorkController

    void Start()
    {
        rigidB2D = GetComponent<Rigidbody2D>();
        photonVieww = GetComponent<PhotonView>();
        playerHealtCurrent = playerHealthMax; // Inicializa a sa�de atual
        UpdateHealthUI(); // Atualiza a UI com a sa�de inicial

        // Encontrar o NetWorkController na cena
        networkController = FindObjectOfType<NetWorkController>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            PlayerMove();
            PlayerTurn();
            Shooting();
            Bombing();
        }
    }

    void Shooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PhotonNetwork.Instantiate(bulletGoPhotonView.name, spawBullet.transform.position, spawBullet.transform.rotation, 0);
        }
    }

    void Bombing()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastBombTime + bombCooldown)
        {
            lastBombTime = Time.time; // Atualiza o tempo do �ltimo lan�amento de bomba
            PhotonNetwork.Instantiate(bombGoPhotonView.name, spawBullet.transform.position, spawBullet.transform.rotation, 0);
        }
    }

    public void TakeDamage(float value)
    {
        photonView.RPC("TakeDamageNetWork", RpcTarget.AllBuffered, value);
    }

    [PunRPC]
    void TakeDamageNetWork(float value)
    {
        Debug.Log("Taking damage: " + value);
        HealthManager(-value); // Subtrai o valor do dano da sa�de

        if (playerHealtCurrent <= 0)
        {
            Debug.Log("***GAME OVER****");
            if (photonView.IsMine)
            {
                networkController.ShowRetry(); // Chama o m�todo para mostrar o retry apenas para o jogador local
            }
            Destroy(this.gameObject);
        }
    }


    void HealthManager(float value)
    {
        playerHealtCurrent += value;
        playerHealtCurrent = Mathf.Clamp(playerHealtCurrent, 0, playerHealthMax); // Garante que a sa�de n�o passe do m�ximo nem fique negativa
        UpdateHealthUI(); // Atualiza a UI com a nova sa�de
    }

    void UpdateHealthUI()
    {
        playerHealthFill.fillAmount = playerHealtCurrent / playerHealthMax; // Atualiza a barra de sa�de
    }

    void PlayerMove()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

        rigidB2D.velocity = new Vector2(x, y) * playerSpeed;
    }

    void PlayerTurn()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);
        transform.up = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Barriers"))
        {
            TakeDamage(500);
        }
    }
}
