using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpPower = 15f;

    [SerializeField] private ColorSO playerColorSO;

    private Rigidbody2D rigid;

    private bool isFirstTouch = false;
    private bool canReverse = true;
    private float coolTime = 0.5f; 

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rigid.gravityScale = 0;
        ChangeColor();  
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        if(Input.GetMouseButtonDown(0) && isFirstTouch)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpPower);
        }
        else if(Input.GetMouseButtonDown(0) && !isFirstTouch)
        {
            isFirstTouch = true;
            rigid.gravityScale = 5;
            rigid.velocity = new Vector2(speed, jumpPower);
        }
    }

    private void ChangeColor()
    {
        int randIdx = 0;

        if (GameManager.Instance.StageUpgrade) randIdx = Random.Range(0, MapManager.Instance.CurrentWallsCnt - 1);
        else randIdx = Random.Range(0, MapManager.Instance.CurrentWallsCnt);

        gameObject.layer = playerColorSO.RandomMapList[randIdx].layerMask;
        gameObject.transform.Find("Visual").GetComponent<Renderer>().material.color = playerColorSO.RandomMapList[randIdx].color;
    }

    private void Reverse()
    {
        if (GameManager.Instance.IsGameOver) return;

        float x = -Mathf.Sign(rigid.velocity.x);

        if (x < 0) 
        {
            if(GameManager.Instance.StageUpgrade)
            {
                MapManager.Instance.DestroyRightWalls();
                GameManager.Instance.CheckUpgrade();
            }
            else MapManager.Instance.ChangeColorRight();
        }
        else if (x > 0)
        {
            if (GameManager.Instance.StageUpgrade)
            {
                MapManager.Instance.DestroyLeftWalls();
                GameManager.Instance.CheckUpgrade();
            }
            else MapManager.Instance.ChangeColorLeft();
        }

        rigid.velocity = new Vector2(x * speed, rigid.velocity.y);
        ChangeColor();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") && canReverse)
        {
            if(collision.gameObject.layer == this.gameObject.layer)
            {
                GameManager.Instance.ScoreUpdate();
            }
            else
            {
                //print("다른레이어"); // -> GameOver
                //GameOver();
            }

            StartCoroutine(ReverseCor());
            //print(LayerMask.LayerToName(collision.gameObject.layer));
        }
        else if(collision.CompareTag("GameOver"))
        {
            GameOver();
        }
    }
    private void GameOver()
    {
        rigid.gravityScale = 0;
        rigid.velocity = Vector2.zero; 
        GameManager.Instance.GameOver();
    }

    private IEnumerator ReverseCor()
    {
        canReverse = false;
        yield return new WaitForSeconds(0.01f);
        Reverse();
        yield return null;
        yield return new WaitForSeconds(coolTime);
        canReverse = true;
    }

    public void AddSpeed()
    {
        rigid.gravityScale += 0.3f;
    }

    public void ResetPlayer()
    {
        this.transform.position = Vector3.zero;
        StopAllCoroutines();
        isFirstTouch = false;
        canReverse = true;
        ChangeColor();
    }
}
