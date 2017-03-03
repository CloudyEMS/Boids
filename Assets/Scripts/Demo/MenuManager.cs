using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public BoidSpawner blobSpawner, cubeSpawner;
	public Slider distanceSlider, speedSlider, rotationSlider;
	public Button cubeSpawnButton, cubeDeleteButton;

	private BoidSpawner currentSpawner;
	private Gun gun;

	// Update is called once per frame
	void Start(){
		gun = GetComponent<Gun>();
		cubeSpawner.Spawn(100);
		currentSpawner = cubeSpawner;
		ResetSliders();
	}

	void ResetSliders(){
		distanceSlider.value = currentSpawner.boidDistance;
		speedSlider.value = currentSpawner.boidSpeed;
		rotationSlider.value = currentSpawner.boidAngular;
	}

	public void TurnBoidsOn(int i){
		if(i == 0){
			blobSpawner.Toggle(false);
			cubeSpawner.Toggle(true);
			cubeSpawnButton.interactable = cubeDeleteButton.interactable = true;
			currentSpawner = cubeSpawner;
		} else if (i == 1){
			cubeSpawner.Toggle(false);
			blobSpawner.Toggle(true);
			cubeSpawnButton.interactable = cubeDeleteButton.interactable = false;
			currentSpawner = blobSpawner;
		}
		ResetSliders();
	}

	public void SpawnBoids(){
		cubeSpawner.Spawn(100);
	}
	public void DeleteBoids(){
		cubeSpawner.Remove(100);
	}

	public void ChangeDistance(float s){
		currentSpawner.boidDistance = s;
	}
	public void ChangeSpeed(float s){
		currentSpawner.boidSpeed = s;
	}
	public void ChangeRotation(float s){
		currentSpawner.boidAngular = s;
	}
	public void ChangeDisparseRadius(float s){
		gun.disperseRadius = s;
	}
}
