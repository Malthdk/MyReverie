using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDrawer : MonoBehaviour {

	public List<Transform> otherTrans;

	public float position;
	public float minScale = 1.175789f;
	public float maxScale = 5.675f;
	public float minPlatPos = 48.19f;
	public float maxPlatPos = 58.19f;

	public bool scaleX, scaleY;

	void Start () 
	{
		CalcPosition();
	}
	

	void Update () 
	{
		CalcPosition();
		HandleSize();
	}

	void HandleSize()
	{
		for (int i = 0; i < otherTrans.Count; i++)
		{
			if (scaleX)
			{
				otherTrans[i].localScale = new Vector3(Map(position, minPlatPos, maxPlatPos, minScale, maxScale), 0f, 1f);	
			}
			else if (scaleY)
			{
				otherTrans[i].localScale = new Vector3(0.437499f, Map(position, minPlatPos, maxPlatPos, minScale, maxScale), 1f);
			}
		}	
	}

	public float Map(float value, float inMin, float inMax, float outMin, float outMax)
	{
		return(value-inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}

	public void CalcPosition()
	{
		if (scaleX)
		{
			position = transform.position.x;	
		}
		else if (scaleY)
		{
			position = transform.position.y;
		}
	}

}
