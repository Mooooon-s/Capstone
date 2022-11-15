using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class CameraSetup : MonoBehaviour
{
    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        if (this.PV.IsMine)
        {
            var virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            virtualCamera.Follow = this.transform;
            virtualCamera.LookAt = this.transform;
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
