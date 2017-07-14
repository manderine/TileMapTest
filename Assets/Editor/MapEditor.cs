using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor 
{
	GameObject[] prefabs;
	GameObject selectedPrefab;
	List<GameObject> spawnedGO = new List<GameObject>();

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		//Load all prefabs as objects from the 'Prefabs' folder
		Object[] obj =  Resources.LoadAll ("Prefabs",typeof(GameObject));

		//initialize the game object array
		prefabs = new GameObject[obj.Length];

		//store the game objects in the array
		for(int i=0;i<obj.Length;i++)
		{
			prefabs[i]=(GameObject)obj[i];
		}

		GUILayout.BeginHorizontal ();

        if(prefabs!=null)
        {
            int elementsInThisRow=0;
            for( int i=0; i<prefabs.Length; i++)
            {
                elementsInThisRow++;

                //get the texture from the prefabs
                Texture prefabTexture = prefabs[i].GetComponent<SpriteRenderer> ().sprite.texture;

                //create one button for earch prefabs 
                //if a button is clicked, select that prefab and focus on the scene view
                if(GUILayout.Button(prefabTexture,GUILayout.MaxWidth(50), GUILayout.MaxHeight(50)))
                {    
                    selectedPrefab = prefabs[i];
                    EditorWindow.FocusWindowIfItsOpen<SceneView>();

                }

                //move to next row after creating a certain number of buttons so it doesn't overflow horizontally
                if(elementsInThisRow>Screen.width/70)
                {
                    elementsInThisRow=0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal ();        
                }
            }
        }
        GUILayout.EndHorizontal();

	}

	void OnSceneGUI()
	{
		Handles.BeginGUI();
		GUILayout.Box("Map Edit Mode");
		if(selectedPrefab==null)
		{
			GUILayout.Box("No prefab selected!");
 		}
		Handles.EndGUI();     

		Vector3	spawnPosition = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).origin;

	    //if 'E' pressed, spawn the selected prefab
        if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.H) 
        {
            
            TileMap tm = FindObjectOfType<TileMap>();
			if( tm != null ) {
				tm.SetTile( 0, 0, 1 );
			}
        }

	    //if 'E' pressed, spawn the selected prefab
        if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.E) 
        {
            
            Spawn(spawnPosition);
        }


		//if 'X' is pressed, undo (remove the last spawned prefab)
		if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.X) 
		{	
			if(spawnedGO.Count>0)
			{
				DestroyImmediate(spawnedGO[spawnedGO.Count-1]);
				spawnedGO.RemoveAt(spawnedGO.Count-1);
			}
		}

		if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.R) 
    	{
    		
    		Vector2 mouseWorldPosition = new Vector2(spawnPosition.x, spawnPosition.y);
			RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
 
			if(hitInfo.collider != null)
			{
			    selectedGameObject = hitInfo.collider.gameObject;
			}
         }

		if(selectedGameObject!=null)
        {
            Handles.Label(selectedGameObject.transform.position, "X");

        }


		//used to indicate the exact point where the prefab will be instantiated
		Handles.CircleCap (0, spawnPosition, Quaternion.Euler(0,0,0), .05f);

		if(selectedPrefab!=null)
		{
			if(selectedGameObject!=null)
			{
				float selectedGameObjectWidth = selectedGameObject.GetComponent<SpriteRenderer>().bounds.size.x;
				float selectedGameObjectHeight = selectedGameObject.GetComponent<SpriteRenderer>().bounds.size.y;

				float selectedPrefabWidth = selectedPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
				float selectedPrefabHeight = selectedPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

				if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.W) 
				{
					spawnPosition = new Vector3(selectedGameObject.transform.position.x, selectedGameObject.transform.position.y+(selectedGameObjectHeight/2)+(selectedPrefabHeight/2), 0);
					Spawn(spawnPosition);
				}
				if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.S) 
				{
					spawnPosition = new Vector3(selectedGameObject.transform.position.x, selectedGameObject.transform.position.y-(selectedGameObjectHeight/2)-(selectedPrefabHeight/2), 0);
					Spawn(spawnPosition);
				}
				if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.A) 
				{
					spawnPosition = new Vector3(selectedGameObject.transform.position.x-(selectedGameObjectWidth/2)-(selectedPrefabWidth/2), selectedGameObject.transform.position.y, 0);
					Spawn(spawnPosition);
				}
				if (Event.current.type==EventType.KeyDown &&Event.current.keyCode == KeyCode.D) 
				{
					spawnPosition = new Vector3(selectedGameObject.transform.position.x+(selectedGameObjectWidth/2)+(selectedPrefabWidth/2), selectedGameObject.transform.position.y, 0);
					Spawn(spawnPosition);
				}
	
			}
		}

		SceneView.RepaintAll();
	}

	GameObject selectedGameObject;
	void Spawn(Vector2 _spawnPosition)
    {
        GameObject go = (GameObject)Instantiate(selectedPrefab,new Vector3(_spawnPosition.x, _spawnPosition.y, 0), selectedPrefab.transform.rotation);
        selectedGameObject = go;
        go.name = selectedPrefab.name;
        spawnedGO.Add(go);
    }

}
