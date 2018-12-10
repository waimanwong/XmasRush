using FluentAssertions;
using System;
using Xunit;

namespace XmasRush.Tests
{
    public class GridTests
    {
        private readonly Grid _grid;

        public GridTests()
        {
            _grid = new Grid();

            _grid.AddTile(0, 0, new Tile("0110"));
            _grid.AddTile(1, 0, new Tile("1001"));
            _grid.AddTile(2, 0, new Tile("1011"));
            _grid.AddTile(3, 0, new Tile("1111"));
            _grid.AddTile(4, 0, new Tile("1010"));
            _grid.AddTile(5, 0, new Tile("1011"));
            _grid.AddTile(6, 0, new Tile("1010"));
            _grid.AddTile(0, 1, new Tile("1110"));
            _grid.AddTile(1, 1, new Tile("0111"));
            _grid.AddTile(2, 1, new Tile("1001"));
            _grid.AddTile(3, 1, new Tile("1010"));
            _grid.AddTile(4, 1, new Tile("0110"));
            _grid.AddTile(5, 1, new Tile("0101"));
            _grid.AddTile(6, 1, new Tile("0111"));
            _grid.AddTile(0, 2, new Tile("1101"));
            _grid.AddTile(1, 2, new Tile("1101"));
            _grid.AddTile(2, 2, new Tile("0110"));
            _grid.AddTile(3, 2, new Tile("0111"));
            _grid.AddTile(4, 2, new Tile("1010"));
            _grid.AddTile(5, 2, new Tile("0110"));
            _grid.AddTile(6, 2, new Tile("0110"));
            _grid.AddTile(0, 3, new Tile("0111"));
            _grid.AddTile(1, 3, new Tile("1001"));
            _grid.AddTile(2, 3, new Tile("1010"));
            _grid.AddTile(3, 3, new Tile("1010"));
            _grid.AddTile(4, 3, new Tile("1010"));
            _grid.AddTile(5, 3, new Tile("0110"));
            _grid.AddTile(6, 3, new Tile("1101"));
            _grid.AddTile(0, 4, new Tile("1001"));
            _grid.AddTile(1, 4, new Tile("1001"));
            _grid.AddTile(2, 4, new Tile("1010"));
            _grid.AddTile(3, 4, new Tile("1101"));
            _grid.AddTile(4, 4, new Tile("1001"));
            _grid.AddTile(5, 4, new Tile("0111"));
            _grid.AddTile(6, 4, new Tile("0111"));
            _grid.AddTile(0, 5, new Tile("1101"));
            _grid.AddTile(1, 5, new Tile("0101"));
            _grid.AddTile(2, 5, new Tile("1001"));
            _grid.AddTile(3, 5, new Tile("1010"));
            _grid.AddTile(4, 5, new Tile("0110"));
            _grid.AddTile(5, 5, new Tile("1101"));
            _grid.AddTile(6, 5, new Tile("1011"));
            _grid.AddTile(0, 6, new Tile("1010"));
            _grid.AddTile(1, 6, new Tile("1110"));
            _grid.AddTile(2, 6, new Tile("1010"));
            _grid.AddTile(3, 6, new Tile("1111"));
            _grid.AddTile(4, 6, new Tile("1110"));
            _grid.AddTile(5, 6, new Tile("0110"));
            _grid.AddTile(6, 6, new Tile("1001"));

        }
        
        [Fact]
        public void Test1()
        {
            //Arrange
            Console.WriteLine(_grid.ToString());

            //Act
            var newGridAndOutputTile = _grid.Push(0, Direction.DOWN, new Tile("1000"));
            var outputTile = newGridAndOutputTile.Item2;
            var newGrid = newGridAndOutputTile.Item1;

            //Assert
            outputTile.Should().Be(new Tile("1010"));


        }
    }
}
