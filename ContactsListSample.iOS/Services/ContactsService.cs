﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contacts;
using ContactListSample.Helpers;
using ContactListSample.Models;
using ContactListSample.Services;
using ContactsUI;
using Foundation;
using UIKit;

namespace ContactListSample.iOS.Services
{
    public class ContactsService : NSObject, IContactsService
    {
        bool requestStop = false;

        public event EventHandler<ContactEventArgs> OnContactLoaded;

        bool _isLoading = false;
        public bool IsLoading => _isLoading;

        public async Task<IList<Contact>> RetrieveContactsAsync(CancellationToken? cancelToken = null)
        {
            requestStop = false;

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            // We create a TaskCompletionSource of decimal
            var taskCompletionSource = new TaskCompletionSource<IList<Contact>>();

            // Registering a lambda into the cancellationToken
            cancelToken.Value.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                requestStop = true;
                taskCompletionSource.TrySetCanceled();
            });

            _isLoading = true;

            var task = LoadContactsAsync();

            // Wait for the first task to finish among the two
            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);
            _isLoading = false;

            return await completedTask;
           
        }

        public async Task<bool> RequestPermissionAsync()
        {
            var status = CNContactStore.GetAuthorizationStatus(CNEntityType.Contacts);

            Tuple<bool, NSError> authotization=new Tuple<bool, NSError>(status == CNAuthorizationStatus.Authorized,null);

            if(status == CNAuthorizationStatus.NotDetermined)
            {
                using (var store = new CNContactStore())
                {
                    authotization = await store.RequestAccessAsync(CNEntityType.Contacts);
                }
            }
            return authotization.Item1;
           
        }

        async Task<IList<Contact>> LoadContactsAsync()
        {
            IList<Contact> contacts = new List<Contact>();
            var hasPermission = await RequestPermissionAsync();
            if (hasPermission)
            {
               
                NSError error = null;
                var keysToFetch = new[] { CNContactKey.PhoneNumbers, CNContactKey.GivenName, CNContactKey.FamilyName, CNContactKey.EmailAddresses, CNContactKey.ImageDataAvailable, CNContactKey.ThumbnailImageData };

                var request = new CNContactFetchRequest(keysToFetch: keysToFetch);

                using (var store = new CNContactStore())
                {
                    var result = store.EnumerateContacts(request, out error, new CNContactStoreListContactsHandler((CNContact c, ref bool stop) =>
                    {
                       
                        string path = null;
                        if (c.ImageDataAvailable)
                        {
                            path = FileHelper.GetOutputPath(MediaFileType.Image, FileHelper.TemporalDirectoryName, $"{FileHelper.ThumbnailPrefix}-{Guid.NewGuid()}");

                            if (!File.Exists(path))
                            {
                                var imageData = c.ThumbnailImageData;
                                imageData?.Save(path, true);


                            }
                        }
                        var contact = new Contact()
                        {
                            Name = string.IsNullOrEmpty(c.FamilyName) ? c.GivenName : $"{c.GivenName} {c.FamilyName}",
                            Image = path,
                            PhoneNumbers = c.PhoneNumbers?.Select(p => p?.Value?.StringValue).ToArray(),
                            Emails = c.EmailAddresses?.Select(p => p?.Value?.ToString()).ToArray(),

                        };
                        OnContactLoaded?.Invoke(this, new ContactEventArgs(contact));

                        contacts.Add(contact);

                        stop = requestStop;

                    }));
                }
            }

            return contacts;
        }
        
        
    }
}
