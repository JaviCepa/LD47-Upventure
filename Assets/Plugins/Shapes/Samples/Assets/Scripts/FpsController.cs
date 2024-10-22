﻿using System;
using System.Collections;
using UnityEngine;

namespace Shapes {

	[Serializable]
	public class Decayer {
		public float decaySpeed;
		public float magnitude;

		[NonSerialized]
		public float value;

		[NonSerialized]
		public float valueInv;

		public AnimationCurve curve;
		public float t;

		public void SetT( float v ) => t = v;

		public void Update() {
			t = Mathf.Max( 0, ( t - decaySpeed * Time.deltaTime ) );
			float tEval = curve.keys.Length > 0 ? curve.Evaluate( 1f - t ) : t;
			value = tEval * magnitude;
			valueInv = ( 1f - tEval ) * magnitude;
		}
	}

	[ExecuteAlways]
	public class FpsController : MonoBehaviour {

		// components
		public Transform head;
		public Camera cam;
		public Crosshair crosshair;
		public ChargeBar chargeBar;
		public AmmoBar ammoBar;
		public Compass compass;
		public Transform crosshairTransform;

		[Range( 0f, ShapesMath.TAU / 2 )] public float ammoBarAngularSpanRad;
		[Range( 0, 0.05f )] public float ammoBarOutlineThickness = 0.1f;
		[Range( 0, 0.2f )] public float ammoBarThickness;
		[Range( 0, 0.2f )] public float ammoBarRadius;

		public AnimationCurve shakeAnimX = AnimationCurve.Constant( 0, 1, 0 );
		public AnimationCurve shakeAnimY = AnimationCurve.Constant( 0, 1, 0 );

		[Range( 0.8f, 1f )]
		public float smoof = 0.99f;

		public float moveSpeed = 1f;
		public float lookSensitivity = 1f;

		[Range( 0f, 0.3f )]
		public float fireSidebarRadiusPunchAmount = 0.1f;


		float yaw;
		float pitch;
		Vector2 moveInput = Vector2.zero;
		Vector3 moveVel = Vector3.zero;

		public static void DrawRoundedArcOutline( Vector2 origin, float radius, float thickness, float outlineThickness, float angStart, float angEnd ) {
			// inner / outer
			float innerRadius = radius - thickness / 2;
			float outerRadius = radius + thickness / 2;
			float aaMargin = 0.01f;
			Draw.Arc( origin, innerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin );
			Draw.Arc( origin, outerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin );

			// rounded caps
			Vector2 originBottom = origin + ShapesMath.AngToDir( angStart ) * radius;
			Vector2 originTop = origin + ShapesMath.AngToDir( angEnd ) * radius;
			Draw.Arc( originBottom, thickness / 2, outlineThickness, angStart, angStart - ShapesMath.TAU / 2 );
			Draw.Arc( originTop, thickness / 2, outlineThickness, angEnd, angEnd + ShapesMath.TAU / 2 );
		}

		public Vector2 GetShake( float speed, float amp ) {
			float shakeVal = ShapesMath.Frac( Time.time * speed );
			float shakeX = shakeAnimX.Evaluate( shakeVal );
			float shakeY = shakeAnimY.Evaluate( shakeVal );
			return new Vector2( shakeX, shakeY ) * amp;
		}


		bool InputFocus {
			get => !Cursor.visible;
			set {
				Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !value;
			}
		}

		void Awake() {
			if( Application.isPlaying == false )
				return;
			InputFocus = true;
			StartCoroutine( FixedSteps() );
		}

		IEnumerator FixedSteps() {
			while( true ) {
				FixedUpdateManual();
				yield return new WaitForSeconds( 0.01f ); // 100 fps
			}
		}

		void OnEnable() => Camera.onPostRender += DrawShapes;
		void OnDisable() => Camera.onPostRender -= DrawShapes;


		void DrawShapes( Camera cam ) {
			if( cam != this.cam )
				return;

			Draw.Matrix = crosshairTransform.localToWorldMatrix;
			Draw.BlendMode = ShapesBlendMode.Transparent;
			Draw.LineGeometry = LineGeometry.Flat2D;

			crosshair.DrawCrosshair();

			float radiusPunched = ammoBarRadius + fireSidebarRadiusPunchAmount * crosshair.fireDecayer.value;
			ammoBar.DrawBar( this, radiusPunched );
			chargeBar.DrawBar( this, radiusPunched );
			compass.DrawCompass( head.transform.forward );
		}


		void FixedUpdateManual() {
			if( Application.isPlaying == false )
				return;
			if( InputFocus ) {
				Vector3 right = head.right;
				Vector3 forward = head.forward;
				forward.y = 0;
				moveVel += ( moveInput.y * forward + moveInput.x * right ) * ( Time.fixedDeltaTime * moveSpeed );
			}

			// move
			transform.position += moveVel * Time.deltaTime;


			// decelerate
			moveVel *= smoof;
		}


		void Update() {
			if( Application.isPlaying == false )
				return;

			crosshair.UpdateCrosshairDecay();
			chargeBar.UpdateCharge();

			if( InputFocus ) {
				// mouselook
				yaw += Input.GetAxis( "Mouse X" ) * lookSensitivity;
				pitch -= Input.GetAxis( "Mouse Y" ) * lookSensitivity;
				pitch = Mathf.Clamp( pitch, -90, 90 );
				head.localRotation = Quaternion.Euler( pitch, yaw, 0f );

				chargeBar.isCharging = Input.GetMouseButton( 1 ); // rmb

				if( Input.GetKey( KeyCode.R ) )
					ammoBar.Reload();

				// actions
				if( Input.GetMouseButtonDown( 0 ) && ammoBar.HasBulletsLeft ) {
					// Fire
					ammoBar.Fire();
					crosshair.Fire();
					Ray ray = new Ray( head.transform.position, head.transform.forward );
					if( Physics.Raycast( ray, Mathf.Infinity, 1 << 9 ) )
						crosshair.FireHit();
				}

				// move input
				moveInput = Vector2.zero;

				void DoInput( KeyCode key, Vector2 dir ) {
					if( Input.GetKey( key ) )
						moveInput += dir;
				}

				DoInput( KeyCode.W, Vector2.up );
				DoInput( KeyCode.S, Vector2.down );
				DoInput( KeyCode.D, Vector2.right );
				DoInput( KeyCode.A, Vector2.left );

				// leave focus mode stuff
				if( Input.GetKeyDown( KeyCode.Escape ) )
					InputFocus = false;
			} else if( Input.GetMouseButtonDown( 0 ) ) {
				InputFocus = true;
			}
		}
	}

}