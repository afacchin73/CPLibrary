using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPLibrary
{
    using NetDialerProviderInterface;
    using Phones.Messaging;

    public interface IDatabaseHandler
    {

        // proprietà dell'iterfaccia da settare
        #region Proprità
        string ConnectionString
        {
            set;
        }

        string ContactTable
        {
            set;
        }

        string NomeCampagna
        {
            set;
        }
        string IdCampagna
        {
            set;
        }

        string DbName
        {
            set;
        }

        // questa si può leggere e settare
        bool DatabaseRefreshed
        {
            get;
            set;
        }
        #endregion

        // metodi dell'interfaccia
        #region metodi

        bool OpenConnection();
        int CountContacts();
        bool GetContact(PhonesCallData contactCallData, System.Collections.ArrayList phoneNumbers, out string callMode, out bool eof);
        bool SetContact(PhonesCallData contactCallData, string contactStatus, string callMode);
        void RefreshDB();
        void Terminate();

        #endregion


    }
}
