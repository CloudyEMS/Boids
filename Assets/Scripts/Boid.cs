using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

	public BoidSpawner spawner;

	public Collider[] boids;
	private Vector3 seperation, alignment, cohesion;
	private float cohesionMultiplier = 1f;
	private bool isSeperating = false;
	private float speedModifier = 1f, rotationModifier = 1f;

	// Update is called once per frame
	void Update () {

		// init vectors.
		seperation = Vector3.zero;
		alignment = spawner.transform.forward;
		cohesion = spawner.transform.position;

		// Find the nearby boids by casting a sphere around the boid, and checking for any collisions.
		boids = Physics.OverlapSphere(transform.position, spawner.boidDistance, 1 << LayerMask.NameToLayer("Boids"));

		foreach(Collider b in boids){
			Transform t = b.transform;
			if(t == transform)
				continue;

			// Calculate seperation.
			Vector3 diff = transform.position - t.position;
			float len = diff.magnitude;
			// The closer the boids are to each other, the more they should seperate.
			// This results in smoother and more life-like appearance of the boids.
			diff *= Mathf.Clamp01(1.0f - len / spawner.boidDistance) / len; 
			seperation += diff;
			 
			// Caluclate Alignment.
			alignment += t.forward;
			// Calucluate Cohesion.
			cohesion += t.position;
		}
			
		alignment /= boids.Length;
		cohesion /= boids.Length;
		cohesion = (cohesion - transform.position).normalized;

		Vector3 dir = cohesionMultiplier * cohesion + alignment + seperation + BoundPos();
		// Uncomment this line with the function below to have a bad implementation of pseudo collision avoidance to the boids.
//		dir += AvoidCollision();

		// Function ensures that the forward of the boid is facing new dir, in world space.
		Quaternion rot = Quaternion.FromToRotation(Vector3.forward, dir.normalized);
		// Smoothly rotate towards the newly found direction.
		transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationModifier * spawner.boidAngular * Time.deltaTime);

		// always move forward.
//		float noise = 1.0f;//(Random.value);// * 2.0f - 1.0f); //1.0f + Random.Range(0.2f, 0.5f) * 0.6f)
		transform.position += transform.forward * (spawner.boidSpeed * Time.deltaTime * speedModifier) ;
	}

	Vector3 BoundPos(){
		Vector3 v = Vector3.zero;

		if(transform.position.x < spawner.bounds[1].x){
			v.x = 10f;
		} else if (transform.position.x > spawner.bounds[0].x){
			v.x = -10f;
		}

		if(transform.position.y < spawner.bounds[1].y){
			v.y = 10f;
		} else if (transform.position.y > spawner.bounds[0].y){
			v.y = -10f;
		}

		if(transform.position.z < spawner.bounds[1].z){
			v.z = 10f;
		} else if (transform.position.z > spawner.bounds[0].z){
			v.z = -10f;
		}
		return v;
	}

	// Bad implementation of pseudo collision avoidance to the boids.
	/*
	Vector3 AvoidCollision(){
		Vector3 p = Vector3.zero;
		RaycastHit hit;
		// Send a ray in the the direction of flight, check if it collided with anything other than the blobs,
		// Then try its best to steer away.
		if(Physics.Raycast(transform.position, transform.forward, out hit, 5f, ~(1 << LayerMask.NameToLayer("Boids")))){
			Debug.Log("hit " + hit.transform.name);
			Vector3 diff = transform.position - hit.point;
			float len = diff.magnitude;
			speedModifier = (1.0f - len / 5f);// * Time.deltaTime;
			rotationModifier = len / 5f;
			p = -transform.forward;
//			p = Vector3.Reflect(transform.forward, hit.normal) * 100f;
			Debug.DrawRay(transform.position, transform.forward, Color.blue, 2f);
			Debug.DrawRay(transform.position, p, Color.red, 2f);
		} else {
			rotationModifier = speedModifier = 1f;
		}
		return p;
	}
	*/

	public void Disperse(float m){
		if(isSeperating)
			return;

		cohesionMultiplier = m * spawner.boidSpeed;
		StartCoroutine("Flock");
	}

	private IEnumerator Flock(){
		if(isSeperating)
			yield break;
		
		isSeperating = true;
		yield return new WaitForSeconds(2f);
		cohesionMultiplier = 1f;
		isSeperating = false;
	}
}
