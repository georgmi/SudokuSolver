using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public class SudokuCell
    {
        enum GridBox { TL, TC, TR, CL, CC, CR, BL, BC, BR };

        private int _row;
        private int _column;
        private GridBox _square;
        private int _value;
        private int _tempValue;
        private bool _locked;
        public List<int> PossValues = new List<int>();

        public SudokuCell(int row, int column)
        {
            if (row < 0 || row > 8 || column < 0 || column > 8)
            {
                throw new ArgumentOutOfRangeException("Rows and Columns must be in the range 0 to 8.");
            }
            _row = row;
            _column = column;
            this.Reset();

                if (row < 3)
                {
                    if (column < 3)
                    { _square = GridBox.TL; }
                    else if (column < 6)
                    { _square = GridBox.TC; }
                    else
                    { _square = GridBox.TR; }
                }
                else if (row < 6)
                {
                    if (column < 3)
                    { _square = GridBox.CL; }
                    else if (column < 6)
                    { _square = GridBox.CC; }
                    else
                    { _square = GridBox.CR; }
                }
                else
                {
                    if (column < 3)
                    { _square = GridBox.BL; }
                    else if (column < 6)
                    { _square = GridBox.BC; }
                    else
                    { _square = GridBox.BR; }
                }
        }

        public int Row
        { get { return _row; } }

        public int Column
        { get { return _column; } }

        public int Box
        { get { return (int)_square; } }

        public bool Locked
        { 
            get { return _locked; }
            set { _locked = value; }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                if (this.Locked)
                { throw new InvalidOperationException("Cell is locked."); }
                else
                {
                    if (value > -1 && value < 10)
                    {
                        //PossValues.Clear();
                        _value = value;
                        _tempValue = value;
                    }
                    else
                    {
                        _value = 0;
                    }
                }
            }
        }

        public int TempValue
        {
            get { return _tempValue; }
            set { _tempValue = value; }
        }

        public override string ToString()
        {
            if (this.Locked)
            { return this.Value.ToString(); }
            else
            { return "q"; }
        }

        public void Reset()
        {
            _value = 0;
            _tempValue = 0;
            _locked = false;

            PossValues.Clear();
            for (int count = 1; count < 10; count++)
            {
                PossValues.Add(count);
            }
        }

        public bool ShortenPossibles(List<SudokuCell> inputList)
        {
            bool changed = false;
            if (this.Value == 0)
            {
                foreach (SudokuCell othercell in inputList)
                {
                    if (!(this.Row == othercell.Row && this.Column == othercell.Column))
                    {
                        if (this.Row == othercell.Row || this.Column == othercell.Column || this.Box == othercell.Box)
                        {
                            if (othercell.Value != 0 && this.PossValues.Contains(othercell.Value))
                            {
                                this.PossValues.Remove(othercell.Value);
                                changed = true;
                            }
                        }
                    }
                }

            }
            return changed;
        }

        public bool Validate(List<SudokuCell> inputList)
        {
            if (this.Value != 0)
            {
                //Check for conflicts. Check rows, columns, and GridBox, but remember *not* to check the cell against itself.
                foreach (SudokuCell othercell in inputList)
                {
                    if (!(this.Row == othercell.Row && this.Column == othercell.Column))
                    {
                        if (this.Row == othercell.Row || this.Column == othercell.Column || this.Box == othercell.Box)
                        {
                            if (this.Value == othercell.Value)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
