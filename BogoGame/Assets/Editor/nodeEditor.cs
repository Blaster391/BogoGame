using UnityEngine;
using UnityEditor;
using System.Collections;

public class NodeEditor : EditorWindow
{

    GameObject gameMaster;
    GameObject selectedObject;
    GameObject newNodePrefab;
    PrefabStore prefabStore;

    NodeMasterEditor nodeMaster = new NodeMasterEditor();

    enum Direction{
            NORTH = 0,
            SOUTH = 1,
            EAST = 2,
            WEST = 3,
    };

    [MenuItem("Window/Node Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NodeEditor));
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 25), "Create Node"))
        {
            createNode();
        }

        newNodePrefab = EditorGUI.ObjectField(new Rect(3, 125, 200, 20), newNodePrefab, typeof(GameObject), false) as GameObject;
      

        if (Selection.activeGameObject != null && selectedObject.tag == "Node")
        {
            selectedObject = Selection.activeObject as GameObject;
            GUI.Label(new Rect(0, 150, 100, 25), selectedObject.transform.name);

            NodeScript ns = selectedObject.GetComponent<NodeScript>();
            ns.lockedName = GUI.Toggle(new Rect(0, 200, 200, 25), ns.lockedName, "Lock node name");

        }

        if (GUI.Button(new Rect(0, 250, 100, 25), "Debug Table"))
        {
            nodeMaster.printTable();
        }
    }

    void createNode()
    {
        if(newNodePrefab != null)
        {
            Instantiate(newNodePrefab);
        }
    }

    void OnEnable()
    {
        gameMaster = GameObject.Find("GameMaster");
        nodeMaster.generateTable();
        prefabStore = gameMaster.GetComponent<PrefabStore>();
    }

    void Update()
    {
        nodePlace();
    }

    void nodePlace()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.tag == "Node")
        {

            selectedObject = Selection.activeObject as GameObject;
            NodeScript ns = selectedObject.GetComponent<NodeScript>();

            float x, y, z;

            x = Mathf.Round(selectedObject.transform.position.x);
            y = Mathf.Round(selectedObject.transform.position.y);
            z = selectedObject.transform.position.z;

            if (nodeMaster.isSpaceFree((int)x, (int)y))
            {

                selectedObject.transform.position = new Vector3(x, y, z);

                nodeMaster.removeNode(selectedObject);

                clearNodeConnections(ns);

                ns.posX = (int)x;
                ns.posY = (int)y;

                nodeMaster.registerNode(selectedObject);

                if (!ns.lockedName)
                {
                    selectedObject.name = "Node: " + ns.posX + " " + ns.posY;
                }

                
                setConduits(ns);


            }
            else
            {
                selectedObject.transform.position = new Vector3(ns.posX, ns.posY, z);
            }
        }
    }

    void setConduits(NodeScript ns)
    {
        for (int i = 0; i < ns.connection.Length; i++)
        {
            setConduitDirection(ns, (Direction)i);
        }
    }

    void setConduitDirection(NodeScript ns, Direction direct) //TODO only works if you move one node
    {
        int x = ns.posX;
        int y = ns.posY;
        Quaternion rotation;
        Vector3 position;
        string conduitName = "Conduit_" + direct.ToString();

        if(direct == Direction.NORTH)
        {
            y++;
            rotation = Quaternion.Euler(0, 0, 90);
            position = new Vector3(0, 0.5f, 0);
           
        }
        else if(direct == Direction.SOUTH)
        {
            y--;
            if (!nodeMaster.isSpaceFree(x, y))
            {
                setConduitDirection(nodeMaster.getNode(x, y).GetComponent<NodeScript>(), Direction.NORTH);
            }
            return;
        }
        else if (direct == Direction.WEST)
        {
            x--;
            if (!nodeMaster.isSpaceFree(x, y))
            {
                setConduitDirection(nodeMaster.getNode(x, y).GetComponent<NodeScript>(), Direction.EAST);
            }
            return;
        }
        else
        {
            x++;
            rotation = Quaternion.Euler(0, 0, 0);
            position = new Vector3(0.5f, 0, 0);

        }

        clearNodeConnectionDirection(ns, direct);

        if (!nodeMaster.isSpaceFree(x, y))
        {
            GameObject con = Instantiate(prefabStore.emptyConduit, selectedObject.transform.position, rotation) as GameObject;
            con.name = conduitName;
            con.transform.parent = ns.gameObject.transform;
            con.transform.localPosition = position;
            ns.connection[(int)direct] = nodeMaster.getNode(x, y);
            nodeMaster.getNode(x, y).GetComponent<NodeScript>().connection[(int)oppositeDirection(direct)] = selectedObject;
        }
        else
        {
            ns.connection[(int)direct] = null;
        }
    }

    void clearNodeConnectionDirection(NodeScript ns, Direction d)
    {
        int x = ns.posX;
        int y = ns.posY;
        if (d == Direction.SOUTH)
        {
            y--;
            if (!nodeMaster.isSpaceFree(x, y))
            {
                clearNodeConnectionDirection(nodeMaster.getNode(x, y).GetComponent<NodeScript>(), Direction.NORTH);
            }
            return;
        }
        else if (d == Direction.WEST)
        {
            x--;
            if (!nodeMaster.isSpaceFree(x, y))
            {
                clearNodeConnectionDirection(nodeMaster.getNode(x, y).GetComponent<NodeScript>(), Direction.EAST);
            }
            return;
        }

        if (ns.connection[(int)d] != null)
        {
            if ((int)d % 2 == 0) {
                GameObject con = null;
                try
                {
                    con = ns.gameObject.transform.FindChild("Conduit_" + d.ToString()).gameObject; //TODO fix naughty code
                }
                catch { }

                if (con != null)
                {
                    DestroyImmediate(con);
                }
            }
            else
            {

            }


            ns.connection[(int)d].GetComponent<NodeScript>().connection[(int)oppositeDirection(d)] = null;
            ns.connection[(int)d] = null;
        }
    }

    void clearNodeConnections(NodeScript ns)
    {
        for (int i = 0; i < ns.connection.Length; i++)
        {
            clearNodeConnectionDirection(ns, (Direction)i);
        }
    }

    Direction oppositeDirection(Direction d)
    {
        if((int)d % 2 == 0)
        {
            return (Direction)(int)d + 1;
        }
        else
        {
            return (Direction)(int)d - 1;
        }
    }
}
