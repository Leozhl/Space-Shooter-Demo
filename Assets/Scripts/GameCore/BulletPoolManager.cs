using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager Instance;

    public GameObject BullerPrefab;
    public int BulletAmount;

    private Queue<GameObject> mBulletPool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < BulletAmount; i++)
        {
            GameObject tmpBullet = Instantiate(BullerPrefab, transform);
            tmpBullet.SetActive(false);
        }
    }

    public void ShootBullet(Vector3 position, Vector3 direction, int id)
    {
        if (mBulletPool.Count == 0)
        {
            Debug.Log("No Bullet!");
            return;
        }
        GameObject tmpBullet = mBulletPool.Dequeue();
        tmpBullet.transform.position = position;
        tmpBullet.transform.forward = direction;
        tmpBullet.GetComponent<BulletController>().SetID(id);
        tmpBullet.SetActive(true);
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.transform.position = transform.position;
        mBulletPool.Enqueue(bullet);
    }
}
