using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableText : MonoBehaviour {


	public float removeTextTime = 2f;

	// For movement
	private float speed = 0.5f;
	private Vector3 direction;

	//For size
	private float duration = 10f;
	private float time;
	private float t;

	//For fade
	private TextMesh textMesh;
	private Color txtColor;
	private Font font;

	void Start () 
	{
		direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
		textMesh = GetComponent<TextMesh>();
		font = textMesh.font;
	}

	void Update () 
	{
		if (gameObject.activeSelf)
		{
			transform.position += direction * speed * Time.deltaTime;
			StartCoroutine("RemoveText");

			time += Time.deltaTime;
			t = time / duration;

			if (time <= duration)
			{
				textMesh.characterSize = Mathf.Lerp(textMesh.characterSize, textMesh.characterSize + 0.001f, t);
			}

			//StartCoroutine("IncreaseTextSize");
		}
	}

	IEnumerator RemoveText()
	{
		yield return new WaitForSeconds(removeTextTime);

		while (txtColor.a > 0 || font.fontSize < 100)
		{
			FadeText(textMesh);
			yield return new WaitForEndOfFrame();
		}
	}
		
	void FadeText(TextMesh txtMesh)
	{
		txtColor = txtMesh.color;
		txtColor.a -= 0.01f;
		txtMesh.color = txtColor;
	}
}
