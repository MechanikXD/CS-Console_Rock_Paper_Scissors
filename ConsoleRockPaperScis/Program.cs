using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleRockPaperScis {
    internal class Program {
        private readonly static Random Rng = new Random();
        private readonly static JsonSerializer Serializer = new JsonSerializer();
        private const string HelpPrompt = "List of awailable commands:\n" +
                                          "exit - finish current session\n" +
                                          "help - display this prompt\n" +
                                          "statistics - display player statistics stored on current device\n" +
                                          "play - start new match agains pc.\n" +
                                          "after \'play\' command, you will be asked for amount of clashes(rounds) in this match.\n" +
                                          "then you type \'rock\', \'paper\' or \'scissors\' like any other commands to select your option.\n" +
                                          "game will end after asked amount of clashes were played.\n\n" +
                                          "any command can be shortend by typing first letters of the command (eg. \'exit\' can be typed as \'e\')";
        private const string WelcomePrompt =
            "This is a simple console Rock-Paper-Scissors game!\n" +
            "for more command information type \'help\' or \'exit\' to finish the session";
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
            Console.WriteLine(WelcomePrompt);
            ReadStatisticsFromFile();
            while (true) {
                Console.Write("Enter command: ");
                var input = ReadPlayerInput();
                if (TryReadCommand(input, out var command)) {
                    switch (command) {
                        case ConsoleCommand.Help:
                            Console.WriteLine(HelpPrompt);
                            break;
                        case ConsoleCommand.Exit:
                            SerializeStatistics();
                            return;
                        case ConsoleCommand.Statistics:
                            foreach (var pair in GetStatistics()) {
                                Console.WriteLine($"{pair.Key}: {pair.Value}");
                            }
                            break;
                        case ConsoleCommand.Play:
                            Console.Write("Enter number of clashes in following match: ");
                            if (!int.TryParse(ReadPlayerInput(), out var numberOfClashes)) {
                                Console.WriteLine("Can't read number of clashes, will be set to default (5)");
                                numberOfClashes = 5;
                            }
                            
                            StartGame(numberOfClashes);
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
                Console.WriteLine($"Clash {i + 1} / {clashesInMatch}");
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
                            Console.WriteLine("Game ends in a tie, try again");
                            break;
                        case GameResult.PlayerLoss:
                            Console.WriteLine("You lost, better luck next time");
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

            if (matchesWon > matchesLost) {
                Console.WriteLine("You won the match, congrats!");
            }
            else if (matchesWon == matchesLost) {
                Console.WriteLine("Match ended in tie, maybe return match?");
            }
            else {
                Console.WriteLine("You lost the match, better luck next time");
            }
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

        private static void SerializeStatistics() {
                    var serialized = JsonConvert.SerializeObject(GetStatistics());
                    File.WriteAllText("PlayerStatistics.json", serialized);
                }

        private static void ReadStatisticsFromFile() {
            var fileStream = File.Open("PlayerStatistics.json", FileMode.OpenOrCreate);
            var data = Serializer.Deserialize<Dictionary<string, int>>(
                    new JsonTextReader(new StreamReader(fileStream)));
            fileStream.Close();
            if (data == null) return; // file probably empty

            _rockChosen = data["Rock Chosen"];
            _paperChosen = data["Paper Chosen"];
            _scissorsChosen = data["Scissors Chosen"];
            _totalGamesPlayed = data["Total Games"];
            _gamesWon = data["Games Won"];
        }
    }
}