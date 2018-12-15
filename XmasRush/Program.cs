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
    public int x;
    public int y;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int DistanceTo(Position otherPosition)
    {
        return Math.Abs(this.x - otherPosition.x) + Math.Abs(this.y - otherPosition.y);
    }

    public Position GetSibling(Direction direction)
    {
        int x = 0, y = 0;
        switch (direction)
        {
            case Direction.UP:
                x = this.x;
                y = this.y - 1;
                break;
            case Direction.RIGHT:
                x = this.x + 1;
                y = this.y;
                break;
            case Direction.DOWN:
                x = this.x;
                y = this.y + 1;
                break;
            case Direction.LEFT:
                x = this.x - 1;
                y = this.y;
                break;
        }

        return new Position(x, y);
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
    public readonly int id;
    public readonly Tile tile;

    public Player(int id, int x, int y, Tile tile) : base(x, y)
    {
        this.id = id;
        this.tile = tile;
    }

    public Player(Player player) : base(player.x, player.y)
    {
        this.id = player.id;
        this.tile = player.tile;
    }

    public override string ToString()
    {
        return $"Player={id.ToString()} ({x},{y}) has tile {tile.ToString()}";
    }
}

public class Item : Position
{
    public readonly string itemName;
    public readonly int playerId;

    public bool IsInQuest = false;

    public Item(int x, int y, string itemName, int playerId) : base(x, y)
    {
        this.itemName = itemName;
        this.playerId = playerId;
    }

    public override string ToString()
    {
        return $"{itemName}[player{playerId.ToString()}] ({x},{y}) IsInQuest={IsInQuest.ToString()}";
    }
}

public struct Tile
{
    public readonly string directions;
    public Tile(string tile)
    {
        directions = tile;
    }

    public bool IsOpenedTo(Direction direction)
    {
        return directions[(int)direction] == '1';
    }

    public override string ToString()
    {
        return directions;
    }
}

public class Grid
{

    private class CellSet
    {
        public HashSet<int> cells;
        private CellSet()
        {
            cells = new HashSet<int>();
        }

        public CellSet(int x, int y) : this()
        {
            cells.Add(x + (y * Grid.Width));
        }

        public int GetId()
        {
            return cells.Min();
        }

        public bool Contains(int x, int y)
        {
            return cells.Contains(x + (y * Grid.Width));
        }

        public CellSet Union(CellSet otherSet)
        {
            CellSet newCellSet = new CellSet();
            foreach (var cell in this.cells)
            {
                newCellSet.cells.Add(cell);
            }
            foreach (var cell in otherSet.cells)
            {
                newCellSet.cells.Add(cell);
            }
            return newCellSet;
        }

        public override string ToString()
        {
            string cellText = string.Join(",", this.cells.Select(c => c.ToString()).ToArray());

            return $"{this.GetId()}={cellText}";
        }

    }

    public static int Width = 7;
    public static int Heigth = 7;

    public static Position Center = new Position(3, 3);

    public readonly Tile[][] tiles;

    private readonly List<CellSet> _cellsets;

    public static readonly Direction[] AllDirections = new[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT };

    public int CellSetCount => _cellsets.Count;

    public readonly StringBuilder Hash;


    public Grid()
    {
        tiles = new Tile[Width][];

        for (int x = 0; x < Width; x++)
        {
            tiles[x] = new Tile[Heigth];
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

    public void AddTile(int x, int y, Tile tile)
    {
        Hash.Append(tile.directions);

        tiles[x][y] = tile;
        
        MakeSet(x, y);

        if (x != 0)
        {
            //Check left connection
            if (this.tiles[x][y].IsOpenedTo(Direction.LEFT) && this.tiles[x - 1][y].IsOpenedTo(Direction.RIGHT))
            {
                CellSet c1 = FindSet(x, y);
                CellSet c2 = FindSet(x - 1, y);

                if (c1.GetId() != c2.GetId())
                {
                    Union(c1, c2);
                }
            }
        }

        if (y != 0)
        {
            //Check top connection
            if (this.tiles[x][y].IsOpenedTo(Direction.UP) && this.tiles[x][y - 1].IsOpenedTo(Direction.DOWN))
            {
                CellSet c1 = FindSet(x, y);
                CellSet c2 = FindSet(x, y - 1);

                if (c1.GetId() != c2.GetId())
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
        return _cellsets.Single(set => set.Contains(x,y));
    }

    public int CellSetSize(int x, int y)
    {
        return _cellsets.Single(set => set.Contains(x, y)).cells.Count;
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
        _cellsets.Add(new CellSet(x,y));
    }

    public IReadOnlyList<Position> GetConnectedNeighbors(Position from)
    {
        var connectedNeighbors = new List<Position>();
        var fromTile = tiles[from.x][from.y];

        foreach (var direction in AllDirections)
        {
            if (fromTile.IsOpenedTo(direction))
            {
                var neighborPosition = from.GetSibling(direction);
                if (this.PositionIsValid(neighborPosition))
                {
                    var neighborTile = tiles[neighborPosition.x][neighborPosition.y];
                    var oppositeDirection = (Direction)(((int)direction + 2) % 4);
                    if (neighborTile.IsOpenedTo(oppositeDirection))
                    {
                        connectedNeighbors.Add(neighborPosition);
                    }
                }
            }
        }
        return connectedNeighbors;
    }

    private bool PositionIsValid(Position position)
    {
        return 0 <= position.x && position.x < Grid.Width &&
                0 <= position.y && position.y < Grid.Heigth;
    }

    public Tuple<Grid, Tile> Push(int index, Direction direction, Tile tile)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushHorizontal(index, direction, tile);
        else
            return PushVertical(index, direction, tile);
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
            return new Item(index, newY, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
        else if (item.x != index)
        {
            return new Item(item.x, item.y, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
        else
        {
            int newY = direction == Direction.DOWN ? item.y + 1 : item.y - 1;
            if (newY < 0 || newY == Grid.Heigth)
            {
                return new Item(-1, -1, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
            }
            return new Item(item.x, newY, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
    }

    private Item PushItemHorizontal(int index, Direction direction, Item item)
    {
        if (item.y < 0)
        {
            int newX = direction == Direction.RIGHT ? 0 : Grid.Width - 1;
            return new Item(newX, index, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
        else if (item.y != index)
        {
            return new Item(item.x, item.y, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
        else
        {
            int newX = direction == Direction.RIGHT ? item.x + 1 : item.x - 1;
            if (newX < 0 || newX == Grid.Width)
            {
                return new Item(-1, -1, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
            }

            return new Item(newX, item.y, item.itemName, item.playerId) { IsInQuest = item.IsInQuest };
        }
    }

    public Player PushPlayer(int index, Direction direction, Player player, Tile playerTile)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushPlayerHorizontal(index, direction, player, playerTile);
        else
            return PushPlayerVertical(index, direction, player, playerTile);
    }

    private Player PushPlayerVertical(int index, Direction direction, Player player, Tile playerTile)
    {
        if (player.x != index)
        {
            return new Player(player);
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

            return new Player(player.id, player.x, newY, playerTile);
        }
    }

    private Player PushPlayerHorizontal(int index, Direction direction, Player player, Tile playerTile)
    {
        if (player.y != index)
        {
            return new Player(player);
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
            return new Player(player.id, newX, player.y, playerTile);
        }
    }

    private Tuple<Grid, Tile> PushHorizontal(int index, Direction direction, Tile tile)
    {
        Grid newGrid = new Grid();
        Tile? outputTile = null;

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
        return new Tuple<Grid, Tile>(newGrid, outputTile.Value);
    }

    private Tuple<Grid, Tile> PushVertical(int index, Direction direction, Tile tile)
    {
        Grid newGrid = new Grid();
        Tile? outputTile = null;

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

        return new Tuple<Grid, Tile>(newGrid, outputTile.Value);
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
    public int index;
    public Direction direction;
    public string Hash;

    public PushCommand(int index, Direction direction)
    {
        this.index = index;
        this.direction = direction;
        Hash = this.index.ToString() + this.direction.ToString();
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
        var grid = gameState.grid;
        var me = gameState.me;
        var enemy = gameState.enemy;
        var myItems = gameState.GetItems(playerId: 0);
        var myTile = me.tile;

        int bestScore = 0;

        Random rand = new Random();
        PushCommand bestPushCommand = new PushCommand(rand.Next(0, 6), (Direction)rand.Next(0, 4));

        PushCommand[] pushCommands = XmasRush.ComputeAllPossibleCommands().ToArray();

        for(int i = 0; i < pushCommands.Length; i++)
        {
            var pushCommand1 = pushCommands[i];
       
            //XmasRush.Debug("****************************************");
            //XmasRush.Debug($"Evaluate {pushCommand1.ToString()}");

            if(i == 0)
            {
                var hash = GameStateStore.ComputeStoreHash(gameState, pushCommand1);
                XmasRush.Debug(hash);
                XmasRush.Debug($"found hash = {GameStateStore.HasState(gameState, pushCommand1)}");
            }

            var newGameState1 = gameState.RunCommand(pushCommand1);
            var score = newGameState1.ComputeScore(gameState);

            //XmasRush.Debug($"score: {score.ToString()}");

            if (score > bestScore)
            {
                bestScore = score;
                bestPushCommand = pushCommand1;
            }
        }

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
            myPosition = new Position(collectedItemPosition.x, collectedItemPosition.y);
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
            var currentPosition = new Position(current.x, current.y);

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

public static class GameStateStore
{
    public static readonly Dictionary<string, GameState> store = new Dictionary<string, GameState>();

    public static void InitializeStateStore(GameState gameState)
    {
        Stopwatch watch = Stopwatch.StartNew();

        var allPossibleCommands = XmasRush.ComputeAllPossibleCommands();

        Queue<GameState> toVisit = new Queue<GameState>();
        toVisit.Enqueue(gameState);

        Queue<string> hashes = new Queue<string>();

        int maxDepth = 0;

        while (toVisit.Count > 0 && watch.ElapsedMilliseconds < 950)
        {
            var curGameState = toVisit.Dequeue();

            foreach (var command in allPossibleCommands)
            {
                var hash = ComputeStoreHash(curGameState, command);
                if (store.ContainsKey(hash) == false)
                {
                    var newGameState = curGameState.RunCommand(command);
                    hashes.Enqueue(hash);
                    store.Add(hash, newGameState);

                    if(maxDepth < newGameState.Depth)
                    {
                        maxDepth = newGameState.Depth;
                    }

                    toVisit.Enqueue(newGameState);
                }
            }
        }
        XmasRush.Debug($"Max depth reached: {maxDepth.ToString()}");
        XmasRush.Debug($"Store size: {store.Count}, {watch.ElapsedMilliseconds.ToString()}");

    }

    public static bool HasState(GameState state, PushCommand command)
    {
        var hash = ComputeStoreHash(state, command);
        return GameStateStore.store.ContainsKey(hash);
    }

    public static string ComputeStoreHash(GameState gameState, PushCommand command)
    {
        return command.Hash + gameState.grid.Hash.ToString() ;
    }
}

public class XmasRush
{
    public static List<PushCommand> ComputeAllPossibleCommands()
    {
        List<PushCommand> commands = new List<PushCommand>();

        for (int index = 0; index < 7; index++)
        {
            foreach (var direction in Grid.AllDirections)
            {
                commands.Add(new PushCommand(index, direction));
            }
        }

        return commands;
    }

    public static void Debug(string message)
    {
        Console.Error.WriteLine(message);
    }

    static void Main(string[] args)
    {
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
                    grid.AddTile(x, y, new Tile(tile));

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

                players[i] = new Player(i, playerX, playerY, new Tile(playerTile));
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

                items[i] = new Item(itemX, itemY, itemName, itemPlayerId);
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

            if (turnCount == 1)
            {
                GameStateStore.InitializeStateStore(gameState);
            }
            
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            if (turnType == 0)
            {
                //Push turn
                PushAI pushAi = new PushAI(gameState);
                Console.WriteLine(pushAi.ComputeCommand()); // PUSH <id> <direction> | MOVE <direction> | PASS
                //XmasRush.Debug($"PushAI {watch.ElapsedMilliseconds.ToString()} ms");
            }
            else
            {
                //Move turn
                MoveAI moveAI = new MoveAI(gameState);
                Console.WriteLine(moveAI.ComputeCommand());
                //XmasRush.Debug($"MoveAI {watch.ElapsedMilliseconds.ToString()} ms");
            }

            turnCount++;
        }
    }
}