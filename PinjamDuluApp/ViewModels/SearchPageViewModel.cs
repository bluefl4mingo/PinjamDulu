using PinjamDuluApp.Helpers;
using PinjamDuluApp.Models;
using PinjamDuluApp.Services;
using PinjamDuluApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinjamDuluApp.ViewModels
{
    public class SearchPageViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly DatabaseService _databaseService;
        private readonly User _currentUser;
        private ObservableCollection<Gadget> _searchResults;
        private string _searchQuery;
        private string _selectedCategory;
        private decimal? _minPrice;
        private decimal? _maxPrice;
        private float? _minRating;
        private int? _minCondition;
        private bool _isLoading;

        public ObservableCollection<Gadget> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                ExecuteSearch();
            }
        }

        public bool HasNoResults
        {
            get => SearchResults?.Count == 0;
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                if (value == "All") // Handle "All" selection
                {
                    _selectedCategory = null;
                }
                ExecuteSearch();
            }
        }

        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                _minPrice = value;
                OnPropertyChanged(nameof(MinPrice));
                ExecuteSearch();
            }
        }

        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
                ExecuteSearch();
            }
        }

        public float? MinRating
        {
            get => _minRating;
            set
            {
                _minRating = value;
                OnPropertyChanged(nameof(MinRating));
                ExecuteSearch();
            }
        }

        public int? MinCondition
        {
            get => _minCondition;
            set
            {
                _minCondition = value;
                OnPropertyChanged(nameof(MinCondition));
                ExecuteSearch();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand GadgetSelectedCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand goBack { get; }

        public SearchPageViewModel(NavigationService navigationService, SearchParameters searchParams)
        {
            _navigationService = navigationService;
            _currentUser = searchParams.User;
            _databaseService = new DatabaseService();
            SearchResults = new ObservableCollection<Gadget>();
            SearchQuery = searchParams.Query;

            GadgetSelectedCommand = new RelayCommand<Gadget>(OnGadgetSelected);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            goBack = new RelayCommand(() => _navigationService.GoBack());

            // Initial search with query
            ExecuteSearch();
        }

        private async void ExecuteSearch()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"Executing search with query: '{SearchQuery}'");

                var results = await _databaseService.SearchGadgets(
                    SearchQuery,
                    SelectedCategory,
                    MinPrice,
                    MaxPrice,
                    MinRating,
                    MinCondition);

                SearchResults.Clear();

                if (results != null && results.Any())
                {
                    foreach (var gadget in results)
                    {
                        SearchResults.Add(gadget);
                    }
                }

                OnPropertyChanged(nameof(HasNoResults));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching gadgets: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                SearchResults.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnGadgetSelected(Gadget gadget)
        {
            if (gadget != null)
            {
                var navigationParams = new NavigationParameters(_currentUser, gadget);
                _navigationService.NavigateTo(typeof(GadgetDetail), navigationParams.User, navigationParams.Gadget);
            }
        }

        private void ClearFilters()
        {
            SelectedCategory = null;
            MinPrice = null;
            MaxPrice = null;
            MinRating = null;
            MinCondition = null;
        }
    }
}
