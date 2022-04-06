using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyGenerator : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private DifficultyManager difficultyManager;

    private List<GameObject> wrummels = new List<GameObject>();
    [SerializeField] private GameObject wrummelPrefab;
    [SerializeField] private int wrummelCount = 30;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            Destroy(this);
            return;
        }

        playerManager = FindObjectOfType<PlayerManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        difficultyManager = FindObjectOfType<DifficultyManager>();

        for (int i = 0; i < wrummelCount; i++)
        {
            GameObject wrummel = PhotonFunctionHandler.InstantiateGameObject(wrummelPrefab, Vector3.zero, Quaternion.identity);
            wrummel.SetActive(false);
            enemyManager.StatsOfAllEnemies.Add(wrummel.transform.GetChild(0), wrummel.GetComponent<EnemyStats>());
            wrummels.Add(wrummel);
        }
    }

    public List<GameObject> GenerateEnemies()
    {
        int countOfWrummelsToGenerate = 0;

        for (int i = 0; i < playerManager.Players.Count; i++)
        {
            countOfWrummelsToGenerate += Random.Range(6, 8) + (difficultyManager.DifficultyLevel * 2);
        }

        List<GameObject> wrummelsToGive = new List<GameObject>();

        for (int i = 0; i < countOfWrummelsToGenerate; i++)
        {
            wrummelsToGive.Add(wrummels[i]);
        }

        return wrummelsToGive;
    }
}
