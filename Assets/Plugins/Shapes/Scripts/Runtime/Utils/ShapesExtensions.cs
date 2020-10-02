using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesExtensions {
		public static void ForEach<T>( this IEnumerable<T> elems, Action<T> action ) {
			foreach( T e in elems )
				action( e );
		}

		public static Vector3 Rot90CCW( this Vector3 v ) => new Vector3( -v.y, v.x );
		public static int AsInt( this bool b ) => b ? 1 : 0;
		public static Vector4 ToVector4( this Rect r ) => new Vector4( r.x, r.y, r.width, r.height );
		public static float TaxicabMagnitude( this Vector3 v ) => Mathf.Abs( v.x ) + Mathf.Abs( v.y ) + Mathf.Abs( v.z );

		// because outside of play mode, we have to use DestroyImmediate, but we want to use Destroy otherwise
		public static void DestroyBranched( this Object obj ) {
			#if UNITY_EDITOR
			if( EditorApplication.isPlaying == false )
				Object.DestroyImmediate( obj );
			else
				Object.Destroy( obj );
			#else
				Object.Destroy( obj );
			#endif
		}

		// based on https://answers.unity.com/questions/420772/how-to-destroy-linked-components-when-object-is-de.html
		public static void TryDestroyInOnDestroy( this Object caller, Object obj ) {
			if( obj == null ) return;
			#if UNITY_EDITOR
			if( Application.isEditor && Application.isPlaying == false )
				EditorApplication.delayCall += () => {
					if( Application.isPlaying == false && obj != null )
						Object.DestroyImmediate( obj );
				}; // execute late in this editor frame, if still not playing
			else
				Object.Destroy( obj );
			#else
			Object.Destroy( obj );
			#endif
		}

		// linq style product, similar to Sum()
		public static int Product<T>( this IEnumerable<T> arr, Func<T, int> mulVal ) {
			int product = 1;
			foreach( T obj in arr )
				product *= mulVal( obj );
			return product;
		}

		public static float Product<T>( this IEnumerable<T> arr, Func<T, float> mulVal ) {
			float product = 1;
			foreach( T obj in arr )
				product *= mulVal( obj );
			return product;
		}

		// LINQ Zip with three IEnumerables instead of two
		// from https://stackoverflow.com/a/10297160
		public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>( this IEnumerable<T1> source, IEnumerable<T2> second, IEnumerable<T3> third, Func<T1, T2, T3, TResult> func ) {
			using( var e1 = source.GetEnumerator() )
				using( var e2 = second.GetEnumerator() )
					using( var e3 = third.GetEnumerator() )
						while( e1.MoveNext() && e2.MoveNext() && e3.MoveNext() )
							yield return func( e1.Current, e2.Current, e3.Current );
		}

		// from https://gist.github.com/sebgod/708b49b96fd7ce4a2791
		public static int PopCount( this uint i ) {
			i = i - ( ( i >> 1 ) & 0x55555555 ); // reuse input as temporary
			i = ( i & 0x33333333 ) + ( ( i >> 2 ) & 0x33333333 ); // temp
			i = ( ( i + ( i >> 4 ) & 0xF0F0F0F ) * 0x1010101 ) >> 24; // count
			return unchecked( (int)i );
		}


		#if UNITY_EDITOR

		// array deconstruction
		// slightly modified version of from https://stackoverflow.com/a/47816647
		public static void Deconstruct<T>( this IList<T> list, out T first, out T second ) {
			first = list.Count > 0 ? list[0] : default(T); // or throw
			second = list.Count > 1 ? list[1] : default(T); // or throw
		}

		public static bool[] TryGetMultiselectPressedStates( this SerializedProperty prop ) {
			SerializedObject so = prop.serializedObject;
			if( so.isEditingMultipleObjects ) {
				bool[] bools = new bool[prop.enumNames.Length];
				foreach( int shapeID in so.targetObjects.Select( obj => new SerializedObject( obj ).FindProperty( prop.name ).enumValueIndex ) )
					bools[shapeID] = true;
				return bools;
			} else {
				return null;
			}
		}

		// slightly fixed code from https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/
		public static IEnumerable<SerializedProperty> GetVisibleChildren( this SerializedProperty serializedProperty ) {
			SerializedProperty currentProperty = serializedProperty.Copy();
			SerializedProperty nextSiblingProperty = serializedProperty.Copy();
			nextSiblingProperty.NextVisible( false );

			if( currentProperty.NextVisible( true ) ) {
				do {
					if( SerializedProperty.EqualContents( currentProperty, nextSiblingProperty ) )
						break;
					yield return currentProperty.Copy();
				} while( currentProperty.NextVisible( false ) );
			}
		}
		#endif


	}

}