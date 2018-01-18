namespace Mapbox.Unity.Map
{
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Map;
	using System.Collections.Generic;
	using Mapbox.Unity.MeshGeneration.Factories;
	using System;
	using Mapbox.Platform;
	using System.Linq;


	/// <summary>
	/// Map Visualizer
	/// Represents a map.Doesn't contain much logic and at the moment, it creates requested tiles and relays them to the factories 
	/// under itself.It has a caching mechanism to reuse tiles and does the tile positioning in unity world.
	/// Later we'll most likely keep track of map features here as well to allow devs to query for features easier 
	/// (i.e.query all buildings x meters around any restaurant etc).
	/// </summary>
	[CreateAssetMenu(menuName = "Mapbox/MapVisualizer/Basic Map Visualizer")]
	public class BasicMapVisualizer : MapVisualizerBase
	{
		[SerializeField]
		[NodeEditorElementAttribute("TerrainFactory")]
		public TerrainFactoryBase TerrainFactory;

		[SerializeField]
		[NodeEditorElementAttribute("ImageFactory")]
		public ImageFactoryBase ImageFactory;

		[SerializeField]
		[NodeEditorElementAttribute("VectorFactory")]
		public VectorTileFactoryBase VectorFactory;


		[SerializeField]
		Texture2D _loadingTexture;

		protected IMapReadable _map;
		protected Dictionary<UnwrappedTileId, UnityTile> _activeTiles = new Dictionary<UnwrappedTileId, UnityTile>();
		protected Queue<UnityTile> _inactiveTiles = new Queue<UnityTile>();
		private int _counter;

		private ModuleState _state;
		public ModuleState State
		{
			get
			{
				return _state;
			}
			internal set
			{
				if (_state != value)
				{
					_state = value;
					MapVisualizerStateChanged(_state);
				}
			}
		}

		public IMapReadable Map { get { return _map; } }
		public Dictionary<UnwrappedTileId, UnityTile> ActiveTiles { get { return _activeTiles; } }

		/// <summary>
		/// Gets the unity tile from unwrapped tile identifier.
		/// </summary>
		/// <returns>The unity tile from unwrapped tile identifier.</returns>
		/// <param name="tileId">Tile identifier.</param>
		public UnityTile GetUnityTileFromUnwrappedTileId(UnwrappedTileId tileId)
		{
			return _activeTiles[tileId];
		}

		private List<AbstractTileFactory> _factories;

		/// <summary>
		/// Initializes the factories by passing the file source down, which is necessary for data (web/file) calls
		/// </summary>
		/// <param name="fileSource"></param>
		public override void Initialize(IMapReadable map, IFileSource fileSource)
		{
			_map = map;

			// Allow for map re-use by recycling any active tiles.
			var activeTiles = _activeTiles.Keys.ToList();
			foreach (var tile in activeTiles)
			{
				DisposeTile(tile);
			}

			State = ModuleState.Initialized;

			if (TerrainFactory != null && TerrainFactory.Active)
			{
				_factories.Add(TerrainFactory);
			}
			if (ImageFactory != null && ImageFactory.Active)
			{
				_factories.Add(ImageFactory);
			}
			if (VectorFactory != null && VectorFactory.Active)
			{
				_factories.Add(VectorFactory);
			}

			foreach (var factory in _factories)
			{
				factory.Initialize(fileSource);
				UnregisterEvents(factory);
				RegisterEvents(factory);
			}
		}

		private void RegisterEvents(AbstractTileFactory factory)
		{
			factory.OnFactoryStateChanged += UpdateState;
			factory.OnTileError += TileError;
		}

		private void UnregisterEvents(AbstractTileFactory factory)
		{
			factory.OnFactoryStateChanged -= UpdateState;
			factory.OnTileError -= TileError;
		}

		public override void Destroy()
		{

			UnregisterEvents(TerrainFactory);
			UnregisterEvents(ImageFactory);
			UnregisterEvents(VectorFactory);

			// Inform all downstream nodes that we no longer need to process these tiles.
			// This scriptable object may be re-used, but it's gameobjects are likely 
			// to be destroyed by a scene change, for example. 
			foreach (var tileId in _activeTiles.Keys.ToList())
			{
				DisposeTile(tileId);
			}

			_activeTiles.Clear();
			_inactiveTiles.Clear();
		}

		void UpdateState(AbstractTileFactory factory)
		{
			if (State != ModuleState.Working && factory.State == ModuleState.Working)
			{
				State = ModuleState.Working;
			}
			else if (State != ModuleState.Finished && factory.State == ModuleState.Finished)
			{
				var allFinished = true;

				foreach (var fact in _factories)
				{
					allFinished &= fact.State == ModuleState.Finished;
				}

				if (allFinished)
				{
					State = ModuleState.Finished;
				}
			}
		}

		/// <summary>
		/// Registers requested tiles to the factories
		/// </summary>
		/// <param name="tileId"></param>
		public override UnityTile LoadTile(UnwrappedTileId tileId)
		{
			UnityTile unityTile = null;

			if (_inactiveTiles.Count > 0)
			{
				unityTile = _inactiveTiles.Dequeue();
			}

			if (unityTile == null)
			{
				unityTile = new GameObject().AddComponent<UnityTile>();
				unityTile.transform.SetParent(_map.Root, false);
			}

			unityTile.Initialize(_map, tileId, _map.WorldRelativeScale, _map.AbsoluteZoom, _loadingTexture);
			PlaceTile(tileId, unityTile, _map);

			// Don't spend resources naming objects, as you shouldn't find objects by name anyway!
#if UNITY_EDITOR
			unityTile.gameObject.name = unityTile.CanonicalTileId.ToString();
#endif

			foreach (var factory in _factories)
			{
				factory.Register(unityTile);
			}		


			ActiveTiles.Add(tileId, unityTile);

			return unityTile;
		}

		public override void DisposeTile(UnwrappedTileId tileId)
		{
			var unityTile = ActiveTiles[tileId];

			unityTile.Recycle();
			ActiveTiles.Remove(tileId);
			_inactiveTiles.Enqueue(unityTile);

			foreach (var factory in _factories)
			{
				factory.Unregister(unityTile);
			}
		}

		/// <summary>
		/// Repositions active tiles instead of recreating them. Useful for panning the map
		/// </summary>
		/// <param name="tileId"></param>
		public override void RepositionTile(UnwrappedTileId tileId)
		{
			UnityTile currentTile;
			if (ActiveTiles.TryGetValue(tileId, out currentTile))
			{
				PlaceTile(tileId, currentTile, _map);
			}
		}
		
		protected virtual void PlaceTile(UnwrappedTileId tileId, UnityTile tile, IMapReadable map)
		{
			var rect = tile.Rect;

			// TODO: this is constant for all tiles--cache.
			var scale = tile.TileScale;

			var position = new Vector3(
				(float)(rect.Center.x - map.CenterMercator.x) * scale,
				0,
				(float)(rect.Center.y - map.CenterMercator.y) * scale);
			tile.transform.localPosition = position;
		}
	}
}