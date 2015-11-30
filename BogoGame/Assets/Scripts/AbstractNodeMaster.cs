using UnityEngine;
using System.IO;
using System.Collections;

public abstract class AbstractNodeMaster {

    Hashtable nodeTable = new Hashtable();

    public bool isInTable(GameObject node)
    {
         return nodeTable.Contains(node);
    }

    public bool isSpaceFree(int x, int y)
    {
        return !nodeTable.ContainsKey(x + " " + y);
    }

    public GameObject getNode(int x, int y)
    {
        return (GameObject)nodeTable[x + " " + y];
    }

    public void registerNode(GameObject node)
    {
        if (node.tag == "Node")
        {
            if (!nodeTable.Contains(node.GetComponent<NodeScript>().posX + " " + node.GetComponent<NodeScript>().posY)){
                NodeScript ns = node.GetComponent<NodeScript>(); //TODO look up fail states
                nodeTable.Add(ns.posX + " " + ns.posY, node);
              //  Debug.Log("Registered: " + ns.posX + " " + ns.posY);
            }
        }
        else
        {
            Debug.LogError("Tried to register something that wasn't a node");
        }
    }

    public void removeNode(GameObject node)
    {
        if (node.tag == "Node")
        {
            if (nodeTable.Contains(node.GetComponent<NodeScript>().posX + " " + node.GetComponent<NodeScript>().posY)){
                NodeScript ns = node.GetComponent<NodeScript>(); //TODO look up fail states
                nodeTable.Remove(ns.posX + " " + ns.posY);
               // Debug.Log("Removed: " + ns.posX + " " + ns.posY);
            }
            else
            {
                Debug.Log("Tried to remove Node not in table");
            }
        }
        else
        {
            Debug.LogError("Tried to remove something that wasn't a node");
        }
    }

    public void generateTable()
    {
        Hashtable nodeTable = new Hashtable();
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach(GameObject n in nodes)
        {
            registerNode(n);
        }
        Debug.Log("Created Table");
    }

    public void printTable()
    {
        string keys = "Keys: \n";
        string values = "Values: \n";

        foreach (string key in nodeTable.Keys)
        {
            keys = keys + key + "\n";
            GameObject obj = (GameObject)nodeTable[key];
            values = values + obj.name + "\n";
        }

        Debug.Log(keys);
        Debug.Log(values);
    }
}
