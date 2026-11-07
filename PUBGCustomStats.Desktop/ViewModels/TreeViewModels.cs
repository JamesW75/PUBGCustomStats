using PUBGCustomStats.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PUBGCustomStats.Desktop.ViewModels
{
    public class MatchItem : INotifyPropertyChanged
    {
        public Match Match { get; set; }
        public string Display { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Notify(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SessionItem : INotifyPropertyChanged
    {
        public Session Session { get; set; }
        public string Display { get; set; } = string.Empty;
        public ObservableCollection<MatchItem> Matches { get; set; } = new ObservableCollection<MatchItem>();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; Notify(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Notify(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SeasonItem : INotifyPropertyChanged
    {
        public Season Season { get; set; }
        public ObservableCollection<SessionItem> Sessions { get; set; } = new ObservableCollection<SessionItem>();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; Notify(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Notify(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
