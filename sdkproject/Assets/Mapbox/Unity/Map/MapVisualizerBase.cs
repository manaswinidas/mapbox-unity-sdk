namespace Mapbox.Unity.Map
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.MeshGeneration.Data;
	using System;
	using Mapbox.Platform;
	using UnityEngine.Serialization;

	/// <summary>
	/// Map Visualizer
	/// Represents a map.Doesn’t contain much logic and at the moment, it creates requested tiles and relays them to the factories 
	/// under itself.It has a caching mechanism to reuse tiles and does the tile positioning in unity world.
	/// Later we’ll most likely keep track of map features here as well to allow devs to query for features easier 
	/// (i.e.query all buildings x meters around any restaurant etc).
	/// </summary>
	public class MapVisualizerBase : ScriptableObject
	{
		public event Action<ModuleState> OnMapVisualizerStateChanged = delegate { };
		/// <summary>
		/// The  <c>OnTileError</c> event triggers when there's a <c>Tile</c> error.
		/// Returns a <see cref="T:Mapbox.Map.TileErrorEventArgs"/> instance as a parameter, for the tile on which error occurred.
		/// </summary>
		public event EventHandler<TileErrorEventArgs> OnTileError;

		public virtual void Initialize(IMapReadable map, IFileSource fileSource)
		{
		}

		public virtual void Destroy()
		{
			
		}

		public virtual UnityTile LoadTile(UnwrappedTileId tileId)
		{
			return null;
		}

		public virtual void DisposeTile(UnwrappedTileId tileId)
		{
			
		}

		public virtual void RepositionTile(UnwrappedTileId tileId)
		{
			
		}

		public void TileError(object s, TileErrorEventArgs e)
		{
			EventHandler<TileErrorEventArgs> handler = OnTileError;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public void MapVisualizerStateChanged(ModuleState s)
		{
			OnMapVisualizerStateChanged(s);
		}
	}
}