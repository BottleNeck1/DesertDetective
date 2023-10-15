using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GateEvent : MonoBehaviour
{
    [SerializeField] private int ToUnlock = 5;
    private int PlayerCoins;
   
    public void GetPlayerMoney(GameObject obj)
    {
        PlayerCoins = obj.GetComponent<PlayerInteract>().GetMoney();
        print("test");
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            print("test");  
        }
    }
    /*
    public void Interact(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            print(PlayerCoins);
            print("okay");
            if (PlayerCoins >= ToUnlock)
            {
                print("good");
            }
        }
    }
    */
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
