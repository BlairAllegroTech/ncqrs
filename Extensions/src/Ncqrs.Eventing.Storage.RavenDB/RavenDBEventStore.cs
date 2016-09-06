﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Ncqrs.Eventing.Sourcing;


namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenDBEventStore : IEventStore
    {
        private readonly IDocumentStore _documentStore;

        public RavenDBEventStore(string ravenUrl)
        {
            _documentStore = new DocumentStore
            {
                Url = ravenUrl,                
                Conventions = CreateConventions()
            }.Initialize(); 
        }

        public RavenDBEventStore(IDocumentStore externalDocumentStore)
        {
            
            //externalDocumentStore.Conventions = CreateConventions();
            ConfigureConventions(externalDocumentStore.Conventions);
            _documentStore = externalDocumentStore;
        }

        private static DocumentConvention ConfigureConventions(DocumentConvention convention)
        {
            convention.JsonContractResolver = new PropertiesOnlyContractResolver();
            convention.FindTypeTagName = x => "Events";
            //convention.NewDocumentETagGenerator = GenerateETag

            return convention;
        }


        private static DocumentConvention CreateConventions()
        {
            return ConfigureConventions(new DocumentConvention());
            //return new DocumentConvention
            //{
            //    JsonContractResolver = new PropertiesOnlyContractResolver(),
            //    FindTypeTagName = x => "Events"
            //    //NewDocumentETagGenerator = GenerateETag
            //};
        }

        private static Guid? GenerateETag(object entity)
        {
            var sourcedEvent = entity as StoredEvent;
            if (sourcedEvent != null)
            {
                return Guid.NewGuid();
            }
            return null;
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventSourceId == id)
                    .Where(x => x.EventSequence >= minVersion)
                    .Where(x => x.EventSequence <= maxVersion)
                    .ToList().OrderBy(x => x.EventSequence);
                return new CommittedEventStream(id, storedEvents.Select(ToComittedEvent));
            }
        }

        private static CommittedEvent ToComittedEvent(StoredEvent x)
        {
            return new CommittedEvent(x.CommitId, x.EventIdentifier, x.EventSourceId,x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            try
            {
                using (var session = _documentStore.OpenSession())
                {
                    session.Advanced.UseOptimisticConcurrency = true;
                    foreach (var uncommittedEvent in eventStream)
                    {
                        session.Store(ToStoredEvent(eventStream.CommitId, uncommittedEvent));
                    }
                    session.SaveChanges();
                }
            }
            catch (Raven.Abstractions.Exceptions.ConcurrencyException)
            {
                Guid sourceId = Guid.Empty;
                long version = 0;
                if (eventStream.HasSingleSource)
                {
                    sourceId = eventStream.SourceId;
                    version = eventStream.Sources.Single().CurrentVersion;
                }
                throw new ConcurrencyException(sourceId, version);
            }
        }

        private static StoredEvent ToStoredEvent(Guid commitId, UncommittedEvent uncommittedEvent)
        {
            // Assing all values explicityl otherwise they get set to -1 which fails on deserialization
            var version = new Version(
                uncommittedEvent.EventVersion.Major>0 ? uncommittedEvent.EventVersion.Major : 0,
                uncommittedEvent.EventVersion.Minor > 0 ? uncommittedEvent.EventVersion.Minor : 0,
                uncommittedEvent.EventVersion.Build > 0 ? uncommittedEvent.EventVersion.Build : 0,
                uncommittedEvent.EventVersion.Revision > 0 ? uncommittedEvent.EventVersion.Revision : 0);

            return new StoredEvent
                       {
                           Id = uncommittedEvent.EventSourceId + "/" + uncommittedEvent.EventSequence,
                           EventIdentifier = uncommittedEvent.EventIdentifier,
                           EventTimeStamp = uncommittedEvent.EventTimeStamp,
                           Version = version,
                           CommitId = commitId,
                           Data = uncommittedEvent.Payload,
                           EventSequence = uncommittedEvent.EventSequence,
                           EventSourceId = uncommittedEvent.EventSourceId,
                       };
        }
    }
}
