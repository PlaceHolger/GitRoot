using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArenaDefinition
{
    public int width;
    public int height;
    public List<ArenaObstacle> obstacles = new List<ArenaObstacle>();
    public List<ArenaSpawnPoints> spawnPoints = new List<ArenaSpawnPoints>();
}

[System.Serializable]
public class ArenaObstacle
{
    public int x;
    public int y;
    public int width;
    public int height;
}

[System.Serializable]
public class ArenaSpawnPoints
{
    public int x;
    public int y;
}

public class ArenaGridElement
{
    public GameObject groundGameObject = null;
    public GameObject additionalGameObject = null;
}

public enum ArenaGridContentInformation
{
    Empty = 0,
    Ground = 1,
    Obstacle = 2 
}

public class ArenaGridContent
{
    public Dictionary<int,ArenaGridContentInformation> content = new Dictionary<int,ArenaGridContentInformation>();
}

public class ArenaGridConfiguration
{
    public Dictionary<int, ArenaGridContentInformation> contentConfiguration = new Dictionary<int, ArenaGridContentInformation>();
}

public class ArenaManager: MonoBehaviour
{
    [SerializeField]
    private TextAsset arenaDefinitionFile;

    [SerializeField]
    private int arenaGridWidth = 16;
    public int ArenaGridWidth { get { return arenaGridWidth; } }

    private int effectiveArenaGridWidth { get { return arenaGridWidth + (arenaGridBorder * 2); } }

    [SerializeField]
    private int arenaGridLength = 9;
    public int ArenaGridLength { get { return arenaGridLength; } }

    private int effectiveArenaGridLength { get { return arenaGridLength + (arenaGridBorder * 2); } }

    [SerializeField, Range(0.1f, 3f)]
    private float arenaGridCellSize = 1.0f;

    [SerializeField, Range(0, 7)]
    private int arenaGridBorder = 2;
    public int ArenaGridBorder { get { return arenaGridBorder; } }

    [SerializeField]
    private GameObject arenaEmptyGround;

    [SerializeField]
    private GameObject arenaSpawnPoint;

    [SerializeField, Tooltip("Order: Top Left, Top Right, Bottom Left, Bottom Right, Straight, Empty")]
    private List<GameObject> arenaBorderPrefabs;

    [SerializeField]
    private List<GameObject> decorationTreePrefabs;

    private List<GameObject> borderElements = new List<GameObject>();

    private List<GameObject> spawnPoints = new List<GameObject>();
    public List<GameObject> ArenaSpawnPoints { get { return spawnPoints; } }

    private Dictionary<int, ArenaGridElement> arenaGridElements = new Dictionary<int, ArenaGridElement>();

    private ArenaGridContent arenaGridContent = new ArenaGridContent();

    private ArenaDefinition arenaDefinition;

    public int IndexAt(int x, int y)
    {
        return (effectiveArenaGridWidth * x) + y;
    }

    public (int x, int y) CoordAt ( int index )
    {
        return ( index / effectiveArenaGridWidth, index % effectiveArenaGridWidth);
    }

    public void GenerateArena ( ArenaDefinition arenaDefinition )
    {
        arenaGridWidth = arenaDefinition.width;
        arenaGridLength = arenaDefinition.height;

        ArenaGridConfiguration arenaGridConfiguration = new ArenaGridConfiguration();
        arenaGridConfiguration.contentConfiguration[8] = ArenaGridContentInformation.Obstacle;

        arenaGridContent.content.Clear();

        for ( int i = 0; i < effectiveArenaGridLength; i++)
        {
            for (int j = 0; j < effectiveArenaGridWidth; j++)
            {
                if ( i < arenaGridBorder || j < arenaGridBorder || i >= effectiveArenaGridLength - arenaGridBorder || j >= effectiveArenaGridWidth - arenaGridBorder)
                {
                    arenaGridContent.content[IndexAt(i,j)] = ArenaGridContentInformation.Obstacle;
                }
                else
                {
                    arenaGridContent.content[IndexAt(i,j)] = ArenaGridContentInformation.Ground;
                }
            }
        }

        foreach ( ArenaObstacle arenaObstacle in arenaDefinition.obstacles )
        {
            for ( int i = arenaObstacle.x; i < arenaObstacle.x + arenaObstacle.height; i++ )
            {
                for ( int j = arenaObstacle.y; j < arenaObstacle.y + arenaObstacle.width; j++)
                {
                    arenaGridContent.content[IndexAt(i + arenaGridBorder, j + arenaGridBorder)] = ArenaGridContentInformation.Obstacle;
                }
            }
        }

        foreach ( ArenaSpawnPoints spawnPoint in arenaDefinition.spawnPoints )
        {
            GameObject spawnPointObject = Instantiate(arenaSpawnPoint, transform);
            spawnPointObject.transform.localPosition = new Vector3(spawnPoint.x + arenaGridBorder, 0, spawnPoint.y + arenaGridBorder);
            spawnPoints.Add(spawnPointObject);
        }

        foreach ( var contentInformation in arenaGridContent.content )
        {
            GameObject cellContent;

            (GameObject go, Quaternion quat, int cf) classification = ClassifyToCellPrefab(contentInformation.Key);

            ArenaGridElement arenaGridElement = new ArenaGridElement();
            arenaGridElements[contentInformation.Key] = arenaGridElement;
            if ( classification.go != null )
            {
                (int x, int y) coord = CoordAt(contentInformation.Key);
                arenaGridElement.groundGameObject = Instantiate(classification.go, transform);
                arenaGridElement.groundGameObject.transform.localPosition = new Vector3(coord.x, 0, coord.y);
                arenaGridElement.groundGameObject.transform.localRotation = classification.quat;
                arenaGridElement.groundGameObject.name = $"{coord.x} {coord.y} {classification.cf}";
                if ( classification.go != arenaEmptyGround && classification.go != arenaBorderPrefabs[2] )
                {
                    arenaGridElement.additionalGameObject = Instantiate(arenaEmptyGround, transform);
                    arenaGridElement.additionalGameObject.transform.localPosition = new Vector3(coord.x, 0, coord.y);
                    arenaGridElement.additionalGameObject.name = $"{coord.x} {coord.y} {classification.cf} add ground";
                }
            }
            
        }

        for ( int i = - arenaGridBorder; i < arenaGridLength + arenaGridBorder; i++ )
        {
            for ( int j = -arenaGridBorder; j < arenaGridWidth + arenaGridBorder; j++ )
            {
                if ( !(i >= -1 && i <= arenaGridLength && j >= -1 && j <= arenaGridWidth))
                {
                    //Debug.Log( $"deco at {i} {j}" );


                    (int x, int y) decorationCoord = (i + arenaGridBorder, j + arenaGridBorder);

                    Vector3 decorationPosition = new Vector3(decorationCoord.x, 0, decorationCoord.y);

                    int xMul = i;
                    if (xMul >= 0 && xMul < arenaGridLength) xMul = 0;
                    else xMul = xMul % (arenaGridLength - 1);

                    int yMul = j;
                    if (yMul >= 0 && yMul < arenaGridWidth) yMul = 0;
                    else yMul = yMul % (arenaGridWidth - 1);

                    int randFac = Mathf.Max(Mathf.Abs(xMul), Mathf.Abs(yMul));

                    for (int k = 0; k < 1 + (randFac / 2); k++)
                    {
                        if ( Random.Range(0, 2 * (arenaGridBorder + 1) ) < randFac )
                        {
                            decorationPosition += (Random.insideUnitSphere * 0.5f);
                            decorationPosition.y = 0.5f;

                            Vector3 rescale = Vector3.one + ( Vector3.one * 0.5f * ( (float)randFac / (float)arenaGridBorder ) );

                            GameObject decorationObject = Instantiate(decorationTreePrefabs[Random.Range(0, decorationTreePrefabs.Count)], decorationPosition, Quaternion.AngleAxis(Random.Range(0f,180f), Vector3.up), transform);
                            decorationObject.transform.localScale = rescale;
                        }
                    }
                }
                
            }
        }
    }

    private (GameObject, Quaternion, int) ClassifyToCellPrefab (int key)
    {
        int neighbours = 0;
        int nineNeighbours = 0;
        (int x, int y) coordinate = CoordAt(key);
        bool top = false;
        bool left = false;
        bool right = false;
        bool bottom = false;

        bool[] neighbourObstacles = new bool[9] { false, false, false, false, false, false, false, false, false };

        if (!arenaGridContent.content.ContainsKey(key)) return (null, Quaternion.identity, -1);

        if ( arenaGridContent.content[key] == ArenaGridContentInformation.Obstacle )
        {
            for ( int i = -1; i < 2; i++ )
            {
                for ( int j = -1; j < 2; j++ )
                {
                    (int x, int y) newCoord = (coordinate.x + i, coordinate.y + j);
                    if ( newCoord.x < 0 || newCoord.x >= effectiveArenaGridLength -1 || newCoord.y < 0 || newCoord.y >= effectiveArenaGridWidth -1 )
                    {
                        neighbourObstacles[((i + 1) * 3) + (j + 1)] = true;
                        nineNeighbours++;
                    }
                    else
                    {
                        if (arenaGridContent.content[IndexAt(coordinate.x + i, coordinate.y + j)] == ArenaGridContentInformation.Obstacle)
                        {
                            nineNeighbours++;
                            neighbourObstacles[((i + 1) * 3) + (j + 1)] = true;
                        }
                    }
                }
            }


            if (coordinate.x < 1) // top
            {
                neighbours++;
                top = true;
            }
            else
            {
                if (arenaGridContent.content[IndexAt(coordinate.x - 1, coordinate.y)] == ArenaGridContentInformation.Obstacle)
                {
                    top = true;
                    neighbours++;
                }
            }
            if (coordinate.y < 1)
            {
                neighbours++;
                left = true;
            }
            else
            {
                if (arenaGridContent.content[IndexAt(coordinate.x, coordinate.y - 1)] == ArenaGridContentInformation.Obstacle)
                {
                    neighbours++;
                    left = true;
                }
            }
            if (coordinate.x >= effectiveArenaGridLength - 1)
            {
                neighbours++;
                bottom = true;
            }
            else
            {
                if (arenaGridContent.content[IndexAt(coordinate.x + 1, coordinate.y)] == ArenaGridContentInformation.Obstacle)
                {
                    neighbours++;
                    bottom = true;
                }
            };
            if (coordinate.y >= effectiveArenaGridWidth - 1)
            {
                neighbours++;
                right = true;
            }
            else
            {
                if (arenaGridContent.content[IndexAt(coordinate.x, coordinate.y + 1)] == ArenaGridContentInformation.Obstacle)
                {
                    right = true;
                    neighbours++;
                }
            };
        }
        else
        {
            return (arenaEmptyGround, Quaternion.identity, -1);
        }

        switch ( neighbours )
        {
            case 0:
                return (arenaBorderPrefabs[1], Quaternion.identity, 0);
            case 1:
            case 2:
                if (!left && !top) return (arenaBorderPrefabs[3], Quaternion.AngleAxis(180f, Vector3.up), 20);
                else if (!top && !right) return (arenaBorderPrefabs[3], Quaternion.AngleAxis(-90, Vector3.up), 21);
                else if (!right && !bottom) return (arenaBorderPrefabs[3], Quaternion.identity, 22);
                else return (arenaBorderPrefabs[3], Quaternion.AngleAxis(90f, Vector3.up), 23);
            case 3:
                if (!left) return (arenaBorderPrefabs[1], Quaternion.AngleAxis(180f, Vector3.up),30);
                else if (!top) return (arenaBorderPrefabs[1], Quaternion.AngleAxis(-90, Vector3.up), 31);
                else if (!right) return (arenaBorderPrefabs[1], Quaternion.identity, 32);
                else return (arenaBorderPrefabs[1], Quaternion.AngleAxis(90f, Vector3.up), 33);
            default:
                if (nineNeighbours > 8) return (arenaBorderPrefabs[2], Quaternion.identity, 4);
                else
                {
                    if (!neighbourObstacles[0]) return (arenaBorderPrefabs[0], Quaternion.AngleAxis(180f, Vector3.up), 40);
                    if (!neighbourObstacles[2]) return (arenaBorderPrefabs[0], Quaternion.AngleAxis(-90f, Vector3.up), 41);
                    if (!neighbourObstacles[6]) return (arenaBorderPrefabs[0], Quaternion.AngleAxis(90f, Vector3.up), 42);
                    else return (arenaBorderPrefabs[0], Quaternion.identity, 43);
                }
        }

    }

    private ArenaDefinition LoadArenaDefinition ()
    {

        ArenaDefinition fileArenaDefinition = JsonUtility.FromJson<ArenaDefinition>(arenaDefinitionFile.text);


        return fileArenaDefinition;
    }

    private void Awake()
    {
        GenerateArena( LoadArenaDefinition() );
    }
}
