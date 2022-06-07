using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveArea : InteractableObject
{
    private PlayerRevive playerRevive;
    private PlayerStats playerStats;
    private bool reviving = false;
    [SerializeField] private float reviveTime = 7;

    protected override void AfterStart()
    {
        base.AfterStart();

        playerRevive = GetComponentInParent<PlayerRevive>();
        playerStats = GetComponentInParent<PlayerStats>();
        interactText = " to revive Player";
    }

    protected override void PlayerTriggerEntered(PlayerStats stats)
    {
        if (playerStats == stats || playerStats.IsDead || stats.IsDown)
        {
            return;
        }

        base.PlayerTriggerEntered(stats);

        if (reviving)
        {
            return;
        }

        stats.OnInteract += StartRevive;
        stats.OnInteractHoldStop = StopRevive;
    }

    private void Update()
    {
        if (playerStats.IsDead && reviving)
        {
            StopRevive(playerStats);
        }
    }

    private void StartRevive(PlayerStats playerStats)
    {
        playerRevive.SetTimer(true, true);
        reviving = true;
        playerUI.HideInteractPanel();
        playerUI.StartReviveTimer(reviveTime);

        Invoke(nameof(ReviveDone), reviveTime);
    }

    private void ReviveDone()
    {
        playerRevive.Revived(true);
        reviving = false;
    }

    private void StopRevive(PlayerStats playerStats)
    {
        playerRevive.SetTimer(false, true);
        playerUI.StopReviveTimer();
        reviving = false;
        CancelInvoke();
    }
}
