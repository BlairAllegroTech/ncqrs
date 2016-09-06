using System;
using System.IO;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage.NoDB.Tests.Fakes;
using Ncqrs.Spec;
using NUnit.Framework;
using Rhino.Mocks;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    [Category("Integration")]
    public abstract class NoDBEventStoreTestFixture
    {
        protected string rootPath;
        protected NoDBEventStore EventStore;
        protected object[] Events;
        protected Guid EventSourceId;

        [OneTimeSetUp]
        public void BaseSetup()
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

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.Delete(GetPath(), true);
        }

        protected string GetPath()
        {
            var path = Path.Combine(rootPath, "NoDBTests", GetType().Name, EventSourceId.ToString().Substring(0, 2) );
            //return "./NoDBTests/" + GetType().Name+"/"+EventSourceId.ToString().Substring(0, 2);
            return path;
        }
    }
}