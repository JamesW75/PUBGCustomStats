using PUBGCustomStats.Data.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PUBGCustomStats.Desktop.ViewModels
{
    public class MatchItem : INotifyPropertyChanged
    {
        public Match Match { get; set; }
        public string Display { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SessionItem : INotifyPropertyChanged
    {
        public Session Session { get; set; }
        public string Display { get; set; } = string.Empty;
        public List<MatchItem> Matches { get; set; } = [];

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; Notify(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SeasonItem : INotifyPropertyChanged
    {
        public Season Season { get; set; }
        public List<SessionItem> Sessions { get; set; } = [];

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; Notify(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
