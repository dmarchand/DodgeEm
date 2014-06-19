using UnityEngine;
using System.Collections;

public class SpriteRotator : MonoBehaviour {

    public float RotationSpeed;
    public int Direction;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(0, 0, RotationSpeed * Time.deltaTime * Direction);
	}
}
