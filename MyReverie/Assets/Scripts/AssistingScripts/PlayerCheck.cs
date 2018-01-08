using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheck : MonoBehaviour {

	Water waterScript;

	void Start () 
	{
		waterScript = transform.parent.GetComponent<Water>();
	}
	

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			waterScript.hasPlayer = true;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			waterScript.hasPlayer = false;
		}
	}
}
