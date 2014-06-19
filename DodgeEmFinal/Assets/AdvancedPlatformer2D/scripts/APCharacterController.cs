/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(APCharacterMotor))]
[AddComponentMenu("Advanced Platformer 2D/CharacterController")]

public class APCharacterController : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	[System.Serializable]
	public class Inputs
	{
		public APInputJoystick m_axisX = new APInputJoystick("Horizontal", true);
		public APInputJoystick m_axisY = new APInputJoystick("Vertical", false);
		public APInputButton m_runButton;
	}

	public enum eAutoMove
	{
		Disabled,
		Right,
		Left
	}

	[System.Serializable]
	public class Settings
	{
		public bool m_alwaysRun = true;							// run always enabled
		public float m_walkSpeed = 5f;							// in m/s
		public float m_runSpeed = 8f;							// in m/s
		public float m_acceleration = 30f;						// in m/s²
		public float m_deceleration = 20f;						// in m/s²
		public bool m_stopOnRotate = true;						// directly stop when rotating on ground
		public float m_frictionDynamic = 1f;					// friction when accelerating
		public float m_frictionStatic = 1f;						// friction when releasing input / moving in opposite direction
		public bool m_autoRotate = true;						// enable auto rotate
		public float m_gravity = 50f;							// in m/s²
		public float m_airPower = 10f;							// acceleration when in air
		public float m_groundFlipMaxSpeed = -1f;				// maximum allowed speed on ground for flipping player
		public float m_airFlipMaxSpeed = -1f;					// maximum allowed speed in air for flipping player
		public float m_maxAirSpeed = 8f;						// max horizontal speed in air
		public float m_maxFallSpeed = 20f;						// max fall speed
		public float m_crouchSizePercent = 0.5f;				// collision box reduce size percent when crouched
		public float m_uncrouchMinSpeed = 2f;					// used to get unstuck from a crouch
		public bool m_enableCrouchedRotate = true;				// enable rotate while crouched
		public eAutoMove m_autoMove = eAutoMove.Disabled;		// force player to always move in a direction
		public string m_haltAutoMove;							// input for pausing the auto move

		// maxspeed factor in function of current ground slope angle (negative values for down slope, positive value for up)
		// NB : factor must be in [0,1] range
		public AnimationCurve m_slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(45, 1), new Keyframe(70, 1), new Keyframe(70, 0), new Keyframe(90, 0));
	}

	[System.Serializable]
	public class GroundAlign
	{
		public bool m_groundAlign = false;				// align player with ground normal
		public bool m_jumpAlign = false;				// is jump direction aligned with ground normal or not
		public bool m_alignAirMove = false;				// align air move direction with player direction
		public bool m_forceInAirVerticalAlign = true;	// make sure player is always aligned with vertical axis while in air
	}

	[System.Serializable]
	public class DownSlopeSliding
	{
		public bool m_enabled = false;		// enable or not down slope sliding
		public float m_slidingPower = 1f;	// power to make player move downside
		public float m_slopeMinAngle = 20f;	// min slope angle at which sliding occurs (in degrees)
	}

	[System.Serializable]
	public class Advanced
	{
		public float m_groundSnapAngle = 60f;			// down slope max angle at which player is snapped if on ground
		public float m_maxVerticalVelForSnap = 0.01f;	// max vertical velocity for ground snapping if not previously on ground
		public float m_minTimeBetweenTwoJumps = 0.3f;	// minimum time between 2 consecutive jumps
		public float m_maxVelDamping = 0.5f;			// damping when max velocity is reached
		public uint m_carryRayIndex = 0;				// used only when different carriers are detected, same ray is used each time
		public float m_downPushDuration = 0.1f;			// time during which down button status can lasts even if player has release button
		public float m_maxAttackDuration = 10f;			// max time an attack can occur (to prevent infinite state)
		public bool m_debugDraw = false;				// enable debug drawing
		public bool m_enableSendMessages = false;		// enable/disable calls to SendMessage related to this character
	}

	[System.Serializable]
	public class Animations
	{
		public string m_stand;
		public string m_run;
		public string m_walk;
		public string m_crouch;
		public string m_inAir;
		public string m_inAirDown;
		public string m_wallJump;
		public string m_wallSlide;
		public string m_glide;
		public float m_minAirTime = 0f;				// min time in air before launching InAir animation
		public bool m_walkAnimFromInput = false;	// use input filtered value for animation, otherwise use ground speed

		// animation speed in function of input filtered value, 0 means go to stand
		public AnimationCurve m_animFromInput = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		// animation speed in function of ground speed
		public AnimationCurve m_animFromSpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(8, 1));
	}

	[System.Serializable]
	public class SettingsJump
	{
		public bool m_enabled = true;					// enabled status
		public APInputButton m_button;					// disabled if empty
		public float m_minHeight = 3f;					// min jump height
		public float m_maxHeight = 3f;					// max jump height if pushing jump input
		public int m_airJumpCount = 0;					// number of additional jump you can make while in air
	}

	[System.Serializable]
	public class SettingsWallJump
	{
		public bool m_enabled = true;
		public APInputButton m_button;						// disabled if empty
		public Vector2 m_jumpPower = new Vector2(10f, 8f);	// power of jump
		public float m_timeBeforeJump = 0.1f;				// time to be snapped on wall before jumping
		public float m_timeBeforeFlip = 0.1f;				// time before flipping after jump (negative to disable)
		public float m_disableAutoRotateTime = 0.3f;		// min time after wall jump to prevent autorotate
		public int[] m_rayIndexes;							// list of ray indexes to use for wall detection, empty means all rays
	}

	[System.Serializable]
	public class SettingsWallSlide
	{
		public bool m_enabled = true;
		public float m_friction = 8f;			// friction of the wall when sliding on it
		public float m_minTime = 0f;			// minimum time to slide down while pushing input against wall before launching state
		public float m_minSpeed = 0.1f;			// minimum down speed for enabling wall sliding
		public int[] m_rayIndexes;				// list of ray indexes to use for wall detection, empty means all rays
	}

	[System.Serializable]
	public class SettingsMeleeAttacks
	{
		public bool m_enabled = false;
		public APMeleeAttack[] m_attacks;						// list of melee attacks
	}

	[System.Serializable]
	public class SettingsRangedAttacks
	{
		public bool m_enabled = false;
		public APRangedAttack[] m_attacks;								// list of available attacks
	}

	[System.Serializable]
	public class SettingsGlide
	{
		public bool m_enabled = true;					// enabled status
		public APInputButton m_button;					// disabled if empty
		public float m_gravityFactor = 0.5f;			// factor to apply to gravity (in [0,1] range)
		public float m_lateralMoveFactor = 0.5f;		// factor to apply to lateral move (in [0,1] range)
		public float m_maxDuration = 3f;				// max duration a glide can occur
		public int m_maxCount = 1;						// number of glides you can do before touching back the ground
		public float m_minAirTimeBeforeGlide = 0.1f;	// minimum time while in air before glide is allowed
	}

	public Inputs m_inputs;							// inputs handler (common for all interactive objects, we may add one by object if needed...)
	public Settings m_basic;						// basic settings
	public DownSlopeSliding m_downSlopeSliding;		// down slope sliding settings
	public GroundAlign m_groundAlign;				// align player rotation against contact normal
	public Animations m_animations;					// settings for animations
	public SettingsJump m_jump;						// jump settings
	public SettingsWallJump m_wallJump;				// wall jump settings
	public SettingsWallSlide m_wallSlide;			// wall slide settings
	public APLadder.Settings m_ladder;				// settings for ladders (can be overriden for each ladder)
	public APRailings.Settings m_railings;			// settings for railings (can be overriden for each railings)
	public SettingsMeleeAttacks m_meleeAttacks; 	// settings for melee attacks
	public SettingsRangedAttacks m_rangedAttacks;	// settings for ranged attacked system
	public SettingsGlide m_glide;					// settings for glide
	public Advanced m_advanced;						// advanced low level settings
	public APGUI m_GUI;								// link to our custom GUI if any

	// Accessors
	public APCharacterMotor GetMotor()
	{
		return m_motor;
	}

	public Animator GetAnim()
	{
		return m_anim;
	}

	public float GetGroundSpeed()
	{
		return m_groundSpeed;
	}

	public bool IsCrouched()
	{
		return (GetState() == State.Crouch) || (GetRequestedState() == State.Crouch) || IsAttackingCrouched();
	}

	public bool IsCarried() { return m_carrier != null; }
	public Vector2 GetCarrierOffset() { return m_carrierOffset; }
	public bool IsFacingRight() { return m_motor.m_faceRight; }
	
	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL

	// list of different states
	enum State
	{
		Standard = 0,
		Crouch,
		WallJump,
		WallJumpInAir,
		MeleeAttack,
		RangedAttack,
		Glide,
		WallSlide
	}
	// finite state machine of character controller
	class Fsm : APFsm
	{
		/// Virtual method called when state enter, update or leave occur
		public override void OnStateUpdate(APFsmStateEvent eEvent, uint a_oState)
		{
			switch ((State)a_oState)
			{
				case State.Standard:
					m_controller.StateStandard(eEvent);
					break;
				case State.Crouch: 
					m_controller.StateCrouch(eEvent); 
					break;
				case State.WallJump: 
					m_controller.StateWallJump(eEvent); 
					break;
				case State.WallJumpInAir: 
					m_controller.StateWallJumpInAir(eEvent); 
					break;
				case State.MeleeAttack: 
					m_controller.StateMeleeAttack(eEvent); 
					break;
				case State.RangedAttack: 
					m_controller.StateRangedAttack(eEvent); 
					break;
				case State.Glide: 
					m_controller.StateGlide(eEvent); 
					break;
				case State.WallSlide: 
					m_controller.StateWallSlide(eEvent); 
					break;
			}
		}

		public APCharacterController m_controller;
	}
	//	private attributes
	APCharacterMotor m_motor;
	Animator m_anim;
	Fsm m_fsm = new Fsm();
	Collider2D[] m_overlapResult = new Collider2D[8];

	// basics
	float m_groundSpeed;
	bool m_isControlled;
	bool m_onGround;
	float m_speedFactor;
	bool m_sliding;	 // used only for animation
	GameObject m_lastGround;
	bool m_isInStand;
	bool m_requestJumpAnimation;
	bool m_jumpAnimLaunched;
	bool m_bForceDefer;

	//	jump
	float m_lastJumpTime;
	float m_animAirTime;
	float m_airTime;
	bool m_jumpDown;
	float m_jumpButtonTimeDown;
	int m_airJumpCount;
	bool m_deferJump;
	float m_deferJumpRatio;

	// crouch
	float m_crouchBoxOriginalSize;
	float m_crouchBoxOriginalCenter;

	// carry
	APCarrier m_carrier;
	Vector2 m_carrierOffset;

	// wall jump
	bool m_wallJumpFlipDone;
	bool m_wallJumpAutoRotate;

	// attacks
	int m_curAttackId;
	int m_curAttackAnimHash;
	bool m_attackCrouched;
	bool m_attackNoMove;
	float m_rangeAttackInitTime;
	APRangedAttack.ContextId m_rangedContextId;

	// glide
	int m_glideCount;
	bool m_glideDown;
	float m_glideButtonTimeDown;

	// deferring
	bool m_animLaunch;
	bool m_deferImpulse;
	bool m_deferVelocity;
	Vector2 m_deferImpulsePower;
	Vector2 m_deferedVelocity;

	// Use this for initialization
	void Awake()
	{
		m_motor = GetComponent<APCharacterMotor>();
		m_anim = GetComponent<Animator>();
		m_fsm.m_controller = this;
	}

	void OnDisable()
	{
		m_fsm.StopFsm();
	}

	void ClearRuntimeValues()
	{
		m_carrier = null;
		m_jumpDown = false;
		m_glideDown = false;
		m_isControlled = false;
		m_onGround = false;
		m_isInStand = false;
		m_requestJumpAnimation = false;
		m_jumpAnimLaunched = false;
		m_animAirTime = m_animations.m_minAirTime;
		m_airTime = 0f;
		m_speedFactor = 1f;
		m_lastJumpTime = 0f;
		m_crouchBoxOriginalSize = 1f;
		m_crouchBoxOriginalCenter = 0f;
		m_groundSpeed = 0f;
		m_sliding = false;
		m_curAttackId = -1;
		m_rangedContextId = APRangedAttack.ContextId.eStand;
		m_attackCrouched = false;
		m_attackNoMove = false;
		m_jumpButtonTimeDown = 0f;
		m_glideButtonTimeDown = 0f;
		m_airJumpCount = 0;
		m_lastGround = null;
		m_rangeAttackInitTime = 0f;
		m_deferJump = false;
		m_bForceDefer = true;
		m_deferJumpRatio = 0f;
		m_deferImpulse = false;
		m_deferVelocity = false;
		m_deferedVelocity = Vector2.zero;
		m_glideCount = 0;
		m_animLaunch = false;
		m_carrierOffset = Vector2.zero;
		m_deferImpulsePower = Vector2.zero;


		foreach (APMeleeAttack curAttack in m_meleeAttacks.m_attacks)
		{
			curAttack.timeDown = 0f;
			curAttack.inputStatus = false;
		}

		foreach (APRangedAttack curAttack in m_rangedAttacks.m_attacks)
		{
			curAttack.timeDown = 0f;
			curAttack.inputStatus = false;
		}

		SetState(State.Standard);
	}

	void Start()
	{
		ClearRuntimeValues();

		// collect collision info at first frame
		m_motor.Move();
		UpdateGroundStatus();
	}
	// Physic update
	void FixedUpdate()
	{
		if (APSettings.m_fixedUpdate)
		{
			UpdateController();
			UnsetInputs();
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (APSettings.m_fixedUpdate)
		{
			SetInputs();
		} 
		else
		{
			SetInputs();
			UpdateController();
			UnsetInputs();
		}
	}

	protected virtual void SetInputs()
	{
		if(m_jump.m_enabled && m_jump.m_button.IsSpecified() && m_jump.m_button.GetButtonDown())
		{
			m_jumpDown = true;
			m_jumpButtonTimeDown = Time.time;
		}

		if(m_glide.m_enabled && m_glide.m_button.IsSpecified() && m_glide.m_button.GetButtonDown())
		{
			m_glideDown = true;
			m_glideButtonTimeDown = Time.time;
		}

		if(m_meleeAttacks.m_enabled)
		{
			foreach (APMeleeAttack curAttack in m_meleeAttacks.m_attacks)
			{
				if (curAttack.m_button.IsSpecified() && curAttack.m_button.GetButtonDown())
				{
					curAttack.inputStatus = true;
					curAttack.timeDown = Time.time;
				}
			}
		}

		if(m_rangedAttacks.m_enabled)
		{
			foreach (APRangedAttack curAttack in m_rangedAttacks.m_attacks)
			{
				if (curAttack.m_button.IsSpecified() && curAttack.m_button.GetButtonDown())
				{
					curAttack.inputStatus = true;
					curAttack.timeDown = Time.time;
				}
			}
		}
	}

	protected void UnsetInputs()
	{
		// reset inputs after amount of time
		if(m_jumpDown && (Time.time >= m_jumpButtonTimeDown + m_advanced.m_downPushDuration))
		{
			m_jumpDown = false;
		}

		if(m_glideDown && (Time.time >= m_glideButtonTimeDown + m_advanced.m_downPushDuration))
		{
			m_glideDown = false;
		}

		foreach (APMeleeAttack curAttack in m_meleeAttacks.m_attacks)
		{
			if(curAttack.inputStatus && (Time.time >= curAttack.timeDown + m_advanced.m_downPushDuration))
			{
				curAttack.inputStatus = false;
			}
		}

		foreach (APRangedAttack curAttack in m_rangedAttacks.m_attacks)
		{
			if(curAttack.inputStatus && (Time.time >= curAttack.timeDown + m_advanced.m_downPushDuration))
			{
				// auto fire
				if(!curAttack.m_autoFire || !curAttack.m_button.GetButton())
					curAttack.inputStatus = false;
			}
		}
	}

	void LateUpdate()
	{
		HandleAnimation();
		HandleCarry();
	}

	protected void UpdateController()
	{
		// Update inputs in all case (may be used by other objects)
		HandleInputFilter();

		// Do nothing if controlled by something
		if (IsControlled)
			return;

		// Handle buffered requests before
		m_bForceDefer = false;
		if(m_deferJump)
		{
			Jump(m_deferJumpRatio);
			m_deferJump = false;
		}		
		if(m_deferImpulse)
		{
			AddImpulse(m_deferImpulsePower);
			m_deferImpulse = false;
			m_deferImpulsePower = Vector2.zero;
		}
		if(m_deferVelocity)
		{
			SetVelocity(m_deferedVelocity);
			m_deferVelocity = false;
			m_deferedVelocity = Vector2.zero;
		}

		// Update states
		m_fsm.UpdateFsm(Time.deltaTime);

		// Restore deferring mode
		m_bForceDefer = true;

		// Finally move character with its current velocity
		m_motor.Move();

		// Update our own touch ground status
		UpdateGroundStatus();
	}

	void StateStandard(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
			m_jumpAnimLaunched = false;
			m_requestJumpAnimation = !m_onGround;
		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			ApplyGravity();
			HandleCrouch();
			HandleHorizontalMove();
			HandleGlide();
			HandleWallSlide();
			HandleWallJump();
			HandleJump();
			HandleMeleeAttack();
			HandleRangedAttack();
			HandleAutoRotate();
			
			// launch in air animation event if not done
			if (!m_onGround && (m_animAirTime >= m_animations.m_minAirTime) && !m_jumpAnimLaunched)
			{
				m_requestJumpAnimation = true;
			}
		}
		break;

		case APFsmStateEvent.eLeave:
		{
		}
		break;
		}
	}
	
	void StateGlide(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
			m_glideCount++;
			m_animAirTime = m_animations.m_minAirTime;

			PlayAnim(m_animations.m_glide, 0f);
		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			if(m_fsm.GetFsmStateTime() >= m_glide.m_maxDuration || !m_glide.m_button.GetButton() || m_onGround)
			{
				SetState(State.Standard);
			}

			ApplyGravity();
			HandleHorizontalMove();
			HandleWallSlide();
			HandleWallJump();
			HandleJump();
			HandleMeleeAttack();
			HandleRangedAttack();
			HandleAutoRotate();
		}
		break;

		case APFsmStateEvent.eLeave:
		{
		}
		break;
		}
	}
	
	void StateWallSlide(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
			m_animLaunch = false; 

		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			float fWallFriction = 0f;
			if(ShouldWallSlide(out fWallFriction))
			{
				float stateTime = m_fsm.GetFsmStateTime();
				if(stateTime >= m_wallSlide.m_minTime)
				{
					// launch animation if not done during this state
					if(!m_animLaunch)
					{
						m_animLaunch = true;
						PlayAnim(m_animations.m_wallSlide, 0f);
					}

					// handle friction
					float fVerticalVel = ApplyDamping(m_motor.m_velocity.y, fWallFriction);
					m_motor.m_velocity.y = fVerticalVel;
				}
			}
			else
			{
				// leave state if no more respecting conditions
				SetState(State.Standard);
			}

			ApplyGravity();
			HandleHorizontalMove();
			HandleWallJump();
			HandleMeleeAttack();
			HandleRangedAttack();
			HandleAutoRotate();
		}
		break;

		case APFsmStateEvent.eLeave:
		{
		}
		break;
		}
	}

	void StateCrouch(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
			// go back to last crouch frame if coming from melee/ranged attack
			State ePrevState = GetPreviousState();
			PlayAnim(m_animations.m_crouch, ePrevState == State.MeleeAttack || ePrevState == State.RangedAttack ? 1f : 0f);
			if(m_advanced.m_enableSendMessages)
				SendMessage("APOnCharacterCrouch", SendMessageOptions.DontRequireReceiver);
		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			ApplyGravity();
			HandleCrouch();
			HandleHorizontalMove();
			HandleJump();
			HandleMeleeAttack();
			HandleRangedAttack();

			if(m_basic.m_enableCrouchedRotate)
				HandleAutoRotate();
		}
		break;
		}
	}

	void StateWallJump(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				PlayAnim(m_animations.m_wallJump, 0f);

				m_wallJumpAutoRotate = m_basic.m_autoRotate;
				m_airJumpCount = 0;
				m_glideCount = 0;
				m_lastJumpTime = Time.time;
				m_animAirTime = m_animations.m_minAirTime;

				// prevent auto rotate for a while
				if (m_wallJumpAutoRotate)
					m_basic.m_autoRotate = false;

				if(m_advanced.m_enableSendMessages)
					SendMessage("APOnCharacterWallJump", SendMessageOptions.DontRequireReceiver);

			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				// cancel any velocity
				m_motor.m_velocity = Vector2.zero;

				// wait for end of timer before effective jump
				if (m_fsm.GetFsmStateTime() >= m_wallJump.m_timeBeforeJump)
				{
					m_motor.m_velocity.y = m_wallJump.m_jumpPower.y;
					m_motor.m_velocity.x = m_wallJump.m_jumpPower.x * (m_motor.FaceRight ? -1f : 1f);
					SetState(State.WallJumpInAir);
				}
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				// restore initial auto rotate in all cases
				m_basic.m_autoRotate = m_wallJumpAutoRotate;				
			}
			break;
		}
	}

	void StateWallJumpInAir(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{				
			m_wallJumpAutoRotate = m_basic.m_autoRotate;
			m_wallJumpFlipDone = false;
		
			// prevent auto rotate for a while
			if (m_wallJumpAutoRotate)
				m_basic.m_autoRotate = false;
		}
		break;
			
		case APFsmStateEvent.eUpdate:
		{
			// go back to standard state as soon as touching ground
			if (m_onGround)
			{
				SetState(State.Standard);					
			} 
			else
			{
				// flip when needed
				bool bFlipEnabled = (m_wallJump.m_timeBeforeFlip >= 0f);
				if (!m_wallJumpFlipDone && bFlipEnabled && (m_fsm.GetFsmStateTime() >= m_wallJump.m_timeBeforeFlip))
				{
					m_motor.Flip();
					m_wallJumpFlipDone = true;
				}
			
				// enable back auto rotate if needed
				if (m_wallJumpAutoRotate && !m_basic.m_autoRotate && (m_fsm.GetFsmStateTime() >= m_wallJump.m_disableAutoRotateTime) && (m_wallJumpFlipDone || !bFlipEnabled))
				{
					m_basic.m_autoRotate = true;
				}

				// leave state if work is ended and no new state is requested
				if ((!bFlipEnabled || m_wallJumpFlipDone) && (!m_wallJumpAutoRotate || m_basic.m_autoRotate))
				{
					SetState(State.Standard);
				}

				ApplyGravity();
				HandleCrouch();
				HandleHorizontalMove();
				HandleAutoRotate();
			}
		}
		break;
			
		case APFsmStateEvent.eLeave:
		{
			// restore initial auto rotate in all cases
			m_basic.m_autoRotate = m_wallJumpAutoRotate;				
		}
		break;
		}
	}

	void StateMeleeAttack(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
			PlayAnim(m_curAttackAnimHash, 0f);			

			// clear buffer of hits & disable hit zones at init
			APMeleeAttack curAttack = m_meleeAttacks.m_attacks[m_curAttackId];
			foreach (APHitZone curHitZone in curAttack.m_hitZones)
			{
				curHitZone.attackHits.Clear();
			}

			if(m_advanced.m_enableSendMessages)
				SendMessage("APOnCharacterMeleeAttack", SendMessageOptions.DontRequireReceiver);
		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			// update state
			ApplyGravity();
			HandleCrouch();
			HandleHorizontalMove();
			HandleGlide();
			HandleWallJump();
			HandleJump();
			HandleAutoRotate();

			// Compute all hits for current attack
			APMeleeAttack curAttack = m_meleeAttacks.m_attacks [m_curAttackId];
			foreach (APHitZone curHitZone in curAttack.m_hitZones)
			{
				if (curHitZone.m_active && curHitZone.gameObject.activeInHierarchy)
				{
					Vector2 wsPos = curHitZone.transform.position;
					int hitCount = Physics2D.OverlapCircleNonAlloc(wsPos, curHitZone.m_radius, m_overlapResult, m_motor.m_rayLayer);
					for (int i = 0; i < hitCount; i++)
					{
						Collider2D curHit = m_overlapResult[i];

						// notify only hitable objects and not already hit by this hit zone
						APHitable hitable = curHit.GetComponent<APHitable>();
						if (hitable != null && !curHitZone.attackHits.Contains(hitable))
						{
							// alert hitable
							if(hitable.OnMeleeAttackHit(this, curHitZone))
							{
								curHitZone.attackHits.Add(hitable);
							}
						}
					}
				}
			}			
			
			// make sure state does not end infinitely	
			if ((m_fsm.GetFsmStateTime() > m_advanced.m_maxAttackDuration) && !IsNewStateRequested())
			{
				SetState(GetPreviousState());
			}
		}
		break;

		case APFsmStateEvent.eLeave:
		{
			m_curAttackId = -1;
		}
		break;
		}
	}

	// called when melee attack must end and go back to previous state
	void LeaveMeleeAttack()
	{
		if(GetState() == State.MeleeAttack)
		{
			SetState(GetPreviousState());
		}
	}

	void HandleMeleeAttack()
	{
		// check if attack allowed
		bool bCrouched = IsCrouched();
		if (m_meleeAttacks.m_enabled && (bCrouched || (GetState() == State.Standard)) && !IsNewStateRequested())
		{
			// parse list of attack, check if one is launched
			for (int i = 0; i < m_meleeAttacks.m_attacks.Length; i++)
			{
				APMeleeAttack curAttack = m_meleeAttacks.m_attacks [i];
				if (curAttack.inputStatus)
				{
					// make sure this attack is enabled for this state
					string sAttackAnim = bCrouched ? curAttack.m_animCrouched : (m_onGround ? curAttack.m_animStand : curAttack.m_animInAir);
					if (string.IsNullOrEmpty(sAttackAnim))
					{						
						continue;
					}

					// launch attack
					m_curAttackId = i;		
					m_curAttackAnimHash = Animator.StringToHash(sAttackAnim);
					m_attackCrouched = bCrouched;
					m_attackNoMove = true;

					SetState(State.MeleeAttack);
					return;
				}
			}			
		}
	}

	void StateRangedAttack(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
		case APFsmStateEvent.eEnter:
		{
		}
		break;

		case APFsmStateEvent.eUpdate:
		{
			// update state
			ApplyGravity();
			HandleCrouch();
			HandleHorizontalMove();
			HandleGlide();
			HandleWallJump();
			HandleJump();
			HandleAutoRotate();

			// make sure state does not end infinitely	
			if ((Time.time > m_rangeAttackInitTime + m_advanced.m_maxAttackDuration) && !IsNewStateRequested())
			{
				SetState(GetPreviousState());
			}
		}
		break;

		case APFsmStateEvent.eLeave:
		{
			m_curAttackId = -1;
		}
		break;
		}
	}

	bool InitRangedAttack(APRangedAttack newAttack, int id)
	{
		// make sure this attack is enabled for this state
		string sAttackAnim = null;
		bool bCrouched = IsCrouched();
		if (bCrouched)
		{
			sAttackAnim = newAttack.m_contextCrouched.m_anim;
			m_attackNoMove = true;
			m_rangedContextId = APRangedAttack.ContextId.eCrouched;
		}
		else if (m_onGround)
		{
			if(m_isInStand || string.IsNullOrEmpty(newAttack.m_contextRun.m_anim))
			{
				sAttackAnim = newAttack.m_contextStand.m_anim;
				m_rangedContextId = APRangedAttack.ContextId.eStand;
				m_attackNoMove = true;
			}
			else
			{
				// currently running
				sAttackAnim = newAttack.m_contextRun.m_anim;
				m_rangedContextId = APRangedAttack.ContextId.eRun;
				m_attackNoMove = false;
			}
		}
		else
		{
			sAttackAnim = newAttack.m_contextInAir.m_anim;
			m_rangedContextId = APRangedAttack.ContextId.eInAir;
			m_attackNoMove = true;
		}

		if (string.IsNullOrEmpty(sAttackAnim))
		{
			return false;
		}

		// launch attack
		m_curAttackId = id;		
		m_curAttackAnimHash = Animator.StringToHash(sAttackAnim);
		m_attackCrouched = bCrouched;
		m_rangeAttackInitTime = Time.time;

		PlayAnim(m_curAttackAnimHash, 0f);
		
		if(m_advanced.m_enableSendMessages)
			SendMessage("APOnCharacterRangedAttack", SendMessageOptions.DontRequireReceiver);

		return true;
	}

	// called when ranged attack must launch a bullet
	void FireRangedAttack()
	{
		if(GetState() == State.RangedAttack)
		{
			m_rangedAttacks.m_attacks[m_curAttackId].FireBullet(this, m_rangedContextId);
		}
	}

	// called when ranged attack must end and go back to previous state
	void LeaveRangedAttack()
	{
		if(GetState() == State.RangedAttack)
		{
			// stay in same state if player is still firing
			APRangedAttack curAttack = m_rangedAttacks.m_attacks[m_curAttackId];
			if (curAttack.inputStatus)
			{
				InitRangedAttack(curAttack, m_curAttackId);
			}
			else
			{
				// otherwise simply leave state
				SetState(GetPreviousState());
			}
		}
	}

	void HandleRangedAttack()
	{
		// check if attack allowed
		if (m_rangedAttacks.m_enabled && (IsCrouched() || (GetState() == State.Standard)) && !IsNewStateRequested())
		{
			// parse list of attack, check if one is launched
			for (int i = 0; i < m_rangedAttacks.m_attacks.Length; i++)
			{
				APRangedAttack curAttack = m_rangedAttacks.m_attacks[i];
				if (curAttack.inputStatus)
				{
					if(InitRangedAttack(curAttack, i))
					{
						SetState(State.RangedAttack);
						return;					
					}
				}
			}			
		}
	}

	public bool IsAttacking()
	{
		return (GetState() == State.MeleeAttack) || (GetRequestedState() == State.MeleeAttack) || (GetState() == State.RangedAttack) || (GetRequestedState() == State.RangedAttack);
	}

	public bool IsAttackingStopped()
	{
		return IsAttacking() && m_attackNoMove;
	}

	public bool IsAttackingCrouched()
	{
		return IsAttacking() && m_attackCrouched;
	}

	void SetState(State newState)
	{
		m_fsm.SetRequestedState((uint)newState);
	}

	State GetState()
	{
		return (State)m_fsm.GetState();
	}

	State GetRequestedState()
	{
		return (State)m_fsm.GetRequestedState();
	}

	State GetPreviousState()
	{
		return (State)m_fsm.GetPreviousState();
	}

	void ApplyGravity()
	{
		float fFactor = (GetState() == State.Glide && m_motor.m_velocity.y <= 0f) ? m_glide.m_gravityFactor : 1f;
		if (!m_onGround)
		{
			m_motor.m_velocity -= Vector2.up * m_basic.m_gravity * Time.deltaTime * fFactor;
		}
	}

	protected virtual void HandleInputFilter()
	{
		m_inputs.m_axisX.Update(Time.deltaTime);
		m_inputs.m_axisY.Update(Time.deltaTime);

		// handle auto move here
		if(m_basic.m_autoMove != eAutoMove.Disabled)
		{
			bool bHalt = !string.IsNullOrEmpty(m_basic.m_haltAutoMove) && Input.GetButton(m_basic.m_haltAutoMove);
			m_inputs.m_axisX.SetForcedValue(true, bHalt ? 0f : (m_basic.m_autoMove == eAutoMove.Right ? 1f : -1f));
		}
		else
		{
			m_inputs.m_axisX.SetForcedValue(false, 0f);
		}
	}

	void UpdateGroundStatus()
	{
		// first check if we are on ground
		bool bCurrentGroundStatus = false;
		if (m_motor.TouchGround)
		{
			if (!m_onGround)
			{
				// snap on ground if we are close enough and if velocity is toward ground normal
				float fVelDotGround = Vector2.Dot(m_motor.m_velocity, m_motor.GetGroundNormal());
				if ((fVelDotGround < m_advanced.m_maxVerticalVelForSnap) && (m_motor.GetDistanceToGround() < 0.01f))
				{
					bCurrentGroundStatus = true;
				}
			}
			else
			{
				// previously on ground, keep snap on ground if normal or distance is ok and no jump at previous frame
				// NB : ground snapping with angle is enabled only with same ground game object
				Vector2 groundNormal = m_motor.GetGroundNormalLs();
				if ((m_lastJumpTime == 0f) && ((m_motor.GetDistanceToGround() < 0.01f) || (groundNormal.y > Mathf.Cos(m_advanced.m_groundSnapAngle) && m_lastGround == m_motor.GetGroundGameObject())))
				{
					bCurrentGroundStatus = true;
				}
			}
		}

		// snap on ground
		if (bCurrentGroundStatus)
		{
			m_lastGround = m_motor.GetGroundGameObject();
			transform.position -= transform.up * m_motor.GetDistanceToGround(); 

			// handle ground alignment
			if(m_groundAlign.m_groundAlign)
			{
				Vector2 groundNormal = m_motor.GetGroundNormal();
				float fAngle = Vector2.Angle(transform.up, groundNormal);
				if(Vector3.Cross(transform.up, groundNormal).z < 0f)
				{
					fAngle = -fAngle;
				}
				rigidbody2D.angularVelocity = fAngle / Time.deltaTime;
			}

			// clear vertical velocity if we just touched ground
			if (bCurrentGroundStatus != m_onGround)
			{
				m_motor.m_velocity.y = 0f;
				m_animAirTime = 0f;
				m_airJumpCount = 0;
				m_lastJumpTime = 0f;
				m_airTime = 0f;
				m_glideCount = 0;
				m_jumpAnimLaunched = false;
			}
		}
		else
		{
			m_lastGround = null;
			m_animAirTime += Time.deltaTime;
			m_airTime += Time.deltaTime;

			if(m_groundAlign.m_forceInAirVerticalAlign)
			{
				float fAngle = Vector2.Angle(transform.up, Vector2.up);
				if(Vector3.Cross(transform.up, Vector2.up).z < 0f)
				{
					fAngle = -fAngle;
				}
				rigidbody2D.angularVelocity = fAngle / Time.deltaTime;
			}
		}

		// update current status
		if (m_onGround != bCurrentGroundStatus)
		{
			m_onGround = bCurrentGroundStatus;
			if(m_advanced.m_enableSendMessages)
				SendMessage(bCurrentGroundStatus ? "APOnCharacterLand" : "APOnCharacterInAir", SendMessageOptions.DontRequireReceiver);
		}
	}

	void HandleAutoRotate()
	{
		if (m_basic.m_autoRotate)
		{
			// handle in air auto rotate specific case
			float fFlipMaxSpeed = m_onGround ? m_basic.m_groundFlipMaxSpeed : m_basic.m_airFlipMaxSpeed;
			if (fFlipMaxSpeed >= 0f)
			{
				float threshold = fFlipMaxSpeed + 0.001f;
				float fVelHorizontal = Vector2.Dot(m_motor.m_velocity, transform.right);
				if((m_motor.FaceRight && fVelHorizontal > threshold) || (!m_motor.FaceRight && fVelHorizontal < -threshold))
					return;
			}
			
			// standard behavior
			if (m_inputs.m_axisX.GetRawValue() > 0f && !m_motor.FaceRight)
			{
				m_motor.Flip();
			} 
			else if (m_inputs.m_axisX.GetRawValue() < 0f && m_motor.FaceRight)
			{
				m_motor.Flip();
			}
		}
	}

	bool GetInputRunning()
	{
		return m_basic.m_alwaysRun || (!m_inputs.m_runButton.IsSpecified() ? false : m_inputs.m_runButton.GetButton());
	}

	public float ComputeMaxSpeed()
	{
		if (!m_onGround)
		{
			return m_basic.m_maxAirSpeed;
		} 
		else
		{
			return GetInputRunning() ? m_basic.m_runSpeed : m_basic.m_walkSpeed;
		}
	}

	void HandleHorizontalMove()
	{		
		m_sliding = false;

		float maxSpeed = ComputeMaxSpeed();
		float absAxisX = Mathf.Abs(m_inputs.m_axisX.GetRawValue());

		// compute horizontal velocity from input
		float fMoveDir = m_inputs.m_axisX.GetRawValue() != 0f ? Mathf.Sign(m_inputs.m_axisX.GetRawValue()) : (m_motor.FaceRight ? 1f : -1f);

		// compute slope factor
		m_speedFactor = 1f;
		bool downSlide = false;
		if (m_onGround)
		{
			float fGroundAngle = Mathf.Rad2Deg * Mathf.Acos(m_motor.GetGroundNormal().y);

			// handle auto down slide here
			if (m_downSlopeSliding.m_enabled && (GetState() == State.Standard) && (absAxisX == 0f) && (fGroundAngle >= m_downSlopeSliding.m_slopeMinAngle))
			{
				// - force player to slide down
				fMoveDir = Mathf.Sign(m_motor.GetGroundNormal().x);
				absAxisX = m_downSlopeSliding.m_slidingPower;
				m_sliding = true; // this like a slide according to animation !
				downSlide = true;

				// stop any invalid velocity
				m_motor.m_velocity.x = fMoveDir > 0f ? Mathf.Max(0, m_motor.m_velocity.x) : Mathf.Min(0, m_motor.m_velocity.x);
			}
			else
			{				
				fGroundAngle = fMoveDir != Mathf.Sign(m_motor.GetGroundNormal().x) ? fGroundAngle : -fGroundAngle;
				m_speedFactor = Mathf.Clamp01(m_basic.m_slopeSpeedMultiplier.Evaluate(fGroundAngle));
			}
		}
		
		// Compute move direction
		Vector2 hrzMoveDir = new Vector2(fMoveDir, 0f);		
		float hrzMoveLength = absAxisX * maxSpeed * m_speedFactor;

		// align this velocity on ground plane if we touch the ground 
		bool bCrouched = IsCrouched();
		bool bAttacking = IsAttackingStopped();
		if (m_onGround)
		{
			if(m_groundAlign.m_groundAlign)
			{
				hrzMoveDir = transform.TransformDirection(hrzMoveDir);
			}
			else
			{
				float fDot = Vector2.Dot(hrzMoveDir, m_motor.GetGroundNormal());
				if (Mathf.Abs(fDot) > float.Epsilon)
				{
					Vector3 perpAxis = Vector3.Cross(hrzMoveDir, m_motor.GetGroundNormal());
					hrzMoveDir = Vector3.Cross(m_motor.GetGroundNormal(), perpAxis);
					hrzMoveDir.Normalize();
				}
			}

			// cancel input if needed
			if (!downSlide && (bCrouched || bAttacking))
			{
				hrzMoveLength = 0f;
			}
		}
		else if(m_groundAlign.m_alignAirMove)
		{
			hrzMoveDir = transform.TransformDirection(hrzMoveDir);
		}

		// handle dynamic
		if (m_onGround)
		{
			float fDynFriction, fStaticFriction;
			ComputeFrictions(out fDynFriction, out fStaticFriction);

			// update sliding status
			if (fDynFriction < 1f || fStaticFriction < 1f)
			{
				m_sliding = true;
			} 

			float fVelOnMove = Vector2.Dot(m_motor.m_velocity, hrzMoveDir);
			float fDirLength = fVelOnMove;
			if(m_sliding && !downSlide)
			{
				// dynamic is different while sliding
				if(!bCrouched && !bAttacking && absAxisX > 0f)
				{
					float fDiffMax = maxSpeed - fVelOnMove;
					if(fDiffMax > 0f)
					{
						fDirLength = fVelOnMove + Mathf.Min(fDiffMax, absAxisX * fDynFriction * Time.deltaTime * 20f);
					}
				}
				else
				{
					fDirLength = ApplyDamping(fVelOnMove, fStaticFriction);
				}
			}
			else
			{
				// raise deceleration for crouched/attack
				float fDecel = (!downSlide && (bCrouched || bAttacking)) ? m_basic.m_deceleration * 2f : m_basic.m_deceleration;
				fDirLength = APInputJoystick.Update(fVelOnMove, hrzMoveLength, m_basic.m_stopOnRotate, m_basic.m_acceleration, fDecel, Time.deltaTime);
			}

			ClampValueWithDamping(ref fDirLength, m_advanced.m_maxVelDamping, -maxSpeed, maxSpeed);
			m_motor.m_velocity = hrzMoveDir * (fDirLength);
			m_groundSpeed = Mathf.Abs(fDirLength);
		}
		else
		{
			// in air dynamic
			float fVelOnMove = Vector2.Dot(m_motor.m_velocity, hrzMoveDir);
			float fDiffVel = (hrzMoveLength - fVelOnMove);
			float fMaxAccel = m_basic.m_airPower * Time.deltaTime * (GetState() == State.Glide ? m_glide.m_lateralMoveFactor : 1f);
			fDiffVel = Mathf.Clamp(fDiffVel, -fMaxAccel, fMaxAccel);

			m_motor.m_velocity += hrzMoveDir * fDiffVel;

			// TODO : fix clamping when ground align is enabled
			ClampValueWithDamping(ref m_motor.m_velocity.x, m_advanced.m_maxVelDamping, -m_basic.m_maxAirSpeed, m_basic.m_maxAirSpeed);
			ClampValueWithDamping(ref m_motor.m_velocity.y, m_advanced.m_maxVelDamping, -m_basic.m_maxFallSpeed, m_basic.m_maxFallSpeed);
			m_groundSpeed = 0f;
		}
	}

	void ComputeFrictions(out float fDynFriction, out float fStaticFriction)
	{
		fDynFriction = 0f;
		fStaticFriction = 0f;

		// compute material override friction, keep highest value if different grounds are touched
		bool bOverride = false;
		for (int i = 0; i < m_motor.m_RaysGround.Length; i++)
		{
			if (m_motor.m_RaysGround [i].m_collider)
			{
				APMaterial groundMat = m_motor.m_RaysGround [i].m_collider.GetComponent<APMaterial>();
				if (groundMat != null && groundMat.m_overrideFriction)
				{
					bOverride = true;
					fDynFriction = Mathf.Max(fDynFriction, groundMat.m_dynFriction);
					fStaticFriction = Mathf.Max(fStaticFriction, groundMat.m_staticFriction);
				}
			}
		}
		
		// keep default value if no override
		if (!bOverride)
		{		
			fDynFriction = m_basic.m_frictionDynamic;
			fStaticFriction = m_basic.m_frictionStatic;
		}
	}

	void ClampValueWithDamping(ref float fValue, float fDamping, float fMin, float fMax)
	{
		if (fValue > fMax)
		{
			fValue = Mathf.Max(fMax, ApplyDamping(fValue, fDamping));
		} 
		else if (fValue < fMin)
		{
			fValue = Mathf.Min(fMin, ApplyDamping(fValue, fDamping));
		}
	}

	float ApplyDamping(float fValue, float fDampingValue)
	{
		float fDamping = Mathf.Exp(-fDampingValue * Time.deltaTime);
		return fValue * fDamping;
	}

	void HandleCarry()
	{
		m_carrier = null;
		if (m_onGround)
		{
			for (int i = 0; i < m_motor.m_RaysGround.Length; i++)
			{
				if (m_motor.m_RaysGround [i].m_collider)
				{
					APCarrier curCarrier = m_motor.m_RaysGround [i].m_collider.GetComponent<APCarrier>();
					if (curCarrier != null)
					{
						// use prefer index if carrier is already assigned
						if ((m_carrier == null) || (i == m_advanced.m_carryRayIndex))
						{
							m_carrier = curCarrier;
						}
					}
				}
			}
			
			// for now move player by position
			// TODO : we should move by velocity properly and detect collision by the way, must be in future release !
			if (m_carrier != null)
			{
				m_carrierOffset = m_carrier.GetVelocity() * Time.deltaTime;
				transform.position += new Vector3(m_carrierOffset.x, m_carrierOffset.y, 0f);
			}
		}
	}

	void HandleGlide()
	{
		if(m_glide.m_enabled && !IsNewStateRequested() && !m_onGround)
		{
			// additional glides must release input
			bool bGlideInput = m_glideCount == 0 ? m_glide.m_button.GetButton() : m_glideDown;
			if(bGlideInput && (m_glideCount < m_glide.m_maxCount) && (m_airTime >= m_glide.m_minAirTimeBeforeGlide))
			{
				SetState(State.Glide);
			}
		}
	}

	void HandleJump()
	{
		// check if we should jump
		if (m_jump.m_enabled && m_jumpDown && (Time.time - m_lastJumpTime > m_advanced.m_minTimeBetweenTwoJumps))
		{ 
			if(m_onGround || (m_jump.m_airJumpCount > m_airJumpCount))
			{
				// make sure we can jump if crouched
				if (!IsCrouched() || CanUncrouch())
				{ 
					if(!m_onGround)
					{
						m_airJumpCount++;
					}

					Uncrouch();
					Jump(1f);

					if(m_advanced.m_enableSendMessages)
						SendMessage("APOnCharacterJump", SendMessageOptions.DontRequireReceiver);
					return;
				}
			}
		}

		// handle extra jumping
		if (!m_onGround && (m_lastJumpTime > 0f) && m_jump.m_button.GetButton() && !m_motor.TouchHead)
		{
			if (Time.time < m_lastJumpTime + (m_jump.m_maxHeight - m_jump.m_minHeight) / ComputeJumpVerticalSpeed(m_jump.m_minHeight))
			{
				// remove gravity
				m_motor.m_velocity += Vector2.up * m_basic.m_gravity * Time.deltaTime;
			}
		}
	}

	// add an impulse to character
	public void AddImpulse(Vector2 impulse)
	{
		if(m_bForceDefer)
		{
			m_deferImpulse = true;
			m_deferImpulsePower += impulse;
		}
		else
		{
			LeaveAnyState();
			m_onGround = false;
			m_motor.m_velocity += impulse;
		}
	}

	// set character velocity
	public void SetVelocity(Vector2 velocity)
	{
		if(m_bForceDefer)
		{
			m_deferVelocity = true;
			m_deferedVelocity = velocity;
		}
		else
		{
			LeaveAnyState();
			m_onGround = false;
			m_motor.m_velocity = velocity;
		}
	}

	// force character to jump immediately at minimum height * specified ratio
	public void Jump(float fRatio)
	{
		// handle case where jump is requested inside a callback, buffer action in this case
		if(m_bForceDefer)
		{
			m_deferJump = true;
			m_deferJumpRatio = fRatio;
		}
		else
		{
			m_lastJumpTime = Time.time;
			m_animAirTime = m_animations.m_minAirTime;
			m_requestJumpAnimation = true;
			m_jumpAnimLaunched = false;

			float fJumpSpeed = ComputeJumpVerticalSpeed(m_jump.m_minHeight * fRatio);
			if(m_groundAlign.m_jumpAlign)
			{
				float fDot = Vector2.Dot(m_motor.GetGroundNormal(), m_motor.m_velocity);
				m_motor.m_velocity += m_motor.GetGroundNormal() * (fJumpSpeed - fDot);
			}
			else
			{
				m_motor.m_velocity.y = fJumpSpeed;
			}
			
			// inject carrier horizontal velocity
			if (m_carrier)
			{
				m_motor.m_velocity.x += m_carrier.GetVelocity().x;
			}

			// make sure to go back to Standard state
			LeaveAnyState();
			m_onGround = false;
		}
	}

	public void LeaveAnyState()
	{
		// make sure to go back to Standard state
		State curState = GetState();
		if((curState != State.Standard) && (GetRequestedState() != State.Standard))
		{
			if(curState == State.Crouch)
			{
				// crouch special case
				if (CanUncrouch())
				{
					Uncrouch();
				}
			}
			else
			{
				SetState(State.Standard);
			}
		}
	}

	void HandleWallJump()
	{
		// early exit
		if (!m_jump.m_button.IsSpecified() || !m_jumpDown || IsCrouched() || m_onGround || IsNewStateRequested())
			return;
		
		// check if touching wall
		bool bHit = false;
		uint iHitCount = 0;
		APMaterial.BoolValue bMaterialSnap = APMaterial.BoolValue.Default;

		for (int i = 0; i < m_motor.m_RaysFront.Length; i++)
		{
			// check if this ray should be tested
			bool bTestRay = (m_wallJump.m_rayIndexes.Length == 0);
			for (int j = 0; j < m_wallJump.m_rayIndexes.Length; j++)
			{
				if (m_wallJump.m_rayIndexes [j] == i)
				{
					bTestRay = true;
					break;
				}
			}

			if (bTestRay && m_motor.m_RaysFront [i].m_collider)
			{
				iHitCount++;

				// use hit material value, true has priority
				APMaterial hitMat = m_motor.m_RaysFront [i].m_collider.GetComponent<APMaterial>();
				if (hitMat != null)
				{
					if (hitMat.m_wallJump == APMaterial.BoolValue.True)
					{
						bMaterialSnap = APMaterial.BoolValue.True;
					} 
					else if (hitMat.m_wallJump == APMaterial.BoolValue.False && bMaterialSnap != APMaterial.BoolValue.True)
					{
						bMaterialSnap = APMaterial.BoolValue.False;
					}
				}
			}
		}

		if (m_wallJump.m_rayIndexes.Length == 0)
			bHit = (iHitCount == m_motor.m_RaysFront.Length);
		else
			bHit = (iHitCount == m_wallJump.m_rayIndexes.Length);


		// effective jump
		bool bCanJump = bHit && (bMaterialSnap != APMaterial.BoolValue.False) && (bMaterialSnap == APMaterial.BoolValue.True || m_wallJump.m_enabled);
		if (bCanJump && (Time.time - m_lastJumpTime > m_advanced.m_minTimeBetweenTwoJumps))
		{		
			// put in wall jump state
			SetState(State.WallJump);
		}
	}

	void HandleWallSlide()
	{
		// early exit
		if (IsNewStateRequested())
		{
			return;
		}

		// check conditions
		float wallFriction;
		if(ShouldWallSlide(out wallFriction))
		{
			SetState(State.WallSlide);
		}
	}

	bool ShouldWallSlide(out float wallFriction)
	{
		// early exit
		if(IsCrouched() || m_onGround || m_motor.m_velocity.y > -m_wallSlide.m_minSpeed)
		{
			wallFriction = 0f;
			return false;
		}

		// check if touching front wall, save its friction value if any
		bool bWallSlide = false;
		uint iHitCount = 0;
		bool bUseWallFriction = false;
		APMaterial.BoolValue bMaterialWallSlide = APMaterial.BoolValue.Default;
		float fWallFriction = 0f;
		
		for (int i = 0; i < m_motor.m_RaysFront.Length; i++)
		{
			// check if this ray should be tested
			bool bTestRay = (m_wallSlide.m_rayIndexes.Length == 0);
			foreach (int curRayIndex in m_wallSlide.m_rayIndexes)
			{
				if (curRayIndex == i)
				{
					bTestRay = true;
					break;
				}
			}
			
			if (bTestRay && m_motor.m_RaysFront[i].m_collider)
			{
				iHitCount++;
				
				// use collider material value if any, keep highest friction
				APMaterial hitMat = m_motor.m_RaysFront[i].m_collider.GetComponent<APMaterial>();
				if (hitMat != null)
				{
					if (hitMat.m_wallFriction.m_override)
					{
						bUseWallFriction = true;
						fWallFriction = Mathf.Max(fWallFriction, hitMat.m_wallFriction.m_value);
					}
					
					if (hitMat.m_wallSlide == APMaterial.BoolValue.True)
					{
						bMaterialWallSlide = APMaterial.BoolValue.True;
					} 
					else if (hitMat.m_wallSlide == APMaterial.BoolValue.False && bMaterialWallSlide != APMaterial.BoolValue.True)
					{
						bMaterialWallSlide = APMaterial.BoolValue.False;
					}
				}
			}
		}
		
		if (m_wallSlide.m_rayIndexes.Length == 0)
			bWallSlide = (iHitCount == m_motor.m_RaysFront.Length);
		else
			bWallSlide = (iHitCount == m_wallSlide.m_rayIndexes.Length);
		
		// make sure material allows it if any
		bWallSlide &= (bMaterialWallSlide != APMaterial.BoolValue.False) && (bMaterialWallSlide == APMaterial.BoolValue.True || m_wallSlide.m_enabled);
		
		// check if pushing toward wall
		bool bPushTowardWall = (m_inputs.m_axisX.GetRawValue() > 0.5f && m_motor.m_faceRight) || (m_inputs.m_axisX.GetRawValue() < -0.5f && !m_motor.m_faceRight);
		
		// update friction and result
		wallFriction = bUseWallFriction ? fWallFriction : m_wallSlide.m_friction;
		return bWallSlide && bPushTowardWall;
	}

	bool GetInputCrouch()
	{
		return m_inputs.m_axisY.GetRawValue() < -0.5f;
	}

	void HandleCrouch()
	{
		// Do not change state if currently switching or no crouch animation
		if(IsNewStateRequested() || string.IsNullOrEmpty(m_animations.m_crouch))
		   return;

		bool bCrouched = IsCrouched();
		bool bInputCrouch = GetInputCrouch();

		// crouch if needed
		if (!bCrouched && m_onGround && bInputCrouch)
		{
			Crouch();
		}
		else if(bCrouched && !bInputCrouch)
		{
			// uncrouch as soon as we can
			if (CanUncrouch())
			{
				Uncrouch();
			}
			else if(Mathf.Abs(m_motor.m_velocity.x) < m_basic.m_uncrouchMinSpeed)
			{
				// keep moving if we can't uncrouch
				m_motor.m_velocity.x = (m_motor.m_faceRight ? 1f : -1f) * m_basic.m_uncrouchMinSpeed;
			}
		}
	}

	bool CanUncrouch()
	{
		// make sure expanded box does not collide
		Vector2 orgSize = new Vector2(m_motor.GetBoxCollider().size.x, m_crouchBoxOriginalSize);
		Vector2 orgCenter = new Vector2(m_motor.GetBoxCollider().center.x, m_crouchBoxOriginalCenter);
		
		float fScale = 0.9f;
		float fBoxTop = (orgCenter + 0.5f * orgSize).y;
		Vector2 boxA = m_motor.GetBoxCollider().center + Vector2.Scale(m_motor.GetBoxCollider().size * fScale, new Vector2(-0.5f, 0.5f));
		Vector2 boxB = new Vector2(boxA.x + m_motor.GetBoxCollider().size.x * fScale, fBoxTop);
		boxA = transform.TransformPoint(boxA);
		boxB = transform.TransformPoint(boxB);
		
		if (m_advanced.m_debugDraw)
		{
			Debug.DrawLine(boxA, new Vector2(boxA.x, boxB.y), Color.red);
			Debug.DrawLine(new Vector2(boxA.x, boxB.y), boxB, Color.red);
			Debug.DrawLine(boxB, new Vector2(boxB.x, boxA.y), Color.red);
			Debug.DrawLine(new Vector2(boxB.x, boxA.y), boxA, Color.red);
		}

		// ignore player collider itself
		int hitCount = Physics2D.OverlapAreaNonAlloc(boxA, boxB, m_overlapResult, m_motor.m_rayLayer);
		for (int i = 0; i < hitCount; i++)
		{
			if(!m_overlapResult[i].isTrigger && m_overlapResult[i] != collider2D)
				return false;
		}
		return true;
	}

	void Crouch()
	{
		if (!IsCrouched())
		{
			// reduce collision box (and stay on same base plane)
			Vector2 curBoxSize = m_motor.GetBoxCollider().size;
			Vector2 curBoxCenter = m_motor.GetBoxCollider().center;
			m_crouchBoxOriginalSize = curBoxSize.y;
			m_crouchBoxOriginalCenter = curBoxCenter.y;
			
			Vector2 newSize = new Vector2(curBoxSize.x, curBoxSize.y * m_basic.m_crouchSizePercent);
			m_motor.GetBoxCollider().size = newSize;
			
			float fVertOffset = (m_basic.m_crouchSizePercent - 1) * curBoxSize.y * 0.5f;
			Vector2 newCenter = new Vector2(curBoxCenter.x, curBoxCenter.y + fVertOffset);
			m_motor.GetBoxCollider().center = newCenter;
			
			// do the same for the rays
			m_motor.Scale = new Vector2(1f, m_basic.m_crouchSizePercent);
			m_motor.Offset = new Vector2(0f, fVertOffset);

			// play anim
			SetState(State.Crouch);
		}
	}

	void Uncrouch()
	{
		if (IsCrouched())
		{
			// restore collision box
			Vector2 orgSize = new Vector2(m_motor.GetBoxCollider().size.x, m_crouchBoxOriginalSize);
			Vector2 orgCenter = new Vector2(m_motor.GetBoxCollider().center.x, m_crouchBoxOriginalCenter);
			m_motor.GetBoxCollider().size = orgSize;
			m_motor.GetBoxCollider().center = orgCenter;
			
			// and motor
			m_motor.Scale = Vector2.one;
			m_motor.Offset = Vector2.zero;

			if(m_advanced.m_enableSendMessages)
				SendMessage("APOnCharacterUncrouch", SendMessageOptions.DontRequireReceiver);

			SetState(State.Standard);
		}
	}

	float ComputeJumpVerticalSpeed(float targetJumpHeight)
	{
		// estimation of speed for required height
		return Mathf.Sqrt(2 * targetJumpHeight * m_basic.m_gravity);
	}

	public bool IsControlled
	{
		get
		{
			return m_isControlled;
		}
		set
		{

			// reset runtime values when no more controlled
			if (m_isControlled != value && !value)
			{
				ClearRuntimeValues();
			}

			m_isControlled = value;
		}
	}
	
	void HandleAnimation()
	{
		if (m_isControlled)
			return;

		// reset animation speed
		m_anim.speed = 1f;
		m_isInStand = false;

		// setup some scenaric values
		m_anim.SetFloat("VerticalSpeed", Vector2.Dot(m_motor.m_velocity, transform.up));

		// handle standard state
		if (GetState() == State.Standard)
		{
			if (m_onGround || m_animAirTime < m_animations.m_minAirTime)
			{
				// mode from input
				float fFilteredInput = Mathf.Abs(m_inputs.m_axisX.GetValue());
				if (m_animations.m_walkAnimFromInput)
				{
					float fSpeed = m_animations.m_animFromInput.Evaluate(fFilteredInput);
					if (fSpeed < 1e-3f)
					{
						PlayAnim(m_animations.m_stand);
						m_isInStand = true;
					}
					else
					{					
						PlayAnim(GetInputRunning() ? m_animations.m_run : m_animations.m_walk);
						m_anim.speed = Mathf.Abs(fSpeed); 
					}
				}
				else
				{
					// compute speed on ground/in air
					float fGroundSpeed = m_onGround ? m_motor.ComputeVelocityOnGround().magnitude : Mathf.Abs(m_motor.m_velocity.x);

					float fSpeedFromInput = m_animations.m_animFromInput.Evaluate(fFilteredInput);
					float fSpeedFromGround = m_animations.m_animFromSpeed.Evaluate(fGroundSpeed);
					if ((fSpeedFromInput < 1e-3f && m_sliding) || (!m_sliding && fSpeedFromGround < 1e-3f && m_inputs.m_axisX.GetRawValue() == 0f))
					{
						PlayAnim(m_animations.m_stand);
						m_isInStand = true;
					}
					else
					{
						float fSpeed = 0f;
						if (m_sliding)
						{
							fSpeed = fSpeedFromInput;
						}
						else
						{
							fSpeed = fSpeedFromGround;
						}

						PlayAnim(GetInputRunning() ? m_animations.m_run : m_animations.m_walk);
						m_anim.speed = Mathf.Abs(fSpeed); 
					}
				}
			}
			else if(m_requestJumpAnimation)
			{
				m_jumpAnimLaunched = true;
				PlayAnim(m_animations.m_inAir, 0f);
			}
		}
		
		// clear this query each frame
		m_requestJumpAnimation = false;
	}

	public void PlayAnim(string sAnim)
	{
		if (!string.IsNullOrEmpty(sAnim))
		{
			m_anim.Play(sAnim);
		}
	}

	public void PlayAnim(string sAnim, float fNormalizedTime)
	{
		if (!string.IsNullOrEmpty(sAnim))
		{
			m_anim.Play(sAnim, 0, fNormalizedTime);
		}
	}
	
	public void PlayAnim(int iAnimHash, float fNormalizedTime)
	{
		if (iAnimHash != 0)
		{
			m_anim.Play(iAnimHash, 0, fNormalizedTime);
		}
	}

	bool IsNewStateRequested()
	{
		return GetState() != GetRequestedState();
	}
}