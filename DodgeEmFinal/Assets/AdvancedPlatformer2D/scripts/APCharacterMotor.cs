/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Advanced Platformer 2D/CharacterMotor")]

public class APCharacterMotor : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	[System.Serializable]
	public class CharacterRay2D
	{
		public Vector2 m_position = Vector2.zero;  	// current local position
		public float m_penetration = 0f;			// penetration depth inside body volume
		public float m_extraDistance = 0.1f;		// extra distance used for detecting environment

		[HideInInspector]
		public Collider2D m_collider;				// last collider hit
	}

	// Initial settings
	public CharacterRay2D[] m_RaysGround;			// list of rays ground
	public CharacterRay2D[] m_RaysUp;				// list of rays up
	public CharacterRay2D[] m_RaysFront;			// list of rays front
	public CharacterRay2D[] m_RaysBack;				// list of rays back

	public Vector2 m_velocity = Vector2.zero;						// current velocity
	public bool m_faceRight = true; 								// is facing right
	public LayerMask m_rayLayer = Physics2D.DefaultRaycastLayers;	// ray cast collision layer
	public bool m_DrawRays = false;									// show/hide rays


	// Autobuilder
	[System.Serializable]
	public class AutoBuilder
	{
		public int m_rayCountX = 2;
		public int m_rayCountY = 3;
		public float m_extraDistanceFront = 0.1f;
		public float m_extraDistanceBack = 0.1f;
		public float m_extraDistanceUp = 0.1f;
		public float m_extraDistanceDown = 0.1f;
		
		public Vector2 m_rayXBoxScale = new Vector2(0.9f, 0.6f);
		public Vector2 m_rayYBoxScale = new Vector2(0f, 0.9f);
	}

	public AutoBuilder m_autoBuilder = new AutoBuilder();

	// list of ray types
	public enum RayType
	{
		Ground,
		Up,
		Front,
		Back
	}


	////////////////////////////////////////////////////////
	// Services

	// Check if touching
	public bool TouchGround { get {return m_touchGround;} }
	public bool TouchHead { get {return m_touchHead;}	}
	public bool TouchFront { get {return m_touchFront;}	}
	public bool TouchBack { get {return m_touchBack;}	}

	// Is facing right
	public bool FaceRight { get {return m_faceRight;}	}

	// Project current velocity on ground, valid only if character is touching ground
	public Vector2 ComputeVelocityOnGround()
	{ 
		return ProjectOnGround(m_velocity);
	}

	// Quick access to box collider
	public BoxCollider2D GetBoxCollider() { return m_boxCollider; }

	// Get normal of touched ground
	public Vector2 GetGroundNormal() { return m_groundNormal; }

	// Get normal of touched ground in local character space
	public Vector2 GetGroundNormalLs() { return m_groundNormalLs; }

	// Get distance to ground
	public float GetDistanceToGround() { return m_distToGround; }

	// Get ground game object
	public GameObject GetGroundGameObject() { return m_groundGameObject; }

	// Allow local offset of rays at runtime
	public Vector2 Offset {
		get {
			return m_offset;
		}
		set {
			m_offset = value;
		}
	}

	// Allow local scale of rays at runtime
	public Vector2 Scale {
		get {
			return m_scale;
		}
		set {
			m_scale = value;
		}
	}

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL

	// internal low level variables
	RaycastHit2D[] m_rayResults = new RaycastHit2D[8]; 	// buffer for ray result for each ray
	float m_PosErrorMaxVel = 100f; 						// max velocity to correct position error
	float m_allowedPenetration = 0.01f; 				// maximum penetration allowed before position correction is applied
	private BoxCollider2D m_boxCollider;
	APCharacterController m_character;					// ref to character controller owning this motor, can be null

	// runtime attributes
	Vector2 m_curPos;
	Vector2 m_posError;
	bool m_touchGround;
	bool m_touchHead;
	bool m_touchFront;
	bool m_touchBack;
	Vector2 m_offset = Vector2.zero;
	Vector2 m_scale = Vector2.one;
	Vector2 m_groundNormal;
	Vector2 m_groundNormalLs;
	float m_distToGround;
	GameObject m_groundGameObject;

	// Use this for initialization
	void Awake()
	{
		m_boxCollider = GetComponent<BoxCollider2D>();
		m_character = GetComponent<APCharacterController>();
	}

	void Start()
	{
		ClearRuntimeValues();
	}

	void ClearRuntimeValues()
	{
		m_velocity = Vector2.zero;
		m_curPos = Vector2.zero;
		m_posError = Vector2.zero;
		m_touchGround = false;
		m_touchHead = false;
		m_touchFront = false;
		m_touchBack = false;
		m_groundNormal = Vector2.up;
		m_groundNormalLs = Vector2.up;
		m_distToGround = 0f;
		m_offset = Vector2.zero;
		m_scale = Vector2.one;
		m_groundGameObject = null;
	}

	Vector2 ProjectOnGround(Vector2 proj)
	{
		Vector2 groundPlane = Vector2.right;
		float fDot = Vector2.Dot(groundPlane, m_groundNormal);
		if(Mathf.Abs(fDot) > float.Epsilon)
		{
			Vector3 perpAxis = Vector3.Cross(groundPlane, m_groundNormal);
			groundPlane = Vector3.Cross(m_groundNormal, perpAxis);
			groundPlane.Normalize();
		}

		return groundPlane * Vector2.Dot(proj, groundPlane);
	}

	public void Flip ()
	{
		// Switch the way the player is labelled as facing.
		m_faceRight = !m_faceRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Move()
	{
		// compute move offset
		Vector2 moveOffset = m_velocity * Time.deltaTime;

		// Initial position
		m_curPos = transform.position;		

		// Clear detection variables
		m_posError = Vector2.zero;
		m_touchHead = false;
		m_touchFront = false;
		m_touchBack = false;

		ClearRaysCollisionInfo(m_RaysGround);
		ClearRaysCollisionInfo(m_RaysUp);
		ClearRaysCollisionInfo(m_RaysFront);
		ClearRaysCollisionInfo(m_RaysBack);

		Vector2 v2InitialPos = m_curPos;

		// Move in 4 directions, constraint/correct movement if needed on each axis, prevent correction in opposite direction
		// NB : this method is not exact but this works for small displacements
		// 		update this when we will have volume cast available with Box2D !
		//		Notice that order is important here, this should word in most 2d horizontal platformers
		bool bVerticalCorr = MoveVertical(ref moveOffset, false, true);
		bool bHorizCorr = MoveHorizontal(ref moveOffset, true, true);
		MoveHorizontal(ref moveOffset, false, !bHorizCorr);
		MoveVertical(ref moveOffset, true, !bVerticalCorr);

		// move with physic engine so collisions with dynamic environment are valid
		rigidbody2D.velocity = (m_curPos - v2InitialPos) / Time.deltaTime; 
		 
		// compute final velocity (position correction must not inject any velocity !)
		m_velocity = (m_curPos - v2InitialPos - m_posError) / Time.deltaTime; 

		// draw vel
		if(m_DrawRays)
		{
			Vector3 vel = rigidbody2D.velocity;
			Debug.DrawLine(transform.position, transform.position + vel, Color.green);
		}

	}

	void ClearRaysCollisionInfo(CharacterRay2D[] rays)
	{
		for(int i = 0; i < rays.Length; i++)
		{
			CharacterRay2D curRay = rays[i];
			curRay.m_collider = null;
		}
	}

	bool MoveVertical (ref Vector2 moveOffset, bool bDown, bool bCorrectPos) 
	{
		bool bPenetrated = false;
		float fMinPenetration = 0f;
		Vector2 rayDir = bDown ? -transform.up : transform.up;
		float fMoveOffsetOnRay = Vector2.Dot(moveOffset, rayDir);
		float fMaxAllowedMove = Mathf.Abs(fMoveOffsetOnRay);
		bool bCorrectVel = fMoveOffsetOnRay > 0f;


		float fClosestGroundHit = float.MaxValue;
		Vector2 v2SavedPos = transform.position;
		transform.position = m_curPos;

		if(bDown)
		{
			m_touchGround = false;
			m_groundGameObject = null;
			m_distToGround = 0f;
		}
			
			// Launch ground cast from destination position
		CharacterRay2D[] rays = bDown ? m_RaysGround : m_RaysUp;
		for(int i = 0; i < rays.Length; i++)
		{
			CharacterRay2D curRay = rays[i];
			float fRayPen = curRay.m_penetration * Mathf.Abs(transform.lossyScale.y) * m_scale.y;
			float fCurRayLength = fRayPen + (Mathf.Max(curRay.m_extraDistance, 1f)); // make sure ground is detected far enough for slope down snapping

			// add move offset in raycast if needed
			if(bCorrectVel)
			{ 
				fCurRayLength += Mathf.Abs(fMoveOffsetOnRay);
			}

			// launch ray
			Vector2 rayOrigin = transform.TransformPoint(Vector2.Scale(curRay.m_position , m_scale) + m_offset);

			if(m_DrawRays)
				Debug.DrawLine(rayOrigin, rayOrigin + rayDir * fCurRayLength);

			int hitCount = Physics2D.RaycastNonAlloc(rayOrigin, rayDir, m_rayResults, fCurRayLength, m_rayLayer);
			if(hitCount > 0)
			{
				float fClosestHit = float.MaxValue;
				for(int hitId = 0; hitId < hitCount; hitId++)
				{
					RaycastHit2D hit = m_rayResults[hitId];

					// ignore character himself
					if(hit.rigidbody == rigidbody2D)
						continue;
										
					// ignore triggers
					if(hit.collider.isTrigger)
						continue;

					// compute distance of penetration
					float fHitLength = hit.fraction * fCurRayLength;
					float fPen = fHitLength - fRayPen;

					// launch callback 
					if(hit.collider != null)
					{
						APHitable hitable = hit.collider.GetComponent<APHitable>();
						if(hitable)
						{
							// ignore hit if requested
							if(!hitable.OnCharacterTouch(m_character, bDown ? RayType.Ground : RayType.Up, hit, fPen))
							{
								continue;
							}
						}
					}

					// ignore insides
					if(hit.fraction == 0f)
						continue;

					// ignore non opposite normals in local space
					Vector2 localHitNormal = transform.InverseTransformDirection(hit.normal);
					if((bDown && localHitNormal.y <= 0f) || (!bDown && localHitNormal.y >= 0f))
						continue;

					if(m_DrawRays)
						Debug.DrawLine(hit.point, hit.point + hit.normal * 0.1f, Color.red);

					// keep hit with maximum penetration
					if((fPen + m_allowedPenetration) < fMinPenetration)
					{
						bPenetrated = true;
						fMinPenetration = fPen;
					}

					// Compute velocity correction
					if(bCorrectVel)
					{
						fMaxAllowedMove = Mathf.Min(fMaxAllowedMove, fHitLength - fRayPen);
						fMaxAllowedMove = Mathf.Max(0f, fMaxAllowedMove); // cannot be negative
					}

					// we are in skin width
					if(bDown)
					{
						m_touchGround = true;

						// keep normal of closest hit for all rays
						if(fPen < fClosestGroundHit)
						{					
							fClosestGroundHit = fPen;
							m_groundNormal = hit.normal;
							m_groundGameObject = hit.collider.gameObject;
							m_distToGround = fPen;
						}
					}
					else
					{
						m_touchHead = true;
					}

					// save collider of closest hit for this ray
					if(hit.fraction < fClosestHit)
					{
						fClosestHit = hit.fraction;
						curRay.m_collider = hit.collider;
					}
				}
			}
		}

		Vector2 fPrevPos = m_curPos;

		// prevent velocity along ray axis
		if(bCorrectVel)
		{
			float fDot = Vector2.Dot(moveOffset, rayDir);
			moveOffset += rayDir * (fMaxAllowedMove - fDot);

			// integrate
			m_curPos += rayDir * (fMaxAllowedMove);
		}

		// correction by position
		if(bPenetrated && bCorrectPos)
		{
			Vector2 v2PosErr = Mathf.Max(fMinPenetration, -m_PosErrorMaxVel * Time.deltaTime) * rayDir;
			m_curPos += v2PosErr;

			// keep this value so it is removed from velocity computing later
			m_posError += v2PosErr;
		}

		// fix distance to ground
		if(bDown && m_touchGround)
		{
			m_groundNormalLs = transform.InverseTransformDirection(m_groundNormal);
			m_distToGround += Vector2.Dot(fPrevPos - m_curPos, rayDir);
		}

		transform.position = v2SavedPos;
		return bPenetrated;
	}

	bool MoveHorizontal (ref Vector2 moveOffset, bool bFront, bool bCorrectPos) 
	{
		bool bPenetrated = false;
		float fMinPenetration = 0f;
		bool bRayDirRight = (m_faceRight && bFront) || (!m_faceRight && !bFront);
		Vector2 rayDir = bRayDirRight ? transform.right : -transform.right;
		float fMoveOffsetOnRay = Vector2.Dot(moveOffset, rayDir);
		bool bCorrectVel = fMoveOffsetOnRay > 0f;
		float fMaxAllowedMove = Mathf.Abs(fMoveOffsetOnRay);

		Vector2 v2SavedPos = transform.position;
		transform.position = m_curPos;

		// Launch ground cast from destination position
		CharacterRay2D[] rays = bFront ? m_RaysFront : m_RaysBack;
		for(int i = 0; i < rays.Length; i++)
		{
			CharacterRay2D curRay = rays[i];
			float fRayPen = curRay.m_penetration * Mathf.Abs(transform.lossyScale.x) * m_scale.x;
			float fCurRayLength = fRayPen + curRay.m_extraDistance;
			
			// add move offset if needed
			if(bCorrectVel)
			{
				fCurRayLength += Mathf.Abs(fMoveOffsetOnRay);
			}
			
			// launch ray
			Vector2 rayOrigin = transform.TransformPoint(Vector2.Scale(curRay.m_position, m_scale) + m_offset);

			if(m_DrawRays)
				Debug.DrawLine(rayOrigin, rayOrigin + rayDir * fCurRayLength);

			int hitCount = Physics2D.RaycastNonAlloc(rayOrigin , rayDir, m_rayResults, fCurRayLength, m_rayLayer);
			if(hitCount > 0)
			{
				float fClosestHit = float.MaxValue;
				for(int hitId = 0; hitId < hitCount; hitId++)
				{
					RaycastHit2D hit = m_rayResults[hitId];

					// ignore character himself
					if(hit.rigidbody == rigidbody2D)
						continue;

					// ignore triggers
					if(hit.collider.isTrigger)
						continue;

					// compute distance of penetration
					float fHitLength = hit.fraction * fCurRayLength;
					float fPen = fHitLength - fRayPen;

					// launch callback 
					if(hit.collider != null)
					{
						APHitable hitable = hit.collider.GetComponent<APHitable>();
						if(hitable)
						{
							// ignore hit if requested
							if(!hitable.OnCharacterTouch(m_character, bFront ? RayType.Front : RayType.Back, hit, fPen))
							{
								continue;
							}
						}
					}

					// ignore insides
					if(hit.fraction == 0f)
						continue;

					// ignore non facing normals or in invalid slope range
					Vector2 localHitNormal = transform.InverseTransformDirection(hit.normal);
					if((bRayDirRight && localHitNormal.x > 0f) || (!bRayDirRight && localHitNormal.x < 0f))
						continue;

					if(m_DrawRays)
						Debug.DrawLine(hit.point, hit.point + hit.normal * 0.1f, Color.red);

					// if we are in skin width
					if(fHitLength <= fRayPen + curRay.m_extraDistance)
					{
						if(bFront)
						{
							m_touchFront = true;
						}
						else
						{
							m_touchBack = true;
						}

						// keep hit with maximum penetration (allow small penetration to prevent djitering)
						if((fPen + m_allowedPenetration) < fMinPenetration)
						{
							bPenetrated = true;
							fMinPenetration = fPen;
						}

						// save collider of closest hit in skin width
						if(hit.fraction < fClosestHit)
						{
							fClosestHit = hit.fraction;
							curRay.m_collider = hit.collider;
						}
					}
					
					// Compute velocity correction
					if(bCorrectVel)
					{                                         
						fMaxAllowedMove = Mathf.Min(fMaxAllowedMove, fHitLength - fRayPen);
						fMaxAllowedMove = Mathf.Max(0f, fMaxAllowedMove); // cannot be negative
					}
				}
			}
		}
		
		// correction by velocity
		if(bCorrectVel)
		{
			float fDot = Vector2.Dot(moveOffset, rayDir);
			moveOffset += rayDir * (fMaxAllowedMove - fDot);
			
			// integrate
			m_curPos += rayDir * (fMaxAllowedMove);
		}
		
		// correction by position
		if(bPenetrated && bCorrectPos)
		{
			Vector2 v2PosErr = Mathf.Max(fMinPenetration, -m_PosErrorMaxVel * Time.deltaTime) * rayDir;
			m_curPos += v2PosErr;
			
			// keep this value
			m_posError += v2PosErr;
		}

		transform.position = v2SavedPos;
		return bPenetrated;
	}
}
