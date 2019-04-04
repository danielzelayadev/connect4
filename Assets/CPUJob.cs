using Application;
using Unity.Jobs;

public struct CPUJob: IJob {
    public Connect4Board _board;
    public int _depth;
    public int move;

    public void Execute()
    {
        move = MaxPlay(_board, _depth).Column;
    }

    Move MaxPlay(Connect4Board board, int depth, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        if (depth == 0 || board.Done) return new Move { Column = -1, Score = board.EvalScore() };

        var maxMove = new Move { Column = -1, Score = int.MinValue };
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
        if (depth == 0 || board.Done) return new Move { Column = -1, Score = board.EvalScore() };

        var minMove = new Move { Column = -1, Score = int.MaxValue };
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