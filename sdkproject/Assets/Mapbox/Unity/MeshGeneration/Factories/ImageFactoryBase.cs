namespace Mapbox.Unity.MeshGeneration.Factories
{
	using System;
	using Mapbox.Map;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Enums;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.Utilities;

	public enum MapImageType
	{
		BasicMapboxStyle,
		Custom,
		None
	}

	public class ImageFactoryBase : AbstractTileFactory
	{
		internal override void OnInitialized()
		{
			
		}

		internal override void OnRegistered(UnityTile tile)
		{
			
		}

		internal override void OnUnregistered(UnityTile tile)
		{
			
		}
	}
}