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
}

enum Direction
{
    UP = 0,
    RIGHT = 1,
    DOWN = 2,
    LEFT = 3,
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

        public int Id => this.x + ( this.y * Grid.Width);
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
            foreach(var cell in otherSet.cells)
            {
                newCellSet.cells.Add(cell);
            }
            return newCellSet;
        }
    }

    public static int Width = 7;
    public static int Heigth = 7;

    public readonly Tile[][] _tiles;

    private List<CellSet> cellsets;

    public static readonly Direction[] AllDirections = new[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT };

    public int CellSetCount => cellsets.Count;

    public Grid()
    {
        _tiles = new Tile[Width][];

        for (int x = 0; x < Width; x++)
        {
            _tiles[x] = new Tile[Heigth];
        }

        cellsets = new List<CellSet>(Grid.Width * Grid.Heigth);
    }

    public void AddTile(int x, int y, Tile tile)
    {
        _tiles[x][y] = tile;

        var cell = new Cell(x, y);
        MakeSet(cell);

        if(x != 0)
        {
            //Check left connection
            if (this._tiles[x][y].IsOpenedTo(Direction.LEFT) && this._tiles[x-1][y].IsOpenedTo(Direction.RIGHT))
            {
                CellSet c1 = FindSet(new Cell(x, y));
                CellSet c2 = FindSet(new Cell(x -  1, y));

                if (c1.GetId() != c2.GetId())
                {
                    Union(c1, c2);
                }
            }
        }

        if( y != 0)
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
    
    private CellSet FindSet(Cell cell)
    {
        return cellsets.Single(set => set.Contains(cell));
    }

    private void Union(CellSet c1, CellSet c2)
    {
        CellSet newCellSet = c1.Union(c2);

        cellsets.Remove(c1);
        cellsets.Remove(c2);

        cellsets.Add(newCellSet);
    }

    private void MakeSet(Cell cell)
    {
        cellsets.Add(new CellSet(cell));
    }

    public IReadOnlyList<Direction> GetPossibleDirections(Position from)
    {
        List<Direction> possibleDirections = new List<Direction>(4);

        foreach (var direction in AllDirections)
        {
            var siblingPosition = from.GetSibling(direction);
            if(PositionIsValid(siblingPosition))
            {
                if(FindSet(new Cell(from.x, from.y)) == FindSet(new Cell(siblingPosition.x, siblingPosition.y)))
                {
                    possibleDirections.Add(direction);
                }
            }
             
        }

        return possibleDirections;
    }

    private Direction OppositeDirection(Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }

    private bool PositionIsValid(Position position)
    {
        return 0 <= position.x && position.x < Grid.Width &&
                0 <= position.y && position.y < Grid.Heigth;
    }
}

class Player : Position
{
    private readonly int id;
    private readonly Tile tile;

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
    private readonly string itemName;
    private readonly int playerId;

    public Quest(string itemName, int playerId)
    {
        this.itemName = itemName;
        this.playerId = playerId;
    }
}

class Tile
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
}

class PushAI
{
    private readonly GameState gameState;

    public PushAI(GameState gameState)
    {
        this.gameState = gameState;
    }

    public string ComputeCommand()
    {
        var grid = gameState.grid;


        var myPosition = gameState.me;
        var myitem = gameState.items.First(x => x.playerId == 0);

        if (myitem.x == -1)
            return $"PUSH {myPosition.x} DOWN";

        var deltaX = Math.Abs(myPosition.x - myitem.x);
        var deltaY = Math.Abs(myPosition.y - myitem.y);

        int index = 0;
        Direction bestDirection = Direction.DOWN;
        
        if (deltaX > deltaY || deltaX == 0)
        {
            index = myitem.y;
            if (myPosition.x > myitem.x)
            {
                bestDirection = Direction.RIGHT;
            }
            else
            {
                bestDirection = Direction.LEFT;
            }
        }
        else
        {
            index = myitem.x;
            if (myPosition.y > myitem.y)
            {
                bestDirection = Direction.DOWN;
            }
            else
            {
                bestDirection = Direction.UP;
            }
        }

        
        return $"PUSH {index} {bestDirection.ToString()}";
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
        var myitem = gameState.items.First(x => x.playerId == 0);

        if(myitem.x == -1)
        {
            return "PASS";
        }

        List<Direction> directions = new List<Direction>();
        int moveCount = 0;

        var visited = new HashSet<Tuple<int, int>>();
        visited.Add(new Tuple<int, int>(myPosition.x, myPosition.y));

        var borders = new Queue<Tuple<int, int>>();
        
        int lowestDistance = 20;
        borders.Enqueue(new Tuple<int, int>(myPosition.x, myPosition.y));

        while (borders.Count > 0)
        {
            var p = borders.Dequeue();
            var currentPosition = new Position(p.Item1, p.Item2);

            var possibleDirections = grid.GetPossibleDirections(from: currentPosition);
            
            Direction? bestDirection = null;
            Position bestNextPosition = myPosition;
            
            foreach (var direction in possibleDirections)
            {
                var siblingPosition = myPosition.GetSibling(direction);
                var positionValue = new Tuple<int, int>(siblingPosition.x, siblingPosition.y);
                if (visited.Contains(positionValue))
                {
                    continue;
                }

                visited.Add(positionValue);
                borders.Enqueue(positionValue);

                var distance = siblingPosition.DistanceTo(myitem);

                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    bestDirection = direction;
                    bestNextPosition = siblingPosition;
                }
            }

            if (bestDirection != null)
            {
                myPosition = bestNextPosition;
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

            Debug($"set count = {grid.CellSetCount}");

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