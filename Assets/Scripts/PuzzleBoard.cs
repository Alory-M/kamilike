using System;
using System.Collections.Generic;

namespace KamiUnity
{
    [Serializable] public sealed class Level
    {
        public string id, stage, theme, colours;
        public int width, height, gold, silver, bronze;
    }
    [Serializable] public sealed class LevelCatalog { public Level[] levels; }

    // Pure C# rules: row-major rectangular board, four-neighbour connectivity.
    public sealed class PuzzleBoard
    {
        public readonly Level Level;
        public int[] Cells { get; private set; }
        public int Moves { get; private set; }
        readonly Stack<int[]> history = new Stack<int[]>();
        public bool Solved
        {
            get { foreach (int c in Cells) if (c != Cells[0]) return false; return true; }
        }
        public PuzzleBoard(Level level)
        {
            if (level == null || level.width <= 0 || level.height <= 0 ||
                level.colours == null || level.colours.Length != level.width * level.height)
                throw new ArgumentException("Invalid puzzle dimensions or cell data.");
            Level = level; Reset();
        }
        public void Reset()
        {
            Cells = new int[Level.colours.Length];
            for (int i = 0; i < Cells.Length; i++)
            {
                if (Level.colours[i] < '1' || Level.colours[i] > '9')
                    throw new ArgumentException("Invalid colour ID.");
                Cells[i] = Level.colours[i] - '0';
            }
            Moves = 0; history.Clear();
        }
        public bool Paint(int index, int colour)
        {
            if (index < 0 || index >= Cells.Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (colour < 1 || colour > 9 || Level.colours.IndexOf((char)('0' + colour)) < 0)
                throw new ArgumentOutOfRangeException(nameof(colour));
            int old = Cells[index];
            if (Solved || old == colour) return false;
            history.Push((int[])Cells.Clone());
            var queue = new Queue<int>(); queue.Enqueue(index); Cells[index] = colour;
            while (queue.Count > 0)
            {
                int i = queue.Dequeue(), x = i % Level.width, y = i / Level.width;
                if (x > 0) Visit(i - 1, old, colour, queue);
                if (x + 1 < Level.width) Visit(i + 1, old, colour, queue);
                if (y > 0) Visit(i - Level.width, old, colour, queue);
                if (y + 1 < Level.height) Visit(i + Level.width, old, colour, queue);
            }
            Moves++; return true;
        }
        void Visit(int i, int old, int colour, Queue<int> queue)
        {
            if (Cells[i] != old) return;
            Cells[i] = colour; queue.Enqueue(i);
        }
        public void Undo() { if (history.Count > 0) { Cells = history.Pop(); Moves--; } }
        public string Medal => !Solved ? "" : Moves <= Level.gold ? "GOLD" :
            Moves <= Level.silver ? "SILVER" : Level.bronze > 0 && Moves <= Level.bronze ? "BRONZE" : "COMPLETE";
    }
}
