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
    private GameObject Gate = null;
   
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
            //GetComponent<PlayerInteract>().SetGate(transform.parent.gameObject);
            Gate = transform.parent.gameObject;

            if (PlayerCoins >= ToUnlock && Gate != null)
            {
                CanTrigger = true;
            }
            else
            {
                CanTrigger = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            CanTrigger = false;
            //GetComponent<PlayerInteract>().SetGate(null);
            Gate = null;
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
}
