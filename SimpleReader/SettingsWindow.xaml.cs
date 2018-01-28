using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SimpleReader {
    public partial class SettingsWindow : Window {
        Settings settings;
        public SettingsWindow(Settings base_settings) {
            InitializeComponent();
            settings = base_settings;
            _Animation_.IsChecked = base_settings.IsAnimationEnabled ? true : false;
            _Fullscreen_.IsChecked = base_settings.IsFullscreen ? true : false;
            _Sort_.IsChecked = base_settings.IsSortingEnabled ? true : false;
        }

        public Settings GetSettings() { return settings; }

        private void _Accept__Click(object sender, RoutedEventArgs e) {
            settings.SetFullscreenMode((bool)_Fullscreen_.IsChecked);
            if (_Animation_.IsChecked.Value) settings.EnableAnimation();
            else settings.DisableAnimation();
            if (_Sort_.IsChecked.Value) settings.EnableSorting();
            else settings.DisableSorting();
            DialogResult = true;
        }

        private void _Cancel__Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void _Button__MouseEnter(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(150));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
        private void _Button__MouseLeave(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(150));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
    }
}
