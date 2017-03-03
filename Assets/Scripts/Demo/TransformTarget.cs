using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTarget : MonoBehaviour {

	public float radius = 2.0f;
	[Range(0.0f, 1.0f)]
	public float xFreq = 0.3f, yFreq = 0.2f, zFreq = 0.4f;

	public bool changePosition = true, changeRotation = true;
	private Vector3 omega = Vector3.zero, alpha = Vector3.zero;

	// Use this for initialization
	void Start () {
		// Weight to change the angle of movement.
		omega.x = Random.Range(0.3f,0.6f);
		omega.y = Random.Range(0.2f,0.7f);
		omega.z = Random.Range(0.3f,0.55f);
	}
	
	// Update is called once per frame
	void Update () {
		if(!changePosition && !changeRotation)
			return;
		Vector3 newRot = new Vector3(
			xFreq * Mathf.Sin(alpha.x),
			yFreq * Mathf.Sin(alpha.y),
			zFreq * Mathf.Sin(alpha.z)
		);
//		Debug.DrawRay(transform.position, newRot, Color.red, 30f);

		if(changeRotation){
			Quaternion rot = Quaternion.FromToRotation(Vector3.forward, newRot);
			transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
		}
		if(changePosition)
			transform.localPosition = newRot * radius;

		alpha += omega * Time.deltaTime;
	}
}
