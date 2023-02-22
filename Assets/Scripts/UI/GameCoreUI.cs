using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameCoreUI : BaseUI
{
    public float ShootInterval;

    private GameObject mMask;
    private EventTrigger mMaskET;
    private Slider mHealthSlider;
    private Button mShootButton;
    private EventTrigger mShootButtonET;
    private Button mExitButton;
    private Text mKillText;

    private Vector2 mPrePointerPos;
    private bool mShootButtonDown;
    private float mShootTimer;

    private void Awake()
    {
        mMask = UIItems[0];
        mMaskET = mMask.GetComponent<EventTrigger>();
        mHealthSlider = UIItems[1].GetComponent<Slider>();
        mShootButton = UIItems[2].GetComponent<Button>();
        mShootButtonET = mShootButton.GetComponent<EventTrigger>();
        mExitButton = UIItems[4].GetComponent<Button>();
        mKillText = UIItems[5].GetComponent<Text>();

        EventTrigger.Entry dragBeginEntry = new EventTrigger.Entry();
        dragBeginEntry.eventID = EventTriggerType.BeginDrag;
        dragBeginEntry.callback.AddListener(data => OnMaskDragBegin(data as PointerEventData));
        mMaskET.triggers.Add(dragBeginEntry);

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener(data => OnMaskDrag(data as PointerEventData));
        mMaskET.triggers.Add(dragEntry);

        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener(_ => OnShootButtonDown());
        mShootButtonET.triggers.Add(downEntry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener(_ => OnShootButtonUp());
        mShootButtonET.triggers.Add(upEntry);

        mShootButtonDown = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        mShootButton.onClick.AddListener(() => PlayerController.Instance.Shoot());
        mExitButton.onClick.AddListener(() => GameManager.Instance.GameExit());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) GameManager.Instance.GameExit();
        if (mShootButtonDown)
        {
            mShootTimer += Time.deltaTime;
            if (mShootTimer >= ShootInterval)
            {
                PlayerController.Instance.Shoot();
                mShootTimer = 0f;
            }    
        }
    }

    private void OnMaskDragBegin(PointerEventData data)
    {
        mPrePointerPos = data.position;
    }

    private void OnMaskDrag(PointerEventData data)
    {
        Vector2 positionOffset = data.position - mPrePointerPos;
        positionOffset.x /= Screen.width;
        positionOffset.y /= Screen.height;
        PlayerController.Instance.Rotate(positionOffset.normalized);
        mPrePointerPos = data.position;
    }

    private void OnShootButtonDown()
    {
        mShootTimer = 0f;
        mShootButtonDown = true;
    }

    private void OnShootButtonUp()
    {
        mShootTimer = 0f;
        mShootButtonDown = false;
    }

    public void SetHealthSlider(float value)
    {
        mHealthSlider.value = value;
    }

    public void SetKillNumber(int killCount)
    {
        mKillText.text = killCount.ToString();
    }
}
