/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

public abstract class APGUI : MonoBehaviour 
{
    // called when a bullet has been launched
	// - launcher : character controller launching the attack
	// - attack : ranged attack whom launched the bullet
	public abstract void OnBulletLaunched(APCharacterController launcher, APRangedAttack attack);
}

