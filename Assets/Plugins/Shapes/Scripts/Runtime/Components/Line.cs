using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Line" )]
	public class Line : ShapeRenderer {

		public enum LineColorMode {
			Single,
			Double
		}

		public Vector3 this[ int i ] {
			get => i > 0 ? End : Start;
			set => _ = i > 0 ? End = value : Start = value;
		}

		// also called alignment for 2D lines
		[SerializeField] LineGeometry geometry = LineGeometry.Billboard;
		public LineGeometry Geometry {
			get => geometry;
			set {
				geometry = value;
				SetIntNow( ShapesMaterialUtils.propAlignment, (int)geometry );
				UpdateMesh( true );
				UpdateMaterial();
				ApplyProperties();
			}
		}
		[SerializeField] LineColorMode colorMode = LineColorMode.Single;
		public LineColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}
		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorEnd, colorEnd = value );
			}
		}
		public Color ColorStart {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] Color colorEnd = Color.white;
		public Color ColorEnd {
			get => colorEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorEnd, colorEnd = value );
		}
		[SerializeField] Vector3 start = Vector3.zero;
		public Vector3 Start {
			get => start;
			set => SetVector3Now( ShapesMaterialUtils.propPointStart, start = value );
		}
		[SerializeField] Vector3 end = Vector3.right;
		public Vector3 End {
			get => end;
			set => SetVector3Now( ShapesMaterialUtils.propPointEnd, end = value );
		}
		[SerializeField] float thickness = 0.125f;
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = value );
		}
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}
		[SerializeField] bool dashed = false;
		public bool Dashed {
			get => dashed;
			set {
				dashed = value;
				ApplyProperties();
			}
		}
		[SerializeField] float dashSize = 4f;
		public float DashSize {
			get => dashSize;
			set => SetFloatNow( ShapesMaterialUtils.propDashSize, dashSize = value );
		}
		[SerializeField] float dashOffset = 0f;
		public float DashOffset {
			get => dashOffset;
			set => SetFloatNow( ShapesMaterialUtils.propDashOffset, dashOffset = value );
		}
		[SerializeField] LineEndCap endCaps = LineEndCap.Round;
		public LineEndCap EndCaps {
			get => endCaps;
			set {
				endCaps = value;
				ApplyProperties();
			}
		}

		protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propPointStart, start );
			SetVector3( ShapesMaterialUtils.propPointEnd, end );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetFloat( ShapesMaterialUtils.propDashSize, dashed ? dashSize : 0 );
			SetFloat( ShapesMaterialUtils.propDashOffset, dashOffset );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			if( colorMode == LineColorMode.Double )
				SetColor( ShapesMaterialUtils.propColorEnd, colorEnd );
			else
				SetColor( ShapesMaterialUtils.propColorEnd, base.Color );
		}

		protected override Bounds GetBounds() {
			// presume 0 world space padding when pixels or noots are used
			float padding = thicknessSpace == ThicknessSpace.Meters ? thickness : 0f;
			Vector3 center = ( start + end ) / 2f;
			Vector3 size = ShapesMath.Abs( start - end ) + new Vector3( padding, padding, padding );
			return new Bounds( center, size );
		}

		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.GetLineMat( geometry, endCaps )[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.GetLineMesh( geometry, endCaps );

		protected override void ShapeClampRanges() {
			base.ShapeClampRanges();
			thickness = Mathf.Max( 0, thickness );
		}

	}

}