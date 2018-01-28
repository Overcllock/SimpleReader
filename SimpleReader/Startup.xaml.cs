using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace SimpleReader {
    public partial class Startup : Window {
        public Startup() {
            InitializeComponent();
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(2000));
            anim.Completed += Anim_Completed;
            MainGrid.BeginAnimation(OpacityProperty, anim);
        }

        private void Anim_Completed(object sender, EventArgs e) {
            Thread.Sleep(3000);
            DialogResult = true;
        }
    }
}
