using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    class Program
    {
        private static string[] _boxes;
        private static string _cols;
        private static string _rows;
        private static SortedList<string, string[]> _peers;
        private static List<string[]> _unitList;

        public static void Main(string[] args)
        {
            _cols = "123456789";
            _rows = "ABCDEFGHI";
            _boxes = Cross(_rows, _cols);

            var rowUnits = new List<string[]>();
            foreach (var c in _cols)
                rowUnits.Add(Cross(_rows, c.ToString()));

            var colUnits = new List<string[]>();
            foreach (var r in _rows)
                colUnits.Add(Cross(r.ToString(), _cols));

            var squareUnits = new List<string[]>();
            foreach (var rs in new[] { "ABC", "DEF", "GHI" })
                squareUnits.AddRange(new[] { "123", "456", "789" }.Select(cs => Cross(rs, cs)));

            _unitList = new List<string[]>();
            _unitList.AddRange(rowUnits);
            _unitList.AddRange(colUnits);
            _unitList.AddRange(squareUnits);

            var units = new SortedList<string, string[][]>();
            foreach (var s in _boxes)
                units.Add(s, _unitList.Where(x => x.Contains(s)).ToArray());

            _peers = new SortedList<string, string[]>();
            foreach (var s in _boxes)
            {
                var peer = new List<string>();
                foreach (var row in units[s])
                {
                    var elemStrings = row.Where(x => x != s).ToArray();
                    foreach (var elem in elemStrings)
                    {
                        if (!peer.Contains(elem))
                            peer.Add(elem);
                    }
                }

                _peers.Add(s, peer.ToArray());
            }

            var hardPuzzle = "4.....8.5.3..........7......2.....6.....8.4......1.......6.3.7.5..2.....1.4......";
            var puzzle = ".....97..4..1..3.8.1...89....9..42...8.23.179..26.7.833..7.16.......2...97..5..12";

            Console.WriteLine("**** SOLVING SIMPLE PUZZLE *****");
            Display(Eliminate(GridValuesExtended(puzzle)));
            var start = DateTime.Now;
            Search(GridValuesExtended(puzzle));
            var end = DateTime.Now;
            Console.WriteLine($"It took {end.Subtract(start).Seconds}.{end.Subtract(start).Milliseconds} seconds\n");

            Console.WriteLine("**** SOLVING HARD PUZZLE *****");
            Display(Eliminate(GridValuesExtended(hardPuzzle)));
            start = DateTime.Now;
            Search(GridValuesExtended(hardPuzzle));
            end = DateTime.Now;
            Console.WriteLine($"It took {end.Subtract(start).Seconds}.{end.Subtract(start).Milliseconds} seconds\n");
            Console.Read();
        }

        private static string[] Cross(string a, string b)
        {
            var results = new List<string>();

            foreach (var charA in a)
                foreach (var charB in b)
                    results.Add(charA + "" + charB);

            return results.ToArray();
        }

        private static SortedList<string, string> GridValues(string grid)
        {
            if (grid.Length != 81) return null;

            var dict = new SortedList<string, string>();
            foreach (var item in _boxes.Zip(grid.ToCharArray(), (a, b) => new { Box = a, Grid = b }))
                dict.Add(item.Box, item.Grid.ToString());

            return dict;
        }

        private static void Display(SortedList<string, string> values)
        {
            var width = 1 + (_boxes.Select(s => values[s].Length)).Max();
            var line = string.Join("+", Enumerable.Repeat(string.Join("", Enumerable.Repeat("-", width * 3)), 3));

            foreach (var row in _rows)
            {
                string gridLine = "";
                foreach (var col in _cols)
                    gridLine += (values["" + row + col]).CenterString(width) + (col == '3' || col == '6' ? "|" : "");

                Console.WriteLine(gridLine);

                if (row == 'C' || row == 'F')
                    Console.WriteLine(line);
            }

            Console.WriteLine(line);
        }

        private static SortedList<string, string> GridValuesExtended(string grid)
        {
            var values = new List<string>();
            string alldigits = "123456789";

            foreach (var c in grid)
            {
                if (c == '.')
                    values.Add(alldigits);
                else if (alldigits.Contains(c))
                    values.Add("" + c);
            }

            if (grid.Length != 81) return null;

            var dict = new SortedList<string, string>();
            foreach (var item in _boxes.Zip(values, (a, b) => new { Box = a, Grid = b }))
                dict.Add(item.Box, item.Grid);

            return dict;
        }

        private static SortedList<string, string> Eliminate(SortedList<string, string> values)
        {
            var solvedValues = values.Keys.Where(box => values[box].Length == 1).ToList();

            foreach (var box in solvedValues)
            {
                var digit = values[box];
                foreach (var peer in _peers[box])
                {
                    if (digit != "")
                        values[peer] = values[peer].Replace(digit, "");
                }
            }

            return values;
        }

        private static SortedList<string, string> OnlyChoice(SortedList<string, string> values)
        {
            foreach (var unit in _unitList)
            {
                foreach (var digit in "123456789")
                {
                    var dplaces = unit.Where(box => values[box].Contains(digit)).ToList();

                    if (dplaces.Count == 1)
                        values[dplaces[0]] = digit.ToString();
                }
            }

            return values;
        }

        private static SortedList<string, string> ReducePuzzle(SortedList<string, string> values)
        {
            var stalled = false;

            while (!stalled)
            {
                var solvedValuesBefore = values.Keys.Count(box => values[box].Length == 1);

                if (!values.Values.Any(x => x.Length > 1))
                {
                    stalled = true;
                    continue;
                }

                values = Eliminate(values);
                Display(values);
                Console.WriteLine();

                if (!values.Values.Any(x => x.Length > 1))
                {
                    stalled = true;
                    continue;
                }

                values = OnlyChoice(values);
                Display(values);
                Console.WriteLine();

                var solvedValuesAfter = values.Keys.Count(box => values[box].Length == 1);

                stalled = solvedValuesBefore == solvedValuesAfter;

                if (values.Keys.Count(box => values[box].Length == 0) > 0)
                    return null;
            }

            return values;
        }

        private static SortedList<string, string> Search(SortedList<string, string> values)
        {
            values = ReducePuzzle(values);

            if (values == null)
                return null;

            if (_boxes.Select(s => values[s].Length == 1).All(x => x))
                return values; // solved

            var pairs = new SortedList<string, int>();
            foreach (var s in _boxes)
            {
                if (values[s].Length > 1)
                    pairs.Add(s, values[s].Length);
            }

            string boxWithMinLength = null;
            int minLength = 0;
            foreach (var pair in pairs)
            {
                if (boxWithMinLength == null)
                {
                    boxWithMinLength = pair.Key;
                    minLength = pair.Value;
                }
                else
                {
                    if (pair.Value < minLength)
                    {
                        boxWithMinLength = pair.Key;
                        minLength = pair.Value;
                    }
                }
            }

            foreach (var value in values[boxWithMinLength ?? throw new InvalidOperationException()])
            {
                var newSudoku = new SortedList<string, string>();

                foreach (var x in values)
                    newSudoku.Add(x.Key, x.Value);

                newSudoku[boxWithMinLength] = "" + value;
                var attempt = Search(newSudoku);

                if (attempt != null)
                    return attempt;
            }

            return null;
        }
    }
}
