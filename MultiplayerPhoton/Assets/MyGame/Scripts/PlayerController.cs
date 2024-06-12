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
    public float bombCooldown = 2f; // Tempo de espera entre os lançamentos de bomba
    private float lastBombTime; // Tempo do último lançamento de bomba

    private NetWorkController networkController; // Referência ao NetWorkController

    void Start()
    {
        rigidB2D = GetComponent<Rigidbody2D>();
        photonVieww = GetComponent<PhotonView>();
        playerHealtCurrent = playerHealthMax; // Inicializa a saúde atual
        UpdateHealthUI(); // Atualiza a UI com a saúde inicial

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
            lastBombTime = Time.time; // Atualiza o tempo do último lançamento de bomba
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
        HealthManager(-value); // Subtrai o valor do dano da saúde

        if (playerHealtCurrent <= 0)
        {
            Debug.Log("***GAME OVER****");
            if (photonView.IsMine)
            {
                networkController.ShowRetry(); // Chama o método para mostrar o retry apenas para o jogador local
            }
            Destroy(this.gameObject);
        }
    }


    void HealthManager(float value)
    {
        playerHealtCurrent += value;
        playerHealtCurrent = Mathf.Clamp(playerHealtCurrent, 0, playerHealthMax); // Garante que a saúde não passe do máximo nem fique negativa
        UpdateHealthUI(); // Atualiza a UI com a nova saúde
    }

    void UpdateHealthUI()
    {
        playerHealthFill.fillAmount = playerHealtCurrent / playerHealthMax; // Atualiza a barra de saúde
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
