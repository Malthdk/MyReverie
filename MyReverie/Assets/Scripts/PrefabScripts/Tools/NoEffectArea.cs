using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoEffectArea : MonoBehaviour {

	//Publics
	public static bool noTransformEffect;
	public static bool leftArea;

	void Start () 
	{
		
	}
	

	void Update () 
	{
		if (leftArea && !IntoLine.instance.transforming)
		{
			noTransformEffect = false;
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			leftArea = false;
			noTransformEffect = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "player")
		{
			leftArea = true;
		}
	}
}
