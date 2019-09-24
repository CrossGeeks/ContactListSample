using System;
using ContactListSample.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ContactListSample
{
    public partial class App : Application
    {
        IContactsService _contactsService;
        public App(IContactsService contactsService)
        {
            _contactsService = contactsService;
            InitializeComponent();
         
            MainPage = new NavigationPage(new MainPage(_contactsService));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
           

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
          
        }
    }
}
