using System;
using UnityEngine;
using TMPro;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static partial class Draw {

		[OvldGenCallTarget] public static void Line( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													 [OvldDefault( nameof(LineGeometry) )] LineGeometry geometry,
													 [OvldDefault( nameof(LineEndCaps) )] LineEndCap endCaps,
													 [OvldDefault( nameof(LineThicknessSpace) )] ThicknessSpace thicknessSpace,
													 Vector3 start,
													 Vector3 end,
													 [OvldDefault( nameof(Color) )] Color colorStart,
													 [OvldDefault( nameof(Color) )] Color colorEnd,
													 [OvldDefault( nameof(LineThickness) )] float thickness,
													 [OvldDefault( nameof(LineDashSize) )] float dashSize = 0f,
													 [OvldDefault( "0f" )] float dashOffset = 0f ) {
			Material mat = ShapesMaterialUtils.GetLineMat( geometry, endCaps )[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, colorStart );
			mat.SetColor( ShapesMaterialUtils.propColorEnd, colorEnd );
			mat.SetVector( ShapesMaterialUtils.propPointStart, start );
			mat.SetVector( ShapesMaterialUtils.propPointEnd, end );
			mat.SetFloat( ShapesMaterialUtils.propThickness, thickness );
			mat.SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			mat.SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			bool dashed = dashSize > 0f;
			mat.SetFloat( ShapesMaterialUtils.propDashSize, dashed ? 0 : dashSize );
			if( dashed )
				mat.SetFloat( ShapesMaterialUtils.propDashOffset, dashOffset );
			DrawMesh( Vector3.zero, Quaternion.identity, ShapesMeshUtils.GetLineMesh( geometry, endCaps ), mat );
		}


		[OvldGenCallTarget] public static void Polyline( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
														 PolylinePath path,
														 [OvldDefault( "false" )] bool closed,
														 [OvldDefault( nameof(PolylineGeometry) )] PolylineGeometry geometry,
														 [OvldDefault( nameof(PolylineJoins) )] PolylineJoins joins,
														 [OvldDefault( nameof(LineThickness) )] float thickness,
														 [OvldDefault( nameof(LineThicknessSpace) )] ThicknessSpace thicknessSpace,
														 [OvldDefault( nameof(Color) )] Color color ) {
			if( path.EnsureMeshIsReadyToRender( closed, joins, out Mesh mesh ) == false )
				return; // no points defined in the mesh

			switch( path.Count ) {
				case 0:
					Debug.LogWarning( "Tried to draw polyline with no points" );
					return;
				case 1:
					Debug.LogWarning( "Tried to draw polyline with only one point" );
					return;
			}

			Material matPolyLine = ShapesMaterialUtils.GetPolylineMat( joins )[blendMode];
			matPolyLine.SetFloat( ShapesMaterialUtils.propThickness, thickness );
			matPolyLine.SetFloat( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			matPolyLine.SetColor( ShapesMaterialUtils.propColor, color );
			matPolyLine.SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			if( joins == PolylineJoins.Miter ) {
				DrawMesh( Vector3.zero, Quaternion.identity, mesh, matPolyLine );
			} else {
				Material matPolyLineJoins = ShapesMaterialUtils.GetPolylineJoinsMat( joins )[blendMode];
				matPolyLineJoins.SetFloat( ShapesMaterialUtils.propThickness, thickness );
				matPolyLineJoins.SetFloat( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
				matPolyLineJoins.SetColor( ShapesMaterialUtils.propColor, color );
				matPolyLineJoins.SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
				DrawTwoSubmeshes( Vector3.zero, Quaternion.identity, mesh, matPolyLine, matPolyLineJoins );
			}
		}


		[OvldGenCallTarget] public static void Disc( Vector3 pos,
													 [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													 [OvldDefault( nameof(DiscRadius) )] float radius,
													 [OvldDefault( nameof(Color) )] Color colorInnerStart,
													 [OvldDefault( nameof(Color) )] Color colorOuterStart,
													 [OvldDefault( nameof(Color) )] Color colorInnerEnd,
													 [OvldDefault( nameof(Color) )] Color colorOuterEnd ) {
			DiscCore( BlendMode, DiscRadiusSpace, RingThicknessSpace, false, false, pos, rot, radius, 0f, colorInnerStart, colorOuterStart, colorInnerEnd, colorOuterEnd );
		}

		[OvldGenCallTarget] public static void Ring( Vector3 pos,
													 [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													 [OvldDefault( nameof(DiscRadius) )] float radius,
													 [OvldDefault( nameof(RingThickness) )] float thickness,
													 [OvldDefault( nameof(Color) )] Color colorInnerStart,
													 [OvldDefault( nameof(Color) )] Color colorOuterStart,
													 [OvldDefault( nameof(Color) )] Color colorInnerEnd,
													 [OvldDefault( nameof(Color) )] Color colorOuterEnd ) {
			DiscCore( BlendMode, DiscRadiusSpace, RingThicknessSpace, true, false, pos, rot, radius, thickness, colorInnerStart, colorOuterStart, colorInnerEnd, colorOuterEnd );
		}

		[OvldGenCallTarget] public static void Circle( Vector3 pos,
													   [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													   [OvldDefault( nameof(DiscRadius) )] float radius,
													   [OvldDefault( nameof(LineThickness) )] float thickness,
													   [OvldDefault( nameof(Color) )] Color colorInnerStart,
													   [OvldDefault( nameof(Color) )] Color colorOuterStart,
													   [OvldDefault( nameof(Color) )] Color colorInnerEnd,
													   [OvldDefault( nameof(Color) )] Color colorOuterEnd ) {
			DiscCore( BlendMode, DiscRadiusSpace, LineThicknessSpace, true, false, pos, rot, radius, thickness, colorInnerStart, colorOuterStart, colorInnerEnd, colorOuterEnd );
		}

		[OvldGenCallTarget] public static void Pie( Vector3 pos,
													[OvldDefault( "Quaternion.identity" )] Quaternion rot,
													[OvldDefault( nameof(DiscRadius) )] float radius,
													[OvldDefault( nameof(Color) )] Color colorInnerStart,
													[OvldDefault( nameof(Color) )] Color colorOuterStart,
													[OvldDefault( nameof(Color) )] Color colorInnerEnd,
													[OvldDefault( nameof(Color) )] Color colorOuterEnd,
													float angleRadStart,
													float angleRadEnd ) {
			DiscCore( BlendMode, DiscRadiusSpace, RingThicknessSpace, false, true, pos, rot, radius, 0f, colorInnerStart, colorOuterStart, colorInnerEnd, colorOuterEnd, angleRadStart, angleRadEnd );
		}

		[OvldGenCallTarget] public static void Arc( Vector3 pos,
													[OvldDefault( "Quaternion.identity" )] Quaternion rot,
													[OvldDefault( nameof(DiscRadius) )] float radius,
													[OvldDefault( nameof(RingThickness) )] float thickness,
													[OvldDefault( nameof(Color) )] Color colorInnerStart,
													[OvldDefault( nameof(Color) )] Color colorOuterStart,
													[OvldDefault( nameof(Color) )] Color colorInnerEnd,
													[OvldDefault( nameof(Color) )] Color colorOuterEnd,
													float angleRadStart,
													float angleRadEnd,
													[OvldDefault( nameof(ArcEndCap) + "." + nameof(ArcEndCap.None) )] ArcEndCap endCaps ) {
			DiscCore( BlendMode, DiscRadiusSpace, RingThicknessSpace, true, true, pos, rot, radius, thickness, colorInnerStart, colorOuterStart, colorInnerEnd, colorOuterEnd, angleRadStart, angleRadEnd, endCaps );
		}

		static void DiscCore( ShapesBlendMode blendMode, ThicknessSpace spaceRadius, ThicknessSpace spaceThickness, bool hollow, bool sector, Vector3 pos, Quaternion rot, float radius, float thickness, Color colorInnerStart, Color colorOuterStart, Color colorInnerEnd, Color colorOuterEnd, float angleRadStart = 0f, float angleRadEnd = 0f, ArcEndCap arcEndCaps = ArcEndCap.None ) {
			if( sector && Mathf.Abs( angleRadEnd - angleRadStart ) < 0.0001f )
				return;
			Material mat = ShapesMaterialUtils.GetDiscMaterial( hollow, sector )[blendMode];
			mat.SetFloat( ShapesMaterialUtils.propRadius, radius );
			mat.SetInt( ShapesMaterialUtils.propRadiusSpace, (int)spaceRadius );
			if( hollow ) {
				mat.SetInt( ShapesMaterialUtils.propThicknessSpace, (int)spaceThickness );
				mat.SetFloat( ShapesMaterialUtils.propThickness, thickness );
			}

			if( sector ) {
				mat.SetFloat( ShapesMaterialUtils.propAngStart, angleRadStart );
				mat.SetFloat( ShapesMaterialUtils.propAngEnd, angleRadEnd );
				if( hollow )
					mat.SetFloat( ShapesMaterialUtils.propRoundCaps, (int)arcEndCaps );
			}

			mat.SetColor( ShapesMaterialUtils.propColor, colorInnerStart );
			mat.SetColor( ShapesMaterialUtils.propColorOuterStart, colorOuterStart );
			mat.SetColor( ShapesMaterialUtils.propColorInnerEnd, colorInnerEnd );
			mat.SetColor( ShapesMaterialUtils.propColorOuterEnd, colorOuterEnd );
			DrawMesh( pos, rot, ShapesMeshUtils.QuadMesh, mat );
		}

		[OvldGenCallTarget] public static void Rectangle( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
														  [OvldDefault( "false" )] bool hollow,
														  [OvldDefault( "Vector3.zero" )] Vector3 pos,
														  [OvldDefault( "Quaternion.identity" )] Quaternion rot,
														  Rect rect,
														  [OvldDefault( nameof(Color) )] Color color,
														  [OvldDefault( "0f" )] float thickness = 0f,
														  [OvldDefault( "default" )] Vector4 cornerRadii = default ) {
			bool rounded = ShapesMath.MaxComp( cornerRadii ) >= 0.0001f;
			if( hollow && thickness * 2 >= Mathf.Min( rect.width, rect.height ) ) hollow = false;
			Material mat = ShapesMaterialUtils.GetRectMaterial( hollow, rounded )[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, color );
			mat.SetVector( ShapesMaterialUtils.propRect, rect.ToVector4() );
			if( rounded ) mat.SetVector( ShapesMaterialUtils.propCornerRadii, cornerRadii );
			if( hollow ) mat.SetFloat( ShapesMaterialUtils.propThickness, thickness );

			DrawMesh( pos, rot, ShapesMeshUtils.QuadMesh, mat );
		}

		[OvldGenCallTarget] public static void Triangle( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
														 Vector3 a,
														 Vector3 b,
														 Vector3 c,
														 [OvldDefault( nameof(Color) )] Color colorA,
														 [OvldDefault( nameof(Color) )] Color colorB,
														 [OvldDefault( nameof(Color) )] Color colorC ) {
			Material mat = ShapesMaterialUtils.matTriangle[blendMode];
			mat.SetVector( ShapesMaterialUtils.propA, a );
			mat.SetVector( ShapesMaterialUtils.propB, b );
			mat.SetVector( ShapesMaterialUtils.propC, c );
			mat.SetColor( ShapesMaterialUtils.propColor, colorA );
			mat.SetColor( ShapesMaterialUtils.propColorB, colorB );
			mat.SetColor( ShapesMaterialUtils.propColorC, colorC );
			DrawMesh( Vector3.zero, Quaternion.identity, ShapesMeshUtils.TriangleMesh, mat );
		}

		[OvldGenCallTarget] public static void Quad( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													 Vector3 a,
													 Vector3 b,
													 Vector3 c,
													 [OvldDefault( "a + ( c - b )" )] Vector3 d,
													 [OvldDefault( nameof(Color) )] Color colorA,
													 [OvldDefault( nameof(Color) )] Color colorB,
													 [OvldDefault( nameof(Color) )] Color colorC,
													 [OvldDefault( nameof(Color) )] Color colorD ) {
			Material mat = ShapesMaterialUtils.matQuad[blendMode];
			mat.SetVector( ShapesMaterialUtils.propA, a );
			mat.SetVector( ShapesMaterialUtils.propB, b );
			mat.SetVector( ShapesMaterialUtils.propC, c );
			mat.SetVector( ShapesMaterialUtils.propD, d );
			mat.SetColor( ShapesMaterialUtils.propColor, colorA );
			mat.SetColor( ShapesMaterialUtils.propColorB, colorB );
			mat.SetColor( ShapesMaterialUtils.propColorC, colorC );
			mat.SetColor( ShapesMaterialUtils.propColorD, colorD );
			DrawMesh( Vector3.zero, Quaternion.identity, ShapesMeshUtils.QuadMesh, mat );
		}

		[OvldGenCallTarget] public static void Sphere( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													   [OvldDefault( nameof(SphereRadiusSpace) )] ThicknessSpace spaceRadius,
													   Vector3 pos,
													   [OvldDefault( nameof(SphereRadius) )] float radius,
													   [OvldDefault( nameof(Color) )] Color color ) {
			Material mat = ShapesMaterialUtils.matSphere[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, color );
			mat.SetFloat( ShapesMaterialUtils.propRadius, radius );
			mat.SetInt( ShapesMaterialUtils.propRadiusSpace, (int)spaceRadius );
			DrawMesh( pos, Quaternion.identity, ShapesMeshUtils.SphereMesh, mat );
		}

		[OvldGenCallTarget] public static void Cone( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													 [OvldDefault( nameof(ConeSizeSpace) )] ThicknessSpace sizeSpace,
													 Vector3 pos,
													 [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													 float radius,
													 float length,
													 [OvldDefault( "true" )] bool fillCap,
													 [OvldDefault( nameof(Color) )] Color color ) {
			Material mat = ShapesMaterialUtils.matCone[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, color );
			mat.SetFloat( ShapesMaterialUtils.propRadius, radius );
			mat.SetFloat( ShapesMaterialUtils.propLength, length );
			mat.SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
			DrawMesh( pos, rot, fillCap ? ShapesMeshUtils.ConeMesh : ShapesMeshUtils.ConeMeshUncapped, mat );
		}

		[OvldGenCallTarget] public static void Cuboid( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													   [OvldDefault( nameof(CuboidSizeSpace) )] ThicknessSpace sizeSpace,
													   Vector3 pos,
													   [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													   Vector3 size,
													   [OvldDefault( nameof(Color) )] Color color ) {
			Material mat = ShapesMaterialUtils.matCuboid[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, color );
			mat.SetVector( ShapesMaterialUtils.propSize, size );
			mat.SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
			DrawMesh( pos, rot, ShapesMeshUtils.CuboidMesh, mat );
		}

		[OvldGenCallTarget] public static void Torus( [OvldDefault( nameof(BlendMode) )] ShapesBlendMode blendMode,
													  [OvldDefault( nameof(TorusRadiusSpace) )] ThicknessSpace spaceRadius,
													  [OvldDefault( nameof(TorusThicknessSpace) )] ThicknessSpace spaceThickness,
													  Vector3 pos,
													  [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													  float radius,
													  float thickness,
													  [OvldDefault( nameof(Color) )] Color color ) {
			if( thickness < 0.0001f )
				return;
			if( radius < 0.00001f ) {
				Sphere( blendMode, spaceThickness, pos, thickness, color );
				return;
			}

			Material mat = ShapesMaterialUtils.matTorus[blendMode];
			mat.SetColor( ShapesMaterialUtils.propColor, color );
			mat.SetFloat( ShapesMaterialUtils.propRadius, radius );
			mat.SetFloat( ShapesMaterialUtils.propThickness, thickness );
			mat.SetInt( ShapesMaterialUtils.propRadiusSpace, (int)spaceRadius );
			mat.SetInt( ShapesMaterialUtils.propThicknessSpace, (int)spaceThickness );
			DrawMesh( pos, rot, ShapesMeshUtils.TorusMesh, mat );
		}

		[OvldGenCallTarget] public static void Text( Vector3 pos,
													 [OvldDefault( "Quaternion.identity" )] Quaternion rot,
													 string content,
													 [OvldDefault( nameof(Font) )] TMP_FontAsset font,
													 [OvldDefault( nameof(FontSize) )] float fontSize,
													 [OvldDefault( nameof(TextAlign) )] TextAlign align,
													 [OvldDefault( nameof(Color) )] Color color ) {
			TextMeshPro tmp = ShapesTextDrawer.Instance.tmp;

			// Statics
			tmp.font = font;
			tmp.color = color;
			tmp.fontSize = fontSize;

			// Per-instance
			tmp.text = content;
			tmp.alignment = align.GetTMPAlignment();
			tmp.rectTransform.pivot = align.GetPivot();
			tmp.transform.position = pos;
			tmp.rectTransform.rotation = rot;
			tmp.ForceMeshUpdate();

			// Actually draw
			font.material.SetPass( 0 );
			Matrix4x4 mtx = GetDrawingMatrix( tmp.transform.position, tmp.transform.rotation );
			for( int sm = 0; sm < tmp.mesh.subMeshCount; sm++ )
				Graphics.DrawMeshNow( tmp.mesh, mtx, sm );
		}

		static Matrix4x4 GetDrawingMatrix( Vector3 pos, Quaternion rot ) {
			Matrix4x4 mtx = Matrix4x4.TRS( pos, rot, Vector3.one );
			if( hasCustomMatrix )
				mtx = matrix * mtx;
			return mtx;
		}

		public static void DrawMesh( Vector3 pos, Quaternion rot, Mesh mesh, Material mat ) {
			mat.SetPass( 0 );
			Matrix4x4 mtx = GetDrawingMatrix( pos, rot );
			//MaterialPropertyBlock mpb = new MaterialPropertyBlock();
			//mpb.SetColor( MaterialUtils.propColor, color );
			for( int sm = 0; sm < mesh.subMeshCount; sm++ ) {
				// Graphics.DrawMesh( mesh, mtx, mat, 0, null, sm, mpb );
				Graphics.DrawMeshNow( mesh, mtx, sm );
			}
		}

		// used for polyline. 0 = lines, 1 = caps
		static void DrawTwoSubmeshes( Vector3 pos, Quaternion rot, Mesh mesh, Material mat0, Material mat1 ) {
			Matrix4x4 mtx = GetDrawingMatrix( pos, rot );
			mat0.SetPass( 0 );
			Graphics.DrawMeshNow( mesh, mtx, 0 );
			mat1.SetPass( 0 );
			Graphics.DrawMeshNow( mesh, mtx, 1 );
		}

	}

	// these are used by CodegenDrawOverloads
	[AttributeUsage( AttributeTargets.Method )]
	public class OvldGenCallTarget : Attribute {
	}

	[AttributeUsage( AttributeTargets.Parameter )]
	public class OvldDefault : Attribute {
		public string @default;
		public OvldDefault( string @default ) => this.@default = @default;
	}

}