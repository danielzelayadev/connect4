using System;
using System.Collections.Generic;

namespace Application
{
    public class ColumnOutOfBoundsException : Exception
    {
        public ColumnOutOfBoundsException(int col): base("Column index " + col + " is out of bounds.") { }
    }

    public class ColumnIsFullException: Exception
    {
        public ColumnIsFullException(int col): base("Column " + col + " is full.") { }
    }

    public class GameIsOverException : Exception
    {
        public GameIsOverException() : base("Cannot drop chips because the game is over.") { }
    }

    public class BoardPosition
    {
        public int Column { set; get; }
        public int Cell { set; get; }
    }

    public class Move
    {
        public int Column { get; set; }
        public int Score { get; set; }
    }

    public class Connect4Board
    {
        private readonly BoardTile[][] _board;
        private readonly int _columns, _columnSize;

        public bool Done { get; private set; }
        public BoardTile Winner { get; private set; }

        public Connect4Board(int columns, int columnSize)
        {
            _columns = columns;
            _columnSize = columnSize;
            _board = new BoardTile[_columns][];
            Initialize();
        }

        protected Connect4Board(BoardTile[][] board, int columns, int columnSize)
        {
            _board = board;
            _columns = columns;
            _columnSize = columnSize;
        }

        public Connect4Board Clone()
        {
            var board = new BoardTile[_columns][];

            for (var i = 0; i < _columns; i++)
            {
                board[i] = new BoardTile[_columnSize];

                for (var cellIdx = 0; cellIdx < _columnSize; cellIdx++)
                {
                    board[i][cellIdx] = _board[i][cellIdx];
                }
            }

            return new Connect4Board(board, _columns, _columnSize);
        }

        public void Initialize()
        {
            Done = false;
            Winner = BoardTile.Empty;

            for (var column = 0; column < _columns; column++)
            {
                _board[column] = new BoardTile[_columnSize];

                for (var cellIdx = 0; cellIdx < _columnSize; cellIdx++)
                {
                    _board[column][cellIdx] = BoardTile.Empty;
                }
            }
        }

        public Connect4Board GetNewState(BoardTile type, int col)
        {
            var board = Clone();
            board.DropChip(type, col);
            return board;
        }

        public List<int> GetAvailableColumns()
        {
            var availableColumns = new List<int>();
            var finalIndex = _columnSize - 1;

            for (var i = 0; i < _columns; i++)
            {
                if (_board[i][finalIndex] == BoardTile.Empty) availableColumns.Add(i);
            }

            return availableColumns;
        }

        public BoardPosition DropChip(BoardTile type, int col)
        {
            if (Done) throw new GameIsOverException();

            if (col < 0 || col >= _columns) throw new ColumnOutOfBoundsException(col);

            var cellIdx = _getNextColumnCell(col);

            if (cellIdx == -1) throw new ColumnIsFullException(col);

            _board[col][cellIdx] = type;

            var pos = new BoardPosition() { Column = col, Cell = cellIdx };
            var availableColumnCount = GetAvailableColumns().Count;
            var tie = availableColumnCount == 0;

            if (DidMoveWin(type, pos) || tie)
            {
                Done = true;
                Winner = tie ? BoardTile.Empty : type;
            }

            return pos;
        }

        public int EvalScore()
        {
            if (Done)
            {
                if (Winner == BoardTile.CPU) return int.MaxValue;
                if (Winner == BoardTile.Player) return int.MinValue;
                return 0;
            }

            var cpuScore = _evalVerticalScore(BoardTile.CPU) + _evalHorizontalScore(BoardTile.CPU) +
                           _evalLeftDiagonalScore(BoardTile.CPU) + _evalRightDiagonalScore(BoardTile.CPU);
            var humanScore = _evalVerticalScore(BoardTile.Player) + _evalHorizontalScore(BoardTile.Player) +
                             _evalLeftDiagonalScore(BoardTile.Player) + _evalRightDiagonalScore(BoardTile.Player);

            return cpuScore - humanScore;
        }

        private int _evalVerticalScore(BoardTile player)
        {
            var points = 0;

            for (var colIdx = 0; colIdx < _columns; colIdx++)
            {
                var col = _board[colIdx];

                for (var cellIdx = 0; cellIdx < _columnSize; cellIdx++)
                {
                    var cellsLeft = _columnSize - cellIdx;
                    var pointsLeftToWin = 4 - points;
                    var winIsPossible = cellsLeft >= pointsLeftToWin;

                    if (!winIsPossible) return 0;

                    var cell = col[cellIdx];

                    if (cell == BoardTile.Empty) break;

                    if (cell == player) points++;
                    else points = 0;
                }
            }

            return points;
        }

        private int _evalHorizontalScore(BoardTile player)
        {
            var points = 0;

            for (var cellIdx = 0; cellIdx < _columnSize; cellIdx++)
            {
                for (var colIdx = 0; colIdx < _columns - 3; colIdx++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var cell = _board[colIdx + i][cellIdx];

                        if (cell == player) points++;
                        else if (cell != BoardTile.Empty)
                        {
                            points = 0;
                            break;
                        }
                    }
                }
            }

            return points;
        }

        private int _evalRightDiagonalScore(BoardTile player)
        {
            var points = 0;

            for (var cellIdx = 0; cellIdx < _columnSize - 3; cellIdx++)
            {
                for (var colIdx = 0; colIdx < _columns - 3; colIdx++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var cell = _board[colIdx + 1][cellIdx + 1];

                        if (cell == player) points++;
                        else if (cell != BoardTile.Empty)
                        {
                            points = 0;
                            break;
                        }
                    }
                }
            }

            return points;
        }

        private int _evalLeftDiagonalScore(BoardTile player)
        {
            var points = 0;

            for (var cellIdx = 3; cellIdx < _columnSize; cellIdx++)
            {
                for (var colIdx = 0; colIdx <= _columns - 4; colIdx++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var cell = _board[colIdx + 1][cellIdx - 1];

                        if (cell == player) points++;
                        else if (cell != BoardTile.Empty)
                        {
                            points = 0;
                            break;
                        }
                    }
                }
            }

            return points;
        }

        private int _getNextColumnCell(int col)
        {
            var column = _board[col];

            for (var cellIdx = 0; cellIdx < _columnSize; cellIdx++)
            {
                if (column[cellIdx] == BoardTile.Empty) return cellIdx;
            }

            return -1;
        }

        public bool DidMoveWin(BoardTile type, BoardPosition pos)
        {
            return _checkColumn(type, pos) || _checkRow(type, pos) ||
                   _checkRightDiagonal(type, pos) || _checkLeftDiagonal(type, pos);
        }

        private bool _checkColumn(BoardTile type, BoardPosition pos)
        {
            var column = _board[pos.Column];
            var streak = 0;

            for (var cellIdx = 0; cellIdx < _columnSize && (4-streak) <= (_columnSize - cellIdx); cellIdx++)
            {
                var cell = column[cellIdx];

                if (cell == type) streak++;
                else streak = 0;

                if (streak == 4) return true;
            }

            return false;
        }

        private bool _checkRow(BoardTile type, BoardPosition pos)
        {
            var streak = 0;

            for (var colIdx = 0; colIdx < _columns && (4-streak) <= (_columns - colIdx); colIdx++)
            {
                var cell = _board[colIdx][pos.Cell];

                if (cell == type) streak++;
                else streak = 0;

                if (streak == 4) return true;
            }

            return false;
        }

        private bool _checkRightDiagonal(BoardTile type, BoardPosition pos)
        {
            var streak = 0;

            var lowest = pos.Cell < pos.Column ? pos.Cell : pos.Column;
            var origin = new BoardPosition() { Cell = pos.Cell - lowest, Column = pos.Column - lowest };
            var colIdx = origin.Column;
            var cellIdx = origin.Cell;

            for (; colIdx < _columns && cellIdx < _columnSize; colIdx++, cellIdx++)
            {
                var cell = _board[colIdx][cellIdx];

                if (cell == type) streak++;
                else streak = 0;

                if (streak == 4) return true;
            }

            return false;
        }

        private bool _checkLeftDiagonal(BoardTile type, BoardPosition pos)
        {
            var streak = 0;
            var dx = (_columns - 1) - pos.Column;
            var dy = pos.Cell;
            var lowest = dx < dy ? dx : dy;
            var origin = new BoardPosition() { Cell = pos.Cell - lowest, Column = pos.Column + lowest };
            var colIdx = origin.Column;
            var cellIdx = origin.Cell;

            for (; colIdx >= 0 && cellIdx < _columnSize; colIdx--, cellIdx++)
            {
                var cell = _board[colIdx][cellIdx];

                if (cell == type) streak++;
                else streak = 0;

                if (streak == 4) return true;
            }

            return false;
        }
    }
}
