using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CPLibrary
{
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using NetDialerProviderInterface;
    using Phones.Messaging;

    // classe che estende l'interfaccia dbhandler
    class DatabaseMYSQLHandler : IDatabaseHandler
    {
        private const string ToBeCalled = "0";
        private const string Busy = "1";
        private const string NoAnswer = "2";
        private const string BadNumber = "3";
        private const string Failed = "4";
        private const string Completed = "5";
        private const string OperatorNotAvailable = "6";
        private const string InProcess = "9";

        //Call Mode Constants

        private const string PowerCall = "P";
        private const string ForcedPowerCall = "F";

        //Property Name and Value Contants

        private const string OverrideToCompleteName = "overrideToComplete";
        private const string IsToBeOverriddenValue = "YES";

        private const string AgentAllocationName = "agentAllocation";
        private const string IsAgentMandatory = "YES";

        //Call Data Name Constants

        private const string ID_CPG = "cpg_id";
        private const string ID_CPA = "cpa_id";
        private const string NUM_PHO = "numpho";
        private const string CUR_PHO = "curpho";
        private const string MAXCALL = "cpg_maxcall";

        private const string Denominazione = "cpa_nome";
      
        private const string TelefonoCorrente = "cpp_phonum";// letterale
       
        private const string StatoChiamata = "cpa_calsts";
        private const string CallMode = "CALLMODE";
        private const string DataOraApp = "DATAORAAPP";
        private const string CodOper = "CODOPER";
        private const string OperMode = "OPERMODE";
    




        private MySqlConnection connection;
        private MySqlCommand selectCommand;
        private DataSet dataSet;
        private MySqlDataAdapter dataAdapter;
        private IEnumerator dataSetEnumerator;

        private string contactTable;
        private string nomeCampagna;
        private string idCampagna;
        private string databaseName;
        private bool databaseRefreshed;


        public DatabaseMYSQLHandler()
        {
            connection = new MySqlConnection();
            selectCommand = new MySqlCommand();
            selectCommand.Connection = connection;

            dataSet = null;
            dataAdapter = null;

            Logger.Instance().WriteTrace(String.Format("SQL Database Handler created"));
            contactTable = "";
            idCampagna = "";
            nomeCampagna = "";
            databaseName = "";
            databaseRefreshed = false;
        }

        public string ConnectionString { set => connection.ConnectionString = value; }
        public string ContactTable { set => contactTable = value; }
        public string NomeCampagna { set => nomeCampagna = value; }
        public string IdCampagna { set => idCampagna = value; }
        public string DbName { set => databaseName = value; }
        public bool DatabaseRefreshed { get => databaseRefreshed; set => databaseRefreshed = value; }


        #region METODI CPROVIDER

        public int CountContacts()
        {
            //We use a SqlDataReader (from ADO.NET), cause it's faster than a DataSet if we only need to get datas
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;

            try
            {
                string query = "SELECT COUNT(*) FROM cpanagra WHERE cpa_cpgid ='" + idCampagna + "'";
                // in funzione del nome campagna
                //query = @"select count(*) from " + contactTable + " an,cpcamp cm where cm.cpg_campag='"+nomeCampagna+"' and cm.cpg_id=cpa_cpgid";

                command.CommandText = query;
                command.CommandType = System.Data.CommandType.Text;

           
                Int32 contacts = Convert.ToInt32(command.ExecuteScalar().ToString()); 

                Logger.Instance().WriteTrace(String.Format("COUNTCONTACTS: Number of Contacts in database for Service {0}: {1}", nomeCampagna, contacts));

                command.Dispose();

                return contacts;
            }
            catch (Exception e)
            {
                command = null;
                Logger.Instance().WriteTrace(String.Format("COUNTCONTACTS: Exception on counting contacts for Service {0}: {1}", nomeCampagna, e.Message));
                return 0;
            }
        }


        /*     GET E SET CONTACT     */

        #region GET SET CONTACT

        public bool GetContact(PhonesCallData contactCallData, ArrayList phoneNumbers, out string callMode, out bool eof)
        {
            Logger.Instance().WriteTrace(String.Format("Get Contact Inizio campagna {0}", nomeCampagna));

            callMode = "P"; // devo leggerlo dal db...
            eof = false;
            try
            {
              

                Int32 ID_Campagna = 0;// locale
                Int32 ID_Anagrafica = 0;//locale
                Int32 numPho = 0;
                Int32 curPho = 0;

                string NomeContatto = "";              
                string NumeroTelCorrente = "";
                string statoCall = "";
                string operMode = "N";// sempre no non interessa Yes
                string recordString = "";

                

                statoCampagna stsCampagna = determina_stato_Campagna();
                Logger.Instance().WriteTrace(String.Format("Stato campagna "+ nomeCampagna+": " + stsCampagna.isActive.ToString()));
                // se attiva allora faccio la scansione delle anagrafiche relative oppure ricarico il dataset se nuova
                if (stsCampagna.isActive)
                {
                    Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna " + nomeCampagna + " Attiva. Ragione: " + stsCampagna.DescStatus 
                        + "(" + stsCampagna.ReturnStatus.ToString() + ")"));
 

                    //Creation of a new Dataset
                    dataSet = new DataSet();
                    dataSet.DataSetName = databaseName;
                    dataAdapter = new MySqlDataAdapter();

                    string query = @"
                                        SELECT 
                                            cpg_id,cpa_id,cpa_nome,cpp_phonum,cpa_calsts,cpa_numpho,cpa_curpho+1 
                                        from cpanagra an,cpcamp cm ,cpphones ph
                                        WHERE 
                                            cpg_campag='"+ nomeCampagna + @"'
                                            and
                                            cpa_calsts=1 -- stato telefonata = 1 ovvero pronto
                                            and       
                                            cpg_id=cpa_cpgid -- campagna id
                                            and 
                                            cpp_numpho=cpa_curpho + 1 -- contatore telefono partenza da ZERO
                                            and
                                            cpp_cpaid=cpa_id -- join tra id phone e id anagr
    
                                        ORDER by cpa_numpty     
                                        LIMIT 1
                                    ";

                        

               
                    selectCommand.CommandText = query;
                    selectCommand.CommandType = CommandType.Text;

                    Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - query: {1}", nomeCampagna, query));


                    dataAdapter.SelectCommand = selectCommand;
                    MySqlCommandBuilder objCommandBuilder = new MySqlCommandBuilder(dataAdapter);

                    dataAdapter.Fill(dataSet);

                    //we set this enumerator in order to iterate over the rows of the DataTable
                    dataSetEnumerator = (IEnumerator)dataSet.Tables[0].Rows.GetEnumerator();


                    if (dataSetEnumerator != null)
                    {
                        // il move next muove di uno il dataset e poi esce in return = TRUE ad ogni call
                        // se non c'è più nulla va oltre il while e processa le altre righe
                        while (dataSetEnumerator.MoveNext())
                        {

                            Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Service = {0} - Calling MoveNext", nomeCampagna));
                            DataRow currentRow = (DataRow)dataSetEnumerator.Current;

                            Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Service = {0} - Reading data from DataSet", nomeCampagna));

                            if (currentRow[0] != DBNull.Value) ID_Campagna = Convert.ToInt32(currentRow[0]);
                            if (currentRow[1] != DBNull.Value) ID_Anagrafica = Convert.ToInt32(currentRow[1]);
                            if (currentRow[2] != DBNull.Value) NomeContatto = currentRow[2].ToString();
                            if (currentRow[3] != DBNull.Value) NumeroTelCorrente = currentRow[3].ToString();
                            if (currentRow[4] != DBNull.Value) statoCall = currentRow[4].ToString();
                            if (currentRow[5] != DBNull.Value) numPho = Convert.ToInt32(currentRow[5]);
                            if (currentRow[6] != DBNull.Value) curPho = Convert.ToInt32(currentRow[6]);



                            //Filling CallData i dati di chi chiamo servono per la phone bar
                            contactCallData.Add(ID_CPG, ID_Campagna.ToString());
                            contactCallData.Add(ID_CPA, ID_Anagrafica.ToString());
                            contactCallData.Add(Denominazione, NomeContatto);
                            contactCallData.Add(TelefonoCorrente, NumeroTelCorrente);
                            contactCallData.Add(OperMode, operMode);
                            contactCallData.Add(NUM_PHO, numPho.ToString());
                            contactCallData.Add(CUR_PHO, curPho.ToString());
                            

                            recordString = "CHIAMATA:"
                                + "\nID Anagrafica: "+ID_Anagrafica.ToString() 
                                + "\nNome: " + NomeContatto 
                                + "\nphone:" + NumeroTelCorrente 
                                + "\ncurrent phone: " + curPho.ToString()
                                + "\nnum phone: " + numPho.ToString();

                            Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - Data readed= {1}", nomeCampagna, recordString));

                            Debug.WriteLine("Chiamo " + NomeContatto + " tel."+ NumeroTelCorrente +" telefono num:" + curPho + " di " + numPho);



                            /////////////TELEFONO DA CHIAMARE
                            if (phoneNumbers.Count == 0)
                                phoneNumbers.Add(NumeroTelCorrente);
                            else
                                phoneNumbers[0] = NumeroTelCorrente;

                            /////

                            Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - CALL DATA {1}", nomeCampagna, contactCallData.ToString()));


                            ////////////// METTO LO STATO IN LAVORAZIONE
                            Logger.Instance().WriteTrace("Setto come Anagrafica " + NomeContatto + "come 9 - Occupata");

                            Debug.WriteLine("Setto come Anagrafica " + NomeContatto + "come 9 - Occupata");
                            Blocco_Anagrafica(ID_Anagrafica, 9, contactCallData, numPho, curPho);// devo mettere 9 in lavorazione cpa_calsts=9 e incrementare il numero per allinearlo a questo che sto chiamando
                            /////

                            return true;
                        }

                        //////////////////////////////////// FINE CAMPAGNA SE NON HO ALTRI RECORDS OPPURE NESSUN RECORD ALL'INIZIO                      
                     

                        Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - Fine o Nessun Records: EOF imposto Campagna come 2-Chiusa", nomeCampagna));
                        Debug.WriteLine("Ho terminato la campagna"+nomeCampagna+" 2-chiusa");
                        update_Campagna_Status(2, idCampagna);// 2-chiudo campagna id perchè non ho più numeri da contattare
                        

                        eof = true;
                        return false;
                         
                       


                     
                        ///////////////////////////

                    }
                     

                }
                else// altrimenti esco con eof true passato al dialer
                {
                    if (stsCampagna.ReturnStatus == 2)
                    {
                        Debug.WriteLine("La campagna " + nomeCampagna + " è chiusa");
                        Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna " + nomeCampagna + " disattiva. Ragione: "
                            + stsCampagna.DescStatus + "(" + stsCampagna.ReturnStatus.ToString() + ")"));

                        eof = true;// blocco la campagna ma non aggiorno il suo stato di campagna finita 2
                        return false;
                    }
                    else if (stsCampagna.ReturnStatus == 8)
                    {

                        Debug.WriteLine("La campagna " + nomeCampagna + " è stata bloccata dall'utente");
                        Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna " + nomeCampagna + " bloccata. Ragione: "
                            + stsCampagna.DescStatus + "(" + stsCampagna.ReturnStatus.ToString() + ")"));

                        eof = true;// blocco la campagna ma non aggiorno il suo stato di campagna finita 2
                        return false;


                    }
                    else if (stsCampagna.ReturnStatus == 0 || stsCampagna.ReturnStatus == 9)
                    {

                        Debug.WriteLine("La campagna " + nomeCampagna + " è occupata o inattiva");
                        Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna " + nomeCampagna + " occupata o inattiva . Ragione: "
                            + stsCampagna.DescStatus + "(" + stsCampagna.ReturnStatus.ToString() + ")"));

                        eof = true;// blocco la campagna ma non aggiorno il suo stato di campagna finita 2
                        return false;

                    }
                }
                Debug.WriteLine("Errore generico Campagna");
                Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - Errore generico", nomeCampagna));
                eof = true;// blocco la campagna ma non aggiorno il suo stato di campagna finita 2
                return false;

            }
            catch (Exception e)
            {
                Logger.Instance().WriteTrace(String.Format("--> GET CONTACT - Campagna = {0} - Fallita per errore: {1}", nomeCampagna, e.Message));
                dataSetEnumerator = null;
                dataSet = null;
                dataAdapter = null;
                selectCommand = null;
                connection.Close();
                return false;
            }
        }
         
        public bool SetContact(PhonesCallData contactCallData, string contactStatus, string callMode)
        {
            Logger.Instance().WriteTrace(String.Format("Set Contact Inizio per Campagna {0}", nomeCampagna));

            // i 3 update dopo responso DIALER.EXE

           Debug.WriteLine("Aggiornamento per Contatto:" + contactCallData.ToString());
            try
            {
               Boolean transOk = UpdateCPCalls(contactCallData, contactStatus);
               if(transOk) transOk = UpdateCPPhone(contactCallData, contactStatus);
               if(transOk) transOk = UpdateCPAnagra(contactCallData, contactStatus);

               return true;

            }
            catch (Exception e)
            {

                Logger.Instance().WriteTrace(String.Format("--> SET CONTACT - Campanga = {0} - Failed Cause Exception: {1}", nomeCampagna, e.Message));

                connection.Close();
                return false;
            }
        }

        #endregion


        /*        FINE     */





        public void RefreshDB()
        {
            try
            {
                //RefreshDB works on the main DataSet. We clear the DataSet and we refill it
                Logger.Instance().WriteTrace(String.Format("--> REFRESH DB: Inizio - Campagna: {0}", nomeCampagna));

                

                databaseRefreshed = ricarico_Dataset();// ritorna true o false

                Logger.Instance().WriteTrace(String.Format("--> REFRESH DB: Fine - Campagna: {0}", nomeCampagna));
            }
            catch (Exception e)
            {
                Logger.Instance().WriteTrace(String.Format("--> REFRESH DB - Campagna = {0} - Failed Cause Exception: {1}", nomeCampagna, e.Message));
            }
        }

        public void Terminate()
        {
            Logger.Instance().WriteTrace(String.Format("Terminate: Begin - Service : {0}", nomeCampagna));

            if (dataSet != null && dataAdapter != null && dataSetEnumerator != null)
            {
                dataSetEnumerator.Reset();
                dataSet.Clear();
                dataAdapter.Dispose();
                selectCommand.Dispose();
                dataSetEnumerator = null;
                dataSet = null;
                dataAdapter = null;
                selectCommand = null;
                connection.Close();
            }

            Logger.Instance().WriteTrace(String.Format("Terminate: End - Service : {0}", nomeCampagna));
        }



        #endregion

        #region  RICARICAMENTO DATASET

      
        public Boolean ricarico_Dataset()
        {
            if (dataSet != null)
            {
                dataSet.Clear();
                dataSet.DataSetName = databaseName;
                dataAdapter.Dispose();
                dataAdapter = new MySqlDataAdapter();
            }
            else {

                dataSet = new DataSet();
                dataSet.DataSetName = databaseName;
                dataAdapter = new MySqlDataAdapter();

            }


            string query = @"
                                        SELECT 
                                            cpg_id,cpa_id,cpa_nome,cpp_phonum,cpa_calsts,cpa_numpho,cpa_curpho+1 
                                        from cpanagra an,cpcamp cm ,cpphones ph
                                        WHERE 
                                            cpg_campag='" + nomeCampagna + @"'
                                            and
                                            cpa_calsts=1 -- stato telefonata = 1 ovvero pronto
                                            and       
                                            cpg_id=cpa_cpgid -- campagna id
                                            and 
                                            cpp_numpho=cpa_curpho + 1 -- contatore telefono partenza da ZERO
                                            and
                                            cpp_cpaid=cpa_id -- join tra id phone e id anagr
    
                                        ORDER by cpa_numpty     
                                        LIMIT 1
                                    ";

            selectCommand.CommandText = query;
            selectCommand.CommandType = System.Data.CommandType.Text;

            dataAdapter.SelectCommand = selectCommand;
            MySqlCommandBuilder objCommandBuilder = new MySqlCommandBuilder(dataAdapter);

            try
            {
                dataAdapter.Fill(dataSet, contactTable);
                dataSetEnumerator = (IEnumerator)dataSet.Tables[contactTable].Rows.GetEnumerator();
            }
            catch (Exception ex)
            {
                Logger.Instance().WriteTrace("--> RELOAD DATASET - Error: " + ex.Message);
                return false;
            }

            Logger.Instance().WriteTrace("--> RELOAD DATASET - Refreshed dataset");
            Debug.WriteLine("REFRESH ESEGUITO--> Righe " +dataSet.Tables[0].Rows.Count.ToString());
            return true;// chiuso = true
        }

        #endregion


        #region Metodi Aggiornamento e lettura su TABELLE




        private void update_Campagna_Status(Int32 stato, string Id_Campagna)
        {
            // aggiorno a 2-completata la campagna
            string query = @"
                            update cpcamp set  cpg_status=" + stato.ToString() + @"
                            where
                            cpg_id=" + Id_Campagna;

            Logger.Instance().WriteTrace("--> UPDATE CAMPAGNA STATUS - Query:\n" + query);
            Debug.WriteLine("Query UPDATE CAMPAGNA:\n" + query+"\n-------------------\n");      
            MySqlCommand updCmd = new MySqlCommand(); 
            updCmd.CommandText = query;
            updCmd.Connection = connection;
            try
            {
                updCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                Logger.Instance().WriteTrace(String.Format("--> UPDATE CAMPAGNA STATUS - Campagna = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));

            }
        }

        private Boolean UpdateCPPhone(PhonesCallData contactCallData, string contactStatus)
        {
            Logger.Instance().WriteTrace("--> UPDATE CPPHONE - Campagna:" + nomeCampagna + " - Aggiorno tabella CpPhone con stato della call porto anvanti conteggio tentativi al telefono numero " + contactCallData.Collection[NUM_PHO]);
            Debug.WriteLine("Aggiorno tabella CpPhone con stato della call porto anvanti conteggio tentativi al telefono numero " + contactCallData.Collection[NUM_PHO]);
            String CallStatus = contactStatus;

            // -Update CPPhones aggiornando i due campi stato: cpp_censts(con call status ricevuto dal metodo setContact) 
            // + cpp_calsts(calcolato in base allo status);

            MySqlCommand cmd = new MySqlCommand();


            string query = @"
                            update cpphones set cpp_censts=@cntStatus,cpp_calsts='" + CallStatus + @"'  ,cpp_calcnt=cpp_calcnt+1                          
                            where
                            cpp_cpaid=" + contactCallData.Collection[ID_CPA]
                            + " AND cpp_numpho=" + contactCallData.Collection[CUR_PHO];// meno uno perchè avevo già incrementato il numpho per la prossima

            cmd.Parameters.Add("@cntStatus", MySqlDbType.Int32).Value = Convert.ToInt32(contactStatus);

            Logger.Instance().WriteTrace("--> UPDATE CPPHONE - Campagna:"+nomeCampagna+", Query:\n" +query );
            cmd.Connection = connection;

            try
            {

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                return true;

            }
            catch (Exception ex)
            {

                Logger.Instance().WriteTrace(String.Format("--> UpdateCPPhone - Campagna = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));
                return false;
            }
        }

        private Boolean UpdateCPCalls(PhonesCallData contactCallData, string contactStatus)
        {

            //- Update CPCalls con le info restituite dal Dialer, compilando, in caso di risposta,
            //anche i campi ora fine chiamata e durata in secondi. Inoltre, se presente nel calldata il campo Agent, imposta il valore in cpl_agent

            Logger.Instance().WriteTrace("--> UPDATE CPCALLS - Campagna:" + nomeCampagna);
            String oraEnd = DateTime.Now.ToString("HHmmss");
            MySqlCommand cmd = new MySqlCommand();

            string query = @"
                            update cpCalls set  cpl_calsts=@cntStatus,cpl_censts=@cntStatus,cpl_oraend='" + oraEnd + @"',
                            cpl_caldur =((substr('" + oraEnd + @"',1,2)*3600)+(substr('" + oraEnd + @"',3,2)*60)+substr('" + oraEnd + @"',5,2))
                            -((substr(cpl_oracall,1,2)*3600)+(substr(cpl_oracall,3,2)*60)+substr(cpl_oracall,5,2)),
                            cpl_Agent=@agente
                            where
                            cpl_cpaid=" + contactCallData.Collection[ID_CPA] +" and cpl_numpho="+contactCallData[CUR_PHO]+" and cpl_caldur is null";


            cmd.Parameters.Add("@cntStatus", MySqlDbType.Int32).Value = Convert.ToInt32(contactStatus);
            cmd.Parameters.Add("@agente", MySqlDbType.String).Value = "";

            cmd.Connection = connection;
            Logger.Instance().WriteTrace("--> UPDATE CPCALLS - Campagna:" + nomeCampagna + ", Query:\n" + query);
            try
            {

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {

                Logger.Instance().WriteTrace(String.Format("--> UPDATE CPCALLS - Campagna = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));
                return false;
            }
        }

        private Boolean UpdateCPAnagra(PhonesCallData contactCallData, string contactStatus)
        {
            Logger.Instance().WriteTrace("--> UPDATE CPANAGRA - Campagna:" + nomeCampagna);
            Int32 numPho = Convert.ToInt32(contactCallData.Collection[NUM_PHO]);
            Int32 curPho = Convert.ToInt32(contactCallData.Collection[CUR_PHO]);

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;

            string query = @"
                            update cpanagra set  cpa_calsts=2
                            where
                            cpa_id=" + contactCallData.Collection[ID_CPA];



            if (contactStatus == "3")// 2-OK quindi se è 3-KO
            {   // cpa_numpty=0
                Logger.Instance().WriteTrace("--> UPDATE CPANAGRA - Campagna:" + nomeCampagna+" - tentativo fallito 3-KO verifico se ho altri numeri oppure devo settare 3-KO definitivamente l'anagrafica");
                Debug.WriteLine("tentativo fallito 3-KO verifico se ho altri numeri oppure devo settare 3-KO");
                if (curPho < numPho)
                {
                    Logger.Instance().WriteTrace("--> UPDATE CPANAGRA - Campagna:" + nomeCampagna + " - Ho ancora altri numeri da provare in anagrafica quindi la metto a 1-Pronta");
                    Debug.WriteLine("Ho ancora altri numeri da provare in anagrafica quindi la metto a 1-Pronta");
                    query = @"  update cpanagra an set  cpa_calsts=1
                            where
                            cpa_id=" + contactCallData.Collection[ID_CPA];
                }
                else
                {
                    Logger.Instance().WriteTrace("--> UPDATE CPANAGRA - Campagna:" + nomeCampagna + " - Ho finito i numeri e quindi è 3-KO");
                    Debug.WriteLine("Ho finito i numeri e quindi è 3-KO");
                    query = @"  update cpanagra an set  cpa_calsts=3
                            where
                            cpa_id=" + contactCallData.Collection[ID_CPA];
                }

                Debug.WriteLine(query.Trim()+"\n-------\n");
            }
            else if (contactStatus == "2")
            {
                Logger.Instance().WriteTrace("--> UPDATE CPANAGRA - Campagna:" + nomeCampagna + " - sono riuscito a contattare quindi è 2-OK");
                Debug.WriteLine("sono riuscito a contattare quindi è 2-OK");
                Debug.WriteLine(query.Trim() + "\n-------\n");
                query = @"
                            update cpanagra set  cpa_calsts=2
                            where
                            cpa_id=" + contactCallData.Collection[ID_CPA];
            }
             

           

        



            try
            {

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();


                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Errore " + ex.Message);
                Logger.Instance().WriteTrace(String.Format("--> Update CPANAGRA - Campagna = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));
                return false;
            }

        }

        private statoCampagna determina_stato_Campagna()
        {
            statoCampagna res = new statoCampagna();

            string query = @" 
                                select cpg_status from cpcamp where cpg_campag='@NOMECAMPAGNA@'
                           
                            ";
            query = query.Replace("@NOMECAMPAGNA@", nomeCampagna);
            MySqlCommand cm = new MySqlCommand(query, connection);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            MySqlDataReader rd = cm.ExecuteReader();

            rd.Read();
            Int32 status = rd.GetInt32(0);
            if (status == 0)
            {
                res.isActive = false;
                res.ReturnStatus = status;
                res.DescStatus = "Disattiva";
            }
            else if (status == 9)
            {
                res.isActive = false;
                res.ReturnStatus = status;
                res.DescStatus = "Stoppata";
            }
            else if (status == 8)
            {
                res.isActive = false;
                res.ReturnStatus = status;
                res.DescStatus = "Richiesta stop utente";
            }
            else if (status == 1)
            {
                res.isActive = true;
                res.ReturnStatus = status;
                res.DescStatus = "Attiva";
            }
            else
            {
                res.isActive = false;
                res.ReturnStatus = status;
                res.DescStatus = "Stato sconosciuto - Disattivo Campagna";
            }
            rd.Close();// chiudo il datareader
            return res;
        }

        private void Blocco_Anagrafica(Int32 Id_Anagrafica,Int32 StsDaImpostare, PhonesCallData contactCallData,Int32 numPho,Int32 curPho)// num di tel. che ha l'anagrafica
        {
            
            string query = String.Empty;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;


            // incremento di 1 il telefono
            // metto anagrafica in lavorazione-9 (sul numero precedente)
            if (StsDaImpostare == 9)
            { 
                // UPDATE ANAGRAFICA
                // blocco l'anagrafica con 9 e scalo il numero di 1 in avanti(questo serve per capire se ha più numeri) e conteggio il tentativo in anagr
                query = @"  update cpanagra set  cpa_calsts=" + StsDaImpostare 
                        + @",cpa_calcnt=cpa_calcnt+1,cpa_curpho=cpa_curpho+1
                            where
                            cpa_id=" + Id_Anagrafica;

                Logger.Instance().WriteTrace("--> BLOCCO ANAGRAFICA - Campagna:" + nomeCampagna + " - Aggiorno l'anagrafica inserento lo stato attuale per "
                    + "es aumentando il contatore e aggiornando il num tel che sto usando \n" + query.Trim());

                Debug.WriteLine("Aggiorno l'anagrafica inserento lo stato attuale per "
                    +"es aumentando il contatore e aggiornando il num tel che sto usando\n" + query.Trim()+"\n");
 
                cmd.CommandText = query;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    Logger.Instance().WriteTrace(String.Format("--> BLOCCO ANAGRAFICA - Campagna = {0} -  errore: {1}", nomeCampagna, ex.Message));
                }

                // UPDATE PHONES          
                // aggiorno stato telefono che è 9 in questo caso conteggio +1 in telefono ,cpp_calcnt=cpp_calcnt+1
                query = @"update cpphones set   cpp_calsts=" + StsDaImpostare + @"
                            where 
                            cpp_cpaid=" + Id_Anagrafica +" and cpp_numpho="+curPho;

                /// 
                Logger.Instance().WriteTrace("--> BLOCCO ANAGRAFICA: UPDATE CPPHONE - Campagna:" + nomeCampagna 
                    + " - aggiorno stato telefono che è 9 in questo caso conteggio +1 in telefono ,cpp_calcnt=cpp_calcnt+1\n" + query.Trim());


                cmd.CommandText = query;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    Logger.Instance().WriteTrace(String.Format("--> BLOCCO ANAGRAFICA UPDATE CPPHONE - Campagna = {0} - errore: {1}", nomeCampagna, ex.Message));
                }

                ///// inserimento chiamata in corso nella tabella cpcalls
                // inserimento nel log delle chiamate il tentativo di call
                String id_Anag = contactCallData.Collection[ID_CPA];      

                query = @"insert into cpcalls (cpl_cpaid,cpl_numpho,cpl_seqcall,cpl_oracall,cpl_calsts)
                                        values(@idAnag,@numPho,@data,@ora,@stato)";

                cmd.Parameters.Add("@idAnag", MySqlDbType.Int32).Value = Convert.ToInt32(Id_Anagrafica);
                cmd.Parameters.Add("@numPho", MySqlDbType.Int32).Value = Convert.ToInt32(curPho);
                cmd.Parameters.Add("@data", MySqlDbType.String).Value =  DateTime.Now.Date.ToString("yyyyMMdd");
                cmd.Parameters.Add("@ora", MySqlDbType.String).Value =  DateTime.Now.ToString("HHmmss");
                cmd.Parameters.Add("@stato", MySqlDbType.Int32).Value = Convert.ToInt32(StsDaImpostare);

                cmd.CommandText = query;
                Logger.Instance().WriteTrace("--> BLOCCO ANAGRAFICA: INSERT CPCALLS - Campagna:" + nomeCampagna
                   + " - Inserisco in CPCALLS il log dell'inizio della chiamata" + query.Trim());
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    Logger.Instance().WriteTrace(String.Format("--> BLOCCO ANAGRAFICA: INSERT CPCALLS - Service = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));
                }

            }
            else if (StsDaImpostare == 3)// fallito KO
            {
                // se fallito aggiorno l'anagrafica a 3 ma solo se ho passato tutti i numeri
                query = @"  update cpanagra set  cpa_calsts=" + StsDaImpostare + @"
                            where
                            cpa_id=" + Id_Anagrafica;
                cmd.CommandText = query;
                Logger.Instance().WriteTrace("--> BLOCCO ANAGRAFICA: UPDATE CPANAGRA - Campagna:" + nomeCampagna
                   + " - impostazione stato a 3 nell'anagrafica" + query.Trim());
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    Logger.Instance().WriteTrace(String.Format("--> BLOCCO ANAGRAFICA: UPDATE CPANAGRA - Service = {0} - Fallita per errore: {1}", nomeCampagna, ex.Message));
                }

            }

             


        }

        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                Logger.Instance().WriteTrace(String.Format("OPEN CONNECTION: Database connection opened - Service: {0}", nomeCampagna));
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance().WriteTrace(String.Format("OPEN CONNECTION: Exception on opening database connection - Service {0}: {1}", nomeCampagna, e.Message));
                return false;
            }
        }

        #endregion
    }





    internal class statoCampagna
    {
        public Boolean isActive = false;// mi dice se attiva
        public Int32 ReturnStatus = 0;// mi indica in che modo è stata bloccata 0-disattiva 9-stoppata 1-abilitata 8-stop da utente
        public String DescStatus = String.Empty;

    }
}
