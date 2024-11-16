using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1; 

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        switch (state)
        {
            case EnemyState.DEFAULT:
                if (Vector3.Distance(transform.position, playerGameObject.transform.position) <= visionDistance)
                {
                    material.color = Color.red;

                    targetTile = playerGameObject.GetComponent<Player>().currentTile;
                    path = pathFinder.FindPathAStar(currentTile, targetTile);

                    if (path.Count > 0)
                    {
                        state = EnemyState.CHASE;
                    }
                }
                else
                {
                    if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                    if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                break;

            case EnemyState.CHASE:
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                else
                {
                    state = EnemyState.DEFAULT; 
                }
                break;

            case EnemyState.MOVING:
                velocity = targetTile.transform.position - transform.position;
                transform.position += (velocity.normalized * speed) * Time.deltaTime;

                if (Vector3.Distance(transform.position, targetTile.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;

                    if (state == EnemyState.CHASE && path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                    }
                    else
                    {
                        state = EnemyState.DEFAULT;
                    }
                }
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }


    // TODO: Third behavior (Describe what it does)
    private void HandleEnemyBehavior3()
    {
        switch (state)
        {
            case EnemyState.DEFAULT:
                material.color = Color.magenta;

                float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);
                if (distanceToPlayer <= visionDistance)
                {
                    Tile playerTile = playerGameObject.GetComponent<Player>().currentTile;
                    targetTile = GetTileAFewStepsAway(playerTile, 2);

                    if (targetTile != null)
                    {
                        path = pathFinder.FindPath(currentTile, targetTile);
                        state = EnemyState.CHASE;
                    }
                }
                else
                {
                    if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 15);

                    if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                break;

            case EnemyState.CHASE:
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                else
                {
                    state = EnemyState.DEFAULT;
                }
                break;

            case EnemyState.MOVING:
                velocity = targetTile.transform.position - transform.position;
                transform.position += (velocity.normalized * speed) * Time.deltaTime;

                if (Vector3.Distance(transform.position, targetTile.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    private Tile GetTileAFewStepsAway(Tile playerTile, int stepsAway)
    {
        Vector3 direction = (currentTile.transform.position - playerTile.transform.position).normalized;
        Vector3 targetPosition = playerTile.transform.position + direction * stepsAway;

        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (Transform child in GameObject.Find("MapGenerator").transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile.mapTile.Walkable)
            {
                float distance = Vector3.Distance(tile.transform.position, targetPosition);
                if (distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = distance;
                }
            }
        }

        return closestTile;
    }



}
