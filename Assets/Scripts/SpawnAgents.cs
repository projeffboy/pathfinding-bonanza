﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour {
    public Settings settings;
    public GameObject AgentPrefab;
    public GameObject TargetPrefab;
    public int NumberOfAgents = 2;
    public GameObject Obstacles;
    public PlanningStats StatsScript;

    private GameObject AgentContainer;
    private GameObject TargetContainer;

    private float waitStart = 0.25f;

    void Start() {
        Invoke("Initialize", waitStart);
    }

    private void Initialize() {
        List<GameObject> agents = new List<GameObject>();

        Color[] colors = {
            Color.black,
            Color.blue,
            Color.cyan,
            Color.grey,
            Color.green,
            Color.magenta,
            Color.red,
            Color.yellow
        };

        if (!settings.IsRandom) {
            return;
        }

        AgentContainer = new GameObject("Agent Container");
        TargetContainer = new GameObject("Target Container");

        for (int i = 0; i < NumberOfAgents; i++) {
            agents.Add(Spawn(true, agents, colors[i % colors.Length]));
        }
    }

    public GameObject Spawn(
        bool isSource, List<GameObject> agents, Color color
    ) {
        Vector2[] spots = {
            new Vector2(-2.94f, 0.66f),
            new Vector2(0.01f, 0.66f),
            new Vector2(2.22f, 0.66f),
            new Vector2(-1.96f, -1.68f)
        };
        
        float randX;
        float randY;
        Vector2 rand = Vector2.up;
        GameObject agent;
        if (isSource) {
            agent = Instantiate(AgentPrefab, AgentContainer.transform);
        } else {
            agent = Instantiate(TargetPrefab, TargetContainer.transform);
        }
        //Physics2D.SyncTransforms();
        agent.GetComponent<SpriteRenderer>().color = color;
        bool guarantee = true;
        RaycastHit2D hit = Physics2D.Raycast(
            Vector2.up,
            Vector2.up,
            0
        );

        while (guarantee || (
            hit.collider != null
            && (
                hit.collider.gameObject.CompareTag("Barbed Wire")
                || hit.collider.gameObject.CompareTag("Boundaries")
            )
        ) || IsPointInObstacle(rand)
        || Touching(agent, agents)
        ) {
            guarantee = false;

            randX = Random.Range(-7.25f, 5.25f);
            randY = Random.Range(-5.61f, 3.69f);
            rand = new Vector2(randX, randY);
            agent.transform.position = rand;
            //Physics2D.SyncTransforms();

            foreach (Vector2 spot in spots) {
                hit = Physics2D.Raycast(
                    rand,
                    spot - rand,
                    Vector2.Distance(spot, rand)
                );
                /*
                Debug.Log(hit.collider);
                Debug.DrawRay(
                    rand,
                    spot - rand,
                    color, 1000, false
                );
                */

                if (!(
                    hit.collider != null
                    && (
                        hit.collider.gameObject.CompareTag("Barbed Wire")
                        || hit.collider.gameObject.CompareTag("Boundaries")
                    )
                )) {
                    break;
                }
            }
        }

        if (isSource) {
            VisibilityGraph graph = GetComponent<DrawVisibilityGraph>().Graph;
            VisibilityGraph graphCopy = new VisibilityGraph();
            graphCopy.Vertices = graph.ClonedVertices();
            graphCopy.AdjList = graph.ClonedAdjList();
            agent.GetComponent<Pathing>().OriginalGraph = graphCopy;

            agent.GetComponent<Pathing>().SpawnAgentsScript
                = GetComponent<SpawnAgents>();
            agent.GetComponent<Pathing>().DrawVisibilityGraphScript
                = GetComponent<DrawVisibilityGraph>();
            agent.GetComponent<Pathing>().StatsScript = StatsScript;
        }

        return agent;
    }

    private bool IsPointInObstacle(Vector2 rand) {
        foreach (
            PolygonCollider2D obstacle
            in Obstacles.GetComponentsInChildren<PolygonCollider2D>()
        ) {
            if (obstacle.bounds.Contains(rand)) {
                return true;
            }
        }

        return false;
    }

    private bool Touching(GameObject agent, List<GameObject> agents) {
        foreach (GameObject agentToCompare in agents) {
            if (
                (agentToCompare.transform.position - agent.transform.position)
                    .sqrMagnitude < 0.04 // 0.2^2
            ) {
                return true;
            }
        }

        return false;
    }
}
