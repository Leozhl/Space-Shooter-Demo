using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PlayerBase
{
    public static PlayerController Instance;

    public float InvincibleTime;
    public float MaxUpAngle;
    public float RotateHorizontalAngleSpeed;
    public float RotateVerticalAngleSpeed;
    public Transform CameraPosition;

    public float SurviveTime { get { return mSurviveTimer; } }
    public int KillCount { get { return mKillCounter; } }
    public float UpAngle { get { return mCurUpAngle; } }

    private float mInvincibleTimer;
    private float mSurviveTimer;
    private int mKillCounter;
    private float mCurUpAngle;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameCoreUI gameCoreUI = UIManager.Instance.GetUI<GameCoreUI>();
        if (gameCoreUI != null) gameCoreUI.SetHealthSlider(mCurHealth / MaxHealth);
        mInvincibleTimer = InvincibleTime;
        mSurviveTimer = 0f;
        mKillCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
    #if !UNITY_ANDROID || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    #endif

        if (mInvincibleTimer > 0) mInvincibleTimer -= Time.deltaTime;
        if (mCurHealth > 0) mSurviveTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
    #if !UNITY_ANDROID || UNITY_EDITOR
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if (direction.magnitude > 0) Move(direction);
    #endif
    }

    public void Move(Vector2 direction)
    {
        if (isDead) return;
        isMoving = true;
        Vector3 movement = new Vector3(direction.x, 0f, direction.y) * Speed * Time.deltaTime;
        float angle = Vector3.Angle(Vector3.forward, transform.forward);
        if (Vector3.Cross(Vector3.forward, transform.forward).y < 0) angle = -angle;
        movement = Quaternion.Euler(0, angle, 0) * movement + transform.position;
        mRigidbody.MovePosition(movement);
        base.AnimatorMove(direction);
        NetworkManager.Instance.UpdateMove(direction, transform.position);
    }

    public override void Stop()
    {
        base.Stop();
        NetworkManager.Instance.UpdateStop();
    }

    public void Rotate(Vector2 direction)
    {
        if (isDead) return;
        mRigidbody.MoveRotation(mRigidbody.rotation * Quaternion.Euler(0, direction.x * RotateHorizontalAngleSpeed * Time.deltaTime, 0));
        float verticalRotate = -direction.y * RotateVerticalAngleSpeed * Time.deltaTime;
        if (Mathf.Abs(mCurUpAngle + verticalRotate) <= MaxUpAngle)
        {
            UpperRotate(verticalRotate);
            mCurUpAngle += verticalRotate;
        }
        NetworkManager.Instance.UpdateRotate(transform.forward);
    }

    public override void Shoot()
    {
        if (isDead) return;
        base.Shoot();
        BulletPoolManager.Instance.ShootBullet(BulletSpawnPosition.position, BulletSpawnPosition.forward, GameManager.Instance.ID);
        NetworkManager.Instance.UpdateShoot(BulletSpawnPosition.position, BulletSpawnPosition.forward);
    }

    public void TakeDamage(float damage, int id)
    {
        if (mInvincibleTimer > 0) return;
        if (mCurHealth <= 0) return;
        mInvincibleTimer = InvincibleTime;
        mCurHealth -= damage;
        if (mCurHealth <= 0)
        {
            Death();
            NetworkManager.Instance.UpdateDeath(id);
        }
        else if (damage > 0)
        {
            StartCoroutine(Shining(1f, 0.25f));
        }
        GameCoreUI gameCoreUI = UIManager.Instance.GetUI<GameCoreUI>();
        if (gameCoreUI != null) gameCoreUI.SetHealthSlider(mCurHealth / MaxHealth);
        NetworkManager.Instance.UpdateHealth(mCurHealth, MaxHealth);
    }

    public override void Death()
    {
        base.Death();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        GameManager.Instance.GameExit();
    }

    public void IncreaseKill()
    {
        mKillCounter += 1;
        GameCoreUI gameCoreUI = UIManager.Instance.GetUI<GameCoreUI>();
        if (gameCoreUI != null) gameCoreUI.SetKillNumber(mKillCounter);
    }
}
