using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace diplomApp.View.ListPage
{
    public partial class Card : UserControl
    {
        private SolidColorBrush[] _brushes =
        {
            (SolidColorBrush)new BrushConverter().ConvertFromString("#ffa6a6"), //red
            (SolidColorBrush)new BrushConverter().ConvertFromString("#fff2a6"), //yellow
            (SolidColorBrush)new BrushConverter().ConvertFromString("#f6a6ff"), //pink
            (SolidColorBrush)new BrushConverter().ConvertFromString("#a6ffc3"), //green
            (SolidColorBrush)new BrushConverter().ConvertFromString("#a6ffff")  //blue
        };
        public Card()
        {
            InitializeComponent();

            Random random = new Random();
            ValueBorder.Background = _brushes[random.Next(0, _brushes.Length)];
        }
       
        //Caption property
        public static readonly DependencyProperty BoxCaptionProperty =
           DependencyProperty.Register(nameof(BoxCaption), typeof(string), typeof(Card),
               new PropertyMetadata("Описание"));

        public string BoxCaption
        {
            get => (string)GetValue(BoxCaptionProperty);
            set => SetValue(BoxCaptionProperty, value);
        }

        //Value property
        public static readonly DependencyProperty BoxValueProperty =
            DependencyProperty.Register(nameof(BoxValue), typeof(string), typeof(Card),
                new PropertyMetadata("Значение"));

        public string BoxValue
        {
            get => (string)GetValue(BoxValueProperty);
            set => SetValue(BoxValueProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Card),
                new PropertyMetadata(true));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
    }
}