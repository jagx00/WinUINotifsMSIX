using JAGLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using WinUINotifsMSIX.Models;
using WinUINotifsMSIX.Services;

namespace WinUINotifsMSIX.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _exec;
        private readonly Func<bool>? _canExec;
        public event EventHandler? CanExecuteChanged;
        public RelayCommand(Action exec, Func<bool>? canExec = null) { _exec = exec; _canExec = canExec; }
        public bool CanExecute(object? parameter) => _canExec?.Invoke() ?? true;
        public void Execute(object? parameter) => _exec();
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) =>
            _canExecute == null || (parameter is T t && _canExecute(t));

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
            else
                Log.Information($"RelayCommand<{typeof(T).Name}>: Invalid parameter type");
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public class NotificationsViewModel : ViewModelBase, IDisposable
    {
        private readonly DispatcherQueue _dq = DispatcherQueue.GetForCurrentThread() ?? throw new InvalidOperationException("DispatcherQueue unavailable");
        private readonly UserNotificationListener _listener;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource? _debounceCts;
        private CancellationTokenSource? _disposedCts;
        private readonly NotificationParser _parser = new NotificationParser();
        private uint _highestProcessedId = 0;
        private int _refreshCount = 0;
        private string s1;
        private DateTime? _latestLogTimestamp;

        public BettingContext dbContext;

        private string _AppVersion; // 20200722
        public string AppVersion
        {
            get { return _AppVersion; }
            set { SetProperty(ref _AppVersion, value); }
        }

        private bool _isDebugPanelExpanded;
        public bool IsDebugPanelExpanded
        {
            get => _isDebugPanelExpanded;
            set
            {
                if (_isDebugPanelExpanded != value)
                {
                    SetProperty(ref _isDebugPanelExpanded, value);

                    if (value)
                        StartLogTimer();
                    else
                        StopLogTimer();
                }
            }
        }

        public ObservableCollection<NotificationItem> Notifications { get; } = new ObservableCollection<NotificationItem>();

        private NotificationItem? _selectedNotification;
        public NotificationItem? SelectedNotification
        {
            get => _selectedNotification;
            set
            {
                SetProperty(ref _selectedNotification, value);
                //Log.Information("_SelectedNotification was set to {i}", _SelectedNotification.Id);
                // build the combobox with AFL matches in case it is a late change
                BuildAFLMatchesCandidates();
            }
        }

        // 20220524
        // aflmatches are read only so will make them just a list rather than an observable collection for now
        // no i want one way binding at least - i think
        private ObservableCollection<tblAFLMatch> _AFLMatches;
        public ObservableCollection<tblAFLMatch> AFLMatches
        {
            get => _AFLMatches;
            // concise as per enum
            set
            {
                //Log.Verbose("Entered");
                SetProperty(ref _AFLMatches, value);
            }
        }

        private tblAFLMatch _SelectedAFLMatch;
        public tblAFLMatch SelectedAFLMatch
        {
            get => _SelectedAFLMatch;
            set
            {
                //Log.Information("Setting SelectedAFLMatch to {d}",value);
                SetProperty(ref _SelectedAFLMatch, value);
            }
        }

        private string _MatchEventText;
        public string MatchEventText
        {
            get => _MatchEventText;
            set
            {
                //Log.Verbose("Entered");
                SetProperty(ref _MatchEventText, value);
            }
        }
        public ObservableCollection<string> AvailableApplications { get; } = new();

        public ObservableCollection<SeriLog> RecentLogs { get; } = new();
        public string DebugLogHeader =>
    $"Debug Logs ({RecentLogs.Count}) | Errors: {RecentLogs.Count(l => l.Level?.Equals("Error", StringComparison.OrdinalIgnoreCase) == true)} | Warnings: {RecentLogs.Count(l => l.Level?.Equals("Warning", StringComparison.OrdinalIgnoreCase) == true)} ";
        public PowerStateMonitor PowerStateMonitor { get; } = new();
        public LogMaintenanceService logService { get; } = new();
        public TimeSpan InitialLogWindow { get; set; } = TimeSpan.FromMinutes(10);
        public int ApplicationScanLimit { get; set; } = 10000;
        private string _selectedApplication = "WinUINotifsMSIX";
        public string SelectedApplication
        {
            get => _selectedApplication;
            set
            {
                if (SetProperty(ref _selectedApplication, value))
                {
                    //_ = RefreshLogsAsync();
                    RecentLogs.Clear();
                    _ = LoadInitialLogsAsync();
                }
            }
        }

        public int VisibilityThreshold = 6;

        private Visibility _MatchEventVisibility = Visibility.Collapsed;
        public Visibility MatchEventVisibility
        {
            get { return _MatchEventVisibility; }
            set { SetProperty(ref _MatchEventVisibility, value);
                if (MatchEventVisibility == Visibility.Visible)
                {
                    (App.Current as App)?.RequestWindowFocus();
                }
            }
        }
        public ICommand TriggerHousekeepingCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteNotificationCommand { get; }
        public ICommand WriteMatchEventCommand { get; }

           

        public readonly DispatcherTimer _logRefreshTimer = new()
        {
            Interval = TimeSpan.FromSeconds(10)
        };

        public NotificationsViewModel(UserNotificationListener listener)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BettingContext>();
            dbContext = new BettingContext(optionsBuilder.Options);
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            // subscribe
            _listener.NotificationChanged += Listener_NotificationChanged;

            RefreshCommand = new RelayCommand(() => _ = RefreshAsync());
            DeleteNotificationCommand = new RelayCommand<NotificationItem>(DeleteNotification);
            WriteMatchEventCommand = new RelayCommand(WriteMatchEvent);
            TriggerHousekeepingCommand = new RelayCommand(async () => await logService.TriggerHousekeepingAsync());
            _ = InitializeAsync();
            _logRefreshTimer.Tick += async (s, e) => await RefreshLogsAsync();
            StartLogTimer();
        }
        private async Task InitializeAsync()
        {
            // have to be async to await these - cannot do both in ctor
            await LoadApplicationsAsync();
            await LoadInitialLogsAsync();
        }
        public async Task LoadApplicationsAsync()
        {
            var apps = await dbContext.GetRecentApplicationsAsync(ApplicationScanLimit);

            AvailableApplications.Clear();
            AvailableApplications.Add("All");

            foreach (var app in apps)
                AvailableApplications.Add(app);
        }
        public async Task LoadInitialLogsAsync()
        {
            var cutoff = DateTime.Now - InitialLogWindow;
            var logs = await dbContext.GetRecentLogsAsync(cutoff, SelectedApplication);
            Log.Information("Loaded {Count} initial logs since initial cutoff of {Cutoff}", logs.Count, cutoff);
            foreach (var log in logs)
            {
                RecentLogs.Add(log);
                _latestLogTimestamp = log.TimeStamp;
            }
        }
        public async Task RefreshLogsAsync()
        {
            var newLogs = await dbContext.GetRecentLogsAsync(_latestLogTimestamp, SelectedApplication);

            foreach (var log in newLogs)
            {
                RecentLogs.Add(log);
                _latestLogTimestamp = log.TimeStamp;
            }

            while (RecentLogs.Count > 1000)
                RecentLogs.RemoveAt(0);
        }
        private void DeleteNotification(NotificationItem item)
        {
            if (item != null && Notifications.Contains(item))
            {
                Notifications.Remove(item);
                // Optional: also remove from DB if synced
                dbContext.DeleteNotification((int)item.ID);
                SelectedNotification = Notifications.FirstOrDefault();
            }
        }

        private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            // Debounce rapid bursts: schedule a refresh in 300ms, cancel prior if any
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var ct = _debounceCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, ct);
                    if (ct.IsCancellationRequested) return;
                    await RefreshAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Log.Error(ex, "NotificationChanged handler error"); }
            }, ct);
        }

        /// <summary>
        /// Refreshes the Notifications collection by enumerating current toasts.
        /// Keeps UI updates marshalled to the DispatcherQueue and minimizes collection churn.
        /// </summary>
        public async Task RefreshAsync()
        {
            if (_disposedCts?.IsCancellationRequested ?? false) return;

            if (!await _refreshLock.WaitAsync(0).ConfigureAwait(false))
            {
                // another refresh in progress; skip
                return;
            }

            try
            {
                var dbnotifs = new List<NotificationItem>();
                if (_refreshCount == 0)
                {
                    var dbns = dbContext.GetDBNotifications(_parser.NotifVisibilityThreshold);
                    dbnotifs = dbns
                         .Select(dbn => new NotificationItem
                         {
                             ID = (uint)dbn.Id,
                             CreationTime = dbn.CreationTime,
                             Source = dbn.Source,
                             Title = dbn.Title,
                             Body = dbn.Body,
                             Visibility = dbn.Visibility
                         })
                         .OrderByDescending(n => n.ID)
                         .ToList();
                }

                var notifs = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
                Log.Information("Refreshing notifications, count {Count}", notifs.Count);
                _refreshCount++;

                // Build a transient list of items to reconcile with the ObservableCollection
                var winnotifs = notifs
                   .Select(n =>
                   {
                       try
                       {
                           var id = n.Id;
                           var creation = n.CreationTime;
                           var source = n.AppInfo?.DisplayInfo?.DisplayName ?? "unknown";
                           var texts = ExtractTextsFromNotification(n);
                           var title = texts.FirstOrDefault() ?? "<no title>";
                           var body = string.Join(" | ", texts.Skip(1));
                           var visibility = 6;

                           return new NotificationItem
                           {
                               ID = id,
                               CreationTime = creation,
                               Source = source,
                               Title = title,
                               Body = body,
                               Visibility = visibility
                           };
                       }
                       catch (Exception ex)
                       {
                           Log.Warning(ex, "Failed to process notification in RefreshAsync");
                           return null;
                       }
                   })
                   .Where(item => item != null)
                   .OrderByDescending(item => item.ID) // Sort before reconciliation
                   .ToList();

                // merge db and items
                // Create a dictionary from winnotifs keyed by ID
                var notifDict = winnotifs.ToDictionary(n => n.ID);

                // Overwrite or insert DB notifications (preferred on conflict)
                foreach (var dbn in dbnotifs)
                {
                    if (notifDict.ContainsKey(dbn.ID))
                    {
                        Log.Information("Overriding Win notification with DB version for ID {ID}", dbn.ID);
                    }
                    notifDict[dbn.ID] = dbn;
                }

                // Convert to a sorted List<NotificationItem> by ID descending
                var allnotifs = notifDict
                    .Values
                    .OrderByDescending(n => n.ID)
                    .ToList();

                await _dq.EnqueueAsync(async () => await ReconcileNotificationsAsync(allnotifs));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error enumerating toasts");
            }
            finally
            {
                _refreshLock.Release();
            }
        }
        private async Task ReconcileNotificationsAsync(List<NotificationItem?> allnotifs)
        {
            var existing = Notifications.ToDictionary(n => n.ID); // the ones already on the page
            var incoming = allnotifs.ToDictionary(n => n.ID); // all the current ones (db + win)

            // Update or add items
            foreach (var kvp in incoming)
            {
                if (existing.TryGetValue(kvp.Key, out var existingItem))
                {
                    //Log.Information("kvp.Key {id} already exists", kvp.Key);
                    // Update properties if changed - they can't change yet
                    //if (existingItem.Title != kvp.Value.Title) existingItem.Title = kvp.Value.Title;
                    //if (existingItem.Body != kvp.Value.Body) existingItem.Body = kvp.Value.Body;
                    //if (existingItem.Source != kvp.Value.Source) existingItem.Source = kvp.Value.Source;
                    //if (existingItem.CreationTime != kvp.Value.CreationTime) existingItem.CreationTime = kvp.Value.CreationTime;
                    //if (existingItem.Visibility != kvp.Value.Visibility) existingItem.Visibility = kvp.Value.Visibility;
                }
                else
                {
                    // New item — insert - only place this is done
                    // Only add if ID is greater than highest processed ID (to avoid re-adding old ones) except for first run
                    //Log.Information("for notif key {n} seeing if > highest {h}  _refreshCount = {r}", kvp.Key,_highestProcessedId, _refreshCount);
                    if (kvp.Key > _highestProcessedId || _refreshCount == 1)
                    {
                        if (_parser.IsRelevant(kvp.Value))
                        {
                            int insertIndex = Notifications
                                .TakeWhile(n => n.ID > kvp.Value.ID)
                                .Count();

                            Notifications.Insert(insertIndex, kvp.Value);
                            dbContext.WriteNotification(kvp.Value);
                        }
                        if (kvp.Key > _highestProcessedId)
                            _highestProcessedId = kvp.Key;
                    }
                }
            }
            SelectedNotification = Notifications.FirstOrDefault();
            Log.Information("After reconciliation, IsSleepBlocked {sb} Notifications count is {Count} and selected notification is {f} _highestProcessedId {hp} _refreshCount {rc} ",
                            PowerStateMonitor.IsSleepBlocked, Notifications.Count, SelectedNotification.ID, _highestProcessedId, _refreshCount);
        }

        private static string? GetTextProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type elType, object elem)
        {
            var txtProp = elType.GetProperty("Text");
            return txtProp?.GetValue(elem) as string;
        }
        /// <summary>
        /// Robust extraction of text lines from a UserNotification across different projections.
        /// Attempts visual binding + GetTextElements, then Notification.Content (XmlDocument),
        /// then GetXml / GetXmlDocument via reflection, and falls back to empty list.
        /// </summary>
        private static List<string> ExtractTextsFromNotification(UserNotification n)
        {
            var texts = new List<string>();

            if (n == null) return texts;

            // 1) Try Visual -> Binding -> GetTextElements (fast path)
            try
            {
                var visual = n.Notification?.Visual;
                if (visual != null)
                {
                    var binding = visual.GetBinding("ToastGeneric");
                    if (binding != null)
                    {
                        try
                        {
                            var tes = binding.GetTextElements();
                            foreach (var te in tes)
                            {
                                var t = te?.Text?.Trim();
                                if (!string.IsNullOrEmpty(t)) texts.Add(t);
                            }
                            if (texts.Count > 0) return texts;
                        }
                        catch
                        {
                            // projection may not implement GetTextElements - fall through to reflection/Xml
                        }

                        // Try reflection to read a TextElements property (if present)
                        try
                        {
                            var beType = binding.GetType();
                            var prop = beType.GetProperty("TextElements");
                            if (prop != null)
                            {
                                var elems = prop.GetValue(binding) as System.Collections.IEnumerable;
                                if (elems != null)
                                {
                                    foreach (var elem in elems)
                                    {
                                        // Tried to get rid of the warning IL2072 but no joy, so ignore it
#pragma warning disable IL2072

                                        var val = GetTextProperty(elem.GetType(), elem);
#pragma warning restore IL2072

                                        if (!string.IsNullOrEmpty(val)) texts.Add(val.Trim());
                                    }
                                    if (texts.Count > 0) return texts;
                                }
                            }
                        }
                        catch { /* ignore */ }
                    }
                }
            }
            catch { /* ignore */ }

            // 2) Try Notification.Content (XmlDocument) via reflection (projection-agnostic)
            try
            {
                var notifObj = n.Notification;
                if (notifObj != null)
                {
                    var notifType = notifObj.GetType();

                    // Try property "Content" -> Windows.Data.Xml.Dom.XmlDocument
                    var contentProp = notifType.GetProperty("Content");
                    if (contentProp != null)
                    {
                        var xmlDoc = contentProp.GetValue(notifObj) as XmlDocument;
                        if (xmlDoc != null)
                        {
                            var fromXml = ParseTextNodesFromXmlDocument(xmlDoc);
                            if (fromXml.Count > 0) return fromXml;
                        }
                    }

                    // Try method GetXml() returning string
                    var getXmlMethod = notifType.GetMethod("GetXml", Type.EmptyTypes);
                    if (getXmlMethod != null)
                    {
                        var xmlStr = getXmlMethod.Invoke(notifObj, null) as string;
                        if (!string.IsNullOrEmpty(xmlStr))
                        {
                            var fromStr = ParseTextNodesFromXmlString(xmlStr);
                            if (fromStr.Count > 0) return fromStr;
                        }
                    }

                    // Try method GetXmlDocument() returning XmlDocument
                    var getXmlDocMethod = notifType.GetMethod("GetXmlDocument", Type.EmptyTypes);
                    if (getXmlDocMethod != null)
                    {
                        var doc = getXmlDocMethod.Invoke(notifObj, null) as XmlDocument;
                        if (doc != null)
                        {
                            var fromDoc = ParseTextNodesFromXmlDocument(doc);
                            if (fromDoc.Count > 0) return fromDoc;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // 3) Fallback: nothing extracted
            return texts;
        }

        private static List<string> ParseTextNodesFromXmlDocument(XmlDocument xml)
        {
            var result = new List<string>();
            if (xml == null) return result;

            try
            {
                var bindings = xml.GetElementsByTagName("binding");
                foreach (var b in bindings)
                {
                    if (b is XmlElement be)
                    {
                        var template = be.GetAttribute("template");
                        if (!string.IsNullOrEmpty(template) && template.Equals("ToastGeneric", StringComparison.OrdinalIgnoreCase))
                        {
                            var textNodes = be.GetElementsByTagName("text");
                            foreach (var tn in textNodes)
                            {
                                if (tn is XmlElement te)
                                {
                                    var txt = te.InnerText?.Trim();
                                    if (!string.IsNullOrEmpty(txt)) result.Add(txt);
                                }
                            }
                            if (result.Count > 0) return result;
                        }
                    }
                }

                var allTextNodes = xml.GetElementsByTagName("text");
                foreach (var node in allTextNodes)
                {
                    if (node is XmlElement elem)
                    {
                        var txt = elem.InnerText?.Trim();
                        if (!string.IsNullOrEmpty(txt)) result.Add(txt);
                    }
                }
            }
            catch { /* ignore */ }

            return result;
        }

        private static List<string> ParseTextNodesFromXmlString(string xmlString)
        {
            var result = new List<string>();
            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(xmlString);

                var bindings = xml.GetElementsByTagName("binding");
                foreach (var b in bindings)
                {
                    if (b is System.Xml.XmlElement be)
                    {
                        var template = be.GetAttribute("template");
                        if (!string.IsNullOrEmpty(template) && template.Equals("ToastGeneric", StringComparison.OrdinalIgnoreCase))
                        {
                            var textNodes = be.GetElementsByTagName("text");
                            foreach (var tn in textNodes)
                            {
                                if (tn is System.Xml.XmlElement te)
                                {
                                    var txt = te.InnerText?.Trim();
                                    if (!string.IsNullOrEmpty(txt)) result.Add(txt);
                                }
                            }
                            if (result.Count > 0) return result;
                        }
                    }
                }

                var allTextNodes = xml.GetElementsByTagName("text");
                foreach (System.Xml.XmlNode node in allTextNodes)
                {
                    if (node is System.Xml.XmlElement elem)
                    {
                        var txt = elem.InnerText?.Trim();
                        if (!string.IsNullOrEmpty(txt)) result.Add(txt);
                    }
                }
            }
            catch { /* ignore */ }

            return result;
        }
        private void BuildAFLMatchesCandidates()
        {
            // 20220524
            // based on the currently selected notification, populate the AFLMatches combobox's backing vm of ViewModel.AFLMatches
            // and decide upon WriteMatchEvent visibility

            // Only called when the selected notification changes

            try
            {
                bool LateChangeMayExist = false;

                if (SelectedNotification != null)
                {
                    Log.Information("BuildAFLMatchesCandidates: Building AFLMatches combobox candidates for SelectedNotification = {id}", SelectedNotification.ID);

                    using (BettingContext db = new BettingContext())
                    {
                        string[] TeamNames = new string[] { };
                        List<tblAFLMatch> NearMs = new List<tblAFLMatch>();

                        if (AFLMatches is null)
                            AFLMatches = new ObservableCollection<tblAFLMatch>();
                        else
                            AFLMatches.Clear();

                        Log.Information("BuildAFLMatchesCandidates: About to call CheckForLateChange for SelectedNotification = {id}", SelectedNotification.ID);

                        LateChangeMayExist = CheckForLateChange(SelectedNotification.Body);

                        int FutureMinutes = -60 * 24 * 2; // was 8 hrs but sometimes changes announced early so made 2 days
                        int PastMinutes = 15;

                        if (LateChangeMayExist)
                            NearMs = db.GetAFLMatches(SelectedNotification.CreationTime.LocalDateTime, FutureMinutes, PastMinutes).ToList();

                        // if there are candidates see if there is also a match identifier of the form #AFLBluesPies

                        Log.Information("LateChangeMayExist = {a}, NearMs = {b} [near Matches look ahead {f} hrs and {m} mins back]", LateChangeMayExist, NearMs.Count(), FutureMinutes / -60, PastMinutes);

                        if (NearMs != null && NearMs.Count() != 0)
                        {
                            TeamNames = GetTeamNames(SelectedNotification.Body);

                            if (TeamNames.Length == 2)
                            {
                                Log.Information("TeamNames returned from GetTeamNames are {a} {b} Input was {c}", TeamNames[0], TeamNames[1], SelectedNotification.Body);

                                int TeamID1Body = db.GetTeamID(TeamNames[0]);
                                int TeamID2Body = db.GetTeamID(TeamNames[1]);

                                // check the candidate matches by start time [we only care about notifs beforeish match start]

                                string[] MatchTeamNames = new string[] { };

                                foreach (var m in NearMs)
                                {
                                    // see if we also have a team pair for this candidate match and if so make it the only candidate
                                    Log.Information("Candidate MatchName = {m}", m.MatchName);

                                    MatchTeamNames = m.MatchName.Split(" v ");

                                    int TeamID1Match = db.GetTeamID(MatchTeamNames[0]);
                                    int TeamID2Match = db.GetTeamID(MatchTeamNames[1]);

                                    if (TeamNames != null && TeamNames.Length == 2)
                                    {

                                        if (TeamID1Body == TeamID1Match && TeamID2Body == TeamID2Match)
                                        {
                                            AFLMatches.Add(m);
                                            Log.Information("Candidate MatchName = {m} was added to AFLMatches list", m.MatchName);
                                        }

                                    }

                                }
                            }
                            else
                            {
                                Log.Information("Didn't find 2 TeamNames in body text. Found {c}", TeamNames.Count());
                            }

                            // if no match - just add all the candidates
                            if (AFLMatches.Count == 0)
                                foreach (var m in NearMs)
                                {
                                    AFLMatches.Add(m);
                                    //Log.Information("AFLMatchID={id} was added to combo vm", m.ID);
                                }


                            // https://stackoverflow.com/questions/52260658/how-to-load-a-combobox-in-uwp
                            // https://stackoverflow.com/questions/39090923/why-this-uwp-combobox-can-be-blank-after-initialization-but-works-fine-for-selec
                            Log.Information("SelectedAFLMatch will now be set to the first element in the AFLMatches list which contains {c} matches", AFLMatches.Count);

                            SelectedAFLMatch = AFLMatches.FirstOrDefault();
                            if (SelectedAFLMatch != null)
                                Log.Information("SelectedAFLMatch was set to {s}", SelectedAFLMatch.MatchName);
                            else
                                Log.Information("SelectedAFLMatch is null");

                        }
                    }
                }

                // only show write etc if there are matches in combo box 
                if (SelectedAFLMatch != null && LateChangeMayExist)
                {
                    Log.Information("SelectedAFLMatch is not null and a LateChangeMayExist so will make the match event visible");
                    MatchEventText = SelectedNotification.Body;
                    MatchEventVisibility = Visibility.Visible;
                }

                else
                {
                    Log.Information("SelectedAFLMatch is null or LateChangeMayExist is false so NOT making the match event visible. SelectedAFLMatch.ID is {d} LateChangeMayExist is {s}", SelectedAFLMatch != null ? SelectedAFLMatch.ID.ToString() : "null", LateChangeMayExist);
                    MatchEventVisibility = Visibility.Collapsed;
                }
                //MatchEventVisibility = Visibility.Visible; // temp for testing

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }


  
        private bool CheckForLateChange(string? body)
        {
            try
            {
                // based on ParseUserNotification - should make common really

                bool PotentialLateChange = false;

                if (body.Trim().Length == 0)
                {

                }
                else if (_parser.IncludeBodyValues.Any(val =>
                {
                    bool matched = body.ToLower().Contains(val);
                    if (matched)
                        Log.Information("CheckForLateChange: Matched IncludeBodyValue '{val}' in body '{body}'", val, body);
                    return matched;
                }))
                {
                    Log.Information("About to check IncludeBodyValuesOverrides since IncludeBodyValues found a match for body {b}", body);

                    if (_parser.IncludeBodyValuesOverrides.Any(body.ToLower().Contains))
                    {
                        Log.Information("Setting PotentialLateChange false because IncludeBodyValuesOverrides detected for body {b}", body);
                        PotentialLateChange = false;
                    }
                    else
                    {
                        // It is no good just testing for 'late changes' here as it can be: x has been replaced for dogs no late changes for blues.
                        // 20221203 - allow 'no late changes for #AFLxxxyyy' as that should mean no changes for the match xxx v yyy

                        // there’s tricked me for a bit as it isn't there's

                        //var str = body.Replace("\"", string.Empty);
                        //str = str.Replace("\'", string.Empty);
                        //str = str.Replace("`", string.Empty);
                        //str = str.Replace("'", string.Empty);

                        //bool ss2 = body.ToLower().Contains("and there’s no changes");
                        //bool ss = body.ToLower().Contains("and there''s no changes");
                        //bool s22s = body.ToLower().Contains("and there'''s no changes");
                        //bool s2a2s = body.ToLower().Contains("and there\'s no changes");
                        //bool s2s = body.ToLower().Contains("'");
                        //bool s2s22 = body.ToLower().Contains("''");
                        //bool s2s33 = body.ToLower().Contains("'''");
                        //bool s2sq = body.ToLower().Contains("\'");
                        //var xx = body.ToLower();
                        Log.Information("CheckForLateChange: About to check NoLateChangeForEitherSpecified for body {b} - ie see if text explicitly says no late changes", body);

                        //Log.Information("body lower {l}", xx);

                        bool NoLateChangeForEitherSpecified = body.ToLower().Contains("no late change")
                            //|| body.ToLower().Contains("no late changes ahead")
                            //|| body.ToLower().Contains("no late changes at ") 
                            //|| body.ToLower().Contains("no late changes to either")
                            //|| body.ToLower().Contains("no late changes for either")
                            //|| body.ToLower().Contains("no late changes for the ")
                            //|| body.ToLower().Contains("no late changes for Friday")
                            //|| body.ToLower().Contains("no late changes for #afl")
                            || body.ToLower().Contains("neither team making any late changes")
                            || body.ToLower().Contains("there's no changes for #afl")
                            || body.ToLower().Contains("there’s no changes for #afl")
                            //|| body.ToLower().Contains("no late changes for the first")
                            //|| body.ToLower().Contains("no late changes for the later")
                            //|| body.ToLower().Contains("no late changes for the showdown")
                            //|| body.ToLower().Contains("no late changes for tonight")
                            //|| body.ToLower().Contains("no late changes in ")
                            //|| body.ToLower().Contains("no late changes under ")
                            || body.ToLower().Contains("and there's no changes")
                            || body.ToLower().Contains("and there’s no changes")
                            || body.ToLower().Contains("no late changes ")
                            || body.ToLower().Contains("there are no changes.")
                            || body.ToLower().Contains("have made any late changes");
                        //|| body.ToLower().Contains("no late changes this evening");

                        Log.Information("CheckForLateChange: NoLateChangeForEitherSpecified set to {b}", NoLateChangeForEitherSpecified);

                        PotentialLateChange = !NoLateChangeForEitherSpecified;
                    }
                }
                else
                {
                    Log.Information("IncludeBodyValues is false for body {b} so not a potential late change", body);
                }

                return PotentialLateChange;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
                return false;
            }
        }

        private string[] GetTeamNames(string body)
        {

            // https://stackoverflow.com/questions/1212746/linq-what-is-the-difference-between-select-and-where

            // Select is all about transformation.
            // Where is all about filtering.
            // The first one is what we call a Projection Operator, while the last one is a Restriction Operator.
            // Select maps an enumerable to a new structure.
            // If you perform a select on an IEnumerable, you will get an array with the same number of elements, but a different type depending on the mapping you specified. 

            string[] TeamNames = new string[] { };
            string Intext;

            try
            {
                // From this sort of thing: late changes for #AFLDogsSuns
                // extract Dogs and Suns

                // '\u002C' is unicode comma \n line feed

                // get rid of line feeds etc.
                // https://stackoverflow.com/questions/1751371/how-to-use-n-in-a-textbox
                Intext = body.Replace("\n", " ").Replace("\r", " ").Replace(":", "").Replace(".", "").Replace(",", "")
                             .Replace("@JoshGabelich", "").Replace("@AFLcomau", "").Replace("@FOXFOOTY", "").Replace("@RalphyHeraldSun", ""); ;

                Log.Information("Intext is {b} after initial tidy", Intext);

                var NamePairs = Intext.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x.StartsWith("#AFL"))
                    .Select(z => z.Replace("#AFL", ""))
                    .Where(q => q.Count(c => char.IsUpper(c)) == 2)
                    .Where(l => l.Count(c => char.IsLower(c)) >= 4);

                Log.Information("NamePairs.Count() is {b} after initial check", NamePairs.Count());

                // remove dups - 20240716
                NamePairs = NamePairs.Distinct().ToList();

                // https://stackoverflow.com/questions/36147162/c-sharp-string-split-separate-string-by-uppercase

                // If present, there should only be one element and it should contain two team names - DogsSuns

                // Some notifs don't have matchkey in them, in which case return null.
                //if (NamePairs.Count() != 1)
                //    throw new ApplicationException("Not one NamePair found");

                if (NamePairs.Count() == 1)
                    if (NamePairs.First().Length >= 4) // ie not #AFLGF
                        TeamNames = Regex.Split(NamePairs.First(), @"(?<!^)(?=[A-Z])");
                    else
                    {
                    }
                else
                {
                    // of the form #AFLDogsSuns is not present
                    // EG         
                    // The final teams are in for the game between the Cats and Pies and there's no late changes  Medical subs @GeelongCats - Mark O'Connor @CollingwoodFC - Nathan Kreuger  #AFLFinals
                    // try @Team1 @Team2 format
                    // certain @ handles are removed above
                    Log.Information("Intext is not of the form #AFLDogsSuns, trying @Team1 @Team2 format. Intext is {i}", Intext);

                    var Candidates = Intext.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Where(z => z.StartsWith("@"))
                    .Select(z => z.Replace("@", ""));

                    // GeelongCats and CollingwoodFC
                    if (Candidates.Count() == 2)
                    {
                        // https://stackoverflow.com/questions/327916/redim-preserve-in-c
                        Array.Resize(ref TeamNames, 2);
                        TeamNames[0] = Candidates.First();
                        TeamNames[1] = Candidates.Last();
                    }
                    else
                    {
                        Log.Information("found {b} candidate teams in the text but not 2, so won't assume it is for a match but that match should be selectable in the ddl ", Candidates.Count());
                        if (Candidates.Count() > 0)
                        {
                            Log.Information("First candidate is {b}", Candidates.First());
                        }
                    }

                }

                return TeamNames;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
                return null;
            }

        }
        internal async void WriteMatchEvent()
        {
            // 202220525
            // attached to button via ICommand, which is a more flexible way than doing Click events

            try
            {
                string RequiredMET = "ATCH";

                if (SelectedAFLMatch == null || MatchEventText == null)
                {
                    throw new ApplicationException("Need a match and MatchEventText");
                }

                MatchEventText.Trim();

                //var s1 = "/5/";
                ////s1 += AFLMatches.Count().ToString();
                //s1 += SelectedAFLMatch.ID.ToString() + "/" + MatchEventText;

                Log.Debug("MatchEventText is {s} of length {l}", MatchEventText, MatchEventText.Length);

                if (MatchEventText.Length > 300)
                    throw new ApplicationException(String.Format("MatchEventText.Length is {0} it must be <= 300", MatchEventText.Length));

                using (BettingContext db = new BettingContext())
                {
                    MatchEventType MET = db.GetMatchEventType(RequiredMET);

                    if (MET != null)
                    {

                        tblAFLMatchEvent ME = db.WriteMatchEvent(MET.ID, SelectedAFLMatch.ID, SelectedNotification.CreationTime.LocalDateTime, MatchEventText);
                        if (ME == null)
                        {
                            // dup key
                            //StatusMessage = "This key already exists for this MatchEvent";
                        }
                        else
                        {
                            //StatusMessage = "MatchEvent was written successfully";
                            MatchEventText = "";
                        }

                    }
                    else
                    {
                        s1 = String.Format("Did not find MatchEventType of {0} on the database", RequiredMET);
                        throw new ApplicationException(s1);
                    }

                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
            }

        }

        public void StartLogTimer()
        {
            Log.Information("Starting log refresh timer");
            _logRefreshTimer.Start();
        }
        public void StopLogTimer()
        {
            Log.Information("Stopping log refresh timer");
            _logRefreshTimer.Stop();
        }
        public void Dispose()
        {
            try
            {
                _debounceCts?.Cancel();
                _disposedCts?.Cancel();
                _listener.NotificationChanged -= Listener_NotificationChanged;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error during NotificationsViewModel.Dispose");
            }
        }
    }

    // Small extension to marshal synchronous action to DispatcherQueue and await completion
    static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this DispatcherQueue dq, Action action)
        {
            var tcs = new TaskCompletionSource<object?>();
            dq.TryEnqueue(() =>
            {
                try { action(); tcs.SetResult(null); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }
    }
}