using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickUI : BaseUI
{
    public bool Active;

    private RectTransform mStick;
    private EventTrigger mEventTrigger;

    private Vector2 mStickOriginalPos;
    private Vector2 mStickCurPos;
    private bool isMoving;
    private float radius;

    private void Awake()
    {
        mStick = UIItems[0].GetComponent<RectTransform>();
        mEventTrigger = gameObject.GetComponent<EventTrigger>();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener(eventdata => OnDrag(eventdata as PointerEventData));
        mEventTrigger.triggers.Add(dragEntry);

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener(eventdata => OnPointerDown(eventdata as PointerEventData));
        mEventTrigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener(eventdata => OnPointerUp(eventdata as PointerEventData));
        mEventTrigger.triggers.Add(pointerUpEntry);
    }

    private void Start()
    {
        mStickOriginalPos = mStick.position;
        mStickCurPos = mStick.position;
        isMoving = false;
        radius = gameObject.GetComponent<RectTransform>().rect.width / 2;
        radius *= Screen.width / UIManager.Instance.PanelRoot.rect.width;
    }

    private void FixedUpdate()
    {
        if (isMoving && Active)
        {
            Vector2 direction = (mStickCurPos - mStickOriginalPos).normalized;
            PlayerController.Instance.Move(direction);
        }
    }

    private void OnDrag(PointerEventData data)
    {
        isMoving = true;
        if (Vector2.Distance(data.position, mStickOriginalPos) > radius)
        {
            mStick.position = mStickOriginalPos + (data.position - mStickOriginalPos).normalized * radius;
        }
        else
        {
            mStick.position = data.position;
        }
        mStickCurPos = mStick.position;
    }

    private void OnPointerDown(PointerEventData data)
    {
        isMoving = true;
        mStick.position = data.position;
        mStickCurPos = mStick.position;
    }

    private void OnPointerUp(PointerEventData data)
    {
        Reset();
    }

    private void Reset()
    {
        isMoving = false;
        mStick.position = mStickOriginalPos;
        if (Active) PlayerController.Instance.Stop();
    }
}
