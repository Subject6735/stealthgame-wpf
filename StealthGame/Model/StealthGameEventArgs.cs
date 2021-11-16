namespace StealthGame.Model
{
    /// <summary>
    /// StealthGame events, handles game events.
    /// </summary>
    public class StealthGameEventArgs
    {
        /// <summary>
        /// Checks if the game is over (detected or exit reached).
        /// </summary>
        private readonly bool _isOver;

        /// <summary>
        /// Gets if the game is over (detected or exit reached).
        /// </summary>
        public bool IsOver { get { return _isOver; } }

        /// <summary>
        /// StealthGameEventArgs constructor.
        /// </summary>
        /// <param name="isOver">True if the game is over (detected or exit reached), otherwise false.</param>
        public StealthGameEventArgs(bool isOver)
        {
            _isOver = isOver;
        }
    }
}
