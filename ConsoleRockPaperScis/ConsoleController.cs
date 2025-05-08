using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleRockPaperScis {
    public class ConsoleController {
        private readonly HashSet<ConsoleCommand> _commandList = new HashSet<ConsoleCommand> {
            ConsoleCommand.Exit,
            ConsoleCommand.Play,
            ConsoleCommand.Statistics,
            ConsoleCommand.Help
        };

        private const string HelpPrompt = "This will be \'help\' console prompt";
        private const string WelcomePrompt =
            "This is a simple console Rock-Paper-Scissors game!\n" +
            "for more command information type \'help\' or \'exit\' to finish the session";

        public void StartSession() {
            Console.WriteLine(WelcomePrompt);
            var gameController = new GameController();
            gameController.ReadStatisticsFromFile();
            while (true) {
                Console.Write("Enter command: ");
                var input = ReadPlayerInput();
                if (TryReadCommand(input, out var command)) {
                    switch (command) {
                        case ConsoleCommand.Help:
                            Console.WriteLine(HelpPrompt);
                            break;
                        case ConsoleCommand.Exit:
                            gameController.SerializeStatistics();
                            return;
                        case ConsoleCommand.Statistics:
                            foreach (var pair in gameController.GetStatistics()) {
                                Console.WriteLine($"{pair.Key}: {pair.Value}");
                            }
                            break;
                        case ConsoleCommand.Play:
                            Console.Write("Enter number of clashes in following match: ");
                            if (!int.TryParse(ReadPlayerInput(), out var numberOfClashes)) {
                                Console.WriteLine("Can't read number of clashes, will be set to default (5)");
                                numberOfClashes = 5;
                            }
                            
                            StartGame(gameController, numberOfClashes);
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

        private string ReadPlayerInput() {
            var playerInput = Console.ReadLine() ?? string.Empty;
            return playerInput;
        }

        private bool TryReadCommand(string input, out ConsoleCommand result) {
            foreach (var possibleCommand in _commandList.Where(possibleCommand => possibleCommand.ToString()
                         .StartsWith(input, StringComparison.CurrentCultureIgnoreCase))) {
                result = possibleCommand;
                return true;
            }

            result = ConsoleCommand.NoCommand;
            return false;
        }

        private void StartGame(GameController gameController, int clashesInMatch) {
            var matchesWon = 0;
            var matchesLost = 0;
            for (var i = 0; i < clashesInMatch; i++) {
                Console.WriteLine($"Clash {i + 1} / {clashesInMatch}");
                Console.Write("\nPick your fighter: ");
                var playerInput = ReadPlayerInput();

                try {
                    // get both inputs (form player and random) 
                    var playerOption = gameController.InterpretPlayerInput(playerInput);
                    var randomOption = gameController.RandomPickOption();
                    Console.Write($"Computer picked {randomOption.ToString()}. ");

                    // Switch output based on game result
                    var matchUpResult = gameController.GetMatchUpResult(playerOption, randomOption);
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
    }
}