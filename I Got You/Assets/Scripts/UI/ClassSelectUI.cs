using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private Transform classInfoParent;
    [SerializeField] private GameObject waitText;
    [SerializeField] private GameObject fadeIn;
    public GameObject FadeIn { get { return fadeIn; } }
    private ClassInfoUI infoUI;
    
    private PlayerStats.ClassNames currentClass = PlayerStats.ClassNames.SCOUT;
    public int NumberOfPlayersChosen = 0;

    private bool hasClicked = false;
    private int playerIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        fadeIn.SetActive(false);

        string playerName = "YOU";

        if (PhotonFunctionHandler.IsPlayerOnline())
        {
            playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

            string nickName = PhotonNetwork.LocalPlayer.NickName;

            if (nickName == "")
            {
                nickName = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;
                PhotonNetwork.LocalPlayer.NickName = nickName;
            }

            playerName = nickName;
        }
        else
        {
            for (int i = 1; i < classInfoParent.childCount; i++)
            {
                classInfoParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        infoUI = classInfoParent.GetChild(playerIndex).GetComponent<ClassInfoUI>();

        infoUI.FullOpacityInfo();

        infoUI.PlayerName.text = playerName;
        infoUI.UpdateOthersOnNetworkPlayerName();
    }

    public void UpdateCurrentClass(int index)
    {
        hasClicked = true;
        currentClass = (PlayerStats.ClassNames)index;
    }

    public void UpdateClassName()
    {
        if (infoUI == null || !hasClicked)
        {
            return;
        }

        infoUI.UpdateClassName(currentClass);

        if (PhotonNetwork.IsConnected)
        {
            if (NumberOfPlayersChosen == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    infoUI.SendMasterClientLoadLevelRPC();
                }
                else
                {
                    Invoke("LoadGameScene", 1);
                }

                fadeIn.SetActive(true);
            }
            else
            {
                waitText.SetActive(true);
            }
        }
        else
        {
            fadeIn.SetActive(true);
            Invoke("LoadGameScene", 1);
        }
    }

    private void LoadGameScene()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        
        PhotonFunctionHandler.LoadSceneAsync("GameScene");
    }
}
