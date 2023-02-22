using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IntroductionUI : BaseUI
{
    private EventTrigger mMask;

    private void Awake()
    {
        mMask = UIItems[0].GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(_ => Close());
        mMask.triggers.Add(entry);
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 22);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 23);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 1);
        MultilanguageTexts[3].text = MultilanguageManager.Instance.GetText(type, 24);
        MultilanguageTexts[4].text = MultilanguageManager.Instance.GetText(type, 25);
        MultilanguageTexts[5].text = MultilanguageManager.Instance.GetText(type, 26);
    }
}
