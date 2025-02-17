﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Lattice
{
	abstract public class LatticeModifierBase : MonoBehaviour, ISerializationCallbackReceiver
	{
		#region Constants

		private const string TargetMeshTooltip = 
			"The mesh to apply deformations to and to render on this object.";

		private const string ApplyMethodTooltip =
			"How to apply the deformation.";

		private const string StretchChannelTooltip =
			"Which UV channel to apply stretching to. Will overwrite existing data.";

		private const string UpdateModeTooltip =
			"When deformations should update. Only applies to non skinning lattices. " +  
			"If set to Manual, you must call RequestUpdate().";

		private const string LatticesTooltip =
			"Lattices to apply to the target mesh. " + 
			"These will be applied in order and before skinning.";

		private const GraphicsBuffer.Target BufferTargets = GraphicsBuffer.Target.Raw
			| GraphicsBuffer.Target.CopySource | GraphicsBuffer.Target.CopyDestination;

		#endregion

		#region Fields

		[SerializeField, NotKeyable, Tooltip(TargetMeshTooltip)] 
		private Mesh _targetMesh;

		[SerializeField, NotKeyable, Tooltip(ApplyMethodTooltip)]
		private ApplyMethod _applyMethod = ApplyMethod.PositionNormalTangent;

		[SerializeField, NotKeyable, Tooltip(StretchChannelTooltip), ShowIf(nameof(_applyMethod), ApplyMethod.Stretch)]
		private TextureCoordinate _stretchChannel = TextureCoordinate.TexCoord3;

		[SerializeField, Tooltip(UpdateModeTooltip)]
		private UpdateMode _updateMode = UpdateMode.WhenVisible;

		[SerializeField, Tooltip(LatticesTooltip)]
		private List<LatticeItem> _lattices = new() { new LatticeItem() };

		private Mesh _mesh;
		private MeshInfo _meshInfo;

		private GraphicsBuffer _copyBuffer;
		private GraphicsBuffer _vertexBuffer;
		private GraphicsBuffer _additionalBuffer;

		private bool _ranThisFrame = false;
		private ApplyMethod _actualApplyMethod;

		private Mesh _currentTargetMesh;
		private ApplyMethod _currentApplyMethod;
		private TextureCoordinate _currentStretchChannel;

		#endregion

		#region Properties

		/// <summary>
		/// Lattices to apply to this mesh.
		/// </summary>
		public List<LatticeItem> Lattices => _lattices;

		/// <summary>
		/// Method of updating and applying the lattices.
		/// </summary>
		public UpdateMode UpdateMode { get => _updateMode; set => _updateMode = value; }

		/// <summary>
		/// Vertex information about this mesh.
		/// </summary>
		internal MeshInfo MeshInfo => _meshInfo;

		/// <summary>
		/// Setting for how the deformation will be applied.
		/// </summary>
		internal ApplyMethod ApplyMethod => _actualApplyMethod;

		/// <summary>
		/// Texcoord channel to apply the stretch/squish amount to.
		/// </summary>
		internal TextureCoordinate StretchChannel => _stretchChannel;

		/// <summary>
		/// A copy of this mesh's vertex buffer.
		/// </summary>
		internal GraphicsBuffer CopyBuffer => _copyBuffer;

		/// <summary>
		/// The mesh's vertex buffer.
		/// </summary>
		internal GraphicsBuffer VertexBuffer => _vertexBuffer;

		/// <summary>
		/// The mesh's stretch buffer.
		/// </summary>
		internal GraphicsBuffer AdditionalBuffer => _additionalBuffer;

		/// <summary>
		/// The mesh's local to world transform matrix.
		/// </summary>
		internal Matrix4x4 LocalToWorld => transform.localToWorldMatrix;

		/// <summary>
		/// Whether the component is valid and can be applied without errors.
		/// </summary>
		internal bool IsValid => _vertexBuffer != null && _copyBuffer != null;

		#endregion

		#region Public Methods

		/// <summary>
		/// Will force an update of the Lattice deformations this frame.
		/// </summary>
		public void RequestUpdate()
		{
			EnqueueIfNeeded(true);
		}

		/// <summary>
		/// Applies the lattice effect mesh to the renderer.
		/// </summary>
		internal void ApplyMesh()
		{
			SetMesh(_mesh);
		}

		/// <summary>
		/// Applies the original target mesh to the renderer.
		/// </summary>
		internal void ResetMesh()
		{
			SetMesh(_targetMesh);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Releases buffers used for lattice deforming.
		/// </summary>
		protected virtual void Release()
		{
			_copyBuffer?.Release();
			_copyBuffer = null;

			_vertexBuffer?.Release();
			_vertexBuffer = null;

			_additionalBuffer?.Release();
			_additionalBuffer = null;

			_mesh = null;
		}

		/// <summary>
		/// Gets the current mesh, either from a mesh filter or skinned mesh renderer.
		/// </summary>
		protected abstract Mesh GetMesh();

		/// <summary>
		/// Sets a mesh, either on a mesh filter or skinned mesh renderer.
		/// </summary>
		protected abstract void SetMesh(Mesh mesh);

		/// <summary>
		/// Queues the modifier to be run this frame.
		/// </summary>
		protected abstract void Enqueue(bool ignoreMode);

		#endregion

		#region Private Methods

		private void EnqueueIfNeeded(bool ignoreMode)
		{
			if (!_ranThisFrame && IsValid)
			{
				Enqueue(ignoreMode);
				_ranThisFrame = true;
			}
		}

		private void Initialise()
		{
			// Try get target mesh if one is not set
			if (_targetMesh == null)
			{
				_targetMesh = GetMesh();
			}

			_currentStretchChannel = _stretchChannel;
			_currentTargetMesh = _targetMesh;
			_currentApplyMethod = _applyMethod;

			// If still no target mesh, log warning and exit early
			if (_targetMesh == null)
			{
				Debug.LogWarning("No target mesh set. Can not initialise lattice modifier.", this);
				return;
			}

			// If not readable, log error and exit early
			if (!_targetMesh.isReadable)
			{
				Debug.LogError("Target does not have read/write enabled. Enable it in the model import settings.", _targetMesh);
				return;
			}

			// Create a copy which the lattice will be applied to
			_mesh = Instantiate(_targetMesh);
			_mesh.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			_mesh.name = _targetMesh.name + " (Lattice)";
			_mesh.vertexBufferTarget |= BufferTargets;

			bool hasNormals = _mesh.HasVertexAttribute(VertexAttribute.Normal);
			bool hasTangents = _mesh.HasVertexAttribute(VertexAttribute.Tangent);

			_actualApplyMethod = _applyMethod;

			if ((_applyMethod > ApplyMethod.PositionOnly) && (!hasNormals || !hasTangents))
			{
				if (_applyMethod == ApplyMethod.PositionNormalTangent)
				{
					Debug.LogWarning("Cannot deform normals and tangents if the mesh does not have normals and tangents.");
				}

				if (_applyMethod == ApplyMethod.Stretch)
				{
					Debug.LogWarning("Cannot apply stretch if the mesh does not have normals and tangents.");
				}

				_actualApplyMethod = ApplyMethod.PositionOnly;
			}

			// Add at least one bone
			// This will ensure it uses the correct vertex buffer when rendering
			if ((_targetMesh.blendShapeCount > 0) && (_mesh.bindposes.Length == 0))
			{
				Matrix4x4[] bindPoses = new Matrix4x4[] { Matrix4x4.identity };

				var bonesPerVertex = new NativeArray<byte>(_mesh.vertexCount, Allocator.Temp);
				var weights = new NativeArray<BoneWeight1>(_mesh.vertexCount, Allocator.Temp);

				for (int i = 0; i < _mesh.vertexCount; i++)
				{
					bonesPerVertex[i] = 1;
					weights[i] = new()
					{
						boneIndex = 0,
						weight = 0.0f
					};
				}

				_mesh.SetBoneWeights(bonesPerVertex, weights);
				_mesh.bindposes = bindPoses;

				bonesPerVertex.Dispose();
				weights.Dispose();
			}

			// Add stretch and squish vertex channel
			if (_actualApplyMethod == ApplyMethod.Stretch)
			{
				// Create stretch array of all ones
				Vector2[] stretch = new Vector2[_mesh.vertexCount];
				Array.Fill(stretch, Vector2.one);

				// Add to mesh
				int stretchChannel = (int)_stretchChannel;
				_mesh.SetUVs(stretchChannel, stretch);
			}

			// Get mesh information
			_meshInfo = new(_mesh);

			// Get vertex buffers
			_vertexBuffer = _mesh.GetVertexBuffer(0);
			if (_meshInfo.HasAdditionalBuffer())
			{
				_additionalBuffer = _mesh.GetVertexBuffer(1);
			}

			// Create copy of vertex buffer
			// Used for resetting to original every frame
			_copyBuffer = new GraphicsBuffer(
				BufferTargets,
				_meshInfo.VertexCount,
				_meshInfo.BufferStride
			);
			Graphics.CopyBuffer(_vertexBuffer, _copyBuffer);
		}

		#endregion

		#region Unity Methods

#if UNITY_EDITOR
		private void Update()
		{
			// If target mesh, apply method, or stretch channel have changed in inspector
			if ((_targetMesh != _currentTargetMesh) ||
				(_applyMethod != _currentApplyMethod) ||
				(_stretchChannel != _currentStretchChannel))
			{
				// Reset component
				OnDisable();
				OnEnable();
			}

			// Ensure mesh on the renderer is the lattice affected one
			if ((_mesh != null) && (_mesh != GetMesh()))
			{
				ApplyMesh();
			}
		}
#endif

		private void LateUpdate()
		{
			EnqueueIfNeeded(false);
			_ranThisFrame = false;
		}

		private void OnEnable()
		{
			// Initialise everything
			Initialise();
			ApplyMesh();
			EnqueueIfNeeded(true);
		}

		private void OnDisable()
		{
			// Deinitialise everything
			ResetMesh();
			Release();
		}
		
		#endregion

		#region Serialization

		[SerializeField, HideInInspector, FormerlySerializedAs("_stretchTarget")]
		private int d_stretchTarget = -1; // Deprecated stretch target field

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			d_stretchTarget = -1;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// Upgrading from a version which used the stretch target field
			if ((d_stretchTarget >= 0) && (d_stretchTarget <= 8))
			{
				_applyMethod = ApplyMethod.Stretch;
				_stretchChannel = (TextureCoordinate)d_stretchTarget;
			}
		}

		#endregion
	}
}
