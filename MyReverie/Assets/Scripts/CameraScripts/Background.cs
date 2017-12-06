using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

	public bool scrolling = true;
	public bool parallaxX = true;
	public bool parallaxY = true;

	public float backgroundSize;
	public float parallaxSpeedX;
	public float parallaxSpeedY;

	private Transform cameraTransform;
	private Transform[] layers;
	private float viewZone = 10;
	private int leftIndex;
	private int rightIndex;
	private float lastCameraX;
	private float lastCameraY;
	private float backgroundPosY;
	private float backgroundPosZ;

	private void Start() {
		cameraTransform = GameObject.Find("MainCamera").transform;
		lastCameraX = cameraTransform.position.x;
		lastCameraY = cameraTransform.position.y;
		layers = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			layers [i] = transform.GetChild (i);
		}
		backgroundPosY = this.layers[0].transform.position.y;
		backgroundPosZ = this.layers[0].transform.position.z;
		leftIndex = 0;
		rightIndex = layers.Length - 1;
	}

	private void LateUpdate() {
		if (parallaxX) {
			float deltaX = cameraTransform.position.x - lastCameraX;
			transform.position += Vector3.right * (deltaX * parallaxSpeedX);
			lastCameraX = cameraTransform.position.x;
		}
		if (parallaxY) {
			float deltaY = cameraTransform.position.y - lastCameraY;
			transform.position += Vector3.up * (deltaY * parallaxSpeedY);
			lastCameraY = cameraTransform.position.y;
		}

		if (scrolling) {
			if (cameraTransform.position.x < (layers [leftIndex].transform.position.x + viewZone)) {
				ScrollLeft();
			}
			if (cameraTransform.position.x > (layers [rightIndex].transform.position.x - viewZone)) {
				ScrollRight();
			}	
		}
	}

	private void ScrollLeft() {
		int lastRight = rightIndex;
		layers [rightIndex].position = new Vector3((layers[leftIndex].position.x - backgroundSize),backgroundPosY,backgroundPosZ);
		leftIndex = rightIndex;
		rightIndex--;
		if (rightIndex < 0) {
			rightIndex = layers.Length - 1;
		}
	}

	private void ScrollRight() {
		int lastLeft = leftIndex;
		layers [leftIndex].position = new Vector3((layers[rightIndex].position.x + backgroundSize),backgroundPosY,backgroundPosZ);
		rightIndex = leftIndex;
		leftIndex++;
		if (leftIndex == layers.Length) {
			leftIndex = 0;
		}
	}

}
