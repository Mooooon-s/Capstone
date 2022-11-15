using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using Cinemachine.Utility;

public class ThirdPersonCam : MonoBehaviour
{

    [Header("References")]
    public PhotonView PV;
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    [SerializeField] CinemachineFreeLook cam;
    public float rotationSpeed;

    public Transform combatLookAt;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat
    }



    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        if (!this.PV.IsMine)
        {
            Destroy(cam);
        }

    }

    private void Update()
    {
        if (PV.IsMine)
        {
            if (Game_Manager._IsDead == true)
            {
                ResultRoom();
            }


            // rotate orientation
            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;

            if (currentStyle == CameraStyle.Basic)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

                if (inputDir != Vector3.zero)
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }

            else if (currentStyle == CameraStyle.Combat)
            {
                Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
                orientation.forward = dirToCombatLookAt.normalized;

                playerObj.forward = dirToCombatLookAt.normalized;
                player.forward = playerObj.forward;
            }


        }


    }

    void ResultRoom()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


}