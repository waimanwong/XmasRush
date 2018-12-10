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
            grid.AddTile(0, 0, new Tile("0110"));
            grid.AddTile(1, 0, new Tile("0110"));
            grid.AddTile(2, 0, new Tile("0101"));
            grid.AddTile(3, 0, new Tile("0101"));
            grid.AddTile(4, 0, new Tile("0111"));
            grid.AddTile(5, 0, new Tile("0110"));
            grid.AddTile(6, 0, new Tile("0011"));
            grid.AddTile(0, 1, new Tile("1110"));
            grid.AddTile(1, 1, new Tile("1101"));
            grid.AddTile(2, 1, new Tile("1101"));
            grid.AddTile(3, 1, new Tile("1010"));
            grid.AddTile(4, 1, new Tile("0110"));
            grid.AddTile(5, 1, new Tile("0110"));
            grid.AddTile(6, 1, new Tile("1010"));
            grid.AddTile(0, 2, new Tile("0101"));
            grid.AddTile(1, 2, new Tile("0101"));
            grid.AddTile(2, 2, new Tile("1101"));
            grid.AddTile(3, 2, new Tile("1101"));
            grid.AddTile(4, 2, new Tile("1010"));
            grid.AddTile(5, 2, new Tile("0110"));
            grid.AddTile(6, 2, new Tile("1010"));
            grid.AddTile(0, 3, new Tile("0101"));
            grid.AddTile(1, 3, new Tile("0011"));
            grid.AddTile(2, 3, new Tile("0110"));
            grid.AddTile(3, 3, new Tile("0101"));
            grid.AddTile(4, 3, new Tile("1001"));
            grid.AddTile(5, 3, new Tile("1100"));
            grid.AddTile(6, 3, new Tile("0101"));
            grid.AddTile(0, 4, new Tile("1010"));
            grid.AddTile(1, 4, new Tile("1001"));
            grid.AddTile(2, 4, new Tile("1010"));
            grid.AddTile(3, 4, new Tile("0111"));
            grid.AddTile(4, 4, new Tile("0111"));
            grid.AddTile(5, 4, new Tile("0101"));
            grid.AddTile(6, 4, new Tile("0101"));
            grid.AddTile(0, 5, new Tile("1010"));
            grid.AddTile(1, 5, new Tile("1001"));
            grid.AddTile(2, 5, new Tile("1001"));
            grid.AddTile(3, 5, new Tile("1010"));
            grid.AddTile(4, 5, new Tile("0111"));
            grid.AddTile(5, 5, new Tile("0111"));
            grid.AddTile(6, 5, new Tile("1011"));
            grid.AddTile(0, 6, new Tile("1100"));
            grid.AddTile(1, 6, new Tile("1001"));
            grid.AddTile(2, 6, new Tile("1101"));
            grid.AddTile(3, 6, new Tile("0101"));
            grid.AddTile(4, 6, new Tile("0101"));
            grid.AddTile(5, 6, new Tile("1001"));
            grid.AddTile(6, 6, new Tile("1001"));

            //Act
            var result = grid.Push(1, Direction.RIGHT, new Tile("1101"));
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
