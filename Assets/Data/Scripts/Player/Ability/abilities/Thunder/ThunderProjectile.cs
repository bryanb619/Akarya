using FMODUnity;
using UnityEngine;

public class ThunderProjectile : MonoBehaviour
{
    private float _speed = 20f;

    private int EnemyDamage = 30;
    public int enemyDamage => EnemyDamage;

    private int EnemyChaseDamage = 40; // before 80
    public int enemyChaseDamage => EnemyChaseDamage;

    float timer = 0f;

    [SerializeField] private Rigidbody rb;

    // Start is called before the first frame update
    private void Start()
    {
        rb.velocity = transform.forward * _speed;
        RuntimeManager.PlayOneShot("event:/Spells/THUNDERSPELL3D", transform.position);

     
    }
    private void Update()
    {
        GetTimer();

    }

    private void GetTimer()
    {
        timer += Time.deltaTime;
        if (timer >= 6f)
        {
            DestroyBullet();

        }
    }



    private void OnTriggerEnter(Collider hitInfo)
    {
        EnemyBehaviour enemy = hitInfo.GetComponent<EnemyBehaviour>();
        EnemyChaseBehaviour ChaseEnemy = hitInfo.GetComponent<EnemyChaseBehaviour>();
        PlayerMovement player = hitInfo.GetComponent<PlayerMovement>();

        if (enemy != null)
        {
            enemy.TakeDamage(enemyDamage, WeaponType.Thunder);
            DestroyBullet();
            //Debug.Log("HIT");

        }
        else if (ChaseEnemy != null)
        {
            ChaseEnemy.TakeDamage(enemyChaseDamage, WeaponType.Thunder);
            DestroyBullet();
            Debug.Log("HIT");

        }
        else if (player != null)
        {
            DestroyBullet();
        }

        //Instantiate(impactEffect, transform.position, transform.rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DestroyBullet();
    }


    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
