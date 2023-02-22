using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExitUI : BaseUI
{
    private EventTrigger mMask;
    private Button mYesButton;
    private Button mNoButton;

    // Start is called before the first frame update
    private void Awake()
    {
        mMask = UIItems[0].GetComponent<EventTrigger>();
        mYesButton = UIItems[1].GetComponent<Button>();
        mNoButton = UIItems[2].GetComponent<Button>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(_ => Close());
        mMask.triggers.Add(entry);
        mYesButton.onClick.AddListener(() => Application.Quit());
        mNoButton.onClick.AddListener(() => Close());
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 18);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 19);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 20);
    }
}
