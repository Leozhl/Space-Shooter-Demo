using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConnectionErrorUI : BaseUI
{
    private EventTrigger mMask;
    private Button mYesButton;
    private Button mNoButton;

    private void Awake()
    {
        mMask = UIItems[0].GetComponent<EventTrigger>();
        mYesButton = UIItems[1].GetComponent<Button>();
        mNoButton = UIItems[2].GetComponent<Button>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(_ => Close());
        mMask.triggers.Add(entry);
        mYesButton.onClick.AddListener(() =>
        {
            GameManager.Instance.LoadGameScene();
            mYesButton.enabled = false;
        });
        mNoButton.onClick.AddListener(() =>
        {
            UIManager.Instance.DisableAll();
            UIManager.Instance.LoadUI<StartMenuUI>();
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        mYesButton.enabled = true;
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 28);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 19);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 20);
    }
}
