using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// blair
//using Newtonsoft.Json.Serialization;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class PropertiesOnlyContractResolver : DefaultContractResolver
    {
        public PropertiesOnlyContractResolver()
        {
            // Blair
            //DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            //var result = base.GetSerializableMembers(objectType);

            var result = objectType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result.Where(x => x.MemberType == MemberTypes.Property)
                .Cast<MemberInfo>()
                .ToList();
        }
    }
}