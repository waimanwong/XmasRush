using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XmasRush.Tests
{
    public class Test01
    {
        [Fact]
        public void Test()
        {
            //Arrange
            Grid grid = new Grid();
            grid.AddTile(0, 0, "0110");
            grid.AddTile(1, 0, "0110");
            grid.AddTile(2, 0, "0101");
            grid.AddTile(3, 0, "0101");
            grid.AddTile(4, 0, "0111");
            grid.AddTile(5, 0, "0110");
            grid.AddTile(6, 0, "0011");
            grid.AddTile(0, 1, "1110");
            grid.AddTile(1, 1, "1101");
            grid.AddTile(2, 1, "1101");
            grid.AddTile(3, 1, "1010");
            grid.AddTile(4, 1, "0110");
            grid.AddTile(5, 1, "0110");
            grid.AddTile(6, 1, "1010");
            grid.AddTile(0, 2, "0101");
            grid.AddTile(1, 2, "0101");
            grid.AddTile(2, 2, "1101");
            grid.AddTile(3, 2, "1101");
            grid.AddTile(4, 2, "1010");
            grid.AddTile(5, 2, "0110");
            grid.AddTile(6, 2, "1010");
            grid.AddTile(0, 3, "0101");
            grid.AddTile(1, 3, "0011");
            grid.AddTile(2, 3, "0110");
            grid.AddTile(3, 3, "0101");
            grid.AddTile(4, 3, "1001");
            grid.AddTile(5, 3, "1100");
            grid.AddTile(6, 3, "0101");
            grid.AddTile(0, 4, "1010");
            grid.AddTile(1, 4, "1001");
            grid.AddTile(2, 4, "1010");
            grid.AddTile(3, 4, "0111");
            grid.AddTile(4, 4, "0111");
            grid.AddTile(5, 4, "0101");
            grid.AddTile(6, 4, "0101");
            grid.AddTile(0, 5, "1010");
            grid.AddTile(1, 5, "1001");
            grid.AddTile(2, 5, "1001");
            grid.AddTile(3, 5, "1010");
            grid.AddTile(4, 5, "0111");
            grid.AddTile(5, 5, "0111");
            grid.AddTile(6, 5, "1011");
            grid.AddTile(0, 6, "1100");
            grid.AddTile(1, 6, "1001");
            grid.AddTile(2, 6, "1101");
            grid.AddTile(3, 6, "0101");
            grid.AddTile(4, 6, "0101");
            grid.AddTile(5, 6, "1001");
            grid.AddTile(6, 6, "1001");

            //Act
            var result = grid.Push(1, Direction.RIGHT, "1101");
            var newGrid = result.Item1;
            var outputTile = result.Item2;

            //Assert
            outputTile.ToString().Should().Be("1010");
            
            for(int i = 1; i < Grid.Width; i++)
            {
                newGrid.tiles[i][1].ToString().Should().Be(grid.tiles[i - 1][1].ToString());
            }
            
        }
    }
}
