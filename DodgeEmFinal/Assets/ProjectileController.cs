using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	public int Damage;
	public string Name;
	public bool DestroyOnDeath;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if(DestroyOnDeath)
		{
			Destroy(this.gameObject);
		}

		Debug.Log("Collision");
	}
}
