using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

using static Lattice.LatticeShaderProperties;

namespace Lattice
{
	/// <summary>
	/// High level controller of lattice modifiers. Adds the deformation system
	/// to the player loop and implements the application of lattices.
	/// </summary>
	public static class LatticeFeature
	{
		#region Constants

		/// <summary>
		/// Hardcoded max number of lattice handles supported. Can be changed.
		/// </summary>
		internal const int MaxHandles = 1024;

		/// <summary>
		/// The compute shader file name, relative to a Resources folder.
		/// </summary>
		internal const string ComputeShaderName = "LatticeCompute";

		#endregion

		#region Fields

		private static bool _initialised = false;

		private static ComputeShader _compute;
		private static uint _resetGroupSize;
		private static uint _deformGroupSize;
		private static LatticeShaderProperties _properties;

		private static ComputeBuffer _latticeBuffer;
		private static readonly int[] _latticeResolution = new int[3];

		private static readonly CommandBuffer _cmd = new();

		private static readonly List<LatticeModifierBase> _modifiers = new();
		private static readonly List<SkinnedLatticeModifier> _skinnedModifiers = new();

		#endregion

		#region Public Methods

		/// <summary>
		/// Enqueues a mesh to be deformed this frame.
		/// </summary>
		internal static void Enqueue(LatticeModifierBase modifier)
		{
			_modifiers.Add(modifier);
		}

		/// <summary>
		/// Enqueues a skinned mesh to be deformed this frame.
		/// </summary>
		internal static void EnqueueSkinned(SkinnedLatticeModifier modifier)
		{
			_skinnedModifiers.Add(modifier);
		}

		/// <summary>
		/// Sets up the modifiers as part of the player loop.
		/// </summary>
		[RuntimeInitializeOnLoadMethod]
		internal static void Initialise()
		{
			if (_initialised) return;

			// Load compute shader
			_compute = Resources.Load<ComputeShader>(ComputeShaderName);

			// If couldn't find compute shader, log error and exit early
			if (_compute == null)
			{
				Debug.LogError($"Could not load lattice compute. Make sure it's within a Resources folder and called {ComputeShaderName}");
				return;
			}

			if (!Application.isEditor) 
				Application.quitting += Cleanup;

			// Create the buffer for storing lattice information
			_latticeBuffer = new(MaxHandles, 3 * sizeof(float));

			// Setup properties
			_properties = new(_compute);
			_properties.DisableAllKeywords();

			// Setup compute
			_compute.GetKernelThreadGroupSizes(0, out _deformGroupSize, out uint _, out uint _);
			_compute.GetKernelThreadGroupSizes(1, out _resetGroupSize, out uint _, out uint _);
			_compute.SetBuffer(0, LatticeBufferId, _latticeBuffer);

			// Add to player loop
			AddToPlayerLoop();

			_initialised = true;
		}

		/// <summary>
		/// Performs cleanup of lattice related things.
		/// </summary>
		internal static void Cleanup()
		{
			if (!_initialised) return;

			if (!Application.isEditor) 
				Application.quitting -= Cleanup;

			// Release the lattice buffer
			_latticeBuffer?.Release();
			_latticeBuffer = null;

			// Clear existing modifiers
			_modifiers.Clear();
			_skinnedModifiers.Clear();

			// Remove from player loop
			RemoveFromPlayerLoop();

			_initialised = false;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Applies deformations on all queued lattice modifiers.
		/// </summary>
		private static void ApplyModifiers()
		{
			if (_modifiers.Count == 0 || _latticeBuffer == null) return;

			// These two calls are only here because editing the compute shader will cause
			// the asset to refresh and these values will be lost.
			_compute.GetKernelThreadGroupSizes(0, out _deformGroupSize, out uint _, out uint _);
			_compute.GetKernelThreadGroupSizes(1, out _resetGroupSize, out uint _, out uint _);
			_compute.SetBuffer(0, LatticeBufferId, _latticeBuffer);

			// Setup command buffer
			_cmd.Clear();
			_cmd.name = "Lattice Modifiers";

			// Apply all modifiers
			for (int i = 0; i < _modifiers.Count; i++)
			{
				ApplyModifier(_cmd, _modifiers[i]);
			}

			// Execute
			Graphics.ExecuteCommandBuffer(_cmd);

			// Clear modifier queue
			_modifiers.Clear();
		}

		/// <summary>
		/// Applies lattice deformations to a lattice modifier.
		/// </summary>
		private static void ApplyModifier(CommandBuffer cmd, LatticeModifierBase modifier)
		{
			if (modifier == null || !modifier.IsValid) return;

			// Set modifier keywords
			SetupModifier(cmd, modifier);

			// Set vertex buffers
			cmd.SetComputeBufferParam(_compute, 0, VertexBufferId, modifier.VertexBuffer);

			// Copy original buffer back onto vertex buffer
			cmd.CopyBuffer(modifier.CopyBuffer, modifier.VertexBuffer);

			// Reset stretch
			MeshInfo info = modifier.MeshInfo;
			if (modifier.ApplyMethod == ApplyMethod.Stretch && info.HasAdditionalBuffer())
			{
				cmd.SetComputeBufferParam(_compute, 1, AdditionalBufferId, modifier.AdditionalBuffer);
				cmd.DispatchCompute(_compute, 1, info.VertexCount / (int)_resetGroupSize + 1, 1, 1);
			}

			// Apply lattices
			List<LatticeItem> lattices = modifier.Lattices;
			Matrix4x4 localToWorld = modifier.LocalToWorld;
			int groups = info.VertexCount / (int)_deformGroupSize + 1;

			ApplyLattices(cmd, lattices, localToWorld, info, groups);
		}

		/// <summary>
		/// Applies deformations on all queued skinned lattice modifiers.
		/// </summary>
		private static void ApplySkinnedModifiers()
		{
			if (_skinnedModifiers.Count == 0 || _latticeBuffer == null) return;

			// Setup command buffer
			_cmd.Clear();
			_cmd.name = "Skinned Lattice Modifiers";

			// Apply all modifiers
			for (int i = 0; i < _skinnedModifiers.Count; i++)
			{
				ApplySkinnedModifier(_cmd, _skinnedModifiers[i]);
			}

			// Execute
			Graphics.ExecuteCommandBuffer(_cmd);

			// Clear modifier queue
			_skinnedModifiers.Clear();
		}

		/// <summary>
		/// Applies lattice deformations to a skinned lattice modifier.
		/// </summary>
		private static void ApplySkinnedModifier(CommandBuffer cmd, SkinnedLatticeModifier modifier)
		{
			if (modifier == null || !modifier.IsValid || !modifier.TryGetSkinnedBuffer(out var skinnedBuffer)) return;

			MeshInfo info = modifier.MeshInfo;

			// Set modifier keywords
			SetupModifier(cmd, modifier);

			// Set main vertex buffer
			cmd.SetComputeBufferParam(_compute, 0, VertexBufferId, skinnedBuffer);

			// Apply skinned lattices
			Matrix4x4 localToWorld = modifier.SkinnedLocalToWorld;
			List<LatticeItem> lattices = modifier.SkinnedLattices;
			int groups = info.VertexCount / (int)_deformGroupSize + 1;

			ApplyLattices(cmd, lattices, localToWorld, info, groups);
		}

		/// <summary>
		/// Sets the compute shader params for the lattice modifier.
		/// </summary>
		private static void SetupModifier(CommandBuffer cmd, LatticeModifierBase modifier)
		{
			ApplyMethod applyMethod = modifier.ApplyMethod;
			MeshInfo info = modifier.MeshInfo;

			cmd.SetKeyword(_compute, _properties.NormalsKeyword, applyMethod == ApplyMethod.PositionNormalTangent);
			cmd.SetKeyword(_compute, _properties.StretchKeyword, applyMethod == ApplyMethod.Stretch);

			cmd.SetComputeIntParam(_compute, VertexCountId, info.VertexCount);
			cmd.SetComputeIntParam(_compute, BufferStrideId, info.BufferStride);
			cmd.SetComputeIntParam(_compute, PositionOffsetId, info.PositionOffset);

			if (applyMethod >= ApplyMethod.PositionNormalTangent)
			{
				cmd.SetComputeIntParam(_compute, NormalOffsetId, info.NormalOffset);
				cmd.SetComputeIntParam(_compute, TangentOffsetId, info.TangentOffset);
			}

			if (applyMethod == ApplyMethod.Stretch)
			{
				cmd.SetComputeIntParam(_compute, StretchOffsetId, 
					info.GetTexCoordOffset((int)modifier.StretchChannel)
				);
			}

			cmd.SetKeyword(_compute, _properties.MultipleBuffersKeyword, info.HasAdditionalBuffer());

			if (info.HasAdditionalBuffer())
			{
				cmd.SetComputeIntParam(_compute, AdditionalStrideId, info.AdditionalStride);
				cmd.SetComputeBufferParam(_compute, 0, AdditionalBufferId, modifier.AdditionalBuffer);
			}
		}

		/// <summary>
		/// Applies a list of lattices.
		/// </summary>
		private static void ApplyLattices(CommandBuffer cmd, List<LatticeItem> lattices, Matrix4x4 localToWorld, in MeshInfo info, int groups)
		{
			for (int i = 0; i < lattices.Count; i++)
			{
				LatticeItem latticeItem = lattices[i];
				Lattice lattice = latticeItem.Lattice;

				if (lattice == null || !lattice.isActiveAndEnabled) continue;

				if (lattice.Offsets.Count > MaxHandles)
				{
					Debug.LogError($"Lattice has more than {MaxHandles} handles and will be skipped", lattice);
					continue;
				}

				// Set lattice parameters
				cmd.SetKeyword(_compute, _properties.HighQualityKeyword, latticeItem.HighQuality);
				cmd.SetKeyword(_compute, _properties.ZeroOutsideKeyword, !latticeItem.Global);

				Matrix4x4 objectToLattice = lattice.transform.worldToLocalMatrix * localToWorld;
				Matrix4x4 latticeToObject = objectToLattice.inverse;
				cmd.SetComputeMatrixParam(_compute, ObjectToLatticeId, objectToLattice);
				cmd.SetComputeMatrixParam(_compute, LatticeToObjectId, latticeToObject);

				_latticeResolution[0] = lattice.Resolution.x;
				_latticeResolution[1] = lattice.Resolution.y;
				_latticeResolution[2] = lattice.Resolution.z;
				cmd.SetComputeIntParams(_compute, LatticeResolutionId, _latticeResolution);

				// Set lattice offsets
				cmd.SetBufferData(_latticeBuffer, lattice.Offsets);

				// Apply lattice
				cmd.DispatchCompute(_compute, 0, groups, 1, 1);
			}
		}

		/// <summary>
		/// Updates the player loop to include the modifier systems.
		/// </summary>
		private static void AddToPlayerLoop()
		{
			// Get the player loop
			var loop = PlayerLoop.GetCurrentPlayerLoop();

			// Get the PostLateUpdate system
			int postLateUpdateIndex = Array.FindIndex(loop.subSystemList, system => system.type == typeof(PostLateUpdate));
			var postLateUpdate = loop.subSystemList[postLateUpdateIndex];

			// Get the UpdateAllSkinnedMeshes system index
			var postLateSystems = new List<PlayerLoopSystem>(postLateUpdate.subSystemList);
			var skinned = postLateSystems.FindIndex(system => system.type == typeof(PostLateUpdate.UpdateAllSkinnedMeshes));

			// Insert the static modifier before the skinned mesh system,
			// This allows the skinning system to use the lattice modified meshes
			postLateSystems.Insert(skinned, new()
			{
				updateDelegate = ApplyModifiers,
				type = typeof(LatticeFeature)
			});

			// Insert the skinned modifier after the skinned mesh system
			// This allows the modifier system to use the skin modified meshes
			postLateSystems.Insert(skinned + 2, new()
			{
				updateDelegate = ApplySkinnedModifiers,
				type = typeof(LatticeFeature)
			});

			// Update the systems
			postLateUpdate.subSystemList = postLateSystems.ToArray();
			loop.subSystemList[postLateUpdateIndex] = postLateUpdate;

			// Set updated player loop
			PlayerLoop.SetPlayerLoop(loop);
		}

		/// <summary>
		/// Removes the modifier systems from the player loop.
		/// </summary>
		private static void RemoveFromPlayerLoop()
		{
			// Get the player loop
			var loop = PlayerLoop.GetCurrentPlayerLoop();

			// Get the PostLateUpdate system
			int postLateUpdateIndex = Array.FindIndex(loop.subSystemList, system => system.type == typeof(PostLateUpdate));
			var postLateUpdate = loop.subSystemList[postLateUpdateIndex];

			// Remove all systems related to the lattice feature
			var postLateSystems = new List<PlayerLoopSystem>(postLateUpdate.subSystemList);
			postLateSystems.RemoveAll(system => system.type == typeof(LatticeFeature));

			// Update the systems
			postLateUpdate.subSystemList = postLateSystems.ToArray();
			loop.subSystemList[postLateUpdateIndex] = postLateUpdate;

			// Set updated player loop
			PlayerLoop.SetPlayerLoop(loop);
		}

		#endregion
	}
}