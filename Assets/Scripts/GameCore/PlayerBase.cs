using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public float Speed;
    public float MaxHealth;
    public Transform BulletSpawnPosition;
    public Transform CenterPosition;
    public List<SkinnedMeshRenderer> Renderers;
    public List<GameObject> UpperObjects;

    protected Rigidbody mRigidbody;
    protected Animator mAnimator;
    protected AudioSource mMoveAudioSource;
    protected AudioSource mShootAudioSource;
    protected float mCurHealth;
    protected bool isDead;
    protected bool isMoving;
    protected bool isShooting;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        mRigidbody = gameObject.GetComponent<Rigidbody>();
        mAnimator = gameObject.GetComponent<Animator>();
        mMoveAudioSource = gameObject.GetComponents<AudioSource>()[0];
        mShootAudioSource = gameObject.GetComponents<AudioSource>()[1];
        mCurHealth = MaxHealth;
        isDead = false;
        isMoving = false;
        isShooting = false;
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(PlayMoveSound());
        StartCoroutine(PlayShootSound());
    }

    protected IEnumerator Shining(float time, float frequency)
    {
        float timer = 0f;
        while (timer <= time)
        {
            timer += Time.deltaTime;
            bool isEnable = (timer % frequency) > (frequency / 2f);
            foreach (SkinnedMeshRenderer renderer in Renderers)
            {
                renderer.enabled = isEnable;
            }
            yield return null;
        }
        foreach (SkinnedMeshRenderer renderer in Renderers)
        {
            renderer.enabled = true;
        }
    }

    IEnumerator PlayMoveSound()
    {
        while (true)
        {
            if (isMoving)
            {
                yield return new WaitForSeconds(0.08f);
                AudioManager.Instance.PlaySFX(mMoveAudioSource, SFXType.Move);
                isMoving = false;
                yield return new WaitForSeconds(0.6f);
            }
            yield return null;
        }
    }

    IEnumerator PlayShootSound()
    {
        while (true)
        {
            if (isShooting)
            {
                AudioManager.Instance.PlaySFX(mShootAudioSource, SFXType.Shoot);
                isShooting = false;
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }
    }

    protected virtual void AnimatorMove(Vector2 direction)
    {
        mAnimator.SetFloat("VelocityX", direction.x);
        mAnimator.SetFloat("VelocityZ", direction.y);
    }

    public void UpperRotate(float angle)
    {
        foreach (GameObject go in UpperObjects)
        {
            go.transform.RotateAround(CenterPosition.position, go.transform.right, angle);
        }
    }

    public virtual void Stop()
    {
        isMoving = false;
        mAnimator.SetFloat("VelocityX", 0f);
        mAnimator.SetFloat("VelocityZ", 0f);
    }

    public virtual void Shoot()
    {
        if (gameObject.activeSelf && !isDead)
        {
            mAnimator.SetTrigger("Shoot");
            isShooting = true;
        }
    }

    public virtual void Death()
    {
        isDead = true;
        mAnimator.SetTrigger("Dead");
        StopCoroutine(PlayMoveSound());
        StopCoroutine(PlayShootSound());
        Invoke("AfterDeath", 2f);
    }

    public virtual void AfterDeath()
    {
        gameObject.SetActive(false);
    }
}
