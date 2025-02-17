using UnityEngine;
using UnityEngine.Rendering;

namespace Lattice
{
	/// <summary>
	/// All lattice shader keywords and IDs.
	/// </summary>
	public class LatticeShaderProperties
	{
		public static readonly int VertexBufferId = Shader.PropertyToID("VertexBuffer");
		public static readonly int VertexCountId  = Shader.PropertyToID("VertexCount");
		public static readonly int BufferStrideId = Shader.PropertyToID("BufferStride");

		public static readonly int PositionOffsetId = Shader.PropertyToID("PositionOffset");
		public static readonly int NormalOffsetId   = Shader.PropertyToID("NormalOffset");
		public static readonly int TangentOffsetId  = Shader.PropertyToID("TangentOffset");

		public static readonly int AdditionalBufferId = Shader.PropertyToID("AdditionalBuffer");
		public static readonly int AdditionalStrideId = Shader.PropertyToID("AdditionalStride");
		public static readonly int StretchOffsetId    = Shader.PropertyToID("StretchOffset");

		public static readonly int LatticeBufferId     = Shader.PropertyToID("LatticeBuffer");
		public static readonly int ObjectToLatticeId   = Shader.PropertyToID("ObjectToLattice");
		public static readonly int LatticeToObjectId   = Shader.PropertyToID("LatticeToObject");
		public static readonly int LatticeResolutionId = Shader.PropertyToID("LatticeResolution");

		public readonly LocalKeyword HighQualityKeyword;
		public readonly LocalKeyword ZeroOutsideKeyword;
		public readonly LocalKeyword NormalsKeyword;
		public readonly LocalKeyword StretchKeyword;
		public readonly LocalKeyword MultipleBuffersKeyword;

		private readonly ComputeShader _shader;

		public LatticeShaderProperties(ComputeShader shader)
		{
			HighQualityKeyword     = new LocalKeyword(shader, "LATTICE_HIGH_QUALITY");
			ZeroOutsideKeyword     = new LocalKeyword(shader, "LATTICE_ZERO_OUTSIDE");
			NormalsKeyword         = new LocalKeyword(shader, "LATTICE_NORMALS");
			StretchKeyword         = new LocalKeyword(shader, "LATTICE_STRETCH");
			MultipleBuffersKeyword = new LocalKeyword(shader, "LATTICE_MULTIPLE_BUFFERS");

			_shader = shader;
		}

		public void DisableAllKeywords()
		{
			_shader.DisableKeyword(HighQualityKeyword);
			_shader.DisableKeyword(ZeroOutsideKeyword);
			_shader.DisableKeyword(NormalsKeyword);
			_shader.DisableKeyword(StretchKeyword);
			_shader.DisableKeyword(MultipleBuffersKeyword);
		}
	}
}