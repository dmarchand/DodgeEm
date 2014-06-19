using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {

	public int ProjectilesPerRound;
	public float RoundDelay;
	public float FireDelay;
	public float PathTravelTime;
    public string PathName;
    public float InitialDelay;

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



		timeUntilNextRound = InitialDelay;
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
                PathProjectileMovement movement = result.GetComponent<PathProjectileMovement>();
                movement.PathTravelTime = PathTravelTime;
                movement.PathName = PathName;
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
