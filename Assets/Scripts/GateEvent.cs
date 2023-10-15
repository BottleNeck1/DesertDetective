using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GateEvent : MonoBehaviour
{
    [SerializeField] private int ToUnlock = 5;
    private int PlayerCoins;
    private bool CanTrigger = false;
    private GameObject Gate;
   
    public int GetPlayerMoney(GameObject obj)
    {
        int GetCoins = obj.GetComponent<PlayerInteract>().GetMoney();
        return GetCoins;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        GameObject PlayerObject = other.gameObject;
        PlayerCoins = GetPlayerMoney(PlayerObject);
        if (other.gameObject.name == "Player")
        {
            print(other.gameObject.name + " has entered with " + PlayerCoins + " coins");  
        }

        if(PlayerCoins >= ToUnlock)
        {
            CanTrigger = true;
        } 
        else
        {
            CanTrigger = false;
        }
        
        if(CanTrigger)
        {
            Gate = transform.parent.gameObject;
            Destroy(Gate, 0.10f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            CanTrigger = false;
        }
        
    }

    public bool Trigger()
    {
        return CanTrigger;
    }

    public int UnlockAmount()
    {
        return ToUnlock;
    }

    /*
     // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    */
}
