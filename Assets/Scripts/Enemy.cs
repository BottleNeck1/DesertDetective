using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Enemy : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpSpeed;
    [SerializeField] GameObject ball;
    [SerializeField] private float walkRange;
    [SerializeField] private float attackRange;
    [SerializeField] private EnemySounds soundsScript;
    [SerializeField] private int health = 2;
    
    private float timeSinceLastAttack = 0;

    public bool attack;

    private Rigidbody2D rb;
    private GameObject player;
    private Animator animator;
    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponent<Animator>();
        soundsScript = GetComponent<EnemySounds>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (player != null)
        {

            Vector2 dist = player.transform.position - transform.position;
            Vector2 dir = dist.normalized;
            if (dist.magnitude < attackRange && timeSinceLastAttack > 2)
            {
                attack = true;
                timeSinceLastAttack = 0;
            }
            if (dist.magnitude > walkRange)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            } else
            {
                if (dist.y > 1 && Mathf.Abs(dist.x) < 3 && Physics2D.Linecast(this.transform.position, this.transform.position + new Vector3(0, -1.1f, 0), ~(1 << 6)))
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                }

                rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);

            }

            animator.SetBool("FaceLeft", dist.x < 0);

        }

        animator.SetBool("Walking", rb.velocity.magnitude > 0);
        
        if (attack)
            Attack();
    }

    void Attack()
    {
        soundsScript.PlayAttackSound();
        animator.SetTrigger("Attack");

        GameObject ballInstance = Instantiate(ball, transform.position, transform.rotation);
        ballInstance.GetComponent<Ball>().SetDirection(animator.GetBool("FaceLeft") ? -1 : 1);
        ballInstance.GetComponent<Ball>().Go();
        
        attack = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        switch (other.tag)
        {
            case "PlayerBall":
                /*print(this + "  test");
                print(other + "   test2");*/
                Destroy(other.transform.root.gameObject);
                health -= other.GetComponent<harmful>().GetDamage();
                if (health <= 0)
                {
                    Destroy(this.transform.root.gameObject);
                }
                break;
        }
    }
}
