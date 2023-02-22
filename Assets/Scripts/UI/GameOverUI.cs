using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : BaseUI
{
    private Text mScoreDetailText;
    private Text mTotalScoreText;
    private Button mRestartButton;
    private Button mExitButton;
    private string mSurviveTimeContent = "";
    private string mSecondContent = "";
    private string mKillCountContent = "";
    private string mTotalScoreContent = "";

    private void Awake()
    {
        mScoreDetailText = UIItems[0].GetComponent<Text>();
        mTotalScoreText = UIItems[1].GetComponent<Text>();
        mRestartButton = UIItems[2].GetComponent<Button>();
        mExitButton = UIItems[3].GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mRestartButton.onClick.AddListener(() => { GameManager.Instance.StartGame(); mRestartButton.enabled = false; });
        mExitButton.onClick.AddListener(() => { UIManager.Instance.DisableAll(); UIManager.Instance.LoadUI<StartMenuUI>(); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.LoadUI<ExitUI>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        mRestartButton.enabled = true;
    }

    public void SetScoreDetail(int surviveTime, float surviveTimeRatio, int killCount, float killCountRatio)
    {
        mScoreDetailText.text = mSurviveTimeContent + surviveTime + " " + mSecondContent + " * " + surviveTimeRatio + " = " + surviveTime * surviveTimeRatio + '\n'
            + mKillCountContent + killCount + " * " + killCountRatio + " = " + killCount * killCountRatio + '\n';
    }

    public void SetTotalScore(int score)
    {
        mTotalScoreText.text = mTotalScoreContent + score.ToString();
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 12);
        mSurviveTimeContent = MultilanguageManager.Instance.GetText(type, 13);
        mSecondContent = MultilanguageManager.Instance.GetText(type, 14);
        mKillCountContent = MultilanguageManager.Instance.GetText(type, 21);
        mTotalScoreContent = MultilanguageManager.Instance.GetText(type, 15);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 16);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 17);
    }
}
