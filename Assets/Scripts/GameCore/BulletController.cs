using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float RemainTime;
    public float Speed;
    public float Acceleration;
    public float Damage;
    public ParticleSystem Explosion;

    private Rigidbody mRigidbody;
    private SkinnedMeshRenderer mSkinnedMeshRenderer;
    private int mID;
    private float mTimer;
    private float mSpeed;
    private bool mIsPlayingParticle;

    private void Awake()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        mSkinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        mTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (gameObject.activeSelf && mTimer > 0f)
        {
            mTimer -= Time.deltaTime;
            mSpeed += Acceleration * Time.deltaTime;
            if (mTimer > 0f) mRigidbody.MovePosition(transform.position + transform.forward * mSpeed * Time.deltaTime);
            else gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mIsPlayingParticle) return;
        if (!other.CompareTag("Bullet"))
        {
            mTimer = 0;
            mIsPlayingParticle = true;
            Explosion.Play(true);
            mSkinnedMeshRenderer.enabled = false;
            Invoke("Disable", 1f);
        }
        if (other.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(Damage, mID);
        }

    }

    public void SetID(int id)
    {
        mID = id;
    }

    private void OnEnable()
    {
        mTimer = RemainTime;
        mSpeed = Speed;
        mSkinnedMeshRenderer.enabled = true;
        mIsPlayingParticle = false;
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Explosion.Stop(true);
        BulletPoolManager.Instance.ReturnBullet(gameObject);
    }
}
