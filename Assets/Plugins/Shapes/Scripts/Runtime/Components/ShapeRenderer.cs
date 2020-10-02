using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[DisallowMultipleComponent]
	public abstract class ShapeRenderer : MonoBehaviour {

		MeshRenderer rnd;
		MeshFilter mf;
		int meshOwnerID;
		MaterialPropertyBlock mpb;
		MaterialPropertyBlock Mpb => mpb ?? ( mpb = new MaterialPropertyBlock() ); // hecking, gosh, I want the C#8 ??= operator 

		public Mesh Mesh {
			get => mf.sharedMesh;
			private set => mf.sharedMesh = value;
		}

		// Properties
		[SerializeField] ShapesBlendMode blendMode = ShapesBlendMode.Transparent;
		public ShapesBlendMode BlendMode {
			get => blendMode;
			set {
				blendMode = value;
				UpdateMaterial();
			}
		}
		[SerializeField] protected Color color = Color.white;
		public virtual Color Color {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}

		#if UNITY_EDITOR
		public virtual void OnValidate() {
			// OnValidate can get called before awake in editor, so make sure the required things are initialized
			if( rnd == null ) rnd = GetComponent<MeshRenderer>(); // Needed for ApplyProperties
			if( mf == null ) mf = GetComponent<MeshFilter>(); // Needed for UpdateMesh
			ShapeClampRanges();
			InitializeProperties();
			ApplyProperties();

			// UpdateMesh( force:true ); gosh I wish I could do this it would solve so many problems but Unity has some WEIRD quirks here
		}

		public void HideMeshFilterRenderer() {
			const HideFlags flags = HideFlags.HideInInspector; // Hide mesh renderer and filter
			rnd.hideFlags = flags;
			mf.hideFlags = flags;
		}
		#endif

		void MakeSureComponentExists<T>( ref T field ) where T : Component {
			if( field == null ) {
				field = GetComponent<T>();
				if( field == null )
					field = gameObject.AddComponent<T>();
				field.hideFlags = HideFlags.HideInInspector;
			}
		}

		public virtual void Awake() {
			MakeSureComponentExists( ref mf );
			MakeSureComponentExists( ref rnd );
			UpdateMaterial();
			UpdateMesh();
			InitializeProperties();
		}

		bool HasGeneratedOrCopyOfMesh => MeshUpdateMode == MeshUpdateMode.SelfGenerated || MeshUpdateMode == MeshUpdateMode.UseAssetCopy;

		public virtual void OnEnable() {
			UpdateMesh();
			rnd.enabled = true;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				Undo.undoRedoPerformed += UpdateMeshOnUndoRedo;
			#endif
		}

		void OnDisable() {
			if( rnd != null )
				rnd.enabled = false;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				Undo.undoRedoPerformed -= UpdateMeshOnUndoRedo;
			#endif
		}

		#if UNITY_EDITOR
		void UpdateMeshOnUndoRedo() => UpdateMesh( true );
		#endif

		void Reset() {
			InitializeProperties();
			UpdateMesh( true );
		}

		void OnDestroy() {
			if( HasGeneratedOrCopyOfMesh && Mesh != null )
				DestroyImmediate( Mesh );
			this.TryDestroyInOnDestroy( rnd );
			this.TryDestroyInOnDestroy( mf );
		}


		protected virtual void DrawGizmos( bool selected ) => _ = 0;
		private void OnDrawGizmos() => DrawGizmos( false );
		private void OnDrawGizmosSelected() => DrawGizmos( true );
		protected abstract void SetAllMaterialProperties();
		protected virtual void ShapeClampRanges() => _ = 0;
		protected abstract Material[] GetMaterials();
		protected abstract Bounds GetBounds();
		protected virtual void GenerateMesh() => _ = 0;
		protected virtual Mesh GetInitialMeshAsset() => ShapesMeshUtils.QuadMesh;
		protected virtual MeshUpdateMode MeshUpdateMode => MeshUpdateMode.UseAsset;

		void UpdateMeshBounds() => Mesh.bounds = GetBounds();

		protected void UpdateMaterial() {
			#if UNITY_EDITOR
			Material[] targetMats = GetMaterials();
			bool needsUpdate = false;
			if( rnd.sharedMaterials.Length != targetMats.Length ) {
				needsUpdate = true;
			} else {
				for( int i = 0; i < targetMats.Length; i++ ) {
					if( rnd.sharedMaterials[i] != targetMats[i] ) {
						string shMat = rnd.sharedMaterials[i] == null ? "null" : rnd.sharedMaterials[i].GetType().Name;
						needsUpdate = true;
						break;
					}
				}
			}

			if( needsUpdate ) {
				SerializedObject soRnd = new SerializedObject( rnd );
				Undo.RecordObject( rnd, "" );
				rnd.sharedMaterials = targetMats;
			}
			#else
			rnd.sharedMaterials = GetMaterials();
			#endif
		}

		public void UpdateMesh( bool force = false ) {
			MeshUpdateMode mode = MeshUpdateMode;

			// if we're using a mesh asset, we only assign if it's null or mismatching
			if( mode == MeshUpdateMode.UseAsset && ( Mesh == null || Mesh != GetInitialMeshAsset() ) ) {
				Mesh = GetInitialMeshAsset();
				return;
			}

			// the next two modes are copy-sensitive, meaning that if we duplicate this object,
			// we also have to duplicate the mesh and update which mesh the duplicate is pointing to
			int id = gameObject.GetInstanceID();

			bool createMesh = Mesh == null || meshOwnerID != id;

			// create new mesh
			if( createMesh ) {
				meshOwnerID = id;
				if( mode == MeshUpdateMode.UseAssetCopy ) {
					Mesh = Instantiate( GetInitialMeshAsset() );
					Mesh.hideFlags = HideFlags.HideAndDontSave;
					Mesh.MarkDynamic();
				} else if( mode == MeshUpdateMode.SelfGenerated ) {
					Mesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave };
					Mesh.MarkDynamic();
					GenerateMesh();
				}
			} else if( force && mode == MeshUpdateMode.SelfGenerated ) {
				GenerateMesh(); // update existing mesh
			}
		}

		public Bounds GetWorldBounds() {
			Bounds localBounds = GetBounds();
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;

			Transform tf = transform;
			for( int x = -1; x <= 1; x += 2 )
				for( int y = -1; y <= 1; y += 2 )
					for( int z = -1; z <= 1; z += 2 ) {
						Vector3 wPt = tf.TransformPoint( localBounds.center + Vector3.Scale( localBounds.extents, new Vector3( x, y, z ) ) );
						min = Vector3.Min( min, wPt );
						max = Vector3.Max( max, wPt );
					}

			return new Bounds( ( max + min ) / 2f, max - min );
		}

		void OnDidApplyAnimationProperties() => InitializeProperties(); // so this is not great but it works don't judge

		void InitializeProperties() {
			SetColor( ShapesMaterialUtils.propColor, color );
			UpdateMaterial();
			SetAllMaterialProperties();
			ApplyProperties();
		}

		protected void ApplyProperties() {
			rnd.SetPropertyBlock( Mpb );
			if( MeshUpdateMode == MeshUpdateMode.UseAssetCopy )
				UpdateMeshBounds();
		}


		protected void SetColor( int prop, Color value ) => Mpb.SetColor( prop, value );
		protected void SetFloat( int prop, float value ) => Mpb.SetFloat( prop, value );
		protected void SetInt( int prop, int value ) => Mpb.SetInt( prop, value );
		protected void SetVector3( int prop, Vector3 value ) => Mpb.SetVector( prop, value );
		protected void SetVector4( int prop, Vector4 value ) => Mpb.SetVector( prop, value );

		protected void SetColorNow( int prop, Color value ) {
			Mpb.SetColor( prop, value );
			ApplyProperties();
		}

		protected void SetFloatNow( int prop, float value ) {
			Mpb.SetFloat( prop, value );
			ApplyProperties();
		}

		protected void SetIntNow( int prop, int value ) {
			Mpb.SetInt( prop, value );
			ApplyProperties();
		}

		protected void SetVector3Now( int prop, Vector3 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}

		protected void SetVector4Now( int prop, Vector4 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}


	}

}