using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public GameObject user;

    [HideInInspector]
    public uint userID;

    [SerializeField]
    public bool enableLongClick;
    [Tooltip("ºó×ùÁ¦")]
    [SerializeField]
    public float backForce;

    private void Awake()
    {

    }
    public virtual void Attack(WeaponID weaponID)
    {

    }
}
