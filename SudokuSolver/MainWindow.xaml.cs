using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SudokuCell> puzzleBoard = new List<SudokuCell>();
        public TextBox[,] tbMatrix = new TextBox[9,9];

        public MainWindow()
        {
            InitializeComponent();
            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    puzzleBoard.Add(new SudokuCell(row, column));

                    tbMatrix[row, column] = new TextBox();
                    PuzzleGrid.Children.Add(tbMatrix[row, column]);
                    int topMargin = ((60 * row) - 245);
                    int leftMargin = ((60 * column) - 245);
                    tbMatrix[row,column].Height = 25;
                    tbMatrix[row, column].Width = 25;
                    tbMatrix[row, column].Margin = new Thickness(leftMargin, topMargin, 0, 0);
                    tbMatrix[row, column].TextAlignment = TextAlignment.Center;
                    tbMatrix[row, column].Text = "";
                    tbMatrix[row, column].FontWeight = FontWeights.Bold;
                    tbMatrix[row, column].AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(tb_Changed));
                }
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tb_Changed(object sender, RoutedEventArgs e)
        {
            LoadPuzzleFromUI();
            if (CheckPuzzleForValidity())
            {
                lblStatusMessage.Content = "No conflicts found.";
            }
            else
            {
                lblStatusMessage.Content = "Please fix indicated conflicts.";
            }
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            LoadPuzzleFromUI();
            if (CheckPuzzleForValidity())
            {
                lblStatusMessage.Content = "No conflicts found.";
            }
            else
            {
                lblStatusMessage.Content = "Please fix indicated conflicts.";
            }
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            ClearTextChangedHandler();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            LoadPuzzleFromUI();
            lblStatusMessage.Content = "";
            if (CheckPuzzleForValidity())
            {
                //begin code segment for new thread
                {
                    foreach (SudokuCell cell in puzzleBoard)
                    {
                        if (cell.Value != 0)
                        {
                            cell.PossValues.Clear();
                        }
                    }
                    if (SolvePuzzle())
                    {
                        stopwatch.Stop();
                        lblStatusMessage.Content = "Puzzle solved. Elapsed time " + stopwatch.Elapsed.TotalSeconds.ToString() + "sec.";
                    }
                    else
                    {
                        stopwatch.Stop();
                        lblStatusMessage.Content = "Puzzle cannot be solved. Elapsed time " + stopwatch.Elapsed.TotalSeconds.ToString() + "sec.";
                    }
                } //end code segment for new thread.
            }
            else
            {
                stopwatch.Stop();
                lblStatusMessage.Content = "Please fix indicated conflicts.";
            }
            SetTextChangedHandler();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lblStatusMessage.Content = "";
            foreach(SudokuCell cell in puzzleBoard)
            {
                cell.Reset();
                tbMatrix[cell.Row, cell.Column].Text = "";
                tbMatrix[cell.Row, cell.Column].Background = Brushes.White;
            }
        }

        private void DisplayBoard()
        {
            foreach(SudokuCell cell in puzzleBoard)
            { }
        }

        private void LoadPuzzleFromUI()
        {
            foreach(SudokuCell cell in puzzleBoard)
            {
                int tempVal = 0;
                if (tbMatrix[cell.Row, cell.Column].Text != "")
                {
                    if (int.TryParse(tbMatrix[cell.Row, cell.Column].Text, out tempVal))
                    {
                        if (tempVal > 0 && tempVal < 10)
                        {
                            tbMatrix[cell.Row, cell.Column].Background = Brushes.White;
                            cell.Value = tempVal;
                        }
                        else
                        {
                            tbMatrix[cell.Row, cell.Column].Background = Brushes.Red;
                        }
                    }
                    else
                    {
                        tbMatrix[cell.Row, cell.Column].Background = Brushes.Red;
                        tbMatrix[cell.Row, cell.Column].Text = "";
                    }
                }
                else
                {
                    cell.Value = 0;
                }
            }
        }

        private bool CheckPuzzleForValidity()
        {
            bool valid = true;
            foreach (SudokuCell cell in puzzleBoard)
            {
                tbMatrix[cell.Row, cell.Column].Background = Brushes.White;
                if(!cell.Validate(puzzleBoard))
                {
                    tbMatrix[cell.Row, cell.Column].Background = Brushes.Red;
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                    valid = false;
                }
            }
            return valid;
        }

        private bool SolvePuzzle()
        {
            bool solved = false;
            //ReduceUnknowns(); //For difficult puzzles, reducing unknowns actually *increases* time to solve puzzle. WTF?
            List<SudokuCell> unsolvedCells = new List<SudokuCell>();
            foreach (SudokuCell cell in puzzleBoard)
            {
                if (cell.Value == 0)
                {
                    unsolvedCells.Add(cell);
                }
            }
            solved = iterativeSolve(tbMatrix, unsolvedCells);
            //solved = RecursiveSolve(tbMatrix, unsolvedCells);
            return solved;
        }

        private void ReduceUnknowns()
        {
            bool keepGoing = true;

            while (keepGoing)
            {
                keepGoing = false;
                foreach (SudokuCell cell in puzzleBoard)
                {
                    keepGoing = cell.ShortenPossibles(puzzleBoard);
                    if (cell.PossValues.Count == 1)
                    {
                        cell.Value = cell.PossValues[0];
                        cell.PossValues.Clear();
                        tbMatrix[cell.Row, cell.Column].Text = cell.Value.ToString();
                    }

                }
            }
        }

        private bool iterativeSolve(TextBox[,] textBoxes, List<SudokuCell> inputList)
        {
            if (inputList.Count == 0)
            {
                return CheckPuzzleForValidity();
            }

            int iterator = 0;

            while (iterator < inputList.Count && iterator > -1)
            {
                if(inputList[iterator].PossValues.Count == 0)
                {
                    inputList[iterator].Reset();
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Text = "";
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Background = Brushes.White;
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                }
                foreach (int tryValue in inputList[iterator].PossValues)
                {
                    inputList[iterator].Value = tryValue;
                    if (CheckPuzzleForValidity())
                    {
                        textBoxes[inputList[iterator].Row, inputList[iterator].Column].Text = inputList[iterator].Value.ToString();
                        tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Background = Brushes.White;
                        Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                        iterator++;
                        break;
                    }
                }
                if (!CheckPuzzleForValidity())
                {
                    inputList[iterator].Reset();
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Text = "";
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Background = Brushes.White;
                    iterator--;
                    if(iterator < 0)
                    {
                        return false;
                    }
                    inputList[iterator].PossValues.Remove(inputList[iterator].Value);
                    inputList[iterator].Value = 0;
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Text = "";
                    tbMatrix[inputList[iterator].Row, inputList[iterator].Column].Background = Brushes.White;
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                }
            }

            return CheckPuzzleForValidity();
        }

        private bool RecursiveSolve(TextBox[,] textBoxes, List<SudokuCell> inputList)
        {
            if (inputList.Count == 0)
            {
                return CheckPuzzleForValidity();
            }

            int iterator = 0;

            while(iterator < inputList.Count && iterator > -1)
            {
                foreach(int tryValue in inputList[iterator].PossValues)
                {
                    inputList[iterator].Value = tryValue;
                    if(CheckPuzzleForValidity())
                    {
                        textBoxes[inputList[iterator].Row, inputList[iterator].Column].Text = inputList[iterator].Value.ToString();
                        iterator++;
                        break;
                    }
                }
                if(!CheckPuzzleForValidity())
                {
                    inputList[iterator].Reset();
                    iterator--;
                    inputList[iterator].PossValues.Remove(inputList[iterator].Value);
                    inputList[iterator].Value = 0;
                }
                if(iterator == -1)
                {
                    iterator = -1;
                }
            }

            return CheckPuzzleForValidity();
        }

        private void SetTextChangedHandler()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    tbMatrix[row, column].AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(tb_Changed));
                }
            }
        }

        private void ClearTextChangedHandler()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    tbMatrix[row, column].RemoveHandler(TextBox.TextChangedEvent, new RoutedEventHandler(tb_Changed));
                }
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            lblStatusMessage.Content = "";
            foreach (SudokuCell cell in puzzleBoard)
            {
                cell.Reset();
                tbMatrix[cell.Row, cell.Column].Text = "";
                tbMatrix[cell.Row, cell.Column].Background = Brushes.White;
            }

            tbMatrix[0, 0].Text = "2";
            tbMatrix[0, 1].Text = "3";
            tbMatrix[0, 6].Text = "4";

            tbMatrix[1, 2].Text = "8";
            tbMatrix[1, 5].Text = "5";
            tbMatrix[1, 6].Text = "6";
            tbMatrix[1, 8].Text = "1";

            tbMatrix[2, 0].Text = "1";
            tbMatrix[2, 2].Text = "5";
            tbMatrix[2, 5].Text = "9";

            tbMatrix[3, 0].Text = "3";
            tbMatrix[3, 5].Text = "2";
            tbMatrix[3, 6].Text = "1";

            tbMatrix[4, 1].Text = "5";
            tbMatrix[4, 3].Text = "3";
            tbMatrix[4, 5].Text = "8";
            tbMatrix[4, 7].Text = "4";

            tbMatrix[5, 2].Text = "2";
            tbMatrix[5, 3].Text = "6";
            tbMatrix[5, 8].Text = "7";

            tbMatrix[6, 3].Text = "1";
            tbMatrix[6, 6].Text = "8";
            tbMatrix[6, 8].Text = "9";

            tbMatrix[7, 0].Text = "5";
            tbMatrix[7, 2].Text = "1";
            tbMatrix[7, 3].Text = "9";
            tbMatrix[7, 6].Text = "2";

            tbMatrix[8, 2].Text = "7";
            tbMatrix[8, 7].Text = "1";
            tbMatrix[8, 8].Text = "4";

            //LoadPuzzleFromUI();
            //ReduceUnknowns();

            //foreach (SudokuCell cell in puzzleBoard)
            //{
            //    tbMatrix[cell.Row, cell.Column].Text = cell.PossValues.Count.ToString();
            //}
        }
    }
}
