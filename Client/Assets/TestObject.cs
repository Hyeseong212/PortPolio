using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public int solution(int[,] game_board, int[,] table)
    {
        int n = game_board.GetLength(0);

        // Extract pieces from the table
        var tablePieces = GetPieces(table, 1);

        // Extract empty spaces from the game board
        var boardSpaces = GetPieces(game_board, 0);

        int totalFilled = 0;

        // Try to fit pieces into the board
        foreach (var space in boardSpaces)
        {
            foreach (var piece in tablePieces.ToList())
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Fit(space, piece))
                    {
                        totalFilled += piece.Count;
                        tablePieces.Remove(piece);
                        break;
                    }
                    piece.Rotate();
                }
            }
        }

        return totalFilled;
    }

    private List<Piece> GetPieces(int[,] board, int targetValue)
    {
        int n = board.GetLength(0);
        bool[,] visited = new bool[n, n];

        List<Piece> pieces = new List<Piece>();
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (!visited[i, j] && board[i, j] == targetValue)
                {
                    List<Tuple<int, int>> cells = new List<Tuple<int, int>>();
                    Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();
                    queue.Enqueue(Tuple.Create(i, j));
                    visited[i, j] = true;

                    while (queue.Count > 0)
                    {
                        var cell = queue.Dequeue();
                        int x = cell.Item1;
                        int y = cell.Item2;
                        cells.Add(cell);

                        for (int k = 0; k < 4; k++)
                        {
                            int nx = x + dx[k];
                            int ny = y + dy[k];
                            if (nx >= 0 && nx < n && ny >= 0 && ny < n && !visited[nx, ny] && board[nx, ny] == targetValue)
                            {
                                queue.Enqueue(Tuple.Create(nx, ny));
                                visited[nx, ny] = true;
                            }
                        }
                    }

                    pieces.Add(new Piece(cells));
                }
            }
        }

        return pieces;
    }

    private bool Fit(Piece space, Piece piece)
    {
        return space.Equals(piece);
    }
}

public class Piece
{
    public List<Tuple<int, int>> Cells { get; private set; }
    public int Count { get; private set; }

    public Piece(List<Tuple<int, int>> cells)
    {
        Cells = cells;
        Count = cells.Count;
        Normalize();
    }

    public void Rotate()
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            int x = Cells[i].Item1;
            int y = Cells[i].Item2;
            Cells[i] = Tuple.Create(y, -x);
        }
        Normalize();
    }

    public void Normalize()
    {
        int minX = Cells.Min(cell => cell.Item1);
        int minY = Cells.Min(cell => cell.Item2);
        for (int i = 0; i < Cells.Count; i++)
        {
            int x = Cells[i].Item1 - minX;
            int y = Cells[i].Item2 - minY;
            Cells[i] = Tuple.Create(x, y);
        }
        Cells.Sort((a, b) =>
        {
            if (a.Item1 == b.Item1)
                return a.Item2.CompareTo(b.Item2);
            return a.Item1.CompareTo(b.Item1);
        });
    }

    public override bool Equals(object obj)
    {
        var other = obj as Piece;
        if (other == null)
            return false;

        if (Cells.Count != other.Cells.Count)
            return false;

        for (int i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].Item1 != other.Cells[i].Item1 || Cells[i].Item2 != other.Cells[i].Item2)
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var cell in Cells)
        {
            hash = hash * 31 + cell.Item1.GetHashCode();
            hash = hash * 31 + cell.Item2.GetHashCode();
        }
        return hash;
    }
}