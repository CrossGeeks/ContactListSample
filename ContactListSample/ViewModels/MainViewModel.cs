using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactListSample.Models;
using ContactListSample.Services;

namespace ContactListSample.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        IContactsService _contactService;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => "Contacts";

        public string SearchText { get; set; }
        public ObservableCollection<Contact> Contacts { get; set; }
        public ObservableCollection<Contact> FilteredContacts
        {
            get
            {
                return string.IsNullOrEmpty(SearchText) ? Contacts : new ObservableCollection<Contact>(Contacts?.ToList()?.Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Contains(SearchText.ToLower())));
            }
        }
        public MainViewModel(IContactsService contactService)
        {
            _contactService = contactService;
            Contacts = new ObservableCollection<Contact>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(Contacts, null, ObservableCollectionCallback);
            _contactService.OnContactLoaded += OnContactLoaded;
            LoadContacts();
        }


        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }

        private void OnContactLoaded(object sender, ContactEventArgs e)
        {
            Contacts.Add(e.Contact);
        }
        async Task LoadContacts()
        {
            try
            {
                await _contactService.RetrieveContactsAsync();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task was cancelled");
            }
        }
        
    }
}
