using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {

	public int ProjectilesPerRound;
	public float RoundDelay;
	public float FireDelay;
	public float ProjectileSpeed;
	public HorizontalProjectileMovement.HorizontalDirection Direction;

	public GameObject ProjectilePrototype;


	bool isRoundActive;
	float timeUntilNextRound;
	float timeUntilNextShot;
	int numActiveProjectiles;

	// Use this for initialization
	void Start () {
		if (ProjectilePrototype == null) {
			throw new UnityException("No projectile set to this turret");
		}



		timeUntilNextRound = RoundDelay;
	}
	
	// Update is called once per frame
	void Update () {
		if(!isRoundActive) 
		{
			timeUntilNextRound -= Time.deltaTime;

			if(timeUntilNextRound <= 0) 
			{
				isRoundActive = true;
				timeUntilNextShot = 0;
			}
		}
		else 
		{
			if(timeUntilNextShot <= 0)
			{

				GameObject result = (GameObject)Instantiate(ProjectilePrototype);
				HorizontalProjectileMovement movement = result.GetComponent<HorizontalProjectileMovement>();
				movement.Speed = ProjectileSpeed;
				movement.Direction = Direction;
				result.transform.position = this.transform.position;
				timeUntilNextShot = FireDelay;
				numActiveProjectiles++;

				if(numActiveProjectiles >= ProjectilesPerRound)
				{
					numActiveProjectiles = 0;
					isRoundActive = false;
					timeUntilNextRound = RoundDelay;
				}

			}
			else
			{
				timeUntilNextShot -= Time.deltaTime;
			}
		}
	}
}
