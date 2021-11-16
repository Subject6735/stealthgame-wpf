using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using StealthGame.Persistence;
using System.Diagnostics;

namespace StealthGame.Model
{
    /// <summary>
    /// Difficulty levels for the game.
    /// </summary>
    public enum GameDifficulty { Easy, Medium, Hard }

    /// <summary>
    /// StealthGame Model, handles logic behind the game.
    /// </summary>
    public class StealthGameModel
    {
        #region Fields

        /// <summary>
        /// Data access.
        /// </summary>
        private readonly IStealthGameDataAccess _dataAccess;

        /// <summary>
        /// The game table.
        /// </summary>
        private StealthGameTable _table;

        /// <summary>
        /// The game difficulty.
        /// </summary>
        private GameDifficulty _gameDifficulty;

        /// <summary>
        /// The vision cone area of the table.
        /// </summary>
        private int[,] _visionConeArea;

        /// <summary>
        /// Determines whether the player reached the exit.
        /// </summary>
        private bool _exitReached;

        /// <summary>
        /// Determines whether the player is detected.
        /// </summary>
        private bool _detected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets game difficulty.
        /// </summary>
        public GameDifficulty GameDifficulty
        {
            get { return _gameDifficulty; }
            set { _gameDifficulty = value; }
        }

        /// <summary>
        /// Gets the game table.
        /// </summary>
        public StealthGameTable Table { get { return _table; } }

        /// <summary>
        /// Sets the game table, for testing only.
        /// </summary>
        public StealthGameTable SetTable { set { _table = value; } }

        /// <summary>
        /// Gets the vision cone areas of the table.
        /// </summary>
        public int[,] VisionConeArea { get { return _visionConeArea; } }

        /// <summary>
        /// Gets whether the player is detected by a guard, or the player entered a vision cone area.
        /// </summary>
        public bool Detected { set { _detected = value; } }

        /// <summary>
        /// Gets whether the player reached the exit.
        /// </summary>
        public bool ExitReached { set { _exitReached = value; } }

        #endregion

        #region Events

        /// <summary>
        /// Handles the event when the player is detected by a guard.
        /// </summary>
        public event EventHandler<StealthGameEventArgs> PlayerDetected;

        /// <summary>
        /// Handles the event when the player reached the exit.
        /// </summary>
        public event EventHandler<StealthGameEventArgs> PlayerReachedExit;

        /// <summary>
	    /// Create game event
	    /// </summary>
	    public event EventHandler<StealthGameEventArgs> GameCreated;

        #endregion

        #region Constructors

        /// <summary>
        /// StealthGameModel constructor.
        /// </summary>
        /// <param name="dataAccess">The data to access.</param>
        public StealthGameModel(IStealthGameDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _table = new StealthGameTable();
            _gameDifficulty = GameDifficulty.Easy;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void NewGame()
        {
            switch (_gameDifficulty)
            {
                case GameDifficulty.Easy:
                    GenerateFields(@"../../../../StealthGame/Levels/EasyLevel.txt");
                    break;
                case GameDifficulty.Medium:
                    GenerateFields(@"../../../../StealthGame/Levels/MediumLevel.txt");
                    break;
                case GameDifficulty.Hard:
                    GenerateFields(@"../../../../StealthGame/Levels/HardLevel.txt");
                    break;
            }

            OnGameCreated();
        }

        /// <summary>
        /// Loads the game.
        /// </summary>
        /// <param name="path">File path.</param>
        public async Task LoadGameAsync(string path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            _table = await _dataAccess.LoadAsync(path);

            OnGameCreated();
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        /// <param name="path">File path.</param>
        public async Task SaveGameAsync(string path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await _dataAccess.SaveAsync(path, _table);
        }

        /// <summary>
        /// Moves the player to the given coordinates.
        /// </summary>
        /// <param name="row">The row coordinate.</param>
        /// <param name="col">The column coordinate.</param>
        public void MovePlayer(int row, int col)
        {
            _detected = _table.GetValue(row, col) == "V";
            _exitReached = _table.GetValue(row, col) == "E";

            _table.SetValue(_table.GetPlayerCoords()[0], _table.GetPlayerCoords()[1], "F");
            _table.SetValue(row, col, "P");

            OnPlayerDetected();
            OnExitReached();
        }

        /// <summary>
        /// Moves the player to the given row coordinate and the current column coordinate.
        /// </summary>
        /// <param name="row"></param>
        public void MovePlayerVertically(int row)
        {
            int col = _table.GetPlayerCoords()[1];

            _detected = _table.GetValue(row, col) == "V";
            _exitReached = _table.GetValue(row, col) == "E";

            if (_table.GetValue(row, col) == "W")
            {
                return;
            }

            _table.SetValue(_table.GetPlayerCoords()[0], _table.GetPlayerCoords()[1], "F");
            _table.SetValue(row, col, "P");
        }

        /// <summary>
        /// Moves the player to the given column coordinate and the current row coordinate.
        /// </summary>
        /// <param name="col"></param>
        public void MovePlayerHorizontally(int col)
        {
            int row = _table.GetPlayerCoords()[0];

            _detected = _table.GetValue(row, col) == "V";
            _exitReached = _table.GetValue(row, col) == "E";

            if (_table.GetValue(row, col) == "W")
            {
                return;
            }

            _table.SetValue(_table.GetPlayerCoords()[0], _table.GetPlayerCoords()[1], "F");
            _table.SetValue(row, col, "P");
        }

        /// <summary>
        /// Handles guard movements.
        /// </summary>
        public void MoveGuards()
        {
            // Remove vision cones, set them again later
            for (int i = 0; i < _table.TableSize; ++i)
                for (int j = 0; j < _table.TableSize; ++j)
                {
                    if (_table.IsVision(i, j))
                    {
                        _table.SetValue(i, j, "F");
                    }
                }

            // Move the guards
            for (int ind = 0; ind < _table.Guards.Count; ++ind)
            {
                bool canMove = _table.IsFloor(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2)
                        || _table.IsFloor(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1)
                        || _table.IsFloor(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2)
                        || _table.IsFloor(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);

                bool isNotExit = !_table.IsExit(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2)
                        && !_table.IsExit(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1)
                        && !_table.IsExit(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2)
                        && !_table.IsExit(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);

                bool isNotGuard = !_table.IsGuard(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2)
                        && !_table.IsGuard(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1)
                        && !_table.IsGuard(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2)
                        && !_table.IsGuard(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);

                if (!canMove && isNotExit && isNotGuard)
                    continue;

                bool wallReached = false;
                bool exitReached = false;
                bool guardReached = false;
                Random r = new Random();

                do
                {
                    switch (_table.Guards[ind].Item3)
                    {
                        case 0: // Top
                            wallReached = _table.IsWall(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2);
                            exitReached = _table.IsExit(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2);
                            guardReached = _table.IsGuard(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2);
                            if (!wallReached && !exitReached && !guardReached)
                            {
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2, "F");
                                _table.SetValue(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2, "G");
                                _table.Guards[ind] = new Tuple<int, int, int>(_table.Guards[ind].Item1 - 1, _table.Guards[ind].Item2, _table.Guards[ind].Item3);
                            }
                            break;
                        case 1: // Right
                            wallReached = _table.IsWall(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1);
                            exitReached = _table.IsExit(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1);
                            guardReached = _table.IsGuard(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1);
                            if (!wallReached && !exitReached && !guardReached)
                            {
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2, "F");
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1, "G");
                                _table.Guards[ind] = new Tuple<int, int, int>(_table.Guards[ind].Item1, _table.Guards[ind].Item2 + 1, _table.Guards[ind].Item3);
                            }
                            break;
                        case 2: // Bottom
                            wallReached = _table.IsWall(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2);
                            exitReached = _table.IsExit(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2);
                            guardReached = _table.IsGuard(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2);
                            if (!wallReached && !exitReached && !guardReached)
                            {
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2, "F");
                                _table.SetValue(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2, "G");
                                _table.Guards[ind] = new Tuple<int, int, int>(_table.Guards[ind].Item1 + 1, _table.Guards[ind].Item2, _table.Guards[ind].Item3);
                            }
                            break;
                        case 3: // Left
                            wallReached = _table.IsWall(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);
                            exitReached = _table.IsExit(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);
                            guardReached = _table.IsGuard(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1);
                            if (!wallReached && !exitReached && !guardReached)
                            {
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2, "F");
                                _table.SetValue(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1, "G");
                                _table.Guards[ind] = new Tuple<int, int, int>(_table.Guards[ind].Item1, _table.Guards[ind].Item2 - 1, _table.Guards[ind].Item3);
                            }
                            break;
                    }

                    // If a wall is reached, generate a new random direction.
                    if (wallReached || exitReached || guardReached)
                    {
                        int d = r.Next(0, 4);
                        _table.Guards[ind] = new Tuple<int, int, int>(_table.Guards[ind].Item1, _table.Guards[ind].Item2, d);
                    }
                }
                while (wallReached || exitReached || guardReached);
            }
        }

        /// <summary>
        /// Sets the vision cone of the guards.
        /// </summary>
        /// <param name="row">Row coordinate.</param>
        /// <param name="col">Column coordinate.</param>
        /// <param name="table">Table object.</param>
        public void SetVisionCone(int row, int col, StealthGameTable table)
        {
            int n = table.GuardVision;

            int[,] verticalArea = new int[table.TableSize, table.TableSize];

            for (int i = 0; i < table.TableSize; ++i)
                for (int j = 0; j < table.TableSize; ++j)
                    verticalArea[i, j] = 0;

            for (int i = row; i >= row - n; --i)
            {
                if (table.IsValidField(i, col) && (table.IsWall(i, col) || table.IsExit(i, col)))
                    break;

                for (int l = col; l >= col - n; --l)
                {
                    if (table.IsValidField(i, l) && (table.IsWall(i, l) || table.IsExit(i, l)))
                        break;

                    if (table.IsValidField(i, l) && !table.IsWall(i, l) && !table.IsGuard(i, l))
                        verticalArea[i, l] = 1;
                }

                for (int r = col; r <= col + n; ++r)
                {
                    if (table.IsValidField(i, r) && (table.IsWall(i, r) || table.IsExit(i, r)))
                        break;

                    if (table.IsValidField(i, r) && !table.IsWall(i, r) && !table.IsGuard(i, r))
                        verticalArea[i, r] = 1;
                }

                if (table.IsValidField(i, col) && !table.IsWall(i, col) && !table.IsGuard(i, col))
                    verticalArea[i, col] = 1;
            }

            for (int i = row; i <= row + n; ++i)
            {
                if (table.IsValidField(i, col) && (table.IsWall(i, col) || table.IsExit(i, col)))
                    break;

                for (int l = col; l >= col - n; --l)
                {
                    if (table.IsValidField(i, l) && (table.IsWall(i, l) || table.IsExit(i, l)))
                        break;

                    if (table.IsValidField(i, l) && !table.IsWall(i, l) && !table.IsGuard(i, l))
                        verticalArea[i, l] = 1;
                }

                for (int r = col; r <= col + n; ++r)
                {
                    if (table.IsValidField(i, r) && (table.IsWall(i, r) || table.IsExit(i, r)))
                        break;

                    if (table.IsValidField(i, r) && !table.IsWall(i, r) && !table.IsGuard(i, r))
                        verticalArea[i, r] = 1;
                }

                if (table.IsValidField(i, col) && !table.IsWall(i, col) && !table.IsGuard(i, col))
                    verticalArea[i, col] = 1;
            }

            int[,] horizontialArea = new int[table.TableSize, table.TableSize];

            for (int i = 0; i < table.TableSize; ++i)
                for (int j = 0; j < table.TableSize; ++j)
                    horizontialArea[i, j] = 0;

            for (int j = col; j >= col - n; --j)
            {
                if (table.IsValidField(row, j) && (table.IsWall(row, j) || table.IsExit(row, j)))
                    break;

                for (int t = row; t >= row - n; --t)
                {
                    if (table.IsValidField(t, j) && (table.IsWall(t, j) || table.IsExit(t, j)))
                        break;

                    if (table.IsValidField(t, j) && !table.IsWall(t, j) && !table.IsGuard(t, j))
                        horizontialArea[t, j] = 1;
                }

                for (int b = row; b <= row + n; ++b)
                {
                    if (table.IsValidField(b, j) && (table.IsWall(b, j) || table.IsExit(b, j)))
                        break;

                    if (table.IsValidField(b, j) && !table.IsWall(b, j) && !table.IsGuard(b, j))
                        horizontialArea[b, j] = 1;
                }

                if (table.IsValidField(row, j) && !table.IsWall(row, j) && !table.IsGuard(row, j))
                    horizontialArea[row, j] = 1;
            }

            for (int j = col; j <= col + n; ++j)
            {
                if (table.IsValidField(row, j) && (table.IsWall(row, j) || table.IsExit(row, j)))
                    break;

                for (int t = row; t >= row - n; --t)
                {
                    if (table.IsValidField(t, j) && (table.IsWall(t, j) || table.IsExit(t, j)))
                        break;

                    if (table.IsValidField(t, j) && !table.IsWall(t, j) && !table.IsGuard(t, j))
                        horizontialArea[t, j] = 1;
                }

                for (int b = row; b <= row + n; ++b)
                {
                    if (table.IsValidField(b, j) && (table.IsWall(b, j) || table.IsExit(b, j)))
                        break;

                    if (table.IsValidField(b, j) && !table.IsWall(b, j) && !table.IsGuard(b, j))
                        horizontialArea[b, j] = 1;
                }

                if (table.IsValidField(row, j) && !table.IsWall(row, j) && !table.IsGuard(row, j))
                    horizontialArea[row, j] = 1;
            }

            int[,] intersectionArea = new int[table.TableSize, table.TableSize];

            for (int i = 0; i < table.TableSize; ++i)
            {
                for (int j = 0; j < table.TableSize; ++j)
                {
                    intersectionArea[i, j] = verticalArea[i, j] + horizontialArea[i, j];

                    if (intersectionArea[i, j] == 2)
                    {
                        if (table.IsPlayer(i, j) || table.IsVisionPlayer(i, j))
                            table.SetValue(i, j, "VP");
                        else
                            table.SetValue(i, j, "V");
                    }
                }
            }

            MapVisionCone();
        }

        /// <summary>
        /// Maps the vision cone areas.
        /// </summary>
        public void MapVisionCone()
        {
            _visionConeArea = new int[_table.TableSize, _table.TableSize];

            for (int i = 0; i < _table.TableSize; ++i)
            {
                for (int j = 0; j < _table.TableSize; ++j)
                {
                    if (_table.IsVision(i, j))
                        _visionConeArea[i, j] = 1;
                    else if (_table.IsVisionPlayer(i, j))
                    {
                        _visionConeArea[i, j] = 2;
                    }
                    else
                        _visionConeArea[i, j] = 0;
                }
            }
        }

        /// <summary>
        /// Checks if the guards detect the player.
        /// </summary>
        public void GuardDetect()
        {
            _detected = false;

            for (int i = 0; i < _table.TableSize && !_detected; ++i)
                for (int j = 0; j < _table.TableSize && !_detected; ++j)
                {
                    if (_visionConeArea[i, j] == 2)
                    {
                        _detected = true;
                    }
                }

            OnPlayerDetected();
        }

        /// <summary>
        /// Checks if the player entered the exit or a vision cone area.
        /// </summary>
        public void IsGameOver()
        {
            OnPlayerDetected();
            OnExitReached();
        }

        #endregion

        #region Private game methods

        /// <summary>
        /// Generates the fields by reading from the correct file.
        /// </summary>
        /// <param name="path">File path.</param>
        private void GenerateFields(string path)
        {
            using StreamReader reader = new StreamReader(path);

            int tableSize = int.Parse(reader.ReadLine());

            StealthGameTable table = new StealthGameTable(tableSize)
            {
                Guards = new List<Tuple<int, int, int>>()
            };

            for (int i = 0; i < tableSize; ++i)
            {
                string[] line = reader.ReadLine().Split(" ");

                for (int j = 0; j < tableSize; ++j)
                {
                    string str = Convert.ToChar(line[j]).ToString();
                    table.SetValue(i, j, str);

                    // Add a random starting direction to the guards
                    Random r = new Random();
                    int d = r.Next(0, 4);

                    if (str == "G")
                    {
                        table.Guards.Add(new Tuple<int, int, int>(i, j, d));
                    }
                }
            }

            _table = table;
        }

        #endregion

        #region Private event methods

        /// <summary>
        /// Fires event if the player is detected.
        /// </summary>
        private void OnPlayerDetected()
        {
            PlayerDetected?.Invoke(this, new StealthGameEventArgs(_detected));
        }

        /// <summary>
        /// Fires event if the player reached the exit.
        /// </summary>
        private void OnExitReached()
        {
            PlayerReachedExit?.Invoke(this, new StealthGameEventArgs(_exitReached));
        }

        /// <summary>
	    /// Fires event when game is created.
	    /// </summary>
	    private void OnGameCreated()
        {
            GameCreated?.Invoke(this, new StealthGameEventArgs(false));
        }

        #endregion
    }
}
