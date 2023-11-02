using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public MoneyText moneyText;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreen;
    [SerializeField] private GameObject CoinScreen;
    [SerializeField, Tooltip("the script to play sounds")] private PlayerSounds soundsScript;
    [SerializeField ]private int maxHealth = 3;
    private int health = 0;
    [SerializeField]private int money = 0;
    private GameObject GateObject;
    private bool CanUnlock = false;
    private int UnlockAmount = 0;

    [SerializeField] private float invincibleTime = 1;
    private float invincibleTimer = 0;
    private bool invincible = false;
    private float cloakedTime = 0;

    private SpriteRenderer sprite;
    [SerializeField] private Color hurtColor;
    [SerializeField] private Color startColor;
    [SerializeField] private Color cloakColar;

    private Vector2 checkpoint; 
    private bool hasCheckpoint = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        sprite = GetComponentInChildren<SpriteRenderer>();
        sprite.color = startColor;
        if(!soundsScript){
            soundsScript = GetComponent<PlayerSounds>();
        }
        CoinScreen.SetActive(true);
    }

    private void FixedUpdate() {
        if(invincibleTimer >0){
            invincibleTimer -= Time.fixedDeltaTime;
            if(invincibleTimer <= 0){
                invincible = false;
                sprite.color = startColor;
            }
        }

        cloakedTime = GetComponentInParent<PlayerMovement>().GetCloakRefresh();
        if(cloakedTime > 0)
            sprite.color = cloakColar;
        else if(cloakedTime < 0)
            sprite.color = startColor;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        GameObject other = col.gameObject;
        switch (other.tag) {
            case "Damage":
                Damage(other);
                break;
            case "Coin":
                CollectCoin(other);
                break;
            case "EndLevel":
                WinLevel();
                break;
            case "Checkpoint":
                checkpoint = other.transform.position;
                hasCheckpoint = true;
                break;
            case "EnemyHead":
                Destroy(other.transform.root.gameObject);
                this.GetComponent<PlayerMovement>().Bounce(Vector2.up * 10);
                //money += 3;
                //moneyText.UpdatePoints(money);
                break;
            case "Bouncy":
                this.GetComponent<PlayerMovement>().Bounce(other.transform.up * 25);
                break;
            case "Refresher":
                other.GetComponent<Refresher>().Collect();
                this.GetComponent<PlayerMovement>().ResetAirJump();
                break;
            case "Gate":
                UnlockAmount = other.GetComponent<GateEvent>().UnlockAmount();
                CanUnlock = SetCanUnlock(UnlockAmount);
                GateObject = other.transform.parent.gameObject;
                break;
            case "DoubleJump":
                this.GetComponent<PlayerMovement>().SetDoubleJumpUnlock();
                Destroy(other);
                break;
            case "Dash":
                this.GetComponent<PlayerMovement>().SetDashUnlock();
                Destroy(other);
                break;
            case "Shoot":
                this.GetComponent<PlayerMovement>().SetShootUnlock();
                Destroy(other);
                break;
            case "Cloak":
                this.GetComponent<PlayerMovement>().SetCloakUnlock();
                Destroy(other);
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        GameObject other = col.gameObject; 
        switch (other.tag)
        {
            case "Gate":
                CanUnlock = false;
                break;
        }
    }


    public void Damage(GameObject obj){
        if(invincible){
            return;
        }
        health -= obj.GetComponent<harmful>().GetDamage();
        if(health <= 0){
            Die();
        }
        else{
            soundsScript.PlayDamageSound();
            Vector2 DamageForceVector = (this.transform.position - obj.transform.position).normalized * obj.GetComponent<harmful>().GetDamage() * 10;
            DamageForceVector.y = 5;
            this.GetComponent<Rigidbody2D>().AddForce(DamageForceVector, ForceMode2D.Impulse);
        }
        invincibleTimer = invincibleTime;
        invincible = true;
        sprite.color = hurtColor;

    }

    public void CollectCoin(GameObject obj){
        money+=obj.GetComponent<Money>().GetValue();
        moneyText.UpdatePoints(money);
        Destroy(obj);
        soundsScript.PlayCollectSound();
    }

    public int GetMoney()
    {
        return money;
    }
    public bool SetCanUnlock(int Amount)
    {
        if(money >= Amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool GetCanUnlock()
    {
        return CanUnlock;
    }

    public void RemoveMoney()
    {
        money -= UnlockAmount;
        moneyText.UpdatePoints(money);
    }
    
    public GameObject GateReturn()
    {
        return GateObject;
    }

    public void WinLevel(){
        Time.timeScale = 0;
        WinScreen.transform.SetParent(null);
        WinScreen.SetActive(true);
        CoinScreen.SetActive(false);
        soundsScript.PlayWinSound();
    }

    public void Die(){
        if(hasCheckpoint)
        {
            GetComponent<PlayerMovement>().Respawn(checkpoint);
        }
        else
        {
            LoseScreen.transform.SetParent(null);
            LoseScreen.SetActive(true);
            CoinScreen.SetActive(false);
            soundsScript.PlayDeathSound();
            sprite.enabled = false;
            this.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
