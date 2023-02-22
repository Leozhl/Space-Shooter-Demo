using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : BaseUI
{
    private Button mStartButton;
    private Button mSettingButton;
    private Button mIntroductionButton;
    private Button mRankingButton;
    private InputField mNameInputField;

    private void Awake()
    {
        mStartButton = UIItems[0].GetComponent<Button>();
        mSettingButton = UIItems[1].GetComponent<Button>();
        mIntroductionButton = UIItems[2].GetComponent<Button>();
        mRankingButton = UIItems[3].GetComponent<Button>();
        mNameInputField = UIItems[4].GetComponent<InputField>();   
    }

    // Start is called before the first frame update
    void Start()
    {
        mStartButton.onClick.AddListener(() => { GameManager.Instance.StartGame(); mStartButton.enabled = false; });
        mSettingButton.onClick.AddListener(() => UIManager.Instance.LoadUI<SettingUI>());
        mIntroductionButton.onClick.AddListener(() => UIManager.Instance.LoadUI<IntroductionUI>());
        mRankingButton.onClick.AddListener(() => UIManager.Instance.LoadUI<RankingPageUI>());
        mNameInputField.onEndEdit.AddListener((value) => GameManager.Instance.SetName(value));
        mNameInputField.text = GameManager.Instance.Name;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.LoadUI<ExitUI>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        mStartButton.enabled = true;
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 1);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 2);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 3);
        MultilanguageTexts[3].text = MultilanguageManager.Instance.GetText(type, 4);
        MultilanguageTexts[4].text = MultilanguageManager.Instance.GetText(type, 11);
    }
}
