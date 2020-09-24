using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Zorbo.Core;
using Zorbo.Core.Interfaces;
using Zorbo.Core.Interfaces.Server;
using Zorbo.Core.Models;

namespace Zorbo.Ares.Server.Users
{
    public class Admin : ModelList<IClient>, IAdmins
    {
#pragma warning disable IDE0044 // Add readonly modifier
        IServer server = null;
        Passwords passwords = null;
#pragma warning restore IDE0044 // Add readonly modifier

        static readonly string FilePath = Path.Combine(Directories.Cache, "admin.json");

        protected internal IServer Server {
            get { return server; }
            set { OnPropertyChanged(() => server, value); }
        }

        [JsonIgnore]
        public IPasswords Passwords {
            get { return passwords; }
            set { OnPropertyChanged(() => passwords, value); }
        }

        public Admin() { }

        public void Load(IServer server)
        {
            Server = server;
            Server.Users.CollectionChanged += Users_CollectionChanged;
            passwords = new Passwords(server, Persistence.LoadModel<ModelList<Password>>(FilePath));
        }

        public async Task LoadAsync(IServer server) 
        {
            Server = server;
            Server.Users.CollectionChanged += Users_CollectionChanged;
            passwords = new Passwords(server, await Persistence.LoadModelAsync<ModelList<Password>>(FilePath));
        }

        public void Save()
        {
            Persistence.SaveModel(passwords.Cast<Password>(), FilePath);
        }

        public async Task SaveAsync() 
        {
            await Persistence.SaveModelAsync(passwords.Cast<Password>(), FilePath);
        }

        void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

            if (e.Action == NotifyCollectionChangedAction.Add) {
                IClient client = (IClient)e.NewItems[0];

                client.PropertyChanged += User_PropertyChanged;

                if (client.Admin > AdminLevel.User) 
                    AddIfUnique(client);

            }
            else if (e.Action == NotifyCollectionChangedAction.Remove) {
                IClient client = (IClient)e.OldItems[0];

                client.PropertyChanged -= User_PropertyChanged;

                if (client.Admin > AdminLevel.User) 
                    Remove(client);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset) {

                this.ForEach((s) => s.PropertyChanged -= User_PropertyChanged);
                this.Clear();

                RaisePropertyChanged(nameof(Count));
            }
        }

        void User_PropertyChanged(object sender, PropertyChangedEventArgs e) {

            if (e.PropertyName == "Admin") {
                IClient admin = (IClient)sender;

                if (admin.Admin > AdminLevel.User)
                    AddIfUnique(admin);

                else Remove(admin);
            }
        }
    }
}
