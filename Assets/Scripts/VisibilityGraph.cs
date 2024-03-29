﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisibilityGraph {
    public List<Vector2> Vertices = new List<Vector2>();
    public Dictionary<Vector2, List<Vector2>> AdjList
        = new Dictionary<Vector2, List<Vector2>>();
	
	private Vector2 Source;
	private Vector2 Target;

    public void SetSource(Vector2 v) {
        Source = v;
        AddVertex(v);
    }

    public void SetTarget(Vector2 v) {
        Target = v;
        AddVertex(v);
    }
	
	public void AddVertex(Vector2 v) {  
        if (!Vertices.Contains(v)) {
            Vertices.Add(v);
            AdjList.Add(v, new List<Vector2>());
        }
    }

    public void RemoveVertex(Vector2 v) {  
        Vertices.Remove(v);
        AdjList.Remove(v);
    }

    public List<Vector2> ClonedVertices() {
        return new List<Vector2>(Vertices);
    }

	public void PrintVertices() {
        foreach (Vector2 v in Vertices) {
            Debug.Log(v);
        }
    }

	public void AddEdge(Vector2 v1, Vector2 v2) {
		bool v1Exists = false;
        bool v2Exists = false;
        foreach (Vector2 v in Vertices) {
            if (v1 == v) {
                v1Exists = true;
            }
            if (v2 == v) {
                v2Exists = true;
            }
        }

        if (v1Exists && v2Exists && !AdjList[v1].Contains(v2)) {
            AdjList[v1].Add(v2);
            AdjList[v2].Add(v1);
        }
	}

    public Dictionary<Vector2, List<Vector2>> ClonedAdjList() {
        var dict = new Dictionary<Vector2, List<Vector2>>();
        foreach (KeyValuePair<Vector2, List<Vector2>> pair in AdjList) {
            dict[pair.Key] = new List<Vector2>(pair.Value);
        }

        return dict;
    }

    public void PrintEdges() {
        int totalEdges = 0;

        foreach (Vector2 v in Vertices) {
            totalEdges += AdjList[v].Count;

            Debug.Log(AdjList[v].Count);
            foreach(Vector2 adj in AdjList[v]) {
                Debug.Log(adj);
            }
        }

        Debug.Log("Total Edges: " + totalEdges);
    }

    public void PrintNumEdges() {
        int totalEdges = 0;

        foreach (Vector2 v in Vertices) {
            totalEdges += AdjList[v].Count;
        }

        Debug.Log("Total Edges: " + totalEdges);
    }
	
	public List<Vector2> AStar() {
		var Q = new Dictionary<Vector2, Tuple<float, Vector2, bool, float>>();
		// (float, Vector2, bool) = (weight, prev vertex, isVisited, heuristic)
		Q.Add(Source, Tuple.Create(
            0f, Source, false, Vector2.Distance(Source, Target)
        )); // weights are based off of euclidean distance between the two nodes
		var archivedQ = new Dictionary<
            Vector2, Tuple<float, Vector2, bool, float
        >>(); // for backtracking
		
		while(Q.Count != 0) {
			Vector2 u = Min(Q);
			foreach (Vector2 v in AdjList[u]) { // v is neighbor
				if (!(archivedQ.ContainsKey(v) && archivedQ[v].Item3)) {
					float w = Q[u].Item1 + Vector2.Distance(v, u);

					if (Q.ContainsKey(v)) {
						Vector2 prev = Q[v].Item2;
						if (w < Q[v].Item1) {
							prev = u;
						} else {
							w = Q[v].Item1;
						}


						Q[v] = Tuple.Create(w, prev, Q[v].Item3, Q[v].Item4);
					} else {
						Q.Add(v, Tuple.Create(
                            w, u, false, Vector2.Distance(v, Target)
                        )); // heuristic is based off of euclidean distance between this node and target
					}
				}
			}
			
			Q[u] = Tuple.Create(Q[u].Item1, Q[u].Item2, true, Q[u].Item4);
			archivedQ.Add(u, Q[u]);
			Q.Remove(u);
			
			if (Q.ContainsKey(Target) && Min(Q) == Target) {
				// Debug.Log("Arrived at Destination!");
				break;
			}
		}
		
		foreach (
            KeyValuePair<Vector2, Tuple<float, Vector2, bool, float>> pair in Q
        ) {
			archivedQ.Add(pair.Key, pair.Value);
		}
		
        // Backtrack to the get the path
		Vector2 prevPathVertex = archivedQ[Target].Item2;
        List<Vector2> path = new List<Vector2>();
        path.Add(Target);
        path.Add(prevPathVertex);
		while (prevPathVertex != Source) {
			// Debug.Log(prevPathVertex);
			prevPathVertex = archivedQ[prevPathVertex].Item2;

            path.Add(prevPathVertex);
		}

        path.Reverse();
        return path;
	}
	
	private Vector2 Min(
        Dictionary<Vector2, Tuple<float, Vector2, bool, float>> Q
    ) {
		Vector2 u = new Vector2(0, 0);
		float weightAndHeuristic = float.MaxValue;
		foreach (
            KeyValuePair<Vector2, Tuple<float, Vector2, bool, float>> pair in Q
        ) {
            // this is what makes it A*, how we obtain the minimum in "priority queue"
			if (pair.Value.Item1 + pair.Value.Item4 < weightAndHeuristic) {
				weightAndHeuristic = pair.Value.Item1 + pair.Value.Item4;
				u = pair.Key;
			}
		}
		
		return u;
	}
}