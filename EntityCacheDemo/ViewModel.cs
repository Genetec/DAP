namespace EntityCacheDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Queries;
    using Prism.Commands;
    using Prism.Mvvm;

    internal class EntityQueryViewModel : BindableBase
    {
        private string m_description;

        private StringSearchMode m_descriptionSearchMode;

        private bool m_downloadAllRelatedData;

        private bool m_guidsOnly;

        private string m_name;

        private StringSearchMode m_nameSearchMode;

        private Guid m_owner;

        private OrderByType m_sortOrder;

        private bool m_specifiedEndTime;

        private bool m_specifiedStartTime;

        private bool m_strictResults;

        public bool StrictResults
        {
            get => m_strictResults;
            set => SetProperty(ref m_strictResults, value);
        }

        public bool DownloadAllRelatedData
        {
            get => m_downloadAllRelatedData;
            set => SetProperty(ref m_downloadAllRelatedData, value);
        }

        public string Name
        {
            get => m_name;
            set => SetProperty(ref m_name, value);
        }

        public StringSearchMode NameSearchMode
        {
            get => m_nameSearchMode;
            set => SetProperty(ref m_nameSearchMode, value);
        }

        public StringSearchMode DescriptionSearchMode
        {
            get => m_descriptionSearchMode;
            set => SetProperty(ref m_descriptionSearchMode, value);
        }

        public string Description
        {
            get => m_description;
            set => SetProperty(ref m_description, value);
        }

        public OrderByType SortOrder
        {
            get => m_sortOrder;
            set => SetProperty(ref m_sortOrder, value);
        }

        public bool SpecifiedEndTime
        {
            get => m_specifiedEndTime;
            set => SetProperty(ref m_specifiedEndTime, value);
        }

        public bool SpecifiedStartTime
        {
            get => m_specifiedStartTime;
            set => SetProperty(ref m_specifiedStartTime, value);
        }

        public Guid Owner
        {
            get => m_owner;
            set => SetProperty(ref m_owner, value);
        }

        public bool GuidsOnly
        {
            get => m_guidsOnly;
            set => SetProperty(ref m_guidsOnly, value);
        }
    }

    internal class CardholderQueryViewModel : EntityQueryViewModel
    {
        private readonly Engine m_engine = new Engine();

        private string m_email;

        private StringSearchMode m_emailSearchMode;

        private string m_firstName;

        private OrderByType m_firstNameOrder;

        private StringSearchMode m_firstNameSearchMode;

        private string m_fullName;

        private StringSearchMode m_fullNameSearchMode;

        private bool m_isConnected;

        private string m_lastName;

        private OrderByType m_lastNameOrder;

        private StringSearchMode m_lastNameSearchMode;

        private string m_mobilePhoneNumber;

        private StringSearchMode m_mobilePhoneNumberSearchMode;

        private string m_password;

        private string m_server;

        private bool m_shared;

        private bool m_sharedOverride;

        private bool m_sharedOverrideSearch;

        private string m_username;

        private bool m_visisorsOnly;

        public CardholderQueryViewModel()
        {
            m_engine.LoginManager.LoggedOn += (sender, e) => IsConnected = true;
            m_engine.LoginManager.LoggedOff += (sender, e) => IsConnected = false;

            LogonCommand = new DelegateCommand(async () =>
            {
                ConnectionStateCode state = await m_engine.LoginManager.LogOnAsync(Server, Username, Password);

                if (state != ConnectionStateCode.Success)
                {
                    // Show error message
                }
            }, () => !IsConnected).ObservesProperty(() => IsConnected);

            LogOffCommand = new DelegateCommand(() => m_engine.LoginManager.LogOff(), () => IsConnected).ObservesProperty(() => IsConnected);

            QueryCommand = new DelegateCommand(async () =>
            {
                var query = (CardholderConfigurationQuery)m_engine.ReportManager.CreateReportQuery(ReportType.CardholderConfiguration);

                query.FirstName = FirstName;
                query.FirstNameSearchMode = FirstNameSearchMode;

                query.LastName = LastName;
                query.LastNameSearchMode = LastNameSearchMode;

                query.FullName = FullName;
                query.FullNameSearchMode = FullNameSearchMode;

                query.Email = Email;
                query.EmailSearchMode = EmailSearchMode;

                query.MobilePhoneNumber = MobilePhoneNumber;
                query.MobilePhoneNumberSearchMode = MobilePhoneNumberSearchMode;

                query.Name = Name;
                query.NameSearchMode = NameSearchMode;

                query.Description = Description;
                query.DescriptionSearchMode = DescriptionSearchMode;

                foreach (CardholderState state in CardholderStates)
                {
                    query.AccessStatus.Add(state);
                }

                query.VisitorsOnly = VisisorsOnly;

                query.FirstNameOrder = FirstNameOrder;
                query.LastNameOrder = LastNameOrder;
                query.SortOrder = SortOrder;

                query.EntitySharingCriteria = new EntitySharingCriteria { Shared = Shared, SharedOverride = SharedOverride, SharedOverrideSearch = SharedOverrideSearch };

                query.DownloadAllRelatedData = DownloadAllRelatedData;
                query.StrictResults = StrictResults;

                query.ActivationTimeRange.SetTimeRange(ActivationStartTime, ActivationEndTime);

                query.ExpirationTimeRange.SetTimeRange(ExpirationStartTime, ExpirationEndTime);

                query.SpecifiedEndTime = SpecifiedEndTime;

                QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);



                results.Data.AsEnumerable().Select(row => row.Field<Guid>(nameof(Guid))).Select(m_engine.GetEntity).OfType<Cardholder>().ToList();

            }, () => IsConnected).ObservesProperty(() => IsConnected);
        }

        public DateTime ExpirationEndTime { get; set; }

        public DateTime ExpirationStartTime { get; set; }

        public DateTime ActivationEndTime { get; set; }

        public DateTime ActivationStartTime { get; set; }

        public string Server
        {
            get => m_server;
            set => SetProperty(ref m_server, value);
        }

        public string Username
        {
            get => m_username;
            set => SetProperty(ref m_username, value);
        }

        public string Password
        {
            get => m_password;
            set => SetProperty(ref m_password, value);
        }

        public bool SharedOverrideSearch
        {
            get => m_sharedOverrideSearch;
            set => SetProperty(ref m_sharedOverrideSearch, value);
        }

        public bool SharedOverride
        {
            get => m_sharedOverride;
            set => SetProperty(ref m_sharedOverride, value);
        }

        public bool Shared
        {
            get => m_shared;
            set => SetProperty(ref m_shared, value);
        }

        public OrderByType LastNameOrder
        {
            get => m_lastNameOrder;
            set => SetProperty(ref m_lastNameOrder, value);
        }

        public OrderByType FirstNameOrder
        {
            get => m_firstNameOrder;
            set => SetProperty(ref m_firstNameOrder, value);
        }

        public ObservableCollection<CardholderState> CardholderStates { get; }
            = new ObservableCollection<CardholderState>(Enum.GetValues(typeof(CardholderState)).Cast<CardholderState>());

        public bool IsConnected
        {
            get => m_isConnected;
            private set => SetProperty(ref m_isConnected, value);
        }

        public bool VisisorsOnly
        {
            get => m_visisorsOnly;
            set => SetProperty(ref m_visisorsOnly, value);
        }

        public string FirstName
        {
            get => m_firstName;
            set => SetProperty(ref m_firstName, value);
        }

        public StringSearchMode FirstNameSearchMode
        {
            get => m_firstNameSearchMode;
            set => SetProperty(ref m_firstNameSearchMode, value);
        }

        public string LastName
        {
            get => m_lastName;
            set => SetProperty(ref m_lastName, value);
        }

        public StringSearchMode LastNameSearchMode
        {
            get => m_lastNameSearchMode;
            set => SetProperty(ref m_lastNameSearchMode, value);
        }

        public string FullName
        {
            get => m_fullName;
            set => SetProperty(ref m_fullName, value);
        }

        public StringSearchMode FullNameSearchMode
        {
            get => m_fullNameSearchMode;
            set => SetProperty(ref m_fullNameSearchMode, value);
        }

        public string Email
        {
            get => m_email;
            set => SetProperty(ref m_email, value);
        }

        public StringSearchMode EmailSearchMode
        {
            get => m_emailSearchMode;
            set => SetProperty(ref m_emailSearchMode, value);
        }

        public string MobilePhoneNumber
        {
            get => m_mobilePhoneNumber;
            set => SetProperty(ref m_mobilePhoneNumber, value);
        }

        public StringSearchMode MobilePhoneNumberSearchMode
        {
            get => m_mobilePhoneNumberSearchMode;
            set => SetProperty(ref m_mobilePhoneNumberSearchMode, value);
        }

        public DelegateCommand QueryCommand { get; }

        public DelegateCommand LogonCommand { get; }

        public DelegateCommand LogOffCommand { get; }
    }
}