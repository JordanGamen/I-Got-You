using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnlineCharacterHandler : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        bool isPlayer = gameObject.CompareTag("Player");
        bool checkMasterOrMine = photonView.IsMine;

        if (!isPlayer)
        {
            checkMasterOrMine = PhotonNetwork.IsMasterClient;
        }

        if (checkMasterOrMine || !PhotonFunctionHandler.IsPlayerOnline())
        {
            Destroy(this);
            return;
        }

        MonoBehaviour[] scripts =  GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour item in scripts)
        {
            if (item is PhotonView) { continue; }
            if (item is PlayerStats){ continue; }
            if (item is SyncMovement){ continue; }
            if (item is ChasePlayerAI){ continue; }
            if (item is EnemyStats){ continue; }

            if (item != this)
            {
                Destroy(item);
            }                
        }

        if (!isPlayer)
        {
            return;
        }

        GetComponentInChildren<AudioListener>().enabled = false;

        Camera cam = GetComponentInChildren<Camera>();
        cam.enabled = false;

        foreach (Transform child in cam.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = 3;
        }
    }
}