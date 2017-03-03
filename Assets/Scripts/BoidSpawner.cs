using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BoidSpawner : MonoBehaviour {

	private const float k_initRadius = 7f;								// Radius of initial position of the bodis.

	public GameObject boidPrefab;										// The prefab of the boids.

	[Range(1f,20f)]
	public float boidDistance = 2f;										// The distance between each boid.
	[Range(1f,10f)]
	public float boidSpeed = 5f;										// The speed of the boids.
	[Range(0.1f, 4f)]
	public float boidAngular = 1f;										// The angular smoothness (speed) of the boids.

	[HideInInspector]
	public Vector3[] bounds;
	public Vector3 boundSize = new Vector3(20f,20f,20f);

	private GameObject parent;

	void Awake(){
		bounds = new Vector3[2];
		bounds[0] = transform.position + boundSize; // max bounds.
		bounds[1] = transform.position - boundSize; // min bounds.

		parent = new GameObject("boids_" + name);
	}

	// Update is called once per frame
	void Update(){
		// Update the bounds.
		bounds[0] = transform.position + boundSize; 
		bounds[1] = transform.position - boundSize; 
	}
	
	private GameObject Spawn () {
		Vector3 pos = transform.position + Random.insideUnitSphere * k_initRadius;

		GameObject bObj = Instantiate(boidPrefab, pos, Random.rotation) as GameObject;
		if(bObj){
			Boid b = bObj.GetComponent<Boid>();	
			b.spawner = this;
			bObj.transform.SetParent(parent.transform);
		}
		return bObj;
	}

	public void Spawn (int n) {
		for(int i = 0; i < n; i++){
			Spawn();
		}
	}

	public void Spawn (int n, ref GameObject[] boids) {
		boids = new GameObject[n];
		for(int i = 0; i < n; i++){
			boids[i] = Spawn();
		}
	}

	public void Remove(int n){
		if(n > parent.transform.childCount)
			n = parent.transform.childCount;

		for(int i = 0; i < n; i++){
			Transform t = parent.transform.GetChild(0);
			DestroyImmediate(t.gameObject);
		}
	}

	public void Toggle(bool b){
		if(parent != null)
			parent.SetActive(b);
		gameObject.SetActive(b);
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, boundSize);
	}
}
