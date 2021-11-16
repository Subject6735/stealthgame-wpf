using System.Threading.Tasks;

namespace StealthGame.Persistence
{
    /// <summary>
    /// StealthGame control interface.
    /// </summary>
    public interface IStealthGameDataAccess
    {
        /// <summary>
        /// Loads the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>The game table read from the file.</returns>
        Task<StealthGameTable> LoadAsync(string path);

        /// <summary>
        /// Saves the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="table">The game table to write to the file.</param>
        Task SaveAsync(string path, StealthGameTable table);
    }
}
