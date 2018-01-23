using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour, IPrintable {

	[SerializeField] private int id;
	[SerializeField] private PathNode[] neighbours;

	private Node node;

	public void createNode(){
		node = new Node(gameObject.transform.position, id);
	}

	public void addNeighbours(){
		node.setNeighbours(Functions.map((x => x.GetNode()), neighbours));
	}
	public Node GetNode(){
		return node;
	}

	public int getId(){
		return id;
	}

	public string print(){
		return "PathNode ~ id: " + id.ToString() + Functions.print(Functions.map((x => x.id), node.neighbours));
	}
}
