using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LevelGenerationSystemGroup))]
[UpdateAfter(typeof(CreateTilesEntitiesSystem))]
[BurstCompile]
partial struct GraphGenerationSystem : ISystem {
    private EntityQuery _query;

    private Random _random;
    private int _cellSize;
    private int _sideRoomsGap;
    private int _cellsPerRoom;
    private int _roomsCount;
    private int _hallsCount;
    private int _nodesPercentToRemove;
    private float _additionalHallEntranceProbability;

    private Rect _innerZone;
    private NativeArray<int2> _directions;
    private NativeList<int2> _roomsList;
    private NativeList<int2> _hallsList;
    private NativeHashMap<int2, NativeList<int2>> _graph;
    private NativeHashSet<int2> _nodesForbidenForRemoval;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAspect<LevelGenerationAspect>();

        _query = state.GetEntityQuery(builder);
        state.RequireForUpdate(_query);
        state.RequireForUpdate<MatrixData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] GraphGenerationSystem starts");

        var config = _query.GetSingletonEntity();
        var aspect = SystemAPI.GetAspect<LevelGenerationAspect>(config);
        var levelProperties = aspect.LevelGenerationData.ValueRO;
        _cellSize = levelProperties.CellSize;
        _cellsPerRoom = levelProperties.CellsPerRoom;
        _sideRoomsGap = levelProperties.SideRoomsGap;
        _roomsCount = levelProperties.RoomsCount;
        _hallsCount = levelProperties.InnerZoneHallsCount;
        _additionalHallEntranceProbability = levelProperties.AdditionalHallEntranceProbability;
        _nodesPercentToRemove = levelProperties.NodesPercentToRemove;

        var generationDataStorage = SystemAPI.GetSingletonEntity<LevelData>();
        var levelData = SystemAPI.GetComponent<LevelData>(generationDataStorage);
        _innerZone = levelData.InnerZone;

        var edgesData = SystemAPI.GetComponentRW<EdgesData>(generationDataStorage);
        var roomCentersData = SystemAPI.GetComponentRW<RoomCentersData>(generationDataStorage);
        var hallCentersData = SystemAPI.GetComponentRW<HallsCentersData>(generationDataStorage);

        _random = aspect.Random.ValueRW.Value;
        _directions = LevelGenerationAspect.GetDirectionsArray(Allocator.Temp);
        _roomsList = new NativeList<int2>(_roomsCount, Allocator.Temp);
        _hallsList = new NativeList<int2>(_hallsCount, Allocator.Temp);

        ConstructGraph();

        PlaceHallsInTheInnerZone();

        RemoveSomeNodePercentage();

        var edges = GetEdgesListAndDisposeGraph();
        edgesData.ValueRW.Edges = edges.ToArray(Allocator.Temp);
        roomCentersData.ValueRW.RoomCenters = _roomsList;
        hallCentersData.ValueRW.HallCenters = _hallsList;

        UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] GraphGenerationSystem ends");
    }

    private void ConstructGraph() {
        int m = (_innerZone.To.x - _innerZone.From.x) / _cellSize + 1;
        int n = (_innerZone.To.y - _innerZone.From.y) / _cellSize + 1;
        int outerNodes = _roomsCount * 2;
        int nodesCount = m * n + outerNodes;

        _graph = ConstructGraphWithInnerNodes(nodesCount);
        _nodesForbidenForRemoval = new NativeHashSet<int2>(_graph.Count, Allocator.Temp);

        int roomsPerSide = _roomsCount / 4;
        int roomsMod = _roomsCount % 4;
        int randomStartIndex = _random.NextInt(0, 5); //4 dirs, 5 for exlusive upper range

        for (int i = 0; i < _directions.Length; i++) {
            int count = roomsPerSide;

            if (roomsMod > 0) {
                count++;
                roomsMod--;
            }

            int index = (randomStartIndex + i) % _directions.Length;

            ConstructSideNodes(_directions[index], count);
        }
    }
    private NativeHashMap<int2, NativeList<int2>> ConstructGraphWithInnerNodes(int nodesCount) {
        var graph = new NativeHashMap<int2, NativeList<int2>>(nodesCount, Allocator.Temp);
        var visited = new NativeHashSet<int2>(nodesCount, Allocator.Temp);
        //внутренние узлы
        for (int j = _innerZone.From.x; j <= _innerZone.To.x; j += _cellSize) {
            for (int i = _innerZone.From.y; i <= _innerZone.To.y; i += _cellSize) {

                int2 curCell = new(i, j);

                if (_innerZone.Contains(curCell)) {

                    visited.Add(curCell);
                    if (!graph.ContainsKey(curCell)) {
                        graph.Add(curCell, new(4, Allocator.Temp));
                    }

                    foreach (int2 dir in _directions) {

                        int2 adjCell = curCell + (dir * _cellSize);
                        if (_innerZone.Contains(adjCell)) {
                            if (!graph.ContainsKey(adjCell)) {
                                graph.Add(adjCell, new(4, Allocator.Temp));
                            }

                            if (!visited.Contains(adjCell)) {
                                graph[curCell].Add(adjCell);
                                graph[adjCell].Add(curCell);
                            }
                        }
                    }
                }
            }
        }
        visited.Dispose();
        return graph;
    }
    private void RemoveSomeNodePercentage() {

        int nodesToRemove = GetNodesToRemoveCount();

        if (nodesToRemove == 0) {
            return;
        }

        RemoveSomeNodeCount(nodesToRemove);
    }
    private void RemoveSomeNodeCount(int nodesToRemove) {

        var availableNodes = GetAvailableNodes(ref _nodesForbidenForRemoval);

        int removedNodes = 0;

        while (removedNodes < nodesToRemove && availableNodes.Length > 0) {
            int index = _random.NextInt(0, availableNodes.Length);
            int2 node = availableNodes[index];

            if (IsConnectedAfterNodeRemoval(node)) {
                RemoveNode(node);
                removedNodes++;
            }
            availableNodes.RemoveAtSwapBack(index);
        }

        if (removedNodes < nodesToRemove && removedNodes != 0) {
            RemoveSomeNodeCount(nodesToRemove - removedNodes);
        }
    }
    private int GetNodesToRemoveCount() {
        float modifier = math.clamp((float)_nodesPercentToRemove / 100, 0f, 1f);
        int nodesCanBeRemoved = _graph.Count - _roomsList.Length - _hallsList.Length;
        return (int)(nodesCanBeRemoved * modifier);
    }
    private NativeList<int2> GetAvailableNodes(ref NativeHashSet<int2> forbiddenNodes) {
        var availableNodes = new NativeList<int2>(_graph.Count, Allocator.Temp);
        foreach (var kvp in _graph) {
            if (!forbiddenNodes.Contains(kvp.Key)) {
                availableNodes.Add(kvp.Key);
            }
        }
        return availableNodes;
    }
    private NativeList<int2> GetAvailableNodes(Rect zone) {
        var availableNodes = new NativeList<int2>(_graph.Count, Allocator.Temp);
        foreach (var kvp in _graph) {
            if (zone.Contains(kvp.Key)) {
                availableNodes.Add(kvp.Key);
            }
        }
        return availableNodes;
    }
    private void ConstructSideNodes(int2 dir, int count) {

        int stepSize = _cellSize * _cellsPerRoom;

        int startX = _innerZone.From.x - _cellSize;
        int startY = _innerZone.From.y - _cellSize;
        int endX = _innerZone.To.x + _cellSize;
        int endY = _innerZone.To.y + _cellSize;

        int from, to;
        if (dir.x == 0) {
            from = startX + _cellSize;
            to = endX - _cellSize;
            int yPos = dir.y > 0 ? endY : startY;
            int placed = 0;
            for (int xPos = from; xPos <= to && placed < count; xPos += stepSize) {
                int randomPositionModifier = _random.NextInt(1, _cellsPerRoom - 1);
                int modifiedXPos = xPos + randomPositionModifier * _cellSize;
                ConstructConnectedSideGroup(dir, new(modifiedXPos, yPos));
                placed++;
            }
        }
        else {
            from = startY + _cellSize;
            to = endY - _cellSize;
            int xPos = dir.x > 0 ? endX : startX;
            int placed = 0;
            for (int yPos = from; yPos <= to && placed < count; yPos += stepSize) {
                int randomPositionModifier = _random.NextInt(1, _cellsPerRoom - 1);
                int modifiedYPos = yPos + randomPositionModifier * _cellSize;
                ConstructConnectedSideGroup(dir, new(xPos, modifiedYPos));
                placed++;
            }
        }
    }
    private void ConstructConnectedSideGroup(int2 dir, int2 pos) {
        int2 connecter = pos;
        int2 corridor = pos + (-dir * _cellSize);
        int2 room = pos + (dir * _sideRoomsGap / 2);

        if (!_graph.ContainsKey(connecter)) {
            _graph.Add(connecter, new(4, Allocator.Temp));
        }

        if (!_graph.ContainsKey(corridor)) {
            _graph.Add(corridor, new(4, Allocator.Temp));
        }

        if (!_graph.ContainsKey(room)) {
            _graph.Add(room, new(4, Allocator.Temp));
        }

        _roomsList.AddNoResize(room);
        _graph[connecter].Add(corridor);
        _graph[connecter].Add(room);
        _graph[room].Add(connecter);
        _graph[corridor].Add(connecter);
        _nodesForbidenForRemoval.Add(connecter);
        _nodesForbidenForRemoval.Add(room);
    }
    private void PlaceHallsInTheInnerZone() {

        if (_hallsCount == 0) {
            return;
        }

        int offsetToCenter = 2 * _cellSize;
        int2 hallsZoneFrom = new(_innerZone.From.x + offsetToCenter, _innerZone.From.y + offsetToCenter);
        int2 hallsZoneTo = new(_innerZone.To.x - offsetToCenter, _innerZone.To.y - offsetToCenter);

        Rect hallsZone = new(hallsZoneFrom, hallsZoneTo);
        NativeList<int2> availableNodes = GetAvailableNodes(hallsZone);
        NativeHashSet<int2> notAvailable = new(_graph.Count, Allocator.Temp);

        int placedHalls = 0;
        while (availableNodes.Length > 0 && placedHalls < _hallsCount) {
            int randomIndex = _random.NextInt(0, availableNodes.Length);
            int2 placementPosition = availableNodes[randomIndex];

            notAvailable.Add(placementPosition);
            availableNodes.RemoveAtSwapBack(randomIndex);

            if (IsRoomCanBePlaced(placementPosition, ref notAvailable)) {
                PlaceHallInInnerZoneAndConnectToGraph(placementPosition);
                placedHalls++;
            }
        }

    }
    private void PlaceHallInInnerZoneAndConnectToGraph(int2 pos) {

        _graph.Add(pos, new(4, Allocator.Temp));
        _hallsList.Add(pos);
        int randomDirIndex = _random.NextInt(0, _directions.Length + 1);
        bool roomIsPlaced = false;
        float additionalEntrProb = _additionalHallEntranceProbability;

        for (int i = 0; i < _directions.Length; i++) {
            int index = (i + randomDirIndex) % _directions.Length;
            int2 adjPos = _directions[index] * (2 * _cellSize) + pos;

            if (_graph.ContainsKey(adjPos)) {

                roomIsPlaced = true;
                _graph[pos].Add(adjPos);
                _graph[adjPos].Add(pos);
                _nodesForbidenForRemoval.Add(pos);
                _nodesForbidenForRemoval.Add(adjPos);

                if (_random.NextFloat() < additionalEntrProb) {
                    additionalEntrProb *= .75f;
                    continue;
                }

                return;
            }
        }

        if (!roomIsPlaced) {
            _graph[pos].Dispose();
            _graph.Remove(pos);
            UnityEngine.Debug.Log($"Cannot connect node in ({pos.x},{pos.y}) to graph.");
        }
    }
    private bool IsRoomCanBePlaced(int2 pos, ref NativeHashSet<int2> notAvailable) {
        NativeList<int2> nodesToRemove = new(9, Allocator.Temp);
        int2 start = new(pos.x - _cellSize, pos.y - _cellSize); //bottomleft of 3x3 zone
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                int2 node = new(start.x + i * _cellSize, start.y + j * _cellSize);
                if (!_graph.ContainsKey(node)) {
                    nodesToRemove.Dispose();
                    return false;
                }

                nodesToRemove.Add(node);
            }
        }
        RemoveNodes(nodesToRemove);

        foreach (int2 node in nodesToRemove) {
            notAvailable.Add(node);
        }

        nodesToRemove.Dispose();
        return true;
    }
    private bool IsConnectedAfterNodeRemoval(int2 node) {
        if (!_graph.ContainsKey(node) ||
            !_graph[node].IsCreated ||
            _graph[node].IsEmpty) {
            //UnityEngine.Debug.Log($"IsConnectedAfterNodeRemoval: Node ({node.x},{node.y}) not in graph.");
            return true;
        }

        var start = _graph[node][0];
        var q = new NativeQueue<int2>(Allocator.Temp);
        q.Enqueue(start);
        var visited = new NativeHashSet<int2>(_graph.Count, Allocator.Temp);

        while (q.Count > 0) {
            int n = q.Count;

            for (int i = 0; i < n; i++) {
                var cur = q.Dequeue();

                if (visited.Contains(cur)) {
                    continue;
                }

                visited.Add(cur);

                foreach (var next in _graph[cur]) {
                    if ((next.x != node.x || next.y != node.y) && !visited.Contains(next)) {
                        q.Enqueue(next);
                    }
                }
            }
        }

        bool result = visited.Count == _graph.Count - 1;
        visited.Dispose();
        q.Dispose();
        //UnityEngine.Debug.Log($"IsConnectedAfterNodeRemoval: is {result} after removing node ({node.x},{node.y}).");
        return result;
    }
    private NativeList<Edge> GetEdgesListAndDisposeGraph() {

        var edges = new NativeList<Edge>(Allocator.Temp);
        var nodes = new NativeList<int2>(_graph.Count, Allocator.Temp);

        foreach (var kvp in _graph) {
            nodes.Add(kvp.Key);
        }

        while (nodes.Length > 0) {
            int nodeIndex = _random.NextInt(0, nodes.Length);
            int2 node = nodes[nodeIndex];

            if (!_graph[node].IsEmpty) {
                int neigborIndex = _random.NextInt(0, _graph[node].Length);
                var edge = ExtractEdgeFromGraph(node, neigborIndex);
                edges.Add(edge);
            }
            else {
                nodes.RemoveAtSwapBack(nodeIndex);
                RemoveNode(node);
            }
        }

        return edges;
    }
    private void RemoveNodes(NativeList<int2> nodes) {
        foreach (int2 node in nodes) {
            RemoveNode(node);
        }
    }
    private void RemoveNode(int2 node) {
        foreach (var neigbor in _graph[node]) {
            RemoveSourceNodeFromNeigborsNodeList(node, neigbor);
        }
        _graph[node].Dispose();
        _graph.Remove(node);
    }
    private Edge ExtractEdgeFromGraph(int2 node, int neigborIndex) {
        var neighborNode = _graph[node][neigborIndex];
        _graph[node].RemoveAtSwapBack(neigborIndex);
        RemoveSourceNodeFromNeigborsNodeList(node, neighborNode);
        return new(node, neighborNode);
    }
    private void RemoveSourceNodeFromNeigborsNodeList(int2 node, int2 neigbor) {
        for (int i = 0; i < _graph[neigbor].Length; i++) {
            if (_graph[neigbor][i].Equals(node)) {
                _graph[neigbor].RemoveAtSwapBack(i);
                break;
            }
        }
    }
}