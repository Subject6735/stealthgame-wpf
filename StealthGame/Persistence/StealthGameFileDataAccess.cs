using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StealthGame.Persistence
{
    /// <summary>
    /// StealthGame file handler.
    /// </summary>
    public class StealthGameFileDataAccess : IStealthGameDataAccess
    {
        /// <summary>
        /// Loads the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>The StealthGameTable read from the file.</returns>
        public async Task<StealthGameTable> LoadAsync(string path)
        {
            try
            {
                using StreamReader reader = new StreamReader(path);

                int tableSize = int.Parse(await reader.ReadLineAsync());

                StealthGameTable table = new StealthGameTable(tableSize)
                {
                    Guards = new List<Tuple<int, int, int>>()
                };

                for (int i = 0; i < tableSize; ++i)
                {
                    string[] line = (await reader.ReadLineAsync()).Split(" ");

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

                return table;
            }
            catch
            {
                throw new StealthGameDataException();
            }
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="table">The StealthGameTable to write to the file.</param>
        public async Task SaveAsync(string path, StealthGameTable table)
        {
            try
            {
                using StreamWriter writer = new StreamWriter(path);

                writer.Write(table.TableSize);

                await writer.WriteLineAsync("");

                for (int i = 0; i < table.TableSize; ++i)
                {
                    for (int j = 0; j < table.TableSize; ++j)
                    {
                        if (table.IsVision(i, j))
                            await writer.WriteAsync("F ");
                        else
                            await writer.WriteAsync(table.GetValue(i, j) + " ");
                    }

                    await writer.WriteLineAsync("");
                }
            }
            catch
            {
                throw new StealthGameDataException();
            }
        }
    }
}
