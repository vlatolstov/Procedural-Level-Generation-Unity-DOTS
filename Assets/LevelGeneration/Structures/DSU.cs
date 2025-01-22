using Unity.Collections;

public struct DSU {
    private NativeArray<int> _set;
    private NativeArray<int> _rank;

    public DSU(int size, Allocator allocator) {
        _set = new(size, allocator);
        _rank = new(size, allocator);

        for (int i = 0; i < size; i++) {
            _set[i] = i;
            _rank[i] = 0;
        }
    }

    public int Find(int node) {
        return _set[node] == node ? node : _set[node] = Find(_set[node]);
    }

    public bool Union(int a, int b) {
        int parrentA = Find(a), parrentB = Find(b);

        if (parrentA == parrentB) {
            return false;
        }
        else {
            if (_rank[parrentA] < _rank[parrentB]) {
                _set[parrentA] = parrentB;
            }
            else if (_rank[parrentB] < _rank[parrentA]) {
                _set[parrentB] = parrentA;
            }
            else {
                _set[parrentA] = parrentB;
                _rank[parrentB]++;
            }

            return true;
        }
    }
}
