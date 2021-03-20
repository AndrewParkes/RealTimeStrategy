using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdate))]
    private int currentHealth;

    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if(currentHealth == 0) 
        {
            return;
        }
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if(currentHealth != 0)
        {
            return;
        }

        ServerOnDie?.Invoke();

        Debug.Log("Unit Died");
    }

    #endregion
    
    #region Client

    [Client]
    private void HandleHealthUpdate(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
