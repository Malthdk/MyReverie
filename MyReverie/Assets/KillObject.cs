using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillObject : MonoBehaviour {

	private SpriteRenderer sprite;
	private BoxCollider2D coli;
	private Vector2 tempSize;

	void Start () {
		sprite = this.gameObject.GetComponent<SpriteRenderer> ();
		coli = this.gameObject.GetComponent<BoxCollider2D> ();
		SetColliderSize ();
	}

	private void SetColliderSize() {
		tempSize = sprite.size;
		tempSize.x -= .7f;
		tempSize.y = .6f;
		coli.size = tempSize;
	}
}
