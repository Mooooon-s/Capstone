using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class Game_Manager : MonoBehaviourPunCallbacks
{
    public Transform    master_spawn;
    public Transform    spawn;
    public Text         Count_Down_Text;
    public GameObject   introCanvas;

    public static bool _IsDead = false;
    int flag = 1;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        
        //spwan player
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터 플레이어 생성");
            PhotonNetwork.Instantiate("player_m", master_spawn.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("일반 플레이어 생성");
            PhotonNetwork.Instantiate("player_m", spawn.position, Quaternion.identity);
        }


        //game start
        OnGameStart();

    }

    // Update is called once per frame
    void Update()
    {
        IsDead();
    }

    public void OnGameStart()
    {
        //StopCoroutine(Set_Text(Count_Down_Text));
        StartCoroutine(Set_Text(Count_Down_Text));
    }

    IEnumerator Set_Text(Text t)
    {
        t.text = "3";
        yield return new WaitForSeconds(1.0f);
        t.text = "2";
        yield return new WaitForSeconds(1.0f);
        t.text = "1";
        yield return new WaitForSeconds(1.0f);
        introCanvas.SetActive(false);
        yield break;
    }

    public void IsDead()
    {

        if (_IsDead && !PhotonNetwork.IsMasterClient && flag==1)
        {
            flag = 0;
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel(2);
        }

        if (_IsDead && PhotonNetwork.IsMasterClient && flag == 1)
        {
            flag = 0;
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel(2);
        }
    }

}
