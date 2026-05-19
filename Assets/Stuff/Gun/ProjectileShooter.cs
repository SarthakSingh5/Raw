// using UnityEngine;

// [CreateAssetMenu(fileName = "Shooter", menuName = "Gun/Shooter/Projectile")]
// public class ProjectileShooter : Shooter
// {
//     public override void Shoot(Transform muzzle, float bloom)
//     {
//         for (int i = 0; i < bulletsPerShot; i++)
//         {
//             // 1. Calculate the Random Spread based on the passed bloom
//             // Random.insideUnitCircle gives a value between -1 and 1
//             Vector2 spread = Random.insideUnitCircle * bloom * spreadMultiplier;
            
//             // 2. Convert that to a Rotation (Pitch and Yaw)
//             Quaternion spreadRotation = Quaternion.Euler(spread.x, spread.y, 0);
            
//             // 3. Combine with Muzzle Rotation
//             Quaternion finalRotation = muzzle.rotation * spreadRotation;

//             // 4. Spawn Bullet
//             GameObject currentBullet = Instantiate(bullet, muzzle.position, finalRotation);

//             Bullet bulletScript = currentBullet.GetComponent<Bullet>();
//             bulletScript.shooter = this;
//             bulletScript.dir = finalRotation * Vector3.forward;

//             Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
//             rb.AddForce(bulletScript.dir * bulletVelocity, ForceMode.Impulse);
//         }
//     }
// }
