using System;
using Raven.Client;
using Raven.Client.Document;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenDBSnapshotStore : ISnapshotStore
    {
        private readonly IDocumentStore _documentStore;

        public RavenDBSnapshotStore(string ravenUrl)
        {
            _documentStore = new DocumentStore
            {
                Url = ravenUrl,                
                Conventions = CreateConventions()
            }.Initialize(); 
        }

       
        public RavenDBSnapshotStore(IDocumentStore externalDocumentStore)
        {
            //externalDocumentStore.Conventions = CreateConventions();
            ConfigureConventions(externalDocumentStore.Conventions);
            _documentStore = externalDocumentStore;            
        }

        private static DocumentConvention ConfigureConventions(DocumentConvention convention)
        {
            convention.JsonContractResolver = new PropertiesOnlyContractResolver();
            convention.FindTypeTagName = x => "Snapshots";

            return convention;
        }
        private static DocumentConvention CreateConventions()
        {
            return ConfigureConventions(new DocumentConvention());
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            using (var session = _documentStore.OpenSession())
            {
                var snapshot = session.Load<StoredSnaphot>(eventSourceId.ToString());
                if (snapshot == null)
                {
                    return null;
                }
                return snapshot.Version <= maxVersion
                           ? new Snapshot(eventSourceId, snapshot.Version, snapshot.Data)
                           : null;
            }
        }

        public void SaveSnapshot(Snapshot source)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(new StoredSnaphot
                {
                    Id = source.EventSourceId.ToString(),
                    Data = source.Payload,
                    EventSourceId = source.EventSourceId,
                    Version = source.Version
                });

                session.SaveChanges();
            }
        }
    }
}