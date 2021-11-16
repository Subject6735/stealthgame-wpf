using System;
using System.Collections.Generic;

namespace StealthGame.Persistence
{
    /// <summary>
    /// StealthGame table, handles the game table.
    /// </summary>
    public class StealthGameTable
    {
        #region Fields

        /// <summary>
        /// Table sizes.
        /// </summary>
        private readonly int _tableSize;
        private readonly int _easySize = 20;
        private readonly int _mediumSize = 30;
        private readonly int _hardSize = 40;

        /// <summary>
        /// Vision cone range of guards.
        /// </summary>
        private readonly int _guardVision = 2;

        /// <summary>
        /// The guard datas. (Item1 is the row, Item2 is the col coordinate, Item3 is the random direction integer.)
        /// </summary>
        private List<Tuple<int, int, int>> _guards;

        /// <summary>
        /// The game table.
        /// </summary>
        private readonly string[,] _stealthGameTable;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of the table.
        /// </summary>
        public int TableSize { get { return _tableSize; } }

        /// <summary>
        /// Gets the size of the guard vision cone.
        /// </summary>
        public int GuardVision { get { return _guardVision; } }

        /// <summary>
        /// Gets/Sets the guard datas.
        /// </summary>
        public List<Tuple<int, int, int>> Guards
        {
            get { return _guards; }
            set { _guards = value; }
        }

        /// <summary>
        /// Gets the easy difficulty table size.
        /// </summary>
        public int EasySize { get { return _easySize; } }

        /// <summary>
        /// Gets the medium difficulty table size.
        /// </summary>
        public int MediumSize { get { return _mediumSize; } }

        /// <summary>
        /// Gets the hard difficulty table size.
        /// </summary>
        public int HardSize { get { return _hardSize; } }

        #endregion

        #region Constructors

        /// <summary>
        /// StealthGameTable default constructor.
        /// </summary>
        public StealthGameTable() : this(10) { }

        /// <summary>
        /// StealthGameTable constructor.
        /// </summary>
        /// <param name="tableSize">The size of the table.</param>
        public StealthGameTable(int tableSize)
        {
            if (tableSize < 0)
                throw new ArgumentOutOfRangeException("tableSize", "Invalid table size, less than 0.");

            _tableSize = tableSize;
            _stealthGameTable = new string[tableSize, tableSize];
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if a field is the player.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is the player, otherwise false.</returns>
        public bool IsPlayer(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "P";
        }

        /// <summary>
        /// Gets the player coordinates.
        /// </summary>
        /// <returns>An array of integers: the first element is the row coordinate, the second element is the column coordinate.</returns>
        public int[] GetPlayerCoords()
        {
            int[] coords = new int[2];

            for (int i = 0; i < _tableSize; ++i)
                for (int j = 0; j < _tableSize; ++j)
                {
                    if (IsPlayer(i, j))
                    {
                        coords[0] = i;
                        coords[1] = j;
                        return coords;
                    }
                }

            return null;
        }

        /// <summary>
        /// Checks if a field is the exit.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is the exit, otherwise false.</returns>
        public bool IsExit(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "E";
        }

        /// <summary>
        /// Checks if a field is a guard.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is a guard, otherwise false.</returns>
        public bool IsGuard(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "G";
        }

        /// <summary>
        /// Checks if a field is a vision cone area.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is a vision cone area, otherwise false.</returns>
        public bool IsVision(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "V";
        }

        /// <summary>
        /// Checks if a field is a vision cone area containing the player.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if the vision cone contains the player, otherwise false.</returns>
        public bool IsVisionPlayer(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "VP";
        }

        /// <summary>
        /// Checks if a field is a wall.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is a wall, otherwise false.</returns>
        public bool IsWall(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "W";
        }

        /// <summary>
        /// Checks if a field is a floor.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is a floor, otherwise false.</returns>
        public bool IsFloor(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col] == "F";
        }

        /// <summary>
        /// Sets a field value.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <param name="value">The value to set. (P - Player, E - Exit, G - Guard, W - Wall, F - Field, V - Vision cone, VP - Player in vision cone)</param>
        public void SetValue(int row, int col, string value)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");
            if (value != "P" && value != "E" && value != "G" && value != "V" && value != "W" && value != "F" && value != "VP")
                throw new ArgumentException("Invalid value.");

            _stealthGameTable[row, col] = value;
        }

        /// <summary>
        /// Gets a field value.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>The field value.</returns>
        public string GetValue(int row, int col)
        {
            if (row < 0 || row > _tableSize - 1)
                throw new ArgumentOutOfRangeException("row", "Invalid row coordinate, out of range.");
            if (col < 0 || col > _tableSize - 1)
                throw new ArgumentOutOfRangeException("col", "Invalid column coordinate, out of range.");

            return _stealthGameTable[row, col];
        }

        /// <summary>
        /// Checks if a field is in range of the table.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <returns>A boolean value: true if it is in range, otherwise false.</returns>
        public bool IsValidField(int row, int col)
        {
            return row >= 0 && row <= _tableSize - 1 && col >= 0 && col <= _tableSize - 1;
        }

        #endregion
    }
}
