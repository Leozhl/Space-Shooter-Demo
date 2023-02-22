using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingUI : BaseUI
{
    private EventTrigger mMask;
    private Toggle mChineseToggle;
    private Toggle mEnglishToggle;
    private Slider mMusicSlider;
    private Text mMusicPercentageText;
    private Slider mSFXSlider;
    private Text mSFXPercentageText;
    private InputField mServerAddrInputField;

    private void Awake()
    {
        mMask = UIItems[0].GetComponent<EventTrigger>();
        mChineseToggle = UIItems[1].GetComponent<Toggle>();
        mEnglishToggle = UIItems[2].GetComponent<Toggle>();
        mMusicSlider = UIItems[3].GetComponent<Slider>();
        mMusicPercentageText = UIItems[4].GetComponent<Text>();
        mSFXSlider = UIItems[5].GetComponent<Slider>();
        mSFXPercentageText = UIItems[6].GetComponent<Text>();
        mServerAddrInputField = UIItems[7].GetComponent<InputField>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(_ => Close());
        mMask.triggers.Add(entry);
    }

    private void Start()
    {
        if (GameManager.Instance.CurrentLanguage == LanguageType.Chinese)
        {
            mChineseToggle.isOn = true;
        }
        else if (GameManager.Instance.CurrentLanguage == LanguageType.English)
        {
            mEnglishToggle.isOn = true;
        }
        mMusicSlider.value = GameManager.Instance.MusicVolume;
        mMusicPercentageText.text = (int)(GameManager.Instance.MusicVolume * 100) + "%";
        mSFXSlider.value = GameManager.Instance.SFXVolume;
        mSFXPercentageText.text = (int)(GameManager.Instance.SFXVolume * 100) + "%";

        mChineseToggle.onValueChanged.AddListener((on) =>
        {
            if (on) MultilanguageManager.Instance.ChangeLanguage(LanguageType.Chinese);
        });
        mEnglishToggle.onValueChanged.AddListener((on) =>
        {
            if (on) MultilanguageManager.Instance.ChangeLanguage(LanguageType.English);
        });
        mMusicSlider.onValueChanged.AddListener((volume) =>
        {
            GameManager.Instance.SetMusicVolume(volume);
            mMusicPercentageText.text = (int)(volume * 100)+ "%";
        });
        mSFXSlider.onValueChanged.AddListener((volume) =>
        {
            GameManager.Instance.SetSFXVolume(volume);
            mSFXPercentageText.text = (int)(volume * 100)+ "%";
        });

        mServerAddrInputField.onEndEdit.AddListener((value) => NetworkManager.Instance.SetServerAddress(value));
        mServerAddrInputField.text = NetworkManager.Instance.ServerAddress;
    }

    public override void SetMultilanguageText(LanguageType type)
    {
        base.SetMultilanguageText(type);
        MultilanguageTexts[0].text = MultilanguageManager.Instance.GetText(type, 5);
        MultilanguageTexts[1].text = MultilanguageManager.Instance.GetText(type, 6);
        MultilanguageTexts[2].text = MultilanguageManager.Instance.GetText(type, 7);
        MultilanguageTexts[3].text = MultilanguageManager.Instance.GetText(type, 27);
    }
}
