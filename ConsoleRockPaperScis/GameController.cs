using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ConsoleRockPaperScis {
    public class GameController {
        private readonly Random _rng = new Random();
        private readonly JsonSerializer _serializer = new JsonSerializer();
        /*
        Player -> | Rock | Paper | Scissors
        -----------------------------------
        Rock     |  T       W        L
        Paper    |  L       T        W
        Scissors |  W       L        T
        */
        private readonly GameResult[,] _gameResults = {
            {GameResult.Tie, GameResult.PlayerWin, GameResult.PlayerLoss},
            {GameResult.PlayerLoss, GameResult.Tie, GameResult.PlayerWin},
            {GameResult.PlayerWin, GameResult.PlayerLoss, GameResult.Tie}
        };

        #region Game Statistics

        private int _rockChosen;
        private int _paperChosen;
        private int _scissorsChosen;

        private int _totalGamesPlayed;
        private int _gamesWon;
        private int _gamesLost;

        #endregion

        public void SerializeStatistics() {
            var serialized = JsonConvert.SerializeObject(GetStatistics());
            File.WriteAllText("PlayerStatistics.json", serialized);
        }

        public void ReadStatisticsFromFile() {
            var fileStream = File.Open("PlayerStatistics.json", FileMode.OpenOrCreate);
            var data = _serializer.Deserialize<Dictionary<string, int>>(
                    new JsonTextReader(new StreamReader(fileStream)));
            fileStream.Close();
            if (data == null) return; // file probably empty

            _rockChosen = data["Rock Chosen"];
            _paperChosen = data["Paper Chosen"];
            _scissorsChosen = data["Scissors Chosen"];
            _totalGamesPlayed = data["Total Games"];
            _gamesWon = data["Games Won"];
            _gamesLost = data["Games Lost"];
        }
    
        public GameOption RandomPickOption() {
            return (GameOption)_rng.Next(3);
        }

        public GameResult GetMatchUpResult(GameOption playerOption, GameOption otherOption) {
            var gameResult = _gameResults[(int)otherOption, (int)playerOption];
            // Record player statistics
            switch (playerOption) {
                case GameOption.Rock:
                    _rockChosen += 1;
                    break;
                case GameOption.Paper:
                    _paperChosen += 1;
                    break;
                case GameOption.Scissors:
                    _scissorsChosen += 1;
                    break;
                default:
                    throw new ArgumentException("Cannot define player input!",
                        nameof(playerOption));
            }

            switch (gameResult) {
                case GameResult.PlayerWin:
                    _gamesWon += 1;
                    break;
                case GameResult.PlayerLoss:
                    _gamesLost += 1;
                    break;
            }

            _totalGamesPlayed += 1;
            
            return gameResult;
        }

        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                ["Rock Chosen"] = _rockChosen,
                ["Paper Chosen"] = _paperChosen,
                ["Scissors Chosen"] = _scissorsChosen,
                ["Total Games"] = _totalGamesPlayed,
                ["Games Won"] = _gamesWon,
                ["Games Tie"] = _totalGamesPlayed - _gamesWon - _gamesLost,
                ["Games Lost"] = _gamesLost,
                ["Win Rate"] = _totalGamesPlayed == 0 ? 0 : _gamesWon / _totalGamesPlayed
            };
        }

        public GameOption InterpretPlayerInput(string playerInput) {
            GameOption playerOption;
            // Convert player input into Options by first letters in the word, e.g 'r' is first letter of 'rock'
            // Should prevent too long words that contain key words like 'rockpaper', since they too long.
            if (GameOption.Paper.ToString().StartsWith(playerInput, StringComparison.CurrentCultureIgnoreCase)) {
                playerOption = GameOption.Paper;
            }
            else if (GameOption.Rock.ToString().StartsWith(playerInput, StringComparison.CurrentCultureIgnoreCase)) {
                playerOption = GameOption.Rock;
            }
            else if (GameOption.Scissors.ToString().StartsWith(playerInput, StringComparison.CurrentCultureIgnoreCase)) {
                playerOption = GameOption.Scissors;
            }
            else {
                throw new ArgumentException("Can't convert given player input to option", nameof(playerInput));
            }

            return playerOption;
        }
    }
}