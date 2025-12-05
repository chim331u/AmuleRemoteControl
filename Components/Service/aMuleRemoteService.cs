using System.Globalization;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    public class aMuleRemoteService(
        ILogger<aMuleRemoteService> logger,
        IUtilityServices utilityServices,
        INetworkHelper networkHelperServices)
        : IAmuleRemoteServices
    {
        /// <summary>
        /// Thread safety lock object for all state properties.
        /// Prevents race conditions when multiple threads access Status or DownSpeed simultaneously.
        /// </summary>
        private readonly object _statusLock = new object();

        public event EventHandler? StatusChanged;
        private Stats? _status;
        /// <summary>
        /// Gets or sets the current aMule statistics. Thread-safe via lock.
        /// Setting this property triggers the StatusChanged event.
        /// </summary>
        public Stats? Status
        {
            get
            {
                lock (_statusLock)
                {
                    return _status;
                }
            }
            set
            {
                lock (_statusLock)
                {
                    _status = value;
                    // Fire event inside lock to ensure consistency
                    this.StatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? DownChanged;
        private string? _downSpeed;
        /// <summary>
        /// Gets or sets the total download speed. Thread-safe via lock.
        /// Setting this property triggers the DownChanged event.
        /// </summary>
        public string? DownSpeed
        {
            get
            {
                lock (_statusLock)
                {
                    return _downSpeed;
                }
            }
            set
            {
                lock (_statusLock)
                {
                    _downSpeed = value;
                    // Fire event inside lock to ensure consistency
                    this.DownChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        //public string? DownSpeed { get => downSpeed; set { downSpeed = value; NotifyStateChangedDownSpeed(); } }
        //private string? downSpeed;
        //public event Action? OnChangeDownSpeed;
        //private void NotifyStateChangedDownSpeed() => OnChangeDownSpeed?.Invoke();

        ILogger<aMuleRemoteService> _logger = logger;

        private readonly IUtilityServices _utilityServices = utilityServices;
        private readonly INetworkHelper _networkHelperServices = networkHelperServices;
        private const string DownloadPage = "amuleweb-main-dload.php";
        private const string Footer = "footer.php";
        private const string ServerPage = "amuleweb-main-servers.php";
        private const string StatPage = "stats.php";
        private const string SearchPage = "amuleweb-main-search.php";
        private const string LogPage = "log.php";
        private const string ServerInfoPage = "log.php?show=srv";
        private const string PrefPage = "amuleweb-main-prefs.php";
        private const string PreferencePage = "amuleweb-main-prefs.php";

        IList<DownloadFile> _downloadFiles = new List<DownloadFile>();

        public async Task AutoStatus()
        {
            Status = await GetStats();
        }

        public async Task AutoDownSpeed()
        {
            if (_downloadFiles!=null)
            {
                GetTotalDownloadSpeed();
            }

            //GetTotalDownloadSpeed();
        }

        private void GetTotalDownloadSpeed()
        {
            //   193.10 kb/s
            var speeds = _downloadFiles.Select(x => x.DownloadSpeed).ToList();

            double totalSpeed = speeds.Sum(item => GetSpeed(item));

            // Use current culture set by ICultureProvider
            DownSpeed = $" {totalSpeed.ToString("N2", CultureInfo.CurrentCulture)} kb/s";

        }

        private double GetSpeed(string textSpeed)
        {
            var indexOfMetric = textSpeed.IndexOf(" ");

            if (indexOfMetric > 0)
            {
                var speed = textSpeed.Substring(0, indexOfMetric);

                try
                {
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                    {
                        speed = speed.Replace(".", ",");
                    }
                    else
                    {
                        speed = speed.Replace(",", ".");
                    }

                    return Convert.ToDouble(speed);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error converting progress - {ex.Message}");
                    return 0;
                }
            }

            return 0;
        }

        #region Downloading
        public async Task<List<DownloadFile>> GetDownloading()
        {
            try
            {
                var downloading = ParseDownloading(await _networkHelperServices.SendRequest(DownloadPage));
                _downloadFiles = downloading;
                return downloading;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrive downloads: {ex.Message}");
                return null;
            }


        }

        public async Task<List<DownloadFile>> PostDownloadCommand(string fileId, string command)
        {
            // Input validation for fileId
            if (string.IsNullOrWhiteSpace(fileId))
            {
                _logger.LogWarning("PostDownloadCommand: fileId is null or empty");
                return null;
            }

            // Validate fileId is numeric (aMule uses numeric IDs)
            if (!long.TryParse(fileId, out _))
            {
                _logger.LogWarning($"PostDownloadCommand: Invalid fileId format '{fileId}' - must be numeric");
                return null;
            }

            // Input validation for command
            if (string.IsNullOrWhiteSpace(command))
            {
                _logger.LogWarning("PostDownloadCommand: command is null or empty");
                return null;
            }

            // Validate command is in allowed list
            var allowedCommands = new[] { "pause", "resume", "delete", "cancel", "priority" };
            if (!allowedCommands.Contains(command.ToLower()))
            {
                _logger.LogWarning($"PostDownloadCommand: Invalid command '{command}' - allowed: {string.Join(", ", allowedCommands)}");
                return null;
            }

            var param = new Dictionary<string, string>();
            param.Add(fileId, "on");
            param.Add("category", "all");
            param.Add("command", command);
            param.Add("status", "all");

            var result = await _networkHelperServices.PostRequest(DownloadPage, param);
            if (!string.IsNullOrEmpty(result))
            {
                return ParseDownloading(result);
            }

            return null;
        }

        public async Task<List<DownloadFile>> PostDownloadCommand(List<string> filesId, string command)
        {
            // Input validation for filesId list
            if (filesId == null || filesId.Count == 0)
            {
                _logger.LogWarning("PostDownloadCommand: filesId list is null or empty");
                return null;
            }

            // Validate each fileId is numeric
            foreach (var fileId in filesId)
            {
                if (string.IsNullOrWhiteSpace(fileId) || !long.TryParse(fileId, out _))
                {
                    _logger.LogWarning($"PostDownloadCommand: Invalid fileId format '{fileId}' in list - must be numeric");
                    return null;
                }
            }

            // Input validation for command
            if (string.IsNullOrWhiteSpace(command))
            {
                _logger.LogWarning("PostDownloadCommand: command is null or empty");
                return null;
            }

            // Validate command is in allowed list
            var allowedCommands = new[] { "pause", "resume", "delete", "cancel", "priority" };
            if (!allowedCommands.Contains(command.ToLower()))
            {
                _logger.LogWarning($"PostDownloadCommand: Invalid command '{command}' - allowed: {string.Join(", ", allowedCommands)}");
                return null;
            }

            var param = new Dictionary<string, string>();

            foreach (var fileId in filesId)
            {
                param.Add(fileId, "on");
            }
            param.Add("category", "all");
            param.Add("command", command);
            param.Add("status", "all");

            var result = await _networkHelperServices.PostRequest(DownloadPage, param);
            if (!string.IsNullOrEmpty(result))
            {
                return ParseDownloading(result);
            }

            return null;
        }

        public async Task<List<DownloadFile>> AddEd2kLink(string ed2kLink)
        {
            ed2kLink = ed2kLink.ReplaceLineEndings("+");

            var param = new Dictionary<string, string>();
            param.Add("Submit", "Download link");
            param.Add("ed2klink", ed2kLink);
            param.Add("selectcat", "all");

            var result = await _networkHelperServices.PostRequest(Footer, param);

            if (!string.IsNullOrEmpty(result))
            {
                return ParseDownloading(result);
            }

            return null;

        }

        private List<DownloadFile> ParseDownloading(string downloadingPage)
        {
            try
            {
                HtmlDocument docServer = new HtmlDocument();

                docServer.LoadHtml(downloadingPage);

                var query = from table in docServer.DocumentNode.SelectNodes("//table").Skip(6).Take(1).Cast<HtmlNode>()
                            from row in table.SelectNodes("tr").Skip(1).Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td|input").Cast<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int count = 0;
                DownloadFile files = new DownloadFile();
                var _fileList = new List<DownloadFile>();
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText))
                    {
                        if (cell.CellText.Contains("<input"))
                        {
                            files.FileId = GetIdFromInput(cell.CellText);

                        }
                        else
                        {
                            count++;
                            switch (count)
                            {
                                case 1:
                                    files.FileName = cell.CellText;
                                    break;
                                case 2:
                                    files.Size = cell.CellText;
                                    break;
                                case 3:
                                    files.Completed = cell.CellText.Replace($"&nbsp;", " ");
                                    if (!string.IsNullOrEmpty(files.Completed))
                                    {
                                        files.Progress = ConvertProgressToDouble(files.Completed);
                                    }
                                    break;
                                case 4:
                                    files.DownloadSpeed = cell.CellText;
                                    break;
                                case 5:
                                    //files.Progress = cell.CellText;
                                    break;
                                case 6:
                                    files.Sources = cell.CellText;
                                    break;
                                case 7:
                                    files.Status = cell.CellText;
                                    break;
                                case 8:
                                    files.Priority = cell.CellText;

                                    _fileList.Add(files);
                                    break;
                            }
                        }

                    }
                    else
                    {
                        if (count == 4)
                        {
                            count++;
                        }
                        else
                        {
                            count = 0;
                            files = new DownloadFile();
                        }
                    }
                }

                //_logger.LogInformation($"Retrived {_fileList.Count} downloads");
                return _fileList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }
        }

        private double ConvertProgressToDouble(string progress)
        {
            _logger.LogInformation($"string progress= {progress}");
            var startIndex = progress.IndexOf("(");
            var indexPercent = progress.IndexOf("%");
            if (indexPercent >= 0 && startIndex >= 0)
            {
                try
                {
                    var percent = progress.Substring(startIndex + 1, (indexPercent - (startIndex + 1)));

                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                    {
                        percent = percent.Replace(".", ",");
                    }
                    else
                    {
                        percent = percent.Replace(",", ".");
                    }

                    return Convert.ToDouble(percent);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error converting progress - {ex.Message}");
                    return 0;
                }
            }

            return 0;
        }

        private string GetIdFromInput(string input)
        {
            try
            {
                var startIndex = input.IndexOf($"name=\"") + 6;
                var endIndex = input.LastIndexOf("\"");
                return input.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting progress - {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Uploading
        public async Task<List<UploadFile>> GetUploads()
        {
            try
            {
                var uploads = ParseUpload(await _networkHelperServices.SendRequest(DownloadPage));

                return uploads;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrive downloads: {ex.Message}");
                return null;
            }


        }

        private List<UploadFile> ParseUpload(string downloadingPage)
        {
            try
            {
                HtmlDocument docServer = new HtmlDocument();

                docServer.LoadHtml(downloadingPage);

                var query = from table in docServer.DocumentNode.SelectNodes("//table").Skip(8).Take(1).Cast<HtmlNode>()
                            from row in table.SelectNodes("tr").Skip(2).Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td|input").Cast<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int count = 0;
                UploadFile files = new UploadFile();
                var _fileList = new List<UploadFile>();
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText) && !cell.CellText.Contains($"&nbsp;"))
                    {
                        count++;
                        switch (count)
                        {
                            case 1:
                                files.FileName = cell.CellText;
                                break;
                            case 2:
                                files.UserName = cell.CellText;
                                break;
                            case 3:
                                files.Up = cell.CellText;
                                break;
                            case 4:
                                files.Down = cell.CellText;
                                break;
                            case 7:
                                files.Speed = cell.CellText;
                                _fileList.Add(files);
                                break;
                            default:
                                count = 0;
                                break;

                        }
                    }
                    else
                    {
                        if (count == 4 || count == 5)
                        {
                            count++;
                        }
                        else
                        {
                            count = 0;
                            files = new UploadFile();
                        }

                    }

                }

                //_logger.LogInformation($"Retrived {_fileList.Count} downloads");
                return _fileList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }
        }

        #endregion

        #region Server
        public async Task<List<Servers>> GetServers()
        {
            var serverList = ParseServers(await _networkHelperServices.SendRequest(ServerPage));
            return serverList;

        }

        public async Task<List<Servers>> ConnectServer(string serverId, string serverPort)
        {
            //cmd: connect
            //ip: 516650843
            //port: 4321
            var serverList = ParseServers(await _networkHelperServices.SendRequest($"{ServerPage}?cmd=connect&ip={serverId}&port={serverPort}"));
            return serverList;
        }

        public async Task<List<Servers>> RemoveServer(string serverId, string serverPort)
        {
            var serverList = ParseServers(await _networkHelperServices.SendRequest($"{ServerPage}?cmd=remove&ip={serverId}&port={serverPort}"));
            return serverList;
        }

        private List<Servers> ParseServers(string serverHtmlPage)
        {
            HtmlDocument docServer = new HtmlDocument();

            docServer.LoadHtml(serverHtmlPage);

            var query = from table in docServer.DocumentNode.SelectNodes("//table").Skip(1).Cast<HtmlNode>()
                        from row in table.SelectNodes("tr").Skip(3).Cast<HtmlNode>()
                        from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                        select new { CellText = cell.InnerHtml };

            //select new { Table = table.Id, row.InnerHtml, CellText = cell.InnerText };
            int count = 0;
            Servers server = new Servers();
            var serverList = new List<Servers>();
            foreach (var cell in query)
            {
                if (!string.IsNullOrEmpty(cell.CellText))
                {
                    if (cell.CellText.Contains("<a href"))
                    {
                        var startIndex = cell.CellText.IndexOf($"ip=") + 3;
                        var endIndex = cell.CellText.IndexOf("&port=");
                        server.ServerId = cell.CellText.Substring(startIndex, endIndex - startIndex);

                        startIndex = cell.CellText.IndexOf($"port=") + 5;
                        endIndex = cell.CellText.IndexOf("\">");
                        server.Port = cell.CellText.Substring(startIndex, endIndex - startIndex);

                    }
                    else
                    {
                        count++;
                        switch (count)
                        {
                            case 1:
                                server.ServerName = cell.CellText;
                                break;
                            case 2:
                                server.Description = cell.CellText;
                                break;
                            case 3:
                                server.Address = cell.CellText;
                                break;
                            case 4:
                                server.Users = cell.CellText;
                                break;
                            case 5:
                                server.Files = cell.CellText;
                                serverList.Add(server);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    count = 0;
                    server = new Servers();
                }

            }

            _logger.LogInformation($"Retrived {serverList.Count} servers:");
            return serverList;

        }
        #endregion

        #region Stats
        public async Task<Stats> GetStats()
        {
            var stats = ParseStats(await _networkHelperServices.SendRequest(StatPage));
            return stats;

        }

        private Stats ParseStats(string statsHtmlPage)
        {
            HtmlDocument docServer = new HtmlDocument();

            docServer.LoadHtml(statsHtmlPage);

            var query = from table in docServer.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
                        from row in table.SelectNodes("tr").Cast<HtmlNode>()
                        from cell in row.SelectNodes("td").Cast<HtmlNode>()
                        select new { CellText = cell.InnerText };

            Stats _stats = new Stats();

            foreach (var cell in query)
            {
                if (cell.CellText.Contains("Ed2k"))
                {
                    _stats.Ed2kStat = cell.CellText.Replace("\n", string.Empty).Replace("Ed2k :", string.Empty).Trim();
                }

                if (cell.CellText.Contains("Kad"))
                {
                    _stats.KadStat = cell.CellText.Replace("\n", string.Empty).Replace("Kad :", string.Empty).Trim(); ;
                }

            }

            return _stats;
        }
        #endregion

        #region Shared Files

        #endregion

        #region Search

        public async Task<List<Search>> SearchFiles(string searchText, string searchType, string? targetCat)
        {
            //search type:
            //Global, Local, Kad

            // Input validation for searchText
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _logger.LogWarning("SearchFiles: searchText is null or empty");
                return new List<Search>(); // Return empty list instead of null for better UX
            }

            // Validate maximum length to prevent abuse
            const int MAX_SEARCH_LENGTH = 100;
            if (searchText.Length > MAX_SEARCH_LENGTH)
            {
                _logger.LogWarning($"SearchFiles: searchText exceeds maximum length of {MAX_SEARCH_LENGTH} characters");
                return new List<Search>();
            }

            // HTML encode searchText to prevent XSS attacks
            // This is important because the search text will be sent to aMule web interface
            string sanitizedSearchText = System.Web.HttpUtility.HtmlEncode(searchText);

            _logger.LogInformation($"SearchFiles: Searching for '{sanitizedSearchText}' (original length: {searchText.Length})");

            if (string.IsNullOrEmpty(targetCat))
            {
                targetCat = "all";
            }

            //command=search&searchval=ligabue&Search=Search&avail=&minsize=&minsizeu=MByte&searchtype=Local&maxsize=&maxsizeu=MByte&targetcat=all
            var param = new Dictionary<string, string>();
            param.Add("command", "search");
            param.Add("searchval", sanitizedSearchText);
            param.Add("Search", "Search");
            param.Add("avail", "");
            param.Add("minsize", "");
            param.Add("minsizeu", "MByte");
            param.Add("searchtype", searchType);
            param.Add("maxsize", "");
            param.Add("maxsizeu", "MByte");
            param.Add("targetcat", targetCat);

            var listSearch = ParseSearch(await _networkHelperServices.PostRequest(SearchPage, param));
            return listSearch ?? new List<Search>(); // Ensure we always return a list, never null
        }

        public async Task<List<Search>> RefreshSearch()
        {
            var listSearch = ParseSearch(await _networkHelperServices.SendRequest($"{SearchPage}?search_sort="));
            return listSearch;
        }


        private List<Search> ParseSearch(string searchPage)
        {
            try
            {
                Console.WriteLine("***********************************************************");
                HtmlDocument docServer = new HtmlDocument();

                docServer.LoadHtml(searchPage);

                var query = from row in docServer.DocumentNode.SelectNodes("//tr").Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int count = 0;
                Search files = new Search();
                var _fileList = new List<Search>();
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText))
                    {
                        if (cell.CellText.Contains("input type=\"checkbox\"") && !cell.CellText.Contains("table"))
                        {
                            var startIndex = cell.CellText.IndexOf($"name=\"") + 6;
                            var endIndex = cell.CellText.LastIndexOf("\"");
                            files.SearchId = cell.CellText.Substring(startIndex, endIndex - startIndex);
                            count++;
                        }
                        else
                        {

                            switch (count)
                            {
                                case 1:
                                    files.FileName = cell.CellText;
                                    count++;
                                    break;
                                case 2:
                                    files.FileSize = cell.CellText;
                                    count++;
                                    break;
                                case 3:
                                    files.Sources = cell.CellText;
                                    count = 0;
                                    _fileList.Add(files);
                                    break;
                                default:
                                    count = 0;
                                    break;

                            }

                        }
                    }
                    else
                    {

                        count = 0;
                        files = new Search();
                    }
                }

                //Console.WriteLine($"Retrived {_fileList.Count} files:");
                return _fileList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in parse search: {ex.Message}");
                return null;
            }


        }

        public async Task<string> DownloadSearch(string fileId)
        {

            var param = new Dictionary<string, string>();
            param.Add("command", "download");
            param.Add("searchval", "");
            param.Add("avail", "");
            param.Add("minsize", "");
            param.Add("minsizeu", "MByte");
            param.Add("searchtype", "Global");
            param.Add("maxsize", "");
            param.Add("maxsizeu", "MByte");
            param.Add(fileId, "on");
            param.Add("Download", "Download");
            param.Add("targetcat", "all");


            var result = await _networkHelperServices.SendRequest(SearchPage, param);
            if (!string.IsNullOrEmpty(result))
            {
                return "File in download";
            }
            else
            { return null; }



            //download
            ////command: download
            ////searchval: 
            ////avail: 
            ////minsize: 
            ////minsizeu: MByte
            ////searchtype: Local
            ////maxsize: 
            ////maxsizeu: MByte
            ////B3DA63E3AE699395F593D732189C50FB: on
            ////94C46DFBAD8CEE5C8FDE3EF6260C339F: on
            ////Download: Download
            ////targetcat: all

            //command=download&searchval=&avail=&minsize=&minsizeu=MByte&searchtype=Local&maxsize=&maxsizeu=MByte&06D7D1C8CECC6E3CFBDF925A26C0CC8E=on&Download=Download&targetcat=all
        }

        #endregion

        #region Kad

        #endregion

        #region Main Stats

        #endregion

        #region aMule Log

        public async Task<string> GetaMuleLog()
        {
            var _log = ParseLog(await _networkHelperServices.SendRequest(LogPage));
            return _log;

        }

        public async Task<string> GetServerInfo()
        {
            var _serverInfo = ParseLog(await _networkHelperServices.SendRequest(ServerInfoPage));
            return _serverInfo;

        }

        private string ParseLog(string serverHtmlPage)
        {
            HtmlDocument docServer = new HtmlDocument();

            docServer.LoadHtml(serverHtmlPage);

            var query = from cell in docServer.DocumentNode.SelectNodes("//pre").Cast<HtmlNode>()
                        select new { CellText = cell.InnerText };

            return query.FirstOrDefault().CellText.ToString();
        }
        #endregion

        #region aMule Configuration


        public async Task<PreferenceModel> GetPreferences()
        {
            try
            {
                var _preferences = ParsePreferences(await _networkHelperServices.SendRequest(PreferencePage));

                return _preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrive preferences: {ex.Message}");
                return null;
            }


        }

        private PreferenceModel ParsePreferences(string preferencesPage)
        {
            try
            {
                HtmlDocument docServer = new HtmlDocument();

                docServer.LoadHtml(preferencesPage);

                var query = docServer.DocumentNode.SelectNodes("//script");

                PreferenceModel preferenceModel = new PreferenceModel();

                foreach (var row in query)
                {
                    var rowString = row.InnerHtml;
                    if (!string.IsNullOrEmpty(rowString) && rowString.Contains("initvals"))
                    {
                        int startIndex = rowString.IndexOf($"initvals[");
                        int endIndex = rowString.IndexOf($"<!--");
                        var _preferences = rowString.Substring(startIndex, (endIndex - 2) - startIndex);


                        var splittedPreference = _preferences.TrimEnd().Split(';');

                        foreach (var item in splittedPreference)
                        {
                            if (item.Length > 0)
                            {
                                int startPrefIndex = 10;
                                int endPrefIndex = item.IndexOf($"]") - 1;
                                int startValueIndex = item.IndexOf($"= ") + 3;
                                int endValueIndex = item.LastIndexOf($"\"");

                                switch (item.Substring(startPrefIndex, endPrefIndex - startPrefIndex))
                                {
                                    case "max_line_up_cap":
                                        preferenceModel.MaxUploadRate_Statistic = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "max_line_down_cap":
                                        preferenceModel.MaxDownloadRate_Statistic = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "max_up_limit":
                                        preferenceModel.MaxUploadRate = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "max_down_limit":
                                        preferenceModel.MaxDownloadRate = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "slot_alloc":
                                        preferenceModel.SlotAllocation = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "tcp_port":
                                        preferenceModel.TCPport = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "udp_port":
                                        preferenceModel.UDPport = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "udp_dis":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.DisableUDPconnections = true;
                                        }
                                        else
                                        {
                                            preferenceModel.DisableUDPconnections = false;
                                        }
                                        break;
                                    case "max_file_src":
                                        preferenceModel.MaxSourcePerFile = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "max_conn_total":
                                        preferenceModel.MaxTotalConnection = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "autoconn_en":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.AutoConnectAtStartUp = true;
                                        }
                                        else
                                        {
                                            preferenceModel.AutoConnectAtStartUp = false;
                                        }
                                        break;
                                    case "reconn_en":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.ReconnectWhenLostConnection = true;
                                        }
                                        else
                                        {
                                            preferenceModel.ReconnectWhenLostConnection = false;
                                        }
                                        break;
                                    case "ich_en":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.ICHactive = true;
                                        }
                                        else
                                        {
                                            preferenceModel.ICHactive = false;
                                        }
                                        break;
                                    case "aich_trust":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.AICHtrustsEveryHash = true;
                                        }
                                        else
                                        {
                                            preferenceModel.AICHtrustsEveryHash = false;
                                        }
                                        break;
                                    case "new_files_paused":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.AddFileDownloadQueuePauseMode = true;
                                        }
                                        else
                                        {
                                            preferenceModel.AddFileDownloadQueuePauseMode = false;
                                        }
                                        break;
                                    case "new_files_auto_dl_prio":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.AddDownloadFileAutoPriority = true;
                                        }
                                        else
                                        {
                                            preferenceModel.AddDownloadFileAutoPriority = false;
                                        }
                                        break;
                                    case "preview_prio":
                                        //preferenceModel.ReconnectWhenLostConnection = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "new_files_auto_ul_prio":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.NewSharedFileAutoPriority = true;
                                        }
                                        else
                                        {
                                            preferenceModel.NewSharedFileAutoPriority = false;
                                        }
                                        break;
                                    case "upload_full_chunks":
                                        //preferenceModel.AllocFullChunksPartFiles = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "first_last_chunks_prio":
                                        //preferenceModel.NewSharedFileAutoPriority = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "start_next_paused":
                                        //preferenceModel.NewSharedFileAutoPriority = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "resume_same_cat":
                                        //preferenceModel.NewSharedFileAutoPriority = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "save_sources":
                                        //preferenceModel.AllocFullChunksPartFiles = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "extract_metadata":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.ExtractMetadataTags = true;
                                        }
                                        else
                                        {
                                            preferenceModel.ExtractMetadataTags = false;
                                        }
                                        break;
                                    case "alloc_full":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.AllocFullDiskSpacePartFiles = true;
                                        }
                                        else
                                        {
                                            preferenceModel.AllocFullDiskSpacePartFiles = false;
                                        }
                                        break;
                                    case "check_free_space":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.CheckFreeSpace = true;
                                        }
                                        else
                                        {
                                            preferenceModel.CheckFreeSpace = false;
                                        }
                                        break;
                                    case "min_free_space":
                                        preferenceModel.MinimumFreeSpaceMb = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    case "use_gzip":
                                        if (item.Substring(startValueIndex, endValueIndex - startValueIndex) == "1")
                                        {
                                            preferenceModel.UseGzipCompression = true;
                                        }
                                        else
                                        {
                                            preferenceModel.UseGzipCompression = false;
                                        }
                                        break;
                                    case "autorefresh_time":
                                        preferenceModel.PageRefreshInterval = item.Substring(startValueIndex, endValueIndex - startValueIndex);
                                        break;
                                    default:
                                        break;
                                }
                            }

                        }


                    }
                }

                return preferenceModel;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;

            }


        }

        public async Task<PreferenceModel> PostPreferencesCommand(PreferenceModel preference)
        {
            var param = new Dictionary<string, string>();
            param.Add("autorefresh_time", preference.PageRefreshInterval);
            if (preference.UseGzipCompression)
            {
                param.Add("use_gzip", "on");
            }
            param.Add("max_line_down_cap", preference.MaxDownloadRate_Statistic);
            param.Add("max_line_up_cap", preference.MaxUploadRate_Statistic);
            param.Add("max_down_limit", preference.MaxDownloadRate);
            param.Add("max_up_limit", preference.MaxUploadRate);
            param.Add("slot_alloc", preference.SlotAllocation);
            if (preference.CheckFreeSpace)
            {
                param.Add("check_free_space", "on");
            }
            if (preference.AddDownloadFileAutoPriority)
            {
                param.Add("new_files_auto_dl_prio", "on");
            }
            if (preference.NewSharedFileAutoPriority)
            {
                param.Add("new_files_auto_ul_prio", "on");
            }
            if (preference.ICHactive)
            {
                param.Add("ich_en", "on");
            }
            if (preference.AICHtrustsEveryHash)
            {
                param.Add("aich_trust", "on");
            }
            param.Add("max_conn_total", preference.MaxTotalConnection);
            param.Add("max_file_src", preference.MaxSourcePerFile);
            if (preference.AutoConnectAtStartUp)
            {
                param.Add("autoconn_en", "on");
            }
            if (preference.ReconnectWhenLostConnection)
            {
                param.Add("reconn_en", "on");
            }
            if (preference.AddFileDownloadQueuePauseMode)
            {
                param.Add("new_files_paused", "on");
            }
            if (preference.ExtractMetadataTags)
            {
                param.Add("extract_metadata", "on");
            }
            if (preference.AllocFullDiskSpacePartFiles)
            {
                param.Add("alloc_full", "on");
            }
            param.Add("tcp_port", preference.TCPport);
            param.Add("udp_port", preference.UDPport);
            param.Add("min_free_space", preference.MinimumFreeSpaceMb);
            param.Add("Submit", "Apply");
            param.Add("command", "");


            var result = await _networkHelperServices.PostRequest(PreferencePage, param);
            if (!string.IsNullOrEmpty(result))
            {
                return ParsePreferences(result);

            }

            return null;
        }

        //post  amuleweb-main-prefs.php
        //payload autorefresh_time=120&use_gzip=on&max_line_down_cap=300&max_line_up_cap=100&max_down_limit=200&max_up_limit=10&slot_alloc=2&check_free_space=on&min_free_space=1&new_files_auto_dl_prio=on&new_files_auto_ul_prio=on&ich_en=on&max_conn_total=500&max_file_src=300&autoconn_en=on&reconn_en=on&tcp_port=4662&udp_port=4672&Submit=Apply&command=

        //set pref
        ////autorefresh_time: 120
        ////use_gzip: on
        ////max_line_down_cap: 300
        ////max_line_up_cap: 100
        ////max_down_limit: 200
        ////max_up_limit: 10
        ////slot_alloc: 2
        ////check_free_space: on
        ////min_free_space: 1
        ////new_files_auto_dl_prio: on
        ////new_files_auto_ul_prio: on
        ////ich_en: on
        ////max_conn_total: 500
        ////max_file_src: 300
        ////autoconn_en: on
        ////reconn_en: on
        ////tcp_port: 4662
        ////udp_port: 4672
        ////Submit: Apply
        ////command: 

        #endregion
    }
}
