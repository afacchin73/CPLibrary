# CPLibrary
1.	MariaDB
•	Scarica dal link seguente la versione stabile di MariaDB per winX64 (file .msi) https://downloads.mariadb.org/mariadb/10.2.13/
•	Installalo con user root e pwd root così non devi modificare il codice c# nella connessione
•	Dovrebbe contenere il query editor HeidiSQL che va bene per quanto riguarda questo tipo di motore
•	Apri HeidiSQL e connettiti a localhost:3306 root root
•	Menu  file  Esegui file SQL…
•	Carica il database da databaseCP.sql contiene anche dei dati di esempio

2.	Scompatta lo zip in allegato dove preferisci
•	Apri il progetto selezionando CPLibrary.sln 
 
Informazioni:
•	la cartella CPLibrary contiene i sorgenti della dll
•	La cartella TestMariaDB contiene un progetto tipo Console(finestra di DOS) per testare la libreria CPLibrary.
•	La cartella Packages contiene i pacchetti di librerie che si aggiungono al progetto. In questo caso il pacchetto contenuto è quello della libreria di .net per mysql(mariadb) per connettersi al motore di DB e fare le query necessarie.

Per installare un pacchetto qualunque si apre il progetto in visual studio poi:
dal menu  strumenti gestione pacchetti NUGET  gestisci pacchetti nuget per la soluzione.
Si apre il motore di ricerca che ti consente di cercare/installare/disinstallare/aggiornare tantissime librerie del Repository NUGET.
Nel nostro caso:
 



Aperta la soluzione hai i 2 progetti:

 

Il grassetto di TestMariaDB significa che hai impostato questo progetto come default d’avvio per il DEBUG. Infatti è il programma Console che deve partire per simulare il Dialer.exe.
Nel file program.cs trovi il sorgente della simulazione ovvero:

•	creazione istanza dll CPLibrary
•	Init della libreria
•	Avvio di GetContact con i parametri campagna 
•	SetContact che simula se sono riuscito a contattare o meno il numero
•	finchè eof non diventa uguale a TRUE

per debuggare la Dll:
faccio scrivere la sua attività di debug sulla finestra di OUTPUT (vedi il comando Debug.WriteLine(string testo) che serve per scrivere sulla finestra OUTPUT).
La finestra di Output si trova qui:

menu  visualizza Output

Poi c’è la scrittura su file del Log di attività come da esempio che sarà utile quando si passa al test vero della libreria. Man mano lo arricchisco di eventi e lo miglioro.
Il file che implementa il log si trova nel progetto CPLibrary:
•	  Logger.cs
Come dicevamo è stato scritto per essere thread safe in modo che contemporaneamente si possa accerdere al file stesso per scrivere le righe di LOG.

File della DLL:
ContactProvider.cs
•	Contiene le definizioni metodi del contact provider: init, getContact, SetContact etc….
•	Viene creato in automatico in base alle librerie native del cotact provider
•	Solo nella funzione Init è stato scritto del codice che riguarda la connessione al DB
•	La Classe ContactProvider estende le funzioni dell’Interfaccia contenuta nella libreria NetDialerInterfaceProvider (roba loro…).è l’interfaccia che contiene in pratica i metodi get set refresh init….
 
•	Non si tocca più eccetto se si cambia Motore database target (dentro c’è la stringa di connessione).Chiaramente è da parametrizzare. lo farò
 
IDatabaseHandler.cs
•	È l’interfaccia che espone i metodi del DB
 
DatabaseMYSQLHandler.cs
•	Implementa l’interfaccia IDatabaseHandler implementando i metodi della stessa Ovvero GetContact SetContact e così via.

 
•	Ed è questo il file in cui si scrive la logica della DLL

Nella funzione GetContact trovi la query per reperire il contatto le varie logiche di Update così come in SetContact.

Come vedi è tutto molto complesso tuttavia si ragiona alla fine su quest’ultimo file DatabaseMYSQLHandler.cs


Per esempio io non avrei mai utilizzato le interfacce perché creano confusione e credo sia una tecnica vecchia. Ovviamente non mi discosto perché non so cosa possa accadere quindi ce le teniamo tanto sono già implementate e no dobbiamo fare nulla.
Un riferimento a cosa sono è qui:
http://www.html.it/pag/15450/le-interfacce/
non servono a nulla.
