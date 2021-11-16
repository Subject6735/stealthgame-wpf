using System;
using System.Collections.Generic;
using System.Text;

namespace StealthGame.ViewModel
{
    /// <summary>
    /// StealthGame Field (The game table)
    /// </summary>
    public class StealthGameField : ViewModelBase
    {
        /// <summary>
        /// Field types
        /// </summary>
        private bool _isFloor;
        private bool _isWall;
        private bool _isExit;
        private bool _isGuard;
        private bool _isVision;
        private bool _isPlayer;
        private bool _isVisionPlayer;

        /// <summary>
        /// Getters and setters for all field types
        /// </summary>
        public bool IsFloor { get { return _isFloor; } set { _isFloor = value; OnPropertyChanged(); } }
        public bool IsWall { get { return _isWall; } set { _isWall = value; OnPropertyChanged(); } }
        public bool IsExit { get { return _isExit; } set { _isExit = value; OnPropertyChanged(); } }
        public bool IsGuard { get { return _isGuard; } set { _isGuard = value; OnPropertyChanged(); } }
        public bool IsVision { get { return _isVision; } set { _isVision = value; OnPropertyChanged(); } }
        public bool IsPlayer { get { return _isPlayer; } set { _isPlayer = value; OnPropertyChanged(); } }
        public bool IsVisionPlayer { get { return _isVisionPlayer; } set { _isVisionPlayer = value; OnPropertyChanged(); } }

        /// <summary>
        /// Gets or sets the row coordinate.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Gets or sets the col coordinate.
        /// </summary>
        public int Col { get; set; }
    }
}
