using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {
	public LayerMask collisionMask;

	public const float skinWidth = .015f;			//Small field within the character for the ray to start in
	public int horizontalRayCount = 4;				//Number of horizontal rays
	public int verticalRayCount = 4;				//Number of Vertical rays

	[HideInInspector]
	public float horizontalRaySpacing;				//Space betweeen rays on horizontal rays
	[HideInInspector]
	public float verticalRaySpacing;				//Space between rays on vertical rays

	[HideInInspector]
	public BoxCollider2D collider;					//Calling boxcollider
	public RaycastOrigins raycastOrigins;			//Calling raycastorigins

	public virtual void Awake () 					// !!!Hed Start før og ikke Awake, hvis problemer - undersøg!!!
	{
		collider = GetComponent<BoxCollider2D>();	//Getting boxcollider				
	}

	public virtual void Start()
	{
		//CalculateRaySpacing ();						// !!Lå i Awake før!!!
	}
	public virtual void Update ()
	{
		UpdateRaycastOrigins();						//Now capable of scaling at run time
		CalculateRaySpacing();						//Now capable of scaling at run time
	}
	//RAYCAST ORIGIN
	public void UpdateRaycastOrigins() 
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);
		
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);		//Setting bottom left corner origin as minium x and minimum y
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);		//Setting bottom right corner origin as maximum x and minimum y
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);			//Setting top left corner origin as minimum x and maximum y
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);			//Setting top right corner origin as maximum x and maximum y

	}

	//SPACE BETWEEN RAYS
	public void CalculateRaySpacing() 
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
		
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount -1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount -1);
	}
	
	public struct RaycastOrigins 
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
