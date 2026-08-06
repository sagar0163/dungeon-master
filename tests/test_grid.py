"""
Grid system and pathfinding tests for Dungeon Lord.
Grid is 3D: (x, y, floor) with rooms, corridors, connections.
"""

import pytest
from collections import deque


class Grid3D:
    """Simple 3D grid for testing."""
    
    def __init__(self, width: int, height: int, floors: int):
        self.width = width
        self.height = height
        self.floors = floors
        # grid[floor][y][x] = cell_type or None
        self.grid = [[[None for _ in range(width)] for _ in range(height)] for _ in range(floors)]
        self.connections = {}  # (x, y, floor) -> [(nx, ny, nfloor), ...]
    
    def in_bounds(self, x: int, y: int, floor: int) -> bool:
        return 0 <= x < self.width and 0 <= y < self.height and 0 <= floor < self.floors
    
    def set_cell(self, x: int, y: int, floor: int, cell_type: str | None):
        if self.in_bounds(x, y, floor):
            self.grid[floor][y][x] = cell_type
    
    def get_cell(self, x: int, y: int, floor: int):
        if self.in_bounds(x, y, floor):
            return self.grid[floor][y][x]
        return None
    
    def add_connection(self, x: int, y: int, floor: int, nx: int, ny: int, nfloor: int):
        """Add bidirectional connection between adjacent cells."""
        key = (x, y, floor)
        nkey = (nx, ny, nfloor)
        if key not in self.connections:
            self.connections[key] = []
        if nkey not in self.connections:
            self.connections[nkey] = []
        self.connections[key].append(nkey)
        self.connections[nkey].append(key)
    
    def neighbors(self, x: int, y: int, floor: int):
        """Get walkable neighbors (4-directional + floor connections)."""
        for nx, ny, nfloor in self.connections.get((x, y, floor), []):
            if self.in_bounds(nx, ny, nfloor):
                cell = self.get_cell(nx, ny, nfloor)
                if cell in ("room", "corridor", "stairs", "entrance"):  # walkable types
                    yield (nx, ny, nfloor)


def a_star(grid: Grid3D, start: tuple, goal: tuple) -> list:
    """A* pathfinding on 3D grid. Returns list of (x, y, floor) or empty if no path."""
    from heapq import heappush, heappop
    
    def heuristic(a, b):
        # Manhattan distance + floor penalty
        return abs(a[0] - b[0]) + abs(a[1] - b[1]) + abs(a[2] - b[2]) * 10
    
    open_set = [(heuristic(start, goal), 0, start, [])]
    visited = set()
    
    while open_set:
        _, cost, current, path = heappop(open_set)
        
        if current == goal:
            return path + [current]
        
        if current in visited:
            continue
        visited.add(current)
        
        for neighbor in grid.neighbors(*current):
            if neighbor not in visited:
                new_cost = cost + 1
                heappush(open_set, (new_cost + heuristic(neighbor, goal), new_cost, neighbor, path + [current]))
    
    return []  # No path


class TestGrid3D:
    """Test 3D grid operations."""
    
    def test_grid_creation(self):
        grid = Grid3D(10, 10, 3)
        assert grid.width == 10
        assert grid.height == 10
        assert grid.floors == 3
    
    def test_set_get_cell(self):
        grid = Grid3D(5, 5, 2)
        grid.set_cell(2, 2, 0, "room")
        grid.set_cell(3, 2, 0, "corridor")
        assert grid.get_cell(2, 2, 0) == "room"
        assert grid.get_cell(3, 2, 0) == "corridor"
        assert grid.get_cell(0, 0, 0) is None
    
    def test_bounds_checking(self):
        grid = Grid3D(5, 5, 2)
        grid.set_cell(10, 10, 10, "room")  # Out of bounds
        assert grid.get_cell(0, 0, 0) is None
        assert grid.in_bounds(4, 4, 1) is True
        assert grid.in_bounds(5, 0, 0) is False
        assert grid.in_bounds(0, 0, 2) is False
    
    def test_connections_bidirectional(self):
        grid = Grid3D(5, 5, 1)
        grid.set_cell(0, 0, 0, "room")
        grid.set_cell(1, 0, 0, "room")
        grid.add_connection(0, 0, 0, 1, 0, 0)
        
        neighbors_0 = list(grid.neighbors(0, 0, 0))
        neighbors_1 = list(grid.neighbors(1, 0, 0))
        
        assert (1, 0, 0) in neighbors_0
        assert (0, 0, 0) in neighbors_1


class TestAStarPathfinding:
    """Test A* pathfinding on dungeon grid."""
    
    def test_simple_straight_path(self):
        """Path along a straight corridor."""
        grid = Grid3D(10, 3, 1)
        # Create corridor from (0,1) to (9,1)
        for x in range(10):
            grid.set_cell(x, 1, 0, "corridor")
            if x < 9:
                grid.add_connection(x, 1, 0, x + 1, 1, 0)
        
        path = a_star(grid, (0, 1, 0), (9, 1, 0))
        assert len(path) == 10
        assert path[0] == (0, 1, 0)
        assert path[-1] == (9, 1, 0)
    
    def test_path_around_obstacle(self):
        """Path should go around non-walkable cells."""
        grid = Grid3D(5, 5, 1)
        # Create room at center (blocked)
        grid.set_cell(2, 2, 0, "wall")
        # Corridor around it
        for x in range(5):
            grid.set_cell(x, 1, 0, "corridor")
            grid.set_cell(x, 3, 0, "corridor")
        for y in range(1, 4):
            grid.set_cell(1, y, 0, "corridor")
            grid.set_cell(3, y, 0, "corridor")
        
        # Connect corridors
        for x in range(4):
            grid.add_connection(x, 1, 0, x + 1, 1, 0)
            grid.add_connection(x, 3, 0, x + 1, 3, 0)
        for y in range(1, 3):
            grid.add_connection(1, y, 0, 1, y + 1, 0)
            grid.add_connection(3, y, 0, 3, y + 1, 0)
        
        path = a_star(grid, (0, 1, 0), (4, 1, 0))
        # Should go around via y=3 (or y=1 if direct)
        assert len(path) > 0
        # Path should not contain (2, 2, 0) - the wall
        assert (2, 2, 0) not in path
    
    def test_multi_floor_path(self):
        """Path between floors via stairs."""
        grid = Grid3D(5, 5, 2)
        # Floor 0 corridor
        for x in range(5):
            grid.set_cell(x, 2, 0, "corridor")
        # Floor 1 corridor
        for x in range(5):
            grid.set_cell(x, 2, 1, "corridor")
        # Stairs at (2, 2) connecting floors
        grid.set_cell(2, 2, 0, "stairs")
        grid.set_cell(2, 2, 1, "stairs")
        grid.add_connection(2, 2, 0, 2, 2, 1)
        
        # Connect corridors on each floor
        for x in range(4):
            grid.add_connection(x, 2, 0, x + 1, 2, 0)
            grid.add_connection(x, 2, 1, x + 1, 2, 1)
        
        path = a_star(grid, (0, 2, 0), (4, 2, 1))
        assert len(path) > 0
        # Should go through stairs at (2, 2)
        assert (2, 2, 0) in path
        assert (2, 2, 1) in path
    
    def test_no_path_returns_empty(self):
        """Unreachable goal returns empty path."""
        grid = Grid3D(5, 5, 1)
        grid.set_cell(0, 0, 0, "room")
        grid.set_cell(4, 4, 0, "room")
        # No connections
        
        path = a_star(grid, (0, 0, 0), (4, 4, 0))
        assert path == []


class TestDungeonStructure:
    """Test dungeon layout invariants."""
    
    def test_entrance_exists(self):
        """Dungeon must have an entrance on floor 0."""
        grid = Grid3D(10, 10, 3)
        grid.set_cell(5, 0, 0, "entrance")
        entrance_cells = [(x, y, f) for f in range(grid.floors) 
                         for y in range(grid.height) 
                         for x in range(grid.width) 
                         if grid.get_cell(x, y, f) == "entrance"]
        assert len(entrance_cells) >= 1
        assert entrance_cells[0][2] == 0  # Floor 0
    
    def test_core_exists(self):
        """Dungeon must have a core (final room)."""
        grid = Grid3D(10, 10, 3)
        grid.set_cell(5, 9, 2, "core")
        core_cells = [(x, y, f) for f in range(grid.floors)
                     for y in range(grid.height)
                     for x in range(grid.width)
                     if grid.get_cell(x, y, f) == "core"]
        assert len(core_cells) >= 1
    
    def test_all_rooms_reachable_from_entrance(self):
        """Every room/corridor should be reachable from entrance."""
        grid = Grid3D(5, 5, 1)
        grid.set_cell(0, 0, 0, "entrance")
        grid.set_cell(1, 0, 0, "corridor")
        grid.set_cell(2, 0, 0, "room")
        grid.add_connection(0, 0, 0, 1, 0, 0)
        grid.add_connection(1, 0, 0, 2, 0, 0)
        
        # BFS from entrance
        visited = set()
        queue = deque([(0, 0, 0)])
        while queue:
            current = queue.popleft()
            if current in visited:
                continue
            visited.add(current)
            for neighbor in grid.neighbors(*current):
                if neighbor not in visited:
                    queue.append(neighbor)
        
        # All walkable cells should be visited
        walkable = [(x, y, f) for f in range(grid.floors)
                   for y in range(grid.height)
                   for x in range(grid.width)
                   if grid.get_cell(x, y, f) in ("entrance", "corridor", "room")]
        
        for cell in walkable:
            assert cell in visited, f"Cell {cell} not reachable from entrance"


if __name__ == "__main__":
    pytest.main([__file__, "-v"])