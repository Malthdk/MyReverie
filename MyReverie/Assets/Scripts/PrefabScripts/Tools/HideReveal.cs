using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideReveal : MonoBehaviour {

	//Publics
	public OnAndOff onAndOff;
	public Color offColor;
	public Color onColor;
	//Privates
	private BoxCollider2D bCollider;
	private SpriteRenderer sRenderer;
	//private ParticleSystem pSystem;
	private bool canSwitch = true;


	public enum OnAndOff
	{
		On,
		Off
	}

	void Start () 
	{
		bCollider = GetComponent<BoxCollider2D>();
		sRenderer = GetComponent<SpriteRenderer>();
		//pSystem = transform.GetComponentInChildren<ParticleSystem>();
	}

	void Update () 
	{
		if (!IntoLine.instance.transforming)
		{
			canSwitch = true;
		}

		switch(onAndOff)
		{
		case OnAndOff.On:
			bCollider.enabled = true;
			sRenderer.color = onColor;
			//pSystem.Play();
			break;

		case OnAndOff.Off:
			bCollider.enabled = false;
			sRenderer.color = offColor;
			//pSystem.Stop();
			//pSystem.Clear();
			break;
		}

		if (!NoEffectArea.noTransformEffect)
		{
			if (IntoLine.instance.transforming && canSwitch)
			{
				canSwitch = false;
				if (onAndOff == OnAndOff.On)
				{
					onAndOff = OnAndOff.Off;
				}
				else if (onAndOff == OnAndOff.Off)
				{
					onAndOff = OnAndOff.On;
				}
			}	
		}
	}
}
