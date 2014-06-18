using UnityEngine;
using System.Collections;

public class HorizontalProjectileMovement : MonoBehaviour {

	public float Speed;
	public HorizontalDirection Direction;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float moveSpeed = Speed * (Direction == HorizontalDirection.Left ? -1 : 1);

		this.transform.position = new Vector2(this.transform.position.x + moveSpeed * Time.deltaTime, this.transform.position.y);
	}

	public enum HorizontalDirection
	{
		Left,
		Right
	}
}
