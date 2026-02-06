using System.ComponentModel;
using G00DS0ULCHECKERS.Model;

namespace G00DS0ULCHECKERS.ViewModel
{
    public class Square : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Row { get; set; }
        public int Column { get; set; }
        public Piece? CurrentPiece { get; set; }
        public bool IsSelected { get; set; }
        public bool IsDark => (Row + Column) % 2 != 0;


        public Square(int row, int col, Piece? piece)
        {
            Row = row;
            Column = col;
            CurrentPiece = piece;
        }
    }
}

