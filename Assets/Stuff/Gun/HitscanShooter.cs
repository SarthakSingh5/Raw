// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;


// [CreateAssetMenu(fileName = "Shooter", menuName = "Gun/Shooter/HitScan")]
// public class HitscanShooter : Shooter
// {


// 	public override void Shoot(Transform muzzle, float bloom)
// 	{
// 		base.Shoot(muzzle, bloom);
// 		SpawnHitScanProjectile(muzzle);
// 	}


// 	Vector3 ComputeShootingDirection(Transform muzzle)
// 	{
// 		Vector3 Direction = muzzle.forward;

// 		float spreadX = Random.Range(-MaxSpreadAngle, MaxSpreadAngle) * 0.5f;
// 		float spreadY = Random.Range(-MaxSpreadAngle, MaxSpreadAngle) * 0.5f;

// 		return Quaternion.Euler(spreadX, spreadY, 0f) * Direction;
// 	}


// 	void ApplyHitImpulse(RaycastHit hit, Vector3 direction)
// 	{
// 		if (hit.rigidbody != null)
// 		{
// 			hit.rigidbody.AddForceAtPosition(direction * HitImpulse, hit.point, ForceMode.Impulse);
// 		}
// 	}


// 	void SpawnHitScanProjectile(Transform muzzle)
// 	{
// 		Ray ray = new Ray(muzzle.position, ComputeShootingDirection(muzzle));

// 		RaycastHit[] hits = Physics.RaycastAll(ray, MaxRange, HitMask, QueryTriggerInteraction.Ignore);

// 		System.Array.Sort(hits, (a, b) => { return a.distance.CompareTo(b.distance); });


// 		foreach (RaycastHit hit in hits)
// 		{
			

// 			// looking and damaging a component that implements the IDamageable interface
// 			//
// 			IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

// 			if (damageable != null)
// 			{
// 				float damage = Damage + Random.Range(0f, DamageRandom);

// 				damageable.Damage(damage);

// 				MonoBehaviourHelper.RunCoroutine(CR_SpawnTracer(muzzle.position, hit.point));

// 				ApplyHitImpulse(hit, ray.direction);

// 				return;
// 			}
// 			else
// 			{
// 				MonoBehaviourHelper.RunCoroutine(CR_SpawnTracer(muzzle.position, hit.point));

// 				ApplyHitImpulse(hit, ray.direction);

// 				// we hit a wall or obstacle, we need to return.
// 				return;
// 			}
// 		}

// 		MonoBehaviourHelper.RunCoroutine(CR_SpawnTracer(muzzle.position, ray.origin + ray.direction * MaxRange));
// 	}



// 	IEnumerator CR_SpawnTracer(Vector3 start, Vector3 end)
// 	{
// 		LineRenderer tracer = Instantiate(tracerPrefab, start, Quaternion.identity);

// 		float distance = Vector3.Distance(start, end);
// 		float elapsedTime = 0f;

// 		while (elapsedTime < tracerLifetime)
// 		{
// 			elapsedTime += Time.deltaTime;

// 			float t = Mathf.Clamp01(elapsedTime * tracerSpeed / distance);

// 			// Update tracer position to move towards the end point
// 			Vector3 currentPoint = Vector3.Lerp(start, end, t);
// 			tracer.SetPosition(0, start);
// 			tracer.SetPosition(1, currentPoint);

// 			yield return null;
// 		}

// 		// Finalize position and destroy tracer
// 		tracer.SetPosition(1, end);
// 		Destroy(tracer.gameObject, tracerLifetime);
// 	}

// }


