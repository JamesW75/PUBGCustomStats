using Microsoft.EntityFrameworkCore;
using PUBGCustomStats.Data;
using PUBGCustomStats.Logic;
using System;
using System.Text.Json;
using PUBGCustomStats.Desktop.ViewModels;
using System.Linq;

namespace PUBGCustomStats.Desktop
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        // helper to get named controls from XAML
        private T? GetControl<T>(string name) where T : class
        {
            try
            {
                return this.FindByName<T>(name);
            }
            catch
            {
                return null;
            }
        }

        private void OnBrowserBackClicked(object? sender, EventArgs e)
        {
            var browser = GetControl<WebView>("BrowserView");
            if (browser != null && browser.CanGoBack)
                browser.GoBack();
        }

        private void OnBrowserForwardClicked(object? sender, EventArgs e)
        {
            var browser = GetControl<WebView>("BrowserView");
            if (browser != null && browser.CanGoForward)
                browser.GoForward();
        }

        private void OnBrowserHomeClicked(object? sender, EventArgs e)
        {
            var browser = GetControl<WebView>("BrowserView");
            if (browser != null)
                browser.Source = "http://localhost:5209/";
        }

        private void OnBrowserReloadClicked(object? sender, EventArgs e)
        {
            var browser = GetControl<WebView>("BrowserView");
            if (browser != null)
            {
                var src = browser.Source;
                browser.Source = null;
                browser.Source = src;
            }
        }
        private void OnBrowserGoClicked(object? sender, EventArgs e)
        {
            var browser = GetControl<WebView>("BrowserView");
            var urlEntry = GetControl<Entry>("BrowserUrlEntry");
            if (browser == null || urlEntry == null)
                return;

            var url = urlEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(url))
                url = "http://localhost:5209/";

            // Ensure scheme
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            browser.Source = url;
        }
        private string? _connectionString;
        private System.Collections.Generic.List<PUBGCustomStats.Desktop.ViewModels.SeasonItem>? _seasonItems;
        private PUBGCustomStats.Desktop.ViewModels.SeasonItem? _selectedSeason;
        private PUBGCustomStats.Desktop.ViewModels.SessionItem? _selectedSession;
        private PUBGCustomStats.Desktop.ViewModels.MatchItem? _selectedMatch;

        public MainPage()
        {
            InitializeComponent();
            // ensure browser entry shows initial URL and load it
            var urlEntry = GetControl<Entry>("BrowserUrlEntry");
            var browser = GetControl<WebView>("BrowserView");
            if (urlEntry != null && string.IsNullOrWhiteSpace(urlEntry.Text))
                urlEntry.Text = "http://localhost:5209/";
            if (browser != null)
                browser.Source = urlEntry?.Text ?? "http://localhost:5209/";

            LoadSeasons();
        }

        private void OnBrowserNavigating(object? sender, Microsoft.Maui.Controls.WebNavigatingEventArgs e)
        {
            var status = GetControl<Label>("BrowserStatusLabel");
            if (status != null)
            {
                status.Text = "Loading...";
                status.IsVisible = true;
            }
        }

        private void OnBrowserNavigated(object? sender, Microsoft.Maui.Controls.WebNavigatedEventArgs e)
        {
            var status = GetControl<Label>("BrowserStatusLabel");
            if (status == null)
                return;

            if (e.Result == WebNavigationResult.Success )
            {
                status.IsVisible = false;
            }
            else
            {
                status.Text = $"Failed to load: {e.Result}";
                status.IsVisible = true;
            }
        }

        private void LoadSeasons()
        {
            try
            {
                var appSettingsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!System.IO.File.Exists(appSettingsPath))
                    return;

                var json = System.IO.File.ReadAllText(appSettingsPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("ConnectionStrings", out var conn))
                    return;
                if (!conn.TryGetProperty("PUBGCustomStatsContext", out var cs))
                    return;

                var connectionString = cs.GetString() ?? string.Empty;
                connectionString = connectionString.Replace("{AppDataPath}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

                var optionsBuilder = new DbContextOptionsBuilder<PUBGCustomStatsContext>();
                optionsBuilder.UseSqlite(connectionString);

                var options = optionsBuilder.Options;

                var seasonLogic = new Season(options);
                var sessionLogic = new Session(options);
                // Match requires an IntegrationService in the constructor; listing matches uses DB only so pass empty API key
                var matchLogic = new PUBGCustomStats.Logic.Match(options, new PUBGCustomStats.Integration.IntegrationService(""));

                var seasons = seasonLogic.ListSeasons();

                var seasonItems = new System.Collections.Generic.List<PUBGCustomStats.Desktop.ViewModels.SeasonItem>();
                foreach (var s in seasons)
                {
                    var seasonItem = new PUBGCustomStats.Desktop.ViewModels.SeasonItem { Season = s };

                    var rawSessions = sessionLogic.ListSessions(s.SeasonGuid)
                        .OrderByDescending(x => x.StartDateTime ?? DateTime.MinValue)
                        .ToList();

                    foreach (var sess in rawSessions)
                    {
                        var sessionItem = new PUBGCustomStats.Desktop.ViewModels.SessionItem { Session = sess };

                        var matches = matchLogic.ListMatches(sess.SessionGuid);
                        foreach (var m in matches)
                        {
                            var display = string.IsNullOrWhiteSpace(m.MatchName) ? "untitled" : m.MatchName;
                            sessionItem.Matches.Add(new PUBGCustomStats.Desktop.ViewModels.MatchItem { Match = m, Display = display });
                        }

                        sessionItem.Display = (sess.StartDateTime.HasValue ? sess.StartDateTime.Value.ToString("yyyy-MM-dd HH:mm") : "")
                                              + (string.IsNullOrWhiteSpace(sess.SessionName) ? "" : " - " + sess.SessionName);

                        seasonItem.Sessions.Add(sessionItem);
                    }

                    seasonItems.Add(seasonItem);
                }

                // store connection and season items for later
                _connectionString = connectionString;
                _seasonItems = seasonItems;
                // expand first season and its first session by default
                if (_seasonItems != null && _seasonItems.Count > 0)
                {
                    _seasonItems[0].IsExpanded = true;
                    if (_seasonItems[0].Sessions != null && _seasonItems[0].Sessions.Count > 0)
                    {
                        _seasonItems[0].Sessions[0].IsExpanded = true;
                    }
                }

                SeasonsView.ItemsSource = _seasonItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load seasons: {ex.Message}");
            }
        }

        private void OnSeasonTapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.BindingContext is PUBGCustomStats.Desktop.ViewModels.SeasonItem item)
            {
                // Show editor for the season (do not toggle expand/collapse here).
                _selectedSeason = item;
                _selectedSession = null;
                _selectedMatch = null;

                var typeLabel = GetControl<Label>("EditorTypeLabel");
                var nameEntry = GetControl<Entry>("EditorNameEntry");
                var dateEntry = GetControl<Entry>("EditorDateEntry");
                var doNotCount = GetControl<Switch>("EditorDoNotCountSwitch");
                var browser = GetControl<WebView>("BrowserView");

                if (typeLabel != null) typeLabel.Text = "Season";
                if (nameEntry != null) nameEntry.Text = item.Season.SeasonName;
                if (dateEntry != null) dateEntry.Text = string.Empty;
                if (doNotCount != null) doNotCount.IsVisible = false;
                if (browser != null) browser.Source = "http://localhost:5209/";

                // refresh view so selection/editor state is visible
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        private void OnSessionTapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.BindingContext is PUBGCustomStats.Desktop.ViewModels.SessionItem item)
            {
                // Show editor for the session (do not toggle expand/collapse here).
                _selectedSeason = null;
                _selectedSession = item;
                _selectedMatch = null;

                var typeLabel = GetControl<Label>("EditorTypeLabel");
                var nameEntry = GetControl<Entry>("EditorNameEntry");
                var dateEntry = GetControl<Entry>("EditorDateEntry");
                var doNotCount = GetControl<Switch>("EditorDoNotCountSwitch");
                var browser = GetControl<WebView>("BrowserView");

                if (typeLabel != null) typeLabel.Text = "Session";
                if (nameEntry != null) nameEntry.Text = item.Session.SessionName;
                if (dateEntry != null) dateEntry.Text = item.Session.StartDateTime.HasValue ? item.Session.StartDateTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
                if (doNotCount != null) doNotCount.IsVisible = false;
                if (browser != null) browser.Source = "http://localhost:5209/";

                // refresh view so editor selection is visible
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        // Expand/collapse handlers moved to explicit buttons so Name label only opens editor
        private void OnSeasonExpandClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PUBGCustomStats.Desktop.ViewModels.SeasonItem item)
            {
                item.IsExpanded = true;
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        private void OnSeasonCollapseClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PUBGCustomStats.Desktop.ViewModels.SeasonItem item)
            {
                item.IsExpanded = false;
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        private void OnSessionExpandClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PUBGCustomStats.Desktop.ViewModels.SessionItem item)
            {
                item.IsExpanded = true;
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        private void OnSessionCollapseClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PUBGCustomStats.Desktop.ViewModels.SessionItem item)
            {
                item.IsExpanded = false;
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
        }

        private void OnMatchTapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.BindingContext is PUBGCustomStats.Desktop.ViewModels.MatchItem item)
            {
                _selectedSeason = null;
                _selectedSession = null;
                _selectedMatch = item;

                var typeLabel = GetControl<Label>("EditorTypeLabel");
                var nameEntry = GetControl<Entry>("EditorNameEntry");
                var dateEntry = GetControl<Entry>("EditorDateEntry");
                var doNotCount = GetControl<Switch>("EditorDoNotCountSwitch");
                var browser = GetControl<WebView>("BrowserView");

                if (typeLabel != null) typeLabel.Text = "Match";
                if (nameEntry != null) nameEntry.Text = item.Match.MatchName;
                if (dateEntry != null) dateEntry.Text = item.Match.StartTime.HasValue ? item.Match.StartTime.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
                if (doNotCount != null) doNotCount.IsVisible = true;
                if (doNotCount != null) doNotCount.IsToggled = item.Match.DoNotCount.GetValueOrDefault(false);
                if (browser != null) browser.Source = "http://localhost:5209/";
            }
        }

        private void OnEditorSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                    return;

                var optionsBuilder = new DbContextOptionsBuilder<PUBGCustomStatsContext>();
                optionsBuilder.UseSqlite(_connectionString);
                var options = optionsBuilder.Options;

                var seasonLogic = new Season(options);
                var sessionLogic = new Session(options);
                var matchLogic = new PUBGCustomStats.Logic.Match(options, new PUBGCustomStats.Integration.IntegrationService(""));

                var nameEntry = GetControl<Entry>("EditorNameEntry");
                var dateEntry = GetControl<Entry>("EditorDateEntry");
                var doNotCount = GetControl<Switch>("EditorDoNotCountSwitch");

                if (_selectedSeason != null)
                {
                    var newName = nameEntry?.Text ?? string.Empty;
                    seasonLogic.EditSeason(_selectedSeason.Season.SeasonGuid, newName);
                }
                else if (_selectedSession != null)
                {
                    var newName = nameEntry?.Text ?? string.Empty;
                    DateTime newDate = _selectedSession.Session.StartDateTime.GetValueOrDefault();
                    if (!string.IsNullOrWhiteSpace(dateEntry?.Text))
                    {
                        DateTime.TryParse(dateEntry.Text, out newDate);
                    }
                    sessionLogic.EditSession(_selectedSession.Session.SessionGuid, newName, newDate);
                }
                else if (_selectedMatch != null)
                {
                    var newName = nameEntry?.Text ?? string.Empty;
                    matchLogic.EditMatch(_selectedMatch.Match.MatchGuid, newName);
                    // set DoNotCount based on switch
                    var toggled = doNotCount?.IsToggled ?? false;
                    matchLogic.IncludeExcludeMatch(_selectedMatch.Match.MatchGuid, !toggled);
                }

                // refresh
                LoadSeasons();
                // ensure UI tree reloads in case the ItemsSource didn't update
                SeasonsView.ItemsSource = null;
                SeasonsView.ItemsSource = _seasonItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save edits: {ex.Message}");
            }
        }
    }
}
