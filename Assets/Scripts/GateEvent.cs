using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GateEvent : MonoBehaviour
{
    [SerializeField] private int ToUnlock = 5;

    public int UnlockAmount()
    {
        return ToUnlock;
    }
}
