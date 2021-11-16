using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using StealthGame.Model;
using StealthGame.Persistence;
using StealthGame.View;
using StealthGame.ViewModel;

namespace StealthGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private StealthGameModel _model;
        private StealthGameViewModel _viewModel;
        private MainWindow _view;
        private DispatcherTimer _timer;

        #endregion

        #region Constructors

        /// <summary>
        /// App instance
        /// </summary>
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handlers

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Create model
            _model = new StealthGameModel(new StealthGameFileDataAccess());
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_PlayerReachedExit);
            _model.NewGame();

            // Create viewmodel
            _viewModel = new StealthGameViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.LoadGame += new EventHandler(ViewModel_LoadGame);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGame);
            _viewModel.QuitGame += new EventHandler(ViewModel_QuitGame);
            _viewModel.PauseGame += new EventHandler(ViewModel_PauseGame);
            _viewModel.ResumeGame += new EventHandler(ViewModel_ResumeGame);
            _viewModel.Help += new EventHandler(ViewModel_Help);

            // Create view
            _view = new MainWindow
            {
                DataContext = _viewModel
            };

            _view.Show();

            // Create timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.MoveGuards();
            _viewModel.RefreshTable();
            _viewModel.RefreshVisionCone();
            _model.GuardDetect();
        }

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// New game event handler.
        /// </summary>
        private void ViewModel_NewGame(object sender, EventArgs e)
        {
            _model.NewGame();
            _timer.Start();
        }

        /// <summary>
        /// Load game event handler.
        /// </summary>
        private async void ViewModel_LoadGame(object sender, EventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;

            _timer.Stop();

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Load game",
                    Filter = "StealthGame|*.stga"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    await _model.LoadGameAsync(openFileDialog.FileName);

                    _timer.Start();
                }
            }
            catch (StealthGameDataException)
            {
                _ = MessageBox.Show("An error has occured when loading the file!", "Stealth Game", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (restartTimer)
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Save game event handler.
        /// </summary>
        private async void ViewModel_SaveGame(object sender, EventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;

            _timer.Stop();

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save game",
                    Filter = "StealthGame|*.stga"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveGameAsync(saveFileDialog.FileName);
                    }
                    catch (StealthGameDataException)
                    {
                        _ = MessageBox.Show("Játék mentése sikertelen!" + Environment.NewLine + "Hibás az elérési út, vagy a könyvtár nem írható.", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                _ = MessageBox.Show("An error has occured when saving the file!", "Stealth Game", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (restartTimer)
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Quit game event handler.
        /// </summary>
        private void ViewModel_QuitGame(object sender, EventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;
            _timer.Stop();

            if (MessageBox.Show("Are you sure you want to quit?", "Stealth Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _view.Close();
            }
            else
            {
                if (restartTimer)
                    _timer.Start();
            }
        }

        /// <summary>
        /// Pause game event handler.
        /// </summary>
        private void ViewModel_PauseGame(object sender, EventArgs e)
        {
            _timer.Stop();
            _viewModel.EnablePause = false;
            _viewModel.EnableResume = true;
        }

        /// <summary>
        /// Resume game event handler.
        /// </summary>
        private void ViewModel_ResumeGame(object sender, EventArgs e)
        {
            _timer.Start();
            _viewModel.EnablePause = true;
            _viewModel.EnableResume = false;
        }

        /// <summary>
        /// Help option event handler.
        /// </summary>
        private void ViewModel_Help(object sender, EventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;
            _timer.Stop();

            string help =
                "You are the green dot."
                + Environment.NewLine + "You have to reach the exit (indicated by a green area) to win the game."
                + Environment.NewLine + "Avoid being spotted by guards (red dots), they have a vision cone, indicated by blue areas."
                + Environment.NewLine + "To move, use W, A, S, D; you can only move vertically and horizontially."
                + Environment.NewLine + "You can change the difficulty by selecting the difficulty option then starting a new game."
                + Environment.NewLine + "To pause the game, select pause, to resume, select resume."
                + Environment.NewLine + "To start a new game, load a game, save the game or quit, select options.";

            if (MessageBox.Show(help, "Stealth Game", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                if (restartTimer)
                {
                    _timer.Start();
                }
            }
        }

        #endregion

        #region Model event handlers

        /// <summary>
        /// Handles the event when the player is detected.
        /// </summary>
        private void Model_PlayerDetected(object sender, StealthGameEventArgs e)
        {
            if (e.IsOver)
            {
                _timer.Stop();

                var msgbox = MessageBox.Show("Game over. You have been detected!" + Environment.NewLine + "Start new game (Yes) or quit (No)?", "Stealth Game", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (msgbox == MessageBoxResult.Yes)
                {
                    _model.NewGame();

                    _viewModel.EnablePause = true;
                    _viewModel.EnableResume = false;

                    _viewModel.RefreshTable();

                    _timer.Start();
                }
                else
                {
                    _view.Close();
                }
            }
        }

        /// <summary>
        /// Handles the event when the player reached the exit.
        /// </summary>
        private void Model_PlayerReachedExit(object sender, StealthGameEventArgs e)
        {
            if (e.IsOver)
            {
                _timer.Stop();

                var msgbox = MessageBox.Show("Congratulations, you won! You reached the exit." + Environment.NewLine + "Start new game (Yes) or quit (No)?", "Stealth Game", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (msgbox == MessageBoxResult.Yes)
                {
                    _model.NewGame();

                    _viewModel.EnablePause = true;
                    _viewModel.EnableResume = false;

                    _viewModel.RefreshTable();

                    _timer.Start();
                }
                else
                {
                    _view.Close();
                }
            }
        }

        #endregion
    }
}
