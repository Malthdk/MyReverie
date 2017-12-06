using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeText : MonoBehaviour {

	//Publics
	public GameObject target;
	public float smoothTimeX = 2f;
	public float smoothTimeY = 2f;
	public float xOffset;
	public float yOffset;
	public float removeTextTime;

	//Privates
	private Vector2 focusPosition;
	private Vector2 targetPosition;
	private float refVelocityX;
	private float refVelocityY;
	private TextMesh textMesh;
	private Color txtColor;
	private Font font;

	void Start () 
	{
		target = GameObject.FindWithTag("player");
		textMesh = GetComponent<TextMesh>();
		font = textMesh.font;

		StartCoroutine("RemoveText");
	}
	

	void LateUpdate () 
	{
		targetPosition.x = target.transform.position.x + xOffset;
		targetPosition.y = target.transform.position.y + yOffset;

		focusPosition.x = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref refVelocityX, smoothTimeX);
		focusPosition.y = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref refVelocityY, smoothTimeY);
		transform.position = (Vector3)focusPosition + Vector3.forward * -8.5f;	
	}

	//An idea is to make the fading and possible size increase of text proportional to player movement. 
	IEnumerator RemoveText()
	{
		yield return new WaitForSeconds(removeTextTime);

		while (txtColor.a > 0 || font.fontSize < 100)
		{
			FadeText(textMesh);
//			SizeText(textMesh);
			yield return new WaitForEndOfFrame();
		}
	}

	void FadeText(TextMesh txtMesh)
	{
		txtColor = txtMesh.color;
		txtColor.a -= 0.01f;
		txtMesh.color = txtColor;
	}
		
//	void SizeText(TextMesh txtMesh)
//	{
//		var txtSize = font.fontSize;
//		txtSize = txtMesh.fontSize;
//		txtSize += 1;
//		txtMesh.fontSize = txtSize;
//	}
}
