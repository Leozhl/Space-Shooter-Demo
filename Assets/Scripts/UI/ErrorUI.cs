using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorUI : BaseUI
{
    private Button mExitBUtton;

    void Awake()
    {
        mExitBUtton = UIItems[0].GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mExitBUtton.onClick.AddListener(() => Application.Quit());
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 29);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 30);
    }
}
