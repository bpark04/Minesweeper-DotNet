﻿using System;
using System.Linq;
using System.Windows.Forms;

namespace Minesweeper.Core.Boards
{
    public class Board
    {
        public Minesweeper Minesweeper { get; set; }
        public BoardPainter Painter { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NumMines { get; set; }
        public Cell[,] Cells { get; set; }
        public bool ShowMines { get; set; }
        public bool ShowPercentage { get; set; }
        public bool ShowLocation { get; set; }
        public bool GameOver { get; set; }

        public const int CellSize = 32;

        /// <summary>
        /// Constructs a new <see cref="Board"/> with the given properties.
        /// </summary>
        /// <param name="minesweeper"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mines"></param>
        public Board(Minesweeper minesweeper, int width, int height, int mines)
        {
            Minesweeper = minesweeper;
            Width = width;
            Height = height;
            NumMines = mines;
            Cells = new Cell[width, height];
            Painter = new BoardPainter { Board = this };
        }

        /// <summary>
        /// Setup the cells on the board.
        /// </summary>
        public void SetupBoard()
        {
            for (var x = 1; x <= Width; x++)
            {
                for (var y = 1; y <= Height; y++)
                {
                    Cells[x - 1, y - 1] = new Cell(x - 1, y - 1, this);
                }
            }

            GameOver = false;
        }

        /// <summary>
        /// Randomly distribute the mines across the game board.
        /// </summary>
        public void PlaceMines()
        {
            var minesPlaced = 0;
            var random = new Random();

            while (minesPlaced < NumMines)
            {
                int x = random.Next(0, Width);
                int y = random.Next(0, Height);

                if (!Cells[x, y].IsMine)
                {
                    Cells[x, y].CellType = CellType.Mine;
                    minesPlaced += 1;
                }
            }

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var c = Cells[x, y];
                    c.NumMines = c.GetNeighborCells().Where(n => n.IsMine).Count();
                }
            }

            Minesweeper.Invalidate();
        }

        /// <summary>
        /// User opened a mine and lost. Reveal the locations of the remaining mines
        /// and then restart the game.
        /// </summary>
        public void RevealMines()
        {
            // Reveal where the mines where
            GameOver = true;
            Minesweeper.Invalidate();

            // Ask to play again
            HandleGameOver(gameWon: false);
        }

        /// <summary>
        /// Offer the user the option to restart the game.
        /// </summary>
        /// <param name="gameWon"></param>
        private void HandleGameOver(bool gameWon)
        {
            var message = gameWon ? "Congratulations... You won!" : "Unlucky... you opened a mine!";
            message += "\nWould you like to play again?";

            var response = MessageBox.Show(message, "Game Over", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                // Restart the game
                SetupBoard();
                PlaceMines();
            }
        }

        /// <summary>
        /// Determines whether the game has been won.
        /// This is when the user has correctly identified all the mines on the board.
        /// </summary>
        public void CheckForWin()
        {
            var correctMines = 0;
            var incorrectMines = 0;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var c = Cells[x, y];
                    if (c.CellType == CellType.Flagged)
                    {
                        incorrectMines += 1;
                    }
                    if (c.CellType == CellType.FlaggedMine)
                    {
                        correctMines += 1;
                    }
                }
            }

            if (correctMines == NumMines && incorrectMines == 0)
            {
                HandleGameOver(gameWon: true);
            }
        }

        /// <summary>
        /// Calculates the percentage of each cell on the board being a mine.
        /// </summary>
        public void SetMinePercentages()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y].CalculateMinePercentage();
                }
            }
        }
    }
}