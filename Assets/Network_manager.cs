using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Network_manager : MonoBehaviourPunCallbacks
{
    public Text             StatusText,people,Room,RoomStatus;
    public InputField       NickName,RoomName;
    public GameObject       Logincanvas,lobycanvas,RoomCanvas;
    
    //roomlist
    public Button[]         roombtn;
    public Button           prebtn, nextbtn;
    public List<RoomInfo>   roomInfos = new List<RoomInfo>();
    int                     currentPage = 1, maxPage, multiple;

    public Button           Ready;
    public SceneManager     scene;

    public void checkroom(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(roomInfos[num].Name);
        renewRoomlist();
    }

    public void renewRoomlist()
    {

        maxPage=(roomInfos.Count %roombtn.Length==0)? roomInfos.Count / roombtn.Length : roomInfos.Count / roombtn.Length + 1;


        prebtn.interactable = (currentPage <= 1) ? false : true;
        nextbtn.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * roombtn.Length;
        for (int i = 0; i < roombtn.Length; i++)
        {
            roombtn[i].interactable = (multiple + i < roomInfos.Count) ? true : false;
            roombtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < roomInfos.Count) ? roomInfos[multiple + i].Name : "";
        }

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!roomInfos.Contains(roomList[i])) roomInfos.Add(roomList[i]);
                else roomInfos[roomInfos.IndexOf(roomList[i])] = roomList[i];
            }
            else if (roomInfos.IndexOf(roomList[i]) != -1) roomInfos.RemoveAt(roomInfos.IndexOf(roomList[i]));
        }
        renewRoomlist();
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Screen.SetResolution(1080, 740, false);
        Logincanvas.SetActive(true);
        lobycanvas.SetActive(false);
        RoomCanvas.SetActive(false);
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        viewinfo();
    }

    public void Connect() {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LocalPlayer.NickName = NickName.text;
    }

    public override void OnConnectedToMaster()
    {
        Logincanvas.SetActive(false);
        lobycanvas.SetActive(true);
        print("서버접속완료: " + PhotonNetwork.LocalPlayer.NickName);
        JoinLobby();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Logincanvas.SetActive(true);
        lobycanvas.SetActive(false);
        print("연결끊김");
        PhotonNetwork.LocalPlayer.NickName = "";
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby() {
        print("로비접속완료");
        viewinfo();
    }

    public void CreateRoom() {
        PhotonNetwork.CreateRoom(RoomName.text == "" ? "Room"  + Random.Range(0, 100) : RoomName.text, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnCreatedRoom()
    {
        print("방 생성");
        viewinfo();
        Logincanvas.SetActive(false);
        lobycanvas.SetActive(false);
        RoomCanvas.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        JoinLobby();
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomName.text);
    }

    public override void OnJoinedRoom()
    {
        Logincanvas.SetActive(false);
        lobycanvas.SetActive(false);
        RoomCanvas.SetActive(true);
        print("방 참가");
        viewinfo();

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print(this.RoomName.text+" "+ message);
        JoinLobby();
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        RoomStatus.text = "";
        Room.text = "";
        base.OnLeftRoom();
        Logincanvas.SetActive(false);
        lobycanvas.SetActive(true);
        RoomCanvas.SetActive(false);
        viewinfo();
        print("방 나가기");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        viewinfo();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        viewinfo();
    }

    public void getStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen=false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            SceneManager.LoadScene(1);
        }
    }


    void viewinfo()
    {
        if (PhotonNetwork.InRoom)
        {
            RoomStatus.text = "Room "+PhotonNetwork.NetworkClientState.ToString();
            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";

            Room.text = "현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name + "\n"
                + "현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount+"\n"
                + "현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers+"\n"
                +playerStr;
        }
        else
        {
            StatusText.text = PhotonNetwork.NetworkClientState.ToString() + "\n";
            people.text = "플레이어 이름:" + PhotonNetwork.NickName+"\n"
            + "\n" + "접속한 인원 수 : " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "\n"
            + "방 개수 : " + PhotonNetwork.CountOfRooms + "\n";
        }
    }
}
