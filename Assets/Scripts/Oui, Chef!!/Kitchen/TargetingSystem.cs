using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class TargetingSystem {

	private List<Target> targets;

	public TargetingSystem(){
		this.targets = new List<Target>();
	}

	public Target getTarget(Vector3 origin, Vector3 heading){
		if(targets.Count > 0){
			heading = heading.normalized;
			Vector<float> originVector = Vector<float>.Build.DenseOfArray(new float[]{origin.x, origin.y, origin.z});
			Vector<float> headingVector = Vector<float>.Build.DenseOfArray(new float[]{heading.x, heading.y, heading.z});

			//Get positions of all possible targets
			Vector<float>[] vectors = Functions.map((x => x.position), targets.ToArray());

			//Create distance vectors
			vectors = Functions.map((x => x.Subtract(originVector)), vectors);

			//Compare vectors
			float[] vectorSimilarities = Matrix<float>.Build.DenseOfRowVectors(vectors).Multiply(headingVector).ToArray();

			//Return the GameObject with the difference vector most similar to Master Chef's heading
			return targets[Functions.maxIndex(vectorSimilarities)];
		}
		else return null;
	}

	public void addTarget(string targetName, GameObject target, Vector3 position){
		targets.Add(new Target(targetName, target, position));
	}

	public void removeTarget(string target){
		System.Predicate<Target> thisTarget = x => x.targetName == target;
		targets.RemoveAll(thisTarget);
	}
}

public class Target {
	public string targetName;
	public GameObject target;
	public Vector<float> position;

	public Target(string targetName, GameObject target, Vector3 position){
		position = position.normalized;
		this.targetName = targetName;
		this.target = target;
		this.position = Vector<float>.Build.DenseOfArray(new float[]{position.x, position.y, position.z});
	}
}