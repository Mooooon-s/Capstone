using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Result_Room : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

    }

    // Update is called once per frame
    void Update()
    {
       
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LoadLevel(0);
        PhotonNetwork.LeaveRoom();
        
    }
}
