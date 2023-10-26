using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 MovementInput = new Vector2();
    private Vector2 MovementDirection = new Vector2();
    [SerializeField, Tooltip("standard movement speed")] float MovementSpeed = 10;
    [SerializeField, Tooltip("dashing movement speed")] float DashSpeed = 20;
    [SerializeField, Tooltip("the max vertical move speed")] float maxFallSpeed = 10;
    [SerializeField, Tooltip("The force applied during a jump")] float JumpForce = 20;
    [SerializeField, Tooltip("The duration of a dash")] float DashDuration = 0.2f;
    [SerializeField, Tooltip("The cooldown of a dash")] float DashCooldown = 0.5f;
    [SerializeField, Tooltip("The cooldown of shooting")] float ShootCooldown = 3.0f;
    [SerializeField, Tooltip("The cooldown of cloaking")] float CloakCooldown = 5.0f;
    [SerializeField] GameObject ball;
    private Animator animator;
    bool isGrounded = false;
    bool AirJumpReady = true;
    bool DashReady = false;
    bool ShootReady = false;
    bool CloakReady = false;
    bool inCloak = false;
    float dashTime = 0;
    float dashRefresh = 0;
    float bounceTime = 0;
    float shootRefresh = 0;
    float cloakRefresh = 0;
    private bool ridingMovingPlatform = false;
    private bool CanUnlock = false;

    private bool DoubleJumpUnlock = false;
    private bool DashUnlock = false;
    private bool ShootUnlock = false;
    private bool CloakUnlock = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Vector2 dist = this.transform.position - transform.position;
        //animator.SetBool("FaceLeft", dist.x < 0);
        //  rb.AddForce(MovementInput * MovementSpeed, ForceMode2D.Force);
        if (dashTime > 0)
        {
            rb.AddForce(new Vector2(MovementDirection.x * DashSpeed * 10, 0));
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -DashSpeed, DashSpeed), Mathf.Clamp(rb.velocity.y, 0, maxFallSpeed));
            dashTime -= Time.deltaTime;
        }
        else
        {
            dashTime = 0;
            MovementDirection.x = MovementInput.x;

            rb.AddForce(new Vector2(MovementDirection.x * MovementSpeed * 5, 0));

            if (bounceTime > 0)
                bounceTime -= Time.deltaTime;
            else
            {
                bounceTime = 0;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxFallSpeed, Mathf.Infinity));

                // Slow player significantly when not moving
                if (MovementInput.x == 0 || Mathf.Abs(rb.velocity.x) > MovementSpeed)
                    rb.AddForce(new Vector2(-rb.velocity.x * 5, 0));
            }
        }


        RaycastHit2D groundedBox = Physics2D.Linecast(this.transform.position, this.transform.position + new Vector3(0, -1.1f, 0), ~(1 << 3));
        Debug.DrawLine(this.transform.position, this.transform.position + new Vector3(0, -1.1f, 0), Color.red);
        isGrounded = groundedBox;
        if (isGrounded)
        {
            Moving_Platform platformScript = groundedBox.transform.GetComponentInChildren<Moving_Platform>();
            if (platformScript)
            {
                transform.parent = platformScript.GetParentTransform();
                ridingMovingPlatform = true;
            }
            else
            {
                if (ridingMovingPlatform)
                {
                    ridingMovingPlatform = false;
                    transform.parent = null;
                }
            }
            AirJumpReady = true;
        }
        else
        {
            if (ridingMovingPlatform)
            {
                ridingMovingPlatform = false;
                transform.parent = null;
            }
        }
        //note from max - Carson F Cole bad at video games!
        //mote from max 2 - we need to vote to end the VGDC more often, Jack can only vote to keep it around so many times

        if (dashRefresh > 0)
            dashRefresh -= Time.deltaTime;
        else if (isGrounded)
            DashReady = true;

        if(shootRefresh > 0)
            shootRefresh -= Time.deltaTime;
        else ShootReady = true;

        if (cloakRefresh > 0)
            cloakRefresh -= Time.deltaTime;
        else
        {
            CloakReady = true;
            inCloak = false;
        }
    }

    public void Bounce(Vector2 forceVector)
    {
        ResetDash();
        ResetAirJump();
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -MovementSpeed, MovementSpeed), rb.velocity.y);

        // If the bounce gives significant upward velocity, reset vertical speed first
        if (forceVector.normalized.y > 0.3)
            rb.velocity = new Vector2(rb.velocity.x, 0);

        rb.AddForce(forceVector, ForceMode2D.Impulse);
        bounceTime = 0.1f;
    }

    public void ResetDash()
    {
        dashTime = 0;
        DashReady = true;
    }

    public void ResetAirJump()
    {
        AirJumpReady = true;
    }

    public float GetCloakRefresh()
    {
        return cloakRefresh;
    }
    
    public void Move(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        MovementInput.x = input.x;

        if (Mathf.Abs(MovementInput.x) > 0)
        {
            animator.SetBool("Moving", true);
            if (MovementInput.x > 0)
            {
                animator.SetBool("FacingRight", true);
            } else
            {
                animator.SetBool("FacingRight", false);
            }
        } else
        {
            animator.SetBool("Moving", false);
        }
        
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (isGrounded && ctx.started)
        {
            rb.AddForce(new Vector2(0, 1) * JumpForce, ForceMode2D.Impulse);
        }
        else if (AirJumpReady && ctx.started && DoubleJumpUnlock)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0, 1) * JumpForce, ForceMode2D.Impulse);
            AirJumpReady = false;
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (DashReady && ctx.started && DashUnlock)
        {
            if (MovementDirection.x == 0)
                MovementDirection.x = animator.GetBool("FacingRight") ? 1 : -1;

            DashReady = false;
            dashTime = DashDuration;
            dashRefresh = DashCooldown;
        }
    }

    public void Interact(InputAction.CallbackContext ctx)
    {        
        if (ctx.started)
        {
            CanUnlock = GetComponent<PlayerInteract>().GetCanUnlock();
            if (CanUnlock)
            {
                GameObject other = GetComponent<PlayerInteract>().GateReturn();    

                GetComponent<PlayerInteract>().RemoveMoney();
                Destroy(other, 0.10f);
            }
        }
        else if (ctx.canceled)
        {
            CanUnlock = GetComponent<PlayerInteract>().GetCanUnlock();
        }
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started && ShootReady && ShootUnlock)
        {
            //soundsScript.PlayAttackSound();
            //animator.SetTrigger("Attack");
            GameObject ballInstance = Instantiate(ball, transform.position, transform.rotation);
            ballInstance.GetComponent<Ball>().SetDirection(animator.GetBool("FacingRight") ? 1 : -1);
            ballInstance.GetComponent<Ball>().Go();
            ShootReady = false;
            shootRefresh = ShootCooldown;
        } 
    }

    public void InvisibleClock(InputAction.CallbackContext ctx)
    {
        if(ctx.started && CloakReady && CloakUnlock)
        {
            inCloak = true;
            CloakReady = false;
            cloakRefresh = CloakCooldown;
        }
    }

    public bool GetIsCloaked()
    {
        return inCloak;
    }

    public void SetDoubleJumpUnlock()
    {
        DoubleJumpUnlock = true;
    }

    public void SetDashUnlock() 
    {
        DashUnlock = true;
    }

    public void SetShootUnlock()
    {
        ShootUnlock = true;
    }

    public void SetCloakUnlock()
    {
        CloakUnlock = true;
    }
}

