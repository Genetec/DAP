namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Prism.Commands;
    using Prism.Mvvm;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Queries;

    internal class AlarmMonitorViewModel : BindableBase
    {
        private readonly Engine m_engine = new Engine();

        private bool m_isConnected;

        private string m_password;

        private AlarmInstance m_selectedInstance;

        private string m_server = "localhost";

        private string m_username = "admin";

        public AlarmMonitorViewModel()
        {
            m_engine.LoginManager.LoggedOn += (sender, e) => IsConnected = true;
            m_engine.LoginManager.LoggedOff += (sender, e) => IsConnected = false;

            RefreshCommand = new DelegateCommand(async () => await Refresh(), () => IsConnected).ObservesProperty(() => IsConnected);

            LogOnCommand = new DelegateCommand(async () =>
            {
                ConnectionStateCode state = await m_engine.LoginManager.LogOnAsync(Server, Username, Password);

                if (state != ConnectionStateCode.Success)
                {
                }
            }, () => !IsConnected).ObservesProperty(() => IsConnected);

            LogOffCommand = new DelegateCommand(() => m_engine.LoginManager.LogOff(), () => IsConnected).ObservesProperty(() => IsConnected);

            ForceAcknowledgeAllAlarmsCommand = new DelegateCommand(() => m_engine.AlarmManager.ForceAcknowledgeAllAlarms(), () => IsConnected)
                .ObservesProperty(() => IsConnected);

            ForwardAlarmCommand = new DelegateCommand(() =>
                {

                    m_engine.AlarmManager.ForwardAlarm(SelectedInstance.Id,);

                }, () => IsConnected && SelectedInstance != null)
                .ObservesProperty(() => SelectedInstance)
                .ObservesProperty(() => IsConnected);

            AcknowledgeCommand = new DelegateCommand(() => m_engine.AlarmManager.AcknowledgeAlarm(SelectedInstance.Id, SelectedInstance.Alarm, AcknowledgementType.Ack),
                    () => IsConnected && SelectedInstance != null)
                .ObservesProperty(() => SelectedInstance)
                .ObservesProperty(() => IsConnected);

            InvestigateCommand = new DelegateCommand(() => m_engine.AlarmManager.InvestigateAlarm(SelectedInstance.Id, SelectedInstance.Alarm),
                    () => IsConnected && SelectedInstance != null)
                .ObservesProperty(() => SelectedInstance)
                .ObservesProperty(() => IsConnected);


            EditContextCommand = new DelegateCommand(() =>
                    {

                        m_engine.AlarmManager.EditAlarmContext(SelectedInstance.Id, SelectedInstance.Alarm, Context);
                    },
                    () => IsConnected && SelectedInstance != null)
                .ObservesProperty(() => SelectedInstance)
                .ObservesProperty(() => IsConnected);



            TriggerAlarmCommand = new DelegateCommand(() =>
                    {

                        m_engine.AlarmManager.TriggerAlarm();
                    },
                    () => IsConnected)
                .ObservesProperty(() => IsConnected);

            m_engine.AlarmTriggered += (sender, e) =>
            {
                var instance = new AlarmInstance
                {
                    Id = e.InstanceId
                };

                Instances.Add(instance);
            };

            m_engine.AlarmAcknowledged += (sender, e) =>
            {
                AlarmInstance instance = Instances.FirstOrDefault(i => i.Id == e.InstanceId);

                if (instance != null)
                {
                    instance.State = AlarmState.Acked;
                    instance.AcknowledgedOn = e.AckTime;
                    instance.AcknowledgedBy = m_engine.GetEntity(e.AckBy)?.Name;
                }
            };

            m_engine.AlarmInvestigating += (sender, e) =>
            {
                AlarmInstance instance = Instances.FirstOrDefault(i => i.Id == e.InstanceId);

                if (instance != null)
                {
                }
            };
        }

        public DelegateCommand TriggerAlarmCommand { get; set; }

        public DelegateCommand ForwardAlarmCommand { get; set; }

        public DelegateCommand ForceAcknowledgeAllAlarmsCommand { get; }

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

        public bool IsConnected
        {
            get => m_isConnected;
            private set
            {
                if (SetProperty(ref m_isConnected, value))
                {
                    if (!m_isConnected)
                        Instances.Clear();
                    else
                        _ = Refresh();
                }
            }
        }

        public DelegateCommand LogOnCommand { get; }

        public DelegateCommand LogOffCommand { get; }

        public DelegateCommand RefreshCommand { get; }

        public AlarmInstance SelectedInstance
        {
            get => m_selectedInstance;
            set => SetProperty(ref m_selectedInstance, value);
        }

        public ObservableCollection<AlarmInstance> Instances { get; } = new ObservableCollection<AlarmInstance>();

        public DelegateCommand AcknowledgeCommand { get; }

        public DelegateCommand InvestigateCommand { get; }

        public DelegateCommand EditContextCommand { get; }

        private AlarmInstance CreateAlarmInstance(DataRow row)
        {
            var alarm = (Alarm)m_engine.GetEntity(row.Field<Guid>(AlarmActivityQuery.AlarmColumnName));

            Entity triggerEntity = m_engine.GetEntity(row.Field<Guid>(AlarmActivityQuery.TriggerEntityColumnName));

            return new AlarmInstance
            {
                Id = row.Field<int>(AlarmActivityQuery.InstanceIdColumnName),
                Alarm = alarm.Guid,
                Name = alarm.Name,
                TriggerEvent = row.Field<EventType?>(AlarmActivityQuery.TriggerEventColumnName),
                SourceName = triggerEntity.Name,
                SourceType = triggerEntity.EntityType,
                Context = row.Field<string>(AlarmActivityQuery.DynamicContextColumnName),
                InvestigatedBy = m_engine.GetEntity(row.Field<Guid>(AlarmActivityQuery.InvestigatedByColumnName))?.Name,
                InvestigatedOn = row.Field<DateTime?>(AlarmActivityQuery.InvestigatedTimeColumnName),
                AcknowledgedBy = m_engine.GetEntity(row.Field<Guid>(AlarmActivityQuery.AckedByColumnName))?.Name,
                AcknowledgedOn = row.Field<DateTime?>(AlarmActivityQuery.AckedTimeColumnName),
                TriggerTime = row.Field<DateTime>(AlarmActivityQuery.TriggerTimeColumnName),
                State = row.Field<AlarmState>(AlarmActivityQuery.StateColumnName),
                Priority = row.Field<byte>(AlarmActivityQuery.PriorityColumnName),
                OccurencePeriod = row.Field<OfflinePeriodType>(AlarmActivityQuery.OfflinePriodColumnName),
                HasSourceCondition = row.Field<bool>(AlarmActivityQuery.HasSourceConditionColumnName)
            };
        }

        private async Task Refresh()
        {
            var query = (AlarmActivityQuery)m_engine.ReportManager.CreateReportQuery(ReportType.AlarmActivity);

            // Only retreive alarm triggered in the last 30 days
            query.TriggeredTimeRange.SetTimeRange(TimeSpan.FromDays(30));

            QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            Instances.Clear();

            foreach (AlarmInstance instance in result.Data.AsEnumerable().Select(CreateAlarmInstance))
            {
                Instances.Add(instance);
            }
        }
    }
}