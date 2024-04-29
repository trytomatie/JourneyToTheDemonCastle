using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using Unity.VisualScripting;
using UnityEditor;
using static Generator3D;
using System.IO;
using UnityEditor.AI;
using System.Linq;

public class Generator3D : MonoBehaviour {
    enum CellType {
        None,
        Room,
        Hallway,
        Stairs
    }

    public class Room {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }
    public static int gridSize = 4;
    public static Vector3Int size = new Vector3Int(30,5,30);
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;
    [SerializeField] GameObject cubePrefab;
    [SerializeField] GameObject hallwayPrefab;
    [SerializeField] GameObject entrancePrefab;
    [SerializeField] GameObject stairsPrefab;
    [SerializeField] GameObject[] roomPrefabs;
    [SerializeField] GameObject teleporter;
    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material greenMaterial;

    Random random;
    Grid3D<CellType> grid;
    List<Room> rooms;
    List<Vector3Int> positionsGenerated = new List<Vector3Int>();
    Delaunay3D delaunay;
    public HashSet<Prim.Edge> selectedEdges = new HashSet<Prim.Edge>(0);

    // Debug
    public List<Ray> hallwayRays = new List<Ray>();

    void Start() {

        Generate(11248882);
    }

    public void Generate(int seed)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        positionsGenerated.Clear();
        random = new Random(seed);
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        selectedEdges.Clear();
        rooms = new List<Room>();
        SystemMessageManager.DisplayProcessingMessage("Generating Dungeon...");
        PlaceRooms();
        if(!Triangulate())
        {
            Debug.Log("BAD SEED, RETRYING...");
            Generate(UnityEngine.Random.Range(0, 100000));
            return;
        }
        CreateHallways();
        PathfindHallways();
        DungeonGenerationTileController.GenerateAll(this);
        MapGenerator.Instance.GenerateMapTextures(size.y,gridSize);
    }

    private Vector3Int SouthernGridCenter
    {
        get { return new Vector3Int(size.x / 2, size.y /2, 0); }
    }

    void PlaceRooms() {
        // StartRoom
        Room startRoom = new Room(SouthernGridCenter, new Vector3Int(1, 1, 1));
        rooms.Add(startRoom);
        PlaceRoom(startRoom, entrancePrefab);
        foreach (var pos in startRoom.bounds.allPositionsWithin)
        {
            grid[pos] = CellType.Room;
        }

        for (int i = 0; i < 300; i++) {
            Vector3Int location = new Vector3Int(
                random.Next(0, size.x),
                random.Next(0, size.y),
                random.Next(0, size.z)
            );

            int rnd = random.Next(0, roomPrefabs.Length);
            RoomProperties roomProperties = roomPrefabs[rnd].GetComponent<RoomProperties>();

            Vector3Int roomSize = new Vector3Int(
                (int)roomProperties.roomSize.x,
                (int)roomProperties.roomSize.y,
                (int)roomProperties.roomSize.z
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z) {
                add = false;
            }

            if (add) {
                rooms.Add(newRoom);
                PlaceRoom(newRoom, roomPrefabs[rnd]);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }
            if(rooms.Count == roomCount)
            {
                break;
            }
        }
    }

    bool Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
        }
        delaunay = Delaunay3D.Triangulate(vertices);
        bool foundStartRoom = false;
        foreach (var selectedEdge in delaunay.Edges)
        {
            if ((selectedEdge.U as Vertex<Room>).Item.bounds.position == rooms[0].bounds.position)
            {
                foundStartRoom = true;
                break;
            }
        }
        return foundStartRoom;
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        // Create edge for the StartHallway
        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);

        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);
        PlaceCube(SouthernGridCenter * gridSize, Vector3Int.one * gridSize  , greenMaterial);
        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }

    void PathfindHallways() {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);
        hallwayRays.Clear();
        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;
            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, NormalPathFindingCostFunction);
            if(path == null) path = aStar.FindPath(startPos, endPos, ForcePathFindingCostFunction);

            if (path != null) 
            {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0) {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            int yDir = Mathf.Clamp(delta.y, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                            // direction vector to rotation
                            Vector3 directionVector = new Vector3(xDir, 0,zDir);
                            Quaternion rotation = Quaternion.LookRotation(-directionVector);
 
                            grid[prev + horizontalOffset] = CellType.Stairs;
                            grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            if(yDir == -1)
                            {
                                PlaceStairs(prev + horizontalOffset, rotation, false);
                                PlaceStairs(prev + horizontalOffset * 2, rotation, false);
                                PlaceStairs(prev + verticalOffset + horizontalOffset, rotation, false);
                                PlaceStairs(prev + verticalOffset + horizontalOffset * 2, rotation, true);
                            }
                            else
                            {
                                rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 180, rotation.eulerAngles.z);
                                PlaceStairs(prev + horizontalOffset, rotation, false);
                                PlaceStairs(prev + horizontalOffset * 2, rotation, true);
                                PlaceStairs(prev + verticalOffset + horizontalOffset, rotation, false);
                                PlaceStairs(prev + verticalOffset + horizontalOffset * 2, rotation, false);
                            }

                        }
                        Ray debugRay = new Ray((prev + new Vector3(0.5f, 0.5f, 0.5f)) * gridSize, ((Vector3)(current - prev)).normalized);
                        hallwayRays.Add(debugRay);
                        // Debug.DrawRay(debugRay.origin, debugRay.direction * 1.5f, Color.blue, 100, false);
                        // Debug.DrawLine((prev + new Vector3(0.5f, 0.5f, 0.5f))* gridSize, (current + new Vector3(0.5f, 0.5f, 0.5f)) * gridSize, Color.blue, 100, false);
                    }
                }
                // Remove duplicate positions from path
                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway && !positionsGenerated.Contains(pos)) 
                    {
                        PlaceHallway(pos);
                    }
                }
            }
            else // Place Teleporter Instead
            {
                Instantiate(teleporter, startPos * gridSize, Quaternion.identity, transform);
                Instantiate(teleporter, endPos * gridSize, Quaternion.identity, transform);
            }
        }
    }

    private DungeonPathfinder3D.PathCost NormalPathFindingCostFunction(DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b,Vector3Int endPos ){
        var pathCost = new DungeonPathfinder3D.PathCost();
        var delta = b.Position - a.Position;
        if (delta.y == 0)
        {
            //flat hallway
            pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

            if (grid[b.Position] == CellType.Stairs)
            {
                return pathCost;
            }
            else if (grid[b.Position] == CellType.Room)
            {
                pathCost.cost += 5;
            }
            else if (grid[b.Position] == CellType.None)
            {
                pathCost.cost += 1;
            }

            pathCost.traversable = true;
        }
        else
        {
            //staircase
            if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

            pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

            int xDir = Mathf.Clamp(delta.x, -1, 1);
            int zDir = Mathf.Clamp(delta.z, -1, 1);
            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

            if (!grid.InBounds(a.Position + verticalOffset)
                || !grid.InBounds(a.Position + horizontalOffset)
                || !grid.InBounds(a.Position + verticalOffset + horizontalOffset))
            {
                return pathCost;
            }

            if (grid[a.Position + horizontalOffset] != CellType.None
                || grid[a.Position + horizontalOffset * 2] != CellType.None
                || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
            {
                return pathCost;
            }

            pathCost.traversable = true;
            pathCost.isStairs = true;
        }

        return pathCost;
    }
    private DungeonPathfinder3D.PathCost ForcePathFindingCostFunction(DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b, Vector3Int endPos)
    {
        var pathCost = new DungeonPathfinder3D.PathCost();
        var delta = b.Position - a.Position;
        if (delta.y == 0)
        {
            //flat hallway
            pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

            if (grid[b.Position] == CellType.Stairs)
            {
                return pathCost;
            }
            else if (grid[b.Position] == CellType.Room)
            {
                pathCost.cost += 5;
            }
            else if (grid[b.Position] == CellType.None)
            {
                pathCost.cost += 1;
            }

            pathCost.traversable = true;
        }
        else
        {
            //staircase
            if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

            pathCost.cost =  Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

            int xDir = Mathf.Clamp(delta.x, -1, 1);
            int zDir = Mathf.Clamp(delta.z, -1, 1);
            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

            if (!grid.InBounds(a.Position + verticalOffset)
                || !grid.InBounds(a.Position + horizontalOffset)
                || !grid.InBounds(a.Position + verticalOffset + horizontalOffset))
            {
                return pathCost;
            }

            if (grid[a.Position + horizontalOffset] != CellType.None
                || grid[a.Position + horizontalOffset * 2] != CellType.None
                || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
            {
                return pathCost;
            }

            pathCost.traversable = true;
            pathCost.isStairs = true;
        }

        return pathCost;
    }
    void PlaceCube(Vector3Int location, Vector3Int size, Material material) {
        GameObject go = Instantiate(cubePrefab, location, Quaternion.identity, transform);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceRoom(Room room, GameObject roomPrefab) {
        PlaceCube(room.bounds.position * gridSize, room.bounds.size * gridSize, redMaterial);
        GameObject go = Instantiate(roomPrefab, room.bounds.position * gridSize, Quaternion.identity, transform);
        // Add room to positions generatedList
        foreach (var pos in room.bounds.allPositionsWithin)
        {
            positionsGenerated.Add(pos);
        }

    }

    void PlaceHallway(Vector3Int location) {
        GameObject go = Instantiate(hallwayPrefab, location * gridSize, Quaternion.identity, transform);
        positionsGenerated.Add(location);
    }

    void PlaceStairs(Vector3Int location,Quaternion rotation,bool test) {
        if(test == true) 
        {
            GameObject go = Instantiate(stairsPrefab, location * gridSize, Quaternion.identity,transform);
            go.transform.Find("Stair").rotation = rotation;
            positionsGenerated.Add(location);
        }
        else
        {
            PlaceHallway(location);
        }


    }
}

[CustomEditor(typeof(Generator3D))]
public class Generator3DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Generator3D generator = (Generator3D)target;
        if (GUILayout.Button("Generate Random Dungeon"))
        {
            int seed = UnityEngine.Random.Range(0, 40000000);
            Debug.Log("Seed: " + seed);
            generator.Generate(seed);
        }
    }

    private void OnSceneGUI()
    {
        Generator3D generator = (Generator3D)target;
        foreach (var ray in generator.hallwayRays)
        {
            Handles.color = Color.blue;
            Handles.DrawLine(ray.origin, ray.GetPoint(4));
        }
        foreach (var edge in generator.selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;
            Handles.color = Color.cyan;
            Handles.DrawLine(startRoom.bounds.center * 4, endRoom.bounds.center * 4);
        }
    }
}
