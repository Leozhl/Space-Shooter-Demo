using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RivalPlayerController : PlayerBase
{
    public Action OnDisable;

    private Canvas mCanvas;
    private Slider mHealthSlider;
    private Text mNameText;
    private Vector3 sliderOffset = new Vector3(0, 2.5f, 0);
    private Vector3 textOffset = new Vector3(0, 3f, 0);

    private void Awake()
    {
        mCanvas = GetComponentInChildren<Canvas>();
        mHealthSlider = GetComponentInChildren<Slider>();
        mNameText = GetComponentInChildren<Text>();
    }

    protected override void Start()
    {
        base.Start();
        mCanvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        mHealthSlider.transform.position = transform.position + sliderOffset;
        mNameText.transform.position = transform.position + textOffset;
    }

    public void SetName(string name)
    {
        mNameText.text = name;
    }

    public void Move(Vector3 position, Vector2 direction)
    {
        isMoving = true;
        mRigidbody.MovePosition(position);
        AnimatorMove(direction);
    }

    public void Rotate(Vector3 forward)
    {
        mRigidbody.MoveRotation(Quaternion.LookRotation(forward));
    }

    public void SetHealth(float curHealth, float maxHealth)
    {
        if (curHealth <= 0)
        {
            Death();
        }
        else if (curHealth < mCurHealth)
        {
            StartCoroutine(Shining(1f, 0.25f));
        }
        mCurHealth = curHealth;
        MaxHealth = maxHealth;
        mHealthSlider.value = mCurHealth / MaxHealth;
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        OnDisable?.Invoke();
    }
}
