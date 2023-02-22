using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum LanguageType
{
    Chinese = 0,
    English = 1,
}

public class MultilanguageManager : MonoBehaviour
{
    public static MultilanguageManager Instance;
    public Action<LanguageType> OnLanguageChange;

    private Dictionary<LanguageType, Dictionary<int, string>> mTypeDictionary = new Dictionary<LanguageType, Dictionary<int, string>>();
    private Dictionary<int, string> mChineseDictionary = new Dictionary<int, string>();
    private Dictionary<int, string> mEnglishDictionary = new Dictionary<int, string>();

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

        mTypeDictionary.Add(LanguageType.Chinese, mChineseDictionary);
        mTypeDictionary.Add(LanguageType.English, mEnglishDictionary);
        try
        {
            string[] inputs = Resources.Load<TextAsset>("Setting/MultiLanguage").text.Split('\n');

            for (int i = 1; i < inputs.Length-1; ++i)
            {
                string[] info = inputs[i].Split(',');
                int id = int.Parse(info[0]);
                mChineseDictionary.Add(id, info[1]);
                mEnglishDictionary.Add(id, info[2]);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    public string GetText(LanguageType type, int id)
    {
        return mTypeDictionary[type][id];
    }

    public void ChangeLanguage(LanguageType type)
    {
        GameManager.Instance.SetLanguage(type);
        OnLanguageChange?.Invoke(type);
    }
}
