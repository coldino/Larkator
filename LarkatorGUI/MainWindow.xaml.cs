using FastMember;
using GongSolutions.Wpf.DragDrop;
using Larkator.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDropTarget
    {
        public ObservableCollection<SearchCriteria> ListSearches { get; } = new ObservableCollection<SearchCriteria>();
        public Collection<DinoViewModel> ListResults { get; } = new Collection<DinoViewModel>();
        public List<string> AllSpecies { get { return arkReaderWild.AllSpecies; } }

        public string ApplicationVersion
        {
            get
            {
                try
                {
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                catch (InvalidDeploymentException)
                {
                    return "DEVELOPMENT";
                }
            }
        }

        public string WindowTitle { get { return $"{Properties.Resources.ProgramName} {ApplicationVersion}"; } }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public string StatusDetailText
        {
            get { return (string)GetValue(StatusDetailTextProperty); }
            set { SetValue(StatusDetailTextProperty, value); }
        }

        public SearchCriteria NewSearch
        {
            get { return (SearchCriteria)GetValue(NewSearchProperty); }
            set { SetValue(NewSearchProperty, value); }
        }

        public bool CreateSearchAvailable
        {
            get { return (bool)GetValue(CreateSearchAvailableProperty); }
            set { SetValue(CreateSearchAvailableProperty, value); }
        }

        public bool NewSearchActive
        {
            get { return (bool)GetValue(NewSearchActiveProperty); }
            set { SetValue(NewSearchActiveProperty, value); }
        }

        public bool ShowHunt
        {
            get { return (bool)GetValue(ShowHuntProperty); }
            set { SetValue(ShowHuntProperty, value); }
        }

        public bool ShowTames
        {
            get { return (bool)GetValue(ShowTamesProperty); }
            set { SetValue(ShowTamesProperty, value); }
        }

        public static readonly DependencyProperty ShowTamesProperty =
            DependencyProperty.Register("ShowTames", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowHuntProperty =
            DependencyProperty.Register("ShowHunt", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty NewSearchActiveProperty =
            DependencyProperty.Register("NewSearchActive", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty CreateSearchAvailableProperty =
            DependencyProperty.Register("CreateSearchAvailable", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty NewSearchProperty =
            DependencyProperty.Register("NewSearch", typeof(SearchCriteria), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public static readonly DependencyProperty StatusDetailTextProperty =
            DependencyProperty.Register("StatusDetailText", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        ArkReader arkReaderWild;
        ArkReader arkReaderTamed;
        FileSystemWatcher fileWatcher;
        DispatcherTimer reloadTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        readonly List<bool?> nullableBoolValues = new List<bool?> { null, false, true };

        public MainWindow()
        {
            ValidateWindowPositionAndSize();

            arkReaderWild = new ArkReader(true);
            arkReaderTamed = new ArkReader(false);

            DataContext = this;

            InitializeComponent();

            LoadSavedSearches();
            EnsureOutputDirectory();

            // Setup file watcher
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(Properties.Settings.Default.SaveFile));
            fileWatcher.Renamed += FileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
            reloadTimer.Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.ConvertDelay);
            reloadTimer.Tick += ReloadTimer_Tick;

            // Sort the results
            resultsList.Items.SortDescriptions.Add(new SortDescription("Dino.BaseLevel", ListSortDirection.Descending));

            // Sort the searches
            searchesList.Items.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            searchesList.Items.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));

            // Add grouping to the searches list
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(searchesList.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
        }

        private void ValidateWindowPositionAndSize()
        {
            var settings = Properties.Settings.Default;

            if (settings.MainWindowLeft <= -10000 || settings.MainWindowTop <= -10000)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (settings.MainWindowWidth < 0 || settings.MainWindowHeight < 0)
            {
                settings.MainWindowWidth = (double)settings.Properties["MainWindowWidth"].DefaultValue;
                settings.MainWindowHeight = (double)settings.Properties["MainWindowHeight"].DefaultValue;
                settings.Save();
            }
        }

        private void EnsureOutputDirectory()
        {
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.OutputDir))
            {
                Properties.Settings.Default.OutputDir = Path.Combine(Path.GetTempPath(), Properties.Resources.ProgramName);
                if (!Directory.Exists(Properties.Settings.Default.OutputDir))
                {
                    Directory.CreateDirectory(Properties.Settings.Default.OutputDir);
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!String.Equals(e.FullPath, Properties.Settings.Default.SaveFile)) return;

            Dispatcher.Invoke(() => StatusText = "Detected change to saved ARK...");

            // Cancel any existing timer to ensure we're not called multiple times
            if (reloadTimer.IsEnabled) reloadTimer.Stop();

            reloadTimer.Start();
        }

        private async void ReloadTimer_Tick(object sender, EventArgs e)
        {
            reloadTimer.Stop();
            await Dispatcher.InvokeAsync(() => ReReadArk(), DispatcherPriority.Background);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateArkToolsData();
            await ReReadArk();
        }

        private void Searches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentSearch();
        }

        private void RemoveSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ShowTames) return;

            var button = sender as Button;
            if (button?.DataContext is SearchCriteria search) ListSearches.Remove(search);
            UpdateCurrentSearch();

            MarkSearchesChanged();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Handler to maintain window aspect ratio
            WindowAspectRatio.Register((Window)sender, height => (int)Math.Round(
                height - statusPanel.ActualHeight - 2 * SystemParameters.ResizeFrameHorizontalBorderHeight - SystemParameters.WindowCaptionHeight
                + leftPanel.ActualWidth + rightPanel.ActualWidth + 2 * SystemParameters.ResizeFrameVerticalBorderWidth));
        }

        private void CreateSearch_Click(object sender, RoutedEventArgs e)
        {
            NewSearch = new SearchCriteria();
            NewSearchActive = true;
            CreateSearchAvailable = false;

            speciesCombo.ItemsSource = arkReaderWild.AllSpecies;
            groupsCombo.ItemsSource = ListSearches.Select(sc => sc.Group).Distinct().OrderBy(g => g).ToArray();
        }

        private async void SaveSearch_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(NewSearch.Species)) return;

            try
            {
                NewSearch.Order = ListSearches.Where(sc => sc.Group == NewSearch.Group).Max(sc => sc.Order) + 100;
            }
            catch (InvalidOperationException)
            { }

            await arkReaderWild.EnsureSpeciesIsLoaded(NewSearch.Species);

            ListSearches.Add(NewSearch);
            NewSearch = null;
            NewSearchActive = false;
            CreateSearchAvailable = true;

            MarkSearchesChanged();
        }

        private void AdjustableInteger_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var tb = (TextBlock)sender;
            var diff = Math.Sign(e.Delta) * Properties.Settings.Default.LevelStep;
            var bexpr = tb.GetBindingExpression(TextBlock.TextProperty);
            var accessor = TypeAccessor.Create(typeof(SearchCriteria));
            var value = (int?)accessor[bexpr.ResolvedSource, bexpr.ResolvedSourcePropertyName];
            if (value.HasValue)
            {
                value = value + diff;
                if (value < 0 || value > Properties.Settings.Default.MaxLevel) value = null;
            }
            else
            {
                value = (diff > 0) ? 0 : Properties.Settings.Default.MaxLevel;
            }

            accessor[bexpr.ResolvedSource, bexpr.ResolvedSourcePropertyName] = value;
            bexpr.UpdateTarget();

            if (null != searchesList.SelectedItem)
                UpdateCurrentSearch();

            MarkSearchesChanged();
        }

        private void AdjustableGender_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var im = (Image)sender;
            var diff = Math.Sign(e.Delta);
            var nOptions = nullableBoolValues.Count;
            var bexpr = im.GetBindingExpression(Image.SourceProperty);
            var accessor = TypeAccessor.Create(typeof(SearchCriteria));
            var value = (bool?)accessor[bexpr.ResolvedSource, bexpr.ResolvedSourcePropertyName];
            var index = nullableBoolValues.IndexOf(value);
            index = (index + diff + nOptions) % nOptions;
            value = nullableBoolValues[index];
            accessor[bexpr.ResolvedSource, bexpr.ResolvedSourcePropertyName] = value;
            bexpr.UpdateTarget();

            if (null != searchesList.SelectedItem)
                UpdateCurrentSearch();

            MarkSearchesChanged();
        }

        private void Result_MouseEnter(object sender, MouseEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;
            var dino = fe.DataContext as DinoViewModel;
            if (dino == null) return;
            dino.Highlight = true;
        }

        private void Result_MouseLeave(object sender, MouseEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;
            var dino = fe.DataContext as DinoViewModel;
            if (dino == null) return;
            dino.Highlight = false;
        }

        private void MarkSearchesChanged()
        {
            SaveSearches();
        }

        private void SaveSearches()
        {
            Properties.Settings.Default.SavedSearches = JsonConvert.SerializeObject(ListSearches);
            Properties.Settings.Default.Save();
        }

        private void LoadSavedSearches()
        {
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.SavedSearches))
            {
                Collection<SearchCriteria> searches;
                try
                {
                    searches = JsonConvert.DeserializeObject<Collection<SearchCriteria>>(Properties.Settings.Default.SavedSearches);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception reading saved searches: " + e.ToString());
                    return;
                }

                ListSearches.Clear();
                foreach (var search in searches)
                    ListSearches.Add(search);
            }
        }

        private async Task UpdateArkToolsData()
        {
            StatusText = "Updating ark-tools database";
            try
            {
                await ArkReader.ExecuteArkTools("update-data");
                StatusText = "Updated ark-tools database";
            }
            catch (Exception e)
            {
                StatusText = "Failed to update ark-tools database: " + e.Message;
            }
        }

        private async Task ReReadArk(bool force = false)
        {
            if (IsLoading) return;

            await PerformConversion(force);
            await LoadSearchSpecies();

            var currentSearch = searchesList.SelectedItems.Cast<SearchCriteria>().ToList();
            UpdateSearchResults(currentSearch);
        }

        private async Task LoadSearchSpecies()
        {
            var species = arkReaderWild.AllSpecies.Distinct();
            foreach (var speciesName in species)
                await arkReaderWild.EnsureSpeciesIsLoaded(speciesName);

            species = arkReaderTamed.AllSpecies.Distinct();
            foreach (var speciesName in species)
                await arkReaderTamed.EnsureSpeciesIsLoaded(speciesName);
        }

        private void UpdateSearchResults(IList<SearchCriteria> searches)
        {
            if (searches == null || searches.Count == 0)
            {
                ListResults.Clear();
            }
            else
            {
                // Find dinos that match the given searches
                var found = new List<Dino>();
                var reader = ShowTames ? arkReaderTamed : arkReaderWild;
                foreach (var search in searches)
                {
                    if (String.IsNullOrWhiteSpace(search.Species))
                    {
                        foreach (var speciesDinos in reader.FoundDinos.Values)
                            found.AddRange(speciesDinos);
                    }
                    else
                    {
                        if (reader.FoundDinos.ContainsKey(search.Species))
                        {
                            var dinoList = reader.FoundDinos[search.Species];
                            found.AddRange(dinoList.Where(d => search.Matches(d)));
                        }
                    }
                }

                ListResults.Clear();
                foreach (var result in found)
                    ListResults.Add(result);
            }

            var cv = (CollectionView)CollectionViewSource.GetDefaultView(ListResults);
            cv.Refresh();
        }

        private async Task PerformConversion(bool force)
        {
            IsLoading = true;
            try
            {
                StatusText = "Processing saved ARK : Wild";
                await arkReaderWild.PerformConversion(force);
                StatusText = "Processing saved ARK : Tamed";
                await arkReaderTamed.PerformConversion(force);
                StatusText = "ARK processing completed";
                StatusDetailText = $"{arkReaderWild.NumberOfSpecies} wild and {arkReaderTamed.NumberOfSpecies} species located";
            }
            catch (Exception ex)
            {
                StatusText = "ARK processing failed";
                StatusDetailText = "";
                MessageBox.Show(ex.Message, "ARK Tools Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateCurrentSearch()
        {
            var search = (SearchCriteria)searchesList.SelectedItem;
            var searches = new List<SearchCriteria>();
            if (search != null) searches.Add(search);
            UpdateSearchResults(searches);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            SearchCriteria sourceItem = dropInfo.Data as SearchCriteria;
            SearchCriteria targetItem = dropInfo.TargetItem as SearchCriteria;

            if (sourceItem != null && targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var sourceItem = (SearchCriteria)dropInfo.Data;
            var targetItem = (SearchCriteria)dropInfo.TargetItem;

            var ii = dropInfo.InsertIndex;
            var ip = dropInfo.InsertPosition;

            // Change source item's group
            sourceItem.Group = targetItem.Group;

            // Try to figure out the other item to insert between, or pick a boundary
            var options = ListSearches
                .OrderBy(sc => sc.Group).ThenBy(sc => sc.Order).ThenBy(sc => sc.Species).ThenBy(sc => sc.MinLevel).ThenBy(sc => sc.MaxLevel)
                .Where(sc => (ip == RelativeInsertPosition.BeforeTargetItem) ? sc.Order < targetItem.Order : sc.Order > targetItem.Order).ToArray();

            double bound;
            if (options.Length > 0)
            {
                var otherItem = (ip == RelativeInsertPosition.BeforeTargetItem) ? options.Last() : options.First();
                bound = otherItem.Order;
            }
            else
            {
                bound = targetItem.Order + ((ip == RelativeInsertPosition.BeforeTargetItem) ? -100 : 100);
            }

            // Update the order to be mid-way between the two
            sourceItem.Order = (targetItem.Order + bound) / 2;

            // Force binding update
            CollectionViewSource.GetDefaultView(searchesList.ItemsSource).Refresh();

            // Save list
            MarkSearchesChanged();
        }

        private void ShowTames_Click(object sender, MouseButtonEventArgs e)
        {
            ShowTames = true;
            ShowHunt = false;
            NewSearchActive = false;
            CreateSearchAvailable = false;

            ShowTameSearches();
        }

        private void ShowTheHunt_Click(object sender, MouseButtonEventArgs e)
        {
            ShowTames = false;
            ShowHunt = true;
            NewSearchActive = false;
            CreateSearchAvailable = true;

            ShowWildSearches();
        }

        private void ShowTameSearches()
        {
            SetupTamedSearches();
        }

        private void SetupTamedSearches()
        {
            var wildcard = new string[] { null };
            var speciesList = wildcard.Concat(arkReaderTamed.AllSpecies).ToList();
            var orderList = Enumerable.Range(0, speciesList.Count);
            var searches = speciesList.Zip(orderList, (species, order) => new SearchCriteria { Species = species, Order = order });

            ListSearches.Clear();
            foreach (var search in searches)
                ListSearches.Add(search);
        }

        private void ShowWildSearches()
        {
            LoadSavedSearches();
        }

        private void Settings_Click(object sender, MouseButtonEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.ShowDialog();

            OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            EnsureOutputDirectory();

            reloadTimer.Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.ConvertDelay);
        }
    }
}
