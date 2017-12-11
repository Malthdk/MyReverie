using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatSeeking : MonoBehaviour {

	public float speed = 5;
	public float rotationSpeed = 200f;
	public GameObject target;

	private Rigidbody2D rgb;

	void Start () 
	{
		target = GameObject.FindGameObjectWithTag("player");

		rgb = GetComponent<Rigidbody2D>();
	}
	

	void FixedUpdate () 
	{
		Vector2 point2Target = (Vector2)transform.position - (Vector2)target.transform.position;

		point2Target.Normalize();

		float value = Vector3.Cross(point2Target, transform.up).z;

		/*
		if (value > 0)
		{
			rgb.angularVelocity = rotationSpeed;
		}
		else if (value < 0)
		{
			rgb.angularVelocity = -rotationSpeed;
		}
		else
		{
			rgb.angularVelocity = 0;
		}
		*/


		rgb.angularVelocity = rotationSpeed * value;

		rgb.velocity = transform.up*speed;
	}
}
