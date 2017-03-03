using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

	public float disperseRadius = 5.0f;
	// Update is called once per frame
	void Update () {
		// On left mouse...
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			// Cast a ray from the camera in the direction of the mouse click.
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 300f, 1 << LayerMask.NameToLayer("Boids"))){
				// ... if the ray hit a boid, get all boids in its vicinity and disperse them
				Collider[] boids = Physics.OverlapSphere(transform.position, disperseRadius, 1 << LayerMask.NameToLayer("Boids"));
				foreach(Collider c in boids){
					Boid b = c.GetComponent<Boid>();
					if(b){
						Vector3 diff = c.transform.position - hit.point;
						float len = diff.magnitude;
						float scalar = (1.0f - len / disperseRadius) * 100f;
						b.Disperse(-scalar);
					}
				}
			}
		}
	}
}
