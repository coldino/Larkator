using Larkator.Common;
using System.Windows;
using System.Windows.Media;

namespace LarkatorGUI
{
    public class DinoViewModel : DependencyObject
    {
        public DinoViewModel(Dino dino)
        {
            Dino = dino;
        }

        public Dino Dino
        {
            get { return (Dino)GetValue(DinoProperty); }
            set { SetValue(DinoProperty, value); }
        }

        public bool Highlight
        {
            get { return (bool)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(DinoViewModel), new PropertyMetadata(Colors.Red));

        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register("Highlight", typeof(bool), typeof(DinoViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty DinoProperty =
            DependencyProperty.Register("Dino", typeof(Dino), typeof(DinoViewModel), new PropertyMetadata(null));


        public static implicit operator Dino(DinoViewModel vm)
        {
            return vm.Dino;
        }

        public static implicit operator DinoViewModel(Dino dino)
        {
            return new DinoViewModel(dino);
        }
    }
}
