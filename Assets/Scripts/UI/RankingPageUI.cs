using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using msg;

public class RankingPageUI : BaseUI
{
    private EventTrigger mMask;
    private GameObject mRankingScroll;
    private GameObject mCurPlayerRanking;

    private void Awake()
    {
        mMask = UIItems[0].GetComponent<EventTrigger>();
        mRankingScroll = UIItems[1];
        mCurPlayerRanking = UIItems[2];

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(_ => Close());
        mMask.triggers.Add(entry);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkManager.Instance.GetRanking();
        NetworkManager.Instance.GetRankingList();
    }

    public void SetRankingList(ref List<MsgRankingList> rankingList)
    {
        mRankingScroll.GetComponent<RankingScrollUI>().SetRankingList(ref rankingList);
    }

    public void SetRanking(MsgRanking ranking)
    {
        Text[] rankingTexts = mCurPlayerRanking.GetComponentsInChildren<Text>();
        if (ranking.rank <= 0) rankingTexts[0].text = "N/A";
        else rankingTexts[0].text = (ranking.rank+1).ToString();
        rankingTexts[1].text = GameManager.Instance.Name;
        if (GameManager.Instance.BestScore < 0) rankingTexts[2].text = "N/A";
        else rankingTexts[2].text = GameManager.Instance.BestScore.ToString();
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 8);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 9);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 10);
    }
}
