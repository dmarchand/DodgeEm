using UnityEngine;
using System.Collections;

public class RazorSawController : MonoBehaviour {

	public float MoveSpeed = 2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		this.rigidbody2D.velocity = new Vector2(-MoveSpeed, 0);
	}
}
