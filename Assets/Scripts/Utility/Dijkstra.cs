using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra {

	public static Node[] dijkstra(Node[] nodes, int start, int end){
		//nodes = Functions.indexMap(((x, i) => x.setId(i)), nodes);
		//Node[] nodeArray = new Node[nodes.Length];
		// foreach(Node node in nodes){
		// 	nodeArray[node.id] = node;
		// }
		bool[] visited = Functions.initArray(nodes.Length, false);
		NodeDistance[] distances = Functions.map((x => new NodeDistance(x.id)), nodes);
		distances[start].distance = 0.0f;

		while(!Functions.all(visited)){
			//distances.Sort(new NodeDistanceComparer());
			int nextUnvisited = Functions.find(distances, (x => !visited[x.node]));
			for(int i = 0; i < distances.Length; i++){
				if(!visited[i]){
					if(distances[i].distance < distances[nextUnvisited].distance) nextUnvisited = i;
				}
			}
			Vector3 position = nodes[nextUnvisited].position;
			foreach(Node node in nodes[nextUnvisited].neighbours){
				float distance = Mathf.Abs((node.position - position).magnitude);
				if(distances[nextUnvisited].distance + distance < distances[node.id].distance){
					distances[node.id].distance = distances[nextUnvisited].distance + distance;
					distances[node.id].previous = nextUnvisited;
				}
			}
			visited[nextUnvisited] = true;
		}

		//Debug.Log(Functions.print(Functions.map((x => x.print()), distances)));
		return Functions.subset(nodes, createPath(new List<NodeDistance>(distances), end));
	}

// Assumes that index 0 is the start
	private static int[] createPath(List<NodeDistance> nodeDistances, int endNode){
		List<int> path = new List<int>();
		int target = endNode;

		nodeDistances.Sort(new NodeDistanceComparer());

		for(int i = nodeDistances.Count - 1; i >= 0; i--){
			if(nodeDistances[i].node == target){
				path.Add(target);
				target = nodeDistances[i].previous;
			}
		}

		path.Reverse();
		return path.ToArray();
	}
}

public class Node{

	public int id;
	public Vector3 position;
	public Node[] neighbours;

	public Node(Vector3 position, int id = -1, Node[] neighbours = null){
		this.id = id;
		this.position = position;
		this.neighbours = neighbours;
	}

	public Node setId(int id){
		this.id = id;
		return this;
	}

	public void setNeighbours(Node[] neighbours){
		this.neighbours = neighbours;
	}
}

public class NodeComparer : IComparer<Node>{

	int IComparer<Node>.Compare(Node x, Node y){
		if(x.id == y.id){
			return 0;
		}
		else{
			return (x.id < y.id) ? -1 : 1;
		}
	}
}


class NodeDistance : IPrintable {

	public int node;
	public int previous;
	public float distance;

	public NodeDistance(int node, int previous = -1, float distance = float.MaxValue){
		this.node = node;
		this.previous = previous;
		this.distance = distance;
	}

	public string print(){
		return "Node: " + node.ToString() + ", Previous: " + previous.ToString() + ", Distance: " + distance.ToString();
	}
}

public class NodeDistanceComparer : IComparer<NodeDistance>{

	int IComparer<NodeDistance>.Compare(NodeDistance x, NodeDistance y){
		if(x.distance == y.distance){
			return 0;
		}
		else{
			return (x.distance < y.distance) ? -1 : 1;
		}
	}
}
