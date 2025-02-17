using System;
using UnityEngine;

namespace Lattice
{
	/// <summary>
	/// Used to reference Lattices with per modifier settings.
	/// </summary>
	[Serializable]
	public struct LatticeItem
	{
		private const string LatticeTooltip = "Lattice to apply.";

		private const string HighQualityTooltip =
			"Whether to use high quality deformation. " +
			"If enabled, will use tricubic interpolation, otherwise trilinear interpolation.";

		private const string GlobalTooltip =
			"When disabled, deformation only occurs within or close to the lattice.\n" +
			"When enabled, deformation can occur outside the lattice to match the outer handles.";

		[Tooltip(LatticeTooltip)]
		public Lattice Lattice;

		[Tooltip(HighQualityTooltip)]
		public bool HighQuality;

		[Tooltip(GlobalTooltip)]
		public bool Global;
	}
}
