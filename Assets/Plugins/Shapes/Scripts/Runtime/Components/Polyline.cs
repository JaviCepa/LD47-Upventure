using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Polyline" )]
	public class Polyline : ShapeRenderer {

		[SerializeField] List<PolylinePoint> polyPoints = new List<PolylinePoint>() {
			new PolylinePoint( new Vector3( 0, 1, 0 ), Color.white ),
			new PolylinePoint( new Vector3( 0.86602540378f, -.5f, 0 ), Color.white ),
			new PolylinePoint( new Vector3( -0.86602540378f, -.5f, 0 ), Color.white )
		};

		// also called alignment
		[SerializeField] PolylineGeometry geometry = PolylineGeometry.Flat2D;
		public PolylineGeometry Geometry {
			get => geometry;
			set {
				geometry = value;
				SetIntNow( ShapesMaterialUtils.propAlignment, (int)geometry );
				UpdateMaterial();
				ApplyProperties();
			}
		}

		[SerializeField] PolylineJoins joins = PolylineJoins.Miter;
		public PolylineJoins Joins {
			get => joins;
			set {
				joins = value;
				meshOutOfDate = true;
			}
		}

		[SerializeField] bool closed = true;
		public bool Closed {
			get => closed;
			set {
				closed = value;
				meshOutOfDate = true;
			}
		}

		[SerializeField] float thickness = 0.125f;
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = value );
		}

		// todo: make this work
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}

		public int Count => polyPoints.Count;
		public PolylinePoint this[ int i ] {
			get => polyPoints[i];
			set => polyPoints[i] = value;
		}

		public void SetPoints( IReadOnlyCollection<Vector3> points, IReadOnlyCollection<Color> colors = null ) {
			this.polyPoints.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		public void SetPoints( IReadOnlyCollection<Vector2> points, IReadOnlyCollection<Color> colors = null ) {
			this.polyPoints.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		public void SetPoints( IEnumerable<PolylinePoint> points ) {
			this.polyPoints.Clear();
			AddPoints( points );
		}

		void AddPoints( IEnumerable<PolylinePoint> points ) {
			polyPoints.AddRange( points );
			meshOutOfDate = true;
		}


		bool meshOutOfDate = true;

		void OnWillRenderObject() {
			if( meshOutOfDate ) {
				meshOutOfDate = false;
				UpdateMesh( force: true );
			}
		}


		protected override MeshUpdateMode MeshUpdateMode => MeshUpdateMode.SelfGenerated;
		protected override void GenerateMesh() => ShapesMeshGen.GenPolylineMesh( Mesh, polyPoints, closed, joins, true );

		protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
		}

		protected override void ShapeClampRanges() => thickness = Mathf.Max( 0f, thickness );

		protected override Material[] GetMaterials() {
			if( joins.HasJoinMesh() )
				return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode], ShapesMaterialUtils.GetPolylineJoinsMat( joins )[BlendMode] };
			return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode] };
		}

		protected override Bounds GetBounds() {
			if( polyPoints.Count < 2 )
				return default;
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			foreach( Vector3 pt in polyPoints.Select( p => p.point ) ) {
				min = Vector3.Min( min, pt );
				max = Vector3.Max( max, pt );
			}

			return new Bounds( ( max + min ) * 0.5f, ( max - min ) + Vector3.one * ( thickness * 0.5f ) );
		}

	}

}