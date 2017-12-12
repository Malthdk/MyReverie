using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

	//Publics
	public Rigidbody2D rgb;
	public bool noTransKey;
	public static bool hasKey = false;

	//Privates
	private IntoLine.Direction playerDirection;
	private ConstantForce2D customGravity;
	private float gravityForceAmount;
	private GameObject keyPositionObj;
	private Vector3 startLocation;
	private Quaternion startRotation;
	public Vector2 startGravity;

	void Start () 
	{
		customGravity = GetComponentInParent<ConstantForce2D>();
		gravityForceAmount = rgb.mass * Physics2D.gravity.magnitude;
		keyPositionObj = GameObject.Find("Player").transform.GetChild(0).GetChild(0).gameObject;

		AdjustGravity();
		startLocation = transform.position;
		startRotation = transform.rotation;
		startGravity = customGravity.force;
	}
	

	void Update () 
	{
		if (hasKey)
		{
			playerDirection = GameObject.Find("Player").GetComponent<IntoLine>().direction;
			AdjustGravity();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			if(hasKey)
			{
				DropKey();	
			}
		}
		if (noTransKey)
		{
			if (IntoLine.instance.transforming)
			{
				transform.parent.position = transform.parent.transform.parent.transform.parent.position;
				DropKey();
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			PickUpKey(other.transform);
		}
		if (other.tag == "killTag" && !hasKey)
		{
			ResetPosition();
		}
	}

	void PickUpKey(Transform other)
	{
		transform.parent.gameObject.transform.parent = other.GetChild(0).transform;
		rgb.simulated = false;
		hasKey = true;

		//Setting position and rotation to be that of the predefined keyPosition object
		transform.parent.position = keyPositionObj.transform.position;
		transform.parent.rotation = keyPositionObj.transform.rotation;
	}

	void DropKey()
	{
		transform.parent.gameObject.transform.parent = null;
		rgb.simulated = true;
		hasKey = false;
	}

	void ResetPosition()
	{
		customGravity.force = startGravity;
		transform.parent.position = startLocation;
		transform.parent.rotation = startRotation;
		transform.parent.gameObject.transform.parent = null;
		hasKey = false;
		rgb.simulated = false;
		rgb.velocity = new Vector3(0f,0f,0f); 
		rgb.angularVelocity = 0f;
	}

	void AdjustGravity()
	{
		//Setting gravity according to player direction
		if (playerDirection == IntoLine.Direction.Floor)
		{
			customGravity.force = new Vector2 (0, -gravityForceAmount); // gravity down
		}
		else if (playerDirection == IntoLine.Direction.Cieling)
		{
			customGravity.force = new Vector2 (0, gravityForceAmount); // gravity up
		}
		else if (playerDirection == IntoLine.Direction.Leftwall)
		{
			customGravity.force = new Vector2 (-gravityForceAmount, 0); // gravity left
		}
		else if (playerDirection == IntoLine.Direction.Rightwall)
		{
			customGravity.force = new Vector2 (gravityForceAmount, 0); // gravity right
		}
	}
}
