using ResearchHub.Admin.Services;
using System.Text.Json;

namespace ResearchHub.Admin
{
    public partial class Form1 : Form
    {
        private readonly AdminApiClient _api = new();
        private readonly LocalCacheService _cache = new();
        private TextBox _apiUrlBox = null!;
        private TextBox _tokenBox = null!;
        private TabControl _tabs = null!;
        private ListView _usersList = null!;
        private ListView _groupsList = null!;
        private ListView _usageList = null!;
        private ListView _trendingList = null!;
        private Label _statusLabel = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Research Hub Admin";
            Width = 1000;
            Height = 700;

            var top = new Panel { Dock = DockStyle.Top, Height = 90 };
            top.Controls.Add(new Label { Text = "API URL:", Left = 10, Top = 12, AutoSize = true });
            var defaultApiUrl = Environment.GetEnvironmentVariable("ResearchHub__ApiBaseUrl")
                ?? Environment.GetEnvironmentVariable("ApiBaseUrl")
                ?? _api.BaseUrl;
            _apiUrlBox = new TextBox { Left = 80, Top = 8, Width = 320, Text = defaultApiUrl };
            top.Controls.Add(_apiUrlBox);
            top.Controls.Add(new Label { Text = "Admin JWT:", Left = 10, Top = 42, AutoSize = true });
            _tokenBox = new TextBox { Left = 80, Top = 38, Width = 500, UseSystemPasswordChar = true };
            top.Controls.Add(_tokenBox);
            var refreshBtn = new Button { Text = "Refresh all", Left = 600, Top = 36, Width = 100 };
            refreshBtn.Click += async (_, _) => await RefreshAllAsync();
            top.Controls.Add(refreshBtn);
            var offlineBtn = new Button { Text = "Load offline cache", Left = 710, Top = 36, Width = 130 };
            offlineBtn.Click += (_, _) => LoadOffline();
            top.Controls.Add(offlineBtn);
            Controls.Add(top);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _usersList = CreateListView(new[] { "Id", "Name", "Email", "Admin", "Groups", "Bookmarks" });
            _groupsList = CreateListView(new[] { "Id", "Name", "Code", "Creator", "Members", "Papers" });
            _usageList = CreateListView(new[] { "API", "Calls", "Success %", "Avg ms", "Cost" });
            _trendingList = CreateListView(new[] { "Topic / Paper", "Score / Saves" });

            _tabs.TabPages.Add(WrapPage("Users", _usersList));
            _tabs.TabPages.Add(WrapPage("Groups", _groupsList));
            _tabs.TabPages.Add(WrapPage("API usage", _usageList));
            _tabs.TabPages.Add(WrapPage("Trending", _trendingList));
            Controls.Add(_tabs);

            _statusLabel = new Label { Dock = DockStyle.Bottom, Height = 24, Text = "Ready." };
            Controls.Add(_statusLabel);
        }

        private static TabPage WrapPage(string title, Control control)
        {
            var page = new TabPage(title);
            control.Dock = DockStyle.Fill;
            page.Controls.Add(control);
            return page;
        }

        private static ListView CreateListView(string[] columns)
        {
            var lv = new ListView { View = View.Details, FullRowSelect = true, GridLines = true };
            foreach (var col in columns)
                lv.Columns.Add(col, 140);
            return lv;
        }

        private async Task RefreshAllAsync()
        {
            try
            {
                _statusLabel.Text = "Loading...";
                _api.BaseUrl = _apiUrlBox.Text.Trim();
                _api.JwtToken = _tokenBox.Text.Trim();

                var usersJson = await _api.GetJsonAsync("admin/users");
                _cache.Save("users", usersJson);
                PopulateUsers(usersJson);

                var groupsJson = await _api.GetJsonAsync("admin/groups");
                _cache.Save("groups", groupsJson);
                PopulateGroups(groupsJson);

                var usageJson = await _api.GetJsonAsync("admin/analytics/usage");
                _cache.Save("usage", usageJson);
                PopulateUsage(usageJson);

                var trendingJson = await _api.GetJsonAsync("admin/analytics/trending");
                _cache.Save("trending", trendingJson);
                PopulateTrending(trendingJson);

                _statusLabel.Text = $"Updated {DateTime.Now:T}";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Error: " + ex.Message;
                MessageBox.Show(ex.Message, "Admin API error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadOffline()
        {
            var users = _cache.Load("users");
            if (users != null) PopulateUsers(users);
            var groups = _cache.Load("groups");
            if (groups != null) PopulateGroups(groups);
            var usage = _cache.Load("usage");
            if (usage != null) PopulateUsage(usage);
            var trending = _cache.Load("trending");
            if (trending != null) PopulateTrending(trending);
            _statusLabel.Text = "Loaded from offline cache.";
        }

        private void PopulateUsers(string json)
        {
            _usersList.Items.Clear();
            foreach (var u in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                _usersList.Items.Add(new ListViewItem(new[]
                {
                    u.GetProperty("userId").ToString(),
                    u.GetProperty("name").GetString() ?? "",
                    u.GetProperty("email").GetString() ?? "",
                    u.GetProperty("isAdmin").GetBoolean().ToString(),
                    u.GetProperty("groupCount").ToString(),
                    u.GetProperty("bookmarkCount").ToString()
                }));
            }
        }

        private void PopulateGroups(string json)
        {
            _groupsList.Items.Clear();
            foreach (var g in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                _groupsList.Items.Add(new ListViewItem(new[]
                {
                    g.GetProperty("groupId").ToString(),
                    g.GetProperty("groupName").GetString() ?? "",
                    g.GetProperty("groupCode").GetString() ?? "",
                    g.GetProperty("creator").GetString() ?? "",
                    g.GetProperty("memberCount").ToString(),
                    g.GetProperty("paperCount").ToString()
                }));
            }
        }

        private void PopulateUsage(string json)
        {
            _usageList.Items.Clear();
            foreach (var row in JsonDocument.Parse(json).RootElement.EnumerateArray())
            {
                _usageList.Items.Add(new ListViewItem(new[]
                {
                    row.GetProperty("apiName").GetString() ?? "",
                    row.GetProperty("totalCalls").ToString(),
                    row.TryGetProperty("successRate", out var sr) ? sr.GetDouble().ToString("F1") : "",
                    row.TryGetProperty("avgResponseMs", out var ar) ? ar.GetDouble().ToString("F0") : "",
                    row.TryGetProperty("totalCost", out var tc) ? tc.GetDecimal().ToString("F4") : ""
                }));
            }
        }

        private void PopulateTrending(string json)
        {
            _trendingList.Items.Clear();
            var root = JsonDocument.Parse(json).RootElement;
            if (root.TryGetProperty("topics", out var topics))
            {
                foreach (var t in topics.EnumerateArray())
                {
                    _trendingList.Items.Add(new ListViewItem(new[]
                    {
                        t.GetProperty("topic").GetString() ?? "",
                        $"score {t.GetProperty("score")}"
                    }));
                }
            }
            if (root.TryGetProperty("popularPapers", out var papers))
            {
                foreach (var p in papers.EnumerateArray())
                {
                    _trendingList.Items.Add(new ListViewItem(new[]
                    {
                        p.GetProperty("title").GetString() ?? "",
                        $"saves {p.GetProperty("saves")}"
                    }));
                }
            }
        }
    }
}
