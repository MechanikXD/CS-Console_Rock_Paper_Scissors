using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleRockPaperScis.Enums;

namespace ConsoleRockPaperScis {
    internal class Program {
        #region Text Fields
        
        private const string HelpPrompt = "List of awailable commands:\n" +
                                          "exit - finish current session\n" +
                                          "help - display this prompt\n" +
                                          "statistics - display player statistics stored on current device\n" +
                                          "play - start new match agains pc.\n" +
                                          "then you type \'rock\', \'paper\' or \'scissors\' like any other commands to select your option.\n" +
                                          "game will end after asked amount of clashes were played.\n\n" +
                                          "any command can be shortend by typing first letters of the command (eg. \'exit\' can be typed as \'e\')";
        private readonly static string[] VictoryPrompts = {
            "Sharp moves, solid win!", "That was a masterstroke!", "Victory suits you well!",
            "Nothing can stop you now!", "You crushed it like a pro!"
        };
        private readonly static string[] LosePrompts = {
            "Luck wasn't on your side... this time.", "Every loss is a step to greatness.",
            "You'll get 'em next time!", "But hey, even legends stumble.", "It’s just a warm-up round!",
            "Keep your head up—you're learning fast!"
        };
        private const string HorizontalSeparator = "==============================================";
        
        #endregion
        
        private readonly static Random Rng = new Random();
        /*
        Player -> | Rock | Paper | Scissors
        -----------------------------------
        Rock     |  T       W        L
        Paper    |  L       T        W
        Scissors |  W       L        T
        */
        private readonly static GameResult[,] GameResults = {
            {GameResult.Tie, GameResult.PlayerWin, GameResult.PlayerLoss},
            {GameResult.PlayerLoss, GameResult.Tie, GameResult.PlayerWin},
            {GameResult.PlayerWin, GameResult.PlayerLoss, GameResult.Tie}
        };
        private readonly static HashSet<ConsoleCommand> CommandList = new HashSet<ConsoleCommand> {
            ConsoleCommand.Exit,
            ConsoleCommand.Play,
            ConsoleCommand.Statistics,
            ConsoleCommand.Help
        };

        #region Game Statistics

        private static string _playerNickname;
        private static uint _playerAge;

        private static int _rockChosen;
        private static int _paperChosen;
        private static int _scissorsChosen;

        private static int _totalGamesPlayed;
        private static int _gamesWon;

        #endregion
        
        public static void Main(string[] args) {
            if (TryRegisterUser()) {
                StartSession();
            }
        }

        #region Console Interactions

        private static bool TryRegisterUser() {
            Console.WriteLine("This is a simple console Rock-Paper-Scissors game!\n");
            Console.Write("Please Enter your username: ");
            _playerNickname = Console.ReadLine();
            Console.Write($"Hello {_playerNickname}! Please tell me your age: ");
            
            var parseResult = uint.TryParse(Console.ReadLine(), out var age);
            while (!parseResult) {
                Console.Write("Can't read the number, please try again: ");
                parseResult = uint.TryParse(Console.ReadLine(), out age);
            }

            _playerAge = age;

            if (_playerAge >= 12) {
                return true;
            }

            Console.WriteLine("Sorry, users under age of 12 can't play this game :(");
            return false;

        }
        
        private static void StartSession() {
            Console.WriteLine("Welcome! for more information type \'help\' or \'exit\' to finish this session");
            while (true) {
                Console.Write("Enter command: ");
                var input = ReadPlayerInput();
                if (TryReadCommand(input, out var command)) {
                    switch (command) {
                        case ConsoleCommand.Help:
                            Console.WriteLine(HelpPrompt);
                            break;
                        case ConsoleCommand.Exit:
                            Console.WriteLine($"Bye {_playerNickname}, see you next time!");
                            return;
                        case ConsoleCommand.Statistics:
                            Console.WriteLine($"{_playerNickname}'s(age of {_playerAge}) statistic:");
                            foreach (var pair in GetStatistics()) {
                                Console.WriteLine($"{pair.Key}: {pair.Value}");
                            }
                            break;
                        case ConsoleCommand.Play:
                            StartGame(3);
                            break;
                        default:
                            Console.WriteLine("Something went wrong... try again");
                            break;
                    }
                }
                else {
                    Console.WriteLine("No comprehensible command detected");
                }
            }
        }

        private static string ReadPlayerInput() {
            var playerInput = Console.ReadLine() ?? string.Empty;
            return playerInput;
        }

        private static bool TryReadCommand(string input, out ConsoleCommand result) {
            foreach (var possibleCommand in CommandList.Where(possibleCommand => possibleCommand.ToString()
                         .StartsWith(input, StringComparison.CurrentCultureIgnoreCase))) {
                result = possibleCommand;
                return true;
            }

            result = ConsoleCommand.NoCommand;
            return false;
        }

        private static void StartGame(int clashesInMatch) {
            var matchesWon = 0;
            var matchesLost = 0;
            for (var i = 0; i < clashesInMatch; i++) {
                Console.WriteLine($"Round {i + 1} / {clashesInMatch}");
                Console.Write("\nPick your fighter: ");
                var playerInput = ReadPlayerInput();

                try {
                    // get both inputs (form player and random) 
                    var playerOption = InterpretPlayerInput(playerInput);
                    var randomOption = RandomPickOption();
                    Console.Write($"Computer picked {randomOption.ToString()}. ");

                    // Switch output based on game result
                    var matchUpResult = GetMatchUpResult(playerOption, randomOption);
                    switch (matchUpResult) {
                        case GameResult.Tie:
                            Console.WriteLine("Game ends in a tie");
                            break;
                        case GameResult.PlayerLoss:
                            Console.WriteLine("You lost this round");
                            matchesLost += 1;
                            break;
                        case GameResult.PlayerWin:
                            Console.WriteLine("You won, yuppeeee!");
                            matchesWon += 1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(matchUpResult),
                                "Incomprehensible game result");
                    }
                }
                catch (Exception) {
                    Console.WriteLine("There was something wrong with your input, try again");
                    i--;
                }
            }

            Console.WriteLine(matchesWon > matchesLost
                ? $"You won the match. {VictoryPrompts[Rng.Next(VictoryPrompts.Length)]}"
                : $"You lost the match. {LosePrompts[Rng.Next(LosePrompts.Length)]}");
        }
        
        #endregion

        #region Game Logic

        private static GameOption RandomPickOption() => (GameOption)Rng.Next(3);
        
        private static GameResult GetMatchUpResult(GameOption playerOption, GameOption otherOption) {
            var gameResult = GameResults[(int)otherOption, (int)playerOption];
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

            if (gameResult == GameResult.PlayerWin) _gamesWon++;
            _totalGamesPlayed += 1;
            
            return gameResult;
        }

        private static Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                ["Rock Chosen"] = _rockChosen,
                ["Paper Chosen"] = _paperChosen,
                ["Scissors Chosen"] = _scissorsChosen,
                ["Total Games"] = _totalGamesPlayed,
                ["Games Won"] = _gamesWon,
                ["Win Rate"] = _totalGamesPlayed == 0 ? 0 : _gamesWon / _totalGamesPlayed
            };
        }

        private static GameOption InterpretPlayerInput(string playerInput) {
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

        #endregion
    }
}