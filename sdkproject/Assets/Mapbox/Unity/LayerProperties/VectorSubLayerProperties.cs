namespace Mapbox.Unity.Map
{
	using System;
	using System.Collections.Generic;
	using Mapbox.Unity.MeshGeneration.Modifiers;
	using Mapbox.Unity.Utilities;
	using System.Linq;
	using System.Text;
	using UnityEngine;

	[Serializable]
	public class VectorSubLayerProperties : LayerProperties
	{
		public readonly string subLayerId = GenerateUniqueIds();
		public CoreVectorLayerProperties coreOptions = new CoreVectorLayerProperties();
		public VectorFilterOptions filterOptions = new VectorFilterOptions();
		public GeometryExtrusionOptions extrusionOptions = new GeometryExtrusionOptions
		{
			extrusionType = ExtrusionType.None,
			propertyName = "height",
			extrusionGeometryType = ExtrusionGeometryType.RoofAndSide,

		};
		public GeometryMaterialOptions materialOptions = new GeometryMaterialOptions();

		public bool buildingsWithUniqueIds = false;
		public PositionTargetType moveFeaturePositionTo;
		[NodeEditorElement("Mesh Modifiers")]
		public List<MeshModifier> MeshModifiers;
		[NodeEditorElement("Game Object Modifiers")]
		public List<GameObjectModifier> GoModifiers;

		public static string GenerateUniqueIds()
		{
			StringBuilder builder = new StringBuilder();
			Enumerable
				.Range(65,26)
				.Select(e => ((char) e).ToString())
				.Concat((Enumerable.Range(97,26).Select(e => ((char)e).ToString())))
				.Concat(Enumerable.Range(0,10).Select(e => e.ToString()))
				.OrderBy(e=> Guid.NewGuid())
				.Take(11)
				.ToList().ForEach(e=> builder.Append(e));

			Debug.Log(builder.ToString());
			return builder.ToString();
		}
	}
}
