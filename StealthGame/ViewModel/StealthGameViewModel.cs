using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using StealthGame.Model;

namespace StealthGame.ViewModel
{
    public class StealthGameViewModel : ViewModelBase
    {
        #region Fields

        private StealthGameModel _model;

        private int _rowUp;
        private int _rowDown;
        private int _colLeft;
        private int _colRight;

        private bool _enablePause;
        private bool _enableResume;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the new game command.
        /// </summary>
        public DelegateCommand NewGameCommand { get; private set; }

        /// <summary>
        /// Gets the load game command.
        /// </summary>
        public DelegateCommand LoadGameCommand { get; private set; }

        /// <summary>
        /// Gets the save game command.
        /// </summary>
        public DelegateCommand SaveGameCommand { get; private set; }

        /// <summary>
        /// Gets the quit game command.
        /// </summary>
        public DelegateCommand QuitGameCommand { get; private set; }

        /// <summary>
        /// Gets the pause game command.
        /// </summary>
        public DelegateCommand PauseCommand { get; private set; }

        /// <summary>
        /// Gets the resume game command.
        /// </summary>
        public DelegateCommand ResumeCommand { get; private set; }

        /// <summary>
        /// Gets the help command.
        /// </summary>
        public DelegateCommand HelpCommand { get; private set; }

        /// <summary>
        /// Gets the game field collection.
        /// </summary>
        public ObservableCollection<StealthGameField> Fields { get; set; }

        public int TableSize
        {
            get { return _model.Table.TableSize; }
        }

        public bool EnablePause
        {
            get { return _enablePause; }
            set { _enablePause = value; OnPropertyChanged(); }
        }

        public bool EnableResume
        {
            get { return _enableResume; }
            set { _enableResume = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Directions from a cell.
        /// </summary>
        public int RowUp { get { return _rowUp; } set { _rowUp = value; OnPropertyChanged(); } }
        public int RowDown { get { return _rowDown; } set { _rowDown = value; OnPropertyChanged();  } }
        public int ColLeft { get { return _colLeft; } set { _colLeft = value; OnPropertyChanged(); } }
        public int ColRight { get { return _colRight; } set { _colRight = value; OnPropertyChanged(); } }

        /// <summary>
        /// Command to move the player vertically.
        /// </summary>
        public DelegateCommand MovePlayerVertical { get; set; }

        /// <summary>
        /// Command to move the player horizontally.
        /// </summary>
        public DelegateCommand MovePlayerHorizontal { get; set; }

        /// <summary>
        /// Gets the easy game difficulty.
        /// </summary>
        public bool IsGameEasy
        {
            get { return _model.GameDifficulty == GameDifficulty.Easy; }
            set
            {
                if (_model.GameDifficulty == GameDifficulty.Easy)
                {
                    return;
                }

                _model.GameDifficulty = GameDifficulty.Easy;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }

        /// <summary>
        /// Gets the medium game difficulty.
        /// </summary>
        public bool IsGameMedium
        {
            get { return _model.GameDifficulty == GameDifficulty.Medium; }
            set
            {
                if (_model.GameDifficulty == GameDifficulty.Medium)
                {
                    return;
                }

                _model.GameDifficulty = GameDifficulty.Medium;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }

        /// <summary>
        /// Gets the hard game difficulty.
        /// </summary>
        public bool IsGameHard
        {
            get { return _model.GameDifficulty == GameDifficulty.Hard; }
            set
            {
                if (_model.GameDifficulty == GameDifficulty.Hard)
                {
                    return;
                }

                _model.GameDifficulty = GameDifficulty.Hard;
                OnPropertyChanged("IsGameEasy");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameHard");
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// ViewModel instance.
        /// </summary>
        /// <param name="model">The game model.</param>
        public StealthGameViewModel(StealthGameModel model)
        {
            // Init model and add events
            _model = model;
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_PlayerReachedExit);
            _model.GameCreated += new EventHandler<StealthGameEventArgs>(Model_GameCreated);

            // Commands
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            QuitGameCommand = new DelegateCommand(param => OnQuitGame());
            PauseCommand = new DelegateCommand(param => OnPauseGame());
            ResumeCommand = new DelegateCommand(param => OnResumeGame());
            HelpCommand = new DelegateCommand(param => OnHelp());

            MovePlayerVertical = new DelegateCommand(param => OnMoveVertical((int)param));
            MovePlayerHorizontal = new DelegateCommand(param => OnMoveHorizontal((int)param));

            EnablePause = true;
            EnableResume = false;

            // Game table
            Fields = new ObservableCollection<StealthGameField>();

            for (int i = 0; i < _model.Table.TableSize; ++i)
            {
                for (int j = 0; j < _model.Table.TableSize; ++j)
                {
                    Fields.Add(new StealthGameField
                    {
                        Row = i,
                        Col = j,
                    });
                }
            }

            RefreshTable();
            RefreshVisionCone();
            RefreshPlayerPos();
        }

        #endregion

        #region Public methods

        public void RefreshTable()
        {
            foreach (StealthGameField field in Fields)
            {
                field.IsFloor = _model.Table.GetValue(field.Row, field.Col) == "F";
                field.IsWall = _model.Table.GetValue(field.Row, field.Col) == "W";
                field.IsExit = _model.Table.GetValue(field.Row, field.Col) == "E";
                field.IsGuard = _model.Table.GetValue(field.Row, field.Col) == "G";

                if (field.IsGuard)
                {
                    _model.SetVisionCone(field.Row, field.Col, _model.Table);
                }
            }
        }

        public void RefreshVisionCone()
        {
            foreach (StealthGameField field in Fields)
            {
                field.IsVision = _model.Table.GetValue(field.Row, field.Col) == "V";
                field.IsVisionPlayer = _model.Table.GetValue(field.Row, field.Col) == "VP";
            }
        }

        public void RefreshPlayerPos()
        {
            foreach (StealthGameField field in Fields)
            {
                field.IsPlayer = _model.Table.GetValue(field.Row, field.Col) == "P";
            }

            RowUp = _model.Table.GetPlayerCoords()[0] - 1;
            RowDown = _model.Table.GetPlayerCoords()[0] + 1;
            ColLeft = _model.Table.GetPlayerCoords()[1] - 1;
            ColRight = _model.Table.GetPlayerCoords()[1] + 1;
        }

        #endregion

        #region Game event handlers

        /// <summary>
        /// Player detected event handler.
        /// </summary>
        private void Model_PlayerDetected(object sender, StealthGameEventArgs e)
        {
            RefreshTable();
            return;
        }

        /// <summary>
        /// Player reached exit event handler.
        /// </summary>
        private void Model_PlayerReachedExit(object sender, StealthGameEventArgs e)
        {
            RefreshTable();
            return;
        }

        /// <summary>
	    /// Game created event handler.
	    /// </summary>
		private void Model_GameCreated(object sender, StealthGameEventArgs e)
        {
            // Game table
            Fields.Clear();

            for (int i = 0; i < _model.Table.TableSize; ++i)
            {
                for (int j = 0; j < _model.Table.TableSize; ++j)
                {
                    Fields.Add(new StealthGameField
                    {
                        Row = i,
                        Col = j,
                    });
                }
            }

            RefreshTable();
            RefreshVisionCone();
            RefreshPlayerPos();
        }

        #endregion

        #region Events

        /// <summary>
        /// New game event.
        /// </summary>
        public event EventHandler NewGame;

        /// <summary>
        /// Load game event.
        /// </summary>
        public event EventHandler LoadGame;

        /// <summary>
        /// Save game event.
        /// </summary>
        public event EventHandler SaveGame;

        /// <summary>
        /// Quit game event.
        /// </summary>
        public event EventHandler QuitGame;

        /// <summary>
        /// Pause game event.
        /// </summary>
        public event EventHandler PauseGame;

        /// <summary>
        /// Resume game event.
        /// </summary>
        public event EventHandler ResumeGame;

        /// <summary>
        /// Help event.
        /// </summary>
        public event EventHandler Help;

        #endregion

        #region Event methods

        /// <summary>
        /// Fires the new game event.
        /// </summary>
        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the load game event.
        /// </summary>
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the save game event.
        /// </summary>
        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the quit game event.
        /// </summary>
        private void OnQuitGame()
        {
            QuitGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the pause game event.
        /// </summary>
        private void OnPauseGame()
        {
            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the resume game event.
        /// </summary>
        private void OnResumeGame()
        {
            ResumeGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the help event.
        /// </summary>
        private void OnHelp()
        {
            Help?.Invoke(this, EventArgs.Empty);
        }

        private void OnMoveVertical(int row)
        {
            if (EnablePause)
            {
                RefreshPlayerPos();
                _model.MovePlayerVertically(row);
                RefreshPlayerPos();
                _model.IsGameOver();
            }
            else
            {
                return;
            }
        }

        private void OnMoveHorizontal(int col)
        {
            if (EnablePause)
            {
                RefreshPlayerPos();
                _model.MovePlayerHorizontally(col);
                RefreshPlayerPos();
                _model.IsGameOver();
            }
            else
            {
                return;
            }
        }

        #endregion
    }
}
