// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Bullet : MonoBehaviour
// {
//     [SerializeField] float timeToDestroy;
//     [HideInInspector] public ProjectileShooter shooter;
//     [HideInInspector] public Vector3 dir;


//     // Start is called before the first frame update
//     void Start()
//     {
//         Destroy(this.gameObject, timeToDestroy);
//     }

//     // Update is called once per frame


//     private void OnCollisionEnter(Collision collision)
//     {
//         IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
//         if (damageable != null)
// 		{
// 			float damage = shooter.Damage + Random.Range(0f, shooter.DamageRandom);

// 			damageable.Damage(damage);

// 		}
//         Destroy(this.gameObject);
//     }
// }
