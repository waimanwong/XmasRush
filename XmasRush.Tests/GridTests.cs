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
            var newGridAndOutputTile = _grid.Push(0, Direction.DOWN, "1000");
            var outputTile = newGridAndOutputTile.Item2;
            var newGrid = newGridAndOutputTile.Item1;

            //Assert
            outputTile.Should().Be("1010");
            
        }
    }
}
