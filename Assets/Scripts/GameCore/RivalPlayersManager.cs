using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using msg;

public class RivalPlayersManager : MonoBehaviour
{
    public static RivalPlayersManager Instance;

    public RivalPlayerController rivalPlayerPrefab;

    private Queue<RivalPlayerController> mUnusedRivalPlayers = new Queue<RivalPlayerController>();
    private Dictionary<int, RivalPlayerController> mInGameRivalPlayers = new Dictionary<int, RivalPlayerController>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void RivalPlayerEnter(MsgPlayerEnter msg)
    {
        if (mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " already exist!");
            return;
        }

        RivalPlayerController rivalPlayer;
        if (mUnusedRivalPlayers.Count == 0)
        {
            Vector3 rivalPlayerPosition = new Vector3(msg.positionX, msg.positionY, msg.positionZ);
            Quaternion rivalPlayerRotation = Quaternion.LookRotation(new Vector3(msg.forwardX, msg.forwardY, msg.forwardZ));
            rivalPlayer = Instantiate(rivalPlayerPrefab, rivalPlayerPosition, rivalPlayerRotation, transform);
        }
        else
        {
            rivalPlayer = mUnusedRivalPlayers.Dequeue();
            rivalPlayer.transform.position = new Vector3(msg.positionX, msg.positionY, msg.positionZ);
            rivalPlayer.transform.forward = new Vector3(msg.forwardX, msg.forwardY, msg.forwardZ);
        }
        NetworkManager.Instance.GetName(msg.id, rivalPlayer.SetName);
        rivalPlayer.SetHealth(msg.curHealth, msg.maxHealth);
        rivalPlayer.gameObject.SetActive(true);
        rivalPlayer.OnDisable += (() => mUnusedRivalPlayers.Enqueue(rivalPlayer));
        mInGameRivalPlayers.Add(msg.id, rivalPlayer);

        if (mInGameRivalPlayers.Count > 200) //尝试用以应对未知情况导致的无限制创建玩家bug
        {
            NetworkManager.Instance.ExitGame();
            UIManager.Instance.LoadUI<ErrorUI>();
        }
    }

    public void RivalPlayerMove(MsgPlayerMove msg)
    {
        if (!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        rivalPlayer.Move(new Vector3(msg.positionX, msg.positionY, msg.positionZ), new Vector2(msg.directionX, msg.directionY));
    }

    public void RivalPlayerRotate(MsgPlayerRotate msg)
    {
        if (!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        rivalPlayer.Rotate(new Vector3(msg.forwardX, msg.forwardY, msg.forwardZ));
    }

    public void RivalPlayerStop(MsgPlayerStop msg)
    {
        if (!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        rivalPlayer.Stop();
    }

    public void RivalPlayerShoot(MsgPlayerShoot msg)
    {
        if(!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        Vector3 position = new Vector3(msg.positionX, msg.positionY, msg.positionZ);
        Vector3 forward = new Vector3(msg.forwardX, msg.forwardY, msg.forwardZ);
        BulletPoolManager.Instance.ShootBullet(position, forward, msg.id);
        rivalPlayer.Shoot();
    }

    public void RivalPlayerSetHealth(MsgPlayerSetHealth msg)
    {
        if (!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        rivalPlayer.SetHealth(msg.curHealth, msg.maxHealth);
        if (msg.curHealth <= 0)
        {
            mInGameRivalPlayers.Remove(msg.id); 
        }
    }

    public void RivalPlayerExit(MsgPlayerExit msg)
    {
        if (!mInGameRivalPlayers.ContainsKey(msg.id))
        {
            Debug.Log("Rival Plaer " + msg.id + " not exist!");
            return;
        }

        RivalPlayerController rivalPlayer = mInGameRivalPlayers[msg.id];
        rivalPlayer.gameObject.SetActive(false);
        mInGameRivalPlayers.Remove(msg.id);
    }
}
