using System;

namespace SudokuSolver
{
    public static class LocalExtensions
    {
        public static string CenterString(this string stringToCenter, int totalLength)
        {
            return stringToCenter.PadLeft(((totalLength - stringToCenter.Length) / 2)
                                          + stringToCenter.Length)
                .PadRight(totalLength);
        }
    }
}
