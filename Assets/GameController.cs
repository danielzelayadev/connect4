using System;
using System.Collections.Generic;
using Application;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}

public class GameController : MonoBehaviour {

    public Tile playerChip, cpuChip, slotChip;
    public Tilemap gameBoard;

    //public Difficulty difficulty = Difficulty.Medium;

    public Text winText;

    public Button exitBtn, resetBtn;

    public Dropdown difficultyDropdown;

    private bool isPlayerTurn = true;

    private readonly int COLUMNS = 7, COLUMN_SIZE = 6;

    private Connect4Board _board;

    private bool done, cpuMoved;

    private Vector3Int mouseCell;

    private CPUJob cpuJob;
    private JobHandle cpuJobHandle;

    private Difficulty difficulty;

	// Use this for initialization
	void Start () {
        _board = new Connect4Board(COLUMNS, COLUMN_SIZE);
        mouseCell = new Vector3Int(-1, COLUMN_SIZE, 0);

        exitBtn.onClick.AddListener(OnExitBtnClick);
        resetBtn.onClick.AddListener(OnResetBtnClick);

        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChange);
    }
	
	// Update is called once per frame
	void Update () {
        if (done) return;
        if (_board.Done)
        {
            if (_board.Winner == BoardTile.Player) winText.text = "Player won!";
            else if (_board.Winner == BoardTile.CPU) winText.text = "CPU won!";
            else winText.text = "It's a tie!";
            done = true;
        }
        else
        {
            if (isPlayerTurn) PlayerTurn();
            else CpuTurn();
        }
    }

    void OnExitBtnClick()
    {
        UnityEngine.Application.Quit();
    }

    void OnResetBtnClick()
    {
        ClearBoardUI();
        _board.Initialize();
        winText.text = "";
        done = false;
    }

    void OnDifficultyChange(int value)
    {
        difficulty = (Difficulty)value;
    }

    void ClearBoardUI()
    {
        for (var i = 0; i < COLUMNS; i++)
        {
            for (var k = 0; k < COLUMN_SIZE; k++)
            {
                var cell = new Vector3Int(i, k, 0);
                var tile = gameBoard.GetTile(cell);
                if (tile.name != "slot") gameBoard.SetTile(cell, slotChip);
            }
        }
    }

    BoardPosition MakeMove(BoardTile player, int column)
    {
        if (player == BoardTile.Empty) throw new Exception("Cannot place an empty chip.");

        try
        {
            var boardPos = _board.DropChip(player, column);
            gameBoard.SetTile(new Vector3Int(column, boardPos.Cell, 0), player == BoardTile.Player ? playerChip : cpuChip);

            return boardPos;
        }
        catch (ColumnIsFullException) { }
        catch (ColumnOutOfBoundsException) { }

        return null;
    }

    void PlayerTurn()
    {
        gameBoard.SetTile(mouseCell, null);

        var mouseColumn = gameBoard.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)).x;

        if (mouseColumn >= 0 && mouseColumn < COLUMNS)
        {
            mouseCell = new Vector3Int(mouseColumn, COLUMN_SIZE, 0);
            gameBoard.SetTile(mouseCell, playerChip);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var boardPos = MakeMove(BoardTile.Player, mouseColumn);

            isPlayerTurn = boardPos == null;
        }
    }

    void CpuTurn()
    {
        gameBoard.SetTile(mouseCell, null);

        var move = MaxPlay(_board, GetDepthByDifficulty(difficulty));

        MakeMove(BoardTile.CPU, move.Column);

        isPlayerTurn = true;
    }

    void CpuJobTurn()
    {
        if (cpuJobHandle.IsCompleted)
        {
            if (!cpuMoved)
            {
                MakeMove(BoardTile.CPU, cpuJob.move);
                isPlayerTurn = false;
                cpuMoved = true;
            }
            else
            {
                cpuJob = new CPUJob { _board = _board, _depth = GetDepthByDifficulty(difficulty) };
                cpuJobHandle = cpuJob.Schedule();
                cpuMoved = false;
            }
        }
    }

    int GetDepthByDifficulty(Difficulty d)
    {
        if (d == Difficulty.Easy) return 2;
        if (d == Difficulty.Medium) return 5;
        return 8;
    }

    Move MaxPlay(Connect4Board board, int depth, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        if (depth == 0 || board.Done) return new Move() { Column = -1, Score = board.EvalScore() };

        var maxMove = new Move() { Column = -1, Score = int.MinValue };
        var possibleColumns = board.GetAvailableColumns();

        for (var i = 0; i < possibleColumns.Count; i++)
        {
            var col = possibleColumns[i];
            var newBoard = board.GetNewState(BoardTile.CPU, col);

            var nextMove = MinPlay(newBoard, depth - 1, alpha, beta);

            if (maxMove.Column == -1 || nextMove.Score > maxMove.Score)
            {
                maxMove.Column = col;
                maxMove.Score = nextMove.Score;
                alpha = nextMove.Score;
            }

            if (alpha >= beta) return maxMove;
        }

        return maxMove;
    }

    Move MinPlay(Connect4Board board, int depth, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        if (depth == 0 || board.Done) return new Move() { Column = -1, Score = board.EvalScore() };

        var minMove = new Move() { Column = -1, Score = int.MaxValue };
        var possibleColumns = board.GetAvailableColumns();

        for (var i = 0; i < possibleColumns.Count; i++)
        {
            var col = possibleColumns[i];
            var newBoard = board.GetNewState(BoardTile.Player, col);

            var nextMove = MaxPlay(newBoard, depth - 1, alpha, beta);

            if (minMove.Column == -1 || nextMove.Score < minMove.Score)
            {
                minMove.Column = col;
                minMove.Score = nextMove.Score;
                beta = nextMove.Score;
            }

            if (alpha >= beta) return minMove;
        }

        return minMove;
    }

}
