using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using msg;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public string ServerAddress { get { return mServerAddress; } }

    private string mServerAddress = "0.0.0.0";

    private TCPClient mClient;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        mClient = gameObject.GetComponent<TCPClient>();
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += () => mClient.StartReceive();
    }

    IEnumerator SendRequest(string postfix, Action<string> onReceiveResponse, Action onReceiveError)
    {
        //Debug.Log(postfix);
        UnityWebRequest www = UnityWebRequest.Get(mServerAddress + postfix);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            onReceiveError?.Invoke();
        }
        else
        {
            //Debug.Log(www.downloadHandler.text);
            onReceiveResponse?.Invoke(www.downloadHandler.text);
        }
    }

    public void StartGame()
    {
        string postfix = "/start?id=" + GameManager.Instance.ID;
        StartCoroutine(SendRequest(postfix, HandleResponseStartGame, HandleErrorStartGame));
    }

    private void HandleResponseStartGame(string text)
    {
        mClient.StartConnect(mServerAddress, int.Parse(text));
        RegReceiveMsg();

        MsgPlayerEnter pe = new MsgPlayerEnter();
        pe.id = GameManager.Instance.ID;
        pe.positionX = 0;
        pe.positionY = 0;
        pe.positionZ = 0;
        pe.forwardX = 0;
        pe.forwardY = 0;
        pe.forwardZ = 1;
        pe.curHealth = 100;
        pe.maxHealth = 100;
        mClient.SendMessage(MessageType.PlayerEnter, pe);
        GameManager.Instance.LoadGameScene();
    }

    private void HandleErrorStartGame()
    {
        UIManager.Instance.LoadUI<ConnectionErrorUI>();
    }

    public void ExitGame()
    {
        MsgPlayerExit pe = new MsgPlayerExit();
        pe.id = GameManager.Instance.ID;
        mClient.SendMessage(MessageType.PlayerExit, pe);
        mClient.StopReceive();
    }

    public void SetID(int ID, string name, Action<string> HandleResponseSetID)
    {
        string postfix = "/setID?id=" + ID + "&name=" + name;
        StartCoroutine(SendRequest(postfix, HandleResponseSetID, null));
    }

    public void SetName(string name)
    {
        string postfix = "/setName?id=" + GameManager.Instance.ID + "&name=" + name;
        StartCoroutine(SendRequest(postfix, null, null));
    }

    public void GetName(int ID, Action<string> HandleResponseGetName)
    {
        string postfix = "/getName?id=" + ID;
        StartCoroutine(SendRequest(postfix, HandleResponseGetName, null));
    }

    public void GetRankingList()
    {
        string postfix = "/ranking";
        StartCoroutine(SendRequest(postfix, HandleResponseRankingList, null));
    }

    private void HandleResponseRankingList(string text)
    {
        string[] rankingList = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<MsgRankingList> rankingItems = new List<MsgRankingList>();

        for (int i = 0; i < rankingList.Length; i++)
        {
            int index = rankingList[i].IndexOf(' ');
            MsgRankingList tmp = new MsgRankingList();
            tmp.score = int.Parse(rankingList[i].Substring(0, index));
            tmp.id = rankingList[i].Substring(index + 1);
            rankingItems.Add(tmp);
        }

        RankingPageUI panel = UIManager.Instance.GetUI<RankingPageUI>();
        if (panel != null) panel.SetRankingList(ref rankingItems);
    }

    public void SetRanking(int score)
    {
        string postfix = "/setRanking?id=" + GameManager.Instance.ID + "&score=" + score.ToString();
        StartCoroutine(SendRequest(postfix, null, null));
    }

    public void GetRanking()
    {
        string postfix = "/ranking?id=" + GameManager.Instance.ID;
        StartCoroutine(SendRequest(postfix, HandleResponseRanking, null));
    }

    private void HandleResponseRanking(string text)
    {
        MsgRanking ranking = new MsgRanking();
        ranking.rank = int.Parse(text);

        RankingPageUI panel = UIManager.Instance.GetUI<RankingPageUI>();
        if (panel != null) panel.SetRanking(ranking);
    }

    public void UpdateMove(Vector2 direction, Vector3 position)
    { 
        MsgPlayerMove pm = new MsgPlayerMove();
        pm.id = GameManager.Instance.ID;
        pm.directionX = direction.x;
        pm.directionY = direction.y;
        pm.positionX = position.x;
        pm.positionY = position.y;
        pm.positionZ = position.z;
        mClient.SendMessage(MessageType.PlayerMove, pm);
    }

    public void UpdateRotate(Vector3 forward)
    {
        MsgPlayerRotate pr = new MsgPlayerRotate();
        pr.id = GameManager.Instance.ID;
        pr.forwardX = forward.x;
        pr.forwardY = forward.y;
        pr.forwardZ = forward.z;
        mClient.SendMessage(MessageType.PlayerRotate, pr);
    }

    public void UpdateStop()
    {
        MsgPlayerStop ps = new MsgPlayerStop();
        ps.id = GameManager.Instance.ID;
        mClient.SendMessage(MessageType.PlayerStop, ps);
    }

    public void UpdateShoot(Vector3 position, Vector3 forward)
    {
        MsgPlayerShoot ps = new MsgPlayerShoot();
        ps.id = GameManager.Instance.ID;
        ps.positionX = position.x;
        ps.positionY = position.y;
        ps.positionZ = position.z;
        ps.forwardX = forward.x;
        ps.forwardY = forward.y;
        ps.forwardZ = forward.z;
        mClient.SendMessage(MessageType.PlayerShoot, ps);
    }

    public void UpdateHealth(float curHealth, float maxHealth)
    {
        MsgPlayerSetHealth psh = new MsgPlayerSetHealth();
        psh.id = GameManager.Instance.ID;
        psh.curHealth = curHealth;
        psh.maxHealth = maxHealth;
        mClient.SendMessage(MessageType.PlayerSetHealth, psh);
    }

    public void UpdateDeath(int killerID)
    {
        MsgPlayerDeath pd = new MsgPlayerDeath();
        pd.id = GameManager.Instance.ID;
        pd.killerID = killerID;
        mClient.SendMessage(MessageType.PlayerDeath, pd);
    }

    private void OnPlayerEnter(MsgPlayerEnter msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerEnter(msg);
        }
    }

    private void OnPlayerMove(MsgPlayerMove msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerMove(msg);
        }
    }

    private void OnPlayerRotate(MsgPlayerRotate msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerRotate(msg);
        }
    }

    private void OnPlayerStop(MsgPlayerStop msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerStop(msg);
        }
    }

    private void OnPlayerShoot(MsgPlayerShoot msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerShoot(msg);
        }
    }

    private void OnPlayerSetHealth(MsgPlayerSetHealth msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerSetHealth(msg);
        }
    }

    private void OnPlayerExit(MsgPlayerExit msg)
    {
        if (msg.id != GameManager.Instance.ID)
        {
            RivalPlayersManager.Instance.RivalPlayerExit(msg);
        }
    }

    private void OnPlayerDeath(MsgPlayerDeath msg)
    {
        if (msg.killerID == GameManager.Instance.ID)
        {
            PlayerController.Instance.IncreaseKill();
        }
    }

    private void RegReceiveMsg()
    {
        mClient.RegReceiveMessage<MsgPlayerEnter>(MessageType.PlayerEnter, OnPlayerEnter);
        mClient.RegReceiveMessage<MsgPlayerMove>(MessageType.PlayerMove, OnPlayerMove);
        mClient.RegReceiveMessage<MsgPlayerRotate>(MessageType.PlayerRotate, OnPlayerRotate);
        mClient.RegReceiveMessage<MsgPlayerStop>(MessageType.PlayerStop, OnPlayerStop);
        mClient.RegReceiveMessage<MsgPlayerShoot>(MessageType.PlayerShoot, OnPlayerShoot);
        mClient.RegReceiveMessage<MsgPlayerSetHealth>(MessageType.PlayerSetHealth, OnPlayerSetHealth);
        mClient.RegReceiveMessage<MsgPlayerExit>(MessageType.PlayerExit, OnPlayerExit);
        mClient.RegReceiveMessage<MsgPlayerDeath>(MessageType.PlayerDeath, OnPlayerDeath);
    }

    private void OnApplicationQuit()
    {
        if (mClient.Connected)
        {
            ExitGame();
        }
    }

    public void SetServerAddress(string address)
    {
        mServerAddress = address;
    }
}
