using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ANCHOR_TYPE {
	RIGHT_UP = 0,
	RIGHT_DOWN,
	LEFT_UP,
	LEFT_DOWN
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileMap : MonoBehaviour {
	public ANCHOR_TYPE _anchor_type = ANCHOR_TYPE.LEFT_DOWN;

	public int _sprite_size = 100;
	public float _tile_scale = 0.1f;

	public int _tile_count_x = 1;
	public int _tile_count_y = 1;

	public Texture2D _terrainTiles;
	public List<int> _ids = new List<int>();

		ANCHOR_TYPE __anchor_type = ANCHOR_TYPE.LEFT_DOWN;

		int __sprite_size = 0;
		float __tile_scale = 0;

		int __tile_count_x = 0;
		int __tile_count_y = 0;

		Texture2D __terrainTiles;
		int [] __ids = null;

	private Rect[] _tiles;
	private Mesh _mesh;

	private MeshFilter _mesh_filter;

	// Use this for initialization
	void Start () {
		_mesh_filter = GetComponent<MeshFilter>();

		LateUpdate();
	}

	public bool isEqual() {
		if( _anchor_type != __anchor_type ) {
			return false;
		}

		if( _tile_scale != __tile_scale ) {
			return false;
		}

		if( _terrainTiles != __terrainTiles ) {
			return false;
		}

		int [] ids1_arr = { _sprite_size, _tile_count_x, _tile_count_y };
		List<int> ids1_list = new List<int>( ids1_arr );
		if( (_ids != null) && (_ids.Count > 0) ) {
			ids1_list.AddRange( _ids );
		}

		int [] ids2_arr = { __sprite_size, __tile_count_x, __tile_count_y };
		List<int> ids2_list = new List<int>( ids2_arr );
		if( (__ids != null) && (__ids.Length > 0) ) {
			ids2_list.AddRange( __ids );
		}

		if( ids1_list.Count != ids2_list.Count ) {
			return false;
		}

		for( int i=0; i<ids1_list.Count; i++ ) {
			if( ids1_list[i] != ids2_list[i] ) {
				return false;
			}
		}

		return true;
	}

	public void SetEqual() {
		__anchor_type = _anchor_type;

		__sprite_size = _sprite_size;
		__tile_scale = _tile_scale;

		__tile_count_x = _tile_count_x;
		__tile_count_y = _tile_count_y;

		__terrainTiles = _terrainTiles;

		__ids = _ids.ToArray();
	}

	private void LateUpdate() {
		if( isEqual() == false ) {
			SetEqual();

			BuildMesh();
		}
	}

	private void OnDestroy() {
		_tiles = null;

		if( _mesh != null ) {
			_mesh.Clear();
			_mesh = null;
		}

		_mesh_filter.sharedMesh = null;
	}

	int GetIndex( int x, int y ) {
		return ((_tile_count_y - 1 - y) * _tile_count_x + x);
	}

	int GetIndexToID( int index ) {
		if( (index >= 0) && (index < _ids.Count) ) {
			return _ids[ index ];
		}
		return -1;
	}

	bool GetTile( int id, out Rect tile ) {
		tile = new Rect();
		if( (id >= 0) && (id < _tiles.Length) ) {
			tile = _tiles[ id ];
			return true;
		}
		return false;
	}

	Rect [] GetTiles() {
		int count_x = (_terrainTiles.width + _sprite_size - 1) / _sprite_size;
		int count_y = (_terrainTiles.height + _sprite_size - 1) / _sprite_size;

		float x_offset = (float) _sprite_size / _terrainTiles.width;
		float y_offset = (float) _sprite_size / _terrainTiles.height;

		Rect [] tiles = new Rect [ count_x * count_y ];

		int index;
		for( int y=0; y<count_y; y++ ) {
			for( int x=0; x<count_x; x++ ) {
				index = (count_y - 1 - y) * count_x + x;
				tiles[ index ] = new Rect( x_offset * x, y_offset * y, x_offset, y_offset );
			}
		}

		return tiles;
	}

	public void SetTile( int x, int y, int id_new ) {
		int index = GetIndex( x, y );

		int id = GetIndexToID( index );
		if( id == id_new ) {
			return;
		}

		int tileIndex = 4 * index;
		Vector2 [] uv = _mesh_filter.sharedMesh.uv;

		Rect tile;
		if( GetTile( id_new, out tile ) == true ) {
			uv[ tileIndex + 0 ] = new Vector2( tile.x, tile.y );
			uv[ tileIndex + 1 ] = new Vector2( tile.x + tile.width, tile.y );
			uv[ tileIndex + 2 ] = new Vector2( tile.x + tile.width, tile.y + tile.height );
			uv[ tileIndex + 3 ] = new Vector2( tile.x, tile.y + tile.height );
		} else {
			uv[ tileIndex + 0 ] = Vector2.zero;
			uv[ tileIndex + 1 ] = Vector2.zero;
			uv[ tileIndex + 2 ] = Vector2.zero;
			uv[ tileIndex + 3 ] = Vector2.zero;
		}

		_mesh_filter.sharedMesh.uv = uv;
	}

	public void BuildMesh() {
		OnDestroy();

		if( (_terrainTiles == null) ||
			(_sprite_size == 0) || (_tile_scale == 0) ||
			(_tile_count_x == 0) || (_tile_count_y == 0) ) {
			return;
		}

		int numTiles = _tile_count_x * _tile_count_y;
		int numTriangles = numTiles * 6;
		int numVertices = numTiles * 4;

		_tiles = GetTiles();

		// Generate the mesh data
		Vector3 [] vertices = new Vector3[ numVertices ];
		Vector3 [] normals = new Vector3[ numVertices ];
		Vector2 [] uv = new Vector2[ numVertices ];
		int [] triangles = new int [ numTriangles ];

		float offset = _sprite_size * _tile_scale;
		
		for( int y=0; y<_tile_count_y; y++ ) {
			for( int x=0; x<_tile_count_x; x++ ) {
				float _x = (float)x * offset;
				float _y = (float)y * offset;

				int index = GetIndex( x, y );
				int id = GetIndexToID( index );

				Rect tile;
				if( GetTile( id, out tile ) == false ) {
					continue;
				}

				int tileIndex = 4 * index;

				vertices[ tileIndex + 0 ] = new Vector3( _x, _y );
				vertices[ tileIndex + 1 ] = new Vector3( _x + offset, _y );
				vertices[ tileIndex + 2 ] = new Vector3( _x + offset, _y + offset );
				vertices[ tileIndex + 3 ] = new Vector3( _x, _y + offset );

				uv[ tileIndex + 0 ] = new Vector2( tile.x, tile.y );
				uv[ tileIndex + 1 ] = new Vector2( tile.x + tile.width, tile.y );
				uv[ tileIndex + 2 ] = new Vector2( tile.x + tile.width, tile.y + tile.height );
				uv[ tileIndex + 3 ] = new Vector2( tile.x, tile.y + tile.height );

				normals[ tileIndex + 0 ] = new Vector3( 0, 0, -1 );
				normals[ tileIndex + 1 ] = new Vector3( 0, 0, -1 );
				normals[ tileIndex + 2 ] = new Vector3( 0, 0, -1 );
				normals[ tileIndex + 3 ] = new Vector3( 0, 0, -1 );

				int triangleIndex = 6 * index;

				triangles[triangleIndex + 0] = tileIndex;
				triangles[triangleIndex + 1] = tileIndex + 2;
				triangles[triangleIndex + 2] = tileIndex + 1;

				triangles[triangleIndex + 3] = tileIndex;
				triangles[triangleIndex + 4] = tileIndex + 3;
				triangles[triangleIndex + 5] = tileIndex + 2;

			}
		}

		// Create a new Mesh and populate with the data
		if( _mesh == null ) {
			_mesh = new Mesh();
		}

		_mesh.name = "tile_map";
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.uv = uv;
		_mesh.triangles = triangles;

		_mesh_filter.sharedMesh = _mesh;
		
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		if( (mesh_renderer.sharedMaterials != null) && (mesh_renderer.sharedMaterials.Length > 0) && (mesh_renderer.sharedMaterials[0] != null) ) {
			mesh_renderer.sharedMaterials[0].mainTexture = _terrainTiles;
		}

		Debug.Log( "Done Mesh!" );
	}
}
