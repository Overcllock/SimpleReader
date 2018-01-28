using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SimpleReader {
    public partial class BookAddingWindow : Window {
        Book newBook;
        Settings settings;
        string coverPath;
        public BookAddingWindow(Settings base_settings) {
            coverPath = string.Empty;
            settings = base_settings;
            InitializeComponent();
        }

        public Book GetBook() { return newBook; }

        private void _Accept__Click(object sender, RoutedEventArgs e) {
            if ((bool)_InternetResource_.IsChecked) {
                if (Path.GetExtension(_Path_.Text).Equals(string.Empty)) {
                    MessageBox.Show("Ошибка при добавлении книги: указанная ссылка не содержит документа.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    return;
                }
                newBook = new Book(_Path_.Text, Book.ResourceType.Internet);
            }
            if (newBook == null) {
                MessageBox.Show("Ошибка при добавлении книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                return;
            }
            if (!coverPath.Equals(string.Empty)) {
                newBook.SetCoverSource(coverPath);
                if (!Directory.Exists("Covers"))
                    Directory.CreateDirectory("Covers");
                string newFilePath = string.Format("Covers/{0}{1}", newBook.GetName(), Path.GetExtension(coverPath));
                if (!File.Exists(newFilePath))
                    File.Copy(coverPath, newFilePath);
            }
            if (!Directory.Exists("Books"))
                Directory.CreateDirectory("Books");
            if ((bool)_LocalFile_.IsChecked) {
                string newPath = string.Format("Books/{0}{1}", newBook.GetName(), Path.GetExtension(newBook.GetSource()));
                if (!File.Exists(newPath))
                    File.Copy(newBook.GetSource(), newPath);
            }
            newBook.SetAuthor(_Author_.Text);
            newBook.SetName(_Name_.Text);
            newBook.IsFavorite = (bool)_IsFavorite_.IsChecked;
            DialogResult = true;
        }

        private void _Cancel__Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void _AddCover__Click(object sender, RoutedEventArgs e) {
            OpenFileDialog coverDialog = new OpenFileDialog();
            coverDialog.Title = "Выбор обложки";
            coverDialog.Filter = "Изображения (*.jpg; *.png; *.bmp) | *.jpg; *.png; *.bmp";
            if ((bool)coverDialog.ShowDialog()) {
                _AddCover_.Background = new ImageBrush(new BitmapImage(new Uri(coverDialog.FileName)));
                _AddCover_.Content = string.Empty;
                coverPath = coverDialog.FileName;
            }
        }

        private void _PathChosing__PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if ((bool)_InternetResource_.IsChecked) return;
            OpenFileDialog bookDialog = new OpenFileDialog();
            bookDialog.Title = "Выбор файла";
            bookDialog.Filter = "PDF-файлы (*.pdf) | *.pdf";
            if ((bool)bookDialog.ShowDialog()) {
                newBook = new Book(bookDialog.FileName, Book.ResourceType.Local);
                _Path_.Text = bookDialog.FileName;
                _Name_.Text = newBook.GetName();
                _Size_.Text = newBook.GetSize();
            }
        }

        /*********************************************************************/
        private void _PathChosing__MouseEnter(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            if ((bool)_LocalFile_.IsChecked) {
                var anim = new DoubleAnimation(1, 0.8, TimeSpan.FromMilliseconds(250));
                (sender as Label).BeginAnimation(OpacityProperty, anim);
            }
        }
        private void _PathChosing__MouseLeave(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            if ((bool)_LocalFile_.IsChecked) {
                var anim = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(250));
                (sender as Label).BeginAnimation(OpacityProperty, anim);
            }
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
        private void _LocalFile__Checked(object sender, RoutedEventArgs e) {
            if (_Path_ == null) return;
            _Path_.Text = string.Empty;
            _Path_.IsEnabled = false;
            _Author_.Text = string.Empty;
            _Size_.Text = string.Empty;
        }
        private void _InternetResource__Checked(object sender, RoutedEventArgs e) {
            if (_Path_ == null) return;
            _Path_.Text = string.Empty;
            _Path_.IsEnabled = true;
            _Author_.Text = string.Empty;
            _Size_.Text = string.Empty;
        }
        private void _Path__TextChanged(object sender, TextChangedEventArgs e) {
            if (_Path_ == null) return;
            _Name_.Text = Book.GetNameByPath(_Path_.Text);
        }
    }
}
