using System;

namespace ConsoleRockPaperScis {
    internal class Program {
        public static void Main(string[] args) {
            Console.Write("This is simple console Rock-Paper-Scissors game. To exit the game type \"end\" in console");
            var gameController = new GameController();
            
            // Strangely, but they are type of int, then division between they will be int as well
            double gamesPlayed = 0;
            double gamesWon = 0;
            
            while (true) {
                Console.Write("\nPick your fighter: ");
                // Read player input and check for null
                var playerInput = Console.ReadLine() ?? string.Empty;
                // Check for 'end' command
                if ("end".StartsWith(playerInput, StringComparison.CurrentCultureIgnoreCase)) {
                    if (gamesPlayed > 0) Console.WriteLine($"Your win rate: {Math.Round(gamesWon / gamesPlayed * 100, 3)}%");
                    break;
                }
                
                try {
                    // get both inputs (form player and random) 
                    var playerOption = gameController.InterpretPlayerInput(playerInput);
                    var randomOption = gameController.RandomPickOption();
                    Console.Write($"Computer picked {randomOption.ToString()}. ");
                    // Switch output based on game result
                    var matchUpResult = gameController.GetMatchUpResult(playerOption, randomOption);
                    gamesPlayed++;
                    switch (matchUpResult) {
                        case GameResult.Tie:
                            Console.WriteLine("Game ends in a tie, try again");
                            break;
                        case GameResult.PlayerLoss:
                            Console.WriteLine("You lost, better luck next time");
                            break;
                        case GameResult.PlayerWin:
                            gamesWon++;
                            Console.WriteLine("You won, yuppeeee!");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(matchUpResult), "Incomprehensible game result");
                    }
                }
                catch (ArgumentException) {
                    Console.WriteLine("ooops, probably bad player input...");
                }
            }
        }
    }
}