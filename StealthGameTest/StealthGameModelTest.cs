using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StealthGame.Model;
using StealthGame.Persistence;

namespace StealthGame.Test
{
    [TestClass]
    public class StealthGameModelTest
    {
        private StealthGameModel _model;
        private StealthGameTable _mockedTable;
        private Mock<IStealthGameDataAccess> _mock;

        [TestInitialize]
        public void Initialize()
        {
            // Generate a test table
            int tableSize = 10;

            _mockedTable = new StealthGameTable(tableSize)
            {
                Guards = new List<Tuple<int, int, int>>()
            };

            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    if (i == 0 || i == 9)
                        _mockedTable.SetValue(i, j, "W");
                    else if (j == 0 || j == 9)
                        _mockedTable.SetValue(i, j, "W");
                    else
                        _mockedTable.SetValue(i, j, "F");
                }
            }

            _mockedTable.SetValue(8, 1, "P");
            _mockedTable.SetValue(1, 8, "E");
            _mockedTable.SetValue(4, 2, "G");
            _mockedTable.Guards.Add(new Tuple<int, int, int>(4, 2, 3)); // Moves to bottom

            _mock = new Mock<IStealthGameDataAccess>();

            // Always load the previously declared table
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult(_mockedTable));

            // Initialize model object
            _model = new StealthGameModel(_mock.Object)
            {
                SetTable = _mockedTable
            };
        }

        [TestMethod]
        public void Model_NewGameEasyTest()
        {
            _model.GameDifficulty = GameDifficulty.Easy;
            _model.NewGame();
            Assert.AreEqual(GameDifficulty.Easy, _model.GameDifficulty);
            Assert.AreEqual(_model.Table.TableSize, 20);
        }

        [TestMethod]
        public void Model_NewGameMediumTest()
        {
            _model.GameDifficulty = GameDifficulty.Medium;
            _model.NewGame();
            Assert.AreEqual(GameDifficulty.Medium, _model.GameDifficulty);
            Assert.AreEqual(_model.Table.TableSize, 30);
        }

        [TestMethod]
        public void Model_NewGameHardTest()
        {
            _model.GameDifficulty = GameDifficulty.Hard;
            _model.NewGame();
            Assert.AreEqual(GameDifficulty.Hard, _model.GameDifficulty);
            Assert.AreEqual(_model.Table.TableSize, 40);
        }

        [TestMethod]
        public void Model_PlayerMoveTest()
        {
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerNotDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_ExitNotReached);

            _model.Detected = false;
            _model.ExitReached = false;

            Assert.IsTrue(_model.Table.IsPlayer(8, 1));
            _model.MovePlayer(8, 2);
            Assert.IsTrue(_model.Table.IsPlayer(8, 2));
        }

        [TestMethod]
        public void Model_PlayerReachedExit()
        {
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerNotDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_ExitReached);
            _model.Detected = false;
            _model.ExitReached = true;

            _model.MovePlayer(1, 8);
        }

        [TestMethod]
        public void Model_PlayerDetectedByPlayerMove()
        {
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_ExitNotReached);
            _model.ExitReached = false;

            _model.SetVisionCone(4, 2, _model.Table);
            _model.Detected = _model.VisionConeArea[6, 2] == 1;
            _model.MovePlayer(6, 2);
        }

        [TestMethod]
        public void Model_PlayerDetectedByGuardMove()
        {
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_ExitNotReached);
            _model.ExitReached = false;

            _model.Table.SetValue(4, 2, "F");
            _model.Table.SetValue(6, 2, "G");
            _model.SetVisionCone(6, 2, _model.Table);
        }

        [TestMethod]
        public async Task Model_LoadTest()
        {
            _model.NewGame();

            await _model.LoadGameAsync(string.Empty);

            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < 10; ++j)
                {
                    Assert.AreEqual(_mockedTable.GetValue(i, j), _model.Table.GetValue(i, j));
                }

            _mock.Verify(dataAccess => dataAccess.LoadAsync(string.Empty), Times.Once());
        }

        private void Model_PlayerNotDetected(object sender, StealthGameEventArgs e)
        {
            Assert.IsFalse(e.IsOver);
        }

        private void Model_PlayerDetected(object sender, StealthGameEventArgs e)
        {
            Assert.IsTrue(e.IsOver);
        }

        private void Model_ExitNotReached(object sender, StealthGameEventArgs e)
        {
            Assert.IsFalse(e.IsOver);
        }

        private void Model_ExitReached(object sender, StealthGameEventArgs e)
        {
            Assert.IsTrue(e.IsOver);
        }
    }
}
