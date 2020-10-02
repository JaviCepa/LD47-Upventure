using System;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesMaterialUtils {

		// properties
		public static readonly int propColor = Shader.PropertyToID( "_Color" ); // used for all shapes
		public static readonly int propColorMode = Shader.PropertyToID( "_ColorMode" ); // polyline
		public static readonly int propColorEnd = Shader.PropertyToID( "_ColorEnd" ); // line
		public static readonly int propColorOuterStart = Shader.PropertyToID( "_ColorOuterStart" ); // disc sectors
		public static readonly int propColorInnerEnd = Shader.PropertyToID( "_ColorInnerEnd" ); // disc sectors
		public static readonly int propColorOuterEnd = Shader.PropertyToID( "_ColorOuterEnd" ); // disc sectors
		public static readonly int propColorB = Shader.PropertyToID( "_ColorB" ); // triangle and quad
		public static readonly int propColorC = Shader.PropertyToID( "_ColorC" ); // triangle and quad
		public static readonly int propColorD = Shader.PropertyToID( "_ColorD" ); // quad
		public static readonly int propPointStart = Shader.PropertyToID( "_PointStart" ); // line
		public static readonly int propPointEnd = Shader.PropertyToID( "_PointEnd" ); // line
		public static readonly int propA = Shader.PropertyToID( "_A" ); // triangle and quad
		public static readonly int propB = Shader.PropertyToID( "_B" ); // triangle and quad
		public static readonly int propC = Shader.PropertyToID( "_C" ); // triangle and quad
		public static readonly int propD = Shader.PropertyToID( "_D" ); // quad
		public static readonly int propRect = Shader.PropertyToID( "_Rect" ); // rect
		public static readonly int propRadius = Shader.PropertyToID( "_Radius" ); // disc, cone, sphere
		public static readonly int propCornerRadii = Shader.PropertyToID( "_CornerRadii" ); // rect
		public static readonly int propLength = Shader.PropertyToID( "_Length" ); // cone
		public static readonly int propAngStart = Shader.PropertyToID( "_AngleStart" ); // disc sectors
		public static readonly int propAngEnd = Shader.PropertyToID( "_AngleEnd" ); // disc sectors
		public static readonly int propRoundCaps = Shader.PropertyToID( "_RoundCaps" ); // arcs
		public static readonly int propThickness = Shader.PropertyToID( "_Thickness" ); // line, rect
		public static readonly int propThicknessSpace = Shader.PropertyToID( "_ThicknessSpace" ); // line
		public static readonly int propRadiusSpace = Shader.PropertyToID( "_RadiusSpace" ); // ring, torus
		public static readonly int propDashSize = Shader.PropertyToID( "_DashSize" ); // line
		public static readonly int propDashOffset = Shader.PropertyToID( "_DashOffset" ); // line
		public static readonly int propSize = Shader.PropertyToID( "_Size" ); // cuboid
		public static readonly int propSizeSpace = Shader.PropertyToID( "_SizeSpace" ); // cuboid
		public static readonly int propAlignment = Shader.PropertyToID( "_Alignment" ); // line, todo: polyline

		// materials
		static readonly ShapesMaterials matDisc = new ShapesMaterials( "Disc" );
		static readonly ShapesMaterials matCircleSector = new ShapesMaterials( "Disc", "SECTOR" );
		static readonly ShapesMaterials matRing = new ShapesMaterials( "Disc", "INNER_RADIUS" );
		static readonly ShapesMaterials matRingSector = new ShapesMaterials( "Disc", "INNER_RADIUS", "SECTOR" );
		static readonly ShapesMaterials matRectSimple = new ShapesMaterials( "Rect" );
		static readonly ShapesMaterials matRectRounded = new ShapesMaterials( "Rect", "CORNER_RADIUS" );
		static readonly ShapesMaterials matRectBorder = new ShapesMaterials( "Rect", "BORDERED" );
		static readonly ShapesMaterials matRectBorderRounded = new ShapesMaterials( "Rect", "CORNER_RADIUS", "BORDERED" );
		public static readonly ShapesMaterials matTriangle = new ShapesMaterials( "Triangle" );
		public static readonly ShapesMaterials matQuad = new ShapesMaterials( "Quad" );
		public static readonly ShapesMaterials matSphere = new ShapesMaterials( "Sphere" );
		public static readonly ShapesMaterials matCone = new ShapesMaterials( "Cone" );
		public static readonly ShapesMaterials matCuboid = new ShapesMaterials( "Cuboid" );
		public static readonly ShapesMaterials matTorus = new ShapesMaterials( "Torus" );

		static readonly ShapesMaterials[ /*cap*/] matsLine = {
			new ShapesMaterials( "Line 2D" ),
			new ShapesMaterials( "Line 2D", "CAP_SQUARE" ),
			new ShapesMaterials( "Line 2D", "CAP_ROUND" )
		};

		static readonly ShapesMaterials[ /*cap*/] matsLine3D = {
			new ShapesMaterials( "Line 3D" ),
			new ShapesMaterials( "Line 3D", "CAP_SQUARE" ),
			new ShapesMaterials( "Line 3D", "CAP_ROUND" )
		};

		static readonly ShapesMaterials[] matsPolyline = {
			new ShapesMaterials( "Polyline 2D" ), // simple join
			new ShapesMaterials( "Polyline 2D", "JOIN_MITER" ),
			new ShapesMaterials( "Polyline 2D", "JOIN_ROUND" ),
			new ShapesMaterials( "Polyline 2D", "JOIN_BEVEL" )
		};

		static readonly ShapesMaterials[] matsPolylineJoin = {
			new ShapesMaterials( "Polyline 2D", "IS_JOIN_MESH" ), // simple join
			new ShapesMaterials( "Polyline 2D", "IS_JOIN_MESH", "JOIN_MITER" ),
			new ShapesMaterials( "Polyline 2D", "IS_JOIN_MESH", "JOIN_ROUND" ),
			new ShapesMaterials( "Polyline 2D", "IS_JOIN_MESH", "JOIN_BEVEL" )
		};

		// helper functions
		public static ShapesMaterials GetDiscMaterial( bool hollow, bool sector ) {
			if( hollow )
				return sector ? matRingSector : matRing;
			else
				return sector ? matCircleSector : matDisc;
		}

		public static ShapesMaterials GetDiscMaterial( Disc.DiscType type ) {
			switch( type ) {
				case Disc.DiscType.Disc: return matDisc;
				case Disc.DiscType.Pie:  return matCircleSector;
				case Disc.DiscType.Ring: return matRing;
				case Disc.DiscType.Arc:  return matRingSector;
				default:                 return null;
			}
		}

		public static ShapesMaterials GetRectMaterial( bool hollow, bool rounded ) {
			if( hollow )
				return rounded ? matRectBorderRounded : matRectBorder;
			else
				return rounded ? matRectRounded : matRectSimple;
		}

		public static ShapesMaterials GetRectMaterial( Rectangle.RectangleType type ) {
			switch( type ) {
				case Rectangle.RectangleType.HardSolid:     return matRectSimple;
				case Rectangle.RectangleType.RoundedSolid:  return matRectRounded;
				case Rectangle.RectangleType.HardHollow:    return matRectBorder;
				case Rectangle.RectangleType.RoundedHollow: return matRectBorderRounded;
				default:                                    return null;
			}
		}

		public static ShapesMaterials GetPolylineMat( PolylineJoins join ) => matsPolyline[(int)join];

		public static ShapesMaterials GetPolylineJoinsMat( PolylineJoins join ) => matsPolylineJoin[(int)join];

		public static ShapesMaterials GetLineMat( LineGeometry geometry, LineEndCap cap ) {
			switch( geometry ) {
				case LineGeometry.Billboard:
				case LineGeometry.Flat2D:
					return matsLine[(int)cap];
				case LineGeometry.Volumetric3D:
					return matsLine3D[(int)cap];
				default:
					throw new ArgumentOutOfRangeException( nameof(geometry), geometry, null );
			}
		}


	}

}