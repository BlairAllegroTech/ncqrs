using System;
using System.IO;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage.NoDB.Tests.Fakes;
using Ncqrs.Spec;
using Rhino.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    public abstract class NoDBEventStoreTestFixture: IDisposable
    {
        protected string rootPath;
        protected NoDBEventStore EventStore;
        protected object[] Events;
        protected Guid EventSourceId;

        public NoDBEventStoreTestFixture()
        {
            var uri = new Uri(this.GetType().Assembly.CodeBase);
            rootPath = Path.GetDirectoryName(uri.LocalPath);

            var path = Path.Combine(rootPath, "NoDBTests", GetType().Name);
            EventStore = new NoDBEventStore(path);
            EventSourceId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            Events = new object[] {new AccountTitleChangedEvent("Title")};
            var eventStream = Prepare.Events(Events)
                .ForSourceUncomitted(EventSourceId, Guid.NewGuid());
            EventStore.Store(eventStream);
        }


        protected string GetPath()
        {
            var path = Path.Combine(rootPath, "NoDBTests", GetType().Name, EventSourceId.ToString().Substring(0, 2) );
            //return "./NoDBTests/" + GetType().Name+"/"+EventSourceId.ToString().Substring(0, 2);
            return path;
        }

        public void Dispose()
        {
            Directory.Delete(GetPath(), true);
        }
    }
}