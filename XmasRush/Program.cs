using System;
using System.Collections.Generic;
using System.Linq;
/**
 * Help the Christmas elves fetch presents in a magical labyrinth!
 **/

class Position
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

enum Direction
{
    UP = 0,
    RIGHT = 1,
    DOWN = 2,
    LEFT = 3,
}

class Player : Position
{
    private readonly int id;
    public readonly Tile tile;

    public Player(int id, int x, int y, Tile tile) : base(x, y)
    {
        this.id = id;
        this.tile = tile;
    }
}

class Item : Position
{
    public readonly string itemName;
    public readonly int playerId;

    public Item(int x, int y, string itemName, int playerId) : base(x, y)
    {
        this.itemName = itemName;
        this.playerId = playerId;
    }
}

class Quest
{
    public readonly string itemName;
    public readonly int playerId;

    public Quest(string itemName, int playerId)
    {
        this.itemName = itemName;
        this.playerId = playerId;
    }
}

struct Tile
{
    private readonly string _directions;
    public Tile(string tile)
    {
        _directions = tile;
    }

    public bool IsOpenedTo(Direction direction)
    {
        return _directions[(int)direction] == '1';
    }
}

class Grid
{
    private struct Cell
    {
        public int x, y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int Id => this.x + (this.y * Grid.Width);

        public override string ToString()
        {
            return $"({x.ToString()},{y.ToString()})";
        }
    }

    private class CellSet
    {
        public HashSet<Cell> cells;
        private CellSet()
        {
            cells = new HashSet<Cell>();
        }

        public CellSet(Cell cell) : this()
        {
            cells.Add(cell);
        }

        public int GetId()
        {
            return cells.Min(c => c.Id);
        }

        public bool Contains(Cell cell)
        {
            return cells.Contains(cell);
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

    private readonly Tile[][] _tiles;

    private readonly List<CellSet> _cellsets;

    public static readonly Direction[] AllDirections = new[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT };

    public int CellSetCount => _cellsets.Count;

    public Grid()
    {
        _tiles = new Tile[Width][];

        for (int x = 0; x < Width; x++)
        {
            _tiles[x] = new Tile[Heigth];
        }

        _cellsets = new List<CellSet>(Grid.Width * Grid.Heigth);
    }

    public void AddTile(int x, int y, Tile tile)
    {
        _tiles[x][y] = tile;

        var cell = new Cell(x, y);
        MakeSet(cell);

        if (x != 0)
        {
            //Check left connection
            if (this._tiles[x][y].IsOpenedTo(Direction.LEFT) && this._tiles[x - 1][y].IsOpenedTo(Direction.RIGHT))
            {
                CellSet c1 = FindSet(new Cell(x, y));
                CellSet c2 = FindSet(new Cell(x - 1, y));

                if (c1.GetId() != c2.GetId())
                {
                    Union(c1, c2);
                }
            }
        }

        if (y != 0)
        {
            //Check top connection
            if (this._tiles[x][y].IsOpenedTo(Direction.UP) && this._tiles[x][y - 1].IsOpenedTo(Direction.DOWN))
            {
                CellSet c1 = FindSet(new Cell(x, y));
                CellSet c2 = FindSet(new Cell(x, y - 1));

                if (c1.GetId() != c2.GetId())
                {
                    Union(c1, c2);
                }
            }
        }
    }

    public bool ArePositionsConnected(Position pos1, Position pos2)
    {
        Cell c1 = new Cell(pos1.x, pos1.y);
        Cell c2 = new Cell(pos2.x, pos2.y);
        return FindSet(c1) == FindSet(c2);
    }

    private CellSet FindSet(Cell cell)
    {
        return _cellsets.Single(set => set.Contains(cell));
    }
    
    private void Union(CellSet c1, CellSet c2)
    {
        CellSet newCellSet = c1.Union(c2);

        _cellsets.Remove(c1);
        _cellsets.Remove(c2);

        _cellsets.Add(newCellSet);
    }

    private void MakeSet(Cell cell)
    {
        _cellsets.Add(new CellSet(cell));
    }

    public IReadOnlyList<Direction> GetPossibleDirections(Position from)
    {
        List<Direction> possibleDirections = new List<Direction>(4);

        foreach (var direction in AllDirections)
        {
            var siblingPosition = from.GetSibling(direction);

            if (PositionIsValid(siblingPosition))
            {
                if (FindSet(new Cell(from.x, from.y)) ==
                    FindSet(new Cell(siblingPosition.x, siblingPosition.y)))
                {
                    possibleDirections.Add(direction);
                }
            }

        }

        return possibleDirections;
    }

    private bool PositionIsValid(Position position)
    {
        return 0 <= position.x && position.x < Grid.Width &&
                0 <= position.y && position.y < Grid.Heigth;
    }

    public void DumpDisjoinSets()
    {
        foreach (var cellSet in _cellsets)
        {
            XmasRush.Debug(cellSet.ToString());
        }
    }

    public Grid Push(int index, Direction direction, Tile tile)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushHorizontal(index, direction, tile);
        else
            return PushVertical(index, direction, tile);
    }

    public Position PushItem(int index, Direction direction, Position itemPosition)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushItemHorizontal(index, direction, itemPosition);
        else
            return PushItemVertical(index, direction, itemPosition);
    }

    private Position PushItemVertical(int index, Direction direction, Position itemPosition)
    {
        if (itemPosition.x != index || itemPosition.x < 0 )
        {
            return itemPosition;
        }
        else
        {
            int newY = direction == Direction.DOWN ? itemPosition.y + 1 : itemPosition.y - 1;
            if(newY < 0 || newY == Grid.Heigth)
            {
                return new Position(-1, -1);
            }
            return new Position(x: itemPosition.x, y: newY );
        }
    }

    private Position PushItemHorizontal(int index, Direction direction, Position itemPosition)
    {
        if (itemPosition.y != index || itemPosition.y < 0)
        {
            return itemPosition;
        }
        else
        {
            int newX = direction == Direction.RIGHT ? itemPosition.x + 1 : itemPosition.x - 1;
            if(newX < 0 || newX == Grid.Width)
            {
                return new Position(-1, -1);
            }

            return new Position(x: newX , y: itemPosition.y);
        }
    }
    
    public Position PushPlayer(int index, Direction direction, Position playerPosition)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
            return PushPlayerHorizontal(index, direction, playerPosition);
        else
            return PushPlayerVertical(index, direction, playerPosition);
    }

    private Position PushPlayerVertical(int index, Direction direction, Position playerPosition)
    {
        if(playerPosition.x != index)
        {
            return playerPosition;
        }
        else
        {
            int newY = direction == Direction.DOWN ? playerPosition.y + 1 : playerPosition.y - 1;

            if(newY < 0)
            {
                newY = Grid.Heigth - 1;
            }
            if(newY >= Grid.Heigth)
            {
                newY = 0;
            }

            return new Position(x: playerPosition.x, y: newY);
        }
    }

    private Position PushPlayerHorizontal(int index, Direction direction, Position playerPosition)
    {
        if(playerPosition.y != index)
        {
            return playerPosition;
        }
        else
        {
            int newX = direction == Direction.RIGHT ? playerPosition.x + 1 : playerPosition.x - 1;
            if(newX < 0)
            {
                newX = Grid.Width - 1;
            }
            if(newX >= Grid.Width)
            {
                newX = 0;
            }
            return new Position(x: newX, y: playerPosition.y);
        }
    }

    private Grid PushHorizontal(int index, Direction direction, Tile tile)
    {
        Grid newGrid = new Grid();

        for(int y = 0; y < Grid.Heigth; y++)
        {
            if(y != index)
            {
                for(int x = 0; x < Grid.Width; x++)
                {
                    newGrid.AddTile(x, y, this._tiles[x][y]);
                }
            }
            else
            {
                if(direction == Direction.LEFT)
                {
                    int x = 0;
                    for(; x < Grid.Width - 1;  x++)
                    {
                        newGrid.AddTile(x, y, this._tiles[x + 1][y]);
                    }
                    newGrid.AddTile(x, y, tile);
                }
                else if(direction == Direction.RIGHT)
                {
                    newGrid.AddTile(0, y, tile);
                    for (int x = 1; x < Grid.Width; x++)
                    {
                        newGrid.AddTile(x, y, this._tiles[x - 1][y]);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        return newGrid;
    }

    private Grid PushVertical(int index, Direction direction, Tile tile)
    {
        Grid newGrid = new Grid();

        for(int y = 0; y < Grid.Heigth; y++)
        {
            for(int x = 0; x < Grid.Width; x++)
            {
                if (x != index)
                {
                    newGrid.AddTile(x, y, this._tiles[x][y]);
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
                            newGrid.AddTile(x, y, this._tiles[x][y-1]);
                        }
                    }
                    else if (direction == Direction.UP)
                    {
                        if (y == Grid.Heigth - 1)
                        {
                            newGrid.AddTile(x, y, tile);
                        }
                        else
                        {
                            newGrid.AddTile(x, y, this._tiles[x][y + 1]);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        return newGrid;
    }
}

class GameState
{
    public readonly Grid grid;

    public readonly Player me;
    public readonly Player enemy;

    public readonly Quest[] quests;
    public readonly Item[] items;

    public GameState(Grid grid, Player me, Player enemy, Quest[] quests, Item[] items)
    {
        this.grid = grid;
        this.me = me;
        this.enemy = enemy;
        this.quests = quests;
        this.items = items;
    }

    public Item[] GetRevealedItems(int playerId)
    {
        var revealedItems = quests.Where(q => q.playerId == playerId).Select(x => x.itemName).ToHashSet();

        return items.Where(item => item.playerId == playerId && revealedItems.Contains(item.itemName)).ToArray();
    }
}

class PushAI
{
    private readonly GameState gameState;
    private struct PushCommand
    {
        public int index;
        public Direction direction;
        public PushCommand(int index, Direction direction)
        {
            this.index = index;
            this.direction = direction;
        }

        public override string ToString()
        {
            return $"PUSH {index.ToString()} {direction.ToString()}";
        }
    }
    public PushAI(GameState gameState)
    {
        this.gameState = gameState;
    }

    public string ComputeCommand()
    {
        var grid = gameState.grid;
        var me = gameState.me;
        var enemy = gameState.enemy;

        var myItems = gameState.GetRevealedItems(0);
        var enemyItems = gameState.GetRevealedItems(1);

        var myTile = me.tile;

        int bestScore = 0;
        Random rand = new Random();
        PushCommand bestPushCommand = new PushCommand(rand.Next(0, 6), (Direction)rand.Next(0,4));
        
        for (int i = 0; i < 7; i++)
        {
            foreach(var direction in Grid.AllDirections)
            {
                PushCommand commandUnderTest = new PushCommand(i, direction);
                int score = 0;

                var newGrid = grid.Push(i, direction, myTile);
                var newMyPlayerPosition = grid.PushPlayer(i, direction, me);
                var newMyItemPositions = myItems.Select( it => grid.PushItem(i, direction, it)).ToArray();

                var newEnemyPosition = grid.PushPlayer(i, direction, enemy);
                var newEnemyItemPositions = enemyItems.Select(it => grid.PushItem(i, direction, it)).ToArray();

                foreach (var newNewItemPosition in newMyItemPositions)
                {
                    if (newNewItemPosition.x >= 0)
                    {
                        if (newGrid.ArePositionsConnected(newMyPlayerPosition, newNewItemPosition))
                        {
                            score += 1000;
                        }
                    }
                }

                foreach(var newEnemyItemPosition in newEnemyItemPositions)
                {
                    if (newEnemyItemPosition.x >= 0)
                    {
                        if (newGrid.ArePositionsConnected(newEnemyItemPosition, newEnemyItemPosition))
                        {
                            score -= 1000;
                        }
                    }
                }
                
                if(score > bestScore)
                {
                    bestScore = score;
                    bestPushCommand = commandUnderTest;
                }
            }
        }

        return bestPushCommand.ToString();

    }
}

class MoveAI
{
    private readonly GameState gameState;

    public MoveAI(GameState gameState)
    {
        this.gameState = gameState;
    }

    public string ComputeCommand()
    {
        Position myPosition = gameState.me;
        var grid = gameState.grid;
        var myItems = gameState.GetRevealedItems(0).OrderBy(it => it.DistanceTo(myPosition)).ToArray();

        List<Direction> directions = new List<Direction>();
        int moveCount = 0;

        var visited = new HashSet<Tuple<int, int>>
        {
            new Tuple<int, int>(myPosition.x, myPosition.y)
        };

        var frontier = new Queue<Tuple<int, int>>();

        int lowestDistance = 20;
        frontier.Enqueue(new Tuple<int, int>(myPosition.x, myPosition.y));

        while (frontier.Count > 0)
        {
            var p = frontier.Dequeue();
            var currentPosition = new Position(p.Item1, p.Item2);

            var possibleDirections = grid.GetPossibleDirections(from: currentPosition);

            Direction? bestDirection = null;
            Position bestNextPosition = currentPosition;

            foreach (var direction in possibleDirections)
            {
                var siblingPosition = currentPosition.GetSibling(direction);
                var positionValue = new Tuple<int, int>(siblingPosition.x, siblingPosition.y);
                if (visited.Contains(positionValue))
                {
                    continue;
                }

                visited.Add(positionValue);
                frontier.Enqueue(positionValue);

                var distance = siblingPosition.DistanceTo(myItems[0]);

                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    bestDirection = direction;
                    bestNextPosition = siblingPosition;
                }
            }

            if (bestDirection != null)
            {
                currentPosition = bestNextPosition;
                directions.Add(bestDirection.Value);
            }

            moveCount++;
        }

        if (directions.Count > 0)
        {
            var moves = string.Join(" ", directions.Take(20).Select(x => x.ToString()));

            return $"MOVE {moves}";
        }
        else
        {
            return "PASS";
        }
    }

}

class XmasRush
{
    public static void Debug(string message)
    {
        Console.Error.WriteLine(message);
    }

    static void Main(string[] args)
    {
        string[] inputs;

        // game loop
        while (true)
        {
            int turnType = int.Parse(Console.ReadLine());

            Grid grid = new Grid();

            for (int y = 0; y < 7; y++)
            {
                inputs = Console.ReadLine().Split(' ');
                for (int x = 0; x < 7; x++)
                {
                    string tile = inputs[x];
                    grid.AddTile(x, y, new Tile(tile));
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
            Quest[] quests = new Quest[numQuests];

            for (int i = 0; i < numQuests; i++)
            {
                string line = Console.ReadLine();

                inputs = line.Split(' ');
                string questItemName = inputs[0];
                int questPlayerId = int.Parse(inputs[1]);
                quests[i] = new Quest(questItemName, questPlayerId);
            }

            GameState gameState = new GameState(grid, players[0], players[1], quests, items);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            if (turnType == 0)
            {
                //Push turn
                PushAI pushAi = new PushAI(gameState);
                Console.WriteLine(pushAi.ComputeCommand()); // PUSH <id> <direction> | MOVE <direction> | PASS
            }
            else
            {
                //Move turn
                MoveAI moveAI = new MoveAI(gameState);
                Console.WriteLine(moveAI.ComputeCommand());
            }

        }
    }
}