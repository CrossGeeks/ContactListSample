using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ObservableCollection<Contact> Contacts { get; set; }
        public MainViewModel(IContactsService contactService)
        {
            _contactService = contactService;
            Contacts = new ObservableCollection<Contact>();
            _contactService.OnContactLoaded += OnContactLoaded;

            LoadContacts();
           
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
