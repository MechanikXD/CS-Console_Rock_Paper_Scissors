using System;

namespace ConsoleRockPaperScis {
    public class GameController {
        private readonly Random _rng = new Random();
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
    
        public GameOption RandomPickOption() {
            return (GameOption)_rng.Next(3);
        }

        public GameResult GetMatchUpResult(GameOption playerOption, GameOption otherOption) {
            return _gameResults[(int)otherOption, (int)playerOption];
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