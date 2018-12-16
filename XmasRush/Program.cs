using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
/**
* Help the Christmas elves fetch presents in a magical labyrinth!
**/

public class Position
{
    public static int PositionAllocation = 0;

    public int x;
    public int y;

    public static Position Allocate(int x, int y)
    {
        PositionAllocation++;
        return new Position(x, y);
    }

    protected Position() { }

    protected Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int DistanceTo(Position otherPosition)
    {
        return Math.Abs(this.x - otherPosition.x) + Math.Abs(this.y - otherPosition.y);
    }

    public override string ToString()
    {
        return $"Position ({x.ToString()},{y.ToString()})";
    }
}

public enum Direction
{
    UP = 0,
    RIGHT = 1,
    DOWN = 2,
    LEFT = 3,
}

public class Player : Position
{
    public static int PlayerAllocation = 0;

    private static Player[] playerPool = new Player[2000];

    static Player()
    {
        for (int i = 0; i < playerPool.Length; i++)
        {
            playerPool[i] = new Player();
        }
    }

    public static Player Allocate(int id, int x, int y, string tile)
    {
        var player = playerPool[PlayerAllocation++];

        player.id = id;
        player.x = x;
        player.y = y;
        player.tile = tile;

        return player;
    }

    public static void FreeAll()
    {
        PlayerAllocation = 0;
    }

    public int id;
    public string tile;

    public override string ToString()
    {
        return $"Player={id.ToString()} ({x},{y}) has tile {tile.ToString()}";
    }
}

public class Item : Position
{
    public static int ItemAllocation = 0;
    private static Item[] itemPool = new Item[50000];

    static Item()
    {
        for (int i = 0; i < itemPool.Length; i++)
        {
            itemPool[i] = new Item();
        }
    }

    public static void FreeAll()
    {
        ItemAllocation = 0;
    }

    public static Item Allocate(int x, int y, string itemName, int playerId, bool isInQuest)
    {
        var item = itemPool[ItemAllocation++];
        item.x = x;
        item.y = y;
        item.playerId = playerId;
        item.itemName = itemName;
        item.IsInQuest = isInQuest;

        return item;
    }

    public string itemName;
    public int playerId;
    public bool IsInQuest = false;

    public override string ToString()
    {
        return $"{itemName}[player{playerId.ToString()}] ({x},{y}) IsInQuest={IsInQuest.ToString()}";
    }
}

public class CellSet
{
    public static int CellSetAllocation = 0;
    private static CellSet[] pool = new CellSet[20_000];

    static CellSet()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = new CellSet() { cells = new bool[49] };
        }
    }

    public static CellSet Allocate()
    {
        var cellSet = pool[CellSetAllocation];

        for (int i = 0; i < 49; i++)
        {
            cellSet.cells[i] = false;
        }

        CellSetAllocation++;

        return cellSet;
    }

    public CellSet Assign(int x, int y)
    {
        cells[x + (y * Grid.Width)] = true;

        return this;
    }

    public static void FreeAll()
    {
        CellSetAllocation = 0;
    }

    public bool[] cells;

    public bool Contains(int x, int y)
    {
        return cells[x + (y * Grid.Width)];
    }

    public CellSet Union(CellSet otherSet)
    {
        CellSet newCellSet = CellSet.Allocate();

        for (int i = 0; i < 49; i++)
        {
            newCellSet.cells[i] = this.cells[i] || otherSet.cells[i];
        }

        return newCellSet;
    }

    public override string ToString()
    {
        string cellText = string.Join(",", this.cells.Select(c => c.ToString()).ToArray());

        return $"Cellset {cellText}";
    }

}

public class Grid
{
    public static Position Center = Position.Allocate(3, 3);


    public static int GridAllocation = 0;

    public static int Width = 7;
    public static int Heigth = 7;

    public readonly string[][] tiles;

    private readonly List<CellSet> _cellsets;

    public static readonly Direction[] AllDirections = new[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT };

    public readonly StringBuilder Hash;


    public Grid()
    {
        Grid.GridAllocation++;

        tiles = new string[Width][];

        for (int x = 0; x < Width; x++)
        {
            tiles[x] = new string[Heigth];
        }

        _cellsets = new List<CellSet>(Grid.Width * Grid.Heigth);
        Hash = new StringBuilder();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int y = 0; y < Grid.Heigth; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                sb.Append(tiles[x][y].ToString());
                sb.Append("|");
            }
            sb.AppendLine();
            sb.AppendLine(new string('-', 4 * Grid.Width));
        }
        return sb.ToString();
    }

    public void AddTile(int x, int y, string tile)
    {
        Hash.Append(tile);

        tiles[x][y] = tile;

        MakeSet(x, y);

        if (x != 0)
        {
            //Check left connection
            if (this.tiles[x][y][(int)Direction.LEFT] == '1' &&
                this.tiles[x - 1][y][(int)Direction.RIGHT] == '1')
            {
                CellSet c1 = FindSet(x, y);
                CellSet c2 = FindSet(x - 1, y);

                if (c1 != c2)
                {
                    Union(c1, c2);
                }
            }
        }

        if (y != 0)
        {
            //Check top connection
            if (this.tiles[x][y][(int)Direction.UP] == '1' &&
                this.tiles[x][y - 1][(int)Direction.DOWN] == '1')
            {
                CellSet c1 = FindSet(x, y);
                CellSet c2 = FindSet(x, y - 1);

                if (c1 != c2)
                {
                    Union(c1, c2);
                }
            }
        }
    }

    public bool ArePositionsConnected(Position pos1, Position pos2)
    {
        return FindSet(pos1.x, pos1.y) == FindSet(pos2.x, pos2.y);
    }

    private CellSet FindSet(int x, int y)
    {
        return _cellsets.Single(set => set.Contains(x, y));
    }

    public int CellSetSize(int x, int y)
    {
        return _cellsets.Single(set => set.Contains(x, y)).cells.Count(c => c);
    }

    private void Union(CellSet c1, CellSet c2)
    {
        CellSet newCellSet = c1.Union(c2);

        _cellsets.Remove(c1);
        _cellsets.Remove(c2);

        _cellsets.Add(newCellSet);
    }

    private void MakeSet(int x, int y)
    {
        _cellsets.Add(CellSet.Allocate().Assign(x, y));
    }

    public IReadOnlyList<Position> GetConnectedNeighbors(Position from)
    {
        var connectedNeighbors = new List<Position>();
        var fromTile = tiles[from.x][from.y];

        foreach (var direction in AllDirections)
        {
            if (fromTile[(int)direction] == '1')
            {
                var neighborPosition = GetSibling(from, direction);
                if (this.PositionIsValid(neighborPosition))
                {
                    var neighborTile = tiles[neighborPosition.x][neighborPosition.y];
                    var oppositeDirection = (Direction)(((int)direction + 2) % 4);
                    if (neighborTile[(int)oppositeDirection] == '1')
                    {
                        connectedNeighbors.Add(neighborPosition);
                    }
                }
            }
        }
        return connectedNeighbors;
    }

    private Position GetSibling(Position p, Direction direction)
    {
        int x = 0, y = 0;
        switch (direction)
        {
            case Direction.UP:
                x = p.x;
                y = p.y - 1;
                break;
            case Direction.RIGHT:
                x = p.x + 1;
                y = p.y;
                break;
            case Direction.DOWN:
                x = p.x;
                y = p.y + 1;
                break;
            case Direction.LEFT:
                x = p.x - 1;
                y = p.y;
                break;
        }

        return Position.Allocate(x, y);
    }

    private bool PositionIsValid(Position position)
    {
        return 0 <= position.x && position.x < Grid.Width &&
                0 <= position.y && position.y < Grid.Heigth;
    }


    public Item PushItem(int index, Direction direction, Item item)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushItemHorizontal(index, direction, item);
        else
            return PushItemVertical(index, direction, item);
    }

    private Item PushItemVertical(int index, Direction direction, Item item)
    {
        if (item.x < 0)
        {
            int newY = direction == Direction.DOWN ? 0 : Grid.Heigth - 1;
            return Item.Allocate(index, newY, item.itemName, item.playerId, item.IsInQuest);
        }
        else if (item.x != index)
        {
            return Item.Allocate(item.x, item.y, item.itemName, item.playerId, item.IsInQuest);
        }
        else
        {
            int newY = direction == Direction.DOWN ? item.y + 1 : item.y - 1;
            if (newY < 0 || newY == Grid.Heigth)
            {
                return Item.Allocate(-1, -1, item.itemName, item.playerId, item.IsInQuest);
            }
            return Item.Allocate(item.x, newY, item.itemName, item.playerId, item.IsInQuest);
        }
    }

    private Item PushItemHorizontal(int index, Direction direction, Item item)
    {
        if (item.y < 0)
        {
            int newX = direction == Direction.RIGHT ? 0 : Grid.Width - 1;
            return Item.Allocate(newX, index, item.itemName, item.playerId, item.IsInQuest);
        }
        else if (item.y != index)
        {
            return Item.Allocate(item.x, item.y, item.itemName, item.playerId, item.IsInQuest);
        }
        else
        {
            int newX = direction == Direction.RIGHT ? item.x + 1 : item.x - 1;
            if (newX < 0 || newX == Grid.Width)
            {
                return Item.Allocate(-1, -1, item.itemName, item.playerId, item.IsInQuest);
            }

            return Item.Allocate(newX, item.y, item.itemName, item.playerId, item.IsInQuest);
        }
    }

    public Player PushPlayer(int index, Direction direction, Player player, string playerTile)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushPlayerHorizontal(index, direction, player, playerTile);
        else
            return PushPlayerVertical(index, direction, player, playerTile);
    }

    private Player PushPlayerVertical(int index, Direction direction, Player player, string playerTile)
    {
        if (player.x != index)
        {
            return Player.Allocate(player.id, player.x, player.y, player.tile);
        }
        else
        {
            int newY = direction == Direction.DOWN ? player.y + 1 : player.y - 1;

            if (newY < 0)
            {
                newY = Grid.Heigth - 1;
            }
            if (newY >= Grid.Heigth)
            {
                newY = 0;
            }

            return Player.Allocate(player.id, player.x, newY, playerTile);
        }
    }

    private Player PushPlayerHorizontal(int index, Direction direction, Player player, string playerTile)
    {
        if (player.y != index)
        {
            return Player.Allocate(player.id, player.x, player.y, player.tile);
        }
        else
        {
            int newX = direction == Direction.RIGHT ? player.x + 1 : player.x - 1;
            if (newX < 0)
            {
                newX = Grid.Width - 1;
            }
            if (newX >= Grid.Width)
            {
                newX = 0;
            }
            return Player.Allocate(player.id, newX, player.y, playerTile);
        }
    }

    public Tuple<Grid, string> Push(int index, Direction direction, string tile)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushHorizontal(index, direction, tile);
        else
            return PushVertical(index, direction, tile);
    }

    private Tuple<Grid, string> PushHorizontal(int index, Direction direction, string tile)
    {
        Grid newGrid = new Grid();
        string outputTile = null;

        for (int y = 0; y < Grid.Heigth; y++)
        {
            if (y != index)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    newGrid.AddTile(x, y, this.tiles[x][y]);
                }
            }
            else
            {
                if (direction == Direction.LEFT)
                {
                    int x = 0;
                    for (; x < Grid.Width - 1; x++)
                    {
                        newGrid.AddTile(x, y, this.tiles[x + 1][y]);
                    }
                    newGrid.AddTile(x, y, tile);
                    outputTile = this.tiles[0][y];
                }
                else if (direction == Direction.RIGHT)
                {
                    newGrid.AddTile(0, y, tile);
                    for (int x = 1; x < Grid.Width; x++)
                    {
                        newGrid.AddTile(x, y, this.tiles[x - 1][y]);
                    }
                    outputTile = this.tiles[Grid.Width - 1][y];
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        return new Tuple<Grid, string>(newGrid, outputTile);
    }

    private Tuple<Grid, string> PushVertical(int index, Direction direction, string tile)
    {
        Grid newGrid = new Grid();
        string outputTile = null;

        for (int y = 0; y < Grid.Heigth; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                if (x != index)
                {
                    newGrid.AddTile(x, y, this.tiles[x][y]);
                }
                else
                {
                    if (direction == Direction.DOWN)
                    {
                        if (y == 0)
                        {
                            newGrid.AddTile(x, y, tile);
                        }
                        else
                        {
                            newGrid.AddTile(x, y, this.tiles[x][y - 1]);
                        }
                        outputTile = this.tiles[x][Grid.Heigth - 1];
                    }
                    else if (direction == Direction.UP)
                    {
                        if (y == Grid.Heigth - 1)
                        {
                            newGrid.AddTile(x, y, tile);
                        }
                        else
                        {
                            newGrid.AddTile(x, y, this.tiles[x][y + 1]);
                        }
                        outputTile = this.tiles[x][0];
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        return new Tuple<Grid, string>(newGrid, outputTile);
    }

}

public class GameState
{
    public readonly Grid grid;

    public readonly Player me;
    public readonly Player enemy;

    public readonly Item[] items;

    public int Depth = 0;

    public GameState(Grid grid, Player me, Player enemy, Item[] items)
    {
        this.grid = grid;
        this.me = me;
        this.enemy = enemy;
        this.items = items;
    }

    public Item[] GetItems(int playerId)
    {
        return items
            .Where(item => item.playerId == playerId)
            .ToArray();
    }

    public GameState RunCommand(PushCommand pushCommand)
    {
        var index = pushCommand.index;
        var direction = pushCommand.direction;
        var myTile = this.me.tile;

        var newGridAndOutputTile = grid.Push(index, direction, myTile);
        var newGrid = newGridAndOutputTile.Item1;
        var newPlayerTile = newGridAndOutputTile.Item2;
        var newMe = grid.PushPlayer(index, direction, this.me, newPlayerTile);
        var newEnemy = grid.PushPlayer(index, direction, this.enemy, this.enemy.tile);
        var newItems = this.items.Select(it => grid.PushItem(index, direction, it)).ToArray();

        return new GameState(newGrid, newMe, newEnemy, newItems) { Depth = this.Depth + 1 };

    }

    public int ComputeScore(GameState oldState)
    {
        int score = 0;
        Item[] myItems = items.Where(it => it.playerId == 0).ToArray();

        foreach (var item in myItems.Where(it => it.IsInQuest).ToArray())
        {
            if (item.x >= 0)
            {
                if (grid.ArePositionsConnected(this.me, item))
                {
                    //XmasRush.Debug($"Me: {this.me.ToString()} and item: {item.ToString()}");
                    score += 2000;
                    score += (20 - this.me.DistanceTo(item));
                }

                score += item.DistanceTo(Grid.Center);

            }
            if (item.x < 0)
            {
                score += 1000;
            }
        }

        return score;
    }
}

public struct PushCommand
{
    public static int PushCommandAllocation = 0;

    public int index;
    public Direction direction;

    public PushCommand(int index, Direction direction)
    {
        PushCommandAllocation++;

        this.index = index;
        this.direction = direction;
    }

    public override string ToString()
    {
        return $"PUSH {index.ToString()} {direction.ToString()}";
    }
}

public class PushAI
{
    private readonly GameState gameState;

    public PushAI(GameState gameState)
    {
        this.gameState = gameState;
    }

    public string ComputeCommand()
    {
        var watch = Stopwatch.StartNew();

        int bestScore = int.MinValue;
        PushCommand bestPushCommand = new PushCommand(0, Direction.DOWN);
        int simulations = 0;
        var pushCommands = XmasRush.AllPushCommands;
        
        for (int i = 0; i < pushCommands.Length && watch.ElapsedMilliseconds < 48; i++)
        {
            //XmasRush.Debug("****************************************");
            //XmasRush.Debug($"Evaluate {pushCommands[i].ToString()}");

            var newGameState1 = gameState.RunCommand(pushCommands[i]);
            simulations++;

            var score = newGameState1.ComputeScore(gameState);
            if (score > bestScore)
            {
                bestScore = score;
                bestPushCommand = pushCommands[i];
            }
        }

        XmasRush.Debug($"Nb simulations = {simulations.ToString()} {watch.ElapsedMilliseconds.ToString()}ms");

        return bestPushCommand.ToString();
    }

}

public class MoveAI
{

    private const int MaxMoveCount = 20;

    private struct PointValue
    {
        public int x;
        public int y;
        public PointValue(Position p)
        {
            this.x = p.x;
            this.y = p.y;
        }
        public bool AreSame(PointValue other)
        {
            return other.x == this.x && other.y == this.y;
        }

        public int DistanceTo(int x, int y)
        {
            return Math.Abs(this.x - x) + Math.Abs(this.y - y);
        }
    }

    private readonly GameState gameState;

    public MoveAI(GameState gameState)
    {
        this.gameState = gameState;
    }

    public string ComputeCommand()
    {
        Position myPosition = gameState.me;

        var grid = gameState.grid;
        var myItems = gameState.GetItems(0);

        var directions = new List<Direction>();

        //Start with connected items in quest on board
        var myNewPosition = CollectItems(myPosition, grid, myItems, ref directions);

        if (directions.Count < MoveAI.MaxMoveCount)
        {
            MoveToGridBorder(myNewPosition, ref directions);
        }

        if (directions.Count > 0)
        {
            var moves = string.Join(" ", directions.Take(MoveAI.MaxMoveCount).Select(x => x.ToString()));

            return $"MOVE {moves}";
        }
        else
        {
            return "PASS";
        }
    }

    private void MoveToGridBorder(Position myNewPosition, ref List<Direction> directions)
    {
        var cameFrom = ComputeBFS(fromPosition: myNewPosition);
        var targetPosition = cameFrom.OrderByDescending(kvp => kvp.Key.DistanceTo(Grid.Center.x, Grid.Center.y)).First();
        var path = ComputePath(
            from: new PointValue(myNewPosition),
            goal: targetPosition.Key,
            cameFrom: cameFrom);

        directions.AddRange(ComputeDirections(path));
    }

    private Position CollectItems(Position myPosition, Grid grid, Item[] myItems, ref List<Direction> directions)
    {
        var myConnectedItemsInQuest = myItems
                        .Where(it => it.IsInQuest == true && it.x >= 0 && grid.ArePositionsConnected(it, myPosition))
                        .ToList();

        while (directions.Count < MaxMoveCount && myConnectedItemsInQuest.Count > 0)
        {
            var cameFrom = ComputeBFS(fromPosition: myPosition);

            //Closest item
            PointValue[] shortestPath = new PointValue[100];
            Item closestItem = null;
            foreach (var item in myConnectedItemsInQuest)
            {
                var path = ComputePath(
                    from: new PointValue(myPosition),
                    goal: new PointValue(item),
                    cameFrom: cameFrom);
                if (path.Length < shortestPath.Length)
                {
                    shortestPath = path;
                    closestItem = item;
                }
            }

            directions.AddRange(ComputeDirections(shortestPath));

            var collectedItemPosition = shortestPath.Last();
            myConnectedItemsInQuest.Remove(closestItem);
            myPosition = Position.Allocate(collectedItemPosition.x, collectedItemPosition.y);
        }

        return myPosition;
    }

    private IReadOnlyList<Direction> ComputeDirections(PointValue[] path)
    {
        List<Direction> directions = new List<Direction>();

        if (path.Length < 2)
            return directions;

        for (int i = 1; i < path.Length; i++)
        {
            var direction = GetDirectionFrom(path[i - 1], path[i]);
            directions.Add(direction);
        }

        return directions;
    }

    private Direction GetDirectionFrom(PointValue from, PointValue to)
    {
        if (from.x == to.x - 1)
        {
            return Direction.RIGHT;
        }
        if (from.x == to.x + 1)
        {
            return Direction.LEFT;
        }
        if (from.y == to.y - 1)
        {
            return Direction.DOWN;
        }
        if (from.y == to.y + 1)
        {
            return Direction.UP;
        }
        throw new NotSupportedException();
    }

    private PointValue[] ComputePath(PointValue from, PointValue goal, Dictionary<PointValue, PointValue?> cameFrom)
    {
        PointValue? current = goal;
        var path = new List<PointValue>();

        while (current != null && current.Value.AreSame(from) == false)
        {
            path.Add(current.Value);
            current = cameFrom[current.Value];
        }

        path.Add(from);

        path.Reverse();

        return path.ToArray();
    }

    private Dictionary<PointValue, PointValue?> ComputeBFS(Position fromPosition)
    {

        var grid = gameState.grid;

        var cameFrom = new Dictionary<PointValue, PointValue?>();
        cameFrom[new PointValue(fromPosition)] = null;

        var frontier = new Queue<PointValue>();
        frontier.Enqueue(new PointValue(fromPosition));

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var currentPosition = Position.Allocate(current.x, current.y);

            var neigbors = grid.GetConnectedNeighbors(from: currentPosition)
                .Select(p => new PointValue(p))
                .ToArray();

            foreach (var next in neigbors)
            {
                if (cameFrom.ContainsKey(next) == false)
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        return cameFrom;
    }
}

public class XmasRush
{
    public static PushCommand[] AllPushCommands = null;

    private static void ComputeAllPossibleCommands()
    {
        List<PushCommand> commandList = new List<PushCommand>();
        for (int index = 0; index < 7; index++)
        {
            foreach (var direction in Grid.AllDirections)
            {
                commandList.Add(new PushCommand(index, direction));
            }
        }
        AllPushCommands = commandList.ToArray();
    }

    public static void GridTests()
    {
        Grid _grid;

        _grid = new Grid();

        _grid.AddTile(0, 0, "0110");
        _grid.AddTile(1, 0, "1001");
        _grid.AddTile(2, 0, "1011");
        _grid.AddTile(3, 0, "1111");
        _grid.AddTile(4, 0, "1010");
        _grid.AddTile(5, 0, "1011");
        _grid.AddTile(6, 0, "1010");
        _grid.AddTile(0, 1, "1110");
        _grid.AddTile(1, 1, "0111");
        _grid.AddTile(2, 1, "1001");
        _grid.AddTile(3, 1, "1010");
        _grid.AddTile(4, 1, "0110");
        _grid.AddTile(5, 1, "0101");
        _grid.AddTile(6, 1, "0111");
        _grid.AddTile(0, 2, "1101");
        _grid.AddTile(1, 2, "1101");
        _grid.AddTile(2, 2, "0110");
        _grid.AddTile(3, 2, "0111");
        _grid.AddTile(4, 2, "1010");
        _grid.AddTile(5, 2, "0110");
        _grid.AddTile(6, 2, "0110");
        _grid.AddTile(0, 3, "0111");
        _grid.AddTile(1, 3, "1001");
        _grid.AddTile(2, 3, "1010");
        _grid.AddTile(3, 3, "1010");
        _grid.AddTile(4, 3, "1010");
        _grid.AddTile(5, 3, "0110");
        _grid.AddTile(6, 3, "1101");
        _grid.AddTile(0, 4, "1001");
        _grid.AddTile(1, 4, "1001");
        _grid.AddTile(2, 4, "1010");
        _grid.AddTile(3, 4, "1101");
        _grid.AddTile(4, 4, "1001");
        _grid.AddTile(5, 4, "0111");
        _grid.AddTile(6, 4, "0111");
        _grid.AddTile(0, 5, "1101");
        _grid.AddTile(1, 5, "0101");
        _grid.AddTile(2, 5, "1001");
        _grid.AddTile(3, 5, "1010");
        _grid.AddTile(4, 5, "0110");
        _grid.AddTile(5, 5, "1101");
        _grid.AddTile(6, 5, "1011");
        _grid.AddTile(0, 6, "1010");
        _grid.AddTile(1, 6, "1110");
        _grid.AddTile(2, 6, "1010");
        _grid.AddTile(3, 6, "1111");
        _grid.AddTile(4, 6, "1110");
        _grid.AddTile(5, 6, "0110");
        _grid.AddTile(6, 6, "1001");

        var newGridAndOutputTile = _grid.Push(0, Direction.DOWN, "1000");
    }


    public static void Debug(string message)
    {
        Console.Error.WriteLine(message);
    }

    static void Main(string[] args)
    {
        XmasRush.ComputeAllPossibleCommands();

        //for (int i = 0; i < 10000; i++)
        //{
        //    GridTests();

        //    Item.FreeAll();
        //    CellSet.FreeAll();
        //    Player.FreeAll();
        //}

        string[] inputs;
        GameState gameState = null;
        int turnCount = 1;

        // game loop
        while (true)
        {
            int turnType = int.Parse(Console.ReadLine());

            Grid grid = new Grid();
            //XmasRush.Debug("Grid grid = new Grid();");
            for (int y = 0; y < 7; y++)
            {
                inputs = Console.ReadLine().Split(' ');
                for (int x = 0; x < 7; x++)
                {
                    string tile = inputs[x];
                    grid.AddTile(x, y, tile);

                    //XmasRush.Debug($@"grid.AddTile({x}, {y}, new Tile(""{tile}""));");
                }
            }

            Player[] players = new Player[2];

            for (int i = 0; i < 2; i++)
            {
                string line = Console.ReadLine();

                inputs = line.Split(' ');
                int numPlayerCards = int.Parse(inputs[0]); // the total number of quests for a player (hidden and revealed)
                int playerX = int.Parse(inputs[1]);
                int playerY = int.Parse(inputs[2]);
                string playerTile = inputs[3];

                players[i] = Player.Allocate(i, playerX, playerY, playerTile);
            }

            int numItems = int.Parse(Console.ReadLine()); // the total number of items available on board and on player tiles
            Item[] items = new Item[numItems];

            for (int i = 0; i < numItems; i++)
            {
                string line = Console.ReadLine();

                inputs = line.Split(' ');
                string itemName = inputs[0];
                int itemX = int.Parse(inputs[1]);
                int itemY = int.Parse(inputs[2]);
                int itemPlayerId = int.Parse(inputs[3]);

                items[i] = Item.Allocate(itemX, itemY, itemName, itemPlayerId, false);
            }

            int numQuests = int.Parse(Console.ReadLine()); // the total number of revealed quests for both players
            for (int i = 0; i < numQuests; i++)
            {
                string line = Console.ReadLine();

                inputs = line.Split(' ');
                string questItemName = inputs[0];
                int questPlayerId = int.Parse(inputs[1]);

                items.Single(it => it.itemName == questItemName && it.playerId == questPlayerId).IsInQuest = true;
            }

            Stopwatch watch = Stopwatch.StartNew();
            gameState = new GameState(grid, players[0], players[1], items);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            if (turnType == 0)
            {
                //Push turn
                //XmasRush.Debug("Push turn");

                PushAI pushAi = new PushAI(gameState);
                Console.WriteLine(pushAi.ComputeCommand()); // PUSH <id> <direction> | MOVE <direction> | PASS
                //XmasRush.Debug($"PushAI {watch.ElapsedMilliseconds.ToString()} ms");
            }
            else
            {
                //Move turn
                //XmasRush.Debug("Move turn");

                MoveAI moveAI = new MoveAI(gameState);
                Console.WriteLine(moveAI.ComputeCommand());
                //XmasRush.Debug($"MoveAI {watch.ElapsedMilliseconds.ToString()} ms");
            }

            //XmasRush.Debug($"Position allocation:{Position.PositionAllocation.ToString()}");
            //XmasRush.Debug($"Player allocation:{Player.PlayerAllocation.ToString()}");
            //XmasRush.Debug($"Item allocation:{Item.ItemAllocation.ToString()}");
            //XmasRush.Debug($"Grid allocation:{Grid.GridAllocation.ToString()}");
            //XmasRush.Debug($"GridCellSet allocation:{CellSet.CellSetAllocation.ToString()}");
            //XmasRush.Debug($"PushCommand allocation:{PushCommand.PushCommandAllocation.ToString()}");

            Item.FreeAll();
            CellSet.FreeAll();
            Player.FreeAll();

            turnCount++;
        }
    }
}