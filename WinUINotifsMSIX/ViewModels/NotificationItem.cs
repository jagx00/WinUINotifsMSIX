using System;

namespace WinUINotifsMSIX.ViewModels
{
    public class NotificationItem : ViewModelBase
    {
        private uint _id = 0;
        private DateTimeOffset _creationTime;
        private string? _source;
        private string? _title;
        private string? _body;
        private int _visibility;

        public uint ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public DateTimeOffset CreationTime
        {
            get => _creationTime;
            set => SetProperty(ref _creationTime, value);
        }

        public string? Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string? Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }

        /// <summary>
        /// Integer visibility value (your domain specific meaning). Consider using an enum instead.
        /// </summary>
        public int Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }
    }
}