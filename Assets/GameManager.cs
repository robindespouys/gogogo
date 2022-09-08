using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Board;
    public GameObject BlackPawnModel;
    public GameObject WhitePawnModel;
    public Material YellowMaterial;
    public Material BlueMaterial;

    private int sizeBoard;
    private string currentPawnTeam;
    private Vector2 lastPawnPosition;
    private GameObject currentPawnPlaying;
    private GameObject blackPawnsContainer;
    private GameObject whitePawnsContainer;
    private GameObject blackPrison;
    private GameObject WhitePrison;
    private List<GameObject> blackPawns;
    private List<GameObject> whitePawns;
    private List<GameObject> stringOfPawnsAllies = new List<GameObject>();
    private List<GameObject> stringOfPawnsEnemies = new List<GameObject>();
    private Dictionary<Vector2, GameObject> pawnsOnTheBoard = new Dictionary<Vector2, GameObject>();

    #region Initiatization

    // Start is called before the first frame update
    void Start()
    {
        sizeBoard = Board.GetComponent<BoardGenerator>().SizeBoard;
        blackPawns = new List<GameObject>();
        whitePawns = new List<GameObject>();
        blackPawnsContainer = Board.transform.Find("BlackPawns").gameObject;
        whitePawnsContainer = Board.transform.Find("WhitePawns").gameObject;
        blackPrison = Board.transform.Find("BlackPrison").gameObject;
        WhitePrison = Board.transform.Find("WhitePrison").gameObject;
        currentPawnPlaying = SpawnBlackPawn();
        currentPawnTeam = "Black";
    }

    #endregion

    #region Utils Functions

    public static Vector3 GetMousePositionOnXZPlane(GameObject board)
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane XZPlane = new Plane(board.transform.up, Vector3.zero);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hitPoint.y = 0f;
            return hitPoint;
        }
        return Vector3.zero;
    }

    public bool IsPawnInsideBoard(GameObject pawn)
    {
        if (pawn.transform.localPosition.x >= -0.5f &&
            pawn.transform.localPosition.x <= sizeBoard - 0.5f &&
            pawn.transform.localPosition.z >= -0.5f &&
            pawn.transform.localPosition.z <= sizeBoard - 0.5f)
        {
            return true;
        }
        return false;
    }

    public bool IsPawnOnAnotherPawn(GameObject pawn)
    {
        foreach(GameObject otherPawn in blackPawns)
        {
            if (otherPawn.transform.localPosition == pawn.transform.localPosition)
            {
                return true;
            }
        }
        foreach (GameObject otherPawn in whitePawns)
        {
            if (otherPawn.transform.localPosition == pawn.transform.localPosition)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 GetPawnPosition(GameObject pawn)
    {
        return new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z);
    }

    public GameObject SpawnBlackPawn()
    {
        GameObject newPawn = Instantiate(BlackPawnModel, blackPawnsContainer.transform, false);
        MovePawn(newPawn);
        return newPawn;
    }

    public GameObject SpawnWhitePawn()
    {
        GameObject newPawn = Instantiate(WhitePawnModel, whitePawnsContainer.transform, false);
        MovePawn(newPawn);
        return newPawn;
    }

    private bool PawnPositionChanged()
    {
        if (new Vector2(currentPawnPlaying.transform.localPosition.x, currentPawnPlaying.transform.localPosition.z) != lastPawnPosition
            && Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
        {
            lastPawnPosition = new Vector2(currentPawnPlaying.transform.localPosition.x, currentPawnPlaying.transform.localPosition.z);
            return true;
        }
        return false;
    }

    public void MovePawn(GameObject pawn)
    {
        Vector3 mousePos = GetMousePositionOnXZPlane(Board);
        Vector3 truePosition = new Vector3(Mathf.Round(mousePos.x) - 0.5f, 0.75f, Mathf.Round(mousePos.z) - 0.5f);
        pawn.transform.localPosition = truePosition;
    }

    #endregion

    #region Visual Functions
    public void ClearStringColoringAllies()
    {
        foreach (GameObject neighbor in stringOfPawnsAllies)
        {
            if (currentPawnTeam == "Black")
            {
                neighbor.GetComponent<Renderer>().material = BlackPawnModel.GetComponent<Renderer>().sharedMaterial;
            }
            else
            {
                neighbor.GetComponent<Renderer>().material = WhitePawnModel.GetComponent<Renderer>().sharedMaterial;
            }
        }
    }

    public void ClearStringColoringEnemies()
    {
        foreach (GameObject neighbor in stringOfPawnsEnemies)
        {
            if (currentPawnTeam == "Black")
            {
                neighbor.GetComponent<Renderer>().material = WhitePawnModel.GetComponent<Renderer>().sharedMaterial;
            }
            else
            {
                neighbor.GetComponent<Renderer>().material = BlackPawnModel.GetComponent<Renderer>().sharedMaterial;
            }
        }
    }

    private void ShowAllyString(GameObject pawn)
    {
        ClearStringColoringAllies();
        stringOfPawnsAllies.Clear();
        stringOfPawnsAllies = FindStringOfPawn(pawn);
        Material savedMaterial = pawn.GetComponent<Renderer>().material;
        foreach (GameObject neighbor in stringOfPawnsAllies)
        {
            neighbor.GetComponent<Renderer>().material = YellowMaterial;
        }
        pawn.GetComponent<Renderer>().material = savedMaterial;
    }

    private void ShowEnemyString(GameObject pawn)
    {
        ClearStringColoringEnemies();
        stringOfPawnsEnemies.Clear();
        stringOfPawnsEnemies = FindEnemyStringsOfPawn(pawn);
        foreach (GameObject neighbor in stringOfPawnsEnemies)
        {
            neighbor.GetComponent<Renderer>().material = BlueMaterial;
        }
    }

    #endregion

    #region Searching Strings Functions

    public GameObject CheckNeighbor(GameObject pawn, Vector2 neighborPosition, List<GameObject> enemyToVisit, List<GameObject> enemyString)
    {
        GameObject neighbor;
        if (pawnsOnTheBoard.TryGetValue(neighborPosition, out neighbor))
        {
            if (neighbor.name == pawn.name && !enemyString.Contains(neighbor))
            {
                if (!enemyToVisit.Contains(neighbor))
                {
                    enemyToVisit.Add(neighbor);
                }
                return neighbor;
            }
        }
        return null;
    }

    public List<GameObject> RecursiveSearchStringOfPawn(GameObject pawn, List<GameObject> pawnsToVisit, List<GameObject> pawnsString)
    {
        GameObject pawnToVisit = null;

        if (pawn.transform.localPosition.x >= 0.5f)
        {
            Vector2 westNeighbor = new Vector2(pawn.transform.localPosition.x - 1f, pawn.transform.localPosition.z);
            pawnToVisit = CheckNeighbor(pawn, westNeighbor, pawnsToVisit, pawnsString);
        }

        if (pawn.transform.localPosition.x <= sizeBoard - 1.5f)
        {
            Vector2 eastNeighbor = new Vector2(pawn.transform.localPosition.x + 1f, pawn.transform.localPosition.z);
            pawnToVisit = CheckNeighbor(pawn, eastNeighbor, pawnsToVisit, pawnsString);
        }


        if (pawn.transform.localPosition.z >= 0.5f)
        {
            Vector2 southNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z - 1f);
            pawnToVisit = CheckNeighbor(pawn, southNeighbor, pawnsToVisit, pawnsString);
        }

        if (pawn.transform.localPosition.z <= sizeBoard - 1.5f)
        {
            Vector2 northNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z + 1f);
            pawnToVisit = CheckNeighbor(pawn, northNeighbor, pawnsToVisit, pawnsString);
        }

        if (pawnToVisit == null && pawnsToVisit.Count == 0)
        {
            return pawnsString;
        }

        if (pawnToVisit == null)
        {
            pawnToVisit = pawnsToVisit[pawnsToVisit.Count - 1];
            pawnsString.Add(pawnToVisit);
            pawnsToVisit.Remove(pawnToVisit);
        }
        else
        {
            pawnsToVisit.Remove(pawnToVisit);
            pawnsString.Add(pawnToVisit);
        }

        return RecursiveSearchStringOfPawn(pawnToVisit, pawnsToVisit, pawnsString);
    }

    public List<GameObject> FindStringOfPawn(GameObject pawn)
    {
        List<GameObject> pawnsToVisit = new List<GameObject>();
        List<GameObject> stringOfPawn = new List<GameObject>();
        stringOfPawn.Add(pawn);
        return RecursiveSearchStringOfPawn(pawn, pawnsToVisit, stringOfPawn);
    }

    private List<GameObject> FindOneStringOfEnemyNeighbor(GameObject pawn, Vector2 neighborPosition, List<GameObject> stringsOfEnemy)
    {
        GameObject neighbor;
        if (pawnsOnTheBoard.TryGetValue(neighborPosition, out neighbor))
        {
            if (neighbor.name != pawn.name && !stringsOfEnemy.Contains(neighbor))
            {
                return FindStringOfPawn(neighbor);
            }
        }
        return null;
    }

    private List<GameObject> FindEnemyStringsOfPawn(GameObject pawn)
    {
        List<GameObject> enemyStrings = new List<GameObject>();
        if (pawn.transform.localPosition.x >= 0.5f)
        {
            Vector2 westNeighbor = new Vector2(pawn.transform.localPosition.x - 1f, pawn.transform.localPosition.z);
            List<GameObject> oneEnemyString = FindOneStringOfEnemyNeighbor(pawn, westNeighbor, enemyStrings);
            if (oneEnemyString != null)
            {
                enemyStrings.AddRange(oneEnemyString);
            }
        }

        if (pawn.transform.localPosition.x <= sizeBoard - 1.5f)
        {
            Vector2 eastNeighbor = new Vector2(pawn.transform.localPosition.x + 1f, pawn.transform.localPosition.z);
            List<GameObject> oneEnemyString = FindOneStringOfEnemyNeighbor(pawn, eastNeighbor, enemyStrings);
            if (oneEnemyString != null)
            {
                enemyStrings.AddRange(oneEnemyString);
            }
        }

        if (pawn.transform.localPosition.z >= 0.5f)
        {
            Vector2 southNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z - 1f);
            List<GameObject> oneEnemyString = FindOneStringOfEnemyNeighbor(pawn, southNeighbor, enemyStrings);
            if (oneEnemyString != null)
            {
                enemyStrings.AddRange(oneEnemyString);
            }
        }

        if (pawn.transform.localPosition.z <= sizeBoard - 1.5f)
        {
            Vector2 northNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z + 1f);
            List<GameObject> oneEnemyString = FindOneStringOfEnemyNeighbor(pawn, northNeighbor, enemyStrings);
            if (oneEnemyString != null)
            {
                enemyStrings.AddRange(oneEnemyString);
            }
        }
        return enemyStrings;
    }

    #endregion

    #region Rules Functions

    public int ComputeLibertiesOfStringOfPawns(List<GameObject> stringOfPawns, bool isAllyString = false)
    {
        Dictionary<Vector2, bool> mapOfLiberties = new Dictionary<Vector2, bool>();
        foreach (GameObject pawn in stringOfPawns)
        {
            if (pawn.transform.localPosition.z <= sizeBoard - 1.5f)
            {
                Vector2 northNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z + 1f);
                if (!pawnsOnTheBoard.ContainsKey(northNeighbor))
                {
                    mapOfLiberties.TryAdd(northNeighbor, true);
                }
            }
            if (pawn.transform.localPosition.z >= 0.5f)
            {
                Vector2 southNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z - 1f);
                if (!pawnsOnTheBoard.ContainsKey(southNeighbor))
                {
                    mapOfLiberties.TryAdd(southNeighbor, true);
                }
            }
            if (pawn.transform.localPosition.x <= sizeBoard - 1.5f)
            {
                Vector2 eastNeighbor = new Vector2(pawn.transform.localPosition.x + 1f, pawn.transform.localPosition.z);
                if (!pawnsOnTheBoard.ContainsKey(eastNeighbor))
                {
                    mapOfLiberties.TryAdd(eastNeighbor, true);
                }
            }
            if (pawn.transform.localPosition.x >= 0.5f)
            {
                Vector2 westNeighbor = new Vector2(pawn.transform.localPosition.x - 1f, pawn.transform.localPosition.z);
                if (!pawnsOnTheBoard.ContainsKey(westNeighbor))
                {
                    mapOfLiberties.TryAdd(westNeighbor, true);
                }
            }
        }

        if (isAllyString)
        {
            mapOfLiberties.Remove(GetPawnPosition(currentPawnPlaying));
        }

        return mapOfLiberties.Count;
    }

    public void NextTurn()
    {
        ClearStringColoringAllies();
        ClearStringColoringEnemies();
        stringOfPawnsEnemies.Clear();
        stringOfPawnsAllies.Clear();
        if (currentPawnTeam == "Black")
        {
            currentPawnTeam = "White";
            blackPawns.Add(currentPawnPlaying);
            currentPawnPlaying = SpawnWhitePawn();
        }
        else
        {
            currentPawnTeam = "Black";
            whitePawns.Add(currentPawnPlaying);
            currentPawnPlaying = SpawnBlackPawn();
        }
    }

    #endregion

    #region Actions Functions

    public void PlaceInPrison(GameObject prison, GameObject pawn)
    {
        int nbPrisonners = prison.transform.childCount;
        pawn.transform.SetParent(prison.transform);
        int x = nbPrisonners / sizeBoard;
        int z = nbPrisonners % sizeBoard;
        pawn.transform.localPosition = new Vector3(x, 0, 1.5f + z);
    }

    public void TryCaptureNeighborString(Vector2 neighborPosition, GameObject pawn)
    {
        GameObject neighbor;
        if (pawnsOnTheBoard.TryGetValue(neighborPosition, out neighbor))
        {
            if (neighbor.name != pawn.name)
            {
                List<GameObject> enemies = FindStringOfPawn(neighbor);
                int liberties = ComputeLibertiesOfStringOfPawns(enemies);
                if (liberties == 1)
                {
                    foreach (GameObject enemy in enemies.ToArray())
                    {
                        pawnsOnTheBoard.Remove(new Vector2(enemy.transform.localPosition.x, enemy.transform.localPosition.z));
                    }
                    if (currentPawnTeam == "Black")
                    {
                        foreach (GameObject enemy in enemies.ToArray())
                        {
                            whitePawns.Remove(enemy);
                            PlaceInPrison(blackPrison, enemy);
                        }
                    }
                    else
                    {
                        foreach (GameObject enemy in enemies.ToArray())
                        {
                            blackPawns.Remove(enemy);
                            PlaceInPrison(WhitePrison, enemy);
                        }
                    }
                }
            }
        }
    }

    public void TryToCaptureAdversaryStrings(GameObject pawn)
    {
        if (pawn.transform.localPosition.z <= sizeBoard - 1.5f)
        {
            Vector2 northNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z + 1f);
            TryCaptureNeighborString(northNeighbor, pawn);
        }
        if (pawn.transform.localPosition.z >= 0.5f)
        {
            Vector2 southNeighbor = new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z - 1f);
            TryCaptureNeighborString(southNeighbor, pawn);
        }
        if (pawn.transform.localPosition.x <= sizeBoard - 1.5f)
        {
            Vector2 eastNeighbor = new Vector2(pawn.transform.localPosition.x + 1f, pawn.transform.localPosition.z);
            TryCaptureNeighborString(eastNeighbor, pawn);
        }
        if (pawn.transform.localPosition.x >= 0.5f)
        {
            Vector2 westNeighbor = new Vector2(pawn.transform.localPosition.x - 1f, pawn.transform.localPosition.z);
            TryCaptureNeighborString(westNeighbor, pawn);
        }
    }

    public bool PlacePawn(GameObject pawn)
    {
        if (!IsPawnInsideBoard(pawn))
        {
            return false;
        }

        if (IsPawnOnAnotherPawn(pawn))
        {
            return false;
        }

        TryToCaptureAdversaryStrings(pawn);

        stringOfPawnsAllies.Clear();
        stringOfPawnsAllies = FindStringOfPawn(pawn);
        stringOfPawnsAllies.Add(pawn);
        int result = ComputeLibertiesOfStringOfPawns(stringOfPawnsAllies, true);
        if (result == 0)
        {
            return false;
        }

        pawnsOnTheBoard.Add(new Vector2(pawn.transform.localPosition.x, pawn.transform.localPosition.z), pawn);
        return true;
    }

    #endregion

    #region Update Functions

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlacePawn(currentPawnPlaying))
            {
                NextTurn();
            }
        }
    }

    void FixedUpdate()
    {
        if (!Input.GetMouseButton(1))
        {
            MovePawn(currentPawnPlaying);
        }
        if (PawnPositionChanged())
        {

            ShowAllyString(currentPawnPlaying);
            ShowEnemyString(currentPawnPlaying);
        }
    }

    #endregion
}
