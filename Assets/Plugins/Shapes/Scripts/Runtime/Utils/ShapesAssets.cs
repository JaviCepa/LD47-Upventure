using TMPro;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
[CreateAssetMenu]
public class ShapesAssets : ScriptableObject {
	
	[Header( "Config" )]
	public TMP_FontAsset defaultFont;

	[Header( "Meshes" )]
	public Mesh meshCube;

	public Mesh meshSphere;
	public Mesh meshTorus;
	public Mesh meshCapsule;
	public Mesh meshCylinder;
	public Mesh meshCone;
	public Mesh meshConeUncapped;

	[Header( "Editor UI" )]
	public Texture2D discIconSolid;

	public Texture2D discIconPie;
	public Texture2D discIconHollow;
	public Texture2D discIconArc;

	public Texture2D lineCapNone;
	public Texture2D lineCapSquare;
	public Texture2D lineCapRound;

	public Texture2D rectIconHardSolid;
	public Texture2D rectIconHardHollow;
	public Texture2D rectIconRoundedSolid;
	public Texture2D rectIconRoundedHollow;

	[Header( "Misc" )]
	public TextAsset packageJson;

	static ShapesAssets instance;
	public static ShapesAssets Instance {
		get {
			if( instance == null )
				instance = Resources.Load<ShapesAssets>( "Shapes Assets" );
			return instance;
		}
	}


	static GUIContent[] discTypeButtonContents = null;
	public static GUIContent[] DiscTypeButtonContents {
		get {
			if( discTypeButtonContents == null ) {
				discTypeButtonContents = new GUIContent[] {
					new GUIContent( Instance.discIconSolid, "Disc / Filled Circle" ),
					new GUIContent( Instance.discIconPie, "Pie / Circular Sector" ),
					new GUIContent( Instance.discIconHollow, "Ring / Annulus" ),
					new GUIContent( Instance.discIconArc, "Arc / Ring Sector / Annulus Sector" )
				};
			}

			return discTypeButtonContents;
		}
	}

	static GUIContent[] rectTypeButtonContents = null;
	public static GUIContent[] RectTypeButtonContents {
		get {
			if( rectTypeButtonContents == null ) {
				rectTypeButtonContents = new GUIContent[] {
					new GUIContent( Instance.rectIconHardSolid, "Solid Hard" ),
					new GUIContent( Instance.rectIconRoundedSolid, "Solid Rounded" ),
					new GUIContent( Instance.rectIconHardHollow, "Hollow Hard" ),
					new GUIContent( Instance.rectIconRoundedHollow, "Hollow Rounded" )
				};
			}

			return rectTypeButtonContents;
		}
	}

	static GUIContent[] lineCapButtonContents = null;
	public static GUIContent[] LineCapButtonContents {
		get {
			if( lineCapButtonContents == null ) {
				lineCapButtonContents = new GUIContent[] {
					new GUIContent( Instance.lineCapNone, "No caps" ),
					new GUIContent( Instance.lineCapSquare, "Square caps" ),
					new GUIContent( Instance.lineCapRound, "Round caps" )
				};
			}

			return lineCapButtonContents;
		}
	}

	static GUIContent[] angleUnitButtonContents = null;
	public static GUIContent[] AngleUnitButtonContents {
		get {
			if( angleUnitButtonContents == null ) {
				angleUnitButtonContents = new GUIContent[] {
					new GUIContent( "Radians" ),
					new GUIContent( "Degrees" ),
					new GUIContent( "Turns" )
				};
			}

			return angleUnitButtonContents;
		}
	}

	static GUIContent[] angleUnitButtonContentsShort = null;
	public static GUIContent[] AngleUnitButtonContentsShort {
		get {
			if( angleUnitButtonContentsShort == null ) {
				angleUnitButtonContentsShort = new GUIContent[] {
					new GUIContent( "Rad" ),
					new GUIContent( "Deg" ),
					new GUIContent( "Turns" )
				};
			}

			return angleUnitButtonContentsShort;
		}
	}


	static GUIContent[] lineThicknessSpaceLabelsLong = null;
	public static GUIContent[] LineThicknessSpaceLabelsLong {
		get {
			if( lineThicknessSpaceLabelsLong == null ) {
				lineThicknessSpaceLabelsLong = new GUIContent[] {
					new GUIContent( "meters" ),
					new GUIContent( "pixels" ),
					new GUIContent( "noots" )
				};
			}

			return lineThicknessSpaceLabelsLong;
		}
	}

	static GUIContent[] lineThicknessSpaceLabelsShort = null;
	public static GUIContent[] LineThicknessSpaceLabelsShort {
		get {
			if( lineThicknessSpaceLabelsShort == null ) {
				lineThicknessSpaceLabelsShort = new GUIContent[] {
					new GUIContent( "m" ),
					new GUIContent( "px" ),
					new GUIContent( "nt" )
				};
			}

			return lineThicknessSpaceLabelsShort;
		}
	}


}