using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[Serializable]
	public struct PolylinePoint {
		public Vector3 point;
		public Color color;
		public float thickness;


		public PolylinePoint( Vector3 point, Color color ) {
			this.point = point;
			this.color = color;
			this.thickness = 1;
		}

		public PolylinePoint( Vector2 point, Color color ) {
			this.point = point;
			this.color = color;
			this.thickness = 1;
		}

		public PolylinePoint( Vector3 point, Color color, float thickness ) {
			this.point = point;
			this.color = color;
			this.thickness = thickness;
		}

		public PolylinePoint( Vector2 point, Color color, float thickness ) {
			this.point = point;
			this.color = color;
			this.thickness = thickness;
		}
		
	}

}