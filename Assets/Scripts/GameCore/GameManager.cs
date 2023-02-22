using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Action OnGameStart;
    public Action OnGameExit;

    public int ID { get { return mID; } }
    public string Name { get { return mName; } }
    public int BestScore { get { return mBestScore; } }
    public LanguageType CurrentLanguage { get { return mCurrentLanguage; } }
    public float MusicVolume { get { return mMusicVolume; } }
    public float SFXVolume { get { return mSFXVolume; } }

    private int mID;
    private string mName;
    private int mBestScore;
    private LanguageType mCurrentLanguage;
    private float mMusicVolume;
    private float mSFXVolume;
    private AsyncOperation ao;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        LoadManager("AudioManager");
        LoadManager("NetworkManager");

        mID = PlayerPrefs.GetInt("ID", -1);
        mName = PlayerPrefs.GetString("Name", GetRandomName());
        mBestScore = PlayerPrefs.GetInt("BestScore", -1);
        mCurrentLanguage = (LanguageType)PlayerPrefs.GetInt("Language", 0);
        mMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        mSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mID < 0)
        {
            mID = GetID();
            NetworkManager.Instance.SetID(mID, mName, ResetID);
        }
        AudioManager.Instance.SetMusicVolume(mMusicVolume);
        AudioManager.Instance.SetSFXVolume(mSFXVolume);
        AudioManager.Instance.PlayMusic(MusicType.MenuMusic);
    }

    private void ResetID(string text)
    {
        int response = int.Parse(text);
        if (response < 0)
        {
            mID = GetID();
            NetworkManager.Instance.SetID(mID, mName, ResetID);
        }
        else
        {
            PlayerPrefs.SetInt("ID", mID);
        }
    }

    private int GetID()
    {
        System.Random rd = new System.Random();
        int randomID = rd.Next(9) + 1;
        for (int i = 0; i < 7; i++)
        {
            randomID = randomID * 10 + rd.Next(10);
        }
        return randomID;
    }

    private string GetRandomName()
    {
        System.Random rd = new System.Random();
        string name = "";
        for (int i = 0; i < 6; ++i)
        {
            name += (char)('A' + rd.Next(26));
        }
        return name;
    }

    public void SetName(string name)
    {
        mName = name;
        PlayerPrefs.SetString("Name", mName);
        NetworkManager.Instance.SetName(mName);
    }

    public void SetLanguage(LanguageType type)
    {
        mCurrentLanguage = type;
        PlayerPrefs.SetInt("Language", (int)type);
    }

    public void SetMusicVolume(float volume)
    {
        mMusicVolume = volume;
        AudioManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mSFXVolume = volume;
        AudioManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void StartGame()
    {
        NetworkManager.Instance.StartGame();
    }

    public void LoadGameScene()
    {
        ao = SceneManager.LoadSceneAsync("Game");
        ao.allowSceneActivation = false;
        ao.completed += OnGameSceneLoaded;
        ao.allowSceneActivation = true;
    }

    private void OnGameSceneLoaded(AsyncOperation ao)
    {
        LoadManager("RivalPlayersManager");
        LoadManager("BulletPoolManager");
        Instantiate(Resources.Load<GameObject>("Prefabs/Player"), Vector3.zero, Quaternion.identity);
        Instantiate(Resources.Load<GameObject>("Prefabs/Map"), new Vector3(0, 13, -12), Quaternion.identity);
        if (OnGameStart != null)
        {
            OnGameStart.Invoke();
        }
    }

    public void GameExit()
    {
        if (!ao.isDone) return;
        ao = SceneManager.LoadSceneAsync("Start");
        ao.completed += OnStartSceneLoaded;
        NetworkManager.Instance.ExitGame();
    }

    private void OnStartSceneLoaded(AsyncOperation ao)
    {
        if (OnGameExit != null)
        {
            OnGameExit.Invoke();
        }
        CalculateScore();
    }

    private void CalculateScore()
    {
        int surviveTime = (int)PlayerController.Instance.SurviveTime;
        int killCount = PlayerController.Instance.KillCount;
        int score = surviveTime * 1 + killCount * 50;
        if (score > mBestScore)
        {
            NetworkManager.Instance.SetRanking(score);
            PlayerPrefs.SetInt("BestScore", score);
            mBestScore = score;
        }
        GameOverUI gameOverUI = UIManager.Instance.GetUI<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.SetScoreDetail(surviveTime, 1f, killCount, 50f);
            gameOverUI.SetTotalScore(score);
        }
    }

    private void LoadManager(string name)
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Managers/" + name), Vector3.zero, Quaternion.identity);
    }
}
