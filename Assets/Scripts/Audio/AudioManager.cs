using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicType
{
    MenuMusic = 0,
    BattleMusic = 1,
}

public enum SFXType
{
    Shoot = 0,
    Move = 1,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource mMusicAudioSource;
    private AudioSource mSFXAudioSource;

    private AudioClip mMenuClip;
    private AudioClip mBattleClip;

    private AudioClip mShootClip;
    private List<AudioClip> mMoveClips;

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

        mMusicAudioSource = gameObject.GetComponents<AudioSource>()[0];
        mSFXAudioSource = gameObject.GetComponents<AudioSource>()[1];

        mMenuClip = Resources.Load<AudioClip>("Audio/Space Voyager");
        mBattleClip = Resources.Load<AudioClip>("Audio/Never Back Down");
        mMoveClips = new List<AudioClip>();
        for (int i = 0; i < 5; ++i)
        {
            mMoveClips.Add(Resources.Load<AudioClip>("Audio/Ground_Step" + i));
        }
        mShootClip = Resources.Load<AudioClip>("Audio/Machine Gun");
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += () => PlayMusic(MusicType.BattleMusic);
        GameManager.Instance.OnGameExit += () => PlayMusic(MusicType.MenuMusic);
    }

    public void SetMusicVolume(float volume)
    {
        mMusicAudioSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        mSFXAudioSource.volume = volume;
    }

    public void PlayMusic(MusicType type)
    {
        if (type == MusicType.MenuMusic)
        {
            mMusicAudioSource.clip = mMenuClip;
        }
        else if (type == MusicType.BattleMusic)
        {
            mMusicAudioSource.clip = mBattleClip;
        }
        mMusicAudioSource.Play();
    }

    public void PlaySFX(SFXType type)
    {
        if (type == SFXType.Move)
        {
            mSFXAudioSource.clip = mMoveClips[Random.Range(0, 5)];
        }
        else if (type == SFXType.Shoot)
        {
            mSFXAudioSource.volume *= 0.8f;
            mSFXAudioSource.clip = mShootClip;
        }

        mSFXAudioSource.Play();
    }

    public void PlaySFX(AudioSource source, SFXType type)
    {
        source.volume = GameManager.Instance.SFXVolume;

        if (type == SFXType.Move)
        {
            source.clip = mMoveClips[Random.Range(0, 5)];
        }
        else if (type == SFXType.Shoot)
        {
            source.volume *= 0.8f;
            source.clip = mShootClip;
        }

        source.Play();
    }
}
