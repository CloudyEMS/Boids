using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidSpawner))]
public class MetaBalls : MonoBehaviour {

	// must be the same as SIZE in the MetaBall_Toon.shader
	// After testing, it is best at 20 blobs. Any more than that, and the FPS drops hugely.
	const int blobCount = 20;

	public Material mat;
	[Range(0.0f, 10.0f)] public float blobSize = 7.88f;

	private BoidSpawner spawner;
	private GameObject[] boids;
	private Vector4[] boidsPos;

	// Use this for initialization
	void Start () {
		boidsPos = new Vector4[blobCount];
		spawner = GetComponent<BoidSpawner>();
		spawner.Spawn(blobCount, ref boids);

		mat.SetVectorArray("_ParticlesPos", boidsPos);
		mat.SetFloat("_KAtten", blobSize);
	}
	
	// Update is called once per frame
	void Update () {
		// Get the position of all boids in this frame.
		for(int i = 0; i < blobCount; i++){
			boidsPos[i] = boids[i].transform.position;
		}
		// Update the position array in the shader.
		mat.SetVectorArray("_ParticlesPos", boidsPos);

		// Uncomment this to allow changing the size of the blob in realTime through this script. 
		// Though it can be done through the material by changing the _KAtten attribute.

//		mat.SetFloat("_KAtten", blobSize);
	}
}
