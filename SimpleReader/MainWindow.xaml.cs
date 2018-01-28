using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace SimpleReader {
    public partial class MainWindow : Window {
        const string FILE_PATH = "data.json";
        const string CONFIG_PATH = "config.ini";
        Thread internetConnectionChecker;
        private int connectionStatus = -1;
        private bool IsReadingModeActive;
        private bool IsFavoritesVisible;
        List<Book> Books;
        Settings settings;

        public MainWindow() {
            Microsoft.Win32.RegistryKey adobe = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Adobe");
            Microsoft.Win32.RegistryKey acrobatReader = adobe.OpenSubKey("Acrobat Reader");
            if (adobe == null || acrobatReader == null) {
                MessageBox.Show("Для работы приложения необходим Adobe Reader.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Diagnostics.Process.Start("https://get.adobe.com/ru/reader/");
                Environment.Exit(1);
            }
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            Opacity = 0;
            _Search_.TextChanged += _Search__TextChanged;
            Books = new List<Book>();
            settings = new Settings();
            IsReadingModeActive = false;
            IsFavoritesVisible = false;
            _Add_.Opacity = 0;
            _Favorites_.Opacity = 0;
            _Settings_.Opacity = 0;
            _ReadingMode_.Opacity = 0;
            internetConnectionChecker = new Thread(CheckInternetConnection);
            internetConnectionChecker.Start();
            LoadData();
            AcceptSettings();
            Startup window = new Startup();
            window.ShowDialog();
            Visibility = Visibility.Visible;
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1500));
            BeginAnimation(OpacityProperty, anim);
        }

        //Загрузка данных
        private void LoadData() {
            if (File.Exists(CONFIG_PATH)) {
                DataContractJsonSerializer jsonSettingsFormatter = new DataContractJsonSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(CONFIG_PATH, FileMode.Open)) {
                    settings = (Settings)jsonSettingsFormatter.ReadObject(fs);
                }
            }
            if (!File.Exists(FILE_PATH)) return;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Book>));
            using (FileStream fs = new FileStream(FILE_PATH, FileMode.Open)) {
                Books = (List<Book>)jsonFormatter.ReadObject(fs);
            }
        }

        //Сохранение данных
        private void SaveData() {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Book>));
            DataContractJsonSerializer jsonSettingsFormatter = new DataContractJsonSerializer(typeof(Settings));
            if (File.Exists(CONFIG_PATH)) File.Delete(CONFIG_PATH);
            if (File.Exists(FILE_PATH)) File.Delete(FILE_PATH);
            using (FileStream fs = new FileStream(FILE_PATH, FileMode.OpenOrCreate)) {
                jsonFormatter.WriteObject(fs, Books);
            }
            using (FileStream fs = new FileStream(CONFIG_PATH, FileMode.OpenOrCreate)) {
                jsonSettingsFormatter.WriteObject(fs, settings);
            }
        }

        //Обновить отображаемый список книг
        private void UpdateBookGrid(bool isFavorites) {
            if (!_Search_.IsFocused) _Search_.Text = "Введите название книги";
            _Header_.Content = isFavorites ? "Избранное" : "Мои книги";
            BooksPanel.Children.Clear();
            foreach (Book book in Books) {
                if (!isFavorites || (isFavorites && book.IsFavorite)) {
                    Button bookControl = GetCopyOfTemplate();
                    if (!book.GetCoverSource().Equals(string.Empty)) {
                        bookControl.Background = new ImageBrush(new BitmapImage(new Uri(book.GetCoverSource())));
                        bookControl.Content = string.Empty;
                    }
                    bookControl.Click += _Book_Click;
                    bookControl.MouseEnter += _Book__MouseEnter;
                    bookControl.MouseLeave += _Book__MouseLeave;
                    bookControl.Style = (Style)FindResource("ButtonStyle1");
                    bookControl.ContextMenu = CreateContextMenu(book.IsFavorite, bookControl);
                    bookControl.Tag = book;
                    bookControl.ToolTip = book.ToString();
                    BooksPanel.Children.Add(bookControl);
                }
            }
        }

        //Применить настройки
        private void AcceptSettings() {
            if (!settings.IsAnimationEnabled) {
                _Add_.Opacity = 1;
                _Favorites_.Opacity = 0;
                _Settings_.Opacity = 1;
                _ReadingMode_.Opacity = 1;
            }
            else {
                _Add_.Opacity = 0;
                _Favorites_.Opacity = 0;
                _Settings_.Opacity = 0;
                _ReadingMode_.Opacity = 0;
            }
            if (!IsReadingModeActive)
                WindowState = settings.IsFullscreen ? WindowState.Maximized : WindowState.Normal;
            if (settings.IsSortingEnabled)
                Books = Books.OrderBy(x => x.GetName()).ToList();
            UpdateBookGrid(IsFavoritesVisible);
        }

        //Возвращает контекстное меню для указанного компонента Button
        private ContextMenu CreateContextMenu(bool isFavorite, Button tag) {
            ContextMenu menu = new ContextMenu();

            MenuItem read = new MenuItem();
            MenuItem edit = new MenuItem();
            MenuItem edit_favorite = new MenuItem();
            MenuItem delete = new MenuItem();

            read.Header = "Читать";
            edit.Header = "Редактировать";
            edit_favorite.Header = isFavorite ? "Удалить из избранного" : "Добавить в избранное";
            delete.Header = "Удалить";

            read.Click += _ReadBook_Click;
            edit.Click += _EditBook_Click;
            if (isFavorite) edit_favorite.Click += _DeleteAtFavorites_Click;
            else edit_favorite.Click += _AddToFavorites_Click;
            delete.Click += _DeleteBook_Click;

            menu.Items.Add(read);
            menu.Items.Add(edit);
            menu.Items.Add(edit_favorite);
            menu.Items.Add(delete);

            menu.DataContext = tag;
            menu.PlacementTarget = tag;

            return menu;
        }

        //Проверка уникальности имени
        private bool CheckName(Book book) {
            foreach (Book _book in Books)
                if (_book.GetName().Equals(book.GetName()) ||
                    _book.GetSource().Equals(book.GetSource()))
                    return false;
            return true;
        }

        //Добавление
        private void _Add__Click(object sender, RoutedEventArgs e) {
            BookAddingWindow window = new BookAddingWindow(settings);
            if ((bool)window.ShowDialog()) {
                Book newBook = window.GetBook();
                if (!CheckName(newBook)) {
                    MessageBox.Show("Книга с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Books.Add(newBook);
                UpdateBookGrid(IsFavoritesVisible);
            }
        }

        //Избранное
        private void _Favorites__Click(object sender, RoutedEventArgs e) {
            IsFavoritesVisible = IsFavoritesVisible ? false : true;
            UpdateBookGrid(IsFavoritesVisible);
        }

        //Настройки
        private void _Settings__Click(object sender, RoutedEventArgs e) {
            SettingsWindow window = new SettingsWindow(settings);
            if (window.ShowDialog().Value) {
                settings = window.GetSettings();
                AcceptSettings();
            }
        }

        //Режим чтения
        private void _ReadingMode__Click(object sender, RoutedEventArgs e) {
            PDFReader.Focus();
            if (!IsReadingModeActive) {
                WindowStyle = WindowStyle.None;
                if (settings.IsFullscreen)
                    WindowState = WindowState.Normal;
                WindowState = WindowState.Maximized;
                IsReadingModeActive = true;
            }
            else {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                if (settings.IsFullscreen)
                    WindowState = WindowState.Maximized;
                IsReadingModeActive = false;
            }
        }

        //Чтение выбранной книги
        private void _Book_Click(object sender, RoutedEventArgs e) {
            int index = Books.IndexOf((Book)(sender as Button).Tag);
            Book chosenBook = Books[index];
            switch (chosenBook.GetSourceType()) {
                case Book.ResourceType.Local:
                    ReaderGrid.Visibility = Visibility.Visible;
                    PDFReader.LoadFile(chosenBook.GetSource());
                    PDFReader.Show();
                    PDFReader.Focus();
                    break;
                case Book.ResourceType.Internet:
                    try {
                        PDFBrowser.Navigate(chosenBook.GetSource());
                        WebReaderGrid.Visibility = Visibility.Visible;
                    }
                    catch (UriFormatException ex) {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    break;
            }
            _Back_.Visibility = Visibility.Visible;
        }

        //Возврат в меню
        private void _Back__Click(object sender, RoutedEventArgs e) {
            if (ReaderGrid.Visibility == Visibility.Visible) {
                ReaderGrid.Visibility = Visibility.Hidden;
                PDFReader.Hide();
            }
            else if (WebReaderGrid.Visibility == Visibility.Visible)
                WebReaderGrid.Visibility = Visibility.Hidden;
            _Back_.Visibility = Visibility.Hidden;
        }

        //Проверка интернет-соединения
        void CheckInternetConnection() {
            _ConnectionStatus_.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => {
                int status = (int)ConnectivityChecker.CheckInternet();
                if (status == (int)ConnectivityChecker.ConnectionStatus.Connected) {
                    if (connectionStatus != status) {
                        _ConnectionStatus_.Source = new BitmapImage(new Uri("Resources/connection_on.png", UriKind.Relative));
                        _ConnectionStatus_.ToolTip = "Доступ к Интернету";
                    }
                }
                else {
                    if (connectionStatus != status) {
                        _ConnectionStatus_.Source = new BitmapImage(new Uri("Resources/connection_off.png", UriKind.Relative));
                        _ConnectionStatus_.ToolTip = "Нет доступа к Интернету";
                    }
                }
                connectionStatus = status;
            }));
            Thread.Sleep(1000);
            CheckInternetConnection();
        }

        private Button GetCopyOfTemplate() {
            Button copy = new Button();
            copy.Width = BookTemplate.Width;
            copy.Height = BookTemplate.Height;
            copy.Background = BookTemplate.Background;
            copy.Foreground = BookTemplate.Foreground;
            copy.BorderBrush = BookTemplate.BorderBrush;
            copy.Content = BookTemplate.Content;
            copy.FontFamily = BookTemplate.FontFamily;
            copy.FontSize = BookTemplate.FontSize;
            copy.Margin = BookTemplate.Margin;
            copy.HorizontalContentAlignment = BookTemplate.HorizontalContentAlignment;
            copy.VerticalContentAlignment = BookTemplate.VerticalContentAlignment;
            copy.Visibility = Visibility.Visible;
            return copy;
        }
        /*************************************************************/
        private void _Search__GotFocus(object sender, RoutedEventArgs e) {
            _Search_.Text = string.Empty;
        }
        private void _Search__LostFocus(object sender, RoutedEventArgs e) {
            if (_Search_.Text.Length == 0)
                _Search_.Text = "Введите название книги";
        }
        private void _Button_MouseLeave(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
        private void _Button__MouseEnter(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
        private void _FButton_MouseLeave(object sender, MouseEventArgs e) {
            if (IsFavoritesVisible) return;
            if (settings.IsAnimationEnabled) {
                var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
                (sender as Button).BeginAnimation(OpacityProperty, anim);
            }
            else
                (sender as Button).Opacity = 0;
        }
        private void _FButton__MouseEnter(object sender, MouseEventArgs e) {
            if (IsFavoritesVisible) return;
            if (settings.IsAnimationEnabled) {
                var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
                (sender as Button).BeginAnimation(OpacityProperty, anim);
            }
            else
                (sender as Button).Opacity = 1;
        }
        private void _Book__MouseEnter(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(150));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
        private void _Book__MouseLeave(object sender, MouseEventArgs e) {
            if (!settings.IsAnimationEnabled) return;
            var anim = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(150));
            (sender as Button).BeginAnimation(OpacityProperty, anim);
        }
        private void _Button__MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                if (settings.IsAnimationEnabled) {
                    var anim = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(250));
                    (sender as Button).BeginAnimation(OpacityProperty, anim);
                }
                else
                    (sender as Button).Opacity = 0.5;
            }
        }
        private void _Button__MouseUp(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                if (settings.IsAnimationEnabled) {
                    var anim = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(250));
                    (sender as Button).BeginAnimation(OpacityProperty, anim);
                }
                else
                    (sender as Button).Opacity = 1;
            }
        }
        private void Main_Closed(object sender, EventArgs e) {
            internetConnectionChecker.Abort();
            SaveData();
        }
        //Контекстное меню
        private void _ReadBook_Click(object sender, RoutedEventArgs e) {
            _Book_Click(((sender as MenuItem).Parent as ContextMenu).DataContext, e);
        }
        private void _EditBook_Click(object sender, RoutedEventArgs e) {
            object bookContainer = ((sender as MenuItem).Parent as ContextMenu).DataContext;
            int index = Books.IndexOf((Book)(bookContainer as Button).Tag);
            EditWindow window = new EditWindow(Books[index], settings);
            if ((bool)window.ShowDialog()) {
                Book newBook = window.GetBook();
                for (int i = 0; i < Books.Count; i++) {
                    if (i == index) continue;
                    if (Books[i].GetName().Equals(newBook.GetName()) ||
                        Books[i].GetSource().Equals(newBook.GetSource())) {
                        MessageBox.Show("Книга с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                Books[index] = newBook;
                UpdateBookGrid(IsFavoritesVisible);
            }
        }
        private void _AddToFavorites_Click(object sender, RoutedEventArgs e) {
            object bookContainer = ((sender as MenuItem).Parent as ContextMenu).DataContext;
            int index = Books.IndexOf((Book)(bookContainer as Button).Tag);
            Books[index].IsFavorite = true;
            UpdateBookGrid(IsFavoritesVisible);
        }
        private void _DeleteAtFavorites_Click(object sender, RoutedEventArgs e) {
            object bookContainer = ((sender as MenuItem).Parent as ContextMenu).DataContext;
            int index = Books.IndexOf((Book)(bookContainer as Button).Tag);
            Books[index].IsFavorite = false;
            UpdateBookGrid(IsFavoritesVisible);
        }
        private void _DeleteBook_Click(object sender, RoutedEventArgs e) {
            object bookContainer = ((sender as MenuItem).Parent as ContextMenu).DataContext;
            int index = Books.IndexOf((Book)(bookContainer as Button).Tag);
            Books.RemoveAt(index);
            UpdateBookGrid(IsFavoritesVisible);
        }
        //Поиск
        private void _Search__TextChanged(object sender, TextChangedEventArgs e) {
            UpdateBookGrid(IsFavoritesVisible);
            if (_Search_.Text.Equals(string.Empty) ||
                _Search_.Text.Equals("Введите название книги")) return;
            string name = _Search_.Text;
            List<UIElement> results = new List<UIElement>();;
            foreach (UIElement container in BooksPanel.Children) {
                Book book = (Book)(container as Button).Tag;
                if (book.GetName().Contains(name))
                    results.Add(container);
            }
            BooksPanel.Children.Clear();
            if (results.Count == 0) {
                _Header_.Content = "Ничего не найдено";
                return;
            }
            _Header_.Content = "Результаты поиска";
            foreach (UIElement element in results)
                BooksPanel.Children.Add(element);
        }
    }
}
