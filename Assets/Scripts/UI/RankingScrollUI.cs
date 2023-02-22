using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using msg;

public class RankingScrollUI : BaseUI, LoopScrollDataSource, LoopScrollPrefabSource //排行滚动界面
{
    private LoopVerticalScrollRect ls;
    private GameObject mScrollItem;
    private Stack<GameObject> mItems = new Stack<GameObject>();
    private List<MsgRankingList> mRankingItems = new List<MsgRankingList>();

    private void Awake()
    {
        mScrollItem = UIItems[0];
        mScrollItem.SetActive(false);

        ls = gameObject.GetComponent<LoopVerticalScrollRect>();
        ls.prefabSource = this;
        ls.dataSource = this;
    }

    public void SetRankingList(ref List<MsgRankingList> rankingList)
    {
        mRankingItems = rankingList;
        ls.totalCount = mRankingItems.Count;
        ls.RefillCells();
    }

    public GameObject GetObject(int index)
    {
        if (mItems.Count == 0)
        {
            return Instantiate(mScrollItem);
        }
        GameObject newItem = mItems.Pop();
        newItem.SetActive(true);
        return newItem;
    }

    public void ProvideData(Transform transform, int idx)
    {
        if (idx < 0 || idx > mRankingItems.Count) return;
        Text[] rankingTexts = transform.GetComponentsInChildren<Text>();
        rankingTexts[0].text = (idx + 1).ToString();
        rankingTexts[1].text = mRankingItems[idx].id;
        rankingTexts[2].text = mRankingItems[idx].score.ToString();
    }

    public void ReturnObject(Transform trans)
    {
        trans.gameObject.SetActive(false);
        trans.SetParent(transform, false);
        mItems.Push(trans.gameObject);
    }
}
