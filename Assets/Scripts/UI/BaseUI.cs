using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseUI : MonoBehaviour
{
    public List<GameObject> UIItems = new List<GameObject>();
    public List<Text> MultilanguageTexts = new List<Text>();

    public virtual void Close()
    {
        UIManager.Instance.DisableUI(gameObject);
    }

    protected virtual void OnEnable()
    {
        MultilanguageManager.Instance.OnLanguageChange += SetMultilanguageText;
    }

    protected virtual void OnDisable()
    {
        MultilanguageManager.Instance.OnLanguageChange -= SetMultilanguageText;
    }

    public virtual void SetMultilanguageText(LanguageType type) { }
}
