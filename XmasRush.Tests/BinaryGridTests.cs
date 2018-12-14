using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace XmasRush.Tests
{

    public class BinaryGrid
    {
        public const int Height = 7;
        public const int Width = 7;

        public bool[][] cells;

        public BinaryGrid()
        {
            cells = new bool[Height][];
            for(int y = 0; y < Height; y++)
            {
                cells[y] = new bool[Width * 4];
            }
        }

        public void AddTile(int x, int y, string tile)
        {
            this.AddTile(x, y, tile.Select(c => c == '1').ToArray());
        }

        public void AddTile(int x, int y, bool[] tile)
        {
            for (int ix = 0; ix < 4; ix++)
            {
                cells[y][ (4 * x) + ix] = tile[ix];
            }
        }

        public Tuple<BinaryGrid,bool[]> Push(int index, Direction direction, bool[] tile)
        {
            if (direction == Direction.LEFT || direction == Direction.RIGHT)
                return PushHorizontal(index, direction, tile);
            else
                return PushVertical(index, direction, tile);
        }

        private Tuple<BinaryGrid, bool[]> PushHorizontal(int index, Direction direction, bool[] tile)
        {
            BinaryGrid newGrid = new BinaryGrid();
            bool[] outputTile = null;

            for (int y = 0; y < Grid.Heigth; y++)
            {
                if (y != index)
                {
                    for (int x = 0; x < Grid.Width; x++)
                    {
                        newGrid.AddTile(x, y, this.GetTile(x,y));
                    }
                }
                else
                {
                    if (direction == Direction.LEFT)
                    {
                        int x = 0;
                        for (; x < Grid.Width - 1; x++)
                        {
                            newGrid.AddTile(x, y, this.GetTile(x + 1,y));
                        }
                        newGrid.AddTile(x, y, tile);
                        outputTile = this.GetTile(0,y);
                    }
                    else if (direction == Direction.RIGHT)
                    {
                        newGrid.AddTile(0, y, tile);
                        for (int x = 1; x < Grid.Width; x++)
                        {
                            newGrid.AddTile(x, y, this.GetTile(x - 1,y));
                        }
                        outputTile = this.GetTile(Grid.Width - 1,y);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            return new Tuple<BinaryGrid, bool[]>(newGrid, outputTile);
        }

        public Tuple<BinaryGrid, bool[]> PushVertical(int index, Direction direction, bool[] tile)
        {
            BinaryGrid newGrid = new BinaryGrid();
            bool[] outputTile = null;

            for (int y = 0; y < Grid.Heigth; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    if (x != index)
                    {
                        newGrid.AddTile(x, y, this.GetTile(x,y));
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
                                newGrid.AddTile(x, y, this.GetTile(x, y - 1));
                            }
                            outputTile = this.GetTile(x, Grid.Heigth - 1);
                        }
                        else if (direction == Direction.UP)
                        {
                            if (y == Grid.Heigth - 1)
                            {
                                newGrid.AddTile(x, y, tile);
                            }
                            else
                            {
                                newGrid.AddTile(x, y, this.GetTile(x, y + 1));
                            }
                            outputTile = this.GetTile(x, 0);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            return new Tuple<BinaryGrid, bool[]>(newGrid, outputTile);
        }

        public bool[] GetTile(int x, int y)
        {
            int internalX = 4 * x;
            return new[] { cells[y][internalX], cells[y][internalX + 1], cells[y][internalX + 2], cells[y][internalX + 3] };
        }

    }

    public class BinaryGridTest
    {
        private readonly BinaryGrid _grid;

        public BinaryGridTest()
        {
            _grid = new BinaryGrid();

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

        }

        [Fact]
        public void Test1()
        {
            //Arrange
            Console.WriteLine(_grid.ToString());

            //Act
            var tile = "1000".Select(c => c == '1').ToArray();
            var newGridAndOutputTile = _grid.Push(0, Direction.DOWN, tile);

            var outputTile = newGridAndOutputTile.Item2;
            var newGrid = newGridAndOutputTile.Item1;

            //Assert
            outputTile[0].Should().Be(true);
            outputTile[1].Should().Be(false);
            outputTile[2].Should().Be(true);
            outputTile[3].Should().Be(false);

            newGrid.GetTile(0, 0).Should().ContainInOrder(new[] { true, false, false, false });

        }
    }

}
