using UnityEngine;

namespace Shapes {

	public class Compass : MonoBehaviour {

		public float uiRadius = 1f;
		public Vector2 position;
		public float width = 1f;

		[Range( 0, 0.2f )] public float lineSocketThickness = 0.1f;
		[Range( 0, 0.2f )] public float socketHeight = 0.1f;

		[Range( 0f, ShapesMath.TAU )]
		public float angularRange = ShapesMath.TAU / 4;

		public int tickCount = 12;

		[Range( 0f, 1f )]
		public float edgeFadeFraction = 0.1f;

		public float tickLabelOffset = 0.01f;
		public float fontSize = 1f;
		public float fontSizeLookLabel = 1f;
		public Vector2 lookAngLabelOffset;

		[Range( 0, 0.05f )] public float triangleNootSize = 0.1f;

		public void DrawCompass( Vector3 worldDir ) {
			Vector2 compArcOrigin = position + Vector2.down * uiRadius;

			float angUiMin = ShapesMath.TAU * 0.25f - ( width / 2 ) / uiRadius;
			float angUiMax = ShapesMath.TAU * 0.25f + ( width / 2 ) / uiRadius;
			Vector2 dirWorld = new Vector2( worldDir.x, worldDir.z ).normalized;
			float lookAng = ShapesMath.DirToAng( dirWorld );
			float angWorldMin = lookAng + angularRange / 2;
			float angWorldMax = lookAng - angularRange / 2;

			Draw.Arc( compArcOrigin, uiRadius, lineSocketThickness, angUiMin, angUiMax, ArcEndCap.Round );

			void CompassArcNoot( float worldAng, float size, string label = null ) {
				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, worldAng );
				float uiAng = Mathf.Lerp( angUiMin, angUiMax, tCompass );
				Vector2 uiDir = ShapesMath.AngToDir( uiAng );
				Vector2 a = compArcOrigin + uiDir * uiRadius;
				Vector2 b = compArcOrigin + uiDir * ( uiRadius - size * socketHeight );
				float fade = Mathf.InverseLerp( 0, edgeFadeFraction, ( 1f - Mathf.Abs( tCompass * 2 - 1 ) ) );
				Draw.Line( a, b, LineEndCap.None, new Color( 1, 1, 1, fade ) );
				if( label != null ) {
					Draw.FontSize = fontSize;
					Draw.Text( b - uiDir * tickLabelOffset, uiAng - ShapesMath.TAU / 4f, label, TextAlign.Center, new Color( 1, 1, 1, fade ) );
				}
			}

			Draw.LineEndCaps = LineEndCap.Square;
			Draw.LineThickness = lineSocketThickness;


			Vector2 trianglePos = compArcOrigin + Vector2.up * ( uiRadius + 0.01f );
			Vector2 labelPos = compArcOrigin + Vector2.up * ( uiRadius ) + lookAngLabelOffset * 0.1f;
			string lookLabel = Mathf.RoundToInt( -lookAng * Mathf.Rad2Deg + 180f ) + "°";
			Draw.FontSize = fontSizeLookLabel;
			Draw.Text( labelPos, 0f, lookLabel, TextAlign.Center );
			Vector2 triA = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 ) * triangleNootSize;
			Vector2 triB = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 + ShapesMath.TAU / 3 ) * triangleNootSize;
			Vector2 triC = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 + 2 * ShapesMath.TAU / 3 ) * triangleNootSize;
			Draw.Triangle( triA, triB, triC );

			for( int i = 0; i < tickCount; i++ ) {
				float t = i / ( (float)tickCount );
				float ang = ShapesMath.TAU * t;
				bool cardinal = i % ( tickCount / 4 ) == 0;

				string label = null;
				if( cardinal ) {
					int angInt = Mathf.RoundToInt( ( 1f - t ) * 4 );
					switch( angInt ) {
						case 0:
						case 4:
							label = "S";
							break;
						case 1:
							label = "W";
							break;
						case 2:
							label = "N";
							break;
						case 3:
							label = "E";
							break;
					}
				}

				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, ang );
				if( tCompass < 1f && tCompass > 0f )
					CompassArcNoot( ang, cardinal ? 0.8f : 0.5f, label );
			}
		}

	}

}