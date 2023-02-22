using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public RectTransform PanelRoot;

    private Dictionary<Type, GameObject> mCreatedUIDic = new Dictionary<Type, GameObject>();
    private HashSet<GameObject> mActiveUISet = new HashSet<GameObject>();

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
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadUI<StartMenuUI>();
        GameManager.Instance.OnGameStart += (() =>
        {
            DisableAll();
            LoadUI<GameCoreUI>();
        });
        GameManager.Instance.OnGameExit += (() =>
        {
            DisableAll();
            LoadUI<GameOverUI>();
        });
    }

    public void LoadUI<T> () where T : BaseUI
    {
        GameObject go;
        if (!mCreatedUIDic.TryGetValue(typeof(T), out go))
        {
            go = Instantiate(Resources.Load<GameObject>("Prefabs/UI/" + typeof(T).Name), PanelRoot.transform);
            mCreatedUIDic.Add(typeof(T), go);
        }
        go.GetComponent<T>().SetMultilanguageText(GameManager.Instance.CurrentLanguage);
        go.transform.SetAsLastSibling();
        go.SetActive(true);
        mActiveUISet.Add(go);
    }

    public T GetUI<T> () where T : BaseUI
    {
        GameObject go;
        if (mCreatedUIDic.TryGetValue(typeof(T), out go))
        {
            return go.GetComponent<T>();
        }
        else
        {
            Debug.Log(typeof(T).Name + " not loaded!");
            return null;
        }
    }

    public void DisableUI(GameObject go) 
    {
        go.SetActive(false);
        if (mActiveUISet.Contains(go))
        {
            mActiveUISet.Remove(go);
        }
    }    

    public void DisableAll()
    {
        foreach (GameObject go in mActiveUISet)
        {
            go.SetActive(false);
        }
        mActiveUISet.Clear();
    }
}
